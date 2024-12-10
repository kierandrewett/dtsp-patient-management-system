using PMS.Components;
using PMS.Context;

namespace PMS.Pages
{
    /// <summary>
    /// Interaction logic for WindowTabUsers.xaml
    /// </summary>
    public partial class WindowTabUsers : PMSWindowTabBase
    {
        public WindowTabUsers(DataItem[]? requestDataItems = null) : base(requestDataItems)
        {
            InitializeComponent();

            DataContext = new UsersDataContext();
        }
    }
}
