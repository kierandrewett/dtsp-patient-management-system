using PMS.Components;
using PMS.Controllers;
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
        Scheduling,
        Registration,
        Users,
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TabController<WindowTab> TabsController;

        private TabContent<WindowTab>[] Tabs = [];

        private WindowManager wm
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

            Init();
        }

        private void Init()
        {
            User user = wm.AuthorisedUser;

            this.Tabs =
            [
                .. this.Tabs,
                new TabContent<WindowTab>(
                    WindowTab.Overview,
                    "Overview",
                    new WindowTabOverview(),
                    (tab) => PermissionController.CanAccessTabContent(user, tab) != null
                ),
                new TabContent<WindowTab>(
                    WindowTab.Patients,
                    "Patients",
                    new WindowTabUnknown(),
                    (tab) => PermissionController.CanAccessTabContent(user, tab) != null
                ),
                new TabContent<WindowTab>(
                    WindowTab.Scheduling,
                    "Scheduling",
                    new WindowTabUnknown(),
                    (tab) => PermissionController.CanAccessTabContent(user, tab) != null
                ),
                new TabContent<WindowTab>(
                    WindowTab.Registration,
                    "Registration",
                    new WindowTabUnknown(),
                    (tab) => PermissionController.CanAccessTabContent(user, tab) != null
                ),
                new TabContent<WindowTab>(
                    WindowTab.Users,
                    "Users",
                    new WindowTabUsers(),
                    (tab) => PermissionController.CanAccessTabContent(user, tab) != null
                )
            ];

            this.TabsController = new TabController<WindowTab>(
                this,
                TabsStrip,
                TabsContent,
                Tabs
            );
        }

        private void OnBrandName_Click(object sender, RoutedEventArgs e)
        {
            this.TabsController.SelectedTab = this.Tabs.First(t => t.Value == WindowTab.Overview);
        }
    }
}