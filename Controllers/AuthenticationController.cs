using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Data.OleDb;
using System.Diagnostics;
using System.Windows;
using PMS.Models;
using PMS.Util;
using System.Reflection;
using System.Windows.Controls.Primitives;
using System.Collections;
using System.Text.RegularExpressions;
using PMS.Dialogs;
using System.Security.Cryptography;
using PMS.Components;
using Microsoft.IdentityModel.Tokens;
using System.CodeDom;

namespace PMS.Controllers
{
    public class AuthenticationController
    {
        private int FailedLoginAttempts = 0;
        private DateTime? LoginUnlockedAt = null;

        private bool SecurityQuestionsVerifiedLock = false;

        public static string DEFAULT_ADMIN_USERNAME = "root@prs.uk";
        private PMSWindow Window { get; set; }

        public AuthenticationController(PMSWindow window)
        {
            Window = window;
        }

        private Exception? MaybeBlockRequest()
        {
            DateTime dateTime = DateTime.Now;

            if (LoginUnlockedAt != null)
            {
                int comparisonDate = dateTime.CompareTo(LoginUnlockedAt);

                if (comparisonDate < 0)
                {
                    TimeSpan timeUntilUnlocked = ((DateTime)LoginUnlockedAt).Subtract(dateTime);

                    return new Exception($"Login has been temporarily disabled for {timeUntilUnlocked.TotalMinutes:0.00} minutes, please try again later.");
                }
                else
                {
                    LoginUnlockedAt = null;
                }
            }

            return null;
        }

        private void RecordFailedLoginAttempt()
        {
            FailedLoginAttempts++;
            if (FailedLoginAttempts >= AppConstants.MaximumFailedLoginAttempts)
            {
                LoginUnlockedAt = DateTime.Now.AddMinutes(AppConstants.FailedLoginsTimeoutMins);
                MaybeBlockRequest();
            }
        }

        public async Task<Result<User, Exception>> HandleAuthenticationRequest(string username, string plainTextPassword)
        {
            // Add artificial delay to prevent automated attacks
            Random rng = new();
            await Task.Delay(rng.Next(300, 800));

            Exception? blocked = MaybeBlockRequest();
            if (blocked != null)
            {
                return Result<User, Exception>.Err(blocked);
            }

            string hashedPassword = PasswordHelper.HashPassword(plainTextPassword);

            User? user = AppDatabase.QueryFirst<User>(
                "SELECT * FROM tblUser WHERE Username=? AND HashedPassword=?",
                [username, hashedPassword]
            );

            if (user != null && !user.IsDisabled)
            {
                Result<User, Exception> firstLoginResult = MaybeHandleFirstLogin(user);

                if (firstLoginResult.IsErr())
                {
                    return Result<User, Exception>.Err(
                        firstLoginResult.Error
                    );
                }

                return Result<User, Exception>.Ok(user);
            }

            RecordFailedLoginAttempt();

            if (user?.IsDisabled == true)
            {
                return Result<User, Exception>.Err(
                    new Exception("User account has been disabled by your system administrator, please contact them for more information.")
                );
            } else
            {
                return Result<User, Exception>.Err(
                    new Exception("Username or password is invalid, please try again.")
                );
            }

        }

        public Result<bool, Exception> HandlePasswordChangeRequest(User? authorisedUser = null, User? targetUser = null)
        {
            Result<User, Exception> passwordChangeRequestResult =
                this.InternalHandlePasswordChangeRequest(authorisedUser, targetUser);

            if (passwordChangeRequestResult.IsErr())
            {
                return Result<bool, Exception>.Err(passwordChangeRequestResult.Error);
            }

            PMSPasswordChangeWindow passwordChangeWindow = new(passwordChangeRequestResult.Value);
            bool? result = passwordChangeWindow.ShowDialog();

            // This occurs when the dialog is closed or canceled
            if (result == null || result == false)
            {
                return Result<bool, Exception>.Ok(false);
            }

            return Result<bool, Exception>.Ok(true);
        }


