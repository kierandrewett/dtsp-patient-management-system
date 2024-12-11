using PMS.Components;
using PMS.Controllers;
using PMS.Models;
using PMS.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for PMSPasswordChangeWindow.xaml
    /// </summary>
    public partial class PMSPasswordChangeWindow : PMSWindow
    {
        private User TargetUser;

        public string Username
        {
            get => TargetUser.Username;
        }

        public PMSPasswordChangeWindow(User TargetUser) : base()
        {
            this.TargetUser = TargetUser;

            InitializeComponent();

            DataContext = this;

            LogController.WriteLine($"Prompted to change password for '{TargetUser.Username}'.", LogCategory.PasswordChangeWindow);

            UpdatePasswordRequirements();
        }

        public void UpdatePasswordRequirements()
        {
            string password = PasswordField.Password.Trim();

            PasswordRequirement_CharsCount.IsChecked = password.Length >= AppConstants.PasswordMinLength;
            PasswordRequirement_LowerUpperChars.IsChecked = Regex.IsMatch(password, "^(?=.*[a-zA-Z]).*$");
            PasswordRequirement_Numbers.IsChecked = Regex.IsMatch(password, "^(?=.*\\d).*");
            PasswordRequirement_SpecialCharsCount.IsChecked = Regex.IsMatch(password, "[`!@#$£%^&*\\\\\\--_=+'\\\\\\/.,(){}\\[\\]|;:\"<>~]{1,}");
            PasswordRequirement_Match.IsChecked = password.Length >= 1 && PasswordField.Password == PasswordConfirmField.Password;

            bool isPasswordValid = (
                (PasswordRequirement_CharsCount.IsChecked ?? false) &&
                (PasswordRequirement_LowerUpperChars.IsChecked ?? false) &&
                (PasswordRequirement_Numbers.IsChecked ?? false) &&
                (PasswordRequirement_SpecialCharsCount.IsChecked ?? false) &&
                (PasswordRequirement_Match.IsChecked ?? false)
            );

            SubmitButton.IsEnabled = isPasswordValid;
        }

        private void PasswordField_KeyDown(object sender, KeyEventArgs e)
        {
            UpdatePasswordRequirements();
        }

        private void PasswordField_KeyUp(object sender, KeyEventArgs e)
        {
            UpdatePasswordRequirements();
        }

        private void PasswordField_TextInput(object sender, TextCompositionEventArgs e)
        {
            UpdatePasswordRequirements();

        }

        private void PasswordField_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void PasswordConfirmField_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            string newPassword = PasswordField.Password.Trim();

            WindowManager wm = (WindowManager)Application.Current.MainWindow;
            Result<bool, Exception> passwordChangedRequest = 
                wm.HandlePasswordChangeFinalRequest(TargetUser, newPassword);

            if (passwordChangedRequest.IsErr())
            {
                MessageBoxController.Show(
                    this,
                    passwordChangedRequest.Error.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            this.DialogResult = passwordChangedRequest.Value;

            if (passwordChangedRequest.Value == true)
            {
                MessageBoxController.Show(
                    this,
                    $"The password for user '{TargetUser.Username}' has been successfully updated.",
                    "Password changed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                Close();
            }
        }

        private void Button_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Button_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
