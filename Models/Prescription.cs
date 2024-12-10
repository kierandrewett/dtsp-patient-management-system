using Microsoft.IdentityModel.Tokens;
using PMS.Util;
using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Controls;

namespace PMS.Models
{
    public enum MedicationAdministration
    {
        Oral,
        Topical,
        Injection,
        Inhalation,
        Sublingual,
        Rectal,
        Other
    }

    public class MedicationAdministrationObject : PropertyObservable
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

        protected string _Administration;
        public string Administration
        {
            get { return _Administration; }
            set
            {
                _Administration = value;
                DidUpdateProperty("Administration");
            }
        }

        public static Dictionary<MedicationAdministration, string> GetAllMedicationAdministrationOptions()
        {
            Dictionary<MedicationAdministration, string> administration = new();

            MedicationAdministrationObject[]? databaseAdministration = AppDatabase.QueryAll<MedicationAdministrationObject>(
                "SELECT * FROM tblPrescriptionAdministration",
                []
            );

            if (databaseAdministration != null)
            {
                foreach (MedicationAdministrationObject administrationObj in databaseAdministration)
                {
                    administration.Add(
                        (MedicationAdministration)administrationObj.ID,
                        administrationObj.Administration
                    );
                }
            }

            return administration;
        }
    }

    public enum MedicationForm
    {
        Tablet,
        Capsule,
        Liquid,
        Cream,
        Ointment,
        Spray,
        Patch,
        Other
    }

    public class MedicationFormObject : PropertyObservable
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

        protected string _Form;
        public string Form
        {
            get { return _Form; }
            set
            {
                _Form = value;
                DidUpdateProperty("Form");
            }
        }

        public static Dictionary<MedicationForm, string> GetAllMedicationFormOptions()
        {
            Dictionary<MedicationForm, string> form = new();

            MedicationFormObject[]? databaseForm = AppDatabase.QueryAll<MedicationFormObject>(
                "SELECT * FROM tblPrescriptionForm",
                []
            );

            if (databaseForm != null)
            {
                foreach (MedicationFormObject formObj in databaseForm)
                {
                    form.Add(
                        (MedicationForm)formObj.ID,
                        formObj.Form
                    );
                }
            }

            return form;
        }
    }

    public class Prescription : BaseModel
    {
        public override string ORM_TABLE => "tblPrescription";
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

        protected DateTime _DateExpire;
        public DateTime DateExpire
        {
            get { return _DateExpire; }
            set
            {
                _DateExpire = value;
                DidUpdateProperty("DateExpire");
            }
        }
        public string FormattedDateExpire
        {
            get => DateHelper.ConvertToString(DateExpire, false);
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

        protected int _PrescriberID;
        public int PrescriberID
        {
            get { return _PrescriberID; }
            set
            {
                _PrescriberID = value;
                DidUpdateProperty("PrescriberID");
            }
        }

        private User? _Prescriber;
        public User? Prescriber
        {
            get
            {
                if (_Prescriber == null)
                {
                    _Prescriber ??= User.GetUserByID(PrescriberID);
                    return _Prescriber;
                }

                return _Prescriber;
            }
        }

        public string PrescriberFullName
        {
            get
            {
                if (Prescriber != null)
                {
                    return Prescriber.FormatFullName();
                }

                return "";
            }
        }

        protected string _MedicationBrand;
        public string MedicationBrand
        {
            get { return _MedicationBrand; }
            set
            {
                _MedicationBrand = value;
                DidUpdateProperty("MedicationBrand");
            }
        }

        protected string _MedicationName;
        public string MedicationName
        {
            get { return _MedicationName; }
            set
            {
                _MedicationName = value;
                DidUpdateProperty("MedicationName");
            }
        }

        protected string _MedicationDosage;
        public string MedicationDosage
        {
            get { return _MedicationDosage; }
            set
            {
                _MedicationDosage = value;
                DidUpdateProperty("MedicationDosage");
            }
        }

        protected MedicationForm _MedicationForm;
        public MedicationForm MedicationForm
        {
            get { return _MedicationForm; }
            set
            {
                _MedicationForm = value;
                DidUpdateProperty("MedicationForm");
            }
        }

        protected int _MedicationQuantity;
        public int MedicationQuantity
        {
            get { return _MedicationQuantity; }
            set
            {
                _MedicationQuantity = value;
                DidUpdateProperty("MedicationQuantity");
            }
        }

        protected MedicationAdministration _MedicationAdministration;
        public MedicationAdministration MedicationAdministration
        {
            get { return _MedicationAdministration; }
            set
            {
                _MedicationAdministration = value;
                DidUpdateProperty("MedicationAdministration");
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
            get
            {
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


        public static int GeneratePrescriptionID()
        {
            int currentSequence = AppDatabase.QueryFirst<QueryCount>(
                "SELECT MAX(ID) AS RecordCount FROM tblPrescription",
                []
            )?.RecordCount ?? -10;

            return currentSequence + 1;
        }

        public static Prescription[]? GetAllPrescriptionsByPatientID(string patientID)
        {
            return AppDatabase.QueryAll<Prescription>(
                "SELECT * FROM tblPrescription WHERE PatientID=?",
                [patientID]
            );
        }
    }
}