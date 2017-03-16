using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DCAdapter;

namespace DCCleaner
{
    public partial class Frm_Cleaner : Form
    {
        DCConnector conn;
        List<ArticleInfo> articleList = null;
        List<CommentInfo> commentList = null;

        public Frm_Cleaner(DCConnector _conn)
        {
            InitializeComponent();
            this.conn = _conn;
        }

        private void btn_LoadArticles_Click(object sender, EventArgs e)
        {
            if ((articleList = conn.LoadGallogArticles()) == null)
            {
                MessageBox.Show("내가 쓴 글 목록을 불러올 수 없습니다.", "오류");
                return;
            }

            dgv_ArticleList.Rows.Clear();

            foreach(ArticleInfo info in articleList)
            {
                dgv_ArticleList.Rows.Add(info.Title);
            }
        }

        private void btn_LoadComments_Click(object sender, EventArgs e)
        {
            if ((commentList = conn.LoadGallogComments()) == null)
            {
                MessageBox.Show("내가 쓴 글 목록을 불러올 수 없습니다.", "오류");
                return;
            }

            dgv_CommentList.Rows.Clear();

            foreach (CommentInfo info in commentList)
            {
                dgv_CommentList.Rows.Add(info.Name, info.Content, info.Date);
            }
        }

        private void btn_RemoveGallArticle_Click(object sender, EventArgs e)
        {
            if (articleList == null)
                return;

            foreach(ArticleInfo info in articleList)
            {
                ArticleInfo res = conn.DeleteArticle(info, false);
                if(res.ActualDelete == false)
                    for(int i = 0; i < 5; i++)
                    {
                        // 실패시, 최대 5회 재시도
                        res = conn.DeleteArticle(info, false);
                        if (res.ActualDelete)
                            break;
                    }
                info.ActualDelete = res.ActualDelete;
                info.GallogDelete = res.GallogDelete;
                info.DeleteMessage = res.DeleteMessage;
            }

            MessageBox.Show("삭제 완료 - 쓴글 갤만", "알림");
        }

        private void btn_RemoveAllArticle_Click(object sender, EventArgs e)
        {
            if (articleList == null)
                return;

            foreach (ArticleInfo info in articleList)
            {
                ArticleInfo res = conn.DeleteArticle(info, true);
                if (res.GallogDelete == false)
                    for (int i = 0; i < 5; i++)
                    {
                        // 실패시, 최대 5회 재시도
                        res = conn.DeleteArticle(info, false);
                        if (res.GallogDelete)
                            break;
                    }
                info.ActualDelete = res.ActualDelete;
                info.GallogDelete = res.GallogDelete;
                info.DeleteMessage = res.DeleteMessage;
            }

            MessageBox.Show("삭제 완료 - 쓴글 갤로그도", "알림");
        }

        private void btn_RemoveGallComment_Click(object sender, EventArgs e)
        {
            if (commentList == null)
                return;

            foreach (CommentInfo info in commentList)
            {
                CommentInfo res = conn.DeleteComment(info, false);
                if (res.ActualDelete == false)
                    for (int i = 0; i < 5; i++)
                    {
                        // 실패시, 최대 5회 재시도
                        res = conn.DeleteComment(info, false);
                        if (res.ActualDelete)
                            break;
                    }
                info.ActualDelete = res.ActualDelete;
                info.GallogDelete = res.GallogDelete;
                info.DeleteMessage = res.DeleteMessage;
            }

            MessageBox.Show("삭제 완료 - 쓴 댓글 갤만", "알림");
        }

        private void btn_RemoveAllComent_Click(object sender, EventArgs e)
        {
            if (commentList == null)
                return;

            foreach (CommentInfo info in commentList)
            {
                CommentInfo res = conn.DeleteComment(info, true);
                if (res.GallogDelete == false)
                    for (int i = 0; i < 5; i++)
                    {
                        // 실패시, 최대 5회 재시도
                        res = conn.DeleteComment(info, false);
                        if (res.GallogDelete)
                            break;
                    }
                info.ActualDelete = res.ActualDelete;
                info.GallogDelete = res.GallogDelete;
                info.DeleteMessage = res.DeleteMessage;
            }

            MessageBox.Show("삭제 완료 - 쓴 댓글 갤로그도", "알림");
        }
    }
}
