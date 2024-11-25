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
            Model = typeof(Patient);
            DataSource = Patient.GetAllPatients() ?? Array.Empty<Patient>();
            Columns = new()
            {
                // "Model Property" - "Column Display Name"
                { "ID", "ID" },
                { "Title", "Title" },
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
                new FormItemGroup([
                    new FormItemText {
                        Label = "ID",
                        DataBinding = nameof(Patient.ID),
                        IsReadOnly = true,
                        Required = true,
                        DefaultValue = (_) => Patient.GeneratePatientID()
                    },
                    new FormItemText {
                        Label = "NHS Number",
                        DataBinding = nameof(Patient.NHSNumber),
                        Required = true,
                        Pattern = new Regex("^\\d{0,10}$")
                    }
                ]),
                new FormItemGroup([
                    new FormItemCombo<Title> {
                        Label = "Title",
                        Required = true,
                        DataBinding = nameof(Patient.Title),
                        Options = TitleObject.GetAllTitleOptions(),
                        MaxWidth = 100,
                    },
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
                ]),
                new FormItemGroup([
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
                        DefaultValue = (_) => DateTime.Now
                    }
                ]),
                new FormItemGroup([
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
                ]),
                /* Begin tblPatient:Address <-> tblAddress:PatientID relationship */
                new FormItemText {
                    Label = "tblPatient:Address",
                    DataBinding = "Address",
                    IsReadOnly = true,
                    Hidden = true,
                    DefaultValue = (_) => Patient.GeneratePatientID()
                },
                new FormItemText {
                    Label = "tblAddress:PatientID",
                    DataBinding = "ComputedAddress.PatientID",
                    IsReadOnly = true,
                    Hidden = true,
                    DefaultValue = (_) => Patient.GeneratePatientID()
                },
                /* End relationship */
                new FormItemGroup([
                    new FormItemText {
                        Label = "Line 1",
                        DataBinding = "ComputedAddress.Line1",
                    },
                    new FormItemText {
                        Label = "Line 2",
                        DataBinding = "ComputedAddress.Line2",
                    },
                    new FormItemText {
                        Label = "Town / City / Locality",
                        DataBinding = "ComputedAddress.Locality",
                    },
                    new FormItemText {
                        Label = "County",
                        DataBinding = "ComputedAddress.County",
                    },
                    new FormItemText {
                        Label = "Postcode",
                        DataBinding = "ComputedAddress.Postcode",
                    }
                ], "Address"),
            ];

        }
    }
}
