﻿using System;
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
                    "Changes you have made to data will NOT be saved.",
                    "Leave edit mode?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning,
                    MessageBoxResult.No
                );

            return result == MessageBoxResult.Yes;
        }
    }
}
