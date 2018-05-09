﻿using System;
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
        public ArticleInformation ArticleInformation { get; }
        public CommentInformation CommentInformation { get; }

        public DeleteInformationRow(ArticleInformation articleInfo, DataGridView dgv)
        {
            ArticleInformation = articleInfo;
            this.CreateCells(dgv, ArticleInformation.Title);
        }

        public DeleteInformationRow(CommentInformation commentInfo, DataGridView dgv)
        {
            CommentInformation = commentInfo;
            this.CreateCells(dgv, CommentInformation.Name, CommentInformation.Content, CommentInformation.Date);
        }
    }
}
