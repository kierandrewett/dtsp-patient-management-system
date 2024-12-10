using PMS.Components;
using PMS.Controllers;
using PMS.Models;
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

namespace PMS.Context
{
    public class DataContext<T>
    {
        public Type Model { get; set; }
        public T[] DataSource { get; set; }
        public Dictionary<string, string> Columns { get; set; }
        public Dictionary<string, string> CompactColumns { get; set; }
        public SortDescription? ColumnSort { get; set; }

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

        public virtual void Refresh()
        {
            throw new Exception("no impl");
        }

        public virtual string GetAccessibleName(T row)
        {
            throw new Exception("no impl");
        }

        public ObservableCollection<DataItem> CreateObservableDataSource()
        {
            ObservableCollection<DataItem> observableDataSource = new ObservableCollection<DataItem>();

            foreach (var item in DataSource)
            {
                if (item == null) continue;

                observableDataSource.Add(new DataItem
                {
                    Value = item,
                    IsSelected = false
                });
            }

            return observableDataSource;
        }

        public void RenderColumns(DataGrid dataGrid, ObservableCollection<DataItem>? observableDataSource, bool? hasSelectionCheckbox = false, string? bindingPrefix = "Value.")
        {
            // In the cases when any of these are true
            // don't bother updating datagrid columns.
            if (
                Columns == null ||
                observableDataSource == null ||
                observableDataSource?.Count <= 0
            ) return;

            dataGrid.Columns.Clear();

            Type? modelDataType = observableDataSource!.FirstOrDefault()?.Value.GetType();

            if (hasSelectionCheckbox == true)
            {
                // Add selected column
                DataGridCheckBoxColumn isSelectedColumn = new()
                {
                    Binding = new Binding("IsSelected")
                };

                dataGrid.Columns.Add(isSelectedColumn);
            }

            foreach (KeyValuePair<string, string> pair in CompactColumns ?? Columns)
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

                LogController.WriteLine($"Currently processing on {modelDataType}: {modelName} ({headerName}): {propertyType?.ToString()}", LogCategory.DataViewReflection);

                // Special case for bool types
                if (propertyType == typeof(bool))
                {
                    column = new DataGridCheckBoxColumn()
                    {
                        Header = headerName,
                        Binding = new Binding($"{bindingPrefix}{modelName}"),
                        SortMemberPath = $"{bindingPrefix}{modelName}"
                    };
                }
                else
                {
                    // Otherwise, use standard text columns
                    column = new DataGridTextColumn()
                    {
                        Header = headerName,
                        Binding = new Binding($"{bindingPrefix}{modelName}"),
                        SortMemberPath = $"{bindingPrefix}{modelName}"
                    };
                }

                column.Visibility = isColumnHidden
                    ? Visibility.Collapsed
                    : Visibility.Visible;

                dataGrid.Columns.Add(column);
            }
        }

        public void ApplySorting(DataGrid dataGrid, string? bindingPrefix = "Value.")
        {
            if (ColumnSort is SortDescription sortDesc)
            {
                var view = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);

                if (view != null)
                {
                    view.SortDescriptions.Clear();
                    view.SortDescriptions.Add(new SortDescription($"{bindingPrefix}{sortDesc.PropertyName}", sortDesc.Direction));
                    view.Refresh();
                }
            }
        }
    }
}
