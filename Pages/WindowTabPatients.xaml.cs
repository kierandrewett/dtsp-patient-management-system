using PMS.Components;
using PMS.Context;

namespace PMS.Pages
{
    /// <summary>
    /// Interaction logic for WindowTabPatients.xaml
    /// </summary>
    public partial class WindowTabPatients : PMSWindowTabBase
    {
        public WindowTabPatients(DataItem[]? requestDataItems = null) : base(requestDataItems)
        {
            InitializeComponent();

            DataContext = new PatientsDataContext();
        }
    }
}
