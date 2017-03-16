using System.Net;
using System.Threading;
using System;
using System.Collections.Generic;

namespace DCAdapter
{
    /// <summary>
    /// DC인사이드 연결을 처리합니다.
    /// </summary>
    public class DCConnector
    {
        LoginStatus status = LoginStatus.NotLogin;
        string errMsg;
        CookieContainer cookies;
        bool isLogin;
        string user_id = "";

        /// <summary>
        /// 로그인 여부를 표시합니다.
        /// </summary>
        public bool IsLogin
        {
            get
            {
                return isLogin;
            }
        }

        /// <summary>
        /// 로그인시 에러 메시지를 나타냅니다.
        /// </summary>
        public string ErrorMessage
        {
            get
            {
                return errMsg;
            }
        }
        /// <summary>
        /// 로그인 상태를 표시합니다.
        /// </summary>
        public LoginStatus LoginStatus
        {
            get
            {
                return status;
            }
        }

        public DCConnector()
        {
            status = LoginStatus.NotLogin;
            isLogin = false;
            cookies = new CookieContainer();
        }

        /// <summary>
        /// DC인사이드 서버에 주어진 ID와 비밀번호로 로그인을 요청합니다.
        /// </summary>
        /// <param name="id">사용자의 ID</param>
        /// <param name="pw">사용자의 비밀번호</param>
        /// <returns>로그인 성공시 True, 실패시 False를 반환합니다.</returns>
        public bool LoginDCInside(string id, string pw)
        {
            if(string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(pw))
            {
                status = LoginStatus.ErrorBoth;
                errMsg = "아이디 또는 비밀번호를 입력해주세요.";
                return false;
            }

            if (HttpRequest.RequestLogin(id, pw, out status, ref cookies))
            {
                errMsg = "";
                isLogin = true;
                this.user_id = id;

                return true;
            }
            else
            {
                isLogin = false;

                if (status == LoginStatus.IDError)
                {
                    errMsg = "존재하지 않는 아이디입니다.";

                }
                else if (status == LoginStatus.PasswordError)
                {
                    errMsg = "잘못된 비밀번호입니다.";
                }
                else if (status == LoginStatus.ErrorBoth)
                {
                    errMsg = "아이디 또는 비밀번호가 잘못되었습니다.";
                }
                else if(status == LoginStatus.Unknown)
                {
                    errMsg = "서버 통신중 알 수없는 에러가 발생하였습니다.";
                }

                return false;
            }
        }

        /// <summary>
        /// 갤로그의 글 목록을 불러옵니다.
        /// </summary>
        /// <returns>갤로그의 글 목록을 반환합니다.</returns>
        public List<ArticleInfo> LoadGallogArticles()
        {
            string html = "";
            int articleCounts;
            int pageCnts;

            List<ArticleInfo> articles = new List<ArticleInfo>();

            try
            {
                // 갤로그의 HTML 소스를 요청
                html = HttpRequest.RequestGallogHtml(this.user_id, ref cookies);
            }
            catch
            {
                return null;
            }

            // 갤로그의 HTML 소스에서 총 쓴 글의 갯수를 가져와서 총 페이지 갯수를 지정.
            articleCounts = HtmlParser.GetArticleCounts(html);
            pageCnts = (int)(articleCounts / 10) + (articleCounts % 10 > 0 ? 1 : 0);
            
            // 총 페이지 수만큼 반복하여 총 글 목록을 리스트에 저장함.
            for (int i = 0; i < pageCnts; i++)
            {
                articles.AddRange(LoadArticleList(i + 1));
            }
            
            // 저장한 리스트를 반환
            return articles;
        }

        public List<CommentInfo> LoadGallogComments()
        {
            string html = "";
            int commentCounts;
            int pageCnts;

            List<CommentInfo> commentList = new List<CommentInfo>();

            try
            {
                html = HttpRequest.RequestGallogHtml(this.user_id, ref cookies);
            }
            catch
            {
                return null;
            }

            commentCounts = HtmlParser.GetCommentCounts(html);
            pageCnts = (int)(commentCounts / 10) + (commentCounts % 10 > 0 ? 1 : 0);
            
            for (int i = 0; i < pageCnts; i++)
            {
                Thread.Sleep(50);
                commentList.AddRange(LoadCommentList(i + 1));
            }

            return commentList;
        }

        private List<ArticleInfo> LoadArticleList(int page)
        {
            string html = HttpRequest.RequestWholePage(user_id, page, 1, ref cookies);
            List<ArticleInfo> articleList = HtmlParser.GetArticleList(html);

            return articleList;
        }

