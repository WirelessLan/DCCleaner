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
                int rmIdx = 0;  // 삭제 인덱스. 0부터 위로
                int delCnt = articleList.Count;

                for (int i = 0; i < delCnt; i++)
                {
                    ArticleInfo info = articleList[rmIdx];
                    ArticleInfo res = null;
                    try
                    {
                        res = conn.DeleteArticle(info, both);
                    }
                    catch
                    {
                        // 삭제 못한 글은 무시
                        rmIdx++;
                        continue;
                    }

                    if (!res.ActualDelete || (both && !res.GallogDelete))
                        for (int j = 0; j < 1; j++)
                        {
                            // 실패시, Sleep 후 1회 재시도
                            Thread.Sleep(100);
                            res = conn.DeleteArticle(info, both);
                            if (res.ActualDelete && (!both || res.GallogDelete))
                                break;
                        }

                    // 재시도에도 삭제 실패했을 경우,
                    if (!res.ActualDelete || (both && !res.GallogDelete))
                    {
                        rmIdx++;
                        continue;   // 무시
                    }

                    info.ActualDelete = res.ActualDelete;
                    info.GallogDelete = res.GallogDelete;
                    info.DeleteMessage = res.DeleteMessage;

                    articleList[rmIdx] = info;

                    // 갤로그도 삭제일 경우에만 화면 지움
                    if (both)
                    {
                        articleList.RemoveAt(rmIdx);
                        this.Invoke(new Action(() =>
                        {
                            dgv_ArticleList.Rows.RemoveAt(rmIdx);
                            gb_ArticleGroup.Text = "내가 쓴 글 [" + articleList.Count.ToString() + "]";
                        }));
                    }
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

        /// <summary>
        /// 댓글 목록 삭제 함수
        /// </summary>
        /// <param name="both">True : 갤로그도 False : 갤러리만</param>
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
                int rmIdx = 0;  // 삭제 인덱스. 0부터 위로
                int delCnt = commentList.Count;

                for (int i = 0; i < delCnt; i++)
                {
                    CommentInfo info = commentList[rmIdx];
                    CommentInfo res = null;
                    try
                    {
                        res = conn.DeleteComment(info, both);
                    }
                    catch
                    {
                        // 삭제 못한 리플은 무시
                        rmIdx++;
                        continue;
                    }
                    if (!res.ActualDelete || (both && !res.GallogDelete))
                        for (int j = 0; j < 1; j++)
                        {
                            // 실패시, Sleep 후 1회 재시도
                            Thread.Sleep(100);
                            res = conn.DeleteComment(info, both);
                            if (res.ActualDelete && (!both || res.GallogDelete))
                                break;
                        }

                    // 재시도에도 삭제 실패했을 경우,
                    if (!res.ActualDelete || (both && !res.GallogDelete))
                    {
                        rmIdx++;
                        continue;   // 무시
                    }

                    info.ActualDelete = res.ActualDelete;
                    info.GallogDelete = res.GallogDelete;
                    info.DeleteMessage = res.DeleteMessage;

                    commentList[rmIdx] = info;

                    // 갤로그도 삭제일 경우에만 화면 지움
                    if (both)
                    {
                        commentList.RemoveAt(rmIdx);
                        this.Invoke(new Action(() =>
                        {
                            dgv_CommentList.Rows.RemoveAt(rmIdx);
                            gb_CommentGroup.Text = "내가 쓴 리플 [" + commentList.Count.ToString() + "]";
                        }));
                    }
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

                // 갤로그와 갤러리 둘다 삭제 되었을 경우
                if (target.ActualDelete && target.GallogDelete)
                {
                    articleList.RemoveAt(selectedIdx);

                    this.Invoke(new Action(() =>
                    {
                        dgv_ArticleList.Rows.RemoveAt(selectedIdx);
                        gb_ArticleGroup.Text = "내가 쓴 글 [" + articleList.Count.ToString() + "]";
                        lbl_Status.Text = "글을 삭제하였습니다.";
                    }));
                }
                else
                {
                    string rmErrMsg = "";
                    if (!target.ActualDelete)
                        rmErrMsg = "글을 삭제하는데 실패하였습니다. - 갤러리 삭제 실패";
                    else
                        rmErrMsg = "글을 삭제하는데 실패하였습니다. - 갤로그 삭제 실패";

                    this.Invoke(new Action(() =>
                    {
                        lbl_Status.Text = rmErrMsg;
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

                // 갤로그와 갤러리 둘다 삭제 되었을 경우
                if (target.ActualDelete && target.GallogDelete)
                {
                    commentList.RemoveAt(selectedIdx);

                    this.Invoke(new Action(() =>
                    {
                        dgv_CommentList.Rows.RemoveAt(selectedIdx);
                        gb_CommentGroup.Text = "내가 쓴 리플 [" + commentList.Count.ToString() + "]";
                        lbl_Status.Text = "리플을 삭제하였습니다.";
                    }));
                }
                else
                {
                    string rmErrMsg = "";
                    if (!target.ActualDelete)
                        rmErrMsg = "리플을 삭제하는데 실패하였습니다. - 갤러리 삭제 실패";
                    else
                        rmErrMsg = "리플을 삭제하는데 실패하였습니다. - 갤로그 삭제 실패";

                    this.Invoke(new Action(() =>
                    {
                        lbl_Status.Text = rmErrMsg;
                    }));
                }
            }));

            lbl_Status.Text = "리플을 삭제하는 중입니다...";

            loadingThread.Start();
        }

        private void dgv_ArticleList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            int currentMouseOverRow = dgv_ArticleList.HitTest(e.X, e.Y).RowIndex;

            if (currentMouseOverRow >= 0)
            {
                dgv_ArticleList.ClearSelection();

                dgv_ArticleList.Rows[currentMouseOverRow].Selected = true;
            }

            if (dgv_ArticleList.SelectedRows == null)
                return;

            int selectedIdx = dgv_ArticleList.SelectedRows[0].Index;
            ArticleInfo target = articleList[selectedIdx];

            string msg = "갤러리 삭제 : " + (target.ActualDelete ? "삭제됨" : "삭제안됨") + Environment.NewLine
                       + "갤로그 삭제 : " + (target.GallogDelete ? "삭제됨" : "삭제안됨") + Environment.NewLine
                       + "메시지 : " + (target.DeleteMessage);

            MessageBox.Show(msg, "글 삭제 정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void dgv_CommentList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if(e.Button != MouseButtons.Left)
            {
                return;
            }

            int currentMouseOverRow = dgv_CommentList.HitTest(e.X, e.Y).RowIndex;

            if (currentMouseOverRow >= 0)
            {
                dgv_CommentList.ClearSelection();

                dgv_CommentList.Rows[currentMouseOverRow].Selected = true;
            }

            if (dgv_CommentList.SelectedRows == null)
                return;

            int selectedIdx = dgv_CommentList.SelectedRows[0].Index;
            CommentInfo target = commentList[selectedIdx];

            string msg = "갤러리 삭제 : " + (target.ActualDelete ? "삭제됨" : "삭제안됨") + Environment.NewLine
                       + "갤로그 삭제 : " + (target.GallogDelete ? "삭제됨" : "삭제안됨") + Environment.NewLine
                       + "메시지 : " + (target.DeleteMessage);

            MessageBox.Show(msg, "리플 삭제 정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
