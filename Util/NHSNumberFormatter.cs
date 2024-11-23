using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PMS.Util
{
    class NHSNumberFormatter
    {
        public static string Format(string nhsNumber)
        {
            // https://www.merseywestlancs.nhs.uk/your-nhs-number
            // Most practices use the 3-3-4 format
            // 999 999 9999
            if (nhsNumber.IsNullOrEmpty())
            {
                throw new Exception("NHS Number must not be null or empty.");
            }

            // Trim all whitespace
            nhsNumber = Regex.Replace(nhsNumber.Trim(), "\\W", "");

            // Shortest possible NHS Number length
            if (nhsNumber.Length < 10)
            {
                throw new Exception("NHS Number is too short.");
            } else if (nhsNumber.Length > 10)
            {
                throw new Exception("NHS Number is too long.");
            }

            return string.Join(" ", [
                nhsNumber[0..3],
                nhsNumber[3..6],
                nhsNumber[6..10]
            ]);
        }
    }
}
