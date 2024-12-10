using PMS.Components;
using PMS.Context;

namespace PMS.Pages
{
    /// <summary>
    /// Interaction logic for WindowTabUsers.xaml
    /// </summary>
    public partial class WindowTabScheduling : PMSWindowTabBase
    {
        public WindowTabScheduling(DataItem[]? requestDataItems = null) : base(requestDataItems)
        {
            InitializeComponent();

            DataContext = new SchedulingDataContext();
        }
    }
}
