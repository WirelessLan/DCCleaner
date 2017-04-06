using System;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using System.Threading;

namespace DCAdapter
{
    class HttpRequest
    {
        /// <summary>
        /// 서버와의 통신에 사용되는 Fake User-Agent
        /// </summary>
        readonly static string UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36";

        /// <summary>
        /// 로그인을 요청합니다.
        /// </summary>
        /// <param name="id">사용자의 ID</param>
        /// <param name="pw">사용자의 비밀번호</param>
        /// <param name="status">(out) 로그인 상태를 가져옵니다</param>
        /// <param name="cookies">(ref) 서버의 쿠키 정보를 저장하는 쿠키 컨테이너입니다.</param>
        /// <returns>로그인 성공시 True, 실패시 False 반환</returns>
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

        internal static void RequestAjaxDB(string user_id, string mode, ref CookieContainer cookies)
        {
            const string _gallogURL = "http://gallog.dcinside.com/ajdb.php";
            string referer = "http://gallog.dcinside.com/" + user_id;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_gallogURL);

            req.Method = "POST";
            req.UserAgent = UserAgent;
            req.CookieContainer = cookies;
            req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            req.Proxy = null;
            req.Host = "gallog.dcinside.com";
            req.Referer = referer;

            using (StreamWriter writer = new StreamWriter(req.GetRequestStream()))
            {
                string reqParam = "mode=" + mode + "&gid=" + user_id;
                writer.Write(reqParam);
            }

            using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
            {
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream responseStream = res.GetResponseStream())
                    {
                        using (StreamReader readStream = new StreamReader(responseStream, Encoding.UTF8))
                        {
                            string result = readStream.ReadToEnd();

                            return;
                        }
                    }
                }
            }

            throw new Exception("AJAX DB를 불러올 수 없습니다.");
        }

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

        internal static string RequestGallogMainList(string user_id, ref CookieContainer cookies)
        {
            const string _reqURL = "http://gallog.dcinside.com/inc/_mainList.php";
            string referer = "http://gallog.dcinside.com/" + user_id;

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_reqURL + "?gid=" + user_id);

            req.Referer = referer;
            req.Host = "gallog.dcinside.com";
            req.UserAgent = UserAgent;
            req.Headers.Add("Upgrade-Insecure-Requests", "1");
            req.Method = "GET";

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

            throw new Exception("갤로그 글 리스트를 불러올 수 없습니다.");
        }

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

        internal static DeleteResult RequestDeleteAritcle(string gallId, string no, ref CookieContainer cookies)
        {
            string pageHtml = RequestDeleteAritclePage(gallId, no, ref cookies);
            string ci_t = null, dcc_key = null;

            try
            {
                HtmlParser.GetDeleteArticleParameters(pageHtml, out dcc_key);
                ci_t = cookies.GetCookies(new Uri("http://gall.dcinside.com/"))["ci_c"].Value;
            }
            catch(Exception e)
            {
                return new DeleteResult(false, e.Message);
            }
            
            const string _reqURL = "http://gall.dcinside.com/forms/delete_submit";
            const string host = "gall.dcinside.com";
            string referer = "http://gall.dcinside.com/board/delete/?id=" + gallId + "&no=" + no;

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
                string reqData = "ci_t=" + ci_t + "&id=" + gallId + "&no=" + no + "&key=&dcc_key=" + dcc_key;
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

                            if(result.Contains("true"))
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

        internal static DeleteResult RequestDeleteComment(string gallid, string articleid, string commentid, ref CookieContainer cookies)
        {
            string pageHtml = RequestArticlePage(gallid, articleid, ref cookies);
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

                            // 성공/실패 여부를 확인할 수 없어 그냥 성공했다 가정
                            return new DeleteResult(true, "");
                        }
                    }
                }
            }

            return new DeleteResult(false, "알 수 없는 오류입니다.");
        }

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

                            // 성공/실패 여부를 확인할 수 없어 그냥 성공했다 가정
                            return new DeleteResult(true, "");
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

        private static string RequestArticlePage(string gallid, string articleid, ref CookieContainer cookies)
        {
            const string _reqURL = "http://gall.dcinside.com/board/view/";
            string referer = "http://gall.dcinside.com/board/view/?id=" + gallid;
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

        private static string RequestDeleteAritclePage(string gallId, string no, ref CookieContainer cookies)
        {
            const string _reqURL = "http://gall.dcinside.com/board/delete/";
            string referer = "http://gall.dcinside.com/board/view/?id=" + gallId + "&no=" + no;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_reqURL + "?id=" + gallId + "&no=" + no);

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
