using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAdapter
{
    /// <summary>
    /// 삭제할 댓글 정보를 표시합니다.
    /// </summary>
    public class CommentInformation
    {
        /// <summary>
        /// 댓글의 닉네임
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// 댓글 내용
        /// </summary>
        public string Content { get; }
        /// <summary>
        /// 댓글 작성일
        /// </summary>
        public string Date { get; }
        /// <summary>
        /// 댓글의 삭제 URL
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
        public GallogCommentDeleteParameter GallogDeleteParameter;
        public GalleryCommentDeleteParameter GalleryDeleteParameter;

        public CommentInformation(string name, string content, string date, string url)
        {
            this.Name = name;
            this.Content = content;
            this.Date = date;
            this.DeleteUrl = url;
            this.IsGalleryDeleted = false;
            this.IsGallogDeleted = false;
        }
    }
}
