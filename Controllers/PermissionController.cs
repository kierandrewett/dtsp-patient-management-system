using PMS.Models;
using PMS.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS.Controllers
{
    public class PermissionController
    {
        public static User? CanUserPasswordChange(User? user, User? targetUser = null, bool? didProveSecurityQuestions = null)
        {
            // This state should never occur, but handle it anyway in case we get too far
            if (user == null && targetUser == null)
            {
                return null;
            }

            // If we don't supply a target user, assume we're wanting to change
            // our own password.
            if (targetUser == null)
            {
                targetUser = user;
            }

            // !!! This may require extended permissions !!!
            // !!! Another or no user is trying to change the target user's password !!!
            // We must check if the authorised user is an administrator
            // before permitting the current user to change another user's
            // password.
            if (targetUser != user)
            {
                // We're checking for permission outside of an authorised user.
                // Since we have no idea who is logged in, be extra cautious
                // when authorising certain actions.
                if (user == null)
                {
                    // We utilise this method for the account recovery self-service application.
                    // Therefore, if we have already proved the user possesses the answers to
                    // their security questions, they can skip the administrator check and
                    // reset their own password.
                    if (didProveSecurityQuestions != null && didProveSecurityQuestions == true)
                    {
                        return targetUser;
                    }

                    return null;
                }
                else
                {
                    // If the user isn't an admin, throw an exception
                    if (user.UserType != UserType.Admin)
                    {
                        return null;
                    }

                    return targetUser;
                }
            } else
            {
                // The target user is ourself:
                // We can easily authorise this.
                return targetUser;
            }
        }

        public static User? CanAccessTabContent(User user, WindowTab windowTab)
        {
            UserType userType = user.UserType;

            switch (windowTab)
            {
                case WindowTab.Patients:
                    if (
                        userType == UserType.Admin ||
                        userType == UserType.Doctor ||
                        userType == UserType.Nurse
                    )
                    {
                        return user;
                    }

         
                    break;
                case WindowTab.Scheduling:
                    if (
                        userType == UserType.Doctor ||
                        userType == UserType.Nurse
                    )
                    {
                        return user;
                    }

                    break;
                case WindowTab.Users:
                    if (userType == UserType.Admin)
                    {
                        return user;
                    }

                    break;
                default:
                    return user;
            }

            return null;
        }

        public static User? CanEditRecord(User user, Type model)
        {
            if (model.Equals(typeof (Patient)))
            {
                // 2.3: Only doctors and nurses can edit the full patient record
                return user.UserType == UserType.Doctor || user.UserType == UserType.Nurse
                        ? user
                        : null;
            }

            return user;
        }

        public static User? CanCreateRecord(User user, Type model)
        {
            if (model.Equals(typeof(Patient)))
            {
                // 2.1: Only admins and nurses can add new patients 
                return user.UserType == UserType.Admin || user.UserType == UserType.Nurse
                        ? user
                        : null;
            }

            return user;
        }
    }
}
