using PMS.Dialogs;
using PMS.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PMS.Components
{
    /// <summary>
    /// Interaction logic for PMSHeaderUser.xaml
    /// </summary>
    public partial class PMSHeaderUser : UserControl
    {
        public User? User
        {
            get => ((WindowManager)Application.Current.MainWindow).AuthorisedUser;
            set => ((WindowManager) Application.Current.MainWindow).AuthorisedUser = value;
        }

        public string UserFullName
        {
            get => User?.FormatFullName() ?? "Nobody";
        }

        public string UserRole
        {
            get => User is not null ? User.UserType.ToString() : "Unknown";
        }

        private bool _PreventMenuOpen { get; set; }
        private DispatcherTimer _ResetMenuTimer { get; set; }


        public PMSHeaderUser()
        {
            InitializeComponent();

            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = (ContextMenu)FindResource("UserContextMenu");

            if (_PreventMenuOpen)
            {
                return;
            }

            menu.PlacementTarget = (Button)sender;
            menu.IsOpen = !menu.IsOpen;

            menu.Opened += Menu_Opened;
            menu.Closed += Menu_Closed;
        }

        private void Menu_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu contextMenu)
            {
                if (Window.GetWindow(this) is PMSWindow win)
                {
                    if (contextMenu.Parent is Popup popup)
                    {
                        win.AccessibilityController.RegisterPopup(popup);
                    }
                }
            }
        }

        private void Menu_Closed(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu contextMenu)
            {
                if (Window.GetWindow(this) is PMSWindow win)
                {
                    if (contextMenu.Parent is Popup popup)
                    {
                        win.AccessibilityController.UnregisterPopup(popup);
                    }
                }
            }

            _PreventMenuOpen = true;

            if (_ResetMenuTimer == null)
            {
                _ResetMenuTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
                _ResetMenuTimer.Tick += ResetPreventMenuOpenFlag;
            }

            _ResetMenuTimer.Start();
        }

        private void ResetPreventMenuOpenFlag(object sender, EventArgs e)
        {
            _ResetMenuTimer.Stop();
            _PreventMenuOpen = false;
        }

        private void LogOff_Click(object sender, RoutedEventArgs e)
        {
            WindowManager wm = (WindowManager)Application.Current.MainWindow;
            wm.HandleLogOffRequest();
        }

        private void Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Since we've observed a new mouse down, reset the timer now before we trigger the click
            ResetPreventMenuOpenFlag(sender, null);
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            PMSSettingsWindow sw = new();
            sw.ShowDialog();
        }
    }
}
