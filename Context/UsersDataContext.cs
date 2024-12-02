using Microsoft.IdentityModel.Tokens;
using PMS.Components;
using PMS.Controllers;
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
            string passwordHelpLabel = "Note: The plain-text password is only visible once during creation, keep a note of it.";

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
                { "IsDisabled", "Account disabled" },
                { "HasFirstLogin", "Logged in once?" },

            };
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

                            return id != null && wm.AuthorisedUser?.ID == int.Parse(id!);
                        },
                        DataBinding = nameof(User.IsDisabled)
                    },
                    new FormItemCheckbox {
                        Label = "Has first login?",
                        Required = true,
                        IsReadOnly = (form) => form.IsNewEntry,
                        DataBinding = nameof(User.HasFirstLogin),
                        DefaultValue = (form) => !form.IsNewEntry,
                        HelpLabel = "Disabling this will enforce a user password reset upon login."
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
            ];
        }
    }
}
