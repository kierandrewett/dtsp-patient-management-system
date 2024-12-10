using PMS.Components;
using PMS.Context;
using PMS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PMS.Pages
{
    /// <summary>
    /// Interaction logic for WindowTabOverview.xaml
    /// </summary>
    public partial class WindowTabOverview : UserControl, INotifyPropertyChanged
    {
        public SchedulingDataContext Appointments { get; set; }
        public PatientsDataContext Patients { get; set; }
        public UsersDataContext Users { get; set; }


        public ObservableCollection<DataItem> AppointmentsDataSource { get; set; }
        public ObservableCollection<DataItem> PatientsDataSource { get; set; }
        public ObservableCollection<DataItem> UsersDataSource { get; set; }

        public WindowTabOverview()
        {
            InitializeComponent();

            WindowManager wm = (WindowManager)Application.Current.MainWindow;
            bool isAdmin = wm.AuthorisedUser?.UserType == UserType.Admin;

            AppointmentsGrid.Visibility = !isAdmin ? Visibility.Visible : Visibility.Collapsed;
            PatientsGrid.Visibility = !isAdmin ? Visibility.Visible : Visibility.Collapsed;
            UsersGrid.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;

            if (isAdmin)
            {
                Users = new UsersDataContext();

                UsersDataSource = Users.CreateObservableDataSource();
                Users.RenderColumns(UsersDataGrid.InnerDataGrid, UsersDataSource, true);

                UsersDataGrid.Visibility = UsersDataSource.Count > 0 ? Visibility.Visible : Visibility.Hidden;
                UsersErrorBoundary.Visibility = UsersDataGrid.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
            } else
            {
                Patients = new PatientsDataContext(wm.AuthorisedUser?.ID);

                PatientsDataSource = Patients.CreateObservableDataSource();
                Patients.RenderColumns(PatientsDataGrid.InnerDataGrid, PatientsDataSource, true);

                PatientsDataGrid.Visibility = PatientsDataSource.Count > 0 ? Visibility.Visible : Visibility.Hidden;
                PatientsErrorBoundary.Visibility = PatientsDataGrid.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;

                Appointments = new SchedulingDataContext();

                AppointmentsDataSource = Appointments.CreateObservableDataSource();
                Appointments.RenderColumns(AppointmentsDataGrid.InnerDataGrid, AppointmentsDataSource, true);

                AppointmentsDataGrid.Visibility = AppointmentsDataSource.Count > 0 ? Visibility.Visible : Visibility.Hidden;
                AppointmentsErrorBoundary.Visibility = AppointmentsDataGrid.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
            }
        }

        private WindowManager wm
        {
            get => (WindowManager)Application.Current.MainWindow;
        }

        public string UserFullName
        {
            get => wm.AuthorisedUser?.FormatFullName() ?? "Nobody";
        }

        public string UserSubtitleText
        {
            get => $"{wm.AuthorisedUser?.DepartmentWithID} ({wm.AuthorisedUser?.UserType.ToString()})";
        }

        public bool HasPatientSelectedItems
        {
            get => PatientsDataGrid.InnerDataGrid.SelectedIndex != -1;
        }

        public bool HasAppointmentsSelectedItems
        {
            get => AppointmentsDataGrid.InnerDataGrid.SelectedIndex != -1;
        }
        public bool HasUsersSelectedItems
        {
            get => UsersDataGrid.InnerDataGrid.SelectedIndex != -1;
        }

        private void HandleDataGridItemClick(object sender, WindowTab tab, bool openSelected = false)
        {
            if (sender is PMSDataGrid dataGrid)
            {
                WindowManager wm = (WindowManager)Application.Current.MainWindow;

                if (wm.MainWindow is MainWindow mainWindow)
                {
                    DataItem[] selected = openSelected 
                        ? dataGrid.InnerDataGrid.SelectedItems
                            .OfType<DataItem>()
                            .ToArray() 
                        : Array.Empty<DataItem>();

                    mainWindow.TabsController.LoadTab(tab, selected);
                }
            }

        }

        private void OnPatientsViewSelected_Click(object sender, RoutedEventArgs e)
        {
            HandleDataGridItemClick(
                sender is PMSDataGrid ? sender : PatientsDataGrid, 
                WindowTab.Patients, 
                true
            );
        }

        private void OnAppointmentsViewSelected_Click(object sender, RoutedEventArgs e)
        {
            HandleDataGridItemClick(
                sender is PMSDataGrid ? sender : AppointmentsDataGrid, 
                WindowTab.Scheduling, 
                true
            );
        }
        private void OnUsersViewSelected_Click(object sender, RoutedEventArgs e)
        {
            HandleDataGridItemClick(
                sender is PMSDataGrid ? sender : UsersDataGrid, 
                WindowTab.Users, 
                true
            );
        }

        private void OnPatientsView_Click(object sender, RoutedEventArgs e)
        {
            HandleDataGridItemClick(
                sender is PMSDataGrid ? sender : PatientsDataGrid,
                WindowTab.Patients
            );
        }

        private void OnAppointmentsView_Click(object sender, RoutedEventArgs e)
        {
            HandleDataGridItemClick(
                sender is PMSDataGrid ? sender : AppointmentsDataGrid,
                WindowTab.Scheduling
            );
        }
        private void OnUsersView_Click(object sender, RoutedEventArgs e)
        {
            HandleDataGridItemClick(
                sender is PMSDataGrid ? sender : UsersDataGrid,
                WindowTab.Users
            );
        }

        private void OnDataGrid_SelectedCellsChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PMSDataGrid dataGrid)
            {
                foreach (var item in (ObservableCollection<DataItem>)dataGrid.DataSource)
                {
                    item.IsSelected = dataGrid.InnerDataGrid.SelectedItems.Contains(item);
                }
            }
            

            DidUpdateProperty("HasPatientSelectedItems");
            DidUpdateProperty("HasAppointmentsSelectedItems");
            DidUpdateProperty("HasUsersSelectedItems");
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void DidUpdateProperty(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
