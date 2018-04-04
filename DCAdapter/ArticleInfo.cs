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
    public class ArticleInfo
    {
        /// <summary>
        /// 글 제목
        /// </summary>
        public string Title;
        /// <summary>
        /// 글 작성일
        /// </summary>
        public string Date;
        /// <summary>
        /// 글의 삭제 URL
        /// </summary>
        public string DeleteUrl;
        /// <summary>
        /// 실제 갤러리에서 삭제 결과 여부
        /// </summary>
        public bool IsGalleryDeleted = false;
        /// <summary>
        /// 갤로그에서 삭제 결과 여부
        /// </summary>
        public bool IsGallogDeleted = false;
        /// <summary>
        /// 삭제 메시지. 삭제 에러 발생시 사용.
        /// </summary>
        public string DeleteMessage;
        public GallogArticleDeleteParameter GallogArticleDeleteParameters;
        public GalleryArticleDeleteParameter GalleryArticleDeleteParameters;
    }
}
