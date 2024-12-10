using System;
using System.Collections;
using System.Collections.Generic;
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

namespace PMS.Components
{
    /// <summary>
    /// Interaction logic for PMSDataGrid.xaml
    /// </summary>
    public partial class PMSDataGrid : UserControl
    {
        public PMSDataGrid()
        {
            InitializeComponent();

            DataContext = this;
        }

        public DataGrid InnerDataGrid
        {
            get => DataGrid;
        }

        public static readonly DependencyProperty DataSourceProperty = DependencyProperty.Register("DataSource", typeof(IEnumerable), typeof(PMSDataGrid));

        public IEnumerable DataSource
        {
            get { return (IEnumerable)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }

        public static readonly RoutedEvent CurrentCellChangedEvent = EventManager.RegisterRoutedEvent("CurrentCellChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PMSDataGrid));
        public event RoutedEventHandler CurrentCellChanged
        {
            add { AddHandler(CurrentCellChangedEvent, value); }
            remove { RemoveHandler(CurrentCellChangedEvent, value); }
        }

        public static readonly RoutedEvent SelectedCellsChangedEvent = EventManager.RegisterRoutedEvent("SelectedCellsChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PMSDataGrid));
        public event RoutedEventHandler SelectedCellsChanged
        {
            add { AddHandler(SelectedCellsChangedEvent, value); }
            remove { RemoveHandler(SelectedCellsChangedEvent, value); }
        }

        public static readonly RoutedEvent DataItemDoubleClickEvent = EventManager.RegisterRoutedEvent("DataItemDoubleClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PMSDataGrid));
        public event RoutedEventHandler DataItemDoubleClick
        {
            add { AddHandler(DataItemDoubleClickEvent, value); }
            remove { RemoveHandler(DataItemDoubleClickEvent, value); }
        }

        private void OnDataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(CurrentCellChangedEvent));
        }

        private void OnDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(SelectedCellsChangedEvent));
        }

        public void OnDataItem_DoubleClick(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(DataItemDoubleClickEvent));
        }

        private void OnDataItem_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                e.Handled = true;
                RaiseEvent(new RoutedEventArgs(DataItemDoubleClickEvent));
            }
        }
    }
}
