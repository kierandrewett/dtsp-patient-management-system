using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PMS.Controllers
{
    class SearchController
    {
        public static Func<object, bool> SimpleSearchPredicate(string filter)
        {
            return value =>
            {
                bool matches = false;

                Debug.WriteLine(value);
                foreach (PropertyInfo prop in value.GetType().GetProperties())
                {

                    var propValue = prop.GetValue(value);

                    if (propValue is string || propValue is int || propValue is float)
                    {
                        string strValue = propValue!.ToString();

                        if (strValue.Contains(filter, StringComparison.OrdinalIgnoreCase))
                        {
                            matches = true;
                            break;
                        }
                    }
                }

                return matches;
            };
        }
    }
}
