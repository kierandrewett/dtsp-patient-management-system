using PMS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS.Models
{
    public enum Title
    {
        None,
        Mr,
        Miss,
        Master,
        Mrs,
        Ms,
        Mx,
        Dr
    }

    public class TitleObject : PropertyObservable
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

        protected string _Title;
        public string Title
        {
            get { return _Title; }
            set
            {
                _Title = value;
                DidUpdateProperty("Title");
            }
        }

        public static Dictionary<Title, string> GetAllTitleOptions()
        {
            Dictionary<Title, string> titles = new() { };

            TitleObject[]? databaseTitles = AppDatabase.QueryAll<TitleObject>(
                "SELECT * FROM tblTitle",
                []
            );

            if (databaseTitles != null)
            {
                foreach (TitleObject titleObj in databaseTitles)
                {
                    titles.Add(
                        (Title)titleObj.ID,
                        titleObj.Title
                    );
                }
            }

            return titles;
        }
    }
}
