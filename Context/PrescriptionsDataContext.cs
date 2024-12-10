using PMS.Controllers;
using PMS.Models;
using PMS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PMS.Context
{
    public class PrescriptionsDataContext : DataContext<Prescription>
    {
        public string PatientID { get; set; }

        public PrescriptionsDataContext(string patientID)
        {
            this.PatientID = patientID;

            Refresh();
        }

        public override void Refresh()
        {
            WindowManager wm = (WindowManager)Application.Current.MainWindow;

            bool isMedicalProfessional = wm.AuthorisedUser?.UserType == UserType.Doctor || wm.AuthorisedUser?.UserType == UserType.Nurse;

            Model = typeof(Prescription);
            DataSource = Prescription.GetAllPrescriptionsByPatientID(PatientID) ?? Array.Empty<Prescription>();
            Columns = new()
            {
                // "Model Property" - "Column Display Name"
                { "ID", "_ID" },
                { "FormattedDateCreated", "Issue date" },
                { "FormattedDateExpire", "Continue until" },
                { "MedicationQuantity", "Quantity" },
                { "MedicationBrand", "Brand" },
                { "MedicationName", "Medication" },
                { "MedicationDosage", "Dosage" },
                { "MedicationForm", "Form" },
                { "MedicationAdministration", "Administration" },
                { "PrescriberFullName", "Prescribed by" },
            };
            ColumnSort = new SortDescription(nameof(Prescription.DateCreated), ListSortDirection.Descending);
            Form = [
                new FormItemGroup([
                    new FormItemText {
                        Label = "ID",
                        DataBinding = nameof(Prescription.ID),
                        IsReadOnly = (_) => true,
                        Required = true,
                        MaxWidth = 100,
                        DefaultValue = (_) => Prescription.GeneratePrescriptionID().ToString()
                    },
                    new FormItemText {
                        Label = "Patient",
                        DataBinding = nameof(Prescription.PatientID),
                        IsReadOnly = (_) => true,
                        Required = true,
                        DefaultValue = (_) => PatientID
                    },
                    new FormItemDatePicker {
                        Label = "Prescribed Date",
                        Required = true,
                        DataBinding = nameof(Prescription.DateCreated),
                        DatePickerFormat = DatePickerFormat.Long,
                        IsReadOnly = (_) => true,
                        DefaultValue = (_) => DateTime.Now
                    }
                ]),
                new FormItemGroup([
                    new FormItemCombo<int> {
                        Label = "Prescribed by",
                        Required = true,
                        DataBinding = nameof(Prescription.PrescriberID),
                        IsReadOnly = (form) => !form.IsNewEntry,
                        DefaultValue = (_) => wm.AuthorisedUser?.ID,
                        Options = (User.GetUsersByType(UserType.Doctor) ?? []).Concat(User.GetUsersByType(UserType.Nurse) ?? [])
                                .ToDictionary(user => user.ID, user => user.FormatFullName()),
                        HelpLabel = "Field cannot be changed after creation."
                    },
                    new FormItemButton {
                        Label = "",
                        ButtonLabel = "Assign self",
                        IsReadOnly = (_) => !isMedicalProfessional,
                        Hidden = (form) => !form.IsNewEntry,
                        OnClick = (form) => {
                            form.SetFieldValue("Prescribed by", wm.AuthorisedUser?.ID, true);

                            return true;
                        }
                    }
                ]),
                new FormItemGroup([
                    new FormItemText {
                        Label = "Qty",
                        DataBinding = nameof(Prescription.MedicationQuantity),
                        MaxWidth = 40,
                        Required = true,
                        Pattern = new Regex("[0-9]+"),
                    },
                    new FormItemText {
                        Label = "Brand",
                        DataBinding = nameof(Prescription.MedicationBrand),
                        Required = true
                    },
                    new FormItemText {
                        Label = "Name",
                        DataBinding = nameof(Prescription.MedicationName),
                        Required = true
                    },
                    new FormItemText {
                        Label = "Dosage",
                        DataBinding = nameof(Prescription.MedicationDosage),
                        Required = true
                    },
                ], "Medication"),
                new FormItemGroup([
                    new FormItemCombo<MedicationForm> {
                        Label = "Form",
                        DataBinding = nameof(Prescription.MedicationForm),
                        Options = MedicationFormObject.GetAllMedicationFormOptions(),
                        Required = true
                    },
                    new FormItemCombo<MedicationAdministration> {
                        Label = "Administration",
                        DataBinding = nameof(Prescription.MedicationAdministration),
                        Options = MedicationAdministrationObject.GetAllMedicationAdministrationOptions(),
                        Required = true
                    },
                    new FormItemDatePicker {
                        Label = "Continue usage until",
                        Required = true,
                        DataBinding = nameof(Prescription.DateExpire),
                        DatePickerFormat = DatePickerFormat.Long,
                    }
                ]),
                new FormItemGroup([
                    /* Begin tblPrescription:Note <-> tblNote:ID relationship */
                    new FormItemText {
                        Label = "tblPrescription:Note",
                        DataBinding = "Note",
                        Hidden = (_) => true,
                        IsReadOnly = (_) => true,
                        DefaultValue = (_) => Note.GenerateNoteID().ToString()
                    },
                    new FormItemText {
                        Label = "tblNote:ID",
                        DataBinding = "ComputedNote.ID",
                        Hidden = (_) => true,
                        IsReadOnly = (_) => true,
                        DefaultValue = (_) => Note.GenerateNoteID().ToString()
                    },
                    new FormItemText {
                        Label = "tblNote:PatientID",
                        DataBinding = "ComputedNote.PatientID",
                        Hidden = (_) => true,
                        IsReadOnly = (_) => true,
                        DefaultValue = (_) => PatientID
                    },
                    new FormItemText {
                        Label = "tblNote:UserID",
                        DataBinding = "ComputedNote.UserID",
                        Hidden = (_) => true,
                        IsReadOnly = (_) => true
                    },
                    new FormItemDatePicker {
                        Label = "tblNote:DateCreated",
                        DataBinding = "ComputedNote.DateCreated",
                        Hidden = (_) => true,
                        IsReadOnly = (_) => true,
                        DefaultValue = (_) => DateTime.Now
                    },
                    /* End relationship */
                    new FormItemText {
                        Label = "Notes",
                        IsLong = true,
                        DataBinding = "ComputedNote.NoteBody",
                    },
                ])
            ];
        }
    }
}
