using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DCAdapter
{
    class Crypt
    {
        internal static string DecryptCode(string jsEncCode, string service_code)
        {
            string decCode = DecryptBase64WithKey(jsEncCode);
            int firstCode = int.Parse(decCode.Substring(0, 1));
            decCode = (firstCode > 5 ? firstCode - 5 : firstCode + 4).ToString() + decCode.Substring(1);

            string[] cmdList = decCode.Split(',');

            string rc = "";

            for (int i = 0; i < cmdList.Length; i++)
            {
                float rkkk = float.Parse(cmdList[i]);
                double aa = ((rkkk - i - 1) * 2) / (13 - i - 1);
                rc += ((char)aa).ToString();
            }
            
            return service_code.Substring(0, service_code.Length - 10) + rc;
        }

        private static string DecryptBase64WithKey(string jsEncCode)
        {
            string key = "qAr0Bs1Ct2D3uE4Fv5G6wH7I8xJ9K+yL/M=zNaObPcQdReSfTgUhViWjXkYlZmnop";

            string o = "";
            int c1, c2, c3;
            int e1, e2, e3, e4;
            int i = 0;
            jsEncCode = jsEncCode.Replace("[^A-Za-z0-9+/=]", "");
            while (i < jsEncCode.Length)
            {
                e1 = key.IndexOf(jsEncCode[i++]);
                e2 = key.IndexOf(jsEncCode[i++]);
                e3 = key.IndexOf(jsEncCode[i++]);
                e4 = key.IndexOf(jsEncCode[i++]);
                c1 = (e1 << 2) | (e2 >> 4);
                c2 = ((e2 & 15) << 4) | (e3 >>
                    2);
                c3 = ((e3 & 3) << 6) | e4;
                o = o + ((char)c1).ToString();
                if (e3 != 64)
                {
                    o = o + ((char)c2).ToString();
                }
                if (e4 != 64)
                {
                    o = o + ((char)c3).ToString();
                }
            }
            return o;
        }
    }
}
