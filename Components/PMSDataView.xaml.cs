using PMS.Controllers;
using PMS.Models;
using PMS.Pages;
using PMS.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
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
    public class DataItem : PropertyObservable
    {
        protected object _Value;
        public object Value
        {
            get { return _Value; }
            set
            {
                _Value = value;
                DidUpdateProperty("Value");
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
    /// Interaction logic for PMSDataView.xaml
    /// </summary>
    public partial class PMSDataView : UserControl, INotifyPropertyChanged
    {
        public PMSDataView()
        {
            InitializeComponent();

            DataContext = this;
        }

        private ObservableCollection<DataItem> _ObservableDataSource;
        public ObservableCollection<DataItem> ObservableDataSource
        {
            get
            {
                if (this._ObservableDataSource == null)
                {
                    ForceUpdateDataSource();
                }

                return this._ObservableDataSource;
            }
        }

        private static void OnDataSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Debug.WriteLine("data source changed" + obj);
            if (obj is PMSDataView dv)
            {
                dv.ForceUpdateDataSource();
            }
        }

        public void ForceUpdateDataSource()
        {
            if (DataSource != null)
            {
                _ObservableDataSource = new ObservableCollection<DataItem>();

                foreach (var item in DataSource)
                {
                    _ObservableDataSource.Add(new DataItem
                    {
                        Value = item,
                        IsSelected = false
                    });
                }

                _ObservableDataSource.CollectionChanged += OnObservableDataSource_Changed;
                ForceUpdateDataGridColumns();

                DidUpdateProperty("ObservableDataSource");
            }
        }

        public void ForceUpdateDataGridColumns()
        {
            // In the cases when any of these are true
            // don't bother updating datagrid columns.
            if (
                Columns == null || 
                ObservableDataSource == null || 
                ObservableDataSource?.Count <= 0
            ) {
                return;
            }

            DataGrid.Columns.Clear();

            // Add selected column
            DataGridCheckBoxColumn isSelectedColumn = new()
            {
                Binding = new Binding("IsSelected")
            };

            DataGrid.Columns.Add(isSelectedColumn);

            Type? modelDataType = ObservableDataSource?.FirstOrDefault()?.Value.GetType();

            foreach (KeyValuePair<string, string> pair in Columns)
            {
                string modelName = pair.Key;
                string headerName = pair.Value;

                PropertyInfo? prop = modelDataType?
                    .GetProperty(modelName);

                if (prop == null)
                {
                    Debug.WriteLine($"(Data View Reflection): No property with name '{modelName}' on {modelDataType}, skipping...");
                    continue;
                }

                Type? propertyType = prop?
                    .PropertyType;

                DataGridBoundColumn column;

                Debug.WriteLine($"(Data View Reflection): Currently processing on {modelDataType}: {modelName} ({headerName}): {propertyType?.ToString()}");

                // Special case for bool types
                if (propertyType == typeof(bool))
                {
                    column = new DataGridCheckBoxColumn()
                    {
                        Header = headerName,
                        Binding = new Binding($"Value.{modelName}")
                    };
                }
                else
                {
                    // Otherwise, use standard text columns
                    column = new DataGridTextColumn()
                    {
                        Header = headerName,
                        Binding = new Binding($"Value.{modelName}")
                    };
                }

                DataGrid.Columns.Add(column);
            }

            DataGridTemplateColumn editColumn = new();
            editColumn.CellTemplate = (DataTemplate)this.FindResource("DataGrid_EditButton");

            DataGrid.Columns.Add(editColumn);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void DidUpdateProperty(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(property));
            }
        }

        private void OnObservableDataSource_Changed(object sender, NotifyCollectionChangedEventArgs? e)
        {
            ForceUpdateDataGridColumns();
        }

        public static readonly DependencyProperty DataSourceProperty =
            DependencyProperty.Register("DataSource", typeof(IEnumerable), typeof(PMSDataView), new PropertyMetadata(null, OnDataSourceChanged));

        public IEnumerable DataSource
        {
            get { return (IEnumerable)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }

        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register("Columns", typeof(Dictionary<string, string>), typeof(PMSDataView), new PropertyMetadata(null, OnDataSourceChanged));

        public Dictionary<string, string> Columns
        {
            get { return (Dictionary<string, string>)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        private void UpdateSelectedCells()
        {
            foreach (var item in ObservableDataSource)
            {
                item.IsSelected = DataGrid.SelectedItems.Contains(item);
            }

            AllItemsSelectedCheckbox.IsChecked = DataGrid.SelectedItems.Count >= DataGrid.Items.Count;

            DidUpdateProperty("HasSelectedItems");
        }

        private void OnDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            UpdateSelectedCells();
        }

        private void OnSelectAllCheckBox_Click(object sender, RoutedEventArgs e)
        {
            int selectedCells = DataGrid.SelectedCells.Count;
            int itemsCount = DataGrid.Items.Count;

            if (selectedCells >= itemsCount)
            {
                DataGrid.UnselectAll();
            }
            else
            {
                foreach (var cell in DataGrid.Items)
                {
                    DataGrid.SelectedItems.Add(cell);
                }
                DataGrid.SelectAll();
            }
        }

        private void OnDataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            UpdateSelectedCells();
        }

        public void ApplyDataGridFilter(string filter)
        {
            ICollectionView cvData = CollectionViewSource.GetDefaultView(
                DataGrid.ItemsSource
            );

            if (cvData == null) return;

            cvData.Filter = new Predicate<object>(d =>
            {
                if (d is DataItem dataItem)
                {
                    return SearchController.SimpleSearchPredicate(filter)(dataItem.Value);
                }

                return false;
            });
        }

        public bool HasSelectedItems
        {
            get => DataGrid.SelectedItems.Count > 0;
        }
    }
}
