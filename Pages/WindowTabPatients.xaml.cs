using PMS.Components;
using PMS.Context;
using PMS.Controllers;
using PMS.Models;
using PMS.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Printing;
using System.Security.RightsManagement;
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
    /// Interaction logic for WindowTabPatients.xaml
    /// </summary>
    public partial class WindowTabPatients : UserControl
    {
        public WindowTabPatients()
        {
            InitializeComponent();

            DataContext = new PatientsDataContext();
        }
    }
}
