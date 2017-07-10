using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DCAdapter
{
    class JSParser
    {
        internal static void ParseAdditionalDeleteParameter(string script, GalleryType gallType, out string encCode, out string name, out string value)
        {
            name = null;
            value = null;
            encCode = null;

#if false
            Match encData = Regex.Match(script, "var _r = _d\\(\'(.*)\'\\)");
#else
            Match encData = Regex.Match(script, "_d\\(\'(.*)\'\\)");
#endif
            if (encData.Success)
            {
                encCode = encData.Groups[1].Value;
            }
            else
            {
                if(gallType == GalleryType.Normal)
                    throw new Exception("스크립트 파싱에 실패하였습니다.");
            }

            Match frmData = Regex.Match(script, "formData \\+= \"&(.*)=(.*)\";");
            if (frmData.Success)
            {
                name = frmData.Groups[1].Value;
                value = frmData.Groups[2].Value;
            }
            else
                throw new Exception("스크립트 파싱에 실패하였습니다.");
        }
    }
}
