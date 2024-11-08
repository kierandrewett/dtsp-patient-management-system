using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PMS.Components
{
    /// <summary>
    /// Interaction logic for PMSHeaderClock.xaml
    /// </summary>
    public partial class PMSHeaderClock : UserControl
    {
        DispatcherTimer Timer = new DispatcherTimer();

        public string DateTimeTooltip
        {
            get => $"{Time.Text} {Date.Text}";
        }

        public PMSHeaderClock()
        {
            InitializeComponent();

            Tick(null, null);

            Timer.Tick += new EventHandler(Tick);
            Timer.Interval = new TimeSpan(0, 0, 1);
            Timer.Start();

            DataContext = this;
        }

        private void Tick(object? sender, EventArgs? e)
        {
            DateTime date = DateTime.Now;

            string hour = date.Hour.ToString().PadLeft(2, '0');
            string minute = date.Minute.ToString().PadLeft(2, '0');
            string day = date.Day.ToString().PadLeft(2, '0');
            string month = date.Month.ToString().PadLeft(2, '0');
            string year = date.Year.ToString();

            Time.Text = $"{hour}:{minute}";
            Date.Text = $"{day}/{month}/{year}";
        }
    }
}
