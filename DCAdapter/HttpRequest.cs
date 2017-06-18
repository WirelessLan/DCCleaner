using System;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DCAdapter
{
    class HttpRequest
    {
        /// <summary>
        /// 서버와의 통신에 사용되는 Fake User-Agent
        /// </summary>
        readonly static string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.86 Safari/537.36";

        /// <summary>
        /// 로그인을 요청하는 함수
        /// </summary>
        /// <param name="id">사용자의 ID</param>
        /// <param name="pw">사용자의 비밀번호</param>
        /// <param name="status">로그인 결과</param>
        /// <param name="cookies">클리너 쿠키 컨테이너</param>
        /// <returns>로그인 성공 여부를 반환. 성공시 true, 실패시 false</returns>
        internal static bool RequestLogin(string id, string pw, out LoginStatus status, ref CookieContainer cookies)
        {
            string gallUrl = "http://gall.dcinside.com";

            // 로그인 페이지에 한번은 접속해야 정상 동작함
            RequestLoginPage(gallUrl, ref cookies);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://dcid.dcinside.com/join/member_check.php");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = cookies;
            request.UserAgent = UserAgent;
            request.Proxy = null;
            request.Referer = "https://dcid.dcinside.com/join/login.php?s_url=" + HttpUtility.UrlEncode(gallUrl);

            using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write("s_url=" + HttpUtility.UrlEncode(gallUrl) + "&tieup=&url=&user_id=" + id + "&password=" + pw + "&x=0&y=0&ssl_chk=on");
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (StreamReader readStream = new StreamReader(responseStream, Encoding.UTF8))
                        {
                            string result = readStream.ReadToEnd();

                            if (result.Contains("등록된 아이디가 아닙니다."))
                            {
                                status = LoginStatus.IDError;
                                return false;
                            }
                            else if (result.Contains("비밀번호가 틀렸습니다."))
                            {
                                status = LoginStatus.PasswordError;
                                return false;
                            }
                            else if (result.Contains("아이디 또는 비밀번호가 잘못되었습니다."))
                            {
                                status = LoginStatus.ErrorBoth;
                                return false;
                            }
                        }
                    }                    

                    status = LoginStatus.Success;
                    return true;
                }
                else
                {
                    status = LoginStatus.Unknown;
                    return false;
                }
            }
        }

        /// <summary>
        /// 갤로그의 메인 화면을 요청하는 함수
        /// </summary>
        /// <param name="id">사용자의 ID</param>
        /// <param name="cookies">클리너 쿠키 컨테이너</param>
        /// <returns>갤로그 메인화면의 HTML 소스</returns>
        internal static string RequestGallogHtml(string id, ref CookieContainer cookies)
        {
            const string _gallogURL = "http://gallog.dcinside.com/";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_gallogURL + id);

            req.UserAgent = UserAgent;
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Referer = _gallogURL;

            using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
            {
                if (res.StatusCode == HttpStatusCode.OK)
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
        
        /// <summary>
        /// 갤로그에서 글 또는 댓글 페이지를 가져오는 함수
        /// </summary>
        /// <param name="user_id">사용자의 ID</param>
        /// <param name="page">쓴 글의 갤로그 페이지 번호</param>
        /// <param name="cPage">쓴 댓글의 갤로그 페이지 번호</param>
        /// <param name="cookies">클리너 쿠키 컨테이너</param>
        /// <returns>요청한 페이지의 HTML 소스</returns>
        internal static string RequestWholePage(string user_id, int page, int cPage, ref CookieContainer cookies)
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

            using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
            {
                if(res.StatusCode == HttpStatusCode.OK)
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

        /// <summary>
        /// 갤러리에서 닉네임으로 검색하는 함수
        /// </summary>
        /// <param name="gall_id">갤러리 ID</param>
        /// <param name="gallType">갤러리 구분</param>
        /// <param name="nickname">검색할 닉네임</param>
        /// <param name="searchPos">갤러리 검색에 사용되는 위치변수</param>
        /// <param name="searchPage">갤러리 검색에 사용되는 페이지</param>
        /// <param name="cookies">클리너 쿠키 컨테이너</param>
        /// <returns>검색된 페이지의 HTML 소스</returns>
        internal static string RequestGalleryNickNameSearchPage(string gall_id, GalleryType gallType, string nickname, ref int searchPos, ref int searchPage, ref CookieContainer cookies)
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

            searchPath += "&s_type=search_all&s_keyword=" + HttpUtility.UrlEncode(nickname);

            const string host = "gall.dcinside.com";
            string referer = basePath;

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(searchPath);

            req.Method = "GET";
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Referer = referer;
            req.UserAgent = UserAgent;
            req.Host = host;
            req.Headers.Add("Upgrade-Insecure-Requests", "1");

            using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
            {
                if (res.StatusCode == HttpStatusCode.OK)
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

                            return result;
                        }
                    }
                }
            }

            throw new Exception("알 수 없는 오류입니다.");
        }

        /// <summary>
        /// 고정닉의 갤러리 글 삭제를 요청하는 함수
        /// </summary>
        /// <param name="gallId">갤러리 ID</param>
        /// <param name="no">글 번호</param>
        /// <param name="gallType">갤러리 구분</param>
        /// <param name="delay">삭제 딜레이</param>
        /// <param name="cookies">클리너 쿠키 컨테이너</param>
        /// <returns>갤러리 글 삭제 결과</returns>
        internal static DeleteResult RequestDeleteArticle(string gallId, string no, GalleryType gallType, int delay, ref CookieContainer cookies)
        {
            string pageHtml = RequestDeleteAritclePage(gallId, no, null, gallType, ref cookies);
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
                referer = "http://gall.dcinside.com/board/delete/?id=" + gallId + "&no=" + no;
            }
            else if (gallType == GalleryType.Minor)
            {
                _reqURL = "http://gall.dcinside.com/mgallery/forms/delete_submit";
                referer = "http://gall.dcinside.com/mgallery/board/delete/?id=" + gallId + "&no=" + no;
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

            using (StreamWriter writer = new StreamWriter(req.GetRequestStream()))
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

                writer.Write(reqData);
            }

            using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
            {
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream stream = res.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string result = reader.ReadToEnd();

                            if(result == "true||" + gallId)
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

        /// <summary>
        /// 유동닉의 갤러리 글 삭제를 요청하는 함수
        /// </summary>
        /// <param name="gallId">갤러리 ID</param>
        /// <param name="no">글 번호</param>
        /// <param name="password">글 비밀번호</param>
        /// <param name="gallType">갤러리 구분</param>
        /// <param name="delay">삭제 딜레이</param>
        /// <param name="cookies">클리너 쿠키 컨테이너</param>
        /// <returns>갤러리 글 삭제 결과</returns>
        internal static DeleteResult RequestDeleteFlowArticle(string gallId, string no, string password, GalleryType gallType, int delay, ref CookieContainer cookies)
        {
            string pageHtml = RequestDeleteAritclePage(gallId, no, null, gallType, ref cookies);
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
                referer = "http://gall.dcinside.com/board/delete/?id=" + gallId + "&no=" + no;
            }
            else if (gallType == GalleryType.Minor)
            {
                _reqURL = "http://gall.dcinside.com/mgallery/forms/delete_password_submit";
                referer = "http://gall.dcinside.com/mgallery/board/delete/?id=" + gallId + "&no=" + no;
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

            using (StreamWriter writer = new StreamWriter(req.GetRequestStream()))
            {
                string reqData = null;
                
                foreach (KeyValuePair<string, string> kv in delete_params)
                {
                    reqData += HttpUtility.UrlEncode(kv.Key) + "=" + HttpUtility.UrlEncode(kv.Value) + "&";
                }
                reqData += "password=" + password;

                writer.Write(reqData);
            }

            using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
            {
                if (res.StatusCode == HttpStatusCode.OK)
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
                                    return RequestDeleteMinorFlowArticle(gallId, no, result.Replace("true||", ""), ref cookies);
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

        /// <summary>
        /// 유동닉의 갤러리 글 삭제를 요청하는 함수 (마이너 갤러리는 2번 요청이 필요함)
        /// </summary>
        /// <param name="gall_id">갤러리 ID</param>
        /// <param name="no">글 번호</param>
        /// <param name="key">삭제 키</param>
        /// <param name="cookies">클리너 쿠키 컨테이너</param>
        /// <returns>갤러리 글 삭제 결과</returns>
        private static DeleteResult RequestDeleteMinorFlowArticle(string gall_id, string no, string key, ref CookieContainer cookies)
        {
            string pageHtml = RequestDeleteAritclePage(gall_id, no, key, GalleryType.Minor, ref cookies);
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
            string referer = "http://gall.dcinside.com/mgallery/board/delete/?id=" + gall_id + "&no=" + no + "&key=" + key;

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_reqURL);

            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Referer = referer;
            req.UserAgent = UserAgent;
            req.Headers.Add("X-Requested-With", "XMLHttpRequest");

            using (StreamWriter writer = new StreamWriter(req.GetRequestStream()))
            {
                string reqData = null;

                foreach (KeyValuePair<string, string> kv in delete_params)
                {
                    reqData += HttpUtility.UrlEncode(kv.Key) + "=" + HttpUtility.UrlEncode(kv.Value) + "&";
                }
                reqData = reqData.Substring(0, reqData.Length - 1);

                writer.Write(reqData);
            }

            using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
            {
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream stream = res.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string result = reader.ReadToEnd();

                            if (result.StartsWith("true||" + gall_id))
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

        /// <summary>
        /// 갤러리의 댓글 삭제를 요청하는 함수
        /// </summary>
        /// <param name="gallid">갤러리 ID</param>
        /// <param name="articleid">글 ID</param>
        /// <param name="commentid">댓글 ID</param>
        /// <param name="cookies">클리너 쿠키 컨테이너</param>
        /// <returns>갤러리 댓글 삭제 결과</returns>
        internal static DeleteResult RequestDeleteComment(string gallid, string articleid, string commentid, ref CookieContainer cookies)
        {
            string pageHtml = RequestArticleCommentViewPage(gallid, articleid, ref cookies);
            string ci_t = null, check7 = null;
            
            try
            {
                HtmlParser.GetDeleteCommentParameters(pageHtml, out check7);
                ci_t = cookies.GetCookies(new Uri("http://gall.dcinside.com/"))["ci_c"].Value;
            }
            catch (Exception e)
            {
                return new DeleteResult(false, e.Message);
            }

            const string _reqURL = "http://gall.dcinside.com/forms/comment_delete_submit";
            const string host = "gall.dcinside.com";
            string referer = "http://gall.dcinside.com/board/view/?id=" + gallid + "&no=" + articleid;

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_reqURL);

            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Referer = referer;
            req.UserAgent = UserAgent;
            req.Host = host;
            req.Headers.Add("X-Requested-With", "XMLHttpRequest");

            using (StreamWriter writer = new StreamWriter(req.GetRequestStream()))
            {
                string reqData = "ci_t=" + ci_t + "&id=" + gallid + "&no=" + articleid + "&p_no=" + articleid +
                                "&re_no=" + commentid + "&best_origin=&check_7=" + check7;
                writer.Write(reqData);
            }

            using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
            {
                if (res.StatusCode == HttpStatusCode.OK)
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

        /// <summary>
        /// 갤로그의 쓴 글 삭제를 요청하는 함수
        /// </summary>
        /// <param name="id">사용자 ID</param>
        /// <param name="gall_id">갤러리 ID</param>
        /// <param name="gall_no">갤러리 번호</param>
        /// <param name="art_id">글 ID</param>
        /// <param name="logNo">갤로그 Log 번호</param>
        /// <param name="delay">삭제 딜레이</param>
        /// <param name="cookies">클리너 쿠키 컨테이너</param>
        /// <returns>갤로그 글 삭제 결과</returns>
        internal static DeleteResult RequestDeleteGallogArticle(string id, string gall_id, string gall_no, string art_id, string logNo, int delay, ref CookieContainer cookies)
        {
            string pageHtml = RequestDeleteGallogArticlePage(id, gall_no, art_id, logNo, ref cookies);
            string dcc_key = null, randKey = null, randVal = null;

            try
            {
                HtmlParser.GetDeleteGallogArticleParameters(pageHtml, out gall_id, out dcc_key, out randKey, out randVal);
            }
            catch (Exception e)
            {
                return new DeleteResult(false, e.Message);
            }

            Thread.Sleep(delay);

            const string _reqURL = "http://gallog.dcinside.com/inc/_deleteArticle.php";
            const string host = "gallog.dcinside.com";
            string referer = "http://gallog.dcinside.com/inc/_deleteLog.php?gid=" + id;

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_reqURL);

            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Referer = referer;
            req.UserAgent = UserAgent;
            req.Host = host;
            req.Headers.Add("Upgrade-Insecure-Requests", "1");

            using (StreamWriter writer = new StreamWriter(req.GetRequestStream()))
            {
                string reqData = "rb=&dTp=1&gid=" + id + "&cid=" + gall_no +
                    "&pno=" + art_id + "&no=" + art_id + "&logNo=" + logNo + "&id=" + gall_id +
                    "&nate=&dcc_key=" + dcc_key + (randKey == "dcc_key" ? "" : ("&" + HttpUtility.UrlEncode(randKey) + "=" + HttpUtility.UrlEncode(randVal)));
                writer.Write(reqData);
            }

            using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
            {
                if (res.StatusCode == HttpStatusCode.OK)
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

        /// <summary>
        /// 갤로그의 댓글 삭제를 요청하는 함수
        /// </summary>
        /// <param name="id">사용자 ID</param>
        /// <param name="gall_id">갤러리 ID</param>
        /// <param name="gall_no">갤러리 번호</param>
        /// <param name="art_id">댓글을 쓴 글의 번호</param>
        /// <param name="comment_id">댓글의 번호</param>
        /// <param name="logNo">갤로그의 Log 번호</param>
        /// <param name="delay">삭제 딜레이</param>
        /// <param name="cookies">클리너 쿠키 컨테이너</param>
        /// <returns>갤로그 댓글 삭제 결과</returns>
        internal static DeleteResult RequestDeleteGallogComment(string id, string gall_id, string gall_no, string art_id, string comment_id, string logNo, int delay, ref CookieContainer cookies)
        {
            string pageHtml = RequestDeleteGallogCommentPage(id, art_id, comment_id, logNo, ref cookies);
            string randomKey = null, randomVal = null;

            try
            {
                HtmlParser.GetDeleteGallogCommentParameters(pageHtml, out gall_id, out randomKey, out randomVal);
            }
            catch (Exception e)
            {
                return new DeleteResult(false, e.Message);
            }

            Thread.Sleep(delay);

            const string _reqURL = "http://gallog.dcinside.com/inc/_deleteRepOk.php";
            const string host = "gallog.dcinside.com";
            string referer = "http://gallog.dcinside.com/inc/_deleteLogRep.php?gid=" + id + "&cid=" + gall_no + "&id=" + gall_id + "&no=" + art_id + "&logNo=" + logNo + "&rpage=";
            
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_reqURL);

            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Referer = referer;
            req.UserAgent = UserAgent;
            req.Host = host;
            req.Headers.Add("Upgrade-Insecure-Requests", "1");

            using (StreamWriter writer = new StreamWriter(req.GetRequestStream()))
            {
                string reqData = "rb=&dTp=1&gid=" + id + "&cid=" + gall_no + "&page=&pno=" +
                    "&no=" + art_id + "&c_no=" + comment_id + "&logNo=" + logNo + "&id=" + gall_id +
                    "&nate=&" + HttpUtility.UrlEncode(randomKey) + "=" + HttpUtility.UrlEncode(randomVal);
                writer.Write(reqData);
            }

            using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
            {
                if (res.StatusCode == HttpStatusCode.OK)
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

        /// <summary>
        /// 로그인 페이지를 요청합니다.
        /// </summary>
        /// <param name="gallUrl">DCInside 갤러리 메인 페이지 주소</param>
        /// <param name="cookies">(ref) 서버의 쿠키 정보를 저장하는 쿠키 컨테이너입니다.</param>
        private static void RequestLoginPage(string gallUrl, ref CookieContainer cookies)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://dcid.dcinside.com/join/login.php?s_url=" + HttpUtility.UrlEncode(gallUrl));

            request.Method = "GET";
            request.Referer = gallUrl;
            request.CookieContainer = cookies;
            request.UserAgent = UserAgent;
            request.Proxy = null;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            response.Close();
        }

        /// <summary>
        /// 댓글 삭제시 댓글을 쓴 글을 요청하는 함수
        /// </summary>
        /// <param name="gallid">갤러리 ID</param>
        /// <param name="articleid">글 번호</param>
        /// <param name="cookies">클리너 쿠키 컨테이너</param>
        /// <returns>요청한 페이지의 HTML 소스</returns>
        private static string RequestArticleCommentViewPage(string gallid, string articleid, ref CookieContainer cookies)
        {
            const string _reqURL = "http://gall.dcinside.com/board/comment_view/";
            string referer = "http://gall.dcinside.com/board/lists/?id=" + gallid;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_reqURL + "?id=" + gallid + "&no=" + articleid);
            
            req.Method = "GET";
            req.UserAgent = UserAgent;
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Headers.Add("Upgrade-Insecure-Requests", "1");
            req.Host = "gall.dcinside.com";
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

            throw new Exception("글을 불러올 수 없습니다.");
        }

        /// <summary>
        /// 갤러리의 글 삭제 페이지를 요청하는 함수
        /// </summary>
        /// <param name="gallId">갤 ID</param>
        /// <param name="no">글 번호</param>
        /// <param name="key">글 삭제시 필요한 Key(유동닉 마이너 갤러리 2차 삭제시 필요)</param>
        /// <param name="gallType">갤러리 구분</param>
        /// <param name="cookies">클리너 쿠키 컨테이너</param>
        /// <returns>삭제 페이지의 HTML 소스</returns>
        private static string RequestDeleteAritclePage(string gallId, string no, string key, GalleryType gallType, ref CookieContainer cookies)
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
            req.UserAgent = UserAgent;
            req.CookieContainer = cookies;
            req.Proxy = null;
            req.Headers.Add("Upgrade-Insecure-Requests", "1");
            req.Host = "gall.dcinside.com";
            req.Referer = referer;

            using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
            {
                if(res.StatusCode == HttpStatusCode.OK)
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

        /// <summary>
        /// 갤로그의 글 삭제 페이지를 요청하는 함수
        /// </summary>
        /// <param name="id">사용자 ID</param>
        /// <param name="gall_no">갤러리 번호</param>
        /// <param name="art_id">글 ID</param>
        /// <param name="logNo">갤로그 Log 번호</param>
        /// <param name="cookies">클리너 쿠키 컨테이너</param>
        /// <returns>갤로그 글 삭제 페이지의 HTML 소스</returns>
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

        /// <summary>
        /// 갤로그의 글 삭제 페이지를 요청하는 함수
        /// </summary>
        /// <param name="url">삭제할 글의 URL</param>
        /// <param name="id">사용자 ID</param>
        /// <param name="cookies">클리너 쿠키 컨테이너</param>
        /// <returns>삭제 페이지의 HTML 소스</returns>
        internal static string RequestDeleteGallogArticlePage(string url, string id, ref CookieContainer cookies)
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

        /// <summary>
        /// 갤로그의 댓글 삭제 페이지를 요청하는 함수
        /// </summary>
        /// <param name="id">사용자 ID</param>
        /// <param name="art_id">댓글을 쓴 글의 ID</param>
        /// <param name="commentId">댓글 ID</param>
        /// <param name="logNo">갤로그 Log 번호</param>
        /// <param name="cookies">클리너 쿠키 컨테이너</param>
        /// <returns>삭제 페이지의 HTML 소스</returns>
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

        /// <summary>
        /// 갤로그의 댓글 삭제 페이지를 요청하는 함수
        /// </summary>
        /// <param name="url">삭제 페이지의 URL</param>
        /// <param name="user_id">사용자 ID</param>
        /// <param name="cookies">클리너 쿠키 컨테이너</param>
        /// <returns>삭제 페이지의 HTML 소스</returns>
        internal static string RequestDeleteGallogCommentPage(string url, string user_id, ref CookieContainer cookies)
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
    }
}
