using Microsoft.IdentityModel.Tokens;
using PMS.Components;
using PMS.Context;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Xml.Linq;

namespace PMS.Controllers
{
    public class AccessibilityController
    {
        private Window Window { get; set; }
        private SettingsController SettingsController { get; set; }
        private SynthesizerController SynthesizerController
        {
            get {
                WindowManager wm = (WindowManager)Application.Current.MainWindow;

                return wm.SynthesizerController;
            }
        }

        private DispatcherTimer? FocusTimer { get; set; }
        private DispatcherTimer? ClickTimer { get; set; }

        private DispatcherTimer? TypingTimer { get; set; }

        public AccessibilityController(Window win) 
        {
            Window = win;

            SettingsController = new SettingsController();

            RegisterFocusEvents(Window);

            Window.Activated += Window_Activated;
            Window.Deactivated += Window_Deactivated;
        }
         
        private void RegisterFocusEvents(UIElement target)
        {
            target.AddHandler(UIElement.GotFocusEvent, new RoutedEventHandler(OnElementGotFocus));
            target.AddHandler(UIElement.LostFocusEvent, new RoutedEventHandler(OnElementLostFocus));
        }

        private void UnregisterFocusEvents(UIElement target)
        {
            target.RemoveHandler(UIElement.GotFocusEvent, new RoutedEventHandler(OnElementGotFocus));
            target.RemoveHandler(UIElement.LostFocusEvent, new RoutedEventHandler(OnElementLostFocus));
        }

        private void Window_Deactivated(object? sender, EventArgs e)
        {
            RemoveFocusListeners(Window);
        }

        private void Window_Activated(object? sender, EventArgs e)
        {
            AddFocusListeners(Window);
        }

        private static bool IsFrameworkElement(object element)
        {
            return element is FrameworkElement || element is ContentElement;
        }

        private bool CanAnnounceWindowElements()
        {
            return SettingsController.StoredSettings.IsTTSEnabled ?? false;
        }

        public void RegisterPopup(Popup popup)
        {
            RegisterFocusEvents(popup);
        }

        public void UnregisterPopup(Popup popup)
        {
            UnregisterFocusEvents(popup);
        }

        private Label? FindNearestLabel(Control control, int siblingDirection = -1)
        {
            try
            {
                DependencyObject? parent = control.Parent;

                if (
                    parent == null ||
                    // DataGridX elements cause issues, as they are typically virtualised
                    // This means the children count can be inflated and cause a stack overflow
                    // when trying to iterate over each item; just skip it.
                    parent is DataGrid ||
                    parent is DataGridCell
                ) {
                    return null;
                }

                int childIdx = -1;
                int childCount = VisualTreeHelper.GetChildrenCount(parent);

                // Iterate over our parent's children until we find the index of our provided control
                // This gives us a starting point to start searching for a label
                for (int i = 0; i < childCount; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                    if (child == control)
                    {
                        childIdx = i;
                        break;
                    }
                }

                int startIdx = childIdx + siblingDirection;
                for (int i = startIdx; i >= 0 && i < childCount; i += siblingDirection)
                {
                    DependencyObject previousSibling = VisualTreeHelper.GetChild(parent, i);

                    if (previousSibling is Label label)
                    {
                        return label;
                    }
                }

                // Try looking after the checkbox if we haven't found a label yet
                if (control is CheckBox checkBox && siblingDirection < 0)
                {
                    return FindNearestLabel(control, 1);
                }
            } catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            
            return null;
        }

        private string[] FindTextNodes(DependencyObject control, List<string>? textNodes = null)
        {
            if (textNodes == null)
            {
                textNodes = new List<string>();
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(control); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(control, i);

                switch (child)
                {
                    case TextBlock textBlock:
                        {
                            textNodes.Add(textBlock.Text.Trim());
                            break;
                        }
                    case Label label:
                        {
                            if (label.Content is string content)
                            {
                                textNodes.Add(content);
                            }
                            break;
                        }
                }

                FindTextNodes(child, textNodes);
            }

            // Distinct() ensures we only have unique values in the list
            return textNodes.Distinct().ToArray();
        }

        private string GetTextFromInlines(InlineCollection inlines)
        {
            string text = "";

            foreach (var inline in inlines)
            {
                if (inline is Run run)
                {
                    text += run.Text.Trim();
                }
            }

            return text;
        }

