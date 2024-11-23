using PMS.Controllers;
using PMS.Pages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PMS.Components
{
    /// <summary>
    /// Interaction logic for PMSContentHeader.xaml
    /// </summary>
    public partial class PMSContentHeader : UserControl
    {
        public PMSContentHeader()
        {
            InitializeComponent();

            DataContext = this;
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(PMSContentHeader));
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(Geometry), typeof(PMSContentHeader));
        public Geometry Icon
        {
            get { return (Geometry)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty HasSearchProperty = DependencyProperty.Register("HasSearch", typeof(bool), typeof(PMSContentHeader));
        public bool HasSearch
        {
            get { return (bool)GetValue(HasSearchProperty); }
            set { SetValue(HasSearchProperty, value); }
        }

        public string SearchBoxValue { get => SearchBox.Text.Trim(); }

        public static readonly RoutedEvent SearchBoxUpdatedEvent = EventManager.RegisterRoutedEvent("SearchBoxUpdated", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PMSContentHeader));

        public event RoutedEventHandler SearchBoxUpdated
        {
            add { AddHandler(SearchBoxUpdatedEvent, value); }
            remove { RemoveHandler(SearchBoxUpdatedEvent, value); }
        }

        private void OnSearchBoxUpdated(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(SearchBoxUpdatedEvent));
        }
    }
}
