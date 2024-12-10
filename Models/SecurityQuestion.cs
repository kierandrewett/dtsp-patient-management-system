using PMS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS.Models
{
    public class SecurityQuestion
    {
        public int ID { get; set; }
        public string Question { get; set; }

        public static SecurityQuestion[]? GetAllSecurityQuestions()
        {
            return AppDatabase.QueryAll<SecurityQuestion>(
                "SELECT * FROM tblSecurityQuestion",
                []
            );
        }

        public static SecurityQuestion? GetSecurityQuestionByID(int id)
        {
            return AppDatabase.QueryFirst<SecurityQuestion>(
               "SELECT * FROM tblSecurityQuestion WHERE ID=?",
               [id.ToString()]
           );
        }

        public static Dictionary<int, string> GetSecurityQuestionOptions()
        {
            SecurityQuestion[]? databaseSecurityQuestions = SecurityQuestion.GetAllSecurityQuestions();

            Dictionary<int, string> securityQuestions = new();

            foreach (SecurityQuestion question in databaseSecurityQuestions ?? [])
            {
                securityQuestions.Add(question.ID, question.Question);
            }

            return securityQuestions.OrderBy(q => q.Key).ToDictionary();
        }
    }
}
