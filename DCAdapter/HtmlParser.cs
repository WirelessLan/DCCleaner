﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Web;
using System.Threading.Tasks;

namespace DCAdapter
{
    class HtmlParser
    {
        /// <summary>
        /// 갤로그의 글 갯수를 가져오는 함수
        /// </summary>
        /// <param name="html">갤로그의 HTML 소스</param>
        /// <returns>글 갯수</returns>
        internal static async Task<int> GetItemCountAsync<T>(string html)
        {
            if (typeof(T) != typeof(ArticleInformation) && typeof(T) != typeof(CommentInformation))
                throw new NotSupportedException();

            return await Task.Run(() =>
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                int criticalIndexer = 0;

                if (typeof(T) == typeof(ArticleInformation))
                    criticalIndexer = 1;
                else if (typeof(T) == typeof(CommentInformation))
                    criticalIndexer = 3;

                string xPath = "//div[@id='statusDiv']/table[1]/tr[1]/td[1]/table[1]/tr[1]/td[1]/table[1]/tr[1]/td[1]/table[1]/tr["+ criticalIndexer + "]/td[1]/font[1]";

                HtmlNode cntNode = doc.DocumentNode.SelectSingleNode(xPath);

                return ExtractNumberAsync(cntNode.InnerText);
            });
        }

        /// <summary>
        /// 문자열에서 정수를 추출하는 함수
        /// </summary>
        /// <param name="text">정수값을 추출할 문자열</param>
        /// <returns>추출된 정수</returns>
        private static async Task<int> ExtractNumberAsync(string text)
        {
            return await Task.Run(() =>
            {
                string result = Regex.Replace(text, @"[^\d]", "");

                return int.Parse(result);
            });
        }

        /// <summary>
        /// 삭제할 글의 파라미터를 가져오는 함수
        /// </summary>
        /// <param name="html">글 삭제 페이지의 HTML 소스</param>
        /// <param name="gallType">갤러리 구분</param>
        /// <param name="delete_Params">글 삭제에 필요한 파라미터</param>
        /// <param name="lately_gallery">최근 방문한 갤러리</param>
        internal static async Task<Tuple<ParameterStorage, string>> GetDeleteArticleParameterAsync(string html, GalleryType gallType)
        {
            return await Task.Run(async () =>
            {
                string lately_gallery = null;
                ParameterStorage delete_Params = new ParameterStorage();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                HtmlNode deleteNode = null;
                try
                {
                    deleteNode = doc.GetElementbyId("id").ParentNode.SelectSingleNode(".//form");
                    lately_gallery = doc.GetElementbyId("lately_gallery").GetAttributeValue("value", "");
                }
                catch { }

                // 삭제 노드가 없는 경우 이미 삭제된 글이거나 오류
                if (deleteNode == null)
                {
                    if (html.Contains("/error/deleted"))
                    {
                        throw new Exception("이미 삭제된 글입니다.");
                    }
                    else if (doc.GetElementbyId("password_confirm") != null)
                    {
                        throw new Exception("이미 삭제된 글입니다.");
                    }
                    else
                    {
                        throw new Exception("알 수 없는 오류입니다.");
                    }
                }

                // 회원글인데 비밀번호 입력하는 페이지인 경우 이미 삭제된 글
                if (deleteNode.Attributes["action"].Value.Contains("delete_password_submit"))
                    throw new Exception("이미 삭제된 글입니다.");

                foreach (HtmlNode input in deleteNode.ParentNode.Descendants("input").Where(n => n.GetAttributeValue("type", "") == "hidden"))
                    delete_Params.Push(input.GetAttributeValue("name", ""), input.GetAttributeValue("value", ""));

                string jsEncCode;
                ParameterStorage jsParam;
                string jsScript = "";

                foreach (HtmlNode scriptNode in doc.DocumentNode.Descendants("script"))
                    jsScript += scriptNode.InnerHtml;

                if (jsScript == "")
                    throw new Exception("알 수 없는 오류입니다.");

                // 글 삭제시 실행되는 스크립트의 추가 값을 가져옴
                var parseResult = await JSParser.GetDeleteGalleryArticleParameterAsync(jsScript, gallType);
                jsEncCode = parseResult.Item1;
                jsParam = parseResult.Item2;

                delete_Params.Push(jsParam);
                if (gallType == GalleryType.Normal)
                    delete_Params["service_code"] = Cryption.DecryptCode(jsEncCode, delete_Params["service_code"]);

                return new Tuple<ParameterStorage, string>(delete_Params, lately_gallery);
            });
        }

