using PMS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS.Models
{
    public class GenderObject : PropertyObservable
    {
        protected int _ID;
        public int ID
        {
            get { return _ID; }
            set
            {
                _ID = value;
                DidUpdateProperty("ID");
            }
        }

        protected string _Gender;
        public string Gender
        {
            get { return _Gender; }
            set
            {
                _Gender = value;
                DidUpdateProperty("Gender");
            }
        }
    }
}
