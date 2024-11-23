using PMS.Controllers;
using PMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS.Context
{
    public class PatientsDataContext : DataContext<Patient>
    {
        public PatientsDataContext()
        {
            DataSource = Patient.GetAllPatients() ?? Array.Empty<Patient>();
            Columns = new()
            {
                // "Model Property" - "Column Display Name"
                { "ID", "ID" },
                { "Forenames", "Forename(s)" },
                { "Surname", "Surname" },
                { "Gender", "Gender" },
                { "DateOfBirth", "Date of Birth" },
                { "ReadableAddress", "Address" },
                { "FormattedNHSNumber", "NHS Number" },
                { "FormattedMobilePhoneNumber", "Mobile Number" },
                { "FormattedLandlinePhoneNumber", "Landline Number" },
                { "DateCreated", "Date created" },
            };
        }
    }
}