        private void SetFocusListeners(DependencyObject dependencyObject, bool state = false)
        {
            Func<RoutedEvent, RoutedEventHandler, bool> HandlerFn = state
                ? (evt, handler) => { Window.AddHandler(evt, handler); return true; }
                : (evt, handler) => { Window.RemoveHandler(evt, handler); return true; };

            switch (dependencyObject)
            {
                case Button:
                    {
                        HandlerFn(Button.PreviewMouseUpEvent, new RoutedEventHandler(OnButtonPreviewClick));
                        break;
                    }
                case CheckBox:
                    {
                        HandlerFn(CheckBox.CheckedEvent, new RoutedEventHandler(OnCheckBoxCheckedStateChange));
                        HandlerFn(CheckBox.UncheckedEvent, new RoutedEventHandler(OnCheckBoxCheckedStateChange));

                        break;
                    }
                case ComboBox:
                    {
                        HandlerFn(ComboBox.SelectionChangedEvent, new RoutedEventHandler(OnComboBoxSelectionChange));

                        break;
                    }
                case TextBox:
                    {
                        HandlerFn(TextBox.TextChangedEvent, new RoutedEventHandler(OnTextBoxTextChanged));

                        break;
                    }
                case Slider:
                    {
                        HandlerFn(Slider.ValueChangedEvent, new RoutedEventHandler(OnSliderValueChanged));

                        break;
                    }
            }
        }

        private void AddFocusListeners(DependencyObject dependencyObject)
        {
            SetFocusListeners(dependencyObject, true);
        }

        private void RemoveFocusListeners(DependencyObject dependencyObject)
        {
            SetFocusListeners(dependencyObject, false);
        }

        public void CancelCurrentSpeech()
        {
            if (ClickTimer != null && ClickTimer.IsEnabled)
            {
                ClickTimer.Stop();
            }

            SynthesizerController.CancelSpeech(
                SynthesizerController.CurrentPrompt
            );
        }

        public void CancelAll()
        {
            SynthesizerController.CancelSpeech();
        }

        public Prompt MaybeAnnounceArbritary(string synthesisOutput, bool cancelCurrent = true)
        {
            // We want the synthesizer to be running all the time!
            //
            // This allows the process to be fail-close as we can pick
            // up issues early without needing the TTS flag enabled.
            //
            // If TTS is disabled, just mute the synthesizer's output.
            int volume = CanAnnounceWindowElements() ? 100 : 0;

            return SynthesizerController.Speak(synthesisOutput, volume, cancelCurrent);
        }

        private string? GetAutomationName(DependencyObject dependencyObject)
        {
            string? name = AutomationProperties.GetName(dependencyObject)?.Trim();

            return !name.IsNullOrEmpty()
                ? name
                : null;
        }

