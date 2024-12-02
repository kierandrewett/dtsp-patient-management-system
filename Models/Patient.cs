using Microsoft.IdentityModel.Tokens;
using PMS.Util;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Controls;

namespace PMS.Models
{
    public enum PatientGender
    {
        Unspecified,
        Male,
        Female
    }

    public class PatientAddress : BaseModel
    {
        public override string ORM_TABLE => "tblAddress";
        public override string ORM_PRIMARY_KEY => "PatientID";

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

        protected string _Line1;
        public string Line1
        {
            get { return _Line1; }
            set
            {
                _Line1 = value;
                DidUpdateProperty("Line1");
            }
        }

        protected string _Line2;
        public string Line2
        {
            get { return _Line2; }
            set
            {
                _Line2 = value;
                DidUpdateProperty("Line2");
            }
        }

        protected string _Locality;
        public string Locality
        {
            get { return _Locality; }
            set
            {
                _Locality = value;
                DidUpdateProperty("Locality");
            }
        }

        protected string _County;
        public string County
        {
            get { return _County; }
            set
            {
                _County = value;
                DidUpdateProperty("County");
            }
        }

        protected string _Postcode;
        public string Postcode
        {
            get { return _Postcode; }
            set
            {
                _Postcode = value;
                DidUpdateProperty("Postcode");
            }
        }
    }

    public class Patient : Person<string>
    {
        public override string ORM_TABLE => "tblPatient";
        public override string ORM_PRIMARY_KEY => "ID";

        protected PatientGender _Gender;
        public PatientGender Gender
        {
            get { return _Gender; }
            set
            {
                _Gender = value;
                DidUpdateProperty("Gender");
            }
        }

        protected DateTime _DateOfBirth;
        public DateTime DateOfBirth
        {
            get { return _DateOfBirth; }
            set
            {
                _DateOfBirth = value;
                DidUpdateProperty("DateOfBirth");
            }
        }

        public string FormattedDateOfBirth
        {
            get => DateHelper.ConvertToString(DateOfBirth, false);
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

        protected string _NHSNumber;
        public string NHSNumber
        {
            get { return _NHSNumber; }
            set
            {
                _NHSNumber = value;
                DidUpdateProperty("NHSNumber");
            }
        }

        public string FormattedNHSNumber
        {
            get
            {
                try
                {
                    return NHSNumberFormatter.Format(NHSNumber);
                }
                catch (Exception _)
                {
                    return "";
                }
            }
        }

        protected string _Address;
        public string Address
        {
            get {
                if (_Address == null)
                {
                    _Address = ID;
                    return ID;
                }
                return _Address; 
            }
            set
            {
                _Address = value;
                DidUpdateProperty("Address");
            }
        }

        protected PatientAddress? _ComputedAddress;
        public PatientAddress? ComputedAddress
        {
            get => _ComputedAddress;
            set
            {
                _ComputedAddress = value;
            }
        }

        public string? ReadableAddress
        { 
            get {
                this._ComputedAddress ??= AppDatabase.QueryFirst<PatientAddress>(
                    "SELECT * FROM tblAddress WHERE PatientID=?",
                    [Address]
                );

                string formattedPostcode = ComputedAddress?.Postcode;

                try
                {
                    formattedPostcode = PostcodeFormatter.Format(ComputedAddress?.Postcode);
                } catch (Exception ex) {
                    Debug.WriteLine(ex);
                }

                return string.Join(", ", new[]
                {
                    ComputedAddress?.Line1,
                    ComputedAddress?.Line2,
                    ComputedAddress?.Locality,
                    ComputedAddress?.County,
                    formattedPostcode
                }.Where(part => !string.IsNullOrEmpty(part)));
            }
        }

        protected string _MobilePhoneNumber;
        public string MobilePhoneNumber
        {
            get { return _MobilePhoneNumber; }
            set
            {
                _MobilePhoneNumber = value;
                DidUpdateProperty("MobilePhoneNumber");
            }
        }

        public string FormattedMobilePhoneNumber
        {
            get
            {
                if (MobilePhoneNumber == null || MobilePhoneNumber.Trim().Length <= 0)
                {
                    return "";
                }

                try
                {
                    return PhoneNumberFormatter.Format(MobilePhoneNumber);
                } catch (Exception _)
                {
                    return "";
                }
            }
        }

        protected string _LandlinePhoneNumber;
        public string LandlinePhoneNumber
        {
            get { return _LandlinePhoneNumber; }
            set
            {
                _LandlinePhoneNumber = value;
                DidUpdateProperty("LandlinePhoneNumber");
            }
        }

        public string FormattedLandlinePhoneNumber
        {
            get
            {
                if (LandlinePhoneNumber == null || LandlinePhoneNumber.Trim().Length <= 0)
                {
                    return "";
                }

                try
                {
                    return PhoneNumberFormatter.Format(LandlinePhoneNumber);
                }
                catch (Exception _)
                {
                    return "";
                }
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

        public static Patient[]? GetAllPatients()
        {
            return AppDatabase.QueryAll<Patient>(
                "SELECT * FROM tblPatient",
                []
            );
        }

        // PRS-DATE_ADDED-SEQUENCE_NUMBER
        public static string GeneratePatientID()
        {
            DateTime dt = DateTime.Now;

            int currentSequence = AppDatabase.QueryCount<Patient>(
                "SELECT * FROM tblPatient WHERE DateCreated = Date()",
                []
            );

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

        public static Dictionary<PatientGender, string> GetAllGenderOptions()
        {
            Dictionary<PatientGender, string> genders = new() { };

            GenderObject[]? databaseGenders = AppDatabase.QueryAll<GenderObject>(
                "SELECT * FROM tblGender",
                []
            );

            if (databaseGenders != null)
            {
                foreach (GenderObject genderObj in databaseGenders)
                {
                    genders.Add(
                        (PatientGender)genderObj.ID, 
                        genderObj.Gender
                    );
                }
            }

            return genders;
        }

        public static Patient? GetPatientByID(string id)
        {
            return AppDatabase.QueryFirst<Patient>(
                "SELECT * FROM tblPatient WHERE ID=?",
                [id]
            );
        }
        public static Dictionary<string, string> GetAllPatientOptions()
        {
            Dictionary<string, string> patients = new() { };

            Patient[]? databasePatients = Patient.GetAllPatients();

            if (databasePatients != null)
            {
                foreach (Patient patientObj in databasePatients)
                {
                    patients.Add(
                        patientObj.ID,
                        patientObj.FormatFullName()
                    );
                }
            }

            return patients;
        }
    }
}
