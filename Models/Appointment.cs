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
        CancelledByPatient = 2,
        CancelledByDoctor = 3,
        Completed = 4,
        InProgress = 5
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
        Other = 0,
        CheckUp = 1,
        FollowUp = 2,
        Exam = 3,
        Procedure = 4,
        Surgery = 5
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
            get {
                string dtDate = DateScheduledStart.ToShortDateString();

                DateTime dt = DateTime.Parse($"{dtDate} {TimeScheduledStart ?? "12:00"}");

                return DateHelper.ConvertToString(dt, true);
            }
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
            get
            {
                string dtDate = DateScheduledEnd.ToShortDateString();

                DateTime dt = DateTime.Parse($"{dtDate} {TimeScheduledEnd ?? "12:00"}");

                return DateHelper.ConvertToString(dt, true);
            }
        }

        protected string _TimeScheduledStart;
        public string TimeScheduledStart
        {
            get { return _TimeScheduledStart; }
            set
            {
                _TimeScheduledStart = value;
                DidUpdateProperty("TimeScheduledStart");
            }
        }

        protected string _TimeScheduledEnd;
        public string TimeScheduledEnd
        {
            get { return _TimeScheduledEnd; }
            set
            {
                _TimeScheduledEnd = value;
                DidUpdateProperty("TimeScheduledEnd");
            }
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
                if (_Patient == null)
                {
                    Patient? patient = Patient.GetPatientByID(PatientID);

                    _Patient = patient;
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

        public AppointmentStatus ComputedStatus
        {
            get
            {
                if (
                    Status != AppointmentStatus.Scheduled &&
                    Status != AppointmentStatus.InProgress &&
                    Status != AppointmentStatus.Completed
                )
                {
                    return Status;
                }

                if (DateScheduledStart < DateTime.Now && DateScheduledEnd > DateTime.Now)
                {
                    return AppointmentStatus.InProgress;
                }
                else if (DateScheduledStart > DateTime.Now)
                {
                    return AppointmentStatus.Scheduled;
                }
                else if (DateScheduledEnd < DateTime.Now)
                {
                    return AppointmentStatus.Completed;
                }

                return Status;
            }
        }

        protected int _Note;
        public int Note
        {
            get { return _Note; }
            set
            {
                _Note = value;
                DidUpdateProperty("Note");
            }
        }

        protected Note? _ComputedNote;
        public Note? ComputedNote
        {
            get {
                if (_ComputedNote == null)
                {
                    _ComputedNote ??= AppDatabase.QueryFirst<Note>("SELECT * FROM tblNote WHERE ID=?", [Note.ToString()]);
                    return _ComputedNote;
                }

                return _ComputedNote;
            }
            set
            {
                _ComputedNote = value;
                DidUpdateProperty("ComputedNote");
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
                "SELECT * FROM tblAppointment WHERE DoctorID=? AND PatientID IN (SELECT ID FROM tblPatient) AND DateScheduledStart > Date() OR (DateScheduledStart <= Date() AND DateScheduledEnd > Date())",
                [authorisedUser?.ID.ToString() ?? ""]
            );
        }

        public static Appointment[]? GetAllAppointmentsForDoctorID(int doctorID)
        {
            return AppDatabase.QueryAll<Appointment>(
                "SELECT * FROM tblAppointment WHERE DoctorID=?",
                [doctorID.ToString()]
            );
        }

        // PRS-DATE_ADDED-SEQUENCE_NUMBER
        public static string GenerateAppointmentID()
        {
            DateTime dt = DateTime.Now;

            int currentSequence = AppDatabase.QueryFirst<QueryCount>(
                "SELECT COUNT(*) AS RecordCount FROM tblAppointment WHERE DateCreated = Date()",
                []
            )?.RecordCount ?? -10;

            string paddedDay = dt.Day.ToString().PadLeft(2, '0');
            string paddedMonth = dt.Month.ToString().PadLeft(2, '0');
            string year = (dt.Year % 100).ToString(); // modulo 100 of 2024 returns the last 2 digits = 24

            string formattedDate = $"{paddedDay}{paddedMonth}{year}";

            string nextSequence = (currentSequence + 1).ToString().PadLeft(2, '0');

            return string.Join("-", [
                AppConstants.AppName,
                formattedDate,
                nextSequence
            ]).ToUpper();
        }
    }
}