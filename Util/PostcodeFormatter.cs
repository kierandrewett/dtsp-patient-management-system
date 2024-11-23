using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PMS.Util
{
    class PostcodeFormatter
    {
        public static string Format(string postcode)
        {
            // https://ideal-postcodes.co.uk/guides/uk-postcode-format
            // Possible postcode formats:
            // AA9A 9AA
            // A9A 9AA
            // A9 9AA
            // A99 9AA
            // AA9 9AA
            // AA99 9AA

            if (postcode.IsNullOrEmpty())
            {
                throw new Exception("Postcode must not be null or empty.");
            }

            // Trim all whitespac
            postcode = Regex.Replace(postcode.Trim(), "\\W", "");

            // Shortest possible postcode length
            if (postcode.Length < 5)
            {
                throw new Exception("Postcode is too short.");
            }

            int inwardCodeStartIdx = postcode.Length - 3;
            string outwardCode = postcode[0..inwardCodeStartIdx];
            string inwardCode = postcode[inwardCodeStartIdx..postcode.Length];

            return $"{outwardCode} {inwardCode}";
        }
    }
}