        private string GetAccessibleAnnouncementValue(
            DependencyObject dependencyObject, 
            bool isFirstFocus = true,
            bool announceFieldContents = false
        ) {
            string synthesisOutput = GetAutomationName(dependencyObject) ?? "";

            switch (dependencyObject)
            {
                case PMSWindow window:
                    {
                        synthesisOutput = $"Window: {window.Title}";
                        break;
                    }
                case Button button:
                    {
                        string visualLabel = string.Join(' ', FindTextNodes(button));

                        synthesisOutput = GetAutomationName(dependencyObject) ?? $"Button: {visualLabel}";
                        break;
                    }
                case PMSTabItem tabItem:
                    {
                        string visualLabel = string.Join(' ', FindTextNodes(tabItem));

                        synthesisOutput = $"Tab: {visualLabel}";
                        break;
                    }
                case Label label:
                    {
                        synthesisOutput = label.Content?.ToString() ?? "";
                        break;
                    }
                case TextBlock textBlock:
                    {
                        synthesisOutput = textBlock.Text?.ToString() ?? "";
                        break;
                    }
                case TextBox textBox:
                    {
                        string textBoxText = textBox.Text.Trim();

                        bool shouldAnnounceFieldsContentsAnyway = textBoxText.Length < 100;
                        string synthesisedTextBoxContent = textBoxText.Length > 100
                                ? $"(Text is long, press TAB to skip). {textBoxText}"
                                : textBoxText;

                        Label? labelEl = FindNearestLabel(textBox);

                        if (announceFieldContents)
                        {
                            synthesisOutput = synthesisedTextBoxContent;
                        } else
                        {
                            synthesisOutput = (
                                labelEl != null
                                    ? GetAccessibleAnnouncementValue(labelEl, isFirstFocus)
                                    : GetAutomationName(dependencyObject)
                            ) ?? "Text Field";
                        }

                        // If the field text is short enough, we can announce it anyway
                        if (!announceFieldContents && shouldAnnounceFieldsContentsAnyway)
                        {
                            synthesisOutput = $"{synthesisOutput}: {synthesisedTextBoxContent}";
                        }

                        break;
                    }
                case PasswordBox passwordBox:
                    {
                        Label? labelEl = FindNearestLabel(passwordBox);
                        if (labelEl != null)
                        {
                            return GetAccessibleAnnouncementValue(labelEl, isFirstFocus);
                        }
                        synthesisOutput = GetAutomationName(dependencyObject) ?? "Password Field";

                        break;
                    }
                case CheckBox checkBox:
                    {
                        Label? labelEl = FindNearestLabel(checkBox);

                        string name = GetAutomationName(dependencyObject) ?? "Check Box";

                        if (GetAutomationName(dependencyObject) == null && labelEl != null)
                        {
                            string labelText = string.Format(
                                labelEl!.ContentStringFormat ?? "",
                                labelEl!.Content?.ToString() ?? ""
                            );

                            if (labelText.Trim().Length > 0)
                            {
                                name = labelText;
                            }
                        }

                        string checkedState = checkBox.IsChecked == true
                            ? "Checked"
                            : checkBox.IsChecked == false
                                ? "Unchecked"
                                : "Indeterminate";

                        synthesisOutput = $"{checkedState}: {name}";

                        break;
                    }
                case Hyperlink hyperlink:
                    {
                        string hyperlinkInlines = GetTextFromInlines(hyperlink.Inlines).Trim();

                        synthesisOutput = hyperlinkInlines.Length > 0 
                            ? $"Link: {hyperlinkInlines}" 
                            : GetAutomationName(dependencyObject) ?? "";

                        break;
                    }
                case ComboBox comboBox:
                    {
                        Label? labelEl = FindNearestLabel(comboBox);

                        string name = GetAutomationName(dependencyObject) ?? "Dropdown";

                        if (GetAutomationName(dependencyObject) == null && labelEl != null)
                        {
                            string labelText = labelEl.Content?.ToString() ?? "";

                            if (labelText.Trim().Length > 0)
                            {
                                name = labelText;
                            }
                        }

                        string comboSelectedItem = comboBox.SelectedItem?.ToString() ?? "";
                        Type? comboSelectedItemType = comboBox.SelectedItem?.GetType();

                        // Losely check if the selected item is a KeyValuePair
                        if (
                            comboBox.SelectedItem != null &&
                            comboSelectedItemType != null &&
                            // Since KVP is a struct not a class, we need to get its generic to
                            // compare rather than compare it normally with .IsSubclassOf
                            comboSelectedItemType.IsGenericType &&
                            comboSelectedItemType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>)
                        ) {
                            var keyValuePair = (dynamic)comboBox.SelectedItem;

                            comboSelectedItem = keyValuePair
                                .Value?.ToString() ?? "";
                        }

                        string comboState = comboSelectedItem.Trim().Length > 0
                            ? comboSelectedItem
                            : "No selection";

                        synthesisOutput = isFirstFocus
                            ? $"{name}: {comboState}"
                            : comboState;

                        break;
                    }
                case DataGridCell cell:
                    {
                        string columnName = cell.Column.Header?.ToString() ?? "";
                        string cellValue = cell.Content is DependencyObject dpObject
                            ? GetAccessibleAnnouncementValue(dpObject, isFirstFocus)
                            : cell.Content.ToString() ?? "";

                        synthesisOutput = $"{columnName}: {cellValue}";

                        break;
                    }
                case Slider slider:
                    {
                        Label? labelEl = FindNearestLabel(slider);

                        string name = GetAutomationName(dependencyObject) ?? "Slider";

                        if (GetAutomationName(dependencyObject) == null && labelEl != null)
                        {
                            string labelText = labelEl.Content?.ToString() ?? "";

                            if (labelText.Trim().Length > 0)
                            {
                                name = labelText;
                            }
                        }

                        synthesisOutput = announceFieldContents
                            ? slider.Value.ToString()
                            : $"{name}: {slider.Value}";

                        break;
                    }
                default:
                    LogController.WriteLine(
                        $"Unhandled element type for {dependencyObject.ToString()}.",
                        LogCategory.AccessibilityController
                    );
                    break;
            }

            if (synthesisOutput.Trim().IsNullOrEmpty())
            {
                LogController.WriteLine(
                    $"No accessible name for {dependencyObject.ToString()}.",
                    LogCategory.AccessibilityController
                );
            }

            return synthesisOutput;
        }

        private void MaybeAnnounceElement(
            DependencyObject dependencyObject, 
            bool canCancelCurrent = true, 
            bool isFirstFocus = true,
            bool announceFieldContents = false
        ) {
            string synthesisOutput = GetAccessibleAnnouncementValue(
                dependencyObject, 
                isFirstFocus,
                announceFieldContents
            );

            MaybeAnnounceArbritary(
                synthesisOutput, 
                canCancelCurrent
            );
        }

        private void OnElementGotFocus(object sender, RoutedEventArgs e)
        {
            if (IsFrameworkElement(e.OriginalSource))
            {
                DependencyObject? dpObject = e.OriginalSource as DependencyObject;

                if (dpObject == null)
                {
                    LogController.WriteLine(
                        $"Failed to cast {e.OriginalSource.ToString()} to a DependencyObject.", 
                        LogCategory.AccessibilityController
                    );
                    return;
                }

                AddFocusListeners(dpObject);

                FocusTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
                FocusTimer.Tick += (_, _) =>
                {
                    if (FocusTimer != null)
                    {
                        FocusTimer.Stop();
                        FocusTimer = null;

                        MaybeAnnounceElement(dpObject);
                    }
                };
                FocusTimer.Start();
            }
        }

        private void OnElementLostFocus(object sender, RoutedEventArgs e)
        {
            if (FocusTimer != null && FocusTimer.IsEnabled)
            {
                FocusTimer.Stop();
                FocusTimer = null;
            }

            if (ClickTimer != null && ClickTimer.IsEnabled)
            {
                ClickTimer.Stop();
                ClickTimer = null;
            }

            if (TypingTimer != null)
            {
                TypingTimer.Stop();
                TypingTimer = null;
            }

            if (IsFrameworkElement(e.OriginalSource))
            {
                RemoveFocusListeners((DependencyObject)e.OriginalSource);

                DependencyObject? newFocusElement = (DependencyObject)Keyboard.FocusedElement;

                // Only cancel speech if we don't have a new focus element
                // OR: if the new focus element has an actual announcement value
                if (
                    newFocusElement == null || 
                    (newFocusElement != null && !GetAccessibleAnnouncementValue(newFocusElement).IsNullOrEmpty())
                ) {
                    CancelCurrentSpeech();
                }
            }
        }

        private void OnCheckBoxCheckedStateChange(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is CheckBox checkBox)
            {
                CancelAll();
                MaybeAnnounceElement(checkBox);
            }
        }

        private void OnButtonPreviewClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button button)
            {
                MaybeAnnounceElement(button, false);
            }
        }

        private void OnComboBoxSelectionChange(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is ComboBox comboBox)
            {
                CancelAll();
                MaybeAnnounceElement(comboBox, true, false);
            }
        }

        private void OnTextBoxTextChanged(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is TextBox textBox)
            {
                if (TypingTimer != null)
                {
                    TypingTimer.Stop();
                    TypingTimer = null;
                }

                TypingTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
                TypingTimer.Tick += (_, _) =>
                {
                    if (TypingTimer != null)
                    {
                        TypingTimer.Stop();
                        TypingTimer = null;

                        CancelAll();
                        MaybeAnnounceElement(textBox, true, false, true);
                    }
                };
                TypingTimer.Start();
            }
        }

        private void OnSliderValueChanged(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Slider slider)
            {
                CancelAll();
                MaybeAnnounceElement(slider, true, false, true);
            }
        }
    }
}
