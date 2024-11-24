using Microsoft.IdentityModel.Tokens;
using PMS.Components;
using PMS.Util;
using System;
using System.Collections.Generic;
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

        private string _DataBinding;
        public string DataBinding
        {
            get => _DataBinding;
            set
            {
                _DataBinding = value;
                DidUpdateProperty("DataBinding");
            }
        }

        private bool _IsReadOnly;
        public bool IsReadOnly
        {
            get => _IsReadOnly;
            set
            {
                _IsReadOnly = value;
                DidUpdateProperty("IsReadOnly");
            }
        }

        public Func<object> DefaultValue;

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

        public FrameworkElement RenderedWidget { get; set; }

        public void RegisterField(PMSContextualForm form, FrameworkElement widget)
        {
            Debug.WriteLine("registering " + this + " " + widget);
            form.RegisterField(this, widget);
            RenderedWidget = widget;
        }

        public virtual FrameworkElement Render(PMSContextualForm form, DataItem? dataItem)
        {
            throw new Exception("no impl");
        }

        public virtual string? IsValid()
        {
            if (RenderedWidget == null)
            {
                return "No rendered widget.";
            }

            return null;
        }

        public TextBlock RenderLabel()
        {
            TextBlock textBlock = new();
            Label label = new()
            {
                Content = Label,
                FontWeight = FontWeights.Bold
            };

            textBlock.Inlines.Add(label);

            if (Required)
            {
                Label requiredAsterisk = new()
                {
                    Content = "*",
                    Foreground = Brushes.Red,
                    FontWeight = FontWeights.Bold
                };

                textBlock.Inlines.Add(requiredAsterisk);
            }

            return textBlock;
        }

        public StackPanel RenderFinal(FrameworkElement[] items)
        {
            StackPanel sp = new();
            sp.Margin = new Thickness(0, 4, 0, 4);

            foreach (var item in items)
            {
                sp.Children.Add(item);
            }

            return sp;
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

        public Func<string, string> FormatValue;

        public override string? IsValid()
        {
            string? baseValid = base.IsValid();
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

            return null;
        }

        public override FrameworkElement Render(PMSContextualForm form, DataItem? dataItem)
        {
            TextBlock label = RenderLabel();

            TextBox widget = new();
            widget.DataContext = dataItem?.Value;

            // Begin binding
            widget.IsReadOnly = IsReadOnly;
            widget.IsEnabled = !IsReadOnly;

            Binding textBinding = new Binding($"{DataBinding}")
            {
                FallbackValue = DefaultValue != null ? DefaultValue() : "",
                Mode = BindingMode.OneTime
            };

            widget.SetBinding(TextBox.TextProperty, textBinding);

            widget.PreviewTextInput += Widget_PreviewTextInput;
            widget.LostFocus += Widget_LostFocus;

            this.RegisterField(form, widget);

            return RenderFinal([label, widget]);
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
                    Debug.WriteLine(text);
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

        public override string? IsValid()
        {
            string? baseValid = base.IsValid();
            if (baseValid != null)
            {
                return baseValid;
            }

            if (Required && this.RenderedWidget is ComboBox comboBox)
            {
                if (comboBox.SelectedItem == null)
                {
                    return "Field must have a valid option";
                }
            }

            return null;
        }

        public override FrameworkElement Render(PMSContextualForm form, DataItem? dataItem)
        {
            TextBlock label = RenderLabel();

            ComboBox widget = new();
            widget.DataContext = dataItem?.Value;

            // Begin binding
            widget.IsReadOnly = IsReadOnly;
            widget.IsEnabled = !IsReadOnly;

            // Ensure we bind to the Key and Value props on dictionary
            widget.ItemsSource = Options;
            widget.DisplayMemberPath = "Value";
            widget.SelectedValuePath = "Key";
            
            Binding valueBinding = new Binding($"{DataBinding}")
            {
                FallbackValue = DefaultValue != null ? DefaultValue() : null,
                Mode = BindingMode.OneTime
            };

            widget.SetBinding(ComboBox.SelectedValueProperty, valueBinding);

            this.RegisterField(form, widget);

            return RenderFinal([label, widget]);
        }
    }

    public class FormItemGroup : FormItemBase
    {
        public List<FormItemBase> Items { get; set; }

        public FormItemGroup(params FormItemBase[] items)
        {
            Items = new List<FormItemBase>(items);
        }

        public override FrameworkElement Render(PMSContextualForm form, DataItem? dataItem)
        {
            Grid grid = new();

            List<FrameworkElement> itemsToRender = new();

            for (int i = 0; i < Items.Count; i++)
            {
                FormItemBase item = Items[i];
                FrameworkElement rendered = item.Render(form, dataItem);

                grid.ColumnDefinitions.Add(new ColumnDefinition());
                Grid.SetColumn(rendered, i * 2);
                grid.Children.Add(rendered);

                if (i < Items.Count - 1)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
                }
            }

            return RenderFinal([grid]);
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

        public override string? IsValid()
        {
            string? baseValid = base.IsValid();
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

        public override FrameworkElement Render(PMSContextualForm form, DataItem? dataItem)
        {
            TextBlock label = RenderLabel();

            DatePicker widget = new();
            widget.DataContext = dataItem?.Value;

            // Begin binding
            widget.IsEnabled = !IsReadOnly;
            widget.SelectedDateFormat = DatePickerFormat;

            Binding valueBinding = new Binding($"{DataBinding}")
            {
                FallbackValue = DefaultValue != null ? DefaultValue() : null,
                Mode = BindingMode.OneTime
            };

            widget.SetBinding(DatePicker.SelectedDateProperty, valueBinding);

            this.RegisterField(form, widget);

            return RenderFinal([label, widget]);
        }
    }
}
