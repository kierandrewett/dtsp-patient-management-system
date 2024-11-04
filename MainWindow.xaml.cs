using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        #if DEBUG
        bool IsDebug = true;
#else
        bool IsDebug = false;
#endif

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

        public string BuildEnv
        {
            get => IsDebug ? "Debug" : "Release";
        }
        public string AppVersion
        {
            get
            {
                Version? ver = Assembly.GetExecutingAssembly().GetName().Version;
                return ver != null ? ver.ToString() : "0.0.0";
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            Title = $"Patient Record System ({BuildEnv}) - Version: {AppVersion}";
        }

        private void OnTabSelect(object sender, RoutedEventArgs e)
        { 
            Button? btn = sender as Button;

            if (btn != null)
            {
                // Fallback to the overview tab if we cannot
                // find an appropriate tab to switch to.
                WindowTab newTab = WindowTab.Overview;

                switch (btn.Content)
                {
                    case "Overview":
                        newTab = WindowTab.Overview;
                        break;
                    case "Patients":
                        newTab = WindowTab.Patients;
                        break;
                    case "Reporting":
                        newTab = WindowTab.Reporting;
                        break;
                    case "Dispensing":
                        newTab = WindowTab.Dispensing;
                        break;
                    case "Referrals":
                        newTab = WindowTab.Referrals;
                        break;
                    case "Registration":
                        newTab = WindowTab.Registration;
                        break;
                    default:
                        MessageBox.Show("Received unexpected tab type.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }

                this.SelectedTab = newTab;
            }
        }

        private void SelectTab(WindowTab newTab)
        {
            switch (newTab)
            {
                case WindowTab.Overview:
                    ContentFrame.Navigate("https://google.com");
                    break;
                default:
                    ContentFrame.Navigate("https://bing.com");
                    break;
            }
        }
    }
}