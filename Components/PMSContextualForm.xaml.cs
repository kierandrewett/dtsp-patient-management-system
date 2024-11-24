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
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        public bool IsNewEntry
        {
            get => DataSource == null;
        }
        public Dictionary<FormItemBase, FrameworkElement>? RegisteredFields { get; set; }
        public Dictionary<FormItemBase, string>? FieldErrors { get; set; }


        public bool SubmitForm()
        {
            Debug.WriteLine("Attempting to submit form");

            if (CanSubmitForm())
            {
                return true;
            } else
            {
                string errorMessage = string.Join(
                    "\n",
                    this.FieldErrors?
                        .Select(kv => $"{kv.Key.Label}: {kv.Value}") ?? []
                );

                MessageBox.Show(
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

            FormState.Content = IsNewEntry
                ? $"(New {modelName})"
                : $"(Editing existing {modelName})";

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
                        MessageBox.Show(
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
            Debug.WriteLine($"(Contextual Form): Bound {formItem} to {widget}.");
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
                string? error = formItem.IsValid();

                if (error != null)
                {
                    this.FieldErrors.Add(formItem, error);
                    Debug.WriteLine($"(Contextual Form): {formItem.Label}: {error}");
                    continue;
                }
            }

            return this.FieldErrors.Count <= 0;
        }

        public PMSContextualForm()
        {
            InitializeComponent();

            DataContext = this;

            UpdateDataSource();
        }
    }
}
