using Microsoft.IdentityModel.Tokens;
using PMS.Components;
using PMS.Controllers;
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
using System.Windows.Data;
using System.Windows.Input;
using static PMS.Models.FormItemCheckbox;

namespace PMS.Context
{
    public class UsersDataContext : DataContext<User>
    {
        public static bool OnNameKey(PMSContextualForm form)
        {
            string? username = form.GetFieldValue("Username")?.ToString() ?? "";

            string? forename = form.GetFieldValue("Forename(s)")?.ToString() ?? "";
            string? surname = form.GetFieldValue("Surname")?.ToString() ?? "";


            forename = Regex.Replace(forename.Trim().ToLower(), "\\s+", "-");
            surname = Regex.Replace(surname.Trim().ToLower(), "\\s+", "-");

            string composedEmail = "";

            if (!forename.IsNullOrEmpty() && !surname.IsNullOrEmpty())
            {
                composedEmail = $"{forename}.{surname}";
            }
            else if (!forename.IsNullOrEmpty())
            {
                composedEmail += forename;
            }
            else if (!surname.IsNullOrEmpty())
            {
                composedEmail += surname;
            }

            if (composedEmail.Trim().Length > 0)
            {
                composedEmail += "@prs.uk";
            }

            form.SetFieldValue("Username", composedEmail);

            return true;
        }

        public UsersDataContext()
        {
            Refresh();
        }

        public override void Refresh()
        {
            string passwordHelpLabel = "Note: The plain-text password is only visible once during creation, keep a note of it.";

            static Func<FormItemBase, PMSContextualForm, string?> IsAnswerFieldValid(string idx) {
                return (field, form) =>
                {
                    string? question = form.GetFieldValue($"Question {idx}")?.ToString();
                    string? answer = form.GetFieldValue($"Answer {idx}")?.ToString();

                    if ((question != null && question != "0") && (!question.IsNullOrEmpty() && answer.IsNullOrEmpty()))
                    {
                        return $"Must specify Answer for Question {idx}.";
                    }

                    return null;
                };
            };

            Model = typeof(User);
            DataSource = User.GetAllUsers() ?? Array.Empty<User>();
            Columns = new()
            {
                // "Model Property" - "Column Display Name"
                { "ID", "ID" },
                { "Title", "Title" },
                { "Username", "Username" },
                { "Forenames", "Forename(s)" },
                { "Surname", "Surname" },
                { "UserType", "Type" },
                { "DepartmentWithID", "Department" },
                { "IsDisabled", "Account disabled" },
                { "HasFirstLogin", "Logged in once?" },
                { "FormattedDateCreated", "Date Created" },

            };
            CompactColumns = new()
            {
                // "Model Property" - "Column Display Name"
                { "ID", "ID" },
                { "Title", "Title" },
                { "Username", "Username" },
                { "Forenames", "Forename(s)" },
                { "Surname", "Surname" },
            };
            ColumnSort = new SortDescription(nameof(User.ID), ListSortDirection.Ascending);
            Form = [
                new FormItemGroup([
                    new FormItemText {
                        Label = "ID",
                        DataBinding = nameof(User.ID),
                        IsReadOnly = (_) => true,
                        Required = true,
                        MaxWidth = 100,
                        DefaultValue = (_) => User.GenerateUserID().ToString()
                    },
                    new FormItemText {
                        Label = "Username",
                        Required = true,
                        DataBinding = nameof(User.Username),
                        DefaultValue = (_) => "(Generated from name)",
                        IsReadOnly = (form) => form.IsNewEntry,
                        IsFieldValid = (field, form) => {
                            string? val = field.GetValue()?.ToString();

                            if (!val.IsNullOrEmpty())
                            {
                                if (!EmailValidator.Validate(val))
                                {
                                    return "Username is not a valid email address.";
                                }

                                if (!val.EndsWith("@prs.uk"))
                                {
                                    return "Username must be a part of the @prs.uk domain.";
                                }

                                string userID = form.GetFieldValue("ID")?.ToString() ?? "";

                                if (AppDatabase.QueryCount<User>(
                                    "SELECT * FROM tblUser WHERE Username=? AND NOT ID=?",
                                    [val!, userID]
                                ) > 0) {
                                    return "Username is already in use.";
                                }

                            }

                            return null;
                        }
                    },
                    new FormItemCombo<Department> {
                        Label = "Department",
                        Required = true,
                        DataBinding = nameof(User.Department),
                        Options = DepartmentObject.GetAllDepartmentOptions(),
                        DefaultValue = (_) => Department.General,
                    },
                    new FormItemDatePicker {
                        Label = "Date created",
                        Required = true,
                        DataBinding = nameof(User.DateCreated),
                        DatePickerFormat = DatePickerFormat.Long,
                        IsReadOnly = (_) => true,
                        DefaultValue = (_) => DateTime.Now
                    }
                ]),
                new FormItemGroup([
                    new FormItemButton {
                        Label = "Password update",
                        ButtonLabel = "Update password...",
                        Hidden = (form) => form.IsNewEntry,
                        OnClick = (form) => {
                            WindowManager wm = (WindowManager)Application.Current.MainWindow;

                            string? id = form.GetFieldValue("ID")?.ToString();

                            if (!id.IsNullOrEmpty())
                            {
                                User? user = User.GetUserByID(int.Parse(id!));

                                if (user != null)
                                {
                                    wm.HandlePasswordChangeRequest(user);
                                }
                            }

                            return true;
                        },
                    },
                    new FormItemText {
                        Label = "Password",
                        Required = true,
                        Hidden = (form) => !form.IsNewEntry,
                        IsReadOnly = (_) => true,
                        HelpLabel = passwordHelpLabel,
                        DefaultValue = (_) => User.GeneratePassword(),
                        SerialiseWith = (value) => value != null
                            ? PasswordHelper.HashPassword((string)value!)
                            : "",
                        DataBinding = "HashedPassword"
                    },
                    new FormItemCheckbox {
                        Label = "Account disabled?",
                        Required = true,
                        IsReadOnly = (form) => {
                            WindowManager wm = (WindowManager)Application.Current.MainWindow;

                            string? id = form.GetFieldValue("ID")?.ToString();
                            string? username = form.GetFieldValue("Username")?.ToString();

                            return id != null && (
                                wm.AuthorisedUser?.ID == int.Parse(id!) ||
                                (
                                    username != null &&
                                    username! == AuthenticationController.DEFAULT_ADMIN_USERNAME
                                )
                            );
                        },
                        DataBinding = nameof(User.IsDisabled)
                    },
                    new FormItemCheckbox {
                        Label = "Has first login?",
                        Required = true,
                        IsReadOnly = (form) => {
                            WindowManager wm = (WindowManager)Application.Current.MainWindow;

                            string? id = form.GetFieldValue("ID")?.ToString();
                            string? username = form.GetFieldValue("Username")?.ToString();

                            return form.IsNewEntry || (
                                id != null && (
                                    wm.AuthorisedUser?.ID == int.Parse(id!) ||
                                    (
                                        username != null &&
                                        username! == AuthenticationController.DEFAULT_ADMIN_USERNAME
                                    )
                                ));
                        },
                        DataBinding = nameof(User.HasFirstLogin),
                        DefaultValue = (form) => !form.IsNewEntry,
                        HelpLabel = "Disabling this will enforce a self-service user password reset upon login."
                    }
                ]),
                new FormItemGroup([
                    new FormItemCombo<Title> {
                        Label = "Title",
                        Required = true,
                        DataBinding = nameof(User.Title),
                        Options = TitleObject.GetAllTitleOptions(),
                        MaxWidth = 100,
                    },
                    new FormItemText {
                        Label = "Forename(s)",
                        Required = true,
                        DataBinding = nameof(User.Forenames),
                        OnKey = OnNameKey,
                    },
                    new FormItemText {
                        Label = "Surname",
                        DataBinding = nameof(User.Surname),
                        OnKey = OnNameKey,
                    },
                    new FormItemCombo<UserType> {
                        Label = "Type",
                        Required = true,
                        DataBinding = nameof(User.UserType),
                        Options = UserTypeObject.GetAllUserTypeOptions([UserType.Patient])
                    }
                ]),
                new FormItemGroup([
                    new FormItemGroup([
                        /* Begin tblUser:SecurityQuestion1 <-> tblUserSecurityQuestion:UserID relationship */
                        new FormItemText {
                            Label = "tblUser:SecurityQuestion1",
                            DataBinding = "SecurityQuestion1",
                            IsReadOnly = (_) => true,
                            Hidden = (_) => true,
                            DefaultValue = (form) => {
                                string userID = form.GetFieldValue("ID")?.ToString()!;

                                return $"{userID}-1";
                            }
                        },
                        new FormItemText {
                            Label = "tblUserSecurityQuestion:QuestionAnswerID",
                            DataBinding = "ComputedSecurityQuestion1.QuestionAnswerID",
                            IsReadOnly = (_) => true,
                            Hidden = (_) => true,
                            DefaultValue = (form) => {
                                string userID = form.GetFieldValue("ID")?.ToString()!;

                                return $"{userID}-1";
                            }
                        },
                        new FormItemText {
                            Label = "tblUserSecurityQuestion:UserID",
                            DataBinding = "ComputedSecurityQuestion1.UserID",
                            IsReadOnly = (_) => true,
                            Hidden = (_) => true,
                            DefaultValue = (form) => form.GetFieldValue("ID")?.ToString()!
                        },
                        /* End relationship */
                        new FormItemCombo<int> {
                            Label = "Question 1",
                            DataBinding = "ComputedSecurityQuestion1.SecurityQuestionID",
                            HelpLabel = "Leave these fields blank to prompt the user on login.",
                            Options = SecurityQuestion.GetSecurityQuestionOptions(),
                        },
                        new FormItemText {
                            Label = "Answer 1",
                            DataBinding = "ComputedSecurityQuestion1.Answer",
                            IsFieldValid = IsAnswerFieldValid("1")
                        },
                    ], "Security Question 1"),
                    new FormItemGroup([
                        /* Begin tblUser:SecurityQuestion2 <-> tblUserSecurityQuestion:UserID relationship */
                        new FormItemText {
                            Label = "tblUser:SecurityQuestion2",
                            DataBinding = "SecurityQuestion2",
                            IsReadOnly = (_) => true,
                            Hidden = (_) => true,
                            DefaultValue = (form) => {
                                string userID = form.GetFieldValue("ID")?.ToString()!;

                                return $"{userID}-2";
                            }
                        },
                        new FormItemText {
                            Label = "tblUserSecurityQuestion:QuestionAnswerID",
                            DataBinding = "ComputedSecurityQuestion2.QuestionAnswerID",
                            IsReadOnly = (_) => true,
                            Hidden = (_) => true,
                            DefaultValue = (form) => {
                                string userID = form.GetFieldValue("ID")?.ToString()!;

                                return $"{userID}-2";
                            }
                        },
                        new FormItemText {
                            Label = "tblUserSecurityQuestion:UserID",
                            DataBinding = "ComputedSecurityQuestion2.UserID",
                            IsReadOnly = (_) => true,
                            Hidden = (_) => true,
                            DefaultValue = (form) => form.GetFieldValue("ID")?.ToString()!
                        },
                        /* End relationship */
                        new FormItemCombo<int> {
                            Label = "Question 2",
                            DataBinding = "ComputedSecurityQuestion2.SecurityQuestionID",
                            Options = SecurityQuestion.GetSecurityQuestionOptions(),
                        },
                        new FormItemText {
                            Label = "Answer 2",
                            DataBinding = "ComputedSecurityQuestion2.Answer",
                            IsFieldValid = IsAnswerFieldValid("2")
                        },
                    ], "Security Question 2"),
                ])
            ];
        }
    }
}
