using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAdapter
{
    class Utility
    {
        private readonly static string gallogDefaultDomain = "http://gallog.dcinside.com/inc/";

        public static string GetAbsoulteURL(string input)
        {
            return GetAbsoulteURL(input, gallogDefaultDomain);
        }

        public static string GetAbsoulteURL(string input, string domainPath)
        {
            string prefix = "document.location.href=";
            string retVal = input.Replace(prefix, "");
            retVal = retVal.Replace("'", "").Replace("\"", "").Replace(";", "");

            return domainPath + retVal.Trim();
        }
    }
}
