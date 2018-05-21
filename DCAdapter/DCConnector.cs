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

        public LoginInformation LoginInfo { get; }

        public DCConnector()
        {
            LoginInfo = new LoginInformation();
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

            LoginInfo.Status = await PostLoginAsync(id, pw);

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
        /// 갤로그의 항목 목록을 불러옵니다.
        /// </summary>
        /// <returns>갤로그의 항목 목록을 반환합니다.</returns>
        public async Task<Tuple<List<T>, bool>> 
            LoadGallogItemAsync<T>(int page, CancellationToken token = default(CancellationToken))
        {
            if (typeof(T) != typeof(ArticleInformation) && typeof(T) != typeof(CommentInformation))
                throw new NotSupportedException();

            return await Task.Run(async () =>
            {
                string html = "";
                int itemCount = 0, pageCnt;
                bool cont = false;

                try
                {
                    // 갤로그 메인페이지의 HTML 소스를 요청
                    html = await GetGallogMainPageAsync(user_id);
                }
                catch
                {
                    throw;
                }

                // 갤로그의 HTML 소스에서 총 쓴 항목의 갯수를 가져와서 총 페이지 갯수를 지정.
                itemCount = await HtmlParser.GetItemCountAsync<T>(html);

                pageCnt = itemCount / 10 + (itemCount % 10 > 0 ? 1 : 0);

                if(pageCnt == 0)
                    return new Tuple<List<T>, bool>(new List<T>(), cont);

                if (page < 0 || page > pageCnt)
                    throw new ArgumentOutOfRangeException();

                if (page < pageCnt)
                    cont = true;

                List<T> itemList = null;
                object loadResult;

                try
                {
                    loadResult = await LoadItemListAsync<T>(page);
                    itemList = (List<T>)loadResult;
                }
                catch
                {
                    for (int i = 0; i < 5; i++)
                    {
                        try
                        {
                            await Task.Delay(200);
                            loadResult = await LoadItemListAsync<T>(page);
                            itemList = (List<T>)loadResult;
                        }
                        catch
                        {
                            continue;
                        }

                        if (itemList != null)
                            break;
                    }

                    if (itemList == null)
                        throw new Exception("항목을 불러오는데 실패하였습니다.");
                }

                // 저장한 리스트를 반환
                return new Tuple<List<T>, bool>(itemList, cont);
            }, token);
        }

        /// <summary>
        /// 갤로그의 쓴 항목 목록을 요청하는 함수
        /// </summary>
        /// <param name="page">요청할 페이지 번호</param>
        /// <returns>해당 페이지의 항목 목록</returns>
        private async Task<List<T>> LoadItemListAsync<T>(int page)
        {
            if (typeof(T) != typeof(ArticleInformation) && typeof(T) != typeof(CommentInformation))
                throw new NotSupportedException();

            string html = null;
            if (typeof(T) == typeof(ArticleInformation))
            {
                html = await GetGallogListPageAsync(user_id, page, 1);
            }
            else if (typeof(T) == typeof(CommentInformation))
            {
                html = await GetGallogListPageAsync(user_id, 1, page);
            }
            List<T> itemList = await HtmlParser.GetItemListAsync<T>(html);

            return itemList;
        }

        /// <summary>
        /// 갤러리에서 닉네임으로 검색하는 함수
        /// </summary>
        /// <param name="gall_id">갤러리 ID</param>
        /// <param name="gallType">갤러리 구분</param>
        /// <param name="nickname">사용자 ID</param>
        /// <param name="searchPos">검색 위치</param>
        /// <param name="searchPage">검색 페이지</param>
        /// <returns>검색된 글 목록</returns>
        public async Task<Tuple<List<ArticleInformation>, int, int, bool>> 
            SearchArticleAsync(string gall_id, GalleryType gallType, string nickname, int searchPos, int searchPage, CancellationToken token = default(CancellationToken))
        {
            return await Task.Run(async () =>
            {
                List<ArticleInformation> searchedArticleList = new List<ArticleInformation>();
                int maxPage = 1;

                // 검색이 계속되는지 여부
                bool cont = false;

                if (searchPos != -1)
                {
                    Tuple<string, int, int> req = null;
                    string searchHtml = "";

                    try
                    {
                        req = await GetGalleryNicknameSearchPageAsync(gall_id, gallType, nickname, searchPos, searchPage);
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
                                req = await GetGalleryNicknameSearchPageAsync(gall_id, gallType, nickname, searchPos, searchPage);

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

                    var parseResult = await HtmlParser.GetSearchedArticleList(searchHtml, gall_id, nickname, gallType, LoginInfo.IsLoggedIn, searchPos);
                    List<ArticleInformation> newSearchedList = parseResult.Item1;
                    searchPos = parseResult.Item2;
                    maxPage = parseResult.Item3;

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

                return new Tuple<List<ArticleInformation>, int, int, bool>(searchedArticleList, searchPos, searchPage, cont);
            }, token);
        }
        
        public async Task<ArticleInformation> DeleteArticle(ArticleInformation info, bool both)
        {
            // HTTP 요청에 딜레이를 주어 서버 오류 방지
            int delay = 50;
            
            GallogArticleDeleteParameter delParams = null;
            try
            {
                delParams = await this.GetDeleteArticleInfoAsync(info.DeleteUrl);
            }
            catch(Exception e)
            {
                info.IsGalleryDeleted = false;
                info.IsGallogDeleted = false;
                info.DeleteMessage = e.Message;

                return info;
            }

            info.GalleryDeleteParameter = new GalleryArticleDeleteParameter()
            {
                GalleryId = delParams.GalleryId,
                ArticleID = delParams.ArticleId
            };
            info.GallogDeleteParameter = delParams;

            await Task.Delay(delay);

            return await DeleteArticle(info, GalleryType.Normal, both);
        }
        
        public async Task<ArticleInformation> DeleteArticle(ArticleInformation info, GalleryType gallType, bool both)
        {
            // HTTP 요청에 딜레이를 주어 서버 오류 방지
            int delay = 100;
            
            DeleteResult res1 = null;
            try
            {
                if (LoginInfo.IsLoggedIn)
                    res1 = await PostDeleteGalleryArticleAsync(info.GalleryDeleteParameter, gallType, delay);
                else
                    res1 = await PostDeleteGalleryFlowArticleAsync(info.GalleryDeleteParameter, gallType, delay);
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
                await Task.Delay(delay);

                DeleteResult res2 = null;

                try
                {
                    res2 = await PostDeleteGallogArticleAsync(info.GallogDeleteParameter, delay);
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
        
        public async Task<CommentInformation> DeleteComment(CommentInformation info, bool both)
        {
            // HTTP 요청에 딜레이를 주어 서버 오류 방지
            int delay = 50;
            GallogCommentDeleteParameter delParams = null;

            try
            {
                delParams = await this.GetDeleteCommentInfoAsync(info.DeleteUrl);
            }
            catch (Exception e)
            {
                info.IsGalleryDeleted = false;
                info.IsGallogDeleted = false;
                info.DeleteMessage = e.Message;

                return info;
            }

            info.GalleryDeleteParameter = new GalleryCommentDeleteParameter()
            {
                GalleryId = delParams.GalleryId,
                ArticleId = delParams.ArticleId,
                CommentId = delParams.CommentId
            };
            info.GallogDeleteParameter = delParams;

            await Task.Delay(delay);

            return await DeleteComment(info, true, both);
        }
        
        public async Task<CommentInformation> DeleteComment(CommentInformation info, bool actualDelete, bool both)
        {
            // HTTP 요청에 딜레이를 주어 서버 오류 방지
            int delay = 100;

            DeleteResult res1 = null;

            try
            {
                res1 = await PostDeleteGalleryCommentAsync(info.GalleryDeleteParameter);
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
                await Task.Delay(delay);

                DeleteResult res2 = null;

                try
                {
                    res2 = await PostDeleteGallogCommentAsync(info.GallogDeleteParameter, delay);
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
        
        private async Task<GallogArticleDeleteParameter> GetDeleteArticleInfoAsync(string url)
        {
            string galHtml = await GetDeleteGallogArticlePageAsync(url, user_id);
            GallogArticleDeleteParameter retParams = await HtmlParser.GetDeleteGallogArticleParameterAsync(galHtml);
            retParams.UserId = user_id;
            return retParams;
        }
        
        private async Task<GallogCommentDeleteParameter> GetDeleteCommentInfoAsync(string url)
        {
            string galHtml = await GetDeleteGallogCommentPageAsync(url, user_id);
            GallogCommentDeleteParameter retParams = await HtmlParser.GetDeleteGallogCommentParameterAsync(galHtml);
            retParams.UserId = user_id;
            return retParams;
        }
    }
}