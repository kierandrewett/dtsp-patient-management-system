using PMS.Components;
using PMS.Controllers;
using PMS.Models;
using PMS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PMS.Context
{
    public class SchedulingDataContext : DataContext<Appointment>
    {
        public SchedulingDataContext()
        {
            WindowManager wm = (WindowManager)Application.Current.MainWindow;

            Func<PMSContextualForm, string> ComputeID = (form) =>
            {
                string department = form.GetFieldValue("Department")?.ToString() ?? Department.General.ToString();

                return Appointment.GenerateAppointmentID(
                    (Department)Enum.Parse(typeof(Department), department)
                );
            };

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
                { "Status", "Status" },
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
                        OnChange = (form) => {
                            form.SetFieldValue("ID", ComputeID(form));

                            return true;
                        }
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
                    new FormItemDataGrid<Patient> {
                        Label = "Patient",
                        Required = true,
                        DataBinding = nameof(Appointment.PatientID),
                        InnerDataContext = new PatientsDataContext()
                    },
                    new FormItemGroup([
                        new FormItemList<int> {
                            Label = "Doctor",
                            Required = true,
                            DataBinding = nameof(Appointment.DoctorID),
                            Options = Doctor.GetAllDoctorOptions()
                        },
                        new FormItemButton {
                            Label = "Doctor ",
                            ButtonLabel = "Assign self",
                            Hidden = (_) => {
                                WindowManager wm = (WindowManager)Application.Current.MainWindow;

                                return wm.AuthorisedUser?.UserType != UserType.Doctor;
                            },
                            OnClick = (form) => {
                                WindowManager wm = (WindowManager)Application.Current.MainWindow;

                                form.SetFieldValue("Doctor", wm.AuthorisedUser?.ID, true);

                                return true;
                            }
                        }
                    ], null, Orientation.Vertical),
                ]),
                new FormItemGroup([
                    new FormItemText {
                        Label = "Notes",
                        Required = true,
                        IsLong = true,
                        DataBinding = nameof(Appointment.Notes)
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
                    new FormItemDatePicker {
                        Label = "Start at",
                        Required = true,
                        DataBinding = nameof(Appointment.DateScheduledStart),
                        DefaultValue = (_) => DateTime.Now
                    },
                    new FormItemDatePicker {
                        Label = "Ends at",
                        Required = true,
                        DataBinding = nameof(Appointment.DateScheduledEnd)
                    }
                ])
            ];

        }
    }
}
