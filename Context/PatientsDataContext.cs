using PMS.Components;
using PMS.Controllers;
using PMS.Dialogs;
using PMS.Models;
using PMS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PMS.Context
{
    public class PatientsDataContext : DataContext<Patient>
    {
        public int? AssignedDoctorID { get; set; }

        public PatientsDataContext(int? assignedDoctorID = null)
        {
            AssignedDoctorID = assignedDoctorID;

            Refresh();
        }

        public override string GetAccessibleName(Patient cell)
        {
            return "";
        }

        public override void Refresh()
        {
            WindowManager wm = (WindowManager)Application.Current.MainWindow;

            static bool OpenNoteManager(PMSContextualForm form)
            {
                string patientID = form.GetFieldValue("ID")?.ToString()!;
                PMSPatientNoteManagerWindow pnmWin = new(patientID);

                bool result = pnmWin.ShowDialog() ?? true;

                return result;
            }

            static bool OpenPrescriptionManager(PMSContextualForm form)
            {
                string patientID = form.GetFieldValue("ID")?.ToString()!;
                PMSPatientPrescriptionManagerWindow ppmWin = new(patientID);

                bool result = ppmWin.ShowDialog() ?? true;

                return result;
            }

            string userEditKeyword = PermissionController.CanEditRecord(wm.AuthorisedUser, typeof(Patient)) != null ? "Manage" : "View";

            Patient[]? PatientsList = AssignedDoctorID != null 
                ? Patient.GetAllPatientsForDoctor((int)AssignedDoctorID) 
                : Patient.GetAllPatients();

            Model = typeof(Patient);
            DataSource = PatientsList ?? Array.Empty<Patient>();
            Columns = new()
            {
                // "Model Property" - "Column Display Name"
                { "ID", "ID" },
                { "Title", "Title" },
                { "Forenames", "Forename(s)" },
                { "Surname", "Surname" },
                { "Gender", "Gender" },
                { "FormattedDateOfBirth", "Date of Birth" },
                { "DateOfBirth", "_Date of Birth" }, // Prefixed with _ to hide, purely here for better search indexing
                { "ReadableAddress", "Address" },
                { "FormattedNHSNumber", "NHS Number" },
                { "FormattedMobilePhoneNumber", "Mobile Number" },
                { "FormattedLandlinePhoneNumber", "Landline Number" },
                { "DoctorFullName", "Assigned Doctor" },
                { "FormattedDateCreated", "Date created" },
            };
            CompactColumns = new()
            {
                // "Model Property" - "Column Display Name"
                { "ID", "ID" },
                { "Title", "Title" },
                { "Forenames", "Forename(s)" },
                { "Surname", "Surname" },
                { "Gender", "Gender" },
                { "FormattedDateOfBirth", "Date of Birth" },
                { "ReadableAddress", "Address" },
                { "FormattedNHSNumber", "NHS Number" },
            };
            ColumnSort = new SortDescription(nameof(Patient.ID), ListSortDirection.Ascending);
            Form = [
                new FormItemGroup([
                    new FormItemText {
                        Label = "ID",
                        DataBinding = nameof(Patient.ID),
                        IsReadOnly = (_) => true,
                        Required = true,
                        MaxWidth = 200,
                        DefaultValue = (_) => Patient.GeneratePatientID()
                    },
                    new FormItemText {
                        Label = "NHS Number",
                        DataBinding = nameof(Patient.NHSNumber),
                        Required = true,
                        Pattern = new Regex("^\\d{0,10}$")
                    },
                    new FormItemCombo<int> {
                        Label = "Assigned Doctor",
                        Required = true,
                        DataBinding = nameof(Patient.DoctorID),
                        Options = Doctor.GetAllDoctorOptions()
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
                        Options = Patient.GetAllGenderOptions(),
                        MaxWidth = 175,
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
                        IsReadOnly = (_) => true,
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
                    IsReadOnly = (_) => true,
                    Hidden = (_) => true,
                    DefaultValue = (_) => Patient.GeneratePatientID()
                },
                new FormItemText {
                    Label = "tblAddress:PatientID",
                    DataBinding = "ComputedAddress.PatientID",
                    IsReadOnly = (_) => true,
                    Hidden = (_) => true,
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
                new FormItemGroup([
                    new FormItemButton {
                        Label = "Notes",
                        ForceEnable = true,
                        ButtonLabel = userEditKeyword + " patient notes...",
                        OnClick = (form) => OpenNoteManager(form),
                        Hidden = (form) => form.IsNewEntry
                    },
                    new FormItemButton {
                        Label = "Prescriptions",
                        ForceEnable = true,
                        ButtonLabel = userEditKeyword + " patient prescriptions...",
                        OnClick = (form) => OpenPrescriptionManager(form),
                        Hidden = (form) => form.IsNewEntry
                    }
                ]),

            ];
        }
    }
}
