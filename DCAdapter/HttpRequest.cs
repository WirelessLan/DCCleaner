using System;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DCAdapter
{
    partial class DCConnector
    {
        /// <summary>
        /// 서버와의 통신에 사용되는 Fake User-Agent
        /// </summary>
        private readonly string _userAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.167 Safari/537.36";
        private readonly string _defaultAcceptString = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
        private readonly string _gallogDomain = "gallog.dcinside.com";
        private readonly string _gallogURL = "http://gallog.dcinside.com";
        private readonly string _galleryDomain = "gall.dcinside.com";
        private readonly string _galleryURL = "http://gall.dcinside.com";
        private readonly string _loginURL = "https://dcid.dcinside.com/join/member_check.php";
        private readonly string _loginPageURL = "https://dcid.dcinside.com/join/login.php";

        private async Task<LoginStatus> PostLoginAsync(string id, string pw, bool retry = true)
        {
            string gallUrl = "gallog";
            string gallogUrl = _gallogURL + "/" + id;
            LoginStatus status = new LoginStatus();

            // 로그인 페이지에 한번은 접속해야 정상 동작함
            string loginPageSrc = await GetLoginPageAsync(gallUrl);
            if (loginPageSrc.Contains("로그인 되었습니다"))
            {
                status = LoginStatus.Success;
                return status;
            }

            ParameterStorage LoginParams = await HtmlParser.GetLoginParameterAsync(loginPageSrc);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_loginURL);

            request.Accept = _defaultAcceptString;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = cookies;
            request.UserAgent = _userAgent;
            request.Proxy = null;
            request.Headers.Add("Upgrade-Insecure-Requests", "1");
            request.Referer = _loginPageURL + "?s_url=" + HttpUtility.UrlEncode(gallUrl);

            using (Stream stream = await request.GetRequestStreamAsync())
            using (StreamWriter streamWriter = new StreamWriter(stream))
            {
                string param = "s_url=" + HttpUtility.UrlEncode(gallUrl) + "&tieup=&url=&user_id=" + id + "&password=" + pw + "&x=0&y=0&ssl_chk=on&";
                param += LoginParams.ToString();
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
                                if (result.Contains(gallogUrl))
                                    status = LoginStatus.Success;
                                else if (retry)
                                    return await PostLoginAsync(id, pw, false);
                        }
                    }
                }
                else
                    status = LoginStatus.Unknown;
            }

            return status;
        }

        private async Task<string> GetLoginPageAsync(string gallUrl)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_loginPageURL + "?s_url=" + HttpUtility.UrlEncode(gallUrl));

            request.Accept = _defaultAcceptString;
            request.Method = "GET";
            request.Referer = gallUrl;
            request.CookieContainer = cookies;
            request.UserAgent = _userAgent;
            request.Proxy = null;

            using (WebResponse response = await request.GetResponseAsync())
            {
                if ((response as HttpWebResponse).StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        return reader.ReadToEnd();
                    }
                }
                else
                    throw new Exception("알 수 없는 오류입니다.");
            }
        }

        private async Task<string> GetGallogMainPageAsync(string id)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_gallogURL + "/" + id.ToLower());

            req.UserAgent = _userAgent;
            req.Method = "GET";
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
                            return readStream.ReadToEnd();
                        }
                    }
                }
            }

            throw new Exception("갤로그 페이지를 불러올 수 없습니다.");
        }
        
        private async Task<string> GetGallogListPageAsync(string user_id, int page, int cPage)
        {
            string _reqURL = _gallogURL + "/inc/_mainGallog.php";
            string referer = _gallogURL + "/" + user_id;

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_reqURL + "?page=" + page + "&rpage=" + cPage + "&gid=" + user_id + "&cid=");

            req.Host = _gallogDomain;
            req.Referer = referer;
            req.UserAgent = _userAgent;
            req.CookieContainer = cookies;
            req.Method = "GET";
            req.Proxy = null;

            using (WebResponse res = await req.GetResponseAsync())
            {
                if((res as HttpWebResponse).StatusCode == HttpStatusCode.OK)
                {
                    using (Stream stream = res.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
            
            throw new Exception("갤로그 글 리스트를 불러올 수 없습니다.");
        }
        
        private async Task<Tuple<string, int, int>>
            GetGalleryNicknameSearchPageAsync(string gall_id, GalleryType gallType, string nickname, int searchPos, int searchPage)
        {
            string searchPath = null;
            string basePath = null;

            if (gallType == GalleryType.Normal)
                searchPath = basePath = _galleryURL + "/board/lists/?id=" + gall_id;
            else if(gallType == GalleryType.Minor)
                searchPath = basePath = _galleryURL + "/mgallery/board/lists/?id=" + gall_id;

            if (searchPath == null)
                throw new Exception("예상치 못한 갤러리 형식입니다.");
            
            searchPath += "&page=" + searchPage.ToString();

            if (searchPos != 0)
                searchPath += "&search_pos=" + searchPos.ToString();

            searchPath += "&s_type=search_name&s_keyword=" + HttpUtility.UrlEncode(nickname);

            string referer = basePath;

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(searchPath);

            req.Accept = _defaultAcceptString;
            req.Method = "GET";
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Referer = referer;
            req.UserAgent = _userAgent;
            req.Host = _galleryDomain;
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
        
        private async Task<DeleteResult> PostDeleteGalleryArticleAsync(GalleryArticleDeleteParameter info, GalleryType gallType, int delay)
        {
            string pageHtml = await GetDeleteGalleryArticlePageAsync(info.GalleryId, info.ArticleID, null, gallType);
            Tuple<ParameterStorage, string> parseResult;
            ParameterStorage delete_Params = null;
            string lately_gallery = null;
            
            try
            {
                parseResult = await HtmlParser.GetDeleteArticleParameterAsync(pageHtml, gallType);
            }
            catch (Exception e)
            {
                return new DeleteResult(false, e.Message);
            }

            delete_Params = parseResult.Item1;
            lately_gallery = parseResult.Item2;

            if(gallType == GalleryType.Normal)
                cookies.Add(new Cookie("lately_cookie", HttpUtility.UrlEncode(lately_gallery)) { Domain="dcinside.com" });

            await Task.Delay(delay);

            string _reqURL = null;
            string referer = null;

            if (gallType == GalleryType.Normal)
            {
                _reqURL = _galleryURL + "/forms/delete_submit";
                referer = _galleryURL + "/board/delete/?id=" + info.GalleryId + "&no=" + info.ArticleID;
            }
            else if (gallType == GalleryType.Minor)
            {
                _reqURL = _galleryURL + "/mgallery/forms/delete_submit";
                referer = _galleryURL + "/mgallery/board/delete/?id=" + info.GalleryId + "&no=" + info.ArticleID;
            }

            if (_reqURL == null || referer == null)
                throw new Exception("예상치 못한 갤러리 형식입니다.");

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_reqURL);

            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Referer = referer;
            req.UserAgent = _userAgent;
            req.Host = _galleryDomain;
            req.Headers.Add("X-Requested-With", "XMLHttpRequest");

            using (Stream stream = await req.GetRequestStreamAsync())
            using (StreamWriter writer = new StreamWriter(stream))
            {
                string reqData = delete_Params.ToString();

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
        
        private async Task<DeleteResult> PostDeleteGalleryFlowArticleAsync(GalleryArticleDeleteParameter delParam, GalleryType gallType, int delay)
        {
            string pageHtml = await GetDeleteGalleryArticlePageAsync(delParam.GalleryId, delParam.ArticleID, null, gallType);
            Tuple<ParameterStorage, string> parseResult;
            ParameterStorage delete_Params = null;
            string lately_gallery = null;

            try
            {
                parseResult = await HtmlParser.GetDeleteFlowArticleParameterAsync(pageHtml, gallType);
            }
            catch (Exception e)
            {
                return new DeleteResult(false, e.Message);
            }

            delete_Params = parseResult.Item1;
            lately_gallery = parseResult.Item2;

            if (gallType == GalleryType.Normal)
                cookies.Add(new Cookie("lately_cookie", HttpUtility.UrlEncode(lately_gallery)) { Domain = "dcinside.com" });

            await Task.Delay(delay);

            string _reqURL = null;
            string referer = null;

            if (gallType == GalleryType.Normal)
            {
                _reqURL = _galleryURL + "/forms/delete_password_submit";
                referer = _galleryURL + "/board/delete/?id=" + delParam.GalleryId + "&no=" + delParam.ArticleID;
            }
            else if (gallType == GalleryType.Minor)
            {
                _reqURL = _galleryURL + "/mgallery/forms/delete_password_submit";
                referer = _galleryURL + "/mgallery/board/delete/?id=" + delParam.GalleryId + "&no=" + delParam.ArticleID;
            }

            if (_reqURL == null || referer == null)
                throw new Exception("예상치 못한 갤러리 형식입니다.");

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_reqURL);

            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Referer = referer;
            req.UserAgent = _userAgent;
            req.Host = _galleryDomain;
            req.Headers.Add("X-Requested-With", "XMLHttpRequest");

            using (Stream stream = await req.GetRequestStreamAsync())
            using (StreamWriter writer = new StreamWriter(stream))
            {
                string reqData = delete_Params.ToString();
                reqData += "&password=" + delParam.Password;

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
                                    return await PostDeleteMinorGalleryFlowArticleAsync(delParam, result.Replace("true||", ""));
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
        
        private async Task<DeleteResult> PostDeleteMinorGalleryFlowArticleAsync(GalleryArticleDeleteParameter delParam, string key)
        {
            string pageHtml = await GetDeleteGalleryArticlePageAsync(delParam.GalleryId, delParam.ArticleID, key, GalleryType.Minor);
            Tuple<ParameterStorage, string> parseResult;
            ParameterStorage delete_Params = null;

            try
            {
                parseResult = await HtmlParser.GetDeleteFlowArticleParameterAsync(pageHtml, GalleryType.Minor);
            }
            catch (Exception e)
            {
                return new DeleteResult(false, e.Message);
            }

            delete_Params = parseResult.Item1;

            string _reqURL = _galleryURL + "/mgallery/forms/delete_submit";
            string referer = _galleryURL + "/mgallery/board/delete/?id=" + delParam.GalleryId + "&no=" + delParam.ArticleID + "&key=" + key;

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_reqURL);

            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Referer = referer;
            req.UserAgent = _userAgent;
            req.Headers.Add("X-Requested-With", "XMLHttpRequest");

            using (Stream stream = await req.GetRequestStreamAsync())
            using (StreamWriter writer = new StreamWriter(req.GetRequestStream()))
            {
                string reqData = delete_Params.ToString();

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
        
        private async Task<DeleteResult> PostDeleteGalleryCommentAsync(GalleryCommentDeleteParameter param)
        {
            string commentPageHtml = await GetGalleryCommentViewPageAsync(param.GalleryId, param.ArticleId);
            string ci_t = null, check7 = null;
            
            try
            {
                check7 = await HtmlParser.GetDeleteCommentParameterAsync(commentPageHtml);
                ci_t = cookies.GetCookies(new Uri(_galleryURL))["ci_c"].Value;
            }
            catch (Exception e)
            {
                return new DeleteResult(false, e.Message);
            }

            string _reqURL = _galleryURL + "/forms/comment_delete_submit";
            string referer = _galleryURL + "/board/view/?id=" + param.GalleryId + "&no=" + param.ArticleId;

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_reqURL);

            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Referer = referer;
            req.UserAgent = _userAgent;
            req.Host = _galleryDomain;
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
        
        private async Task<DeleteResult> PostDeleteGallogArticleAsync(GallogArticleDeleteParameter param, int delay)
        {
            string _reqURL = _gallogURL + "/inc/_deleteArticle.php";
            string referer = _gallogURL + "/inc/_deleteLog.php?gid=" + param.UserId;

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_reqURL);

            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Referer = referer;
            req.UserAgent = _userAgent;
            req.Host = _gallogDomain;
            req.Headers.Add("Upgrade-Insecure-Requests", "1");

            using (Stream stream = await req.GetRequestStreamAsync())
            using (StreamWriter writer = new StreamWriter(stream))
            {
                string reqData = "rb=&dTp=1&gid=" + param.UserId + "&cid=" + param.GalleryNo +
                    "&pno=" + param.ArticleId + "&no=" + param.ArticleId + "&logNo=" + param.LogNo + "&id=" + param.GalleryId +
                    "&nate=&dcc_key=" + param.DCCKey
                    + (param.AdditionalParameter["dcc_key"] != null ? "" : ("&" + param.AdditionalParameter.ToString()));
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
        
        private async Task<DeleteResult> PostDeleteGallogCommentAsync(GallogCommentDeleteParameter param, int delay)
        {
            string _reqURL = _gallogURL + "/inc/_deleteRepOk.php";
            string referer = _gallogURL + "/inc/_deleteLogRep.php?gid=" + param.UserId + "&cid=" 
                            + param.GalleryNo + "&id=" + param.GalleryId + "&no=" + param.ArticleId + "&logNo=" + param.LogNo + "&rpage=";
            
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_reqURL);

            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Referer = referer;
            req.UserAgent = _userAgent;
            req.Host = _gallogDomain;
            req.Headers.Add("Upgrade-Insecure-Requests", "1");

            using (Stream stream = await req.GetRequestStreamAsync())
            using (StreamWriter writer = new StreamWriter(stream))
            {
                string reqData = "rb=&dTp=1&gid=" + param.UserId + "&cid=" + param.GalleryNo + "&page=&pno=" +
                    "&no=" + param.ArticleId + "&c_no=" + param.CommentId + "&logNo=" + param.LogNo + "&id=" + param.GalleryId +
                    "&nate=&";
                reqData += param.AdditionalParameter.ToString();
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
        
        private async Task<string> GetGalleryCommentViewPageAsync(string gallid, string articleid)
        {
            string _reqURL = _galleryURL + "/board/comment_view/";
            string referer = _galleryURL + "/board/lists/?id=" + gallid;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_reqURL + "?id=" + gallid + "&no=" + articleid);
            
            req.Accept = _defaultAcceptString;
            req.Method = "GET";
            req.UserAgent = _userAgent;
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Headers.Add("Upgrade-Insecure-Requests", "1");
            req.Host = _galleryDomain;
            req.Referer = referer;

            using (var res = await req.GetResponseAsync())
            {
                if ((res as HttpWebResponse).StatusCode == HttpStatusCode.OK)
                {
                    using (Stream stream = res.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }

            throw new Exception("글을 불러올 수 없습니다.");
        }
        
        private async Task<string> GetDeleteGalleryArticlePageAsync(string gallId, string no, string key, GalleryType gallType)
        {
            string _reqURL = null;
            string referer = null; 

            if(gallType == GalleryType.Normal)
            {
                _reqURL = _galleryURL + "/board/delete/" + "?id=" + gallId + "&no=" + no;
                referer = _galleryURL + "/board/view/?id=" + gallId + "&no=" + no;
            }
            else if(gallType == GalleryType.Minor)
            {
                _reqURL = _galleryURL + "/mgallery/board/delete/" + "?id=" + gallId + "&no=" + no;
                referer = _galleryURL + "/mgallery/board/view/?id=" + gallId + "&no=" + no;
            }

            if (_reqURL == null || referer == null)
                throw new Exception("예상치 못한 갤러리 형식입니다.");

            if(!string.IsNullOrWhiteSpace(key))
            {
                _reqURL += "&key=" + key;
            }

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_reqURL);

            req.Method = "GET";
            req.Accept = _defaultAcceptString;
            req.UserAgent = _userAgent;
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Headers.Add("Upgrade-Insecure-Requests", "1");
            req.Host = _galleryDomain;
            req.Referer = referer;

            using (WebResponse res = await req.GetResponseAsync())
            {
                if((res as HttpWebResponse).StatusCode == HttpStatusCode.OK)
                {
                    using(Stream stream = res.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }

            throw new Exception("갤로그 페이지를 불러올 수 없습니다.");
        }
        
        private string GetDeleteGallogArticlePageAsync(string id, string gall_no, string art_id, string logNo, ref CookieContainer cookies)
        {
            string _reqURL = _gallogURL + "/inc/_deleteLog.php";
            string referer = _gallogURL + "/inc/_mainGallog.php?gid=" + id;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_reqURL + "?gid=" + id + "&cid=" + gall_no + "&pno=" + art_id + "&logNo=" + logNo + "&mode=gMdf");

            req.Method = "GET";
            req.UserAgent = _userAgent;
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Headers.Add("Upgrade-Insecure-Requests", "1");
            req.Host = _gallogDomain;
            req.Referer = referer;

            using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
            {
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream stream = res.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }

            throw new Exception("갤로그 페이지를 불러올 수 없습니다.");
        }
        
        private async Task<string> GetDeleteGallogArticlePageAsync(string url, string id)
        {
            string referer = _gallogURL + "/inc/_mainGallog.php?gid=" + id;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            req.Method = "GET";
            req.UserAgent = _userAgent;
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Headers.Add("Upgrade-Insecure-Requests", "1");
            req.Host = _gallogDomain;
            req.Referer = referer;

            using (WebResponse res = await req.GetResponseAsync())
            {
                if ((res as HttpWebResponse).StatusCode == HttpStatusCode.OK)
                {
                    using (Stream stream = res.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }

            throw new Exception("갤로그 페이지를 불러올 수 없습니다.");
        }
        
        private string GetDeleteGallogCommentPageAsync(string id, string art_id, string commentId, string logNo, ref CookieContainer cookies)
        {
            string _reqURL = _gallogURL + "/inc/_deleteLogRep.php";
            string referer = _gallogURL + "/inc/_mainGallog.php?gid=" + id;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_reqURL + "?gid=" + id + "&cid=&id=&no=" + art_id + "&c_no=" + commentId + "&logNo=" + logNo);

            req.Method = "GET";
            req.UserAgent = _userAgent;
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Headers.Add("Upgrade-Insecure-Requests", "1");
            req.Host = _gallogDomain;
            req.Referer = referer;

            using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
            {
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream stream = res.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }

            throw new Exception("갤로그 페이지를 불러올 수 없습니다.");
        }
        
        private async Task<string> GetDeleteGallogCommentPageAsync(string url, string user_id)
        {
            string referer = _gallogURL + "/inc/_mainGallog.php?gid=" + user_id;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            req.Method = "GET";
            req.UserAgent = _userAgent;
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Headers.Add("Upgrade-Insecure-Requests", "1");
            req.Host = _gallogDomain;
            req.Referer = referer;

            using (WebResponse res = await req.GetResponseAsync())
            {
                if ((res as HttpWebResponse).StatusCode == HttpStatusCode.OK)
                {
                    using (Stream stream = res.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }

            throw new Exception("갤로그 페이지를 불러올 수 없습니다.");
        }
    }
}
