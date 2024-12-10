using PMS.Components;
using PMS.Context;
using PMS.Controllers;
using PMS.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PMS.Dialogs
{


    /// <summary>
    /// Interaction logic for PMSSettingsWindow.xaml
    /// </summary>
    public partial class PMSSettingsWindow : PMSWindow, INotifyPropertyChanged
    {
        private SettingsController SettingsController;

        private SettingsData _Settings;
        public SettingsData Settings
        {
            get => _Settings;
            set
            {
                _Settings = value;
                SettingsController.WriteSettings(_Settings);
                DidUpdateProperty("Settings");
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

        public PMSSettingsWindow() : base()
        {
            InitializeComponent();

            DataContext = this;

            SettingsController = new SettingsController();
            _Settings = SettingsController.ReadSettings();
        }

        private void OnSaveButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsController.WriteSettings(Settings);

            MessageBoxController.Show(
                this, 
                "Saved settings successfully.", 
                "Saved", 
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
    }
}
