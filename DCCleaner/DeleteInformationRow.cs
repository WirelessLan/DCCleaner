using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DCAdapter;

namespace DCCleaner
{
    class DeleteInformationRow : DataGridViewRow
    {
        public ArticleInformation ArticleInformation { get; set; }
        public CommentInformation CommentInformation { get; set; }

        public DeleteInformationRow(ArticleInformation articleInfo, DataGridView dgv, bool useDate)
        {
            ArticleInformation = articleInfo;
            if (useDate)
                this.CreateCells(dgv, ArticleInformation.Title, ArticleInformation.Date);
            else
                this.CreateCells(dgv, ArticleInformation.Title);
        }

        public DeleteInformationRow(CommentInformation commentInfo, DataGridView dgv)
        {
            CommentInformation = commentInfo;
            this.CreateCells(dgv, CommentInformation.Name, CommentInformation.Content, CommentInformation.Date);
        }
    }
}
