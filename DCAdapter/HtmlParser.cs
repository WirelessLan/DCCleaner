using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Web;

namespace DCAdapter
{
    class HtmlParser
    {
        internal static int GetArticleCounts(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            
            HtmlNode cntNode = doc.DocumentNode.SelectSingleNode("//div[@id='statusDiv']/table[1]/tr[1]/td[1]/table[1]/tr[1]/td[1]/table[1]/tr[1]/td[1]/table[1]/tr[1]/td[1]/font[1]");

            return ExtractNumber(cntNode.InnerText);
        }

        internal static int GetCommentCounts(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            HtmlNode cntNode = doc.DocumentNode.SelectSingleNode("//div[@id='statusDiv']/table[1]/tr[1]/td[1]/table[1]/tr[1]/td[1]/table[1]/tr[1]/td[1]/table[1]/tr[3]/td[1]/font[1]");
            
            return ExtractNumber(cntNode.InnerText);
        }

        private static int ExtractNumber(string text)
        {
            string result = Regex.Replace(text, @"[^\d]", "");

            return int.Parse(result);
        }

        internal static void GetDeleteArticleParameters(string html, out string dcc_key)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            
            dcc_key = "";

            HtmlNode deleteNode = doc.GetElementbyId("delete");

            if(deleteNode == null)
            {
                if(html.Contains("/error/deleted"))
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
            
            HtmlNode dcckeyNode = deleteNode.ParentNode.SelectSingleNode(".//input[@id='dcc_key']");

            dcc_key = dcckeyNode.Attributes["value"].Value;
        }

        internal static void GetDeleteFlowArticleParameters(string html, out string dcc_key, out string cur_t, out string randomName, out string randomKey)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            dcc_key = "";
            cur_t = "";
            randomName = "";
            randomKey = "";

            HtmlNode deleteNode = doc.GetElementbyId("password_confirm");

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

            HtmlNode dcckeyNode = deleteNode.ParentNode.SelectSingleNode(".//input[@id='dcc_key']");
            dcc_key = dcckeyNode.Attributes["value"].Value;

            HtmlNode curtNode = deleteNode.ParentNode.SelectSingleNode(".//input[@id='cur_t']");
            cur_t = curtNode.Attributes["value"].Value;

            int inputCnt = deleteNode.ParentNode.Descendants("input").Count();
            HtmlNode randomKeyNode = curtNode.NextSibling;
            randomName = randomKeyNode.Attributes["name"].Value;
            randomKey = randomKeyNode.Attributes["value"].Value;
        }

