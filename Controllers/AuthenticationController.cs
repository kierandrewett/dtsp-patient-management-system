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

namespace PMS.Controllers
{
    public class AuthenticationController
    {
        private int FailedLoginAttempts = 0;
        private DateTime? LoginUnlockedAt = null;

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

            User? user = AppDatabase.Query<User>(
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
    }
}
