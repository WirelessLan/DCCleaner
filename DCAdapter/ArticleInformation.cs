using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAdapter
{
    /// <summary>
    /// 삭제할 글 정보를 표시합니다.
    /// </summary>
    public class ArticleInformation
    {
        /// <summary>
        /// 글 제목
        /// </summary>
        public string Title { get; }
        /// <summary>
        /// 글 작성일
        /// </summary>
        public string Date { get; }
        /// <summary>
        /// 글의 삭제 URL
        /// </summary>
        public string DeleteUrl { get; }
        /// <summary>
        /// 실제 갤러리에서 삭제 결과 여부
        /// </summary>
        public bool IsGalleryDeleted { get; set; }
        /// <summary>
        /// 갤로그에서 삭제 결과 여부
        /// </summary>
        public bool IsGallogDeleted { get; set; }
        /// <summary>
        /// 삭제 메시지. 삭제 에러 발생시 사용.
        /// </summary>
        public string DeleteMessage { get; set; }
        public GallogArticleDeleteParameter GallogDeleteParameter;
        public GalleryArticleDeleteParameter GalleryDeleteParameter;

        public ArticleInformation(string title, string date, string url)
        {
            this.Title = title;
            this.Date = date;
            this.DeleteUrl = url;
            this.IsGalleryDeleted = false;
            this.IsGallogDeleted = false;
        }
    }
}
