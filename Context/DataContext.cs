using PMS.Controllers;
using PMS.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PMS.Context
{
    public class DataContext<T>
    {
        public Type Model { get; set; }
        public T[] DataSource { get; set; }
        public Dictionary<string, string> Columns { get; set; }
        public Dictionary<string, string> CompactColumns { get; set; }

        public FormItemBase[] Form { get; set; }
        public bool CanEdit
        {
            get
            {
                WindowManager wm = (WindowManager)Application.Current.MainWindow;

                return PermissionController.CanEditRecord(wm.AuthorisedUser, Model) != null;
            }
        }
        public bool CanCreate
        {
            get
            {
                WindowManager wm = (WindowManager)Application.Current.MainWindow;

                return PermissionController.CanCreateRecord(wm.AuthorisedUser, Model) != null;
            }
        }
    
        public object None { get; set; }
    }
}
