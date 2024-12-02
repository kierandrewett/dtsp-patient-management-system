using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PMS.Controllers
{
    public class ChangesProtectionController
    {
        public static bool UnsavedChangesGuard()
        {
            MessageBoxResult result = MessageBox.Show(
                    "Any unsaved changes will be lost, do you want to leave edit mode?",
                    "Leave edit mode?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning,
                    MessageBoxResult.No
                );

            return result == MessageBoxResult.Yes;
        }

        public static bool SavedChangesGuard()
        {
            MessageBoxResult result = MessageBox.Show(
                    "Do you want to save your changes to this record?",
                    "Save changes?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning,
                    MessageBoxResult.No
                );

            return result == MessageBoxResult.Yes;
        }
    }
}
