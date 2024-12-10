using PMS.Components;
using PMS.Controllers;
using PMS.Dialogs;
using PMS.Models;
using PMS.Pages;
using PMS.Util;
using System.ComponentModel;
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
        Edit
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : PMSLockableWindow
    {
        public TabController<WindowTab> TabsController;

        private TabContent<WindowTab>[] Tabs = [];

        private WindowManager wm
        {
            get => (WindowManager)Application.Current.MainWindow;
        }

        public MainWindow() : base()
        {
            InitializeComponent();

            if (wm.AuthorisedUser is User user)
            {
                LogController.WriteLine($"Authorised as {user.Username}.", LogCategory.MainWindow);
            }

            Init();

            DataContext = this;
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
                    (_) => new WindowTabOverview(),
                    (tab) => PermissionController.CanAccessTabContent(user, tab) != null
                ),
                new TabContent<WindowTab>(
                    WindowTab.Patients,
                    "Patients",
                    (arg) => new WindowTabPatients(arg as DataItem[]),
                    (tab) => PermissionController.CanAccessTabContent(user, tab) != null
                ),
                new TabContent<WindowTab>(
                    WindowTab.Scheduling,
                    "Scheduling",
                    (arg) => new WindowTabScheduling(arg as DataItem[]),
                    (tab) => PermissionController.CanAccessTabContent(user, tab) != null
                ),
                new TabContent<WindowTab>(
                    WindowTab.Users,
                    "Users",
                    (arg) => new WindowTabUsers(arg as DataItem[]),
                    (tab) => PermissionController.CanAccessTabContent(user, tab) != null
                ),
            ];

            // Start tabs functionality sanity check
            if (AppConstants.IsDebug)
            {
                foreach (TabContent<WindowTab> tab in this.Tabs)
                {
                    // Just attempt to render each tab's content
                    // if it fails at all, we'll know at window init
                    // rather than when the tab is loaded in.
                    tab.RenderContent(null);
                }
            }

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