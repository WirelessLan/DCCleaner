using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using DCAdapter;

namespace DCCleaner
{
    public partial class Frm_Cleaner : Form
    {
        DCConnector conn;
        List<ArticleInfo> articleList = null;
        List<CommentInfo> commentList = null;
        Thread loadingThread = null;
        
        public Frm_Cleaner(DCConnector _conn)
        {
            InitializeComponent();
            this.conn = _conn;
        }

        private void btn_LoadArticles_Click(object sender, EventArgs e)
        {
            if(loadingThread!=null && loadingThread.IsAlive)
            {
                return;
            }

            loadingThread = new Thread(new ThreadStart(delegate()
            {
                try
                {
                    articleList = conn.LoadGallogArticles();
                }
                catch(Exception ex)
                {
                    try
                    {
                        this.Invoke(new Action(() =>
                        {
                            lbl_Status.Text = ex.Message;
                        }));
                    }
                    catch
                    {
                        return;
                    }
                    return;
                }

                this.Invoke(new Action(() =>
                {
                    if (articleList == null)
                    {
                        MessageBox.Show("내가 쓴 글 목록을 불러올 수 없습니다.", "오류");
                        return;
                    }

                    LoadArticleList();

                    lbl_Status.Text = "쓴 글 목록을 불러왔습니다 - 총 " + articleList.Count.ToString() + "개";
                }));
            }));

            lbl_Status.Text = "쓴 글 목록을 불러오는 중입니다...";

            loadingThread.Start();
        }

        private void btn_LoadComments_Click(object sender, EventArgs e)
        {
            if (loadingThread != null && loadingThread.IsAlive)
            {
                return;
            }

            loadingThread = new Thread(new ThreadStart(delegate ()
            {
                try
                {
                    commentList = conn.LoadGallogComments();
                }
                catch (Exception ex)
                {
                    try
                    {
                        this.Invoke(new Action(() =>
                        {
                            lbl_Status.Text = ex.Message;
                        }));
                    }
                    catch
                    {
                        return;
                    }
                    return;
                }

                this.Invoke(new Action(() =>
                {
                    if (commentList == null)
                    {
                        MessageBox.Show("내가 쓴 리플 목록을 불러올 수 없습니다.", "오류");
                        return;
                    }

                    LoadCommentList();

                    lbl_Status.Text = "쓴 리플 목록을 불러왔습니다 - 총 " + commentList.Count.ToString() + "개";
                }));
            }));

            lbl_Status.Text = "쓴 리플 목록을 불러오는 중입니다...";

            loadingThread.Start();
        }

        private void btn_RemoveGallArticle_Click(object sender, EventArgs e)
        {

            RemoveArticles(false);
        }

        private void btn_RemoveAllArticle_Click(object sender, EventArgs e)
        {
            RemoveArticles(true);
        }

        /// <summary>
        /// 글 목록 삭제 함수
        /// </summary>
        /// <param name="both">True : 갤로그도, False : 갤러리만</param>
        private void RemoveArticles(bool both)
        {
            if (articleList == null)
                return;

            if (loadingThread != null && loadingThread.IsAlive)
            {
                return;
            }

            loadingThread = new Thread(new ThreadStart(delegate ()
            {
                foreach (ArticleInfo info in articleList)
                {
                    ArticleInfo res = null;
                    try
                    {
                        res = conn.DeleteArticle(info, both);
                    }
                    catch
                    {
                        continue;
                    }

                    if (res.ActualDelete == false)
                        for (int i = 0; i < 5; i++)
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

                this.Invoke(new Action(() =>
                {
                    if(both)
                        lbl_Status.Text = "쓴 글 - 갤로그도 삭제 완료";
                    else
                        lbl_Status.Text = "쓴 글 - 갤만 삭제 완료";
                }));
            }));

            if(both)
                lbl_Status.Text = "쓴 글 - 갤로그도 삭제중...";
            else
                lbl_Status.Text = "쓴 글 - 갤만 삭제중...";

            loadingThread.Start();
        }

        private void btn_RemoveGallComment_Click(object sender, EventArgs e)
        {
            RemoveComments(false);
        }

        private void btn_RemoveAllComment_Click(object sender, EventArgs e)
        {
            RemoveComments(true);
        }

        private void RemoveComments(bool both)
        {
            if (commentList == null)
                return;

            if (loadingThread != null && loadingThread.IsAlive)
            {
                return;
            }

            loadingThread = new Thread(new ThreadStart(delegate ()
            {
                foreach (CommentInfo info in commentList)
                {
                    CommentInfo res = null;
                    try
                    {
                        res = conn.DeleteComment(info, both);
                    }
                    catch
                    {
                        continue;
                    }
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

                this.Invoke(new Action(() =>
                {
                    if(both)
                        lbl_Status.Text = "쓴 리플 - 갤로그도 삭제 완료";
                    else
                        lbl_Status.Text = "쓴 리플 - 갤만 삭제 완료";
                }));
            }));

            if(both)
                lbl_Status.Text = "쓴 리플 - 갤로그도 삭제중...";
            else
                lbl_Status.Text = "쓴 리플 - 갤만 삭제중...";

            loadingThread.Start();
        }

        private void Frm_Cleaner_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(this.loadingThread != null && this.loadingThread.IsAlive)
            {
                this.loadingThread.Abort();
            }
            else
            {
                this.loadingThread = null;
            }
        }

        private void dgv_ArticleList_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            ContextMenu m = new ContextMenu();
            m.MenuItems.Add("삭제(&R)").Click += menu_DeleteArticle_Clicked;

            int currentMouseOverRow = dgv_ArticleList.HitTest(e.X, e.Y).RowIndex;

            if (currentMouseOverRow >= 0)
            {
                dgv_ArticleList.ClearSelection();

                dgv_ArticleList.Rows[currentMouseOverRow].Selected = true;

                m.Show(dgv_ArticleList, new Point(e.X, e.Y));
            }
        }