        private List<CommentInfo> LoadCommentList(int page)
        {
            string html = HttpRequest.RequestWholePage(user_id, 1, page, ref cookies);
            List<CommentInfo> articleList = HtmlParser.GetCommentList(html);

            return articleList;
        }

        public ArticleInfo DeleteArticle(ArticleInfo info, bool both)
        {
            // HTTP 요청에 딜레이를 주어 서버 오류 방지
            int delay = 50;

            string gall_id, gall_no, article_id, logNo;

            try
            {
                this.GetDeleteArticleInfo(info.DeleteURL, out gall_id, out gall_no, out article_id, out logNo);
            }
            catch(Exception e)
            {
                info.ActualDelete = false;
                info.GallogDelete = false;
                info.DeleteMessage = e.Message;

                return info;
            }

            Thread.Sleep(delay);
            
            DeleteResult res1 =  HttpRequest.RequestDeleteAritcle(gall_id, article_id, ref cookies);

            if (!res1.Success && res1.ErrorMessage != "이미 삭제된 글입니다.")
            {
                info.ActualDelete = false;
                info.GallogDelete = false;
                info.DeleteMessage = "갤러리의 글을 지우는데 실패하였습니다 - [" + res1.ErrorMessage + "]";

                return info;
            }

            if (both)
            {
                Thread.Sleep(delay);

                DeleteResult res2 = HttpRequest.RequestDeleteGallogArticle(user_id, gall_id, gall_no, article_id, logNo, delay, ref cookies);

                if (!res2.Success)
                {
                    info.ActualDelete = true;
                    info.GallogDelete = false;
                    info.DeleteMessage = "갤로그의 글을 지우는데 실패하였습니다 - [" + res2.ErrorMessage + "]";

                    return info;
                }
            }

            info.ActualDelete = true;
            info.GallogDelete = true;
            return info;
        }

        public CommentInfo DeleteComment(CommentInfo info, bool both)
        {
            // HTTP 요청에 딜레이를 주어 서버 오류 방지
            int delay = 50;

            string gall_id, gall_no, article_id, comment_id, logNo;

            try
            {
                this.GetDeleteCommentInfo(info.DeleteURL, out gall_id, out gall_no, out article_id, out comment_id, out logNo);
            }
            catch (Exception e)
            {
                info.ActualDelete = false;
                info.GallogDelete = false;
                info.DeleteMessage = e.Message;

                return info;
            }

            Thread.Sleep(delay);

            DeleteResult res1 = HttpRequest.RequestDeleteComment(gall_id, article_id, comment_id, ref cookies);

            if (!res1.Success && res1.ErrorMessage != "댓글내역이 없습니다.")
            {
                info.ActualDelete = false;
                info.GallogDelete = false;
                info.DeleteMessage = "갤러리의 글을 지우는데 실패하였습니다 - [" + res1.ErrorMessage + "]";

                return info;
            }

            if (both)
            {
                Thread.Sleep(delay);

                DeleteResult res2 = HttpRequest.RequestDeleteGallogComment(user_id, gall_id, gall_no, article_id, comment_id, logNo, delay, ref cookies);

                if (!res2.Success)
                {
                    info.ActualDelete = true;
                    info.GallogDelete = false;
                    info.DeleteMessage = "갤로그의 글을 지우는데 실패하였습니다 - [" + res2.ErrorMessage + "]";

                    return info;
                }
            }

            info.ActualDelete = true;
            info.GallogDelete = true;
            info.DeleteMessage = "";

            return info;
        }

        private void GetDeleteArticleInfo(string url, out string gall_id, out string gall_no, out string article_id, out string logNo)
        {
            gall_id = null;
            gall_no = null;
            article_id = null;
            logNo = null;

            string galHtml = HttpRequest.RequestDeleteGallogArticlePage(url, user_id, ref cookies);
            HtmlParser.GetGallogArticleInfo(galHtml, out gall_id, out gall_no, out article_id, out logNo);
        }

        private void GetDeleteCommentInfo(string url, out string gall_id, out string gall_no, out string article_id, out string comment_id, out string logNo)
        {
            gall_id = null;
            gall_no = null;
            article_id = null;
            comment_id = null;
            logNo = null;

            string galHtml = HttpRequest.RequestDeleteGallogCommentPage(url, user_id, ref cookies);
            HtmlParser.GetGallogCommentInfo(galHtml, out gall_id, out gall_no, out article_id, out comment_id, out logNo);
        }
    }
}