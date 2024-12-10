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
    public class NotesDataContext : DataContext<Note>
    {
        public string PatientID { get; set; }

        public NotesDataContext(string patientID)
        {
            this.PatientID = patientID;

            Refresh();
        }

        public override void Refresh()
        {
            WindowManager wm = (WindowManager)Application.Current.MainWindow;

            bool isMedicalProfessional = wm.AuthorisedUser?.UserType == UserType.Doctor || wm.AuthorisedUser?.UserType == UserType.Nurse;

            Model = typeof(Note);
            DataSource = Note.GetAllNotesByPatientID(PatientID) ?? Array.Empty<Note>();
            Columns = new()
            {
                // "Model Property" - "Column Display Name"
                { "ID", "_ID" },
                { "FormattedDateCreated", "Date" },
                { "NoteBodySummary", "Note Summary" },
                { "NoteBody", "_Note" }, // Prefixed with _ to hide, purely here for better search indexing
                { "AssigneeFullName", "Created by" },
            };
            ColumnSort = new SortDescription(nameof(Note.DateCreated), ListSortDirection.Descending);
            Form = [
                new FormItemGroup([
                    new FormItemText {
                        Label = "ID",
                        DataBinding = nameof(Note.ID),
                        IsReadOnly = (_) => true,
                        Required = true,
                        MaxWidth = 100,
                        DefaultValue = (_) => Note.GenerateNoteID().ToString()
                    },
                    new FormItemText {
                        Label = "Patient",
                        DataBinding = nameof(Note.PatientID),
                        IsReadOnly = (_) => true,
                        Required = true,
                        DefaultValue = (_) => PatientID
                    },
                    new FormItemDatePicker {
                        Label = "Date created",
                        Required = true,
                        DataBinding = nameof(Note.DateCreated),
                        DatePickerFormat = DatePickerFormat.Long,
                        IsReadOnly = (_) => true,
                        DefaultValue = (_) => DateTime.Now
                    }
                ]),
                new FormItemGroup([
                    new FormItemCombo<int> {
                        Label = "Created by",
                        Required = true,
                        DataBinding = nameof(Note.UserID),
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
                            WindowManager wm = (WindowManager)Application.Current.MainWindow;

                            form.SetFieldValue("Created by", wm.AuthorisedUser?.ID, true);

                            return true;
                        }
                    }
                ]),
                new FormItemText {
                    Label = "Body",
                    DataBinding = nameof(Note.NoteBody),
                    Required = true,
                    IsLong = true
                },
            ];
        }
    }
}