        private void menu_DeleteArticle_Clicked(object sender, EventArgs e)
        {
            if (dgv_ArticleList.SelectedRows == null)
                return;

            int selectedIdx = dgv_ArticleList.SelectedRows[0].Index;
            ArticleInfo target = articleList[selectedIdx];

            if (loadingThread != null && loadingThread.IsAlive)
            {
                return;
            }

            loadingThread = new Thread(new ThreadStart(delegate ()
            {
                conn.DeleteArticle(target, true);

                if (target.GallogDelete)
                {
                    articleList.RemoveAt(selectedIdx);

                    this.Invoke(new Action(() =>
                    {
                        dgv_ArticleList.Rows.RemoveAt(selectedIdx);
                        gb_ArticleGroup.Text = "내가 쓴 글 [" + articleList.Count.ToString() + "]";
                        lbl_Status.Text = "글을 삭제하였습니다.";
                    }));
                }
            }));

            lbl_Status.Text = "글을 삭제하는 중입니다...";

            loadingThread.Start();
        }

        private void dgv_CommentList_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button != MouseButtons.Right)
            {
                return;
            }
            
            ContextMenu m = new ContextMenu();
            m.MenuItems.Add("삭제(&R)").Click += menu_DeleteComment_Clicked;

            int currentMouseOverRow = dgv_CommentList.HitTest(e.X, e.Y).RowIndex;

            if (currentMouseOverRow >= 0)
            {
                dgv_CommentList.ClearSelection();

                dgv_CommentList.Rows[currentMouseOverRow].Selected = true;

                m.Show(dgv_CommentList, new Point(e.X, e.Y));
            }
        }

        private void menu_DeleteComment_Clicked(object sender, EventArgs e)
        {
            if (dgv_CommentList.SelectedRows == null)
                return;

            int selectedIdx = dgv_CommentList.SelectedRows[0].Index;
            CommentInfo target = commentList[selectedIdx];

            if (loadingThread != null && loadingThread.IsAlive)
            {
                return;
            }

            loadingThread = new Thread(new ThreadStart(delegate ()
            {
                conn.DeleteComment(target, true);

                if (target.GallogDelete)
                {
                    commentList.RemoveAt(selectedIdx);

                    this.Invoke(new Action(() =>
                    {
                        dgv_CommentList.Rows.RemoveAt(selectedIdx);
                        gb_CommentGroup.Text = "내가 쓴 리플 [" + commentList.Count.ToString() + "]";
                        lbl_Status.Text = "리플을 삭제하였습니다.";
                    }));
                }
            }));

            lbl_Status.Text = "리플을 삭제하는 중입니다...";

            loadingThread.Start();
        }

        private void LoadArticleList()
        {
            dgv_ArticleList.Rows.Clear();

            foreach (ArticleInfo info in articleList)
            {
                dgv_ArticleList.Rows.Add(info.Title);
            }

            string loadedCnt = articleList.Count.ToString();
            gb_ArticleGroup.Text = "내가 쓴 글 [" + loadedCnt + "]";
        }

        private void LoadCommentList()
        {
            dgv_CommentList.Rows.Clear();

            foreach (CommentInfo info in commentList)
            {
                dgv_CommentList.Rows.Add(info.Name, info.Content, info.Date);
            }

            string loadedCnt = commentList.Count.ToString();
            gb_CommentGroup.Text = "내가 쓴 리플 [" + loadedCnt + "]";
        }
    }
}
