using PMS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS.Models
{
    public class Doctor : PropertyObservable
    {
        protected int _DoctorID;
        public int DoctorID
        {
            get { return _DoctorID; }
            set
            {
                _DoctorID = value;
                DidUpdateProperty("DoctorID");
            }
        }

        protected int _UserID;
        public int UserID
        {
            get { return _UserID; }
            set
            {
                _UserID = value;
                DidUpdateProperty("UserID");
            }
        }

        public static User? GetByUserID(int userID)
        {
            return AppDatabase.QueryFirst<User>(
                "SELECT * FROM tblUser WHERE UserID=? AND UserType=?",
                [userID.ToString(), Convert.ToInt32(UserType.Doctor).ToString()]
            );
        }

        public static User[]? GetAllDoctors()
        {
            return AppDatabase.QueryAll<User>(
                "SELECT * FROM tblUser WHERE UserType=?",
                [Convert.ToInt32(UserType.Doctor).ToString()]
            );
        }

        public static Dictionary<int, string> GetAllDoctorOptions()
        {
            User[]? doctors = Doctor.GetAllDoctors();

            Dictionary<int, string> options = [];

            if (doctors != null)
            {
                foreach (User doctor in doctors)
                {
                    options.Add(doctor.ID, doctor.FormatFullName());
                }
            }

            return options;
        }
    }
}
