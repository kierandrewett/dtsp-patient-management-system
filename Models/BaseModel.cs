using PMS.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PMS.Models
{
    public class BaseModel : PropertyObservable
    {
        public virtual string ORM_TABLE => throw new Exception("ORM_TABLE not defined");
        public virtual string ORM_PRIMARY_KEY => throw new Exception("ORM_PRIMARY_KEY not defined.");

        public static explicit operator BaseModel(Type v)
        {
            throw new NotImplementedException();
        }
    }
}
