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
        internal static async Task<Tuple<string, ParameterStorage>> GetDeleteGalleryArticleParameterAsync(string script, GalleryType gallType)
        {
            return await Task.Run(() =>
            {
                string encCode = null;
                ParameterStorage storage = new ParameterStorage();

#if true
            Match encData = Regex.Match(script, "var _r = _d\\(\'(.*)\'\\)");
#else
                Match encData = null;
                throw new Exception("알 수 없는 오류가 발생했습니다.");
#endif
                if (encData.Success)
                {
                    encCode = encData.Groups[1].Value;
                }
                else
                {
                    if (gallType == GalleryType.Normal)
                        throw new Exception("자바스크립트 파싱에 실패하였습니다.");
                }

                Match frmData = Regex.Match(script, "formData \\+= \"&(.*)=(.*)\";");
                if (frmData.Success)
                    storage.Push(frmData.Groups[1].Value, frmData.Groups[2].Value);
                else
                    throw new Exception("자바스크립트 파싱에 실패하였습니다.");

                return new Tuple<string, ParameterStorage>(encCode, storage);
            });
        }

        internal static async Task<ParameterStorage> GetLoginParameterAsync(string script)
        {
            return await Task.Run(() =>
            {
                ParameterStorage retVal = new ParameterStorage();

                Match frmData = Regex.Match(script, "name:\"(.*?)\", value:\"(.*?)\"");
                if (frmData.Success)
                {
                    retVal.Push(frmData.Groups[1].Value, frmData.Groups[2].Value);
                }
                else
                    throw new Exception("스크립트 파싱에 실패하였습니다.");

                return retVal;
            });
        }
    }
}
