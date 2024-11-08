using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PMS.Components
{
    public partial class PMSIcon : UserControl
    {
        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register("Fill", typeof(Brush), typeof(PMSIcon), new PropertyMetadata(Brushes.Black));

        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(Geometry), typeof(PMSIcon), new PropertyMetadata(null));

        public Geometry Source
        {
            get { return (Geometry)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public PMSIcon()
        {
            InitializeComponent();
        }
    }
}