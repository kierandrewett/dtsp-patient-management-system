using PMS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS.Models
{
    public class Person<T> : BaseModel
    {
        protected T _ID;
        public T ID
        {
            get { return _ID; }
            set
            {
                _ID = value;
                DidUpdateProperty("ID");
            }
        }

        protected Title _Title;
        public Title Title
        {
            get { return _Title; }
            set
            {
                _Title = value;
                DidUpdateProperty("Title");
            }
        }

        protected string _Forenames;
        public string Forenames
        {
            get { return _Forenames; }
            set
            {
                _Forenames = value;
                DidUpdateProperty("Forenames");
            }
        }

        protected string _Surname;
        public string Surname
        {
            get { return _Surname; }
            set
            {
                _Surname = value;
                DidUpdateProperty("Surname");
            }
        }
    }
}
