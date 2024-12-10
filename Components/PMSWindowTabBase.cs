using Microsoft.IdentityModel.Tokens;
using PMS.Context;
using PMS.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PMS.Components
{
    public class PMSWindowTabBase : UserControl
    {
        private DataItem[]? RequestDataItems { get; set; }

        private PMSDataManager? Manager
        {
            get => (PMSDataManager)FindName("Manager");
        }

        public PMSWindowTabBase(DataItem[]? requestDataItems = null)
        {
            RequestDataItems = requestDataItems;

            MaybeLoadArguments();

            LayoutUpdated += Tab_LayoutUpdated;
        }

        private void Tab_LayoutUpdated(object? sender, EventArgs e)
        {
            if (Manager != null)
            {
                LayoutUpdated -= Tab_LayoutUpdated;
                MaybeLoadArguments();
            }
        }

        public void MaybeLoadArguments()
        {
            if (Manager != null && !RequestDataItems.IsNullOrEmpty())
            {
                Manager.Loaded += Manager_Loaded;
            }
        }

        private void Manager_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is PMSDataManager manager)
            {
                manager.Focus();

                int i = 0;
                // Really stupid hack but it works
                while (!RequestDataItems.IsNullOrEmpty() && manager.EditingDataItems != RequestDataItems)
                {
                    // If we exceed 1000 iterations for some reason (we shouldn't)
                    // but if we do, stop the loop as a fail safe for running out of memory.
                    if (i > 1000)
                    {
                        MessageBoxController.Show(
                            manager,
                            "Unable to load this tab with that data.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );

                        // Sometimes we can encounter an issue where it
                        // tries to edit the 0th entry after timing out
                        // as the dataItems array is empty, thus the first
                        // index will default out to 0.
                        // 
                        // To solve this, just blow up the tab and reload it
                        // again.
                        Window parent = Window.GetWindow(manager);

                        if (parent is MainWindow mainWindow)
                        {
                            mainWindow.TabsController.ReloadSelectedTab();
                        }

                        break;
                    }

                    manager.EnterEditMode(RequestDataItems);
                    i++;
                }
            }

        }
    }
}
