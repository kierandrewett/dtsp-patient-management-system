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

namespace PMS.Controllers
{
    public class AuthenticationController
    {
        private int FailedLoginAttempts = 0;
        private DateTime? LoginUnlockedAt = null;

        private bool SecurityQuestionsVerifiedLock = false;

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

        public async Task<Result<User, Exception>> HandleAuthenticationRequest(string username, string password)
        {
            // Add artificial delay to prevent automated attacks
            Random rng = new();
            await Task.Delay(rng.Next(300, 800));

            Exception? blocked = MaybeBlockRequest();
            if (blocked != null)
            {
                return Result<User, Exception>.Err(blocked);
            }

            User? user = AppDatabase.QueryFirst<User>(
                "SELECT * FROM tblUser WHERE Username=? AND Password=?", 
                [username, password]
            );

            if (user != null)
            {
                return Result<User, Exception>.Ok(user);
            }

            RecordFailedLoginAttempt();

            return Result<User, Exception>.Err(
                new Exception("Username or password is invalid, please try again.")
            );
        }
        private Result<SecurityQuestionAnswer[], Exception> GetUserSecurityQuestionAnswers(User user)
        {
            string query =
                "SELECT tblSecurityQuestion.Question, tblUserSecurityQuestion.Answer, tblUserSecurityQuestion.SecurityQuestionID AS QuestionID " +
                "FROM((tblSecurityQuestion INNER JOIN " +
                "tblUserSecurityQuestion ON tblSecurityQuestion.ID = tblUserSecurityQuestion.SecurityQuestionID) INNER JOIN " +
                "tblUser ON tblUserSecurityQuestion.UserID = tblUser.ID) " +
                "WHERE(tblUser.ID = ?)";

            SecurityQuestionAnswer[]? securityQuestions = AppDatabase.QueryAll<SecurityQuestionAnswer>(
                query,
                [user.ID.ToString()]
            );

            if (securityQuestions == null)
            {
                return Result<SecurityQuestionAnswer[], Exception>.Err(
                    new Exception("No security questions have been set-up, contact your system administrator for more information.")
                );
            }

            return Result<SecurityQuestionAnswer[], Exception>.Ok(securityQuestions);
        }

        public static SecurityQuestion[]? GetAllSecurityQuestions()
        {
            return AppDatabase.QueryAll<SecurityQuestion>(
                "SELECT * FROM tblSecurityQuestion",
                []
            );
        }

        public static SecurityQuestion? GetSecurityQuestionByID(int id)
        {
            return AppDatabase.QueryFirst<SecurityQuestion>(
               "SELECT * FROM tblSecurityQuestion WHERE ID=?",
               [id.ToString()]
           );
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

            Result<SecurityQuestionAnswer[], Exception> securityQuestionAnswersResult =
                GetUserSecurityQuestionAnswers(user);

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
                        entry.Key.ID == questionAnswer.QuestionID &&
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
            Debug.WriteLine("(Permission Controller): Permitting security question verification.");

            return Result<User, Exception>.Ok(user);
        }

        public Result<User, Exception> HandlePasswordChangeRequest(User? authorisedUser = null, User? targetUser = null)
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

        public Result<bool, Exception> HandlePasswordChangeFinalRequest(User targetUser, string newPassword)
        {
            int? updatedRows = AppDatabase.Update(
                "UPDATE tblUser SET [Password]=? WHERE (ID=?)",
                [newPassword, targetUser.ID.ToString()]
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
    }
}