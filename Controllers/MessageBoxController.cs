using PMS.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PMS.Controllers
{
    public class MessageBoxController
    {
        private PMSWindow Win { get; set; }
        private AccessibilityController AccessibilityController
        {
            get => Win.AccessibilityController;
        }

        public MessageBoxController(DependencyObject dependencyObject)
        {
            if (dependencyObject is PMSWindow window)
            {
                Win = window;
            } else if (Window.GetWindow(dependencyObject) is PMSWindow inheritedWindow)
            {
                Win = inheritedWindow;
            } else
            {
                throw new Exception(
                    $"Tried using MessageBoxController on non-PMS window, given {dependencyObject.ToString()}."
                );
            }
        }



        public static MessageBoxResult Show(DependencyObject dependencyObject,
                                        string message, string title = "Message",
                                        MessageBoxButton button = MessageBoxButton.OK,
                                        MessageBoxImage icon = MessageBoxImage.Asterisk,
                                        MessageBoxResult defaultResult = MessageBoxResult.OK,
                                        MessageBoxOptions options = MessageBoxOptions.None)
        {
            return new MessageBoxController(dependencyObject).Show(
                message, 
                title,
                button, 
                icon, 
                defaultResult, 
                options
            );
        }

        public MessageBoxResult Show(string message, string title = "Message",
                                        MessageBoxButton button = MessageBoxButton.OK,
                                        MessageBoxImage icon = MessageBoxImage.Asterisk,
                                        MessageBoxResult defaultResult = MessageBoxResult.OK,
                                        MessageBoxOptions options = MessageBoxOptions.None)
        {
            AccessibilityController.MaybeAnnounceArbritary($"{title}: {message}", false);

            MessageBoxResult result = MessageBox.Show(
                message,
                title,
                button,
                icon,
                defaultResult,
                options
            );

            AccessibilityController.CancelAll();

            switch (result)
            {
                case MessageBoxResult.OK:
                    AccessibilityController.MaybeAnnounceArbritary("Pressed: OK");
                    break;
                case MessageBoxResult.Yes:
                    AccessibilityController.MaybeAnnounceArbritary("Pressed: Yes");
                    break;
                case MessageBoxResult.No:
                    AccessibilityController.MaybeAnnounceArbritary("Pressed: No");
                    break;
                case MessageBoxResult.Cancel:
                    AccessibilityController.MaybeAnnounceArbritary("Pressed: Cancel");
                    break;
                default:
                    AccessibilityController.MaybeAnnounceArbritary("Dismissed");
                    break;
            }

            return result;
        }
    }
}
