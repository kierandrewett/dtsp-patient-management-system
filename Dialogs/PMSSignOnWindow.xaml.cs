using PMS.Models;
using PMS.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

            this.DialogResult = true;
            Close();
        }
    }
}
