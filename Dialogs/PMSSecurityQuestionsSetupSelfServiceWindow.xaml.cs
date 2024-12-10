using Microsoft.IdentityModel.Tokens;
using PMS.Components;
using PMS.Controllers;
using PMS.Models;
using PMS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for PMSSecurityQuestionsSetupSelfServiceWindow.xaml
    /// </summary>
    public partial class PMSSecurityQuestionsSetupSelfServiceWindow : PMSWindow
    {
        private User TargetUser;

        public string Username
        {
            get => TargetUser.Username;
        }

        public bool DidSetAnswers = false;

        public Dictionary<int, string> SecurityQuestions
        {
            get => SecurityQuestion.GetSecurityQuestionOptions();
        }

        public SecurityQuestionAnswer? SecurityQuestion1
        {
            get => TargetUser.ComputedSecurityQuestion1;
        }

        public SecurityQuestionAnswer? SecurityQuestion2
        {
            get => TargetUser.ComputedSecurityQuestion2;
        }

        public PMSSecurityQuestionsSetupSelfServiceWindow(User TargetUser) : base()
        {
            this.TargetUser = TargetUser;

            InitializeComponent();

            DataContext = this;

            LogController.WriteLine($"Prompted to set security questions for '{TargetUser.Username}'.", LogCategory.SecurityQuestionsSSWindow);

            Closing += Window_Closing;

            Focus();
        }

        private void Window_Closing(object? sender, CancelEventArgs e)
        {
            if (!DidSetAnswers)
            {
                MessageBoxResult result = MessageBoxController.Show(
                    this,
                    "Without setting up security questions, you will be unable to recover your account yourself. Are you sure you want to skip?",
                    "Information",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information,
                    MessageBoxResult.No
                );

                e.Cancel = result != MessageBoxResult.Yes;
                return;
            }
        }

        private int? WriteSecurityQuestions(SecurityQuestionAnswer? sqa, int id, string questionID, string answer)
        {
            string[] args = [questionID, answer, TargetUser.ID.ToString(), $"{TargetUser.ID}-{id}"];

            int? rowsAffected = null;

            if (sqa == null)
            {
                rowsAffected = AppDatabase.Update(
                    "INSERT INTO tblUserSecurityQuestion (SecurityQuestionID, Answer, UserID, QuestionAnswerID) VALUES (?, ?, ?, ?)",
                    args
                );
            }
            else
            {
                rowsAffected = AppDatabase.Update(
                   "UPDATE tblUserSecurityQuestion SET SecurityQuestionID = ?, Answer = ? WHERE UserID = ? AND QuestionAnswerID = ?",
                   args
               );
            }

            if (rowsAffected <= 0)
            {
                MessageBoxController.Show(
                    this,
                    $"Failed to update Security Question {id}.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }

            return rowsAffected;
        }

        private void SetSQAnswers_Click(object sender, RoutedEventArgs e)
        {
            string? q1 = Question1.SelectedValue?.ToString();
            string? a1 = Question1_Answer.Text.ToString();

            string? q2 = Question2.SelectedValue?.ToString();
            string? a2 = Question2_Answer.Text.ToString();

            if ((q1.IsNullOrEmpty() || q1 == "0") || a1.IsNullOrEmpty())
            {
                MessageBoxController.Show(
                    this,
                    "Question 1 and Answer 1 cannot be blank.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }


            if ((q2.IsNullOrEmpty() || q2 == "0") || a2.IsNullOrEmpty())
            {
                MessageBoxController.Show(
                    this,
                    "Question 2 and Answer 2 cannot be blank.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            if (
                WriteSecurityQuestions(TargetUser.ComputedSecurityQuestion1, 1, q1!, a1) > 0 &&
                WriteSecurityQuestions(TargetUser.ComputedSecurityQuestion2, 2, q2!, a2) > 0
            )
            {
                DidSetAnswers = true;

                this.DialogResult = true;

                if (this.DialogResult == true)
                {

                    MessageBoxController.Show(
                        this,
                        $"The security questions for user '{TargetUser.Username}' have been successfully updated.",
                        "Security questions changed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    Close();
                }
            }
        }
    }
}
