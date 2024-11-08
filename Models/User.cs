using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS.Models
{
    public enum UserType
    {
        Admin,
        Doctor,
        Patient,
        Nurse
    }

    public class User
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public UserType UserType { get; set; }
        public Title Title { get; set; }
        public string Forenames { get; set; }
        public string Surname { get; set; }
    }
}