        internal static async Task<Tuple<ParameterStorage, string>> GetDeleteFlowArticleParameterAsync(string html, GalleryType gallType)
        {
            return await Task.Run(async () =>
            {
                ParameterStorage delete_Params = new ParameterStorage();
                string lately_gallery = null;

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                HtmlNode deleteNode = null;
                try
                {
                    deleteNode = doc.GetElementbyId("id").ParentNode.SelectSingleNode(".//form");
                    if (gallType == GalleryType.Normal)
                        lately_gallery = doc.GetElementbyId("lately_gallery").GetAttributeValue("value", "");
                }
                catch { }

                if (deleteNode == null)
                {
                    if (html.Contains("/error/deleted"))
                    {
                        throw new Exception("이미 삭제된 글입니다.");
                    }
                    else
                    {
                        throw new Exception("알 수 없는 오류입니다.");
                    }
                }

                foreach (HtmlNode input in deleteNode.ParentNode.Descendants("input").Where(n => n.GetAttributeValue("type", "") == "hidden"))
                    delete_Params.Push(input.GetAttributeValue("name", ""), input.GetAttributeValue("value", ""));

                string jsEncCode;
                ParameterStorage jsParam;
                string jsScript = "";

                foreach (HtmlNode scriptNode in doc.DocumentNode.Descendants("script"))
                    jsScript += scriptNode.InnerHtml;

                if (jsScript == "")
                    throw new Exception("알 수 없는 오류입니다.");

                var parseResult = await JSParser.GetDeleteGalleryArticleParameterAsync(jsScript, gallType);
                jsEncCode = parseResult.Item1;
                jsParam = parseResult.Item2;

                delete_Params.Push(jsParam);
                if (gallType == GalleryType.Normal)
                    delete_Params["service_code"] = Cryption.DecryptCode(jsEncCode, delete_Params["service_code"]);

                return new Tuple<ParameterStorage, string>(delete_Params, lately_gallery);
            });
        }

        internal static async Task<string> GetDeleteCommentParameterAsync(string pageHtml)
        {
            return await Task.Run(() =>
            {
                string check7 = null;
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(pageHtml);

                HtmlNode chk7Node = doc.DocumentNode.SelectSingleNode("//input[@id='check_7']");

                if (chk7Node == null)
                {
                    if (pageHtml.Contains("/error/deleted") || pageHtml.Contains("/error/comment_error"))
                    {
                        throw new Exception("이미 삭제된 리플입니다.");
                    }
                    else
                    {
                        throw new Exception("알 수 없는 오류입니다.");
                    }
                }

                check7 = chk7Node.Attributes["value"].Value;
                return check7;
            });
        }

        internal static async Task<List<T>> GetItemListAsync<T>(string html)
        {
            if (typeof(T) != typeof(ArticleInformation) && typeof(T) != typeof(CommentInformation))
                throw new NotSupportedException();

            return await Task.Run(async () =>
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                List<T> itemList = new List<T>();

                if (typeof(T) == typeof(ArticleInformation))
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//img[@src='http://wstatic.dcinside.com/gallery/skin/gallog/icon_01.gif']"))
                    {
                        if (node.Attributes["onClick"] != null)
                        {
                            string title = node.ParentNode.PreviousSibling.PreviousSibling.PreviousSibling.PreviousSibling.InnerText;
                            title = HttpUtility.HtmlDecode(title).Trim();
                            string url = await GetAbsoulteURL(node.Attributes["onClick"].Value);
                            string date = node.ParentNode.InnerText;

                            object nItem = new ArticleInformation(title, date, url);
                            itemList.Add((T)nItem);
                        }
                    }
                else if (typeof(T) == typeof(CommentInformation))
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//table[@bgcolor='#F2F2F5']/tr"))
                    {
                        if (node.Descendants("td").Count() == 6)
                        {
                            string name = node.SelectSingleNode("./td[1]").InnerText;
                            name = HttpUtility.HtmlDecode(name).Trim();
                            string content = HttpUtility.HtmlDecode(node.SelectSingleNode("./td[3]").InnerText);
                            string date = node.SelectSingleNode("./td[5]").InnerText;

                            string url = await GetAbsoulteURL(node.SelectSingleNode("./td[6]/span").Attributes["onClick"].Value);

                            object nItem = new CommentInformation(name, content, date, url);
                            itemList.Add((T)nItem);
                        }
                    }

                return itemList;
            });
        }

        private static async Task<string> GetAbsoulteURL(string value)
        {
            return await Task.Run(() =>
            {
                const string gallogDefaultDomain = "http://gallog.dcinside.com/inc/";
                string prefix = "document.location.href=";
                string retVal = value.Replace(prefix, "");
                retVal = retVal.Replace("'", "").Replace("\"", "").Replace(";", "");

                return gallogDefaultDomain + retVal.Trim();
            });
        }

        internal static async Task<GallogArticleDeleteParameter> GetDeleteGallogArticleParameterAsync(string pageHtml)
        {
            return await Task.Run(() =>
            {
                GallogArticleDeleteParameter newParams = new GallogArticleDeleteParameter();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(pageHtml);

                HtmlNode parentNode = doc.GetElementbyId("dTp").ParentNode;
                HtmlNode gallNode = parentNode.SelectSingleNode(".//input[@name='id']");
                HtmlNode cidNode = parentNode.SelectSingleNode(".//input[@name='cid']");
                HtmlNode artNode = parentNode.SelectSingleNode(".//input[@name='pno']");
                HtmlNode logNode = parentNode.SelectSingleNode(".//input[@name='logNo']");
                HtmlNode dcc_keyNode = doc.DocumentNode.SelectSingleNode("//input[@name='dcc_key']");
                int inputCnt = doc.DocumentNode.Descendants("input").Count();
                HtmlNode randomKeyNode = doc.DocumentNode.SelectSingleNode("//input[" + (inputCnt - 1) + "]");

                newParams.GalleryId = gallNode.Attributes["value"].Value;
                newParams.GalleryNo = cidNode.Attributes["value"].Value;
                newParams.ArticleId = artNode.Attributes["value"].Value;
                newParams.LogNo = logNode.Attributes["value"].Value;
                newParams.DCCKey = dcc_keyNode.Attributes["value"].Value;
                newParams.AdditionalParameter.Push(randomKeyNode.Attributes["name"].Value, randomKeyNode.Attributes["value"].Value);

                return newParams;
            });
        }

        internal static async Task<GallogCommentDeleteParameter> GetDeleteGallogCommentParameterAsync(string pageHtml)
        {
            return await Task.Run(() =>
            {
                GallogCommentDeleteParameter newParams = new GallogCommentDeleteParameter();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(pageHtml);

                HtmlNode parentNode = doc.GetElementbyId("dTp").ParentNode;
                HtmlNode idNode = parentNode.SelectSingleNode(".//input[@name='id']");
                HtmlNode cidNode = parentNode.SelectSingleNode(".//input[@name='cid']");
                HtmlNode artNode = parentNode.SelectSingleNode(".//input[@name='no']");
                HtmlNode cNode = parentNode.SelectSingleNode(".//input[@name='c_no']");
                HtmlNode logNode = parentNode.SelectSingleNode(".//input[@name='logNo']");
                int inputCnt = doc.DocumentNode.Descendants("input").Count();
                HtmlNode randomKeyNode = doc.DocumentNode.SelectSingleNode("//input[" + (inputCnt - 1) + "]");

                newParams.GalleryId = idNode.Attributes["value"].Value;
                newParams.GalleryNo = cidNode.Attributes["value"].Value;
                newParams.ArticleId = artNode.Attributes["value"].Value;
                newParams.CommentId = cNode.Attributes["value"].Value;
                newParams.LogNo = logNode.Attributes["value"].Value;
                newParams.AdditionalParameter.Push(randomKeyNode.Attributes["name"].Value, randomKeyNode.Attributes["value"].Value);

                return newParams;
            });
        }
        
        internal static async Task<Tuple<List<ArticleInformation>, int, int>> 
            GetSearchedArticleList(string searchedHtml, string gall_id, string searchNick, GalleryType gallType, bool isFixed, int searchPos)
        {
            return await Task.Run(() =>
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(searchedHtml);

                int maxPage = 0;

                List<ArticleInformation> searchedList = new List<ArticleInformation>();

                string baseUrl = "http://gall.dcinside.com/";

                HtmlNode pageList = doc.GetElementbyId("dgn_btn_paging");
                if (pageList == null)
                    throw new Exception("알 수 없는 오류입니다.");

                HtmlNode lastChild = pageList.Descendants("a").Last();

                if (lastChild.InnerText != "다음검색")
                    searchPos = -1;
                else
                {
                    string src = lastChild.GetAttributeValue("href", "");
                    if (string.IsNullOrWhiteSpace(src))
                    {
                        throw new Exception("알 수 없는 오류입니다.");
                    }

                    src = baseUrl + src.Substring(1);

                    Uri nextSearchUri = new Uri(src);
                    int.TryParse(HttpUtility.ParseQueryString(nextSearchUri.Query).Get("search_pos"), out searchPos);
                }

                HtmlNode lastPage = pageList.Descendants("span").Where(n => n.GetAttributeValue("class", "") == "arrow_2").FirstOrDefault();
                if (lastPage == null)
                {
                    if (lastChild.InnerText != "다음검색")
                    {
                        int.TryParse(lastChild.InnerText, out maxPage);
                    }
                    else
                    {
                        lastPage = lastChild.PreviousSibling;
                        int.TryParse(lastPage.InnerText, out maxPage);
                    }
                }
                else
                {
                    string src = lastPage.ParentNode.GetAttributeValue("href", "");
                    if (string.IsNullOrWhiteSpace(src))
                    {
                        throw new Exception("알 수 없는 오류입니다.");
                    }

                    src = baseUrl + src.Substring(1);

                    Uri maxPageUri = new Uri(src);
                    int.TryParse(HttpUtility.ParseQueryString(maxPageUri.Query).Get("page"), out maxPage);
                }

                string deleteBasePath = null;

                if (gallType == GalleryType.Normal)
                    deleteBasePath = "http://gall.dcinside.com/mgallery/board/delete/?id=" + gall_id;
                else if (gallType == GalleryType.Minor)
                    deleteBasePath = "http://gall.dcinside.com/board/delete/?id=" + gall_id;

                if (deleteBasePath == null)
                    throw new Exception("예상치 못한 갤러리 형식입니다.");

                foreach (HtmlNode article in doc.DocumentNode.Descendants("tr").Where(n => n.GetAttributeValue("class", "") == "tb"))
                {
                    HtmlNode noticeNode = article.Descendants("td").Where(n => n.GetAttributeValue("class", "") == "t_notice").First();
                    if (noticeNode.InnerText == "공지")
                        continue;

                    HtmlNode userNode = article.Descendants("td").Where(n => n.GetAttributeValue("class", "").Contains("t_writer")).First();
                    string user_id = userNode.GetAttributeValue("user_id", "");
                    if (user_id == "" && isFixed)
                        continue;
                    else if (user_id != "" && !isFixed)
                        continue;

                    string nick = userNode.Descendants("span").First().InnerText;
                    if (nick != searchNick)
                        continue;

                    HtmlNode subjectNode = article.Descendants("td").Where(n => n.GetAttributeValue("class", "").Contains("t_subject")).First();

                    string title = subjectNode.InnerText;

                    string articleUrl = subjectNode.Descendants("a").First().GetAttributeValue("href", "");
                    if (string.IsNullOrWhiteSpace(articleUrl))
                    {
                        throw new Exception("알 수 없는 오류입니다.");
                    }

                    articleUrl = baseUrl + articleUrl.Substring(1);

                    Uri subjectUri = new Uri(articleUrl);
                    string articleNo = HttpUtility.ParseQueryString(subjectUri.Query).Get("no");

                    string date, url;
                    date = article.Descendants("td").Where(n => n.GetAttributeValue("class", "").Contains("t_date")).First().InnerText;
                    url = deleteBasePath + "&no=" + articleNo;
                    ArticleInformation info = new ArticleInformation(HttpUtility.HtmlDecode(title), date, url);
                    info.GalleryDeleteParameter = new GalleryArticleDeleteParameter()
                    {
                        GalleryId = gall_id,
                        ArticleID = articleNo
                    };

                    searchedList.Add(info);
                }

                return new Tuple<List<ArticleInformation>, int, int>(searchedList, searchPos, maxPage);
            });
        }

        internal static async Task<ParameterStorage> GetLoginParameterAsync(string src)
        {
            return await Task.Run(async () =>
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(src);

                string jsScript = "";

                foreach (HtmlNode scriptNode in doc.DocumentNode.Descendants("script"))
                {
                    jsScript += scriptNode.InnerHtml;
                }

                if (jsScript == "")
                {
                    throw new Exception("로그인 파라미터를 불러오는데 실패하였습니다.");
                }

                // 로그인시 필요한 파라미터 정보들을 가져옴
                return await JSParser.GetLoginParameterAsync(jsScript);
            });
        }
    }
}
