using PMS.Components;
using PMS.Controllers;
using PMS.Dialogs;
using PMS.Models;
using PMS.Util;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Speech.Synthesis;
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

namespace PMS
{
    /// <summary>
    /// Interaction logic for WindowManager.xaml
    /// </summary>
    public partial class WindowManager : PMSWindow
    {
        public PMSSignOnWindow? SignOnWindow = null;
        public MainWindow? MainWindow = null;

        public User? AuthorisedUser;

        private AuthenticationController AuthenticationController;
        private SettingsController SettingsController;
        public SynthesizerController SynthesizerController;

        public WindowManager()
        {
            InitializeComponent();

            Init();

            // Wait for all windows to be closed before closing the core window
            WaitForShutdown();
        }

        public void Init()
        {
            AuthenticationController = new AuthenticationController(this);
            SettingsController = new SettingsController();
            SynthesizerController = new SynthesizerController(this);

            // Ensure we have the settings dirs set up
            SettingsController.ReadSettings();

            // If we have window remnants, clean them up
            MainWindow?.Close();
            MainWindow = null;

            SignOnWindow?.Close();
            SignOnWindow = null;

            // Ensure the core window is hidden from view upon startup
            Hide();

            // Check that we have seeded the database with our initial user
            MaybeHandleNoUsers();

            // Launch the sign on window
            bool didSignIn = ShowSignOnWindow();

            // If we successfully signed on, open the main window
            if (IsAuthorisedUser() && didSignIn)
            {
                ShowMainWindow();
            }
            else if (IsAuthorisedUser() || didSignIn)
            {
                throw new Exception("Mismatch in authorised user and sign on completion state.");
            }
        }

        public async void WaitForShutdown()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    var windowCount = 0;

                    // Ask the UI thread for the current window count
                    try
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            windowCount = Application.Current.Windows.Count;
                        });
                    } catch (Exception ex) { }

                    // Check if we are the last surviving window
                    if (windowCount <= 1)
                    {
                        break;
                    }
                }
            });

            // Shutdown the application
            Application.Current.Shutdown();
        }

        public bool ShowSignOnWindow()
        {
            SignOnWindow ??= new();
            return (bool?)SignOnWindow.ShowDialog() ?? false;
        }

        public void ShowMainWindow()
        {
            // Only one main window may exist at one time
            if (MainWindow == null)
            {
                MainWindow = new();
            }

            MainWindow.Show();
        }

        public async Task<Result<User, Exception>> HandleAuthenticationRequest(string username, string password)
        {
            return await AuthenticationController.HandleAuthenticationRequest(username, password);
        }

        public bool IsAuthorisedUser()
        {
            return AuthorisedUser != null && AuthorisedUser is User;
        }

        public void HandleLogOffRequest()
        {
            if (
                this.MainWindow != null &&
                this.MainWindow!.ChangesProtectionController.UnsavedChangesLock && 
                !ChangesProtectionController.UnsavedChangesGuard(this)
            ) {
                return;
            }

            this.MainWindow!.ChangesProtectionController.UnsavedChangesLock = false;

            AuthorisedUser = null;

            Init();
        }
        public Result<User, Exception> HandleSecurityQuestionsRecoveryRequest(string username, Dictionary<SecurityQuestion, string> questionToAnswersMap)
        {
            if (AuthorisedUser != null)
            {
                throw new Exception("Unable to handle security questions recovery request with authorised user.");
            }

            return AuthenticationController.HandleSecurityQuestionsRecoveryRequest(
                username, 
                questionToAnswersMap
            );
        }

        public Result<bool, Exception> HandlePasswordChangeRequest(User? targetUser = null)
        {
            return AuthenticationController.HandlePasswordChangeRequest(targetUser);
        }

        public Result<bool, Exception> HandlePasswordChangeFinalRequest(User targetUser, string newPassword)
        {
            return AuthenticationController.HandlePasswordChangeFinalRequest(targetUser, newPassword, AuthorisedUser);
        }

        public Result<bool, Exception> MaybeHandleNoUsers()
        {
            Result<bool, Exception> result = AuthenticationController.MaybeHandleNoUsers();

            if (result.IsErr())
            {
                MessageBox.Show(
                    $"Failed to setup initial system user:\n\n{result.Error.ToString()}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                Close();
                Application.Current.Shutdown();
            }

            if (result.IsValue() && result.Value != true)
            {
                Close();
                Application.Current.Shutdown();
            }

            return Result<bool, Exception>.Ok(result.Value);
        }
    }
}
