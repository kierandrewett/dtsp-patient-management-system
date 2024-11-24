using PMS.Controllers;
using PMS.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

    public enum DataManagerPanel
    {
        Default,
        Edit
    }

    /// <summary>
    /// Interaction logic for PMSDataManager.xaml
    /// </summary>
    public partial class PMSDataManager : UserControl, INotifyPropertyChanged
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

        public static readonly DependencyProperty DataSourceProperty = DependencyProperty.Register("DataSource", typeof(IEnumerable), typeof(PMSDataManager), new PropertyMetadata(null, OnDataSourceChanged));

        public IEnumerable DataSource
        {
            get { return (IEnumerable)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }

        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register("Columns", typeof(Dictionary<string, string>), typeof(PMSDataManager), new PropertyMetadata(null, OnDataSourceChanged));

        public event PropertyChangedEventHandler? PropertyChanged;

        public Dictionary<string, string> Columns
        {
            get { return (Dictionary<string, string>)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        private DataManagerPanel _SelectedPanel;
        public DataManagerPanel SelectedPanel
        {
            get => _SelectedPanel;
            set
            {
                SwapManagerPanel(_SelectedPanel, value);
            }
        }

        public PMSDataManager()
        {
            InitializeComponent();

            DataContext = this;

            ExitEditMode();
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
            if (obj is PMSDataManager dv)
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
            )
            {
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

        private void OnSearchBox_Updated(object sender, RoutedEventArgs e)
        {
            string filter = PanelHeader.SearchBoxValue;

            ApplyDataGridFilter(filter);
        }

        private void OnBackButton_Click(object sender, RoutedEventArgs e)
        {
            ExitEditMode();
        }

        private void UpdateSelectedCells()
        {
            foreach (var item in ObservableDataSource)
            {
                item.IsSelected = DataGrid.SelectedItems.Contains(item);
            }

            AllItemsSelectedCheckbox.IsChecked = DataGrid.SelectedItems.Count <= 0
                ? false
                : DataGrid.SelectedItems.Count >= DataGrid.Items.Count
                    ? true
                    : null;

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
                    return SearchController.SimpleSearchPredicate(filter, Columns.Keys.ToArray())(dataItem.Value);
                }

                return false;
            });
        }

        public bool HasSelectedItems
        {
            get => DataGrid.SelectedItems.Count > 0;
        }

        public bool HasSearch
        {
            get => SelectedPanel == DataManagerPanel.Default;
        }

        public bool HasSave
        {
            get => SelectedPanel == DataManagerPanel.Edit;
        }

        public bool CanGoBack
        {
            get => SelectedPanel != DataManagerPanel.Default;
        }

        private FrameworkElement? GetPanelElement(DataManagerPanel panel)
        {
            return (FrameworkElement?)this.FindName($"Panel{panel.ToString()}");
        }

        private async void SwapManagerPanel(DataManagerPanel? oldPanel, DataManagerPanel newPanel)
        {
            FrameworkElement? oldPanelEl = oldPanel != null 
                ? GetPanelElement((DataManagerPanel)oldPanel) 
                : null;
            FrameworkElement? newPanelEl = GetPanelElement(newPanel);

            if (newPanelEl != null)
            {
                Mouse.OverrideCursor = Cursors.Wait;

                if (oldPanelEl != null)
                {
                    oldPanelEl.Visibility = Visibility.Collapsed;
                }

                Random rnd = new Random();
                await Task.Delay(rnd.Next(100, 400));

                newPanelEl.Visibility = Visibility.Visible;

                _SelectedPanel = newPanel;

                Mouse.OverrideCursor = null;

                DidUpdateProperty("SelectedPanel");

                // Depends on the value of SelectedPanel
                DidUpdateProperty("HasSearch");
                DidUpdateProperty("HasSave");
                DidUpdateProperty("CanGoBack");

            }
        }

        public static readonly RoutedEvent EntryRequestEditEvent = EventManager.RegisterRoutedEvent("EntryRequestEdit", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PMSDataManager));

        public event RoutedEventHandler EntryRequestEdit
        {
            add { AddHandler(EntryRequestEditEvent, value); }
            remove { RemoveHandler(EntryRequestEditEvent, value); }
        }

        public void OnDataItem_RequestEdit(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                DataItem dataItem = (DataItem)button.DataContext;

                button.RaiseEvent(new RoutedEventArgs(EntryRequestEditEvent));
            }
        }

        private void EnterEditMode()
        {
            SelectedPanel = DataManagerPanel.Edit;

            UpdateUnsavedChangesState(true);
        }

        private void ExitEditMode()
        {
            if (SelectedPanel == DataManagerPanel.Edit)
            {
                if (!ChangesProtectionController.UnsavedChangesGuard())
                {
                    return;
                }
            }

            SelectedPanel = DataManagerPanel.Default;
            UpdateUnsavedChangesState(false);
        }

        private void UpdateUnsavedChangesState(bool newValue)
        {
            Window parentWindow = Window.GetWindow(this);
            Debug.WriteLine(parentWindow);
            if (parentWindow is MainWindow mainWindow)
            {

                mainWindow.UnsavedChangesLock = newValue;
            }
        }

        public void OnRequestEditSelected_Click(object sender, RoutedEventArgs e)
        {
            int numSelected = DataGrid.SelectedItems.Count;
            if (numSelected > 3)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Over 3 entries have been selected for batch editing, do you wish to continue?",
                    "Batch Edit",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            Debug.WriteLine("Editing " + numSelected);

            EnterEditMode();
        }

        public void OnSaveButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Saved");
        }
    }
}
