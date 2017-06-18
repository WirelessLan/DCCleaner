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
        public string LoginErrorMessage
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
        /// DC 인사이드 서버에 주어진 ID와 비밀번호로 로그인을 요청합니다.
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

            List<ArticleInfo> articleList = new List<ArticleInfo>();

            try
            {
                // 갤로그의 HTML 소스를 요청
                html = HttpRequest.RequestGallogHtml(this.user_id, ref cookies);
            }
            catch (ThreadAbortException) { throw; }
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
                List<ArticleInfo> newArticleList = null;
                try
                {
                    newArticleList = LoadArticleList(i + 1);
                }
                catch
                {
                    for (int j = 0; j < 3; j++)
                    {
                        try
                        {
                            Thread.Sleep(200);
                            newArticleList = LoadArticleList(i + 1);
                        }
                        catch
                        {
                            continue;
                        }

                        if (newArticleList != null)
                            break;
                    }

                    if (newArticleList == null)
                        throw new Exception("글을 불러오는데 실패하였습니다.");
                }

                articleList.AddRange(newArticleList);
            }
            
            // 저장한 리스트를 반환
            return articleList;
        }

        /// <summary>
        /// 갤로그의 댓글 목록을 불러옵니다.
        /// </summary>
        /// <returns>갤로그의 댓글 목록을 반환합니다.</returns>
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
            catch (ThreadAbortException) { throw; }
            catch
            {
                return null;
            }

            commentCounts = HtmlParser.GetCommentCounts(html);
            pageCnts = (int)(commentCounts / 10) + (commentCounts % 10 > 0 ? 1 : 0);
            
            for (int i = 0; i < pageCnts; i++)
            {
                List<CommentInfo> newCommentList = null;
                try
                {
                    newCommentList = LoadCommentList(i + 1);
                }
                catch
                {
                    for(int j = 0; j < 3; j++)
                    {
                        try
                        {
                            Thread.Sleep(200);
                            newCommentList = LoadCommentList(i + 1);
                        }
                        catch
                        {
                            continue;
                        }

                        if (newCommentList != null)
                            break;
                    }

                    if(newCommentList == null)
                        throw new Exception("리플을 불러오는데 실패하였습니다.");
                }

                commentList.AddRange(newCommentList);
            }

            return commentList;
        }

        /// <summary>
        /// 갤로그의 쓴 글 목록을 요청하는 함수
        /// </summary>
        /// <param name="page">요청할 페이지 번호</param>
        /// <returns>해당 페이지의 글 목록</returns>
        private List<ArticleInfo> LoadArticleList(int page)
        {
            string html = HttpRequest.RequestWholePage(user_id, page, 1, ref cookies);
            List<ArticleInfo> articleList = HtmlParser.GetArticleList(html);

            return articleList;
        }

        /// <summary>
        /// 갤로그의 댓글 목록을 요청하는 함수
        /// </summary>
        /// <param name="page">요청할 페이지 번호</param>
        /// <returns>해당 페이지의 댓글 목록</returns>
        private List<CommentInfo> LoadCommentList(int page)
        {
            string html = HttpRequest.RequestWholePage(user_id, 1, page, ref cookies);
            List<CommentInfo> articleList = HtmlParser.GetCommentList(html);

            return articleList;
        }

        /// <summary>
        /// 갤러리에서 닉네임으로 검색하는 함수
        /// </summary>
        /// <param name="gall_id">갤러리 ID</param>
        /// <param name="gallType">갤러리 구분</param>
        /// <param name="nickname">사용자 ID</param>
        /// <param name="searchPos">검색 위치</param>
        /// <param name="searchPage">검색 페이지</param>
        /// <param name="cont">검색이 계속되는지 여부</param>
        /// <returns>검색된 글 목록</returns>
        public List<SearchedArticleInfo> SearchArticles(string gall_id, GalleryType gallType, string nickname, ref int searchPos, ref int searchPage, out bool cont)
        {
            List<SearchedArticleInfo> searchedArticleList = new List<SearchedArticleInfo>();
            int maxPage = 1;

            cont = false;

            if(searchPos != -1)
            {
                string searchHtml = "";

                try
                {
                    searchHtml = HttpRequest.RequestGalleryNickNameSearchPage(gall_id, gallType, nickname, ref searchPos, ref searchPage, ref cookies);
                }
                catch(ThreadAbortException) { throw; }
                catch(Exception ex)
                {
                    if(ex.Message == "해당 갤러리는 존재하지 않습니다.")
                    {
                        throw new Exception(ex.Message);
                    }

                    searchHtml = null;

                    for (int j = 0; j < 3; j++)
                    {
                        try
                        {
                            Thread.Sleep(200);
                            searchHtml = HttpRequest.RequestGalleryNickNameSearchPage(gall_id, gallType, nickname, ref searchPos, ref searchPage, ref cookies);

                            if (searchHtml != null)
                                break;
                        }
                        catch(ThreadAbortException) { throw; }
                        catch
                        {
                            continue;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(searchHtml))
                        throw new Exception("글을 검색하는데 실패하였습니다.");
                }

                int tmpPos = searchPos;

                List<SearchedArticleInfo> newSearchedList = HtmlParser.GetSearchedArticleList(searchHtml, gall_id, nickname, gallType, isLogin, ref searchPos, out maxPage);

                searchedArticleList.AddRange(newSearchedList);

                if (searchPage < maxPage)
                {
                    searchPage++;
                    searchPos = tmpPos;
                }
                else
                {
                    searchPage = 1;
                }

                cont = true;
            }
            else
            {
                cont = false;
            }

            return searchedArticleList;
        }

        /// <summary>
        /// 글을 삭제하는 함수
        /// </summary>
        /// <param name="info">삭제할 글 정보</param>
        /// <param name="both">갤러리만 삭제/갤로그 까지 삭제 여부</param>
        /// <returns>삭제 요청한 글의 정보</returns>
        public ArticleInfo DeleteArticle(ArticleInfo info, bool both)
        {
            // HTTP 요청에 딜레이를 주어 서버 오류 방지
            int delay = 50;

            string gall_id, gall_no, article_id, logNo;

            try
            {
                this.GetDeleteArticleInfo(info.DeleteURL, out gall_id, out gall_no, out article_id, out logNo);
            }
            catch(ThreadAbortException) { throw; }
            catch(Exception e)
            {
                info.ActualDelete = false;
                info.GallogDelete = false;
                info.DeleteMessage = e.Message;

                return info;
            }

            Thread.Sleep(delay);

            return DeleteArticle(info, gall_id, gall_no, article_id, logNo, GalleryType.Normal, both);
        }

        /// <summary>
        /// 글을 삭제하는 함수
        /// </summary>
        /// <param name="info">삭제할 글의 정보</param>
        /// <param name="gall_id">갤러리 ID</param>
        /// <param name="gall_no">갤러리 번호</param>
        /// <param name="article_id">글 ID</param>
        /// <param name="logNo">갤로그 로그 번호</param>
        /// <param name="gallType">갤러리 구분</param>
        /// <param name="both">갤러리만 삭제/갤로그도 삭제 여부</param>
        /// <returns>삭제 요청한 글의 정보</returns>
        public ArticleInfo DeleteArticle(ArticleInfo info, string gall_id, string gall_no, string article_id, string logNo, GalleryType gallType, bool both)
        {
            // HTTP 요청에 딜레이를 주어 서버 오류 방지
            int delay = 50;

            DeleteResult res1 = null;
            try
            {
                if (IsLogin)
                    res1 = HttpRequest.RequestDeleteArticle(gall_id, article_id, gallType, delay, ref cookies);
                else
                    res1 = HttpRequest.RequestDeleteFlowArticle(gall_id, article_id, logNo, gallType, delay, ref cookies);   // logNo를 패스워드로 사용
            }
            catch (ThreadAbortException) { throw; }
            catch (Exception ex)
            {
                info.ActualDelete = false;
                info.GallogDelete = false;
                info.DeleteMessage = "갤러리의 글을 지우는데 실패하였습니다 - [" + ex.Message + "]";

                return info;
            }

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

                DeleteResult res2 = null;

                try
                {
                    res2 = HttpRequest.RequestDeleteGallogArticle(user_id, gall_id, gall_no, article_id, logNo, delay, ref cookies);
                }
                catch (ThreadAbortException) { throw; }
                catch (Exception ex)
                {
                    info.ActualDelete = true;
                    info.GallogDelete = false;
                    info.DeleteMessage = "갤로그의 글을 지우는데 실패하였습니다 - [" + ex.Message + "]";

                    return info;
                }

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

        /// <summary>
        /// 댓글을 삭제하는 함수
        /// </summary>
        /// <param name="info">삭제할 댓글의 정보</param>
        /// <param name="both">갤러리만/갤로그도 삭제 여부</param>
        /// <returns>삭제 요청한 댓글의 정보</returns>
        public CommentInfo DeleteComment(CommentInfo info, bool both)
        {
            // HTTP 요청에 딜레이를 주어 서버 오류 방지
            int delay = 50;

            string gall_id, gall_no, article_id, comment_id, logNo;

            try
            {
                this.GetDeleteCommentInfo(info.DeleteURL, out gall_id, out gall_no, out article_id, out comment_id, out logNo);
            }
            catch (ThreadAbortException) { throw; }
            catch (Exception e)
            {
                info.ActualDelete = false;
                info.GallogDelete = false;
                info.DeleteMessage = e.Message;

                return info;
            }

            Thread.Sleep(delay);

            return DeleteComment(info, gall_id, gall_no, article_id, comment_id, logNo, both);
        }

        /// <summary>
        /// 댓글을 삭제하는 함수
        /// </summary>
        /// <param name="info">삭제할 댓글의 정보</param>
        /// <param name="gall_id">갤러리 ID</param>
        /// <param name="gall_no">갤러리 번호</param>
        /// <param name="article_id">글 ID</param>
        /// <param name="comment_id">댓글 ID</param>
        /// <param name="logNo">갤로그 Log 번호</param>
        /// <param name="both">갤러리만/갤로그도 삭제 여부</param>
        /// <returns>삭제 요청한 댓글의 정보</returns>
        public CommentInfo DeleteComment(CommentInfo info, string gall_id, string gall_no, string article_id, string comment_id, string logNo, bool both)
        {
            // HTTP 요청에 딜레이를 주어 서버 오류 방지
            int delay = 50;

            DeleteResult res1 = null;

            try
            {
                res1 = HttpRequest.RequestDeleteComment(gall_id, article_id, comment_id, ref cookies);
            }
            catch (ThreadAbortException) { throw; }
            catch (Exception ex)
            {
                info.ActualDelete = false;
                info.GallogDelete = false;
                info.DeleteMessage = "갤러리의 리플을 지우는데 실패하였습니다 - [" + ex.Message + "]";

                return info;
            }

            if (!res1.Success && res1.ErrorMessage != "이미 삭제된 리플입니다.")
            {
                info.ActualDelete = false;
                info.GallogDelete = false;
                info.DeleteMessage = "갤러리의 리플을 지우는데 실패하였습니다 - [" + res1.ErrorMessage + "]";

                return info;
            }

            if (both)
            {
                Thread.Sleep(delay);

                DeleteResult res2 = null;

                try
                {
                    res2 = HttpRequest.RequestDeleteGallogComment(user_id, gall_id, gall_no, article_id, comment_id, logNo, delay, ref cookies);
                }
                catch (Exception ex)
                {
                    info.ActualDelete = true;
                    info.GallogDelete = false;
                    info.DeleteMessage = "갤로그의 리플을 지우는데 실패하였습니다 - [" + ex.Message + "]";

                    return info;
                }

                if (!res2.Success)
                {
                    info.ActualDelete = true;
                    info.GallogDelete = false;
                    info.DeleteMessage = "갤로그의 리플을 지우는데 실패하였습니다 - [" + res2.ErrorMessage + "]";

                    return info;
                }
            }

            info.ActualDelete = true;
            info.GallogDelete = true;
            info.DeleteMessage = "";

            return info;
        }

        /// <summary>
        /// 삭제할 글의 정보를 가져오는 함수
        /// </summary>
        /// <param name="url">삭제할 글의 갤로그 URL</param>
        /// <param name="gall_id">갤러리 ID</param>
        /// <param name="gall_no">갤러리 번호</param>
        /// <param name="article_id">글 ID</param>
        /// <param name="logNo">갤로그 로그 번호</param>
        private void GetDeleteArticleInfo(string url, out string gall_id, out string gall_no, out string article_id, out string logNo)
        {
            gall_id = null;
            gall_no = null;
            article_id = null;
            logNo = null;

            string galHtml = HttpRequest.RequestDeleteGallogArticlePage(url, user_id, ref cookies);
            HtmlParser.GetGallogArticleInfo(galHtml, out gall_id, out gall_no, out article_id, out logNo);
        }

        /// <summary>
        /// 삭제할 댓글의 정보를 가져오는 함수
        /// </summary>
        /// <param name="url">삭제할 댓글의 갤로그 URL</param>
        /// <param name="gall_id">갤러리 ID</param>
        /// <param name="gall_no">갤러리 번호</param>
        /// <param name="article_id">글 ID</param>
        /// <param name="comment_id">댓글 ID</param>
        /// <param name="logNo">갤러리 로그 번호</param>
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