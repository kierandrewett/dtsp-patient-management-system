using Microsoft.IdentityModel.Tokens;
using PMS.Controllers;
using PMS.Models;
using PMS.Util;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for PMSAccountRecoverySelfServiceWindow.xaml
    /// </summary>
    public partial class PMSAccountRecoverySelfServiceWindow : Window
    {
        public PMSAccountRecoverySelfServiceWindow()
        {
            InitializeComponent();

            DataContext = this;

            UsernameField.Focus();
        }

        public string[]? SecurityQuestionStr
        {
            get => AuthenticationController.GetAllSecurityQuestions()?
                    .Select(q => q.Question).ToArray();
        }

        private void LastResort_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Without the answers to your security questions, you will be unable to recover your account yourself. Please contact the system administrator for further steps.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Recover_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameField.Text.Trim();

            int question1ID = Question1.SelectedIndex;
            string question1Answer = Question1_Answer.Text.Trim();

            int question2ID = Question2.SelectedIndex;
            string question2Answer = Question2_Answer.Text.Trim();

            if (username.IsNullOrEmpty())
            {
                MessageBox.Show(
                    "Username must not be blank.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            if (question1ID < 0 || question1Answer.IsNullOrEmpty())
            {
                MessageBox.Show(
                    "Question 1 and Answer must not be blank.", 
                    "Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error
                );
                return;
            }

            if (question2ID < 0 || question2Answer.IsNullOrEmpty())
            {
                MessageBox.Show(
                    "Question 2 and Answer must not be blank.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            SecurityQuestion? question1 = AuthenticationController.GetSecurityQuestionByID(question1ID);
            SecurityQuestion? question2 = AuthenticationController.GetSecurityQuestionByID(question2ID);

            if (question1 == null || question2 == null)
            {
                throw new Exception("Security question data is not available to application.");
            }

            Dictionary<SecurityQuestion, string> questionToAnswersMap = new();

            questionToAnswersMap.Add(question1, question1Answer);
            questionToAnswersMap.Add(question2, question2Answer);

            WindowManager wm = (WindowManager)Application.Current.MainWindow;
            Result<User, Exception> accountRecoveryResult = 
                wm.HandleSecurityQuestionsRecoveryRequest(username, questionToAnswersMap);

            if (accountRecoveryResult.IsErr())
            {
                MessageBox.Show(
                    accountRecoveryResult.Error.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            Hide();

            Result<bool, Exception> passwordChangeResult = 
                wm.HandlePasswordChangeRequest(accountRecoveryResult.Value);

            if (passwordChangeResult.IsErr())
            {
                MessageBox.Show(
                    passwordChangeResult.Error.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            if (passwordChangeResult.Value)
            {
                Close();
            } else
            {
                ShowDialog();
            }
        }
    }
}
