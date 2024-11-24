using PMS.Controllers;
using PMS.Models;
using PMS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PMS.Context
{
    public class PatientsDataContext : DataContext<Patient>
    {
        public PatientsDataContext()
        {
            DataSource = Patient.GetAllPatients() ?? Array.Empty<Patient>();
            Columns = new()
            {
                // "Model Property" - "Column Display Name"
                { "ID", "ID" },
                { "Forenames", "Forename(s)" },
                { "Surname", "Surname" },
                { "Gender", "Gender" },
                { "DateOfBirth", "Date of Birth" },
                { "ReadableAddress", "Address" },
                { "FormattedNHSNumber", "NHS Number" },
                { "FormattedMobilePhoneNumber", "Mobile Number" },
                { "FormattedLandlinePhoneNumber", "Landline Number" },
                { "DateCreated", "Date created" },
            };
            Form = [
                new FormItemGroup(
                    new FormItemText {
                        Label = "ID",
                        DataBinding = nameof(Patient.ID),
                        IsReadOnly = true,
                        Required = true,
                        DefaultValue = Patient.GeneratePatientID
                    },
                    new FormItemText {
                        Label = "NHS Number",
                        DataBinding = nameof(Patient.NHSNumber),
                        Required = true,

                        Pattern = new Regex("^\\d{0,10}$")
                    }
                ),
                new FormItemGroup(
                    new FormItemText {
                        Label = "Forename(s)",
                        Required = true,
                        DataBinding = nameof(Patient.Forenames)
                    },
                    new FormItemText {
                        Label = "Surname",
                        Required = true,
                        DataBinding = nameof(Patient.Surname)
                    },
                    new FormItemCombo<PatientGender> {
                        Label = "Gender",
                        Required = true,
                        DataBinding = nameof(Patient.Gender),
                        Options = Patient.GetAllGenderOptions()
                    }
                ),
                new FormItemGroup(
                    new FormItemDatePicker {
                        Label = "Date of Birth",
                        Required = true,
                        DataBinding = nameof(Patient.DateOfBirth)
                    },
                    new FormItemDatePicker {
                        Label = "Date created",
                        Required = true,
                        DataBinding = nameof(Patient.DateCreated),
                        DatePickerFormat = DatePickerFormat.Long,
                        IsReadOnly = true,
                        DefaultValue = () => DateTime.Now
                    }
                ),
                new FormItemGroup(
                    new FormItemText {
                        Label = "Mobile phone number",
                        DataBinding = nameof(Patient.MobilePhoneNumber),
                        Pattern = new Regex("^(\\+)?\\d{0,13}$")
                    },
                    new FormItemText {
                        Label = "Landline phone number",
                        DataBinding = nameof(Patient.LandlinePhoneNumber),
                        Pattern = new Regex("^(\\+)?\\d{0,13}$")
                    }
                ),
            ];
        }
    }
}
