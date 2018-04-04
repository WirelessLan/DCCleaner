using System.Net;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DCAdapter
{
    /// <summary>
    /// DC인사이드 연결을 처리합니다.
    /// </summary>
    public partial class DCConnector
    {
        CookieContainer cookies;    // 내부 연결에 사용되는 쿠키 컨테이너입니다.
        string user_id = "";

        public LoginInfo LoginInfo { get; }

        public DCConnector()
        {
            LoginInfo = new LoginInfo();
            cookies = new CookieContainer();
        }

        /// <summary>
        /// DC 인사이드 서버에 주어진 ID와 비밀번호로 로그인을 요청합니다.
        /// </summary>
        /// <param name="id">사용자의 ID</param>
        /// <param name="pw">사용자의 비밀번호</param>
        /// <returns>로그인 성공시 True, 실패시 False를 반환합니다.</returns>
        public async Task<bool> Login(string id, string pw)
        {
            if(string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(pw))
            {
                LoginInfo.Status = LoginStatus.ErrorBoth;
                LoginInfo.ErrorMessage = "아이디 또는 비밀번호를 입력해주세요.";
                return false;
            }

            LoginInfo.Status = await RequestLoginAsync(id, pw);

            if (LoginInfo.Status == LoginStatus.Success)
            {
                LoginInfo.IsLoggedIn = true;

                LoginInfo.ErrorMessage = null;
                user_id = id;

                return true;
            }
            else
            {
                LoginInfo.IsLoggedIn = false;

                if (LoginInfo.Status == LoginStatus.IDError)
                    LoginInfo.ErrorMessage = "존재하지 않는 아이디입니다.";
                else if (LoginInfo.Status == LoginStatus.PasswordError)
                    LoginInfo.ErrorMessage = "잘못된 비밀번호입니다.";
                else if (LoginInfo.Status == LoginStatus.ErrorBoth)
                    LoginInfo.ErrorMessage = "아이디 또는 비밀번호가 잘못되었습니다.";
                else if(LoginInfo.Status == LoginStatus.Unknown)
                    LoginInfo.ErrorMessage = "서버 통신중 알 수없는 에러가 발생하였습니다.";

                return false;
            }
        }

        /// <summary>
        /// 갤로그의 글 목록을 불러옵니다.
        /// </summary>
        /// <returns>갤로그의 글 목록을 반환합니다.</returns>
        public async Task<List<ArticleInfo>> LoadGallogArticles()
        {
            string html = "";
            int articleCounts;
            int pageCnts;

            List<ArticleInfo> articleList = new List<ArticleInfo>();

            try
            {
                // 갤로그의 HTML 소스를 요청
                html = await RequestGallogMainPage(user_id);
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
                List<ArticleInfo> newArticleList = null;
                try
                {
                    newArticleList = await LoadArticleList(i + 1);
                }
                catch
                {
                    for (int j = 0; j < 3; j++)
                    {
                        try
                        {
                            await Task.Delay(200);
                            newArticleList = await LoadArticleList(i + 1);
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
        public async Task<List<CommentInfo>> LoadGallogComments()
        {
            string html = "";
            int commentCounts;
            int pageCnts;

            List<CommentInfo> commentList = new List<CommentInfo>();

            try
            {
                // 갤로그의 HTML 소스를 요청
                html = await RequestGallogMainPage(this.user_id);
            }
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
                    newCommentList = await LoadCommentList(i + 1);
                }
                catch
                {
                    for(int j = 0; j < 3; j++)
                    {
                        try
                        {
                            await Task.Delay(200);
                            newCommentList = await LoadCommentList(i + 1);
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
        private async Task<List<ArticleInfo>> LoadArticleList(int page)
        {
            string html = await RequestGallogListPage(user_id, page, 1);
            List<ArticleInfo> articleList = HtmlParser.GetArticleList(html);

            return articleList;
        }

        /// <summary>
        /// 갤로그의 댓글 목록을 요청하는 함수
        /// </summary>
        /// <param name="page">요청할 페이지 번호</param>
        /// <returns>해당 페이지의 댓글 목록</returns>
        private async Task<List<CommentInfo>> LoadCommentList(int page)
        {
            string html = await RequestGallogListPage(user_id, 1, page);
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
        public async Task<Tuple<List<ArticleInfo>, int, int, bool>> SearchArticles(string gall_id, GalleryType gallType, string nickname, int searchPos, int searchPage, bool cont)
        {
            List<ArticleInfo> searchedArticleList = new List<ArticleInfo>();
            int maxPage = 1;

            cont = false;

            if (searchPos != -1)
            {
                Tuple<string, int, int> req = null;
                string searchHtml = "";

                try
                {
                    req = await RequestGalleryNicknameSearchPage(gall_id, gallType, nickname, searchPos, searchPage);
                }
                catch (Exception ex)
                {
                    if (ex.Message == "해당 갤러리는 존재하지 않습니다.")
                    {
                        throw new Exception(ex.Message);
                    }

                    searchHtml = null;

                    for (int j = 0; j < 5; j++)
                    {
                        try
                        {
                            await Task.Delay(500);
                            req = await RequestGalleryNicknameSearchPage(gall_id, gallType, nickname, searchPos, searchPage);

                            if (req != null && !string.IsNullOrWhiteSpace(req.Item1))
                                break;
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    if (req == null || string.IsNullOrWhiteSpace(req.Item1))
                        throw new Exception("글을 검색하는데 실패하였습니다.");
                }

                searchHtml = req.Item1;
                searchPos = req.Item2;
                searchPage = req.Item3;

                int tmpPos = searchPos;

                List<ArticleInfo> newSearchedList = HtmlParser.GetSearchedArticleList(searchHtml, gall_id, nickname, gallType, LoginInfo.IsLoggedIn, ref searchPos, out maxPage);

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

            return new Tuple<List<ArticleInfo>, int, int, bool>(searchedArticleList, searchPos, searchPage, cont);
        }
        
        public async Task<ArticleInfo> DeleteArticle(ArticleInfo info, bool both)
        {
            // HTTP 요청에 딜레이를 주어 서버 오류 방지
            int delay = 50;
            
            GallogArticleDeleteParameters delParams = null;
            try
            {
                delParams = await this.GetDeleteArticleInfo(info.DeleteUrl);
            }
            catch(Exception e)
            {
                info.IsGalleryDeleted = false;
                info.IsGallogDeleted = false;
                info.DeleteMessage = e.Message;

                return info;
            }

            info.GalleryArticleDeleteParameters = new GalleryArticleDeleteParameters()
            {
                GalleryId = delParams.GalleryId,
                ArticleID = delParams.ArticleId
            };
            info.GallogArticleDeleteParameters = delParams;

            Thread.Sleep(delay);

            return await DeleteArticle(info, GalleryType.Normal, both);
        }
        
        public async Task<ArticleInfo> DeleteArticle(ArticleInfo info, GalleryType gallType, bool both)
        {
            // HTTP 요청에 딜레이를 주어 서버 오류 방지
            int delay = 50;
            
            DeleteResult res1 = null;
            try
            {
                if (LoginInfo.IsLoggedIn)
                    res1 = await RequestDeleteGalleryArticle(info.GalleryArticleDeleteParameters, gallType, delay);
                else
                    res1 = await RequestDeleteGalleryFlowArticle(info.GalleryArticleDeleteParameters, gallType, delay);
            }
            catch (Exception ex)
            {
                info.IsGalleryDeleted = false;
                info.IsGallogDeleted = false;
                info.DeleteMessage = "갤러리의 글을 지우는데 실패하였습니다 - [" + ex.Message + "]";

                return info;
            }

            if (!res1.Success && res1.ErrorMessage != "이미 삭제된 글입니다.")
            {
                info.IsGalleryDeleted = false;
                info.IsGallogDeleted = false;
                info.DeleteMessage = "갤러리의 글을 지우는데 실패하였습니다 - [" + res1.ErrorMessage + "]";

                return info;
            }
            
            if (both)
            {
                Thread.Sleep(delay);

                DeleteResult res2 = null;

                try
                {
                    res2 = await RequestDeleteGallogArticle(info.GallogArticleDeleteParameters, delay);
                }
                catch (Exception ex)
                {
                    info.IsGalleryDeleted = true;
                    info.IsGallogDeleted = false;
                    info.DeleteMessage = "갤로그의 글을 지우는데 실패하였습니다 - [" + ex.Message + "]";

                    return info;
                }

                if (!res2.Success)
                {
                    info.IsGalleryDeleted = true;
                    info.IsGallogDeleted = false;
                    info.DeleteMessage = "갤로그의 글을 지우는데 실패하였습니다 - [" + res2.ErrorMessage + "]";

                    return info;
                }
            }

            info.IsGalleryDeleted = true;
            info.IsGallogDeleted = true;
            return info;
        }
        
        public async Task<CommentInfo> DeleteComment(CommentInfo info, bool both)
        {
            // HTTP 요청에 딜레이를 주어 서버 오류 방지
            int delay = 50;
            GallogCommentDeleteParameters delParams = null;

            try
            {
                delParams = await this.GetDeleteCommentInfo(info.DeleteUrl);
            }
            catch (Exception e)
            {
                info.IsGalleryDeleted = false;
                info.IsGallogDeleted = false;
                info.DeleteMessage = e.Message;

                return info;
            }

            info.GalleryCommentDeleteParameters = new GalleryCommentDeleteParameters()
            {
                GalleryId = delParams.GalleryId,
                ArticleId = delParams.ArticleId,
                CommentId = delParams.CommentId
            };
            info.GallogCommentDeleteParameters = delParams;

            Thread.Sleep(delay);

            return await DeleteComment(info, true, both);
        }
        
        public async Task<CommentInfo> DeleteComment(CommentInfo info, bool actualDelete, bool both)
        {
            // HTTP 요청에 딜레이를 주어 서버 오류 방지
            int delay = 50;

            DeleteResult res1 = null;

            try
            {
                res1 = await RequestDeleteGalleryComment(info.GalleryCommentDeleteParameters);
            }
            catch (Exception ex)
            {
                info.IsGalleryDeleted = false;
                info.IsGallogDeleted = false;
                info.DeleteMessage = "갤러리의 리플을 지우는데 실패하였습니다 - [" + ex.Message + "]";

                return info;
            }

            if (!res1.Success && res1.ErrorMessage != "이미 삭제된 리플입니다.")
            {
                info.IsGalleryDeleted = false;
                info.IsGallogDeleted = false;
                info.DeleteMessage = "갤러리의 리플을 지우는데 실패하였습니다 - [" + res1.ErrorMessage + "]";

                return info;
            }

            if (both)
            {
                Thread.Sleep(delay);

                DeleteResult res2 = null;

                try
                {
                    res2 = await RequestDeleteGallogComment(info.GallogCommentDeleteParameters, delay);
                }
                catch (Exception ex)
                {
                    info.IsGalleryDeleted = true;
                    info.IsGallogDeleted = false;
                    info.DeleteMessage = "갤로그의 리플을 지우는데 실패하였습니다 - [" + ex.Message + "]";

                    return info;
                }

                if (!res2.Success)
                {
                    info.IsGalleryDeleted = true;
                    info.IsGallogDeleted = false;
                    info.DeleteMessage = "갤로그의 리플을 지우는데 실패하였습니다 - [" + res2.ErrorMessage + "]";

                    return info;
                }
            }

            info.IsGalleryDeleted = true;
            info.IsGallogDeleted = true;
            info.DeleteMessage = "";

            return info;
        }
        
        private async Task<GallogArticleDeleteParameters> GetDeleteArticleInfo(string url)
        {
            string galHtml = await RequestDeleteGallogArticleSource(url, user_id);
            GallogArticleDeleteParameters retParams = HtmlParser.GetDeleteGallogArticleParameters(galHtml);
            retParams.UserId = user_id;
            return retParams;
        }
        
        private async Task<GallogCommentDeleteParameters> GetDeleteCommentInfo(string url)
        {
            string galHtml = await RequestDeleteGallogCommentSource(url, user_id);
            GallogCommentDeleteParameters retParams = HtmlParser.GetDeleteGallogCommentParameters(galHtml);
            retParams.UserId = user_id;
            return retParams;
        }
    }
}