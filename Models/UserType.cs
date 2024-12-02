using PMS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS.Models
{
    public enum UserType
    {
        Admin,
        Doctor,
        Patient,
        Nurse
    }

    public class UserTypeObject : PropertyObservable
    {
        protected int _ID;
        public int ID
        {
            get { return _ID; }
            set
            {
                _ID = value;
                DidUpdateProperty("ID");
            }
        }

        protected string _Type;
        public string Type
        {
            get { return _Type; }
            set
            {
                _Type = value;
                DidUpdateProperty("Type");
            }
        }

        public static Dictionary<UserType, string> GetAllUserTypeOptions(UserType[]? excludeUserTypes = null)
        {
            Dictionary<UserType, string> types = new() { };

            UserTypeObject[]? databaseTypes = AppDatabase.QueryAll<UserTypeObject>(
                "SELECT * FROM tblUserType",
                []
            );

            if (databaseTypes != null)
            {
                foreach (UserTypeObject userTypeObj in databaseTypes)
                {
                    UserType userTypeID = (UserType)userTypeObj.ID;

                    if (excludeUserTypes != null && excludeUserTypes.Contains(userTypeID))
                    {
                        continue;
                    }

                    types.Add(
                        userTypeID,
                        userTypeObj.Type
                    );
                }
            }

            return types;
        }
    }
}