        public Result<User, Exception> MaybeHandleFirstLogin(User user)
        {
            if (!user.HasFirstLogin)
            {
                MessageBoxController.Show(
                    Window,
                    $"Welcome to {AppConstants.AppName}, {user.FormatFullName()}. As you are logging in for the first time, the system administrator will require you to update your password.",
                    $"Welcome to {AppConstants.AppName}",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                Result<bool, Exception> result = HandlePasswordChangeRequest(user, user);

                if (result.IsErr())
                {
                    return Result<User, Exception>.Err(
                        result.Error
                    );
                }

                int? updatedRows = AppDatabase.Update(
                    "UPDATE tblUser SET [HasFirstLogin]=? WHERE (ID=?)",
                    ["1" /* true in string form */, user.ID.ToString()]
                );

                if (updatedRows != null && updatedRows > 0)
                {
                    return Result<User, Exception>.Ok(user);
                }
                else
                {
                    return Result<User, Exception>.Err(
                        new Exception("No changes were committed to the database.")
                    );
                }
            }

            if (
                user.ComputedSecurityQuestion1 == null ||
                (
                    user.ComputedSecurityQuestion1 != null &&
                    (
                        user.ComputedSecurityQuestion1.SecurityQuestionID == 0 ||
                        user.ComputedSecurityQuestion1.Answer.IsNullOrEmpty()
                    )
                ) ||
                user.ComputedSecurityQuestion2 == null ||
                (
                    user.ComputedSecurityQuestion2 != null &&
                    (
                        user.ComputedSecurityQuestion2.SecurityQuestionID == 0 ||
                        user.ComputedSecurityQuestion2.Answer.IsNullOrEmpty()
                    )
                )
            ) {
                Result<bool, Exception> result = HandleSecurityQuestionsSetupRequest(user);

                if (result.IsErr())
                {
                    return Result<User, Exception>.Err(
                        result.Error
                    );
                }
            }

            return Result<User, Exception>.Ok(user);
        }

        public Result<bool, Exception> HandleSecurityQuestionsSetupRequest(User targetUser)
        {
            PMSSecurityQuestionsSetupSelfServiceWindow securityQuestionsSSWindow = new(targetUser);
            bool? result = securityQuestionsSSWindow.ShowDialog();

            // This occurs when the dialog is closed or canceled
            if (result == null || result == false)
            {
                return Result<bool, Exception>.Ok(false);
            }

            return Result<bool, Exception>.Ok(true);
        }

        public Result<User, Exception> HandleSecurityQuestionsRecoveryRequest(string username, Dictionary<SecurityQuestion, string> questionToAnswerMap)
        {
            User? user = User.GetUserByUsername(username);

            if (user == null)
            {
                return Result<User, Exception>.Err(
                    new Exception("No user could be found with that username, please try again.")
                );
            }

            if(user.IsDisabled)
            {
                return Result<User, Exception>.Err(
                    new Exception("User account cannot be recovered as it has been disabled by the system administrator.")
                );
            }

            Result<SecurityQuestionAnswer[], Exception> securityQuestionAnswersResult =
                SecurityQuestionAnswer.GetUserSecurityQuestionAnswers(user);

            if (securityQuestionAnswersResult.IsErr())
            {
                return Result<User, Exception>.Err(securityQuestionAnswersResult.Error);
            }

            SecurityQuestionAnswer[] securityQuestionAnswers = securityQuestionAnswersResult.Value;

            int matches = 0;

            foreach (KeyValuePair<SecurityQuestion, string> entry in questionToAnswerMap)
            {
                foreach (SecurityQuestionAnswer questionAnswer in securityQuestionAnswers)
                {
                    if (
                        entry.Key.ID == questionAnswer.SecurityQuestionID &&
                        entry.Value.ToLower() == questionAnswer.Answer.ToLower()
                       )
                    {
                        matches++;
                        break;
                    }
                }
            }

            if (matches < questionToAnswerMap.Count)
            {
                return Result<User, Exception>.Err(
                    new Exception("One or more questions and answers were incorrect, please try again.")
                );

            }

            // !!! This is a dangerous operation !!!
            // We need to make sure we only enable this flag
            // upon verifying security questions are valid.
            // It should also be falsified as soon as possible
            // to avoid the flag bypassing importing authorisation
            // checks within permission gates.
            SecurityQuestionsVerifiedLock = true;
            LogController.WriteLine("Permitting security question verification.", LogCategory.PermissionController);

            return Result<User, Exception>.Ok(user);
        }

        public Result<User, Exception> InternalHandlePasswordChangeRequest(User? authorisedUser = null, User? targetUser = null)
        {
            bool securityQuestionsVerified = false;

            // !!! Immediately lock this state after storing its original value !!!
            // We don't want to be in this state for too long.
            {
                securityQuestionsVerified = authorisedUser == null ? SecurityQuestionsVerifiedLock : false;

                if (securityQuestionsVerified)
                {
                    SecurityQuestionsVerifiedLock = false;
                }
            }

            User? changePasswordUser = PermissionController.CanUserPasswordChange(
                authorisedUser, 
                targetUser,
                securityQuestionsVerified
            );

            if (changePasswordUser != null)
            {
                // Again, lock the security questions verified state if it failed the first time
                SecurityQuestionsVerifiedLock = false;

                return Result<User, Exception>.Ok(changePasswordUser);
            } else
            {
                return Result<User, Exception>.Err(
                    new Exception(AppConstants.UnauthorisedMessage)
                );
            }
        }

        public Result<bool, Exception> HandlePasswordChangeFinalRequest(User targetUser, string newPlainTextPassword, User? authorisedUser = null)
        {
            string hashedPassword = PasswordHelper.HashPassword(newPlainTextPassword);

            if (targetUser.HashedPassword == hashedPassword)
            {
                return Result<bool, Exception>.Err(
                    new Exception("New password cannot be the same as the old password.")
                );
            }

            int? updatedRows = AppDatabase.Update(
                "UPDATE tblUser SET [HashedPassword]=? WHERE (ID=?)",
                [hashedPassword, targetUser.ID.ToString()]
            );

            if (updatedRows != null && updatedRows > 0)
            {
                return Result<bool, Exception>.Ok(true);
            } else
            {
                return Result<bool, Exception>.Err(
                    new Exception("No changes were committed to the database.")
                );
            }
        }

        private Result<bool, Exception> DeleteOldAdminUser()
        {
            string qry = "DELETE FROM tblUser WHERE Username=?";
            string[] qryArgs = [DEFAULT_ADMIN_USERNAME];

            if (AppDatabase.QueryFirst<User>(qry, qryArgs) != null)
            {
                if (AppDatabase.Update(qry, qryArgs) <= 0)
                {
                    return Result<bool, Exception>.Err(new Exception("Unable to clean up old administrator user."));
                }
            }

            return Result<bool, Exception>.Ok(true);
        }

        public Result<bool, Exception> MaybeHandleNoUsers()
        {
            int usersCount = AppDatabase.QueryFirst<QueryCount>(
                "SELECT COUNT(*) AS RecordCount FROM tblUser WHERE IsDisabled=False",
                []
            )?.RecordCount ?? 0;

            if (usersCount <= 0)
            {
                MessageBoxResult msgResult = MessageBoxController.Show(
                      Window,
                      $"Welcome to {AppConstants.AppName}.\n\nAs we haven't detected any users within the system, you will need to setup the administrator account to continue.\n\nBy default, the username for this account is 'root@prs.uk'.\nYou will be able to change this upon login.\n\nWould you like to continue with the creation of the administrator account?",
                      $"Welcome to {AppConstants.AppName}",
                      MessageBoxButton.YesNo,
                      MessageBoxImage.Information,
                      MessageBoxResult.Yes
                );

                if (msgResult != MessageBoxResult.Yes)
                {
                    Window.Close();
                    Application.Current.Shutdown();

                    return Result<bool, Exception>.Ok(true);
                }

                Result<bool, Exception> oldAdminUser = DeleteOldAdminUser();

                if (oldAdminUser.IsErr())
                {
                    Debug.WriteLine("Failed to clean up old admin user, aborting...");

                    return Result<bool, Exception>.Err(oldAdminUser.Error);
                }

                User adminUser = new()
                {
                    ID = User.GenerateUserID(),
                    Username = DEFAULT_ADMIN_USERNAME,
                    Forenames = "Administrator",
                    HashedPassword = "",
                    IsDisabled = true,
                    HasFirstLogin = true, // Don't enforce a password reset upon first login
                    DateCreated = DateTime.Now
                };
                AppDatabase.WriteModelUpdate(adminUser);

                // Request a password change, but use the newly created admin as the authorised user.
                //
                // This bypasses our permission checks, as we have no way of validating this request
                // without a real admin user.
                //
                // By creating a new admin user early and writing it to the db, we can act as if the
                // user has always existed in the system.
                Result<bool, Exception> result = HandlePasswordChangeRequest(adminUser, adminUser);

                if (result.IsErr() || (result.IsValue() && result.Value == false))
                {
                    Result<bool, Exception> existingAdminUser = DeleteOldAdminUser();

                    if (existingAdminUser.IsErr())
                    {
                        Debug.WriteLine("Failed to clean up existing admin user, aborting...");

                        return Result<bool, Exception>.Err(existingAdminUser.Error);
                    }
                }

                if (result.IsErr())
                {
                    return Result<bool, Exception>.Err(result.Error);
                }

                User? passwordSetUser = AppDatabase.QueryFirst<User>(
                    "SELECT * FROM tblUser WHERE Username=?",
                    [DEFAULT_ADMIN_USERNAME]
                );

                if (passwordSetUser == null)
                {
                    return Result<bool, Exception>.Err(
                        new Exception("Failed to find administrator user in system after password change.")
                    );
                }

                passwordSetUser.IsDisabled = false;
                AppDatabase.WriteModelUpdate(passwordSetUser);

                return Result<bool, Exception>.Ok(result.Value);
            }

            return Result<bool, Exception>.Ok(true);
        }
    }
}