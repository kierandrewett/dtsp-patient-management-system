using PMS.Models;

namespace PMS.Context
{
    public class DoctorsDataContext : UsersDataContext
    {
        public DoctorsDataContext()
        {
            DataSource = User.GetUsersByType(UserType.Doctor) ?? Array.Empty<User>();
        }
    }
}
