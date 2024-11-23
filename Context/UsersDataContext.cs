using PMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS.Context
{
    public class UsersDataContext : DataContext<User>
    {
        public UsersDataContext()
        {
            DataSource = User.GetAllUsers() ?? Array.Empty<User>();
            Columns = new()
            {
                // "Model Property" - "Column Display Name"
                { "ID", "ID" },
                { "Username", "Username" },
                { "Forenames", "Forename(s)" },
                { "Surname", "Surname" },
                { "UserType", "Type" },
                { "IsDisabled", "Account disabled" },
            };

        }
    }
}
