using PMS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS.Models
{
    public class SecurityQuestionAnswer : BaseModel
    {
        public override string ORM_TABLE => "tblUserSecurityQuestion";
        public override string ORM_PRIMARY_KEY => "QuestionAnswerID";

        public string QuestionAnswerID { get; set; }

        public int UserID { get; set; }
        public int SecurityQuestionID { get; set; }

        public string Answer { get; set; }

        public static Result<SecurityQuestionAnswer[], Exception> GetUserSecurityQuestionAnswers(User user)
        {
            string query =
                "SELECT tblSecurityQuestion.Question, tblUserSecurityQuestion.Answer, tblUserSecurityQuestion.SecurityQuestionID AS QuestionID " +
                "FROM((tblSecurityQuestion INNER JOIN " +
                "tblUserSecurityQuestion ON tblSecurityQuestion.ID = tblUserSecurityQuestion.SecurityQuestionID) INNER JOIN " +
                "tblUser ON tblUserSecurityQuestion.UserID = tblUser.ID) " +
                "WHERE(tblUser.ID = ?)";

            SecurityQuestionAnswer[]? securityQuestions = AppDatabase.QueryAll<SecurityQuestionAnswer>(
                query,
                [user.ID.ToString()]
            );

            if (securityQuestions == null)
            {
                return Result<SecurityQuestionAnswer[], Exception>.Err(
                    new Exception("No security questions have been set-up, contact your system administrator for more information.")
                );
            }

            return Result<SecurityQuestionAnswer[], Exception>.Ok(securityQuestions);
        }
    }
}
