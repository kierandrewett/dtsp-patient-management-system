using PMS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace PMS.Models
{
    public class User : Person<int>
    {
        public override string ORM_TABLE => "tblUser";
        public override string ORM_PRIMARY_KEY => "ID";

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

        protected string _HashedPassword;
        public string HashedPassword
        {
            get { return _HashedPassword; }
            set
            {
                _HashedPassword = value;
                DidUpdateProperty("HashedPassword");
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

        protected bool _HasFirstLogin;
        public bool HasFirstLogin
        {
            get { return _HasFirstLogin; }
            set
            {
                _HasFirstLogin = value;
                DidUpdateProperty("HasFirstLogin");
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

        public static User? GetUserByID(int id)
        {
            return AppDatabase.QueryFirst<User>(
                "SELECT * FROM tblUser WHERE ID=?",
                [id.ToString()]
            );
        }

        public static int GenerateUserID()
        {
            int currentSequence = AppDatabase.QueryCount<User>(
                "SELECT * FROM tblUser",
                []
            );

            return currentSequence + 1;
        }

        public static string GeneratePassword()
        {
            return PasswordHelper.GeneratePassword();
        }
    }
}
