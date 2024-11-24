using PMS.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace PMS.Controllers
{
    class SearchController
    {
        public static Func<object, bool> SimpleSearchPredicate(string filter, string[]? allowedSearchKeys)
        {
            filter = Regex.Replace(filter, "\\s", "");

            return value =>
            {
                bool matches = false;

                foreach (PropertyInfo prop in value.GetType().GetProperties())
                {
                    if (allowedSearchKeys != null && !allowedSearchKeys.Contains(prop.Name))
                    {
                        continue;
                    }

                    var propValue = prop.GetValue(value);

                    string searchablePropValue = "";

                    if (propValue != null && propValue!.ToString() != null)
                    {

                        if (propValue is DateTime dt)
                        {
                            string dayName = DateHelper.GetDayName(dt.DayOfWeek);
                            string monthName = DateHelper.GetMonthName(dt.Month);

                            string paddedDay = dt.Day.ToString().PadLeft(2, '0');
                            string paddedMonth = dt.Month.ToString().PadLeft(2, '0');

                            string paddedHour = dt.Hour.ToString().PadLeft(2, '0');
                            string paddedMinute = dt.Minute.ToString().PadLeft(2, '0');
                            string paddedSecond = dt.Second.ToString().PadLeft(2, '0');

                            searchablePropValue = string.Join(" ", new string[]
                                {
                                    dayName,
                                    monthName,
                                    $"{dt.Day}/{dt.Month}/{dt.Year}",
                                    $"{paddedDay}/{paddedMonth}",
                                    $"{dt.Hour}:{dt.Minute}:{dt.Second}",
                                    $"{paddedHour}:{paddedMinute}:{paddedSecond}"
                                });
                        } else
                        {
                            searchablePropValue = Regex.Replace(propValue!.ToString(), "\\s", "");
                        }
                    } else
                    {
                        Debug.WriteLine($"(Search Controller): Unable to search key '{prop.Name}' as it cannot be casted to a string.");
                    }

                    if (searchablePropValue.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    {
                        matches = true;
                        break;
                    }
                }

                return matches;
            };
        }
    }
}
