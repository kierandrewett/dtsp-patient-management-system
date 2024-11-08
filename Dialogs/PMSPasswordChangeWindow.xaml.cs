﻿using PMS.Models;
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
    public partial class PMSPasswordChangeWindow : Window
    {
        private User TargetUser;

        public string Username
        {
            get => TargetUser.Username;
        }

        public PMSPasswordChangeWindow(User TargetUser)
        {
            this.TargetUser = TargetUser;

            InitializeComponent();

            DataContext = this;

            Debug.WriteLine($"(Password Change Window): Prompted to change password for '{TargetUser.Username}'.");

            UpdatePasswordRequirements();
        }

        public void UpdatePasswordRequirements()
        {
            string password = PasswordField.Password.Trim();

            PasswordRequirement_CharsCount.IsChecked = password.Length >= AppConstants.PasswordMinLength;
            PasswordRequirement_LowerUpperChars.IsChecked = Regex.IsMatch(password, "^(?=.*[a-z])(?=.*[A-Z]).*$");
            PasswordRequirement_Numbers.IsChecked = Regex.IsMatch(password, "^(?=.*\\d).*");
            PasswordRequirement_SpecialCharsCount.IsChecked = Regex.IsMatch(password, "(?:[^`!@#$%^&*\\-_=+'\\/.,]*[`!@#$%^&*\\-_=+'\\/.,]){2,}");
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

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            string newPassword = PasswordField.Password.Trim();

            WindowManager wm = (WindowManager)Application.Current.MainWindow;
            Result<bool, Exception> passwordChangedRequest = 
                wm.HandlePasswordChangeFinalRequest(TargetUser, newPassword);

            if (passwordChangedRequest.IsErr())
            {
                MessageBox.Show(
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
                MessageBox.Show(
                    $"The password for user '{TargetUser.Username}' has been successfully updated.",
                    "Password changed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                Close();
            }
        }
    }
}
