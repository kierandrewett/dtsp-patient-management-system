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

        public static Doctor? GetByDoctorID(int doctorID)
        {
            return AppDatabase.QueryFirst<Doctor>(
                "SELECT * FROM tblDoctor WHERE DoctorID=?",
                [doctorID.ToString()]
            );
        }

        public static Doctor? GetByUserID(int userID)
        {
            return AppDatabase.QueryFirst<Doctor>(
                "SELECT * FROM tblDoctor WHERE UserID=?",
                [userID.ToString()]
            );
        }
    }
}
