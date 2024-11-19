using PMS.Controllers;
using PMS.Models;
using PMS.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Printing;
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
    public class DataUser : PropertyObservable
    {
        protected User _User;
        public User User
        {
            get { return _User; }
            set
            {
                _User = value;
                DidUpdateProperty("User");
            }
        }

        protected bool _IsSelected;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                _IsSelected = value;
                DidUpdateProperty("IsSelected");
            }
        }
    }

    /// <summary>
    /// Interaction logic for WindowTabUsers.xaml
    /// </summary>
    public partial class WindowTabUsers : UserControl
    {
        public ObservableCollection<DataUser> Users { get; set; }

        public WindowTabUsers()
        {
            InitializeComponent();

            User[] users = UserController.GetAllUsers() ?? Array.Empty<User>();
            DataUser[] mappedUsers = users.Select(u => new DataUser
            {
                User = u,
                IsSelected = false
            }).ToArray();

            Users = new ObservableCollection<DataUser>(mappedUsers);

            DataContext = this;
        }

        private void OnSearchBoxUpdated(object sender, object e)
        {
            ICollectionView cvUsers = CollectionViewSource.GetDefaultView(
                DataGrid_Users.ItemsSource
            );

            string filter = SearchBox.Text.Trim();

            cvUsers.Filter = new Predicate<object>(du => SearchController.SimpleSearchPredicate(filter)(((DataUser)du).User));
        }

        private void OnDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            foreach (var user in Users)
            {
                Debug.WriteLineIf(DataGrid_Users.SelectedItems.Contains(user), user.User.Username);
                user.IsSelected = DataGrid_Users.SelectedItems.Contains(user);
            }
        }
    }
}
