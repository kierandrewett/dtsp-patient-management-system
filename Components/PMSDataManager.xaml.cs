using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for PMSDataManager.xaml
    /// </summary>
    public partial class PMSDataManager : UserControl
    {
        public static readonly DependencyProperty PanelTitleProperty = DependencyProperty.Register("PanelTitle", typeof(string), typeof(PMSDataManager));
        public string PanelTitle
        {
            get { return (string)GetValue(PanelTitleProperty); }
            set { SetValue(PanelTitleProperty, value); }
        }

        public static readonly DependencyProperty PanelIconProperty = DependencyProperty.Register("PanelIcon", typeof(Geometry), typeof(PMSDataManager));
        public string PanelIcon
        {
            get { return (string)GetValue(PanelIconProperty); }
            set { SetValue(PanelIconProperty, value); }
        }

        public static readonly DependencyProperty DataSourceProperty = DependencyProperty.Register("DataSource", typeof(IEnumerable), typeof(PMSDataManager));

        public IEnumerable DataSource
        {
            get { return (IEnumerable)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }

        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register("Columns", typeof(Dictionary<string, string>), typeof(PMSDataManager));

        public event PropertyChangedEventHandler? PropertyChanged;

        public Dictionary<string, string> Columns
        {
            get { return (Dictionary<string, string>)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public PMSDataManager()
        {
            InitializeComponent();

            DataContext = this;
        }

        private void OnSearchBoxUpdated(object sender, RoutedEventArgs e)
        {
            string filter = PanelHeader.SearchBoxValue;

            PanelDataView.ApplyDataGridFilter(filter);
        }
    }
}
