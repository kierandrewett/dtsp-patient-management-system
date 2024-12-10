using Microsoft.IdentityModel.Tokens;
using PMS.Components;
using PMS.Context;
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
using System.Media;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        
        private bool _ForceEnable;
        public bool ForceEnable
        {
            get => _ForceEnable;
            set
            {
                _ForceEnable = value;
                DidUpdateProperty("ForceEnable");
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

        public Func<PMSContextualForm, bool> OnPaint;

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

        private bool HasPaintedOnce = false;

        public TextBlock? RenderLabel(PMSContextualForm form)
        {
            if (!Label.IsNullOrEmpty())
            {
                TextBlock textBlock = new();
                textBlock.VerticalAlignment = VerticalAlignment.Center;

                Label label = new()
                {
                    Content = Label,
                    FontWeight = FontWeights.Bold
                };

                textBlock.Inlines.Add(label);

                if (!ForceEnable)
                {
                    if (IsFormLocked)
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
                }

                return textBlock;
            } else
            {
                return null;
            }
        }

        public StackPanel RenderFinal(PMSContextualForm form, FrameworkElement?[] items, Orientation spOrientation = Orientation.Vertical)
        {
            StackPanel parentSp = new();
            parentSp.Orientation = Orientation.Vertical;
            parentSp.Margin = new Thickness(0, 4, 0, 4);

            StackPanel sp = new();
            sp.Orientation = spOrientation;
            sp.VerticalAlignment = VerticalAlignment.Center;

            foreach (var item in items)
            {
                if (item != null)
                {
                    sp.Children.Add(item);
                }
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

            parentSp.LayoutUpdated += ParentSp_LayoutUpdated(form);
            parentSp.Initialized += ParentSp_LayoutUpdated(form);
            parentSp.IsVisibleChanged += ParentSp_IsVisibleChanged(form);

            return parentSp;
        }

        private DependencyPropertyChangedEventHandler ParentSp_IsVisibleChanged(PMSContextualForm form)
        {
            return (object sender, DependencyPropertyChangedEventArgs e) =>
            {
                OnPaint?.Invoke(form);
            };
        }

        private EventHandler ParentSp_LayoutUpdated(PMSContextualForm form)
        {
            return (object sender, EventArgs e) =>
            {
                OnPaint?.Invoke(form);
            };
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
            SetFormLockedState(form);

            TextBlock label = RenderLabel(form);

            TextBox widget = new();
            widget.DataContext = dataItem?.Value;

            // Begin binding
            widget.IsReadOnly = ForceEnable == true ? false : IsFormLocked || (IsReadOnly != null && IsReadOnly(form));
            widget.IsEnabled = !widget.IsReadOnly;
            widget.MaxWidth = MaxWidth;
            widget.MinHeight = 22;
            widget.Padding = new Thickness(2, 2, 2, 2);
            AutomationProperties.SetName(widget, Label);

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
            SetFormLockedState(form);

            TextBlock label = RenderLabel(form);

            ComboBox widget = new();
            widget.DataContext = dataItem?.Value;

            // Begin binding
            widget.IsReadOnly = ForceEnable == true ? false : IsFormLocked || (IsReadOnly != null && IsReadOnly(form));
            widget.IsEnabled = !widget.IsReadOnly;
            widget.MaxWidth = MaxWidth;
            widget.SelectionChanged += CreateChangeEvent(form);
            AutomationProperties.SetName(widget, Label);

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
            SetFormLockedState(form);
            
            TextBlock label = RenderLabel(form);

            ListBox widget = new();
            widget.DataContext = dataItem?.Value;

            // Begin binding
            widget.IsEnabled = ForceEnable == true ? true : !IsFormLocked && !(IsReadOnly != null && IsReadOnly(form));
            widget.MaxWidth = MaxWidth;
            widget.SelectionChanged += CreateChangeEvent(form);
            widget.MaxHeight = 120;
            AutomationProperties.SetName(widget, Label);

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
            List<FormItemBase> hiddenItems = Items.Where(i => i.Hidden != null ? i.Hidden(form) : false).ToList();

            // We do this to register the hidden fields to our form
            // Otherwise, relationship fields are excluded from ORM reflection
            foreach (FormItemBase hiddenItem in hiddenItems)
            {
                hiddenItem.Render(form, dataItem);
            }

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
            SetFormLockedState(form);

            TextBlock label = RenderLabel(form);

            DatePicker widget = new();
            widget.DataContext = dataItem?.Value;

            // Begin binding
            widget.IsEnabled = ForceEnable == true ? true : !IsFormLocked && !(IsReadOnly != null && IsReadOnly(form));
            widget.SelectedDateFormat = DatePickerFormat;
            widget.MaxWidth = MaxWidth;
            AutomationProperties.SetName(widget, Label);

            Binding valueBinding = new Binding($"{DataBinding ?? "None"}")
            {
                FallbackValue = DefaultValue != null ? DefaultValue(form) : null,
                Mode = BindingMode.OneTime
            };

            widget.SetBinding(DatePicker.SelectedDateProperty, valueBinding);

            widget.GotFocus += Widget_GotFocus;
            widget.Loaded += Widget_Loaded;

            this.RegisterField(form, widget);

            return RenderFinal(form, [label, widget]);
        }

        private void Widget_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is DatePicker datePicker)
            {
                // Since the internal TextBox is apart of the DatePicker template
                // Try to find it by its internal WPF part name, then add the Focus
                // listener to it to handle initial focusing.
                TextBox? datePickerTextBox = (TextBox)datePicker.Template.FindName(
                    "PART_TextBox", 
                    datePicker
                );

                if (datePickerTextBox != null)
                {
                    datePickerTextBox.GotFocus += Widget_GotFocus;
                }
            }
        }

        private void HandleDatePickerFocus(RoutedEventArgs e)
        {
            if (RenderedWidget is DatePicker datePicker)
            {
                if (Window.GetWindow(datePicker) is PMSWindow win)
                {
                    e.Handled = true;

                    string accessibleName = Label ?? "Date Picker";
                    string dateTime = datePicker.SelectedDate != null
                        ? DateHelper.ConvertToString(
                            (DateTime)datePicker.SelectedDate, 
                            DatePickerFormat == DatePickerFormat.Long
                          )
                        : "No date selected";

                    win.AccessibilityController.CancelAll();
                    win.AccessibilityController.MaybeAnnounceArbritary(
                        $"{accessibleName}: {dateTime}"
                    );
                }
            }
        }

        private void Widget_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is DatePicker || sender is TextBox)
            {
                HandleDatePickerFocus(e);
            }
        }
    }

    public class FormItemCalendar : FormItemBase
    {
        public CalendarDateRange DateRange { get; set; }
        private PMSContextualForm LazyForm { get; set; }

        private bool _AlreadyGotFocus = false;

        public override string? IsValid(PMSContextualForm form)
        {
            string? baseValid = base.IsValid(form);
            if (baseValid != null)
            {
                return baseValid;
            }

            if (Required && this.RenderedWidget is Calendar calendar)
            {
                if (calendar.SelectedDate == null)
                {
                    return "Field must have a valid date.";
                }
            }

            return null;
        }

        public override object? GetValue()
        {
            return ((Calendar)RenderedWidget).SelectedDate;
        }
        public override void SetValue(object? value)
        {
            ((Calendar)RenderedWidget).SelectedDate = (DateTime?)value;
            OnChange?.Invoke(LazyForm);
        }

        public Func<PMSContextualForm, bool> OnChange;


        public override FrameworkElement Render(PMSContextualForm form, DataItem? dataItem)
        {
            SetFormLockedState(form);
            LazyForm = form;

            TextBlock label = RenderLabel(form);

            Calendar widget = new();
            widget.DataContext = dataItem?.Value;

            // Begin binding
            widget.IsEnabled = ForceEnable == true ? true : !IsFormLocked && !(IsReadOnly != null && IsReadOnly(form));
            widget.MaxWidth = MaxWidth;
            AutomationProperties.SetName(widget, Label);

            if (DateRange != null)
            {
                widget.DisplayDateStart = DateRange.Start;
                widget.DisplayDateEnd = DateRange.End;
            }

            Binding valueBinding = new Binding($"{DataBinding ?? "None"}")
            {
                FallbackValue = DefaultValue != null ? DefaultValue(form) : null,
                Mode = BindingMode.OneTime
            };

            widget.SetBinding(Calendar.SelectedDateProperty, valueBinding);
            OnChange?.Invoke(LazyForm);

            widget.SelectedDatesChanged += Widget_SelectedDatesChanged;
            widget.GotFocus += Widget_GotFocus;
            widget.LostFocus += Widget_LostFocus;

            this.RegisterField(form, widget);

            return RenderFinal(form, [label, widget]);
        }

        private void Widget_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is Calendar calendar)
            {
                FrameworkElement? focusedElement = (FrameworkElement)Keyboard.FocusedElement;

                // Ignore lost focus events that bubble if the new focus will be a CalendarDayButton
                if (
                    focusedElement != null &&
                    focusedElement is not CalendarDayButton
                ) {
                    _AlreadyGotFocus = false;
                }
            }
        }

        private void Widget_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is Calendar calendar)
            {
                if (Window.GetWindow(calendar) is PMSWindow win)
                {
                    string accessibleName = Label ?? "Calendar";
                    string dateTime = calendar.SelectedDate != null
                        ? DateHelper.ConvertToString((DateTime)calendar.SelectedDate, false)
                        : "No date selected";

                    string accessibleAnnouncement = _AlreadyGotFocus
                        ? dateTime
                        : $"{accessibleName}: {dateTime}";

                    win.AccessibilityController.CancelAll();
                    win.AccessibilityController.MaybeAnnounceArbritary(
                        accessibleAnnouncement
                    );

                    _AlreadyGotFocus = true;
                }
            }
        }

        private void Widget_SelectedDatesChanged(object? sender, SelectionChangedEventArgs e)
        {
            OnChange?.Invoke(LazyForm);
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
            SetFormLockedState(form);

            TextBlock label = RenderLabel(form);

            CheckBox widget = new();
            widget.DataContext = dataItem?.Value;
            AutomationProperties.SetName(widget, Label);

            // Begin binding
            widget.IsEnabled = ForceEnable == true ? true : !IsFormLocked && !(IsReadOnly != null && IsReadOnly(form));
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

        private string _AccessibleLabel;
        public string AccessibleLabel
        {
            get => _AccessibleLabel;
            set
            {
                _AccessibleLabel = value;
                DidUpdateProperty("AccessibleLabel");
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
            SetFormLockedState(form);

            TextBlock label = RenderLabel(form);

            Button widget = new();
            widget.DataContext = dataItem?.Value;
            widget.Padding = new Thickness(2, 1, 2, 1);

            if (!AccessibleLabel.IsNullOrEmpty())
            {
                AutomationProperties.SetName(widget, AccessibleLabel);
            }

            // Begin binding
            widget.IsEnabled = ForceEnable == true ? ForceEnable : !IsFormLocked && !(IsReadOnly != null && IsReadOnly(form));
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
        public Func<PMSContextualForm, DataContext<T>> InnerDataContext { get; set; }
        public string ValuePath { get; set; }

        public override string? IsValid(PMSContextualForm form)
        {
            string? baseValid = base.IsValid(form);
            if (baseValid != null)
            {
                return baseValid;
            }

            if (Required && this.RenderedWidget is PMSDataGrid dataGrid)
            {
                if (dataGrid.InnerDataGrid.SelectedValue == null)
                {
                    return "Field requires a selection.";
                }
            }

            return null;
        }

        public override object? GetValue()
        {
            return ((PMSDataGrid)RenderedWidget).InnerDataGrid.SelectedValue;
        }

        public override void SetValue(object? value)
        {
            ((PMSDataGrid)RenderedWidget).InnerDataGrid.SelectedValue = value;
        }

        public Func<PMSContextualForm, bool> OnChange;

        public override FrameworkElement Render(PMSContextualForm form, DataItem? dataItem)
        {
            SetFormLockedState(form);

            TextBlock label = RenderLabel(form);

            DataContext<T> ComputedDataContext = InnerDataContext(form);

            PMSDataGrid widget = new();
            widget.DataContext = ComputedDataContext;

            ObservableCollection<DataItem> observableDataSource = new ObservableCollection<DataItem>();

            foreach (var item in ComputedDataContext.DataSource)
            {
                observableDataSource.Add(new DataItem
                {
                    Value = item,
                    IsSelected = false
                });
            }

            widget.InnerDataGrid.SelectionMode = DataGridSelectionMode.Single;
            widget.InnerDataGrid.SelectedValuePath = ValuePath;
            widget.InnerDataGrid.Columns.Clear();

            Type? modelDataType = observableDataSource?.FirstOrDefault()?.Value.GetType();

            foreach (KeyValuePair<string, string> pair in ComputedDataContext.CompactColumns ?? ComputedDataContext.Columns)
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

            Binding valueBinding = new Binding($"{DataBinding ?? "None"}")
            {
                FallbackValue = DefaultValue != null ? DefaultValue(form) : null,
                Mode = BindingMode.OneTime
            };

            widget.InnerDataGrid.SetBinding(DataGrid.SelectedItemProperty, valueBinding);

            widget.IsEnabled = ForceEnable == true ? true : !IsFormLocked && !(IsReadOnly != null && IsReadOnly(form));

            this.RegisterField(form, widget);

            return RenderFinal(form, [label, widget]);
        }
        private SelectionChangedEventHandler? CreateSelectionChangedEvent(PMSContextualForm form)
        {
            return (object sender, SelectionChangedEventArgs e) =>
            {
                OnChange?.Invoke(form);
            };
        }

        private void ObservableDataSource_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }

    public class FormItemDataManager<T> : FormItemBase
    {
        public Func<PMSContextualForm, DataContext<T>> InnerDataContext { get; set; }

        public override string? IsValid(PMSContextualForm form)
        {
            string? baseValid = base.IsValid(form);
            if (baseValid != null)
            {
                return baseValid;
            }

            return null;
        }

        public override object? GetValue()
        {
            return ((PMSDataManager)RenderedWidget).DataGrid.InnerDataGrid.SelectedValue;
        }

        public override void SetValue(object? value)
        {
            ((PMSDataManager)RenderedWidget).DataGrid.InnerDataGrid.SelectedValue = value;
        }

        public override FrameworkElement Render(PMSContextualForm form, DataItem? dataItem)
        {
            SetFormLockedState(form);

            TextBlock label = RenderLabel(form);

            DataContext<T> ComputedDataContext = InnerDataContext(form);

            PMSDataManager widget = new();

            foreach (var property in ComputedDataContext.GetType().GetProperties())
            {
                var widgetProperty = widget.GetType().GetProperty(property.Name);
                if (widgetProperty != null && widgetProperty.CanWrite)
                {
                    widgetProperty.SetValue(widget, property.GetValue(ComputedDataContext));
                }
            }

            Binding valueBinding = new Binding($"{DataBinding ?? "None"}")
            {
                FallbackValue = DefaultValue != null ? DefaultValue(form) : null,
                Mode = BindingMode.OneTime
            };

            widget.DataGrid.InnerDataGrid.SetBinding(DataGrid.SelectedItemProperty, valueBinding);

            this.RegisterField(form, widget);

            return RenderFinal(form, [label, widget]);
        }
    }

    public class FormItemTimePicker : FormItemBase
    {
        TextBox HourElement { get; set; }
        TextBox MinuteElement { get; set; }

        PMSContextualForm LazyForm { get; set; }

        public override string? IsValid(PMSContextualForm form)
        {
            string? baseValid = base.IsValid(form);
            if (baseValid != null)
            {
                return baseValid;
            }

            if (Required && this.HourElement is TextBox hourTb && this.MinuteElement is TextBox minTb)
            {
                if (
                    (hourTb.Text.IsNullOrEmpty() || hourTb.Text.Trim().Length < 0) ||
                    (minTb.Text.IsNullOrEmpty() || minTb.Text.Trim().Length < 0)
                )
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
            return HourElement.Text + ":" + MinuteElement.Text;
        }

        public override void SetValue(object? value)
        {
            string[] split = value.ToString().Split(":");

            HourElement.Text = split[0];
            MinuteElement.Text = split[1];
        }

        public Func<PMSContextualForm, bool> OnChange;

        private StackPanel CreateStepper(string name, TextBox textBox, int maxValue = 60, int increment = 15)
        {
            string accessibleName = Label ?? "Time Picker";

            StackPanel stepperSp = new();
            stepperSp.Orientation = Orientation.Vertical;

            Button upButton = new();
            upButton.Content = "🞁";
            upButton.FontSize = 8;
            upButton.Style = null;
            upButton.Padding = new Thickness(4, 0, 4, 0);
            upButton.Click += (object? sender, RoutedEventArgs e) =>
            {
                int parsed;

                if (int.TryParse(textBox.Text, out parsed))
                {
                    int newNum = (parsed + increment) % maxValue;

                    textBox.Text = newNum.ToString().PadLeft(2, '0');
                }
            };
            AutomationProperties.SetName(upButton, $"{accessibleName}: {name}: Increment by {increment}");

            Button downButton = new();
            downButton.Content = "🞃";
            downButton.FontSize = 8;
            downButton.Style = null;
            downButton.Padding = new Thickness(4, 0, 4, 0);
            downButton.Click += (object? sender, RoutedEventArgs e) =>
            {
                int parsed;

                if (int.TryParse(textBox.Text, out parsed))
                {
                    int newNum = (parsed + maxValue - increment) % maxValue;

                    textBox.Text = newNum.ToString().PadLeft(2, '0');
                }
            };
            AutomationProperties.SetName(downButton, $"{accessibleName}: {name}: Decrement by {increment}");

            stepperSp.Children.Add(upButton);
            stepperSp.Children.Add(downButton);

            return stepperSp;
        }

        public override FrameworkElement Render(PMSContextualForm form, DataItem? dataItem)
        {
            SetFormLockedState(form);
            LazyForm = form;

            TextBlock label = RenderLabel(form);
            TextBox widget = new();
            widget.Visibility = Visibility.Collapsed;
            widget.DataContext = dataItem?.Value;
            widget.IsEnabled = false;
            widget.MinWidth = 38;
            widget.MaxWidth = 38;
            widget.Margin = new Thickness(0, 0, 8, 0);

            HourElement = new TextBox();
            MinuteElement = new TextBox();

            HourElement.DataContext = dataItem?.Value;
            MinuteElement.DataContext = dataItem?.Value;


            // Begin binding
            HourElement.IsReadOnly = ForceEnable == true ? false : IsFormLocked || (IsReadOnly != null && IsReadOnly(form));
            MinuteElement.IsReadOnly = ForceEnable == true ? false : IsFormLocked || (IsReadOnly != null && IsReadOnly(form));

            HourElement.IsEnabled = !HourElement.IsReadOnly;
            MinuteElement.IsEnabled = !MinuteElement.IsReadOnly;

            HourElement.MaxWidth = 30;
            HourElement.MinWidth = 30;

            MinuteElement.MaxWidth = 30;
            MinuteElement.MinWidth = 30;

            HourElement.MinHeight = 22;
            MinuteElement.MinHeight = 22;
            HourElement.MinHeight = 22;

            HourElement.Padding = new Thickness(2, 2, 2, 2);
            MinuteElement.Padding = new Thickness(2, 2, 2, 2);

            Binding textBinding = new Binding($"{DataBinding ?? "None"}")
            {
                FallbackValue = DefaultValue != null ? DefaultValue(form) : "",
                Mode = BindingMode.OneTime
            };

            widget.SetBinding(TextBox.TextProperty, textBinding);

            string[] split = widget.Text.Split(':');

            if (split.Length == 2)
            {
                HourElement.Text = split[0];
                MinuteElement.Text = split[1];
            }

            widget.TextChanged += Widget_TextChanged;

            HourElement.TextChanged += TimeElement_TextChanged;
            HourElement.PreviewTextInput += TimeElement_PreviewTextInput;
            HourElement.PreviewKeyDown += CreateTimeElementPreviewKeyDownHandler(24);
            HourElement.GotFocus += CreateTimeElementGotFocusHandler("Hour");

            MinuteElement.TextChanged += TimeElement_TextChanged;
            MinuteElement.PreviewTextInput += TimeElement_PreviewTextInput;
            MinuteElement.PreviewKeyDown += CreateTimeElementPreviewKeyDownHandler(60);
            MinuteElement.GotFocus += CreateTimeElementGotFocusHandler("Minute");

            StackPanel sp = new();

            sp.Orientation = Orientation.Horizontal;
            sp.Children.Add(widget);

            StackPanel hourSp = new();
            hourSp.Orientation = Orientation.Horizontal;
            hourSp.Children.Add(HourElement);
            hourSp.Children.Add(CreateStepper("Hour", HourElement, 24, 1));

            StackPanel minSp = new();
            minSp.Orientation = Orientation.Horizontal;
            minSp.Children.Add(MinuteElement);
            minSp.Children.Add(CreateStepper("Minute", MinuteElement));

            sp.Children.Add(hourSp);
            sp.Children.Add(new Label { Content = ":", FontWeight = FontWeights.Black });
            sp.Children.Add(minSp);

            this.RegisterField(form, widget);
            UpdateInternalElement(form);

            widget.Loaded += Widget_Loaded;

            return RenderFinal(form, [label, sp]);
        }

        private void Widget_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateInternalElement(LazyForm);
        }

        private void UpdateInternalElement(PMSContextualForm form)
        { 
            if (RenderedWidget != null)
            {
                ((TextBox)RenderedWidget).Text = $"{HourElement.Text}:{MinuteElement.Text}";

                OnChange?.Invoke(form);
            }
        }

        private void TimeElement_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                bool isDigit = char.IsDigit(e.Text, 0);

                if (!isDigit)
                {
                    SystemSounds.Asterisk.Play();
                }

                e.Handled = !isDigit;
            }
        }

        private void TimeElement_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string currentText = textBox.Text;

                if (currentText.Length == 1)
                {
                    textBox.Text = currentText.PadLeft(2, '0');
                    textBox.SelectionStart = textBox.Text.Length;
                }

                UpdateInternalElement(LazyForm);
            }
        }

        private RoutedEventHandler CreateTimeElementGotFocusHandler(string name)
        {
            return (object sender, RoutedEventArgs e) =>
            {
                if (sender is TextBox textBox)
                {
                    if (Window.GetWindow(textBox) is PMSWindow win)
                    {
                        e.Handled = true;

                        string accessibleTimePickerName = Label ?? "Time Picker";
                        string fullTimeText = ((TextBox)RenderedWidget).Text;
                        string accessibleName = $"{accessibleTimePickerName}: {name}: {fullTimeText}";

                        win.AccessibilityController.CancelAll();
                        win.AccessibilityController.MaybeAnnounceArbritary(
                            accessibleName,
                            true
                        );
                    }
                }
            };
        }

        private KeyEventHandler CreateTimeElementPreviewKeyDownHandler(int maxInt = 99)
        {
            return (object sender, KeyEventArgs e) =>
            {
                if (sender is TextBox textBox)
                {
                    if (textBox.Text.Length == 2 && e.Key != Key.Back && e.Key != Key.Delete && e.Key != Key.Tab)
                    {
                        string currentText = textBox.Text;

                        if (char.IsDigit((char)KeyInterop.VirtualKeyFromKey(e.Key)))
                        {
                            string firstNum = currentText.Substring(1);
                            string secondNum = e.Key.ToString().Last().ToString();

                            string combinedNewNum = firstNum + secondNum;

                            if (int.Parse(combinedNewNum) >= maxInt)
                            {
                                SystemSounds.Asterisk.Play();
                                e.Handled = true;
                                return;
                            }

                            textBox.Text = currentText.Substring(1) + e.Key.ToString().Last();
                            textBox.SelectionStart = 2;

                            UpdateInternalElement(LazyForm);
                        }

                        e.Handled = true;
                    }
                }
            };
        }

        private void Widget_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                SetValue(textBox.Text);

                if (Window.GetWindow(textBox) is PMSWindow win)
                {
                    win.AccessibilityController.CancelAll();
                    win.AccessibilityController.MaybeAnnounceArbritary(
                        textBox.Text,
                        true
                    );
                }
            }
        }
    }
}
