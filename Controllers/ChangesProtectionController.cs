using PMS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PMS.Controllers
{
    public class ChangesProtectionController : PropertyObservable
    {
        public static bool UnsavedChangesGuard(DependencyObject dependencyObject)
        {
            MessageBoxResult result = MessageBoxController.Show(
                dependencyObject,
                "Any unsaved changes will be lost, do you want to leave edit mode?",
                "Leave edit mode?",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No
            );

            return result == MessageBoxResult.Yes;
        }

        public static bool SavedChangesGuard(DependencyObject dependencyObject)
        {
            MessageBoxResult result = MessageBoxController.Show(
                dependencyObject,
                "Do you want to save your changes to this record?",
                "Save changes?",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No
            );

            return result == MessageBoxResult.Yes;
        }


        private bool _UnsavedChangesLock = false;

        public bool UnsavedChangesLock
        {
            get => _UnsavedChangesLock;
            set
            {
                _UnsavedChangesLock = value;

                DidUpdateProperty("UnsavedChangesLock");
            }
        }
        public ChangesProtectionController()
        {

        }
    }
}
