using Microsoft.IdentityModel.Tokens;
using PMS.Controllers;
using PMS.Models;
using PMS.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

        public Dictionary<string, string> Columns
        {
            get { return (Dictionary<string, string>)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }


        public static readonly DependencyProperty ColumnSortProperty = DependencyProperty.Register("ColumnSort", typeof(SortDescription?), typeof(PMSDataManager), new PropertyMetadata(null, OnDataSourceChanged));

        public SortDescription? ColumnSort
        {
            get { return (SortDescription?)GetValue(ColumnSortProperty); }
            set { SetValue(ColumnSortProperty, value); }
        }
        public static readonly DependencyProperty CanEditProperty = DependencyProperty.Register("CanEdit", typeof(bool), typeof(PMSDataManager), new PropertyMetadata(true, OnDataSourceChanged));

        public bool CanEdit
        {
            get { return (bool)GetValue(CanEditProperty); }
            set { 
                SetValue(CanEditProperty, value);
                DidUpdateProperty("CanEdit");
            }
        }

        public static readonly DependencyProperty CanCreateProperty = DependencyProperty.Register("CanCreate", typeof(bool), typeof(PMSDataManager), new PropertyMetadata(true, OnDataSourceChanged));

        public bool CanCreate
        {
            get { return (bool)GetValue(CanCreateProperty); }
            set
            {
                SetValue(CanCreateProperty, value);
                DidUpdateProperty("CanCreate");
            }
        }

        public static readonly DependencyProperty FormProperty = DependencyProperty.Register("Form", typeof(FormItemBase[]), typeof(PMSDataManager));

        public FormItemBase[] Form
        {
            get { return (FormItemBase[])GetValue(FormProperty); }
            set { SetValue(FormProperty, value); }
        }

        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register("Model", typeof(Type), typeof(PMSDataManager));

        public Type Model
        {
            get { return (Type)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;


        public static readonly RoutedEvent ManagerReadyEvent = EventManager.RegisterRoutedEvent("ManagerReady", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PMSDataManager));

        public event RoutedEventHandler ManagerReady
        {
            add { AddHandler(ManagerReadyEvent, value); }
            remove { RemoveHandler(ManagerReadyEvent, value); }
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
                DidUpdateProperty("HasSave");
                DidUpdateProperty("CanView");

                if (ColumnSort is SortDescription sortDesc)
                {
                    var view = CollectionViewSource.GetDefaultView(DataGrid.InnerDataGrid.ItemsSource);
                    if (view != null)
                    {
                        view.SortDescriptions.Clear(); 
                        view.SortDescriptions.Add(new SortDescription($"Value.{sortDesc.PropertyName}", sortDesc.Direction));
                        view.Refresh();
                    }
                }
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

            DataGrid.InnerDataGrid.Columns.Clear();

            // Add selected column
            DataGridCheckBoxColumn isSelectedColumn = new()
            {
                Binding = new Binding("IsSelected")
            };

            DataGrid.InnerDataGrid.Columns.Add(isSelectedColumn);

            Type? modelDataType = ObservableDataSource?.FirstOrDefault()?.Value.GetType();

            foreach (KeyValuePair<string, string> pair in Columns)
            {
                string modelName = pair.Key;
                string headerName = pair.Value.Replace("_", "");

                bool isColumnHidden = pair.Value.ToCharArray()[0] == '_';

                PropertyInfo? prop = modelDataType?
                    .GetProperty(modelName);

                if (prop == null)
                {
                    LogController.WriteLine($"No property with name '{modelName}' on {modelDataType}, skipping...", LogCategory.DataViewReflection);
                    continue;
                }

                Type? propertyType = prop?
                    .PropertyType;

                DataGridBoundColumn column;

                LogController.WriteLine($"(Data View Reflection): Currently processing on {modelDataType}: {modelName} ({headerName}): {propertyType?.ToString()}", LogCategory.DataViewReflection);

                // Special case for bool types
                if (propertyType == typeof(bool))
                {
                    column = new DataGridCheckBoxColumn()
                    {
                        Header = headerName,
                        Binding = new Binding($"Value.{modelName}"),
                        SortMemberPath = $"Value.{modelName}"
                    };
                }
                else
                {
                    // Otherwise, use standard text columns
                    column = new DataGridTextColumn()
                    {
                        Header = headerName,
                        Binding = new Binding($"Value.{modelName}"),
                        SortMemberPath = $"Value.{modelName}"
                    };
                }

                column.Visibility = isColumnHidden 
                    ? Visibility.Collapsed 
                    : Visibility.Visible;

                DataGrid.InnerDataGrid.Columns.Add(column);
            }

            DataGridTemplateColumn editColumn = new();
            editColumn.CellTemplate = (DataTemplate)this.FindResource(CanView ? "DataGrid_ViewButton" : "DataGrid_EditButton");

            DataGrid.InnerDataGrid.Columns.Add(editColumn);
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
                item.IsSelected = DataGrid.InnerDataGrid.SelectedItems.Contains(item);
            }

            AllItemsSelectedCheckbox.IsChecked = DataGrid.InnerDataGrid.SelectedItems.Count <= 0
                ? false
                : DataGrid.InnerDataGrid.SelectedItems.Count >= DataGrid.InnerDataGrid.Items.Count
                    ? true
                    : null;

            DidUpdateProperty("HasSelectedItems");
        }

        private void OnDataGrid_SelectedCellsChanged(object sender, RoutedEventArgs e)
        {
            UpdateSelectedCells();
        }

        private void OnSelectAllCheckBox_Click(object sender, RoutedEventArgs e)
        {
            int selectedCells = DataGrid.InnerDataGrid.SelectedCells.Count;
            int itemsCount = DataGrid.InnerDataGrid.Items.Count;

            if (selectedCells >= itemsCount)
            {
                DataGrid.InnerDataGrid.UnselectAll();
            }
            else
            {
                foreach (var cell in DataGrid.InnerDataGrid.Items)
                {
                    DataGrid.InnerDataGrid.SelectedItems.Add(cell);
                }
                DataGrid.InnerDataGrid.SelectAll();
            }
        }

        private void OnDataGrid_CurrentCellChanged(object sender, RoutedEventArgs e)
        {
            UpdateSelectedCells();
        }

        public void ApplyDataGridFilter(string filter)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            ICollectionView cvData = CollectionViewSource.GetDefaultView(
                DataGrid.InnerDataGrid.ItemsSource
            );

            if (cvData == null) return;

            List<object> items = [];

            foreach (DataItem item in DataGrid.InnerDataGrid.ItemsSource)
            {
                items.Add(item.Value);
            }

            object[] rankedItems = SearchController.SimpleRankedSearch<object>(
                filter,
                items,
                [..(Columns ?? []).Keys]
            );

            cvData.Filter = new Predicate<object>(d =>
            {
                if (d is DataItem dataItem)
                {
                    return rankedItems.Contains(dataItem.Value);
                }

                return false;
            });

            Mouse.OverrideCursor = null;
        }

        public bool HasSelectedItems
        {
            get => DataGrid.InnerDataGrid.SelectedItems.Count > 0;
        }

        public bool HasSearch
        {
            get => SelectedPanel == DataManagerPanel.Default;
        }

        public bool HasSave
        {
            get => SelectedPanel == DataManagerPanel.Edit && (EditingDataItem == null ? CanCreate : CanEdit);
        }

        public bool CanGoBack
        {
            get => SelectedPanel != DataManagerPanel.Default;
        }

        public bool CanView
        {
            get => !CanEdit;
        }

        private DataItem[]? _EditingDataItems;
        public DataItem[]? EditingDataItems
        {
            get => _EditingDataItems;
            set
            {
                _EditingDataItems = value;
                DidUpdateProperty("EditingDataItems");
            }
        }

        public DataItem? EditingDataItem
        {
            get => EditingDataItems?[EditingDataItemIndex ?? 0] ?? null;
        }

        private int? _EditingDataItemIndex;
        public int? EditingDataItemIndex
        {
            get => _EditingDataItemIndex;
            set
            {
                _EditingDataItemIndex = value;
                DidUpdateProperty("EditingDataItemIndex");
            }
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
                newPanelEl.Focus();

                _SelectedPanel = newPanel;

                Mouse.OverrideCursor = null;

                DidUpdateProperty("SelectedPanel");

                // Depends on the value of SelectedPanel
                DidUpdateProperty("HasSearch");
                DidUpdateProperty("HasSave");
                DidUpdateProperty("CanEdit");
                DidUpdateProperty("CanView");
                DidUpdateProperty("CanGoBack");

            }
        }

        public static readonly RoutedEvent EntryRequestEditEvent = EventManager.RegisterRoutedEvent("EntryRequestEdit", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PMSDataManager));

        public event RoutedEventHandler EntryRequestEdit
        {
            add { AddHandler(EntryRequestEditEvent, value); }
            remove { RemoveHandler(EntryRequestEditEvent, value); }
        }

        public bool UnsavedChangesLock
        {
            get
            {
                Window parentWindow = Window.GetWindow(this);

                return ((PMSLockableWindow)parentWindow)?.ChangesProtectionController.UnsavedChangesLock ?? false;
            }
            set
            {
                Window parentWindow = Window.GetWindow(this);

                if (parentWindow is PMSLockableWindow win)
                {
                    win.ChangesProtectionController.UnsavedChangesLock = value;
                }
            }
        }

        public void OnDataItem_RequestEdit(object sender, RoutedEventArgs e)
        {
            DataItem[]? dataItems = null;

            try
            {
                if (sender is Button)
                {
                    dataItems = [(DataItem)((FrameworkElement)sender).DataContext];
                }
                else if (sender is PMSDataGrid)
                {
                    dataItems = ((PMSDataGrid)sender).InnerDataGrid.SelectedItems
                        .Cast<DataItem>()
                        .ToArray();
                }
            } catch (Exception _)
            {
                // dirty workaround for casting errors
            }

            if (!dataItems.IsNullOrEmpty())
            {
                EnterEditMode(dataItems);
            }
        }
        public void EnterEditMode(DataItem[]? dataItems)
        {
            SelectedPanel = DataManagerPanel.Edit;
            EditingDataItems = dataItems?.Length > 0 ? dataItems : null;
            EditingDataItemIndex = 0;
            DidUpdateProperty("EditingDataItem");

            UnsavedChangesLock = EditingDataItem == null ? CanCreate : CanEdit;

            if (Window.GetWindow(this) is PMSWindow window && DataSource != null)
            {
                string modelName = EditingDataItem?.Value.GetType().Name ?? "Entry";

                window.AccessibilityController.CancelAll();
                window.AccessibilityController.MaybeAnnounceArbritary(
                    EditingDataItem == null
                        ? $"New {modelName}"
                        : CanEdit
                            ? $"Editing existing {modelName}"
                            : $"{modelName} is read-only",
                    true
                );
            }
        }

        public void ExitEditMode()
        {
            if (SelectedPanel == DataManagerPanel.Edit)
            {
                if (
                    UnsavedChangesLock == true && 
                    !ChangesProtectionController.UnsavedChangesGuard(this)
                ) {
                    return;
                }
            }

            SelectedPanel = DataManagerPanel.Default;
            EditingDataItems = null;
            EditingDataItemIndex = null;

            UnsavedChangesLock = false;

            RefreshDataManagerOuterContent();
        }

        private void DeleteRecords(DataItem[] dataItems)
        {
            MessageBoxResult result = MessageBoxController.Show(
                                        Parent,
                                        dataItems.Length > 1
                                            ? $"Are you sure you want to permanently delete {dataItems.Length} items?"
                                            : "Are you sure you want to permanently delete this record?",
                                        "Confirm deletion",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question,
                                        MessageBoxResult.No
                                    );
            
            if (result != MessageBoxResult.Yes)
            {
                return;
            }
            
            foreach (DataItem dataItem in dataItems)
            {
                if (dataItem.Value is BaseModel model)
                {
                    AppDatabase.WriteModelDeletion(model);
                }
            }

            RefreshDataManagerOuterContent();
        }

        public void RefreshDataManagerOuterContent()
        {
            Window parent = Window.GetWindow(this);

            if (parent is MainWindow mainWindow)
            {
                mainWindow.TabsController.ReloadSelectedTab();
            }
            else if (parent is PMSLockableWindow win)
            {
                try
                {
                    win.InvalidateDataContext();
                } catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }

            RaiseEvent(new RoutedEventArgs(ManagerReadyEvent));
        }

        public void OnRequestDelete_Click(object sender, RoutedEventArgs e)
        {

            DeleteRecords(DataGrid.InnerDataGrid.SelectedItems.Cast<DataItem>().ToArray());
        }

        public void OnRequestEditSelected_Click(object sender, RoutedEventArgs e)
        {
            int numSelected = DataGrid.InnerDataGrid.SelectedItems.Count;
            if (numSelected > 3 && CanEdit)
            {
                MessageBoxResult result = MessageBoxController.Show(
                    Parent,
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

            EnterEditMode(DataGrid.InnerDataGrid.SelectedItems.Cast<DataItem>().ToArray());
        }

        private void SaveRecordToDatabase()
        {
            if (DataForm?.RegisteredFields != null)
            {
                bool createNewRecord = EditingDataItem == null;

                BaseModel? modelInstance = (BaseModel?)Activator.CreateInstance(Model);

                if (modelInstance != null || modelInstance is BaseModel)
                {

                    foreach (var field in DataForm.RegisteredFields.Keys)
                    {
                        PropertyInfo? property = null;
                        object currentInstance = modelInstance;

                        if (!field.DataBinding.IsNullOrEmpty())
                        {
                            // Split the data binding
                            // Sometimes we have cases like ComputedAddress.Postcode
                            // Reflection is unable to handle the resolution of such
                            // keys, therefore we must split it and handle the creation
                            // of each substring (as ComputedAddress may not exist).
                            string[] split = field.DataBinding.Split(".");

                            for (int i = 0; i < split.Length; i++)
                            {
                                string modelName = currentInstance.GetType().Name;

                                string key = split[i];
                                bool isLastKey = (i + 1) >= split.Length;

                                property = currentInstance.GetType().GetProperty(key);

                                if (property == null || !property.CanWrite)
                                {
                                    LogController.WriteLine($"Property '{key}' not found or not-writable on model '{modelName}', skipping...", LogCategory.DataManager);
                                    continue;
                                }

                                if (isLastKey)
                                {
                                    object? propertyValue = field.GetValue();
                                    object? serialisedPropertyValue = field.SerialiseWith != null 
                                        ? field.SerialiseWith(propertyValue) 
                                        : propertyValue;

                                    LogController.WriteLine($"Setting property on model '{modelName}': {property.Name}: {property.PropertyType} = {serialisedPropertyValue}", LogCategory.DataManager);

                                    try
                                    {
                                        property.SetValue(
                                            currentInstance,
                                            Convert.ChangeType(serialisedPropertyValue, property.PropertyType)
                                        );
                                    } catch (Exception e)
                                    {
                                        LogController.WriteLine($"Unable to cast property to {property.PropertyType} on model '{modelName}': {property.Name}: {property.PropertyType} = {serialisedPropertyValue}\n{e.ToString()}", LogCategory.DataManager);
                                    }
                                }
                                else
                                {

                                    // Handle cases where we need to create sub-instances
                                    object? nestedInstance = property.GetValue(currentInstance);

                                    if (nestedInstance == null)
                                    {
                                        LogController.WriteLine($"Setting property on model '{modelName}': {property.Name}: {property.PropertyType} = [new instance]", LogCategory.DataManager);
                                        nestedInstance = Activator.CreateInstance(property.PropertyType);
                                        property.SetValue(
                                            currentInstance,
                                            nestedInstance
                                        );
                                    }

                                    currentInstance = nestedInstance;
                                }
                            }
                        }
                    }

                    int? rowsAffected = AppDatabase.WriteModelUpdate(modelInstance);

                    if (rowsAffected == null || rowsAffected < 0)
                    {
                        throw AppDatabase.CurrentError ?? new Exception("No rows were affected.");
                    }

                    return;
                } else
                {
                    LogController.WriteLine($"Model is not a valid BaseModel type.", LogCategory.DataManager);
                }
            }
        }

        private bool SafeSaveRecordToDatabase()
        {
            try
            {
                SaveRecordToDatabase();
            }
            catch (Exception ex)
            {
                MessageBoxController.Show(
                    Parent,
                    $"Failed to save record:\n\n{ex.Message.ToString()}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                return false;
            }

            return true;
        }

        public void OnRequestNew_Click(object sender, RoutedEventArgs e)
        {
            EnterEditMode(null);
        }

        public async void OnSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is PMSContentHeader header)
            {
                Mouse.OverrideCursor = Cursors.Wait;

                header.SaveButton.IsEnabled = false;

                Random rnd = new Random();
                await Task.Delay(rnd.Next(100, 400));

                if (DataForm.SubmitForm() && SafeSaveRecordToDatabase())
                {
                    if (EditingDataItem == null || EditingDataItemIndex == (EditingDataItems.Length - 1))
                    {
                        MessageBoxController.Show(
                            Parent,
                            $"Successfully saved {EditingDataItems?.Length ?? 1} records.",
                            "Saved records",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information
                        );

                        UnsavedChangesLock = false;
                        ExitEditMode();
                    }
                    else
                    {
                        // Indicate that the record has been
                        // saved without prompting the user
                        SystemSounds.Asterisk.Play();
                    }

                    Mouse.OverrideCursor = null;

                    header.SaveButton.Content = "Saved";

                    await Task.Delay(1000);

                    header.SaveButton.Content = "Save changes";
                    header.SaveButton.IsEnabled = true;
                }

                Mouse.OverrideCursor = null;
                header.SaveButton.IsEnabled = true;
            }
        }

        public async void ChangeDataItemIndex(int newIdx)
        {
            if (EditingDataItem == null)
            {
                return;
            }

            if (newIdx < 0 || newIdx > EditingDataItems.Length)
            {
                return;
            }

            Mouse.OverrideCursor = Cursors.Wait;

            Random rnd = new Random();
            await Task.Delay(rnd.Next(100, 400));

            EditingDataItemIndex = newIdx;
            DidUpdateProperty("EditingDataItem");

            // Indicate that the record has been
            // saved without prompting the user
            SystemSounds.Asterisk.Play();

            Mouse.OverrideCursor = null;
        }


        public async void OnForm_RequestPreviousRecord(object sender, RoutedEventArgs e)
        {
            if (EditingDataItem == null)
            {
                return;
            }

            ChangeDataItemIndex((EditingDataItemIndex ?? 0) - 1);
        }

        public void OnForm_RequestNextRecord(object sender, RoutedEventArgs e)
        {
            if (EditingDataItem == null)
            {
                return;
            }

            ChangeDataItemIndex((EditingDataItemIndex ?? 0) + 1);
        }

        public void DoCreate()
        {
            CreateButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }
    }
}
