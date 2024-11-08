using PMS.Models;
using PMS.Util;
using System;
using System.Collections.Generic;
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
    }
}
