using PMS.Components;
using PMS.Controllers;
using PMS.Dialogs;
using PMS.Models;
using PMS.Pages;
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
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public TabController<WindowTab> TabsController;

        private TabContent<WindowTab>[] Tabs = [];

        private WindowManager wm
        {
            get => (WindowManager)Application.Current.MainWindow;
        }

        public string WindowTitle
        {
            get {
                string title = AppConstants.AppComputedTitle;

                if (this.UnsavedChangesLock)
                {
                    title += " - (Unsaved changes)";
                }

                return title;
            }
        }

        private bool _UnsavedChangesLock = false;

        public bool UnsavedChangesLock
        {
            get => _UnsavedChangesLock;
            set
            {
                _UnsavedChangesLock = value;

                DidUpdateProperty("WindowTitle");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void DidUpdateProperty(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(property));
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            if (wm.AuthorisedUser is User user)
            {
                Debug.WriteLine($"(Main Window): Authorised as {user.Username}.");
            }

            Closing += MainWindow_Closing;

            Init();

            DataContext = this;
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // Prevent the window from closing if we have unsaved changes
            if (this.UnsavedChangesLock)
            {
                e.Cancel = !ChangesProtectionController.UnsavedChangesGuard();
                return;
            }
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
                    () => new WindowTabOverview(),
                    (tab) => PermissionController.CanAccessTabContent(user, tab) != null
                ),
                new TabContent<WindowTab>(
                    WindowTab.Patients,
                    "Patients",
                    () => new WindowTabPatients(),
                    (tab) => PermissionController.CanAccessTabContent(user, tab) != null
                ),
                new TabContent<WindowTab>(
                    WindowTab.Scheduling,
                    "Scheduling",
                    () => new WindowTabScheduling(),
                    (tab) => PermissionController.CanAccessTabContent(user, tab) != null
                ),
                new TabContent<WindowTab>(
                    WindowTab.Users,
                    "Users",
                    () => new WindowTabUsers(),
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
                    tab.RenderContent();
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