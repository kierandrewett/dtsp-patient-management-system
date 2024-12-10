using PMS.Models;

namespace PMS.Context
{
    public class MedicalProfessionalsDataContext : UsersDataContext
    {
        public MedicalProfessionalsDataContext()
        {
            User[] doctors = User.GetUsersByType(UserType.Doctor) ?? Array.Empty<User>();
            User[] nurses = User.GetUsersByType(UserType.Nurse) ?? Array.Empty<User>();

            DataSource = doctors.Concat(nurses).ToArray();
            if (!CompactColumns.ContainsKey("UserType"))
            {
                CompactColumns.Add("UserType", "Type");
            }
        }
    }
}
