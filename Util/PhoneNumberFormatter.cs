using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PMS.Util
{
    class PhoneNumberFormatter
    {
        public static string UK_COUNTRY_CODE = "44";

        public static string Format(string phoneNumber)
        {
            if (phoneNumber.Length <= 0)
            {
                return "";
            }

            // Strip out everything except for numbers
            phoneNumber = Regex.Replace(phoneNumber, "[^0-9+]+", "");

            List<char> phoneNumberChar = phoneNumber.ToCharArray().ToList();

            // Global phone number
            string countryCode = UK_COUNTRY_CODE;

            if (phoneNumberChar[0] == '+')
            {
                // Clear the country code, we cannot assume it's 44 anymore
                countryCode = "";

                // Start at 1 to skip the plus code
                int idx = 1;
                while (phoneNumberChar[idx] != '0')
                {
                    if (idx > 2)
                    {
                        // Give up, we shouldn't expect it a country code
                        // to be any longer than 3 chars
                        break;
                    }
                    countryCode += phoneNumberChar[idx];
                    idx++;
                }

                // Remove plus code, country code
                phoneNumberChar.RemoveRange(0, idx);
            }

            if (phoneNumberChar[0] == '0')
            {
                // Remove the leading 0
                phoneNumberChar.RemoveAt(0);
            }

            string? phoneNumberFinal = string.Join("", phoneNumberChar);

            if (phoneNumberFinal == null)
            {
                throw new Exception($"Unexpected error formatting phone number, got '{phoneNumber}'.");
            }

            if (phoneNumberFinal.Length < 10)
            {
                throw new Exception($"Phone number is too short, got '{phoneNumber}'.");
            }

            if (phoneNumberFinal.Length > 10)
            {
                throw new Exception($"Phone number is too long, got '{phoneNumber}'.");
            }

            string joinedNumber = string.Join(" ", [
                phoneNumberFinal[0..4],
                phoneNumberFinal[4..10]
            ]);

            string formattedNumber = "";

            // If the country code isn't local to the UK
            // display that before the phone number.
            if (countryCode != UK_COUNTRY_CODE)
            {
                formattedNumber += $"+{countryCode} ";
            }
            else
            {
                // Otherwise, if this is a local number
                // add back the leading 0 to indicate that
                // it is in-fact a local phone number.
                formattedNumber += "0";
            }

            formattedNumber += joinedNumber;

            return formattedNumber;
        }
    }
}
