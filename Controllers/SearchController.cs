using PMS.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ObjectiveC;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace PMS.Controllers
{
    class SearchController
    {
        public static string SanitiseInput(string input)
        {
            input = Regex.Replace(input, "\\s+", " "); // Removes multiple spaces with one space
            input = Regex.Replace(input, "(?<=\\d) (?=\\d)", ""); // Replace spaces if surrounded by numbers
            input = Regex.Replace(input, "[^a-zA-Z0-9]+", ""); // Remove special characters with a space

            return input;
        }

        public static T[] SimpleRankedSearch<T>(string filter, IEnumerable items, string[]? allowedSearchKeys)
        {
            filter = SanitiseInput(filter);

            Dictionary<T, int> results = [];

            foreach (T value in items)
            {
                int highestScore = 0;

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
                        LogController.WriteLine($"Unable to search key '{prop.Name}' as it cannot be casted to a string.", LogCategory.SearchController);
                    }

                    searchablePropValue = SanitiseInput(searchablePropValue);
                    string[] searchableSplit = searchablePropValue.Split(' ');

                    foreach (string s in searchableSplit)
                    {
                        int score = 0;

                        if (s.Contains(filter, StringComparison.OrdinalIgnoreCase) && s.StartsWith(filter, StringComparison.OrdinalIgnoreCase))
                        {
                            score = 4;
                        }
                        if (s.StartsWith(filter, StringComparison.OrdinalIgnoreCase))
                        {
                            score = 3;
                        } else if (s.Equals(filter, StringComparison.OrdinalIgnoreCase))
                        {
                            score = 2;
                        } else if (s.Contains(filter, StringComparison.OrdinalIgnoreCase))
                        {
                            score = 1;
                        }

                        highestScore = Math.Max(highestScore, score);
                    }
                }

                if (highestScore > 0)
                {
                    results.Add(value, highestScore);
                }
            };

            return results
                .ToArray()
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToArray();
        }
    }
}
