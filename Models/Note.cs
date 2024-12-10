using PMS.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PMS.Models
{
    public class Note : BaseModel
    {
        public override string ORM_TABLE => "tblNote";
        public override string ORM_PRIMARY_KEY => "ID";

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

        public Patient? Patient
        {
            get => Patient.GetPatientByID(PatientID);
        }

        public string PatientFullName
        {
            get => Patient?.FormatFullName() ?? "";
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

        public User? Assignee
        {
            get => User.GetUserByID(UserID);
        }

        public string AssigneeFullName
        {
            get => Assignee?.FormatFullName() ?? "";
        }

        protected string _NoteBody;
        public string NoteBody
        {
            get { return _NoteBody; }
            set
            {
                _NoteBody = value;
                DidUpdateProperty("NoteBody");
            }
        }

        public string NoteBodySummary
        {
            get {
                string trimmed = Regex.Replace(NoteBody.Trim(), "\\s", " ");

                if (trimmed.Length > 50)
                {
                    return trimmed.Substring(0, 50) + "...";
                } else
                {
                    return trimmed;
                }
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

        public static Note[]? GetAllNotesByPatientID(string patientID)
        {
            return AppDatabase.QueryAll<Note>(
                "SELECT * FROM tblNote WHERE PatientID=?",
                [patientID]
            );
        }

        public static int GenerateNoteID()
        {
            int currentSequence = AppDatabase.QueryFirst<QueryCount>(
                "SELECT MAX(ID) AS RecordCount FROM tblNote",
                []
            )?.RecordCount ?? -10;

            return currentSequence + 1;
        }
    }
}