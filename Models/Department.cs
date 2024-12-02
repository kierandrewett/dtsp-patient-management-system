using PMS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS.Models
{
    public enum Department
    {
        General = 0,
        Administrative = 1,
        AccidentAndEmergency = 1100,
        CriticalCare = 1200,
        Maternity = 2000,
        Respiratory = 3000,
        Dermatology = 3100,
        Radiology = 3200,
        Pathology = 3300,
        Orthopaedics = 3400
    }

    public class DepartmentObject : PropertyObservable
    {
        protected int _ID;
        public int ID
        {
            get { return _ID; }
            set
            {
                _ID = value;
                DidUpdateProperty("ID");
            }
        }

        protected string _Department;
        public string Department
        {
            get { return _Department; }
            set
            {
                _Department = value;
                DidUpdateProperty("Department");
            }
        }

        public static Dictionary<Department, string> GetAllDepartmentOptions()
        {
            Dictionary<Department, string> departments = new() { };

            DepartmentObject[]? databaseDepartments = AppDatabase.QueryAll<DepartmentObject>(
                "SELECT * FROM tblDepartment",
                []
            );

            if (databaseDepartments != null)
            {
                foreach (DepartmentObject departmentObj in databaseDepartments)
                {
                    string departmentID = departmentObj.ID.ToString();

                    departments.Add(
                        (Department)departmentObj.ID,
                        $"{departmentID} - {departmentObj.Department}"
                    );
                }
            }

            return departments;
        }
    }
}
