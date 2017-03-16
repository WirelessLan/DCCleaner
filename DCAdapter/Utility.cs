using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAdapter
{
    class Utility
    {
        public static string GetAbsoulteURL(string input)
        {
            const string _Domain = "http://gallog.dcinside.com/inc/";
            string prefix = "document.location.href=";
            string retVal = input.Replace(prefix, "");
            retVal = retVal.Replace("'", "").Replace("\"", "").Replace(";", "");

            return _Domain + retVal.Trim();
        }
    }
}
