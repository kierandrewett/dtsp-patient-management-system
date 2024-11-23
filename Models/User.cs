using PMS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace PMS.Models
{
    public enum UserType
    {
        Admin,
        Doctor,
        Patient,
        Nurse
    }

    public class User : Person<int>
    {
        protected string _Username;
        public string Username
        {
            get { return _Username; }
            set
            {
                _Username = value;
                DidUpdateProperty("Username");
            }
        }

        protected string _Password;
        public string Password
        {
            get { return _Password; }
            set
            {
                _Password = value;
                DidUpdateProperty("Password");
            }
        }

        protected UserType _UserType;
        public UserType UserType
        {
            get { return _UserType; }
            set
            {
                _UserType = value;
                DidUpdateProperty("UserType");
            }
        }

        protected bool _IsDisabled;
        public bool IsDisabled
        {
            get { return _IsDisabled; }
            set
            {
                _IsDisabled = value;
                DidUpdateProperty("IsDisabled");
            }
        }

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
