using System;
using System.Net;
using System.Web;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace DCAdapter
{
    partial class DCConnector
    {
        /// <summary>
        /// 서버와의 통신에 사용되는 Fake User-Agent
        /// </summary>
        private readonly string _userAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.167 Safari/537.36";
        private readonly string _defaultAcceptString = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
        private readonly string _gallogURL = "http://gallog.dcinside.com";
        private readonly string _galleryURL = "http://gall.dcinside.com";
        private readonly string _loginURL = "https://dcid.dcinside.com/join/member_check.php";
        private readonly string _loginPageURL = "https://dcid.dcinside.com/join/login.php";
        private readonly int _defaultTimeout = 10000;

        private async Task<LoginStatus> PostLoginAsync(string id, string pw, bool retry = true)
        {
            string gallUrl = "gallog";
            string gallogUrl = _gallogURL + "/" + id;
            string referer = _loginPageURL + "?s_url=" + HttpUtility.UrlEncode(gallUrl);
            LoginStatus status = new LoginStatus();

            // 로그인 페이지에 한번은 접속해야 정상 동작함
            string loginPageSrc = await GetLoginPageAsync(gallUrl);
            if (loginPageSrc.Contains("로그인 되었습니다"))
            {
                status = LoginStatus.Success;
                return status;
            }

            ParameterStorage LoginParams = await HtmlParser.GetLoginParameterAsync(loginPageSrc);
            
            using (HttpClientHandler handler = new HttpClientHandler() { CookieContainer = cookies })
            using (HttpClient client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Accept.ParseAdd(_defaultAcceptString);
                client.DefaultRequestHeaders.Referrer = new Uri(referer);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
                client.Timeout = new TimeSpan(0, 0, 0, 0, _defaultTimeout);

                string param = "s_url=" + HttpUtility.UrlEncode(gallUrl) + "&tieup=&url=&user_id=" + id + "&password=" + pw + "&x=0&y=0&ssl_chk=on&";
                param += LoginParams.ToString();
                using (HttpResponseMessage res = await client.PostAsync(_loginURL, new StringContent(param, Encoding.UTF8, "application/x-www-form-urlencoded")))
                {
                    res.EnsureSuccessStatusCode();

                    using (HttpContent content = res.Content)
                    {
                        string result = await content.ReadAsStringAsync();

                        if (result.Contains("등록된 아이디가 아닙니다."))
                            status = LoginStatus.IDError;
                        else if (result.Contains("비밀번호가 틀렸습니다."))
                            status = LoginStatus.PasswordError;
                        else if (result.Contains("아이디 또는 비밀번호가 잘못되었습니다."))
                            status = LoginStatus.ErrorBoth;
                        else if (result.Contains("로그인을 5번 실패 하셨습니다."))
                            status = LoginStatus.MaximumAttemptFailed;
                        else if (result.Contains("잘못된 접근입니다"))
                            status = LoginStatus.Unknown;
                        else if (result.Contains(gallogUrl))
                            status = LoginStatus.Success;
                        else
                            status = LoginStatus.Unknown;

                        if (retry)
                            return await PostLoginAsync(id, pw, false);
                    }
                }
            }

            return status;
        }

        private async Task<string> GetLoginPageAsync(string gallUrl)
        {
            string reqUrl = _loginPageURL + "?s_url=" + HttpUtility.UrlEncode(gallUrl);
            using (HttpClientHandler handler = new HttpClientHandler() { CookieContainer = cookies })
            using (HttpClient client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Accept.ParseAdd(_defaultAcceptString);
                client.DefaultRequestHeaders.Referrer = new Uri(_gallogURL);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
                client.Timeout = new TimeSpan(0, 0, 0, 0, _defaultTimeout);

                using (HttpResponseMessage res = await client.GetAsync(reqUrl))
                {
                    res.EnsureSuccessStatusCode();

                    using (HttpContent content = res.Content)
                    {
                        return await content.ReadAsStringAsync();
                    }
                }
            }
        }

        private async Task<string> GetGallogMainPageAsync(string id)
        {
            string reqUrl = _gallogURL + "/" + id.ToLower();
            using (HttpClientHandler handler = new HttpClientHandler() { CookieContainer = cookies })
            using (HttpClient client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Accept.ParseAdd(_defaultAcceptString);
                client.DefaultRequestHeaders.Referrer = new Uri(_gallogURL);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
                client.Timeout = new TimeSpan(0, 0, 0, 0, _defaultTimeout);

                using (HttpResponseMessage res = await client.GetAsync(reqUrl))
                {
                    res.EnsureSuccessStatusCode();

                    using (HttpContent content = res.Content)
                    {
                        return await content.ReadAsStringAsync();
                    }
                }
            }
        }
        
        private async Task<string> GetGallogListPageAsync(string user_id, int page, int cPage)
        {
            string reqURL = _gallogURL + "/inc/_mainGallog.php?page=" + page + "&rpage=" + cPage + "&gid=" + user_id + "&cid=";
            string referer = _gallogURL + "/" + user_id;
            
            using (HttpClientHandler handler = new HttpClientHandler() { CookieContainer = cookies })
            using (HttpClient client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Accept.ParseAdd(_defaultAcceptString);
                client.DefaultRequestHeaders.Referrer = new Uri(referer);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
                client.Timeout = new TimeSpan(0, 0, 0, 0, _defaultTimeout);

                using (HttpResponseMessage res = await client.GetAsync(reqURL))
                {
                    res.EnsureSuccessStatusCode();

                    using (HttpContent content = res.Content)
                    {
                        return await content.ReadAsStringAsync();
                    }
                }
            }
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
            
            using (HttpClientHandler handler = new HttpClientHandler() { CookieContainer = cookies })
            using (HttpClient client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Accept.ParseAdd(_defaultAcceptString);
                client.DefaultRequestHeaders.Referrer = new Uri(referer);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
                client.Timeout = new TimeSpan(0, 0, 0, 0, _defaultTimeout);

                using (HttpResponseMessage res = await client.GetAsync(searchPath))
                {
                    res.EnsureSuccessStatusCode();

                    using (HttpContent content = res.Content)
                    {
                        string result = await content.ReadAsStringAsync();
                        
                        if (result.Contains("해당 갤러리는 존재하지 않습니다"))
                        {
                            throw new Exception("해당 갤러리는 존재하지 않습니다.");
                        }

                        if (string.IsNullOrWhiteSpace(result))
                        {
                            throw new Exception("알 수 없는 오류입니다.");
                        }

                        return new Tuple<string, int, int>(result, searchPos, searchPage);
                    }
                }
            }
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

            string reqURL = null;
            string referer = null;

            if (gallType == GalleryType.Normal)
            {
                reqURL = _galleryURL + "/forms/delete_submit";
                referer = _galleryURL + "/board/delete/?id=" + info.GalleryId + "&no=" + info.ArticleID;
            }
            else if (gallType == GalleryType.Minor)
            {
                reqURL = _galleryURL + "/mgallery/forms/delete_submit";
                referer = _galleryURL + "/mgallery/board/delete/?id=" + info.GalleryId + "&no=" + info.ArticleID;
            }

            if (reqURL == null || referer == null)
                throw new Exception("예상치 못한 갤러리 형식입니다.");
            
            using (HttpClientHandler handler = new HttpClientHandler() { CookieContainer = cookies })
            using (HttpClient client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Accept.ParseAdd(_defaultAcceptString);
                client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                client.DefaultRequestHeaders.Referrer = new Uri(referer);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
                client.Timeout = new TimeSpan(0, 0, 0, 0, _defaultTimeout);

                string reqData = delete_Params.ToString();
                if (reqData == null)
                    throw new Exception("예상치 못한 갤러리 형식입니다.");

                using (HttpResponseMessage res =
                    await client.PostAsync(reqURL, new StringContent(reqData, Encoding.UTF8, "application/x-www-form-urlencoded")))
                {
                    res.EnsureSuccessStatusCode();

                    using (HttpContent content = res.Content)
                    {
                        string result = await content.ReadAsStringAsync();

                        if (result == "true||" + info.GalleryId)
                        {
                            return new DeleteResult(true, "");
                        }
                        else if (result.StartsWith("false||"))
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

            string reqURL = null;
            string referer = null;

            if (gallType == GalleryType.Normal)
            {
                reqURL = _galleryURL + "/forms/delete_password_submit";
                referer = _galleryURL + "/board/delete/?id=" + delParam.GalleryId + "&no=" + delParam.ArticleID;
            }
            else if (gallType == GalleryType.Minor)
            {
                reqURL = _galleryURL + "/mgallery/forms/delete_password_submit";
                referer = _galleryURL + "/mgallery/board/delete/?id=" + delParam.GalleryId + "&no=" + delParam.ArticleID;
            }

            if (reqURL == null || referer == null)
                throw new Exception("예상치 못한 갤러리 형식입니다.");
            
            using (HttpClientHandler handler = new HttpClientHandler() { CookieContainer = cookies })
            using (HttpClient client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Accept.ParseAdd(_defaultAcceptString);
                client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                client.DefaultRequestHeaders.Referrer = new Uri(referer);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
                client.Timeout = new TimeSpan(0, 0, 0, 0, _defaultTimeout);

                string reqData = delete_Params.ToString();
                reqData += "&password=" + delParam.Password;

                using (HttpResponseMessage res =
                    await client.PostAsync(reqURL, new StringContent(reqData, Encoding.UTF8, "application/x-www-form-urlencoded")))
                {
                    res.EnsureSuccessStatusCode();

                    using (HttpContent content = res.Content)
                    {
                        string result = await content.ReadAsStringAsync();

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

            string reqURL = _galleryURL + "/mgallery/forms/delete_submit";
            string referer = _galleryURL + "/mgallery/board/delete/?id=" + delParam.GalleryId + "&no=" + delParam.ArticleID + "&key=" + key;
            
            using (HttpClientHandler handler = new HttpClientHandler() { CookieContainer = cookies })
            using (HttpClient client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Accept.ParseAdd(_defaultAcceptString);
                client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                client.DefaultRequestHeaders.Referrer = new Uri(referer);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
                client.Timeout = new TimeSpan(0, 0, 0, 0, _defaultTimeout);

                string reqData = delete_Params.ToString();

                using (HttpResponseMessage res =
                    await client.PostAsync(reqURL, new StringContent(reqData, Encoding.UTF8, "application/x-www-form-urlencoded")))
                {
                    res.EnsureSuccessStatusCode();

                    using (HttpContent content = res.Content)
                    {
                        string result = await content.ReadAsStringAsync();

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

            string reqURL = _galleryURL + "/forms/comment_delete_submit";
            string referer = _galleryURL + "/board/view/?id=" + param.GalleryId + "&no=" + param.ArticleId;
            
            using (HttpClientHandler handler = new HttpClientHandler() { CookieContainer = cookies })
            using (HttpClient client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Accept.ParseAdd(_defaultAcceptString);
                client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                client.DefaultRequestHeaders.Referrer = new Uri(referer);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
                client.Timeout = new TimeSpan(0, 0, 0, 0, _defaultTimeout);

                string reqData = "ci_t=" + ci_t + "&id=" + param.GalleryId + "&no=" + param.ArticleId + "&p_no=" + param.ArticleId +
                                "&re_no=" + param.CommentId + "&best_origin=&check_7=" + check7;

                using (HttpResponseMessage res =
                    await client.PostAsync(reqURL, new StringContent(reqData, Encoding.UTF8, "application/x-www-form-urlencoded")))
                {
                    res.EnsureSuccessStatusCode();

                    using (HttpContent content = res.Content)
                    {
                        string result = await content.ReadAsStringAsync();

                        if (result == "")
                        {
                            return new DeleteResult(true, "");
                        }
                        else if (result == "false||댓글내역이 없습니다")
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
        
        private async Task<DeleteResult> PostDeleteGallogArticleAsync(GallogArticleDeleteParameter param, int delay)
        {
            string reqURL = _gallogURL + "/inc/_deleteArticle.php";
            string referer = _gallogURL + "/inc/_deleteLog.php?gid=" + param.UserId;
            
            using (HttpClientHandler handler = new HttpClientHandler() { CookieContainer = cookies })
            using (HttpClient client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Accept.ParseAdd(_defaultAcceptString);
                client.DefaultRequestHeaders.Referrer = new Uri(referer);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
                client.Timeout = new TimeSpan(0, 0, 0, 0, _defaultTimeout);

                string reqData = "rb=&dTp=1&gid=" + param.UserId + "&cid=" + param.GalleryNo +
                    "&pno=" + param.ArticleId + "&no=" + param.ArticleId + "&logNo=" + param.LogNo + "&id=" + param.GalleryId +
                    "&nate=&dcc_key=" + param.DCCKey
                    + (param.AdditionalParameter["dcc_key"] != null ? "" : ("&" + param.AdditionalParameter.ToString()));

                using (HttpResponseMessage res =
                    await client.PostAsync(reqURL, new StringContent(reqData, Encoding.UTF8, "application/x-www-form-urlencoded")))
                {
                    res.EnsureSuccessStatusCode();

                    using (HttpContent content = res.Content)
                    {
                        string result = await content.ReadAsStringAsync();

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
        
        private async Task<DeleteResult> PostDeleteGallogCommentAsync(GallogCommentDeleteParameter param, int delay)
        {
            string reqURL = _gallogURL + "/inc/_deleteRepOk.php";
            string referer = _gallogURL + "/inc/_deleteLogRep.php?gid=" + param.UserId + "&cid=" 
                            + param.GalleryNo + "&id=" + param.GalleryId + "&no=" + param.ArticleId + "&logNo=" + param.LogNo + "&rpage=";
            
            using (HttpClientHandler handler = new HttpClientHandler() { CookieContainer = cookies })
            using (HttpClient client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Accept.ParseAdd(_defaultAcceptString);
                client.DefaultRequestHeaders.Referrer = new Uri(referer);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
                client.Timeout = new TimeSpan(0, 0, 0, 0, _defaultTimeout);

                string reqData = "rb=&dTp=1&gid=" + param.UserId + "&cid=" + param.GalleryNo + "&page=&pno=" +
                    "&no=" + param.ArticleId + "&c_no=" + param.CommentId + "&logNo=" + param.LogNo + "&id=" + param.GalleryId +
                    "&nate=&";
                reqData += param.AdditionalParameter.ToString();

                using (HttpResponseMessage res =
                    await client.PostAsync(reqURL, new StringContent(reqData, Encoding.UTF8, "application/x-www-form-urlencoded")))
                {
                    res.EnsureSuccessStatusCode();

                    using (HttpContent content = res.Content)
                    {
                        string result = await content.ReadAsStringAsync();

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
        
        private async Task<string> GetGalleryCommentViewPageAsync(string gallid, string articleid)
        {
            string reqURL = _galleryURL + "/board/comment_view/?id=" + gallid + "&no=" + articleid;
            string referer = _galleryURL + "/board/lists/?id=" + gallid;

            using (HttpClientHandler handler = new HttpClientHandler() { CookieContainer = cookies })
            using (HttpClient client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Accept.ParseAdd(_defaultAcceptString);
                client.DefaultRequestHeaders.Referrer = new Uri(referer);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
                client.Timeout = new TimeSpan(0, 0, 0, 0, _defaultTimeout);

                using (HttpResponseMessage res =await client.GetAsync(reqURL))
                {
                    res.EnsureSuccessStatusCode();

                    using (HttpContent content = res.Content)
                    {
                        return await content.ReadAsStringAsync();
                    }
                }
            }
        }
        
        private async Task<string> GetDeleteGalleryArticlePageAsync(string gallId, string no, string key, GalleryType gallType)
        {
            string reqURL = null;
            string referer = null; 

            if(gallType == GalleryType.Normal)
            {
                reqURL = _galleryURL + "/board/delete/" + "?id=" + gallId + "&no=" + no;
                referer = _galleryURL + "/board/view/?id=" + gallId + "&no=" + no;
            }
            else if(gallType == GalleryType.Minor)
            {
                reqURL = _galleryURL + "/mgallery/board/delete/" + "?id=" + gallId + "&no=" + no;
                referer = _galleryURL + "/mgallery/board/view/?id=" + gallId + "&no=" + no;
            }

            if (reqURL == null || referer == null)
                throw new Exception("예상치 못한 갤러리 형식입니다.");

            if(!string.IsNullOrWhiteSpace(key))
            {
                reqURL += "&key=" + key;
            }
            
            using (HttpClientHandler handler = new HttpClientHandler() { CookieContainer = cookies })
            using (HttpClient client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Accept.ParseAdd(_defaultAcceptString);
                client.DefaultRequestHeaders.Referrer = new Uri(referer);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
                client.Timeout = new TimeSpan(0, 0, 0, 0, _defaultTimeout);

                using (HttpResponseMessage res = await client.GetAsync(reqURL))
                {
                    res.EnsureSuccessStatusCode();

                    using (HttpContent content = res.Content)
                    {
                        return await content.ReadAsStringAsync();
                    }
                }
            }
        }
        
        private async Task<string> GetDeleteGallogArticlePageAsync(string url, string id)
        {
            string referer = _gallogURL + "/inc/_mainGallog.php?gid=" + id;
            
            using (HttpClientHandler handler = new HttpClientHandler() { CookieContainer = cookies })
            using (HttpClient client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Accept.ParseAdd(_defaultAcceptString);
                client.DefaultRequestHeaders.Referrer = new Uri(referer);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
                client.Timeout = new TimeSpan(0, 0, 0, 0, _defaultTimeout);

                using (HttpResponseMessage res = await client.GetAsync(url))
                {
                    res.EnsureSuccessStatusCode();

                    using (HttpContent content = res.Content)
                    {
                        return await content.ReadAsStringAsync();
                    }
                }
            }
        }
        
        private async Task<string> GetDeleteGallogCommentPageAsync(string url, string user_id)
        {
            string referer = _gallogURL + "/inc/_mainGallog.php?gid=" + user_id;
            
            using (HttpClientHandler handler = new HttpClientHandler() { CookieContainer = cookies })
            using (HttpClient client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Accept.ParseAdd(_defaultAcceptString);
                client.DefaultRequestHeaders.Referrer = new Uri(referer);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
                client.Timeout = new TimeSpan(0, 0, 0, 0, _defaultTimeout);

                using (HttpResponseMessage res = await client.GetAsync(url))
                {
                    res.EnsureSuccessStatusCode();

                    using (HttpContent content = res.Content)
                    {
                        return await content.ReadAsStringAsync();
                    }
                }
            }
        }
    }
}
