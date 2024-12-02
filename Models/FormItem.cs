using Microsoft.IdentityModel.Tokens;
using PMS.Components;
using PMS.Context;
using PMS.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PMS.Models
{
    public enum FormItemGroupAlignmentMode
    {
        Stretch,
        FillContent
    }

    public class FormItemBase : PropertyObservable
    {
        private string _Label;
        public string Label {
            get => _Label;
            set
            {
                _Label = value;
                DidUpdateProperty("Label");
            }
        }

        private string? _DataBinding;
        public string? DataBinding
        {
            get => _DataBinding;
            set
            {
                _DataBinding = value;
                DidUpdateProperty("DataBinding");
            }
        }

        public Func<PMSContextualForm, bool> IsReadOnly;

        public Func<PMSContextualForm, object> DefaultValue;

        private bool _Required;
        public bool Required
        {
            get => _Required;
            set
            {
                _Required = value;
                DidUpdateProperty("Required");
            }
        }

        public Func<PMSContextualForm, bool> Hidden;

        public Func<object?, object?> SerialiseWith;

        private string _HelpLabel;
        public string HelpLabel
        {
            get => _HelpLabel;
            set
            {
                _HelpLabel = value;
                DidUpdateProperty("HelpLabel");
            }
        }

        private double _MaxWidth = double.PositiveInfinity;
        public double MaxWidth
        {
            get => _MaxWidth;
            set
            {
                _MaxWidth = value;
                DidUpdateProperty("MaxWidth");
            }
        }

        private FormItemGroupAlignmentMode _ItemGroupAlignmentMode = FormItemGroupAlignmentMode.Stretch;
        public virtual FormItemGroupAlignmentMode ItemGroupAlignmentMode
        {
            get => _ItemGroupAlignmentMode;
            set
            {
                _ItemGroupAlignmentMode = value;
                DidUpdateProperty("ItemGroupAlignmentMode");
            }
        }

        public FrameworkElement RenderedWidget { get; set; }

        public bool IsFormLocked = false;

        public void RegisterField(PMSContextualForm form, FrameworkElement widget)
        {
            form.RegisterField(this, widget);
            RenderedWidget = widget;
        }

        public virtual FrameworkElement Render(PMSContextualForm form, DataItem? dataItem)
        {
            throw new Exception("no impl");
        }

        public void SetFormLockedState(PMSContextualForm form)
        {
            IsFormLocked = form.IsNewEntry ? !form.CanCreate : !form.CanEdit;
        }

        public virtual string? IsValid(PMSContextualForm form)
        {
            if (RenderedWidget == null)
            {
                return "No rendered widget.";
            }

            return null;
        }

        public virtual object? GetValue()
        {
            return null;
        }

        public virtual void SetValue(object? value)
        {
            throw new Exception("no impl");
        }

        public TextBlock RenderLabel(PMSContextualForm form)
        {
            TextBlock textBlock = new();
            textBlock.VerticalAlignment = VerticalAlignment.Center;

            Label label = new()
            {
                Content = Label,
                FontWeight = FontWeights.Bold
            };

            textBlock.Inlines.Add(label);

            if (IsReadOnly != null && IsReadOnly(form))
            {
                textBlock.ToolTip = new ToolTip { Content = "Field is locked" };

                PMSIcon lockIcon = new()
                {
                    Source = (Geometry)Application.Current.Resources["LockIcon"],
                    Width = 10,
                    Height = 24,
                    Fill = new SolidColorBrush { Color = Color.FromRgb(150, 130, 0) }
                };

                textBlock.Inlines.Add(lockIcon);
            }
            else if (Required)
            {
                textBlock.ToolTip = new ToolTip { Content = "Field is required" };

                Label asteriskLabel = new Label()
                {
                    Content = "*",
                    Foreground = Brushes.Red,
                    FontWeight = FontWeights.Bold
                };

                textBlock.Inlines.Add(asteriskLabel);
            }

            return textBlock;
        }

        public StackPanel RenderFinal(PMSContextualForm form, FrameworkElement[] items, Orientation spOrientation = Orientation.Vertical)
        {
            StackPanel parentSp = new();
            parentSp.Orientation = Orientation.Vertical;
            parentSp.Margin = new Thickness(0, 4, 0, 4);

            StackPanel sp = new();
            sp.Orientation = spOrientation;
            sp.VerticalAlignment = VerticalAlignment.Center;

            foreach (var item in items)
            {
                sp.Children.Add(item);
            }

            bool isHidden = Hidden != null ? Hidden(form) == true : false;
            parentSp.Visibility = isHidden ? Visibility.Collapsed : Visibility.Visible;

            parentSp.Children.Add(sp);

            if (!HelpLabel.IsNullOrEmpty())
            {
                TextBlock helpLabel = new()
                {
                    Text = HelpLabel,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 4, 0, 0)
                };

                parentSp.Children.Add(helpLabel);
            }

            return parentSp;
        }
    }

    public class FormItemText : FormItemBase 
    {
        private Regex _Pattern;
        public Regex Pattern
        {
            get => _Pattern;
            set
            {
                _Pattern = value;
                DidUpdateProperty("Pattern");
            }
        }

        private bool _IsLong;
        public bool IsLong
        {
            get => _IsLong;
            set
            {
                _IsLong = value;
                DidUpdateProperty("IsLong");
            }
        }

        public Func<string, string> FormatValue;

        public override string? IsValid(PMSContextualForm form)
        {
            string? baseValid = base.IsValid(form);
            if (baseValid != null)
            {
                return baseValid;
            }

            if (Required && this.RenderedWidget is TextBox textBox)
            {
                if (textBox.Text.IsNullOrEmpty() || textBox.Text.Trim().Length < 0)
                {
                    return "Field cannot be empty.";
                }
            }

            if (IsFieldValid != null)
            {
                string? error = IsFieldValid(this, form);

                if (error != null)
                {
                    return error;
                }
            }

            return null;
        }

        public Func<FormItemBase, PMSContextualForm, string?>? IsFieldValid;

        public override string? GetValue()
        {
            return ((TextBox)RenderedWidget).Text;
        }

        public override void SetValue(object? value)
        {
            ((TextBox)RenderedWidget).Text = value?.ToString() ?? "";
        }

        public Func<PMSContextualForm, bool> OnKey;

        public override FrameworkElement Render(PMSContextualForm form, DataItem? dataItem)
        {
            TextBlock label = RenderLabel(form);

            TextBox widget = new();
            widget.DataContext = dataItem?.Value;

            SetFormLockedState(form);

            // Begin binding
            widget.IsReadOnly = IsReadOnly != null && IsReadOnly(form);
            widget.IsEnabled = !widget.IsReadOnly;
            widget.MaxWidth = MaxWidth;
            widget.MinHeight = 22;
            widget.Padding = new Thickness(2, 2, 2, 2);

            if (IsLong)
            {
                widget.TextWrapping = TextWrapping.Wrap;
                widget.AcceptsReturn = true;
                widget.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                widget.MinHeight = 105;
                widget.SpellCheck.IsEnabled = true;
            }

            Binding textBinding = new Binding($"{DataBinding ?? "None"}")
            {
                FallbackValue = DefaultValue != null ? DefaultValue(form) : "",
                Mode = BindingMode.OneTime
            };

            widget.SetBinding(TextBox.TextProperty, textBinding);

            widget.PreviewTextInput += Widget_PreviewTextInput;
            widget.LostFocus += Widget_LostFocus;

            if (OnKey != null)
            {
                widget.KeyDown += CreateKeyEvent(form);
                widget.KeyUp += CreateKeyEvent(form);
            }

            this.RegisterField(form, widget);

            return RenderFinal(form, [label, widget]);
        }

        private KeyEventHandler? CreateKeyEvent(PMSContextualForm form)
        {
            return (object sender, KeyEventArgs e) =>
            {
                OnKey?.Invoke(form);
            };
        }

        private void Widget_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (FormatValue != null)
                {
                    textBox.Text = FormatValue(textBox.Text);
                }
            }
        }

        private void Widget_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (Pattern != null)
                {
                    // https://stackoverflow.com/a/68438937
                    string text = textBox.Text.Insert(textBox.CaretIndex, e.Text);
                    e.Handled = !Pattern.IsMatch(text);
                }
            }
        }
    }

    public class FormItemCombo<O> : FormItemBase
    {
        private Dictionary<O, string> _Options;
        public Dictionary<O, string> Options
        {
            get => _Options;
            set
            {
                _Options = value;

                DidUpdateProperty("Options");
            }
        }

        public override string? IsValid(PMSContextualForm form)
        {
            string? baseValid = base.IsValid(form);
            if (baseValid != null)
            {
                return baseValid;
            }

            if (Required && this.RenderedWidget is ComboBox comboBox)
            {
                if (comboBox.SelectedItem == null)
                {
                    return "Field must have a valid option.";
                }
            }

            return null;
        }

        public override object? GetValue()
        {
            return ((ComboBox)RenderedWidget).SelectedValue;
        }
        public override void SetValue(object? value)
        {
            ((ComboBox)RenderedWidget).SelectedValue = value;
        }

        public Func<PMSContextualForm, bool> OnChange;

        public override FrameworkElement Render(PMSContextualForm form, DataItem? dataItem)
        {
            TextBlock label = RenderLabel(form);

            ComboBox widget = new();
            widget.DataContext = dataItem?.Value;

            SetFormLockedState(form);

            // Begin binding
            widget.IsReadOnly = IsReadOnly != null && IsReadOnly(form);
            widget.IsEnabled = !widget.IsReadOnly;
            widget.MaxWidth = MaxWidth;
            widget.SelectionChanged += CreateChangeEvent(form);

            // Ensure we bind to the Key and Value props on dictionary
            widget.ItemsSource = Options;
            widget.DisplayMemberPath = "Value";
            widget.SelectedValuePath = "Key";
            
            Binding valueBinding = new Binding($"{DataBinding ?? "None"}")
            {
                FallbackValue = DefaultValue != null ? DefaultValue(form) : null,
                Mode = BindingMode.OneTime
            };

            widget.SetBinding(ComboBox.SelectedValueProperty, valueBinding);

            this.RegisterField(form, widget);

            return RenderFinal(form, [label, widget]);
        }

        private SelectionChangedEventHandler? CreateChangeEvent(PMSContextualForm form)
        {
            return (object sender, SelectionChangedEventArgs e) =>
            {
                OnChange?.Invoke(form);
            };
        }

        private void Widget_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }

    public class FormItemList<O> : FormItemBase
    {
        private Dictionary<O, string> _Options;
        public Dictionary<O, string> Options
        {
            get => _Options;
            set
            {
                _Options = value;

                DidUpdateProperty("Options");
            }
        }

        public override string? IsValid(PMSContextualForm form)
        {
            string? baseValid = base.IsValid(form);
            if (baseValid != null)
            {
                return baseValid;
            }

            if (Required && this.RenderedWidget is ListBox listBox)
            {
                if (listBox.SelectedItem == null)
                {
                    return "Field must have a valid option.";
                }
            }

            return null;
        }

        public override object? GetValue()
        {
            return ((ListBox)RenderedWidget).SelectedValue;
        }
        public override void SetValue(object? value)
        {
            ((ListBox)RenderedWidget).SelectedValue = value;
        }

        public Func<PMSContextualForm, bool> OnChange;

        public override FrameworkElement Render(PMSContextualForm form, DataItem? dataItem)
        {
            TextBlock label = RenderLabel(form);

            ListBox widget = new();
            widget.DataContext = dataItem?.Value;

            SetFormLockedState(form);

            // Begin binding
            widget.IsEnabled = !(IsReadOnly != null && IsReadOnly(form));
            widget.MaxWidth = MaxWidth;
            widget.SelectionChanged += CreateChangeEvent(form);
            widget.MaxHeight = 120;

            // Ensure we bind to the Key and Value props on dictionary
            widget.ItemsSource = Options;
            widget.DisplayMemberPath = "Value";
            widget.SelectedValuePath = "Key";

            Binding valueBinding = new Binding($"{DataBinding ?? "None"}")
            {
                FallbackValue = DefaultValue != null ? DefaultValue(form) : null,
                Mode = BindingMode.OneTime
            };

            widget.SetBinding(ListBox.SelectedValueProperty, valueBinding);

            this.RegisterField(form, widget);

            return RenderFinal(form, [label, widget]);
        }

        private SelectionChangedEventHandler? CreateChangeEvent(PMSContextualForm form)
        {
            return (object sender, SelectionChangedEventArgs e) =>
            {
                OnChange?.Invoke(form);
            };
        }

        private void Widget_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }

    public class FormItemGroup : FormItemBase
    {
        public List<FormItemBase> Items { get; set; }

        public Orientation Orientation { get; set; }

        public FormItemGroup(FormItemBase[] items, string? label = null, Orientation orientation = Orientation.Horizontal)
        {
            Items = new List<FormItemBase>(items);
            Label = label;
            Orientation = orientation;
        }

        public override FrameworkElement Render(PMSContextualForm form, DataItem? dataItem)
        {
            Grid grid = new();
            grid.VerticalAlignment = VerticalAlignment.Center;

            List<FrameworkElement> itemsToRender = new();

            List<FormItemBase> visibleItems = Items.Where(i => i.Hidden != null ? !i.Hidden(form) : true).ToList();

            for (int i = 0; i < visibleItems.Count; i++)
            {
                FormItemBase item = visibleItems[i];

                FrameworkElement rendered = item.Render(form, dataItem);
                rendered.VerticalAlignment = VerticalAlignment.Stretch;

                if (Orientation == Orientation.Horizontal)
                {
                    ColumnDefinition colDef = item.MaxWidth == double.PositiveInfinity
                        ? new ColumnDefinition { Width = item.ItemGroupAlignmentMode == FormItemGroupAlignmentMode.FillContent ? GridLength.Auto : new GridLength(1, GridUnitType.Star) }
                        : new ColumnDefinition { Width = new GridLength(item.MaxWidth) };

                    grid.ColumnDefinitions.Add(colDef);
                    Grid.SetColumn(rendered, i * 2);
                }
                else
                {
                    RowDefinition rowDef = item.MaxWidth == double.PositiveInfinity
                        ? new RowDefinition { Height = item.ItemGroupAlignmentMode == FormItemGroupAlignmentMode.FillContent ? GridLength.Auto : new GridLength(1, GridUnitType.Star) }
                        : new RowDefinition { Height = new GridLength(item.MaxWidth) };

                    grid.RowDefinitions.Add(rowDef);
                    Grid.SetRow(rendered, i * 2);
                }

                grid.Children.Add(rendered);

                if (i < visibleItems.Count - 1)
                {
                    if (Orientation == Orientation.Horizontal)
                    {
                        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
                    }
                    else
                    {
                        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(10) });
                    }
                }
            }

            List<FrameworkElement> items = new List<FrameworkElement> { grid };

            if (Label != null)
            {
                items.Insert(0, RenderLabel(form));
            }

            return RenderFinal(form, items.ToArray());
        }
    }

    public class FormItemDatePicker : FormItemBase
    {
        private DatePickerFormat _DatePickerFormat;
        public DatePickerFormat DatePickerFormat
        {
            get => _DatePickerFormat;
            set
            {
                _DatePickerFormat = value;
                DidUpdateProperty("DatePickerFormat");
            }
        }

        public override string? IsValid(PMSContextualForm form)
        {
            string? baseValid = base.IsValid(form);
            if (baseValid != null)
            {
                return baseValid;
            }

            if (Required && this.RenderedWidget is DatePicker datePicker)
            {
                if (datePicker.SelectedDate == null)
                {
                    return "Field must have a valid date.";
                }
            }

            return null;
        }

        public override object? GetValue()
        {
            return ((DatePicker)RenderedWidget).SelectedDate;
        }
        public override void SetValue(object? value)
        {
            ((DatePicker)RenderedWidget).SelectedDate = (DateTime?)value;
        }

        public override FrameworkElement Render(PMSContextualForm form, DataItem? dataItem)
        {
            TextBlock label = RenderLabel(form);

            DatePicker widget = new();
            widget.DataContext = dataItem?.Value;

            SetFormLockedState(form);

            // Begin binding
            widget.IsEnabled = !(IsReadOnly != null && IsReadOnly(form));
            widget.SelectedDateFormat = DatePickerFormat;
            widget.MaxWidth = MaxWidth;

            Binding valueBinding = new Binding($"{DataBinding ?? "None"}")
            {
                FallbackValue = DefaultValue != null ? DefaultValue(form) : null,
                Mode = BindingMode.OneTime
            };

            widget.SetBinding(DatePicker.SelectedDateProperty, valueBinding);

            this.RegisterField(form, widget);

            return RenderFinal(form, [label, widget]);
        }
    }

    public class FormItemCheckbox : FormItemBase
    {
        public override FormItemGroupAlignmentMode ItemGroupAlignmentMode => FormItemGroupAlignmentMode.FillContent;

        public override string? IsValid(PMSContextualForm form)
        {
            string? baseValid = base.IsValid(form);
            if (baseValid != null)
            {
                return baseValid;
            }

            if (Required && this.RenderedWidget is CheckBox checkBox)
            {
                if (checkBox.IsChecked == null)
                {
                    return "Field requires a value.";
                }
            }

            return null;
        }

        public override object? GetValue()
        {
            return ((CheckBox)RenderedWidget).IsChecked;
        }

        public override void SetValue(object? value)
        {
            ((CheckBox)RenderedWidget).IsChecked = (bool?)value;
        }

        public override FrameworkElement Render(PMSContextualForm form, DataItem? dataItem)
        {
            TextBlock label = RenderLabel(form);

            CheckBox widget = new();
            widget.DataContext = dataItem?.Value;

            SetFormLockedState(form);

            // Begin binding
            widget.IsEnabled = !(IsReadOnly != null && IsReadOnly(form));
            widget.MaxWidth = 20;
            widget.HorizontalAlignment = HorizontalAlignment.Left;
            widget.IsThreeState = false;

            Binding valueBinding = new Binding($"{DataBinding ?? "None"}")
            {
                FallbackValue = DefaultValue != null
                ? DefaultValue(form)
                : widget.IsThreeState
                    ? null
                    : false,
                Mode = BindingMode.OneTime
            };

            widget.SetBinding(CheckBox.IsCheckedProperty, valueBinding);

            this.RegisterField(form, widget);

            return RenderFinal(form, [label, widget]);
        }
    }

    public class FormItemButton : FormItemBase
    {
        public override FormItemGroupAlignmentMode ItemGroupAlignmentMode => FormItemGroupAlignmentMode.FillContent;

        private string _ButtonLabel;
        public string ButtonLabel
        {
            get => _ButtonLabel;
            set
            {
                _ButtonLabel = value;
                DidUpdateProperty("ButtonLabel");
            }
        }

        public override string? IsValid(PMSContextualForm form)
        {
            string? baseValid = base.IsValid(form);
            if (baseValid != null)
            {
                return baseValid;
            }

            return null;
        }

        public Func<FormItemBase, PMSContextualForm, string?>? IsFieldValid;

        public override string? GetValue()
        {
            return null;
        }

        public Func<PMSContextualForm, bool> OnClick;

        public override FrameworkElement Render(PMSContextualForm form, DataItem? dataItem)
        {
            TextBlock label = RenderLabel(form);

            Button widget = new();
            widget.DataContext = dataItem?.Value;
            widget.Padding = new Thickness(2, 1, 2, 1);

            SetFormLockedState(form);

            // Begin binding
            widget.IsEnabled = !(IsReadOnly != null && IsReadOnly(form));
            widget.Content = ButtonLabel;

            widget.Click += CreateClickEvent(form);

            this.RegisterField(form, widget);

            return RenderFinal(form, [label, widget]);
        }

        private RoutedEventHandler? CreateClickEvent(PMSContextualForm form)
        {
            return (object sender, RoutedEventArgs e) =>
            {
                OnClick?.Invoke(form);
            };
        }
    }

    public class FormItemDataGrid<T> : FormItemBase
    {
        public DataContext<T> InnerDataContext { get; set; }

        public override string? IsValid(PMSContextualForm form)
        {
            string? baseValid = base.IsValid(form);
            if (baseValid != null)
            {
                return baseValid;
            }

            if (Required && this.RenderedWidget is PMSDataGrid dataGrid)
            {
                if (dataGrid.InnerDataGrid.SelectedItem == null)
                {
                    return "Field requires a selection.";
                }
            }

            return null;
        }

        public override object? GetValue()
        {
            return ((PMSDataGrid)RenderedWidget).InnerDataGrid.SelectedItem;
        }

        public override void SetValue(object? value)
        {
            ((PMSDataGrid)RenderedWidget).InnerDataGrid.SelectedItem = value;
        }

        public override FrameworkElement Render(PMSContextualForm form, DataItem? dataItem)
        {
            TextBlock label = RenderLabel(form);

            PMSDataGrid widget = new();
            widget.DataContext = InnerDataContext;

            ObservableCollection<DataItem> observableDataSource = new ObservableCollection<DataItem>();

            foreach (var item in InnerDataContext.DataSource)
            {
                Debug.WriteLine("!!! " + item);
                observableDataSource.Add(new DataItem
                {
                    Value = item,
                    IsSelected = false
                });
            }

            widget.InnerDataGrid.SelectionMode = DataGridSelectionMode.Single;
            widget.InnerDataGrid.Columns.Clear();

            Type? modelDataType = observableDataSource?.FirstOrDefault()?.Value.GetType();

            foreach (KeyValuePair<string, string> pair in InnerDataContext.CompactColumns)
            {
                string modelName = pair.Key;
                string headerName = pair.Value.Replace("_", "");

                bool isColumnHidden = pair.Value.ToCharArray()[0] == '_';

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
                        Binding = new Binding($"{modelName}")
                    };
                }
                else
                {
                    // Otherwise, use standard text columns
                    column = new DataGridTextColumn()
                    {
                        Header = headerName,
                        Binding = new Binding($"{modelName}")
                    };
                }

                column.Visibility = isColumnHidden
                    ? Visibility.Collapsed
                    : Visibility.Visible;

                widget.InnerDataGrid.Columns.Add(column);
            }

            widget.DataSource = observableDataSource;

            SetFormLockedState(form);

            // Begin binding
            widget.IsEnabled = !(IsReadOnly != null && IsReadOnly(form));

            Binding valueBinding = new Binding($"{DataBinding ?? "None"}")
            {
                FallbackValue = DefaultValue != null ? DefaultValue(form) : null,
                Mode = BindingMode.OneTime
            };

            widget.InnerDataGrid.SetBinding(DataGrid.SelectedItemProperty, valueBinding);

            this.RegisterField(form, widget);

            return RenderFinal(form, [label, widget]);
        }
    }
}
