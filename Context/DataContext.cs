using PMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS.Context
{
    public class DataContext<T>
    {
        public T[] DataSource { get; set; }
        public Dictionary<string, string> Columns { get; set; }
    }
}