        internal static void GetDeleteCommentParameters(string pageHtml, out string check7)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(pageHtml);
            check7 = null;
            
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
        }

        internal static void GetGallogArticleInfo(string pageHtml, out string gall_id, out string gall_no, out string article_id, out string logNo)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(pageHtml);

            gall_id = null;
            gall_no = null;
            article_id = null;
            logNo = null;

            HtmlNode parentNode = doc.GetElementbyId("dTp").ParentNode;
            HtmlNode gallNode = parentNode.SelectSingleNode(".//input[@name='id']");
            HtmlNode cidNode = parentNode.SelectSingleNode(".//input[@name='cid']");
            HtmlNode artNode = parentNode.SelectSingleNode(".//input[@name='pno']");
            HtmlNode logNode = parentNode.SelectSingleNode(".//input[@name='logNo']");

            gall_id = gallNode.Attributes["value"].Value;
            gall_no = cidNode.Attributes["value"].Value;
            article_id = artNode.Attributes["value"].Value;
            logNo = logNode.Attributes["value"].Value;
        }

        internal static List<CommentInfo> GetCommentList(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            List<CommentInfo> coms = new List<CommentInfo>();

            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//table[@bgcolor='#F2F2F5']/tr"))
            {
                if (node.Descendants("td").Count() == 6)
                {
                    string name = node.SelectSingleNode("./td[1]").InnerText;
                    name = HttpUtility.HtmlDecode(name).Trim();
                    string content = HttpUtility.HtmlDecode(node.SelectSingleNode("./td[3]").InnerText);
                    string date = node.SelectSingleNode("./td[5]").InnerText;

                    string url = Utility.GetAbsoulteURL(node.SelectSingleNode("./td[6]/span").Attributes["onClick"].Value);

                    coms.Add(new CommentInfo() { Name = name, Content = content, Date = date, DeleteURL = url });
                }
            }

            return coms;
        }

        internal static List<ArticleInfo> GetArticleList(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            List<ArticleInfo> arts = new List<ArticleInfo>();

            foreach(HtmlNode node in doc.DocumentNode.SelectNodes("//img[@src='http://wstatic.dcinside.com/gallery/skin/gallog/icon_01.gif']"))
            {
                if (node.Attributes["onClick"] != null)
                {
                    string title = node.ParentNode.PreviousSibling.PreviousSibling.PreviousSibling.PreviousSibling.InnerText;
                    title = HttpUtility.HtmlDecode(title).Trim();
                    string url = Utility.GetAbsoulteURL(node.Attributes["onClick"].Value);
                    string date = node.ParentNode.InnerText;

                    arts.Add(new ArticleInfo() { Title = title, DeleteURL = url, Date = date });
                }
            }

            return arts;
        }

        internal static void GetDeleteGallogArticleParameters(string pageHtml, out string gall_id, out string dcc_key, out string randomKey, out string randomValue)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(pageHtml);

            gall_id = "";
            dcc_key = "";
            randomKey = "";
            randomValue = "";

            HtmlNode gallNode = doc.DocumentNode.SelectSingleNode("//input[@name='id']");
            HtmlNode dcc_keyNode = doc.DocumentNode.SelectSingleNode("//input[@name='dcc_key']");
            int inputCnt = doc.DocumentNode.Descendants("input").Count();
            HtmlNode randomKeyNode = doc.DocumentNode.SelectSingleNode("//input[" + (inputCnt - 1) + "]");

            gall_id = gallNode.Attributes["value"].Value;
            dcc_key = dcc_keyNode.Attributes["value"].Value;
            randomKey = randomKeyNode.Attributes["name"].Value;
            randomValue = randomKeyNode.Attributes["value"].Value;
        }

        internal static void GetGallogCommentInfo(string pageHtml, out string gall_id, out string gall_no, out string article_id, out string comment_id, out string logNo)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(pageHtml);

            gall_id = null;
            gall_no = null;
            article_id = null;
            comment_id = null;
            logNo = null;

            HtmlNode parentNode = doc.GetElementbyId("dTp").ParentNode;
            HtmlNode idNode = parentNode.SelectSingleNode(".//input[@name='id']");
            HtmlNode cidNode = parentNode.SelectSingleNode(".//input[@name='cid']");
            HtmlNode artNode = parentNode.SelectSingleNode(".//input[@name='no']");
            HtmlNode cNode = parentNode.SelectSingleNode(".//input[@name='c_no']");
            HtmlNode logNode = parentNode.SelectSingleNode(".//input[@name='logNo']");

            gall_id = idNode.Attributes["value"].Value;
            gall_no = cidNode.Attributes["value"].Value;
            article_id = artNode.Attributes["value"].Value;
            comment_id = cNode.Attributes["value"].Value;
            logNo = logNode.Attributes["value"].Value;
        }

        internal static List<SearchedArticleInfo> GetSearchedArticleList(string searchedHtml, string gall_id, string searchNick, GalleryType gallType, bool isFixed, ref int searchPos, out int maxPage)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(searchedHtml);

            maxPage = 0;

            List<SearchedArticleInfo> searchedList = new List<SearchedArticleInfo>();

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
                if(string.IsNullOrWhiteSpace(src))
                {
                    throw new Exception("알 수 없는 오류입니다.");
                }

                src = baseUrl + src.Substring(1);

                Uri nextSearchUri = new Uri(src);
                int.TryParse(HttpUtility.ParseQueryString(nextSearchUri.Query).Get("search_pos"), out searchPos);
            }

            HtmlNode lastPage = pageList.Descendants("span").Where(n => n.GetAttributeValue("class", "") == "arrow_2").FirstOrDefault();
            if(lastPage == null)
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

                SearchedArticleInfo info = new SearchedArticleInfo();
                info.Date = article.Descendants("td").Where(n => n.GetAttributeValue("class", "").Contains("t_date")).First().InnerText;
                info.Title = title;
                info.Gallery = gall_id;
                info.ArticleID = articleNo;
                info.DeleteURL = deleteBasePath + "&no=" + articleNo;

                searchedList.Add(info);
            }

            return searchedList;
        }

        internal static void GetDeleteGallogCommentParameters(string pageHtml, out string gall_id, out string randomKey, out string randomVal)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(pageHtml);

            gall_id = "";
            randomKey = "";
            randomVal = "";

            HtmlNode gallNode = doc.DocumentNode.SelectSingleNode("//input[@name='id']");
            int inputCnt = doc.DocumentNode.Descendants("input").Count();
            HtmlNode randomKeyNode = doc.DocumentNode.SelectSingleNode("//input[" + (inputCnt - 1) + "]");

            gall_id = gallNode.Attributes["value"].Value;
            randomKey = randomKeyNode.Attributes["name"].Value;
            randomVal = randomKeyNode.Attributes["value"].Value;
        }
    }
}
