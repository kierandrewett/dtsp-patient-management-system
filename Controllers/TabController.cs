﻿using PMS.Components;
using PMS.Models;
using PMS.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PMS.Controllers
{
    internal class TabController<T>
    {
        private MainWindow _Win;

        private Panel _TabsStrip;
        private Panel _TabsContent;

        private TabContent<T>[] _Tabs;

        private TabContent<T> _SelectedTab;
        public TabContent<T> SelectedTab
        {
            get
            {
                return _SelectedTab;
            }
            set
            {
                // Update the internal value
                _SelectedTab = value;

                PaintTabItems();
                PaintTabContent();
            }
        }

        private WindowManager wm
        {
            get => (WindowManager)Application.Current.MainWindow;
        }

        private void OnTabSelect(object target, RoutedEventArgs e)
        {
            PMSTabItem TabItem = (PMSTabItem)target;

            if (TabItem is PMSTabItem)
            {
                this.SelectedTab = (TabContent<T>)TabItem.Value;
            }
        }

        private void PaintTabItems()
        {
            this._TabsStrip.Children.Clear();

            foreach (TabContent<T> Tab in _Tabs)
            {
                if (Tab != null && Tab.IsVisible == true)
                {
                    PMSTabItem TabItem = new PMSTabItem();

                    TabItem.Label = Tab.Name;
                    TabItem.Value = Tab;
                    TabItem.Click += OnTabSelect;
                    TabItem.IsSelected = Tab.Equals(this.SelectedTab);

                    this._TabsStrip.Children.Add(TabItem);
                }
            }

        }

        private async void PaintTabContent()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            this._Win.StatusBar.StatusText = $"Loading {SelectedTab.Name}...";

            this._TabsContent.Children.Clear();

            Random rnd = new Random();
            await Task.Delay(rnd.Next(100, 400));

            this._TabsContent.Children.Clear();
            this._TabsContent.Children.Add(SelectedTab.Content);

            Mouse.OverrideCursor = null;
            this._Win.StatusBar.StatusText = $"Ready";
        }

        private void Init()
        {
            PaintTabItems();

            this.SelectedTab = _Tabs.First();
        }

        public TabController(MainWindow Window, Panel TabsStrip, Panel TabsContent, TabContent<T>[] Tabs)
        {
            this._Win = Window;
            this._TabsStrip = TabsStrip;
            this._TabsContent = TabsContent;
            this._Tabs = Tabs;

            Init();
        }
    }
}