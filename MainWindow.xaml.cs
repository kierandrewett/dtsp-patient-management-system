using PMS.Dialogs;
using PMS.Models;
using PMS.Pages;
using System.Diagnostics;
using System.Media;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PMS
{
    public enum WindowTab
    {
        Overview,
        Patients,
        Reporting,
        Dispensing,
        Referrals,
        Registration,
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Button? OldBtnTab;

        private WindowTab _SelectedTab;
        WindowTab SelectedTab
        {
            get
            {
                return _SelectedTab;
            }
            set
            {
                // Update the internal value
                _SelectedTab = value;

                SelectTab(value);
            }
        }

        WindowManager wm
        {
            get => (WindowManager)Application.Current.MainWindow;
        }

        public MainWindow()
        {
            InitializeComponent();

            if (wm.AuthorisedUser is User user)
            {
                Debug.WriteLine($"(Main Window): Authorised as {user.Username}.");
            }

            this.SelectedTab = WindowTab.Overview;
        }

        private void OnTabSelect(object sender, RoutedEventArgs e)
        {
            Button? btn = sender as Button;

            if (btn != null)
            {
                WindowTab newTab = WindowTab.Overview;

                switch (btn.Name)
                {
                    case "WindowTab_Overview":
                        newTab = WindowTab.Overview;
                        break;
                    case "WindowTab_Patients":
                        newTab = WindowTab.Patients;
                        break;
                    case "WindowTab_Reporting":
                        newTab = WindowTab.Reporting;
                        break;
                    case "WindowTab_Dispensing":
                        newTab = WindowTab.Dispensing;
                        break;
                    case "WindowTab_Referrals":
                        newTab = WindowTab.Referrals;
                        break;
                    case "WindowTab_Registration":
                        newTab = WindowTab.Registration;
                        break;
                    default:
                        MessageBox.Show("Received unexpected tab type.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                }

                this.SelectedTab = newTab;
            }
        }

        private async void SelectTab(WindowTab newTab)
        {
            string tabName = "Overview";
            object? tabInstance = new PMS.Pages.WindowTabUnknown();

            switch (newTab)
            {
                case WindowTab.Overview:
                    tabName = "Overview";
                    tabInstance = new PMS.Pages.WindowTabOverview();
                    break;
                case WindowTab.Patients:
                    tabName = "Patients";
                    break;
                case WindowTab.Reporting:
                    tabName = "Reporting";
                    break;
                case WindowTab.Dispensing:
                    tabName = "Dispensing";
                    break;
                case WindowTab.Referrals:
                    tabName = "Referrals";
                    break;
                case WindowTab.Registration:
                    tabName = "Registration";
                    break;
                default:
                    MessageBox.Show("Received unexpected tab type.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
            }

            object btnTab = FindName($"WindowTab_{tabName}");

            if (btnTab is Button tab)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                StatusBar.StatusLabel.Content = $"Loading {tabName}...";
                ContentFrame.Children.Clear();

                if (this.OldBtnTab != null)
                {
                    this.OldBtnTab.Background.Opacity = 0;
                    this.OldBtnTab.BorderBrush.Opacity = 0;
                }

                this.OldBtnTab = tab;

                tab.BorderBrush.Opacity = 1;
                tab.Background.Opacity = 1;

                Random rnd = new Random();
                await Task.Delay(rnd.Next(50, 200));

                if (tabInstance != null)
                {
                    Mouse.OverrideCursor = null;
                    StatusBar.StatusLabel.Content = "Ready";

                    ContentFrame.Children.Clear();
                    ContentFrame.Children.Add((FrameworkElement)tabInstance);
                    ((FrameworkElement)tabInstance).HorizontalAlignment = HorizontalAlignment.Stretch;
                    ((FrameworkElement)tabInstance).VerticalAlignment = VerticalAlignment.Stretch;
                }
            }
        }
    }
}