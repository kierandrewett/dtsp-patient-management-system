using Microsoft.IdentityModel.Tokens;
using PMS.Util;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Windows.Controls;

namespace PMS.Models
{
    public enum PatientGender
    {
        Unspecified,
        Male,
        Female
    }

    public class PatientAddress : PropertyObservable
    {
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
            get { return _Address; }
            set
            {
                _Address = value;
                DidUpdateProperty("Address");
            }
        }

        protected PatientAddress? _ComputedAddress;
        public PatientAddress? ComputedAddress
        {
            get
            {
                if (_ComputedAddress == null)
                {
                    _ComputedAddress = AppDatabase.QueryFirst<PatientAddress>(
                        "SELECT * FROM tblAddress WHERE PatientID=?",
                        [Address]
                    );
                }

                return _ComputedAddress;
            }
        }

        public string? ReadableAddress
        { 
            get {
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

        public Doctor? Doctor => Doctor.GetByDoctorID(DoctorID);
        public static Patient[]? GetAllPatients()
        {
            return AppDatabase.QueryAll<Patient>(
                "SELECT * FROM tblPatient",
                []
            );
        }
    }
}
