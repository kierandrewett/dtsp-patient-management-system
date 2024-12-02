using Microsoft.IdentityModel.Tokens;
using PMS.Util;
using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Controls;

namespace PMS.Models
{
    public enum AppointmentStatus
    {
        Unknown = 0,
        Scheduled = 1,
        Completed = 2,
        Cancelled = 3
    }

    public class AppointmentStatusObject : PropertyObservable
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

        protected string _Status;
        public string Status
        {
            get { return _Status; }
            set
            {
                _Status = value;
                DidUpdateProperty("Status");
            }
        }

        public static Dictionary<AppointmentStatus, string> GetAllAppointmentStatusOptions()
        {
            Dictionary<AppointmentStatus, string> statuses = new();

            AppointmentStatusObject[]? databaseStatuses = AppDatabase.QueryAll<AppointmentStatusObject>(
                "SELECT * FROM tblAppointmentStatus",
                []
            );

            if (databaseStatuses != null)
            {
                foreach (AppointmentStatusObject statusObj in databaseStatuses)
                {
                    statuses.Add(
                        (AppointmentStatus)statusObj.ID,
                        statusObj.Status
                    );
                }
            }

            return statuses;
        }
    }

    public enum AppointmentReason
    {
        Unknown = 0,
        Other = 1,
        CheckUp = 2,
        FollowUp = 3,
        Exam = 4,
        Procedure = 5,
        Surgery = 6
    }

    public class AppointmentReasonObject : PropertyObservable
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

        protected string _Reason;
        public string Reason
        {
            get { return _Reason; }
            set
            {
                _Reason = value;
                DidUpdateProperty("Reason");
            }
        }

        public static Dictionary<AppointmentReason, string> GetAllAppointmentReasonOptions()
        {
            Dictionary<AppointmentReason, string> reasons = new();

            AppointmentReasonObject[]? databaseReasons = AppDatabase.QueryAll<AppointmentReasonObject>(
                "SELECT * FROM tblAppointmentReason",
                []
            );

            if (databaseReasons != null)
            {
                foreach (AppointmentReasonObject reasonObj in databaseReasons)
                {
                    reasons.Add(
                        (AppointmentReason)reasonObj.ID,
                        reasonObj.Reason
                    );
                }
            }

            return reasons;
        }
    }

    public class Appointment : BaseModel
    {
        public override string ORM_TABLE => "tblAppointment";
        public override string ORM_PRIMARY_KEY => "ID";

        protected string _ID;
        public string ID
        {
            get { return _ID; }
            set
            {
                _ID = value;
                DidUpdateProperty("ID");
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
            get => DateHelper.ConvertToString(DateCreated, false);
        }

        protected DateTime _DateScheduledStart;
        public DateTime DateScheduledStart
        {
            get { return _DateScheduledStart; }
            set
            {
                _DateScheduledStart = value;
                DidUpdateProperty("DateScheduledStart");
            }
        }
        public string FormattedDateScheduledStart
        {
            get => DateHelper.ConvertToString(DateScheduledStart, true);
        }

        protected DateTime _DateScheduledEnd;
        public DateTime DateScheduledEnd
        {
            get { return _DateScheduledEnd; }
            set
            {
                _DateScheduledEnd = value;
                DidUpdateProperty("DateScheduledEnd");
            }
        }
        public string FormattedDateScheduledEnd
        {
            get => DateHelper.ConvertToString(DateScheduledEnd, true);
        }

        protected string _PatientID;
        public string PatientID
        {
            get { return _PatientID; }
            set
            {
                _PatientID = value;
                DidUpdateProperty("PatientID");
            }
        }

        private Patient? _Patient;
        public Patient? Patient
        {
            get
            {
                if (_Patient == null && !string.IsNullOrEmpty(PatientID))
                {
                    _Patient = Patient.GetPatientByID(PatientID);
                }
                return _Patient;
            }
        }
        public string PatientFullName
        {
            get
            {
                if (Patient != null)
                {
                    return Patient.FormatFullName();
                }

                return "";
            }
        }

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

        private User? _Doctor;
        public User? Doctor
        {
            get
            {
                if (_Doctor == null)
                {
                    User? user = User.GetUserByID(DoctorID);

                    if (user?.UserType == UserType.Doctor)
                    {
                        _Doctor = user;
                    }
                }

                return _Doctor;
            }
        }

        public string DoctorFullName
        {
            get
            {
                if (Doctor != null)
                {
                    return Doctor.FormatFullName();
                }

                return "";
            }
        }

        protected AppointmentStatus _Status;
        public AppointmentStatus Status
        {
            get { return _Status; }
            set
            {
                _Status = value;
                DidUpdateProperty("Status");
            }
        }

        protected string _Notes;
        public string Notes
        {
            get { return _Notes; }
            set
            {
                _Notes = value;
                DidUpdateProperty("Notes");
            }
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

        public string DepartmentWithID
        {
            get
            {
                return $"{(int)Department} - {Department}";
            }
        }

        protected AppointmentReason _Reason;
        public AppointmentReason Reason
        {
            get { return _Reason; }
            set
            {
                _Reason = value;
                DidUpdateProperty("Reason");
            }
        }

        public static Appointment[]? GetAllAppointments(User? authorisedUser = null)
        {
            if (authorisedUser != null && authorisedUser.UserType != UserType.Doctor)
            {
                return [];
            }

            return AppDatabase.QueryAll<Appointment>(
                "SELECT * FROM tblAppointment WHERE DoctorID=?",
                [authorisedUser?.ID.ToString() ?? ""]
            );
        }

        // PRS-DEPARTMENT-DATE_ADDED-SEQUENCE_NUMBER
        public static string GenerateAppointmentID(Department department)
        {
            DateTime dt = DateTime.Now;

            string departmentID = ((int)department).ToString();

            int currentSequence = AppDatabase.QueryCount<Patient>(
                "SELECT * FROM tblAppointment WHERE Department=? AND DateCreated = Date()",
                [departmentID]
            );

            string paddedDay = dt.Day.ToString().PadLeft(2, '0');
            string paddedMonth = dt.Month.ToString().PadLeft(2, '0');
            string year = (dt.Year % 100).ToString(); // modulo 100 of 2024 returns the last 2 digits = 24

            string formattedDate = $"{paddedDay}{paddedMonth}{year}";

            string nextSequence = (currentSequence + 1).ToString().PadLeft(2, '0');

            return string.Join("-", [
                AppConstants.AppName,
                departmentID,
                formattedDate,
                nextSequence
            ]).ToUpper();
        }
    }
}