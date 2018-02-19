using System;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DCAdapter
{
    partial class DCConnector
    {
        /// <summary>
        /// 서버와의 통신에 사용되는 Fake User-Agent
        /// </summary>
        static string UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.167 Safari/537.36";
        
        private async Task<LoginStatus> RequestLogin(string id, string pw, bool retry = true)
        {
            string gallUrl = "gallog";
            string gallogUrl = "http://gallog.dcinside.com/" + id;
            LoginStatus status = new LoginStatus();

            // 로그인 페이지에 한번은 접속해야 정상 동작함
            Dictionary<string, string> LoginParams = await RequestLoginSource(gallUrl);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://dcid.dcinside.com/join/member_check.php");

            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = cookies;
            request.UserAgent = UserAgent;
            request.Proxy = null;
            request.Headers.Add("Upgrade-Insecure-Requests", "1");
            request.Referer = "https://dcid.dcinside.com/join/login.php?s_url=" + HttpUtility.UrlEncode(gallUrl);

            using (Stream stream = await request.GetRequestStreamAsync())
            using (StreamWriter streamWriter = new StreamWriter(stream))
            {
                string param = "s_url=" + HttpUtility.UrlEncode(gallUrl) + "&tieup=&url=&user_id=" + id + "&password=" + pw + "&x=0&y=0&ssl_chk=on&";
                foreach (KeyValuePair<string, string> kv in LoginParams)
                {
                    param += HttpUtility.UrlEncode(kv.Key) + "=" + HttpUtility.UrlEncode(kv.Value) + "&";
                }

                param = param.Substring(0, param.Length - 1);

                await streamWriter.WriteAsync(param);
            }

            using (WebResponse response = await request.GetResponseAsync())
            {
                if ((response as HttpWebResponse).StatusCode == HttpStatusCode.OK)
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (StreamReader readStream = new StreamReader(responseStream, Encoding.UTF8))
                        {
                            string result = readStream.ReadToEnd();

                            if (result.Contains("등록된 아이디가 아닙니다."))
                                status = LoginStatus.IDError;
                            else if (result.Contains("비밀번호가 틀렸습니다."))
                                status = LoginStatus.PasswordError;
                            else if (result.Contains("아이디 또는 비밀번호가 잘못되었습니다."))
                                status = LoginStatus.ErrorBoth;
                            else if (result.Contains("잘못된 접근입니다"))
                                status = LoginStatus.Unknown;
                            else
                            {
                                if (result.Contains(gallogUrl))
                                    status = LoginStatus.Success;
                                else if (retry)
                                {
                                    return await RequestLogin(id, pw, false);
                                }
                            }
                        }
                    }
                }
                else
                {
                    status = LoginStatus.Unknown;
                }
            }

            return status;
        }

        private async Task<Dictionary<string, string>> RequestLoginSource(string gallUrl)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://dcid.dcinside.com/join/login.php?s_url=" + HttpUtility.UrlEncode(gallUrl));

            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
            request.Method = "GET";
            request.Referer = gallUrl;
            request.CookieContainer = cookies;
            request.UserAgent = UserAgent;
            request.Proxy = null;

            using (WebResponse response = await request.GetResponseAsync())
            {
                if ((response as HttpWebResponse).StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string result = reader.ReadToEnd();
                        if ((response as HttpWebResponse).Cookies["userAgent"] != null)
                            UserAgent = HttpUtility.UrlDecode((response as HttpWebResponse).Cookies["userAgent"].Value);

                        return HtmlParser.GetLoginParameter(result);
                    }
                }
                else
                    throw new Exception("알 수 없는 오류입니다.");
            }
        }

        private async Task<string> RequestGallogMainSource(string id)
        {
            const string _gallogURL = "http://gallog.dcinside.com/";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_gallogURL + id);

            req.UserAgent = UserAgent;
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Referer = _gallogURL;

            using (WebResponse res = await req.GetResponseAsync())
            {
                if ((res as HttpWebResponse).StatusCode == HttpStatusCode.OK)
                {
                    using (Stream responseStream = res.GetResponseStream())
                    {
                        using (StreamReader readStream = new StreamReader(responseStream, Encoding.UTF8))
                        {
                            string result = readStream.ReadToEnd();

                            return result;
                        }
                    }
                }
            }

            throw new Exception("갤로그 페이지를 불러올 수 없습니다.");
        }
        
        private async Task<string> RequestGallogMainListSource(string user_id, int page, int cPage)
        {
            const string _reqURL = "http://gallog.dcinside.com/inc/_mainGallog.php";
            string referer = "http://gallog.dcinside.com/" + user_id;

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_reqURL + "?page=" + page + "&rpage=" + cPage + "&gid=" + user_id + "&cid=");

            req.Referer = referer;
            req.Host = "gallog.dcinside.com";
            req.UserAgent = UserAgent;
            req.CookieContainer = cookies;
            req.Headers.Add("Upgrade-Insecure-Requests", "1");
            req.Method = "GET";
            req.Proxy = null;
            req.Timeout = 10 * 1000; // 20초 timeout
            req.ServicePoint.ConnectionLeaseTimeout = 10 * 1000;
            req.ServicePoint.MaxIdleTime = 10 * 1000;

            using (WebResponse res = await req.GetResponseAsync())
            {
                if((res as HttpWebResponse).StatusCode == HttpStatusCode.OK)
                {
                    using (Stream stream = res.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string result = reader.ReadToEnd();
                            return result;
                        }
                    }
                }
            }
            
            throw new Exception("갤로그 글 리스트를 불러올 수 없습니다.");
        }
        
        private async Task<Tuple<string, int, int>>
            RequestGalleryNicknameSearchSource(string gall_id, GalleryType gallType, string nickname, int searchPos, int searchPage)
        {
            string searchPath = null;
            string basePath = null;

            if (gallType == GalleryType.Normal)
                searchPath = basePath = "http://gall.dcinside.com/board/lists/?id=" + gall_id;
            else if(gallType == GalleryType.Minor)
                searchPath = basePath = "http://gall.dcinside.com/mgallery/board/lists/?id=" + gall_id;

            if (searchPath == null)
                throw new Exception("예상치 못한 갤러리 형식입니다.");
            
            searchPath += "&page=" + searchPage.ToString();

            if (searchPos != 0)
                searchPath += "&search_pos=" + searchPos.ToString();

            searchPath += "&s_type=search_name&s_keyword=" + HttpUtility.UrlEncode(nickname);

            const string host = "gall.dcinside.com";
            string referer = basePath;

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(searchPath);

            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
            req.Method = "GET";
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Referer = referer;
            req.UserAgent = UserAgent;
            req.Host = host;
            req.Headers.Add("Upgrade-Insecure-Requests", "1");

            using (var res = await req.GetResponseAsync())
            {
                if ((res as HttpWebResponse).StatusCode == HttpStatusCode.OK)
                {
                    using (Stream stream = res.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string result = reader.ReadToEnd();

                            if(result.Contains("해당 갤러리는 존재하지 않습니다"))
                            {
                                throw new Exception("해당 갤러리는 존재하지 않습니다.");
                            }
                            
                            if(string.IsNullOrWhiteSpace(result))
                            {
                                throw new Exception("알 수 없는 오류입니다.");
                            }

                            return new Tuple<string, int, int>(result, searchPos, searchPage);
                        }
                    }
                }
            }

            throw new Exception("알 수 없는 오류입니다.");
        }
        
        private async Task<DeleteResult> RequestDeleteGalleryArticle(GalleryArticleDeleteParameters info, GalleryType gallType, int delay)
        {
            string pageHtml = await RequestDeleteGalleryArticleSource(info.GalleryId, info.ArticleID, null, gallType);
            Dictionary<string, string> delete_params = null;
            string lately_gallery = null;
            
            try
            {
                HtmlParser.GetDeleteArticleParameters(pageHtml, gallType, out delete_params, out lately_gallery);
            }
            catch (Exception e)
            {
                return new DeleteResult(false, e.Message);
            }

            if(gallType == GalleryType.Normal)
                cookies.Add(new Cookie("lately_cookie", HttpUtility.UrlEncode(lately_gallery)) { Domain="dcinside.com" });

            Thread.Sleep(delay);

            string _reqURL = null;
            const string host = "gall.dcinside.com";
            string referer = null;

            if (gallType == GalleryType.Normal)
            {
                _reqURL = "http://gall.dcinside.com/forms/delete_submit";
                referer = "http://gall.dcinside.com/board/delete/?id=" + info.GalleryId + "&no=" + info.ArticleID;
            }
            else if (gallType == GalleryType.Minor)
            {
                _reqURL = "http://gall.dcinside.com/mgallery/forms/delete_submit";
                referer = "http://gall.dcinside.com/mgallery/board/delete/?id=" + info.GalleryId + "&no=" + info.ArticleID;
            }

            if (_reqURL == null || referer == null)
                throw new Exception("예상치 못한 갤러리 형식입니다.");

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_reqURL);

            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Referer = referer;
            req.UserAgent = UserAgent;
            req.Host = host;
            req.Headers.Add("X-Requested-With", "XMLHttpRequest");

            using (Stream stream = await req.GetRequestStreamAsync())
            using (StreamWriter writer = new StreamWriter(stream))
            {
                string reqData = null;

                foreach (KeyValuePair<string, string> kv in delete_params)
                {
                    string header = HttpUtility.UrlEncode(kv.Key);
                    string value = HttpUtility.UrlEncode(kv.Value);

                    reqData += header + "=" + value + "&";
                }
                reqData = reqData.Substring(0, reqData.Length - 1);

                if (reqData == null)
                    throw new Exception("예상치 못한 갤러리 형식입니다.");

                await writer.WriteAsync(reqData);
            }

            using (var res = await req.GetResponseAsync())
            {
                if ((res as HttpWebResponse).StatusCode == HttpStatusCode.OK)
                {
                    using (Stream stream = res.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string result = reader.ReadToEnd();

                            if(result == "true||" + info.GalleryId)
                            {
                                return new DeleteResult(true, "");
                            }
                            else if(result.StartsWith("false||"))
                            {
                                return new DeleteResult(false, result.Replace("false||", ""));
                            }
                            else
                            {
                                return new DeleteResult(false, "알 수 없는 오류입니다.");
                            }
                        }
                    }
                }
            }

            return new DeleteResult(false, "알 수 없는 오류입니다.");
        }
        
        private async Task<DeleteResult> RequestDeleteGalleryFlowArticle(GalleryArticleDeleteParameters delParam, GalleryType gallType, int delay)
        {
            string pageHtml = await RequestDeleteGalleryArticleSource(delParam.GalleryId, delParam.ArticleID, null, gallType);
            Dictionary<string, string> delete_params = null;
            string lately_gallery = null;

            try
            {
                HtmlParser.GetDeleteFlowArticleParameters(pageHtml, gallType, out delete_params, out lately_gallery);
            }
            catch (Exception e)
            {
                return new DeleteResult(false, e.Message);
            }
            
            if (gallType == GalleryType.Normal)
                cookies.Add(new Cookie("lately_cookie", HttpUtility.UrlEncode(lately_gallery)) { Domain = "dcinside.com" });

            Thread.Sleep(delay);

            string _reqURL = null;
            const string host = "gall.dcinside.com";
            string referer = null;

            if (gallType == GalleryType.Normal)
            {
                _reqURL = "http://gall.dcinside.com/forms/delete_password_submit";
                referer = "http://gall.dcinside.com/board/delete/?id=" + delParam.GalleryId + "&no=" + delParam.ArticleID;
            }
            else if (gallType == GalleryType.Minor)
            {
                _reqURL = "http://gall.dcinside.com/mgallery/forms/delete_password_submit";
                referer = "http://gall.dcinside.com/mgallery/board/delete/?id=" + delParam.GalleryId + "&no=" + delParam.ArticleID;
            }

            if (_reqURL == null || referer == null)
                throw new Exception("예상치 못한 갤러리 형식입니다.");

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_reqURL);

            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Referer = referer;
            req.UserAgent = UserAgent;
            req.Host = host;
            req.Headers.Add("X-Requested-With", "XMLHttpRequest");

            using (Stream stream = await req.GetRequestStreamAsync())
            using (StreamWriter writer = new StreamWriter(stream))
            {
                string reqData = null;
                
                foreach (KeyValuePair<string, string> kv in delete_params)
                {
                    reqData += HttpUtility.UrlEncode(kv.Key) + "=" + HttpUtility.UrlEncode(kv.Value) + "&";
                }
                reqData += "password=" + delParam.Password;

                await writer.WriteAsync(reqData);
            }

            using (var res = await req.GetResponseAsync())
            {
                if ((res as HttpWebResponse).StatusCode == HttpStatusCode.OK)
                {
                    using (Stream stream = res.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string result = reader.ReadToEnd();

                            if (result.StartsWith("true||"))
                            {
                                if (gallType == GalleryType.Normal)
                                    return new DeleteResult(true, "");
                                else
                                    return await RequestDeleteMinorGalleryFlowArticle(delParam, result.Replace("true||", ""));
                            }
                            else if (result == "false||비밀번호 인증에 실패하였습니다. 다시 시도해주세요" || 
                                result == "false|| 비밀번호가 맞지 않습니다. 다시 시도해주세요" ||
                                result == "false||비밀번호가 잘못되었습니다. 다시 시도해주세요")
                            {
                                return new DeleteResult(false, "비밀번호가 다릅니다.");
                            }
                            else
                            {
                                return new DeleteResult(false, "알 수 없는 오류입니다.");
                            }
                        }
                    }
                }
            }

            return new DeleteResult(false, "알 수 없는 오류입니다.");
        }
        
        private async Task<DeleteResult> RequestDeleteMinorGalleryFlowArticle(GalleryArticleDeleteParameters delParam, string key)
        {
            string pageHtml = await RequestDeleteGalleryArticleSource(delParam.GalleryId, delParam.ArticleID, key, GalleryType.Minor);
            Dictionary<string, string> delete_params = null;
            string nulString = null;

            try
            {
                HtmlParser.GetDeleteFlowArticleParameters(pageHtml, GalleryType.Minor, out delete_params, out nulString);
            }
            catch (Exception e)
            {
                return new DeleteResult(false, e.Message);
            }

            string _reqURL = "http://gall.dcinside.com/mgallery/forms/delete_submit";
            string referer = "http://gall.dcinside.com/mgallery/board/delete/?id=" + delParam.GalleryId + "&no=" + delParam.ArticleID + "&key=" + key;

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_reqURL);

            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Referer = referer;
            req.UserAgent = UserAgent;
            req.Headers.Add("X-Requested-With", "XMLHttpRequest");

            using (Stream stream = await req.GetRequestStreamAsync())
            using (StreamWriter writer = new StreamWriter(req.GetRequestStream()))
            {
                string reqData = null;

                foreach (KeyValuePair<string, string> kv in delete_params)
                {
                    reqData += HttpUtility.UrlEncode(kv.Key) + "=" + HttpUtility.UrlEncode(kv.Value) + "&";
                }
                reqData = reqData.Substring(0, reqData.Length - 1);

                await writer.WriteAsync(reqData);
            }

            using (var res = await req.GetResponseAsync())
            {
                if ((res as HttpWebResponse).StatusCode == HttpStatusCode.OK)
                {
                    using (Stream stream = res.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string result = reader.ReadToEnd();

                            if (result.StartsWith("true||" + delParam.GalleryId))
                            {
                                return new DeleteResult(true, "");
                            }
                            else if (result == "false||비밀번호 인증에 실패하였습니다. 다시 시도해주세요" ||
                                result == "false|| 비밀번호가 맞지 않습니다. 다시 시도해주세요" ||
                                result == "false||비밀번호가 잘못되었습니다. 다시 시도해주세요")
                            {
                                return new DeleteResult(false, "비밀번호가 다릅니다.");
                            }
                            else
                            {
                                return new DeleteResult(false, "알 수 없는 오류입니다.");
                            }
                        }
                    }
                }
            }

            return new DeleteResult(false, "알 수 없는 오류입니다.");
        }
        
        private async Task<DeleteResult> RequestDeleteGalleryComment(GalleryCommentDeleteParameters param)
        {
            string commentPageHtml = await RequestGalleryCommentViewSource(param.GalleryId, param.ArticleId);
            string ci_t = null, check7 = null;
            
            try
            {
                HtmlParser.GetDeleteCommentParameters(commentPageHtml, out check7);
                ci_t = cookies.GetCookies(new Uri("http://gall.dcinside.com/"))["ci_c"].Value;
            }
            catch (Exception e)
            {
                return new DeleteResult(false, e.Message);
            }

            const string _reqURL = "http://gall.dcinside.com/forms/comment_delete_submit";
            const string host = "gall.dcinside.com";
            string referer = "http://gall.dcinside.com/board/view/?id=" + param.GalleryId + "&no=" + param.ArticleId;

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_reqURL);

            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Referer = referer;
            req.UserAgent = UserAgent;
            req.Host = host;
            req.Headers.Add("X-Requested-With", "XMLHttpRequest");

            using (Stream stream = await req.GetRequestStreamAsync())
            using (StreamWriter writer = new StreamWriter(stream))
            {
                string reqData = "ci_t=" + ci_t + "&id=" + param.GalleryId + "&no=" + param.ArticleId + "&p_no=" + param.ArticleId +
                                "&re_no=" + param.CommentId + "&best_origin=&check_7=" + check7;
                await writer.WriteAsync(reqData);
            }

            using (var res = await req.GetResponseAsync())
            {
                if ((res as HttpWebResponse).StatusCode == HttpStatusCode.OK)
                {
                    using (Stream stream = res.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string result = reader.ReadToEnd();

                            if (result == "")
                            {
                                return new DeleteResult(true, "");
                            }
                            else if(result == "false||댓글내역이 없습니다")
                            {
                                return new DeleteResult(false, "이미 삭제된 리플입니다.");
                            }
                            else
                            {
                                return new DeleteResult(false, "알 수 없는 오류입니다.");
                            }
                        }
                    }
                }
            }

            return new DeleteResult(false, "알 수 없는 오류입니다.");
        }
        
        private async Task<DeleteResult> RequestDeleteGallogArticle(GallogArticleDeleteParameters param, int delay)
        {
            const string _reqURL = "http://gallog.dcinside.com/inc/_deleteArticle.php";
            const string host = "gallog.dcinside.com";
            string referer = "http://gallog.dcinside.com/inc/_deleteLog.php?gid=" + param.UserId;

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_reqURL);

            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Referer = referer;
            req.UserAgent = UserAgent;
            req.Host = host;
            req.Headers.Add("Upgrade-Insecure-Requests", "1");

            using (Stream stream = await req.GetRequestStreamAsync())
            using (StreamWriter writer = new StreamWriter(stream))
            {
                string reqData = "rb=&dTp=1&gid=" + param.UserId + "&cid=" + param.GalleryNo +
                    "&pno=" + param.ArticleId + "&no=" + param.ArticleId + "&logNo=" + param.LogNo + "&id=" + param.GalleryId +
                    "&nate=&dcc_key=" + param.DCCKey
                    + (param.AdditionalKey == "dcc_key" ? "" : ("&" + HttpUtility.UrlEncode(param.AdditionalKey) + "=" + HttpUtility.UrlEncode(param.AdditionalValue)));
                await writer.WriteAsync(reqData);
            }

            using (var res = await req.GetResponseAsync())
            {
                if ((res as HttpWebResponse).StatusCode == HttpStatusCode.OK)
                {
                    using (Stream stream = res.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string result = reader.ReadToEnd();

                            // 성공
                            if (result.Contains("GidMgr.resetGalleryData(2);"))
                            {
                                return new DeleteResult(true, "");
                            }
                            else
                            {
                                return new DeleteResult(false, "알 수 없는 오류입니다.");
                            }
                        }
                    }
                }
            }

            return new DeleteResult(false, "알 수 없는 오류입니다.");
        }
        
        private async Task<DeleteResult> RequestDeleteGallogComment(GallogCommentDeleteParameters param, int delay)
        {
            const string _reqURL = "http://gallog.dcinside.com/inc/_deleteRepOk.php";
            const string host = "gallog.dcinside.com";
            string referer = "http://gallog.dcinside.com/inc/_deleteLogRep.php?gid=" + param.UserId + "&cid=" 
                            + param.GalleryNo + "&id=" + param.GalleryId + "&no=" + param.ArticleId + "&logNo=" + param.LogNo + "&rpage=";
            
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_reqURL);

            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Referer = referer;
            req.UserAgent = UserAgent;
            req.Host = host;
            req.Headers.Add("Upgrade-Insecure-Requests", "1");

            using (Stream stream = await req.GetRequestStreamAsync())
            using (StreamWriter writer = new StreamWriter(stream))
            {
                string reqData = "rb=&dTp=1&gid=" + param.UserId + "&cid=" + param.GalleryNo + "&page=&pno=" +
                    "&no=" + param.ArticleId + "&c_no=" + param.CommentId + "&logNo=" + param.LogNo + "&id=" + param.GalleryId +
                    "&nate=&" + HttpUtility.UrlEncode(param.AdditionalKey) + "=" + HttpUtility.UrlEncode(param.AdditionalValue);
                await writer.WriteAsync(reqData);
            }

            using (var res = await req.GetResponseAsync())
            {
                if ((res as HttpWebResponse).StatusCode == HttpStatusCode.OK)
                {
                    using (Stream stream = res.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string result = reader.ReadToEnd();

                            // 성공
                            if(result.Contains("GidMgr.resetGalleryData(2);"))
                            {
                                return new DeleteResult(true, "");
                            }
                            else
                            {
                                return new DeleteResult(false, "알 수 없는 오류입니다.");
                            }
                        }
                    }
                }
            }

            return new DeleteResult(false, "알 수 없는 오류입니다.");
        }
        
        private async Task<string> RequestGalleryCommentViewSource(string gallid, string articleid)
        {
            const string _reqURL = "http://gall.dcinside.com/board/comment_view/";
            string referer = "http://gall.dcinside.com/board/lists/?id=" + gallid;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_reqURL + "?id=" + gallid + "&no=" + articleid);
            
            req.Method = "GET";
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
            req.UserAgent = UserAgent;
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Headers.Add("Upgrade-Insecure-Requests", "1");
            req.Host = "gall.dcinside.com";
            req.Referer = referer;

            using (var res = await req.GetResponseAsync())
            {
                if ((res as HttpWebResponse).StatusCode == HttpStatusCode.OK)
                {
                    using (Stream stream = res.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string result = reader.ReadToEnd();

                            return result;
                        }
                    }
                }
            }

            throw new Exception("글을 불러올 수 없습니다.");
        }
        
        private async Task<string> RequestDeleteGalleryArticleSource(string gallId, string no, string key, GalleryType gallType)
        {
            string _reqURL = null;
            string referer = null; 

            if(gallType == GalleryType.Normal)
            {
                _reqURL = "http://gall.dcinside.com/board/delete/" + "?id=" + gallId + "&no=" + no;
                referer = "http://gall.dcinside.com/board/view/?id=" + gallId + "&no=" + no;
            }
            else if(gallType == GalleryType.Minor)
            {
                _reqURL = "http://gall.dcinside.com/mgallery/board/delete/" + "?id=" + gallId + "&no=" + no;
                referer = "http://gall.dcinside.com/mgallery/board/view/?id=" + gallId + "&no=" + no;
            }

            if (_reqURL == null || referer == null)
                throw new Exception("예상치 못한 갤러리 형식입니다.");

            if(!string.IsNullOrWhiteSpace(key))
            {
                _reqURL += "&key=" + key;
            }

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_reqURL);

            req.Method = "GET";
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
            req.UserAgent = UserAgent;
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Headers.Add("Upgrade-Insecure-Requests", "1");
            req.Host = "gall.dcinside.com";
            req.Referer = referer;

            using (WebResponse res = await req.GetResponseAsync())
            {
                if((res as HttpWebResponse).StatusCode == HttpStatusCode.OK)
                {
                    using(Stream stream = res.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string result = reader.ReadToEnd();

                            return result;
                        }
                    }
                }
            }

            throw new Exception("갤로그 페이지를 불러올 수 없습니다.");
        }
        
        private static string RequestDeleteGallogArticlePage(string id, string gall_no, string art_id, string logNo, ref CookieContainer cookies)
        {
            const string _reqURL = "http://gallog.dcinside.com/inc/_deleteLog.php";
            string referer = "http://gallog.dcinside.com/inc/_mainGallog.php?gid=" + id;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_reqURL + "?gid=" + id + "&cid=" + gall_no + "&pno=" + art_id + "&logNo=" + logNo + "&mode=gMdf");

            req.Method = "GET";
            req.UserAgent = UserAgent;
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Headers.Add("Upgrade-Insecure-Requests", "1");
            req.Host = "gallog.dcinside.com";
            req.Referer = referer;

            using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
            {
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream stream = res.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string result = reader.ReadToEnd();

                            return result;
                        }
                    }
                }
            }

            throw new Exception("갤로그 페이지를 불러올 수 없습니다.");
        }
        
        private async Task<string> RequestDeleteGallogArticleSource(string url, string id)
        {
            string referer = "http://gallog.dcinside.com/inc/_mainGallog.php?gid=" + id;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            req.Method = "GET";
            req.UserAgent = UserAgent;
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Headers.Add("Upgrade-Insecure-Requests", "1");
            req.Host = "gallog.dcinside.com";
            req.Referer = referer;

            using (WebResponse res = await req.GetResponseAsync())
            {
                if ((res as HttpWebResponse).StatusCode == HttpStatusCode.OK)
                {
                    using (Stream stream = res.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string result = reader.ReadToEnd();

                            return result;
                        }
                    }
                }
            }

            throw new Exception("갤로그 페이지를 불러올 수 없습니다.");
        }
        
        private static string RequestDeleteGallogCommentPage(string id, string art_id, string commentId, string logNo, ref CookieContainer cookies)
        {
            const string _reqURL = "http://gallog.dcinside.com/inc/_deleteLogRep.php";
            string referer = "http://gallog.dcinside.com/inc/_mainGallog.php?gid=" + id;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_reqURL + "?gid=" + id + "&cid=&id=&no=" + art_id + "&c_no=" + commentId + "&logNo=" + logNo);

            req.Method = "GET";
            req.UserAgent = UserAgent;
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Headers.Add("Upgrade-Insecure-Requests", "1");
            req.Host = "gallog.dcinside.com";
            req.Referer = referer;

            using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
            {
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream stream = res.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string result = reader.ReadToEnd();

                            return result;
                        }
                    }
                }
            }

            throw new Exception("갤로그 페이지를 불러올 수 없습니다.");
        }
        
        private async Task<string> RequestDeleteGallogCommentSource(string url, string user_id)
        {
            string referer = "http://gallog.dcinside.com/inc/_mainGallog.php?gid=" + user_id;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            req.Method = "GET";
            req.UserAgent = UserAgent;
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Headers.Add("Upgrade-Insecure-Requests", "1");
            req.Host = "gallog.dcinside.com";
            req.Referer = referer;

            using (WebResponse res = await req.GetResponseAsync())
            {
                if ((res as HttpWebResponse).StatusCode == HttpStatusCode.OK)
                {
                    using (Stream stream = res.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string result = reader.ReadToEnd();

                            return result;
                        }
                    }
                }
            }

            throw new Exception("갤로그 페이지를 불러올 수 없습니다.");
        }
    }
}
