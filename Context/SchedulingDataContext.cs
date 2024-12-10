using Microsoft.IdentityModel.Tokens;
using PMS.Components;
using PMS.Controllers;
using PMS.Dialogs;
using PMS.Models;
using PMS.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PMS.Context
{
    public class SchedulingDataContext : DataContext<Appointment>
    {
        public SchedulingDataContext()
        {
            Refresh();
        }

        public override void Refresh()
        {
            WindowManager wm = (WindowManager)Application.Current.MainWindow;

            Func<PMSContextualForm, string> ComputeID = (form) =>
            {
                return Appointment.GenerateAppointmentID();
            };

            static string? IsTimeFieldValid(FormItemBase field, PMSContextualForm form) {
                string? val = field.GetValue()?.ToString();

                if (!val.IsNullOrEmpty() && val!.Contains(':'))
                {
                    string[] split = val!.Split(':');

                    int hour = int.Parse(split[0]);
                    int min = int.Parse(split[1]);

                    if (min % 15 != 0)
                    {
                        return "Time must be in an increment of 15 minutes.";
                    }
                }

                return null;
            };

            static Func<PMSContextualForm, bool> OnTimeChange(string name)
            {
                return (form) =>
                {
                    DateTime dt = DateTime.Now;

                    DateTime startDate;

                    string rawDate = form.GetFieldValue($"{name} Date")?.ToString().Split(' ')[0];
                    string rawTime = form.GetFieldValue($"{name} Time")?.ToString();

                    if (DateTime.TryParse($"{rawDate} {rawTime}", out startDate))
                    {
                        dt = startDate;
                    }

                    form.SetFieldValue($"{name} Date/Time", dt.ToString());

                    FormItemBase? startDateEl = form.RegisteredFields?.FirstOrDefault(f => f.Key.Label == "Start Date").Key;
                    FormItemBase? startDateTimeEl = form.RegisteredFields?.FirstOrDefault(f => f.Key.Label == "Start Date/Time").Key;

                    FormItemBase? endDateEl = form.RegisteredFields?.FirstOrDefault(f => f.Key.Label == "End Date").Key;
                    FormItemBase? endDateTimeEl = form.RegisteredFields?.FirstOrDefault(f => f.Key.Label == "End Date/Time").Key;

                    if (
                        startDateEl != null &&
                        startDateTimeEl != null &&
                        endDateEl != null &&
                        endDateTimeEl != null
                    ) {
                        Calendar startDateDP = ((Calendar)startDateEl!.RenderedWidget);
                        Calendar endDateDP = ((Calendar)endDateEl!.RenderedWidget);

                        if (
                            startDateDP.SelectedDate != null &&
                            endDateDP.SelectedDate != null
                        )
                        {
                            string startDateTimeRaw = ((TextBox)startDateTimeEl.RenderedWidget).Text;
                            string endDateTimeRaw = ((TextBox)endDateTimeEl.RenderedWidget).Text;


                            DateTime startDateTimeParsed;
                            DateTime endDateTimeParsed;

                            if (
                                DateTime.TryParse(startDateTimeRaw, out startDateTimeParsed) && 
                                DateTime.TryParse(endDateTimeRaw, out endDateTimeParsed
                            ))
                            {
                                TimeSpan subtrSpan = endDateTimeParsed.Subtract(startDateTimeParsed);

                                endDateDP.DisplayDateStart = startDateDP.SelectedDate;

                                if (subtrSpan.TotalMilliseconds < 0)
                                {
                                    endDateDP.SelectedDate = startDateDP.SelectedDate;

                                    // Just redo it all again, we would've messed with the dates
                                    OnTimeChange(name);
                                    return true;
                                }

                                FormItemBase? durationEl = form.RegisteredFields?.FirstOrDefault(f => f.Key.Label == "Appointment Duration").Key;
                                ((TextBox)durationEl.RenderedWidget).Foreground = new SolidColorBrush(subtrSpan.TotalHours <= 0 
                                    ? Colors.Red 
                                    : Colors.Black
                                );

                                form.SetFieldValue("Appointment Duration", $"{subtrSpan.TotalHours:0.00} hours");
                            }
                        }
                    }

                    PopulateNoteFields(form);

                    return true;
                };
            };

            static string? IsComputedDTValid(FormItemBase field, PMSContextualForm form)
            {
                string? startDt = form.GetFieldValue($"Start Date/Time")?.ToString();
                DateTime startDtParsed;

                string? endDt = form.GetFieldValue($"End Date/Time")?.ToString();
                DateTime endDtParsed;

                if (DateTime.TryParse(startDt, out startDtParsed) && DateTime.TryParse(endDt, out endDtParsed))
                {
                    if (startDtParsed == endDtParsed)
                    {
                        return "Appointment duration cannot be 0 hours.";
                    }

                    if (endDtParsed <= startDtParsed)
                    {
                        return "End date cannot be before start date.";
                    } else if (startDtParsed >= endDtParsed)
                    {
                        return "Start date cannot be after end date.";
                    }
                }

                return null;
            };

            static void PopulateNoteFields(PMSContextualForm form)
            {
                form.SetFieldValue("tblNote:PatientID", form.GetFieldValue("Patient")?.ToString());
                form.SetFieldValue("tblNote:UserID", form.GetFieldValue("Doctor")?.ToString());
            }

            OnTimeChange("Start");
            OnTimeChange("End");

            Model = typeof(Appointment);
            DataSource = Appointment.GetAllAppointments(wm.AuthorisedUser) ?? Array.Empty<Appointment>();
            Columns = new()
            {
                // "Model Property" - "Column Display Name"
                { "ID", "ID" },
                { "DepartmentWithID", "Department" },
                { "PatientFullName", "Patient" },
                { "DoctorFullName", "Doctor" },
                { "Reason", "Reason" },
                { "FormattedDateScheduledStart", "Scheduled Start" },
                { "FormattedDateScheduledEnd", "Scheduled End" },
                { "ComputedStatus", "Status" },
                { "FormattedDateCreated", "Date created" },
            };
            Form = [
                new FormItemGroup([
                    new FormItemText {
                        Label = "ID",
                        DataBinding = nameof(Appointment.ID),
                        IsReadOnly = (_) => true,
                        Required = true,
                        MaxWidth = 200,
                        DefaultValue = ComputeID
                    },
                    new FormItemCombo<Department> {
                        Label = "Department",
                        Required = true,
                        DataBinding = nameof(Appointment.Department),
                        Options = DepartmentObject.GetAllDepartmentOptions(),
                        DefaultValue = (_) => Department.General,
                    },
                    new FormItemDatePicker {
                        Label = "Date created",
                        Required = true,
                        DataBinding = nameof(Appointment.DateCreated),
                        DatePickerFormat = DatePickerFormat.Long,
                        IsReadOnly = (_) => true,
                        DefaultValue = (_) => DateTime.Now
                    },
                ]),
                new FormItemGroup([
                    new FormItemCombo<string> {
                        Label = "Patient",
                        Required = true,
                        DataBinding = nameof(Appointment.PatientID),
                        Options = Patient.GetAllPatientOptions(),
                        OnChange = (form) => {
                            PopulateNoteFields(form);

                            return true;
                        }
                    },
                    new FormItemCombo<int> {
                        Label = "Doctor",
                        Required = true,
                        IsReadOnly = (form) => wm.AuthorisedUser.UserType == UserType.Doctor,
                        DataBinding = nameof(Appointment.DoctorID),
                        Options = (User.GetUsersByType(UserType.Doctor) ?? []).Concat(User.GetUsersByType(UserType.Nurse) ?? [])
                            .ToDictionary(user => user.ID, user => user.FormatFullName()),
                        DefaultValue = (_) => wm.AuthorisedUser?.ID,
                        OnChange = (form) => {
                            PopulateNoteFields(form);

                            return true;
                        },
                        HelpLabel = wm.AuthorisedUser?.UserType == UserType.Doctor ? "You can only assign appointments to yourself." : "",
                    },
                ]),
                new FormItemGroup([
                    /* Begin tblAppointment:Note <-> tblNote:ID relationship */
                    new FormItemText {
                        Label = "tblAppointment:Note",
                        DataBinding = "Note",
                        Required = true,
                        IsReadOnly = (_) => true,
                        Hidden = (_) => true,
                        DefaultValue = (_) => Note.GenerateNoteID().ToString(),
                        OnPaint = (form) => {
                            PopulateNoteFields(form);
                            return true;
                        }
                    },
                    new FormItemText {
                        Label = "tblNote:ID",
                        DataBinding = "ComputedNote.ID",
                        Required = true,
                        IsReadOnly = (_) => true,
                        Hidden = (_) => true,
                        DefaultValue = (_) => Note.GenerateNoteID().ToString(),
                        OnPaint = (form) => {
                            PopulateNoteFields(form);
                            return true;
                        }
                    },
                    new FormItemText {
                        Label = "tblNote:PatientID",
                        DataBinding = "ComputedNote.PatientID",
                        Required = true,
                        Hidden = (_) => true,
                        IsReadOnly = (_) => true,
                        DefaultValue = (form) => form.GetFieldValue("Patient")?.ToString() ?? "",
                        OnPaint = (form) => {
                            PopulateNoteFields(form);
                            return true;
                        }
                    },
                    new FormItemText {
                        Label = "tblNote:UserID",
                        DataBinding = "ComputedNote.UserID",
                        Required = true,
                        Hidden = (_) => true,
                        IsReadOnly = (_) => true,
                        DefaultValue = (form) => form.GetFieldValue("Doctor")?.ToString() ?? "",
                        OnPaint = (form) => {
                            PopulateNoteFields(form);
                            return true;
                        }
                    },
                    new FormItemDatePicker {
                        Label = "tblNote:DateCreated",
                        DataBinding = "ComputedNote.DateCreated",
                        Required = true,
                        Hidden = (_) => true,
                        IsReadOnly = (_) => true,
                        DefaultValue = (_) => DateTime.Now,
                        OnPaint = (form) => {
                            PopulateNoteFields(form);
                            return true;
                        }
                    },
                    /* End relationship */
                    new FormItemText {
                        Label = "Notes",
                        Required = true,
                        IsLong = true,
                        DataBinding = "ComputedNote.NoteBody"
                    },
                    new FormItemGroup([
                        new FormItemCombo<AppointmentReason> {
                            Label = "Reason",
                            Required = true,
                            DataBinding = nameof(Appointment.Reason),
                            Options = AppointmentReasonObject.GetAllAppointmentReasonOptions()
                        },
                        new FormItemCombo<AppointmentStatus> {
                            Label = "Status",
                            Required = true,
                            IsReadOnly = (form) => form.IsNewEntry,
                            DataBinding = nameof(Appointment.Status),
                            DefaultValue = (_) => AppointmentStatus.Scheduled,
                            Options = AppointmentStatusObject.GetAllAppointmentStatusOptions()
                        }
                    ], null, Orientation.Vertical)
                ]),
                new FormItemGroup([
                    new FormItemGroup([
                        new FormItemCalendar {
                            Label = "Start Date",
                            DataBinding = nameof(Appointment.DateScheduledStart),
                            DefaultValue = (_) => DateTime.Now,
                            DateRange = new CalendarDateRange(DateTime.Today, DateTime.MaxValue),
                            OnChange = OnTimeChange("Start")
                        },
                        new FormItemGroup([
                            new FormItemTimePicker {
                                Label = "Start Time",
                                DataBinding = nameof(Appointment.TimeScheduledStart),
                                IsFieldValid = IsTimeFieldValid,
                                DefaultValue = (_) => "12:00",
                                OnChange = OnTimeChange("Start")
                            },
                            new FormItemText {
                                Label = "Start Date/Time",
                                IsReadOnly = (_) => true,
                                Required = true,
                                IsFieldValid = IsComputedDTValid
                            },
                            new FormItemButton {
                                ButtonLabel = "Reset date",
                                AccessibleLabel = "Start Date: Reset date",
                                OnClick = (form) => {
                                    form.SetFieldValue("Start Date", DateTime.Now);
                                    OnTimeChange("Start");
                                    return true;
                                }
                            },
                        ], "", Orientation.Vertical),
                    ], ""),
                    new FormItemGroup([
                        new FormItemCalendar {
                            Label = "End Date",
                            DataBinding = nameof(Appointment.DateScheduledEnd),
                            DefaultValue = (_) => DateTime.Now,
                            DateRange = new CalendarDateRange(DateTime.Today, DateTime.MaxValue),
                            OnChange = OnTimeChange("End")
                        },
                        new FormItemGroup([
                            new FormItemTimePicker {
                                Label = "End Time",
                                DataBinding = nameof(Appointment.TimeScheduledEnd),
                                IsFieldValid = IsTimeFieldValid,
                                DefaultValue = (_) => "12:00",
                                OnChange = OnTimeChange("End")
                            },
                            new FormItemText {
                                Label = "End Date/Time",
                                IsReadOnly = (_) => true,
                                Required = true,
                                IsFieldValid = IsComputedDTValid
                            },
                            new FormItemButton {
                                ButtonLabel = "Reset date",
                                AccessibleLabel = "End Date: Reset date",
                                OnClick = (form) => {
                                    form.SetFieldValue("End Date", DateTime.Now);
                                    OnTimeChange("End");
                                    return true;
                                }
                            },
                        ], "", Orientation.Vertical),
                    ], ""),
                ]),
                new FormItemText {
                    Label = "Appointment Duration",
                    IsReadOnly = (_) => true,
                    Required = true,
                    IsFieldValid = (field, form) => {
                        string? startDt = form.GetFieldValue($"Start Date/Time")?.ToString();
                        DateTime startDtParsed;

                        string? endDt = form.GetFieldValue($"End Date/Time")?.ToString();
                        DateTime endDtParsed;

                        if (DateTime.TryParse(startDt, out startDtParsed) && DateTime.TryParse(endDt, out endDtParsed))
                        {
                            string? doctorID = form.GetFieldValue("Doctor")?.ToString();

                            if (doctorID == null)
                            {
                                return "No doctor to look-up appointments for.";
                            }

                            string thisApptID = form.GetFieldValue("ID")?.ToString();

                            Appointment[]? allDoctorAppointments = AppDatabase.QueryAll<Appointment>(
                                "SELECT * FROM tblAppointment WHERE DoctorID=? AND NOT ID=?",
                                [doctorID, thisApptID!]
                            );

                            foreach (Appointment appt in allDoctorAppointments)
                            {
                                string apptDateStartStr = $"{appt.DateScheduledStart.ToShortDateString()} {appt.TimeScheduledStart}";
                                string apptDateEndStr = $"{appt.DateScheduledEnd.ToShortDateString()} {appt.TimeScheduledEnd}";

                                DateTime apptDateStart = DateTime.Parse(apptDateStartStr);
                                DateTime apptDateEnd = DateTime.Parse(apptDateEndStr);

                                if ((startDtParsed < apptDateEnd) && (endDtParsed > apptDateStart))
                                {
                                    string msg = $"Appointment collides with another appointment between {apptDateStartStr} and {apptDateEndStr}.";

                                    return msg;
                                }
                            }
                        }

                        return null;
                    }
                },
            ];
        }
    }
}
