using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAdapter
{
    /// <summary>
    /// 검색한 글 정보를 표시합니다.
    /// </summary>
    public class SearchedArticleInfo : ArticleInfo
    {
        /// <summary>
        /// 갤러리 ID
        /// </summary>
        public string Gallery;
        /// <summary>
        /// 글 ID
        /// </summary>
        public string ArticleID;
    }
}
