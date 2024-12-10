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

        protected DateTime _DateCreated;
        public DateTime DateCreated
        {
            get { return _DateCreated; }
            set
            {
                _DateCreated = value;
                DidUpdateProperty("DateCreated");
            }
        }

        public string FormattedDateCreated
        {
            get => DateHelper.ConvertToString(DateCreated, true);
        }

        protected Department _Department;
        public Department Department
        {
            get { return _Department; }
            set
            {
                _Department = value;
                DidUpdateProperty("Department");
            }
        }

        protected DepartmentObject? _DepartmentObject;
        public DepartmentObject? DepartmentObject
        {
            get
            {
                _DepartmentObject ??= AppDatabase.QueryFirst<DepartmentObject>(
                        "SELECT * FROM tblDepartment WHERE ID=?",
                        [((int)Department).ToString()]
                    );

                return _DepartmentObject;
            }
        }

        public string DepartmentWithID
        {
            get => $"{(int)Department} - {DepartmentObject?.Department ?? "Unknown"}";
        }

        public static User[]? GetAllUsers()
        {
            return AppDatabase.QueryAll<User>(
                "SELECT * FROM tblUser",
                []
            );
        }

        protected string _SecurityQuestion1;
        public string SecurityQuestion1
        {
            get
            {
                if (_SecurityQuestion1 == null)
                {
                    _SecurityQuestion1 = $"{ID}-1";
                    return _SecurityQuestion1;
                }
                return _SecurityQuestion1;
            }
            set
            {
                _SecurityQuestion1 = value;
                DidUpdateProperty("SecurityQuestion1");
            }
        }

        protected string _SecurityQuestion2;
        public string SecurityQuestion2
        {
            get
            {
                if (_SecurityQuestion2 == null)
                {
                    _SecurityQuestion2 = $"{ID}-2";
                    return _SecurityQuestion2;
                }
                return _SecurityQuestion2;
            }
            set
            {
                _SecurityQuestion2 = value;
                DidUpdateProperty("SecurityQuestion2");
            }
        }

        protected SecurityQuestionAnswer? _ComputedSecurityQuestion1;
        public SecurityQuestionAnswer? ComputedSecurityQuestion1
        {
            get {
                if (_ComputedSecurityQuestion1 == null)
                {
                    this._ComputedSecurityQuestion1 ??= AppDatabase.QueryFirst<SecurityQuestionAnswer>(
                       "SELECT * FROM tblUserSecurityQuestion WHERE QuestionAnswerID=?",
                       [SecurityQuestion1]
                    );

                    return _ComputedSecurityQuestion1;
                }

                return _ComputedSecurityQuestion1;
            }
            set
            {
                _ComputedSecurityQuestion1 = value;
            }
        }

        protected SecurityQuestionAnswer? _ComputedSecurityQuestion2;
        public SecurityQuestionAnswer? ComputedSecurityQuestion2
        {
            get
            {
                if (_ComputedSecurityQuestion2 == null)
                {
                    this._ComputedSecurityQuestion2 ??= AppDatabase.QueryFirst<SecurityQuestionAnswer>(
                       "SELECT * FROM tblUserSecurityQuestion WHERE QuestionAnswerID=?",
                       [SecurityQuestion2]
                    );

                    return _ComputedSecurityQuestion2;
                }

                return _ComputedSecurityQuestion2;
            }
            set
            {
                _ComputedSecurityQuestion2 = value;
            }
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
            if (AppDatabase.QueryFirst<QueryCount>(
                "SELECT MAX(ID) AS RecordCount FROM tblUser",
                []
            ) is QueryCount record)
            {
                return record.RecordCount + 1;
            } else
            {
                throw new Exception("Failed to get next sequence count");
            }
        }

        public static string GeneratePassword()
        {
            return PasswordHelper.GeneratePassword();
        }
    }
}
