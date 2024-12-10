using PMS.Controllers;
using PMS.Models;
using System;
using System.Collections.Generic;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PMS.Components
{
    /// <summary>
    /// Interaction logic for PMSContextualForm.xaml
    /// </summary>
    public partial class PMSContextualForm : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty SchemaProperty = DependencyProperty.Register("Schema", typeof(FormItemBase[]), typeof(PMSContextualForm), new PropertyMetadata(null, OnDataSourceChanged));

        public FormItemBase[] Schema
        {
            get { return (FormItemBase[])GetValue(SchemaProperty); }
            set { 
                SetValue(SchemaProperty, value);
                DidUpdateProperty("Schema");
                DidUpdateProperty("IsNewEntry");
            }
        }

        public static readonly DependencyProperty DataSourceProperty = DependencyProperty.Register("DataSource", typeof(DataItem), typeof(PMSContextualForm), new PropertyMetadata(null, OnDataSourceChanged));

        public DataItem? DataSource
        {
            get { return (DataItem?)GetValue(DataSourceProperty); }
            set { 
                SetValue(DataSourceProperty, value);
                DidUpdateProperty("DataSource");
                DidUpdateProperty("IsNewEntry");
            }
        }

        public static readonly DependencyProperty DataSourceIdxProperty = DependencyProperty.Register("DataSourceIdx", typeof(int), typeof(PMSContextualForm), new PropertyMetadata(0, OnDataSourceChanged));

        public int DataSourceIdx
        {
            get { return (int)GetValue(DataSourceIdxProperty); }
            set
            {
                SetValue(DataSourceIdxProperty, value);
                DidUpdateProperty("DataSource");
                DidUpdateProperty("IsNewEntry");
            }
        }

        public static readonly DependencyProperty DataSourceCountProperty = DependencyProperty.Register("DataSourceCount", typeof(int), typeof(PMSContextualForm), new PropertyMetadata(0, OnDataSourceChanged));

        public int DataSourceCount
        {
            get { return (int)GetValue(DataSourceCountProperty); }
            set
            {
                SetValue(DataSourceCountProperty, value);
                DidUpdateProperty("DataSource");
                DidUpdateProperty("IsNewEntry");
            }
        }

        public static readonly DependencyProperty CanEditProperty = DependencyProperty.Register("CanEdit", typeof(bool), typeof(PMSContextualForm), new PropertyMetadata(true, OnDataSourceChanged));

        public bool CanEdit
        {
            get { return (bool)GetValue(CanEditProperty); }
            set { SetValue(CanEditProperty, value); }
        }

        public static readonly DependencyProperty CanCreateProperty = DependencyProperty.Register("CanCreate", typeof(bool), typeof(PMSContextualForm), new PropertyMetadata(true, OnDataSourceChanged));

        public bool CanCreate
        {
            get { return (bool)GetValue(CanCreateProperty); }
            set { SetValue(CanCreateProperty, value); }
        }

        public bool IsNewEntry
        {
            get => DataSource == null;
        }
        public Dictionary<FormItemBase, FrameworkElement>? RegisteredFields { get; set; }
        public Dictionary<FormItemBase, string>? FieldErrors { get; set; }

        private bool _CanGoBack;
        public bool CanGoBack
        {
            get => _CanGoBack;
            set
            {
                _CanGoBack = value;
                DidUpdateProperty("CanGoBack");
            }
        }
        private bool _CanGoForward;
        public bool CanGoForward
        {
            get => _CanGoForward;
            set
            {
                _CanGoForward = value;
                DidUpdateProperty("CanGoForward");
            }
        }

        public bool SubmitForm()
        {
            if (CanSubmitForm())
            {
                return true;
            } else
            {
                string errorMessage = string.Join(
                    "\n",
                    this.FieldErrors?
                        .Where(kv => !kv.Key.Label.Contains(':')) // exclude relationship fieldss
                        .Select(kv => $"{kv.Key.Label}: {kv.Value}") ?? []
                );

                MessageBoxController.Show(
                    Parent,
                    $"Cannot save record as one or more fields have errors:\n\n{errorMessage}", 
                    "Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error
                );

                return false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void DidUpdateProperty(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(property));
            }
        }

        private static void OnDataSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is PMSContextualForm cf)
            {
                cf.UpdateDataSource();
            }
        }

        private void UpdateDataSource()
        {
            string modelName = DataSource?.Value.GetType().Name ?? "Entry";

            FormNumber.Content = $"Record {DataSourceIdx + 1} of {Math.Max(DataSourceCount, 1)}";

            CanGoBack = DataSourceIdx > 0;
            CanGoForward = DataSourceIdx < (Math.Max(DataSourceCount, 1) - 1);

            FormStateLock.Visibility = Visibility.Collapsed;
            FormStateAdd.Visibility = Visibility.Collapsed;
            FormStateEdit.Visibility = Visibility.Collapsed;

            if (IsNewEntry && CanCreate)
            {
                FormStateText.Content = $"New {modelName}";
                FormStateAdd.Visibility = Visibility.Visible;
            }
            else if (!IsNewEntry && CanEdit)
            {
                FormStateText.Content = $"Editing existing {modelName}";
                FormStateEdit.Visibility = Visibility.Visible;
            }
            else
            {
                FormStateText.Content = $"{modelName} is read-only";
                FormStateLock.Visibility = Visibility.Visible;
            }

            this.RegisteredFields = null;
            RenderedForm.Children.Clear();

            if (Schema != null)
            {
                foreach (var item in Schema)
                {
                    try
                    {
                        FrameworkElement rendered = item.Render(this, DataSource);

                        RenderedForm.Children.Add(rendered);
                    }
                    catch (Exception ex)
                    {
                        MessageBoxController.Show(
                            Parent,
                            $"Failed to render form field '{item.Label ?? nameof(item)}'.\n\nError: {ex.ToString()}",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );
                        continue;
                    }
                }
            } else
            {
                RenderedForm.Children.Add(new Label { Content = $"No form schema for shape '{modelName}'." });
            }
        }

        public void RegisterField(FormItemBase formItem, FrameworkElement widget)
        {
            if (RegisteredFields == null)
            {
                RegisteredFields = [];
            }

            RegisteredFields.Add(formItem, widget);
            LogController.WriteLine($"Bound {formItem} ({formItem.Label}) to {widget}.", LogCategory.ContextualForm);
        }

        public bool CanSubmitForm()
        {
            this.FieldErrors = [];

            if (RegisteredFields == null)
            {
                return false;
            }

            foreach (FormItemBase formItem in RegisteredFields.Keys)
            {
                string? error = formItem.IsValid(this);

                if (error != null)
                {
                    this.FieldErrors.Add(formItem, error);
                    LogController.WriteLine($"{formItem.Label}: {error}", LogCategory.ContextualForm);
                    continue;
                }
            }

            return this.FieldErrors.Count <= 0;
        }

        public object? GetFieldValue(string fieldLabel)
        {
            if (this.RegisteredFields?.Keys.FirstOrDefault(f => f.Label == fieldLabel) is FormItemBase formItem)
            {
                return formItem.GetValue();
            }

            return null;
        }
        public void SetFieldValue(string fieldLabel, object? value, bool? focusField = false)
        {
            if (this.RegisteredFields?.Keys.FirstOrDefault(f => f.Label == fieldLabel) is FormItemBase formItem)
            {
                formItem.SetValue(value);

                if (focusField == true)
                {
                    formItem.RenderedWidget?.Focus();
                }
            }
        }

        public PMSContextualForm()
        {
            InitializeComponent();

            DataContext = this;

            UpdateDataSource();
        }

        public static readonly RoutedEvent RequestPreviousEvent = EventManager.RegisterRoutedEvent("RequestPrevious", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PMSContextualForm));

        public event RoutedEventHandler RequestPrevious
        {
            add { AddHandler(RequestPreviousEvent, value); }
            remove { RemoveHandler(RequestPreviousEvent, value); }
        }

        public static readonly RoutedEvent RequestNextEvent = EventManager.RegisterRoutedEvent("RequestNext", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PMSContextualForm));

        public event RoutedEventHandler RequestNext
        {
            add { AddHandler(RequestNextEvent, value); }
            remove { RemoveHandler(RequestNextEvent, value); }
        }

        private void OnPreviousRecord_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(RequestPreviousEvent));
        }
        private void OnNextRecord_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(RequestNextEvent));

        }
    }
}
