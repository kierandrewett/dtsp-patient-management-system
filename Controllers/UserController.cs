using PMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS.Controllers
{
    class UserController
    {
        public static User[]? GetAllUsers()
        {
            return AppDatabase.QueryAll<User>(
                "SELECT * FROM tblUser",
                []
            );
        }

        public static User? GetUserByUsername(string username)
        {
            return AppDatabase.QueryFirst<User>(
                "SELECT * FROM tblUser WHERE Username=?",
                [username]
            );
        }

        public static User[]? GetUsersByType(UserType userType)
        {
            return AppDatabase.QueryAll<User>(
                "SELECT * FROM tblUser WHERE UserType=?",
                [((int)userType).ToString()]
            );
        }
    }
}
