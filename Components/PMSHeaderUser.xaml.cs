using PMS.Models;
using System;
using System.Collections.Generic;
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

        public string FormatUserFullName()
        {
            if (User is User)
            {
                List<string> nameParts = new();

                if (User.Title != Title.None)
                {
                    nameParts.Add(User.Title + ".");
                }

                nameParts.Add(User.Forenames);
                nameParts.Add(User.Surname);

                return string.Join(" ", nameParts);
            }
            else
            {
                return "Nobody";
            }
        }

        public string UserFullName
        {
            get => FormatUserFullName();
        }

        public string UserRole
        {
            get => User is not null ? User.UserType.ToString() : "Unknown";
        }

        public PMSHeaderUser()
        {
            InitializeComponent();

            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = (ContextMenu)FindResource("UserContextMenu");

            menu.PlacementTarget = (Button)sender;
            menu.IsOpen = !menu.IsOpen;
        }

        private void LogOff_Click(object sender, RoutedEventArgs e)
        {
            WindowManager wm = (WindowManager)Application.Current.MainWindow;
            wm.HandleLogOffRequest();
        }
    }
}
