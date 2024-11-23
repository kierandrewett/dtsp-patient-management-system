using PMS.Controllers;
using PMS.Models;
using PMS.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PMS.Dialogs
{
    /// <summary>
    /// Interaction logic for PMSSignOnWindow.xaml
    /// </summary>
    public partial class PMSSignOnWindow : Window
    {
        public PMSSignOnWindow()
        {
            InitializeComponent();

            Title = $"Sign on to {AppConstants.AppName}";

            UsernameField.Focus();
        }

        private async void SignIn_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameField.Text.Trim();
            string password = PasswordField.Password.Trim();

            if (username.Length <= 0)
            {
                MessageBox.Show("Username must not be blank.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (password.Length <= 0)
            {
                MessageBox.Show("Password must not be blank.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SubmitButton.Content = "Signing in...";

            ProgressBar.Opacity = 100;
            ProgressBar.IsIndeterminate = true;
            UsernameField.IsEnabled = false;
            PasswordField.IsEnabled = false;
            SubmitButton.IsEnabled = false;

            WindowManager wm = (WindowManager)Application.Current.MainWindow;
            Result<User, Exception> authenticationResult = await wm.HandleAuthenticationRequest(username, password);

            if (authenticationResult.IsErr())
            {
                ProgressBar.Opacity = 0;
                ProgressBar.IsIndeterminate = false;

                MessageBox.Show(authenticationResult.Error.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                SubmitButton.Content = "Sign in";

                UsernameField.IsEnabled = true;
                PasswordField.IsEnabled = true;
                SubmitButton.IsEnabled = true;

                return;
            }

            User user = authenticationResult.Value;
            wm.AuthorisedUser = user;

            // Ignore errors for this weird edge case where
            // authenticating too quickly causes an error.
            try
            {
                this.DialogResult = true;
            } catch (Exception ex) { }

            Close();
        }

        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            PMSAccountRecoverySelfServiceWindow accRecSSWindow = new();
            accRecSSWindow.Owner = this;
            Hide();
            accRecSSWindow.ShowDialog();
            ShowDialog();
        }

#if DEBUG
        private void DebugSignIn(UserType userType, object sender, RoutedEventArgs e)
        {
            if (AppConstants.IsDebug)
            {
                User[]? users = User.GetUsersByType(userType);

                if (users != null && users.Length >= 1)
                {
                    Random rng = new Random();
                    int userIndex = rng.Next(0, users.Length);
                    User user = users[userIndex];

                    UsernameField.Text = user.Username;
                    PasswordField.Password = user.Password;
                    SignIn_Click(sender, e);
                } else
                {
                    MessageBox.Show(
                        "No users found for that user type!",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
        }

        private void DebugDoctorSignIn_Click(object sender, RoutedEventArgs e)
        {
            DebugSignIn(UserType.Doctor, sender, e);
        }

        private void DebugNurseSignIn_Click(object sender, RoutedEventArgs e)
        {
            DebugSignIn(UserType.Nurse, sender, e);
        }

        private void DebugAdminSignIn_Click(object sender, RoutedEventArgs e)
        {
            DebugSignIn(UserType.Admin, sender, e);
        }
    }
#endif
}
