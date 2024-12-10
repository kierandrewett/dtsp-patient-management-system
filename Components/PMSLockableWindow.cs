using Microsoft.IdentityModel.Tokens;
using PMS.Controllers;
using PMS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace PMS.Components
{
    public class PMSLockableWindow : PMSWindow, INotifyPropertyChanged
    {

        public ChangesProtectionController ChangesProtectionController = new();

        public static readonly DependencyProperty AuxiliaryTitleProperty = DependencyProperty.Register("AuxiliaryTitle", typeof(string), typeof(PMSLockableWindow), new PropertyMetadata(null));
        public string? AuxiliaryTitle
        {
            get { return (string)GetValue(AuxiliaryTitleProperty); }
            set
            {
                SetValue(AuxiliaryTitleProperty, value);
                DidUpdateProperty("AuxiliaryTitle");
            }
        }

        public string? ComputedTitle
        {
            get {
                string title = AuxiliaryTitle ?? AppConstants.AppComputedTitle;

                if (this.ChangesProtectionController.UnsavedChangesLock)
                {
                    title += "- (Unsaved changes)";
                }

                return title;
            }
            set
            {
                AuxiliaryTitle = value;
                DidUpdateProperty("ComputedTitle");
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

        public PMSLockableWindow() : base()
        {
            DataContext = this;

            Closing += LockableWindow_Closing;
        }
        private void LockableWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // Prevent the window from closing if we have unsaved changes
            if (ChangesProtectionController.UnsavedChangesLock)
            {
                e.Cancel = !ChangesProtectionController.UnsavedChangesGuard(this);
                return;
            }
        }

        public virtual void InvalidateDataContext()
        {
            throw new Exception("no impl");
        }
    }
}
