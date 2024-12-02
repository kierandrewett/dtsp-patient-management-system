using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS.Util
{
    public class DateHelper
    {
        public static string GetDayName(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Monday:
                    return "Monday";
                case DayOfWeek.Tuesday:
                    return "Tuesday";
                case DayOfWeek.Wednesday:
                    return "Wednesday";
                case DayOfWeek.Thursday:
                    return "Thursday";
                case DayOfWeek.Friday:
                    return "Friday";
                case DayOfWeek.Saturday:
                    return "Saturday";
                case DayOfWeek.Sunday:
                    return "Sunday";
                default:
                    throw new Exception("Illegal day of week");
            }
        }

        public static string GetMonthName(int month)
        {
            switch (month)
            {
                case 1:
                    return "January";
                case 2:
                    return "February";
                case 3:
                    return "March";
                case 4:
                    return "April";
                case 5:
                    return "May";
                case 6:
                    return "June";
                case 7:
                    return "July";
                case 8:
                    return "August";
                case 9:
                    return "September";
                case 10:
                    return "October";
                case 11:
                    return "November";
                case 12:
                    return "December";
                default:
                    throw new Exception("Illegal month value");
            }
        }

        public static string GetOrdinal(int number)
        {
            // https://stackoverflow.com/a/20175s

            switch (number % 100)
            {
                case 11:
                case 12:
                case 13:
                    return "th";
            }

            switch (number % 10)
            {
                case 1:
                    return "st";
                case 2:
                    return "nd";
                case 3:
                    return "rd";
                default:
                    return "th";
            }
        }

        public static string ConvertToString(DateTime dateTime, bool hasTime = true)
        {
            string dayName = GetDayName(dateTime.DayOfWeek);
            string dayOrdinal = GetOrdinal(dateTime.Day);

            string monthName = GetMonthName(dateTime.Month);

            string day = dateTime.Day.ToString();
            string month = dateTime.Month.ToString().PadLeft(2, '0');
            string year = dateTime.Year.ToString();
            string hour = dateTime.Hour.ToString().PadLeft(2, '0');
            string minute = dateTime.Minute.ToString().PadLeft(2, '0');
            string second = dateTime.Second.ToString().PadLeft(2, '0');

            string dateString = $"{dayName}, {day}{dayOrdinal} {monthName} {year}";
            string timeString = $"{hour}:{minute}:{second}";

            string dateTimeString = dateString;

            if (hasTime)
            {
                dateTimeString += ", " + timeString;
            }

            return dateTimeString;
        }
    }
}
