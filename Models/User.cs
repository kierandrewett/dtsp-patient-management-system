using PMS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace PMS.Models
{
    public enum UserType
    {
        Admin,
        Doctor,
        Patient,
        Nurse
    }

    public class User : PropertyObservable
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

        protected string _Username;
        public string Username
        {
            get { return _Username; }
            set
            {
                _Username = value;
                DidUpdateProperty("Username");
            }
        }

        protected string _Password;
        public string Password
        {
            get { return _Password; }
            set
            {
                _Password = value;
                DidUpdateProperty("Password");
            }
        }

        protected UserType _UserType;
        public UserType UserType
        {
            get { return _UserType; }
            set
            {
                _UserType = value;
                DidUpdateProperty("UserType");
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

        protected bool _IsDisabled;
        public bool IsDisabled
        {
            get { return _IsDisabled; }
            set
            {
                _IsDisabled = value;
                DidUpdateProperty("IsDisabled");
            }
        }
    }
}
