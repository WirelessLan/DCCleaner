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
        List<SearchedArticleInfo> searchedList = null;
        bool isSearching = false;
        Thread loadingThread = null;
        object lockObject = new object();

        public Frm_Cleaner(DCConnector _conn)
        {
            InitializeComponent();
            this.conn = _conn;
        }

        private void Frm_Cleaner_Load(object sender, EventArgs e)
        {
            if(!conn.IsLogin)
            {
                tc_CleanerTabContainer.SelectedTab = tp_SearchBase;
                lbl_SearchBaseWarn.Text = "※ 주의 : 유동닉은 갤로그 기반 삭제 및 리플 삭제가 불가능합니다.";
            }
            else
            {
                tc_CleanerTabContainer.SelectedTab = tp_GallogBase;
                lbl_SearchBaseWarn.Text = "※ 주의 : 검색 기반 삭제는 리플 삭제가 불가능하며, 로그인 시 고정닉 검색 및 삭제만 가능합니다.";
            }
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
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    SetStatusMessage(ex.Message);
                }

                this.Invoke(new Action(() =>
                {
                    if (articleList == null)
                    {
                        SetStatusMessage("내가 쓴 글 목록을 불러올 수 없습니다.");
                        return;
                    }

                    LoadArticleList();

                    SetStatusMessage("쓴 글 목록을 불러왔습니다 - 총 " + articleList.Count.ToString() + "개");
                }));
            }));

            SetStatusMessage("쓴 글 목록을 불러오는 중입니다...");

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
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    SetStatusMessage(ex.Message);
                    return;
                }

                this.Invoke(new Action(() =>
                {
                    if (commentList == null)
                    {
                        SetStatusMessage("내가 쓴 리플 목록을 불러올 수 없습니다.");
                        return;
                    }

                    LoadCommentList();

                    SetStatusMessage("쓴 리플 목록을 불러왔습니다 - 총 " + commentList.Count.ToString() + "개");
                }));
            }));

            SetStatusMessage("쓴 리플 목록을 불러오는 중입니다...");

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
            if (articleList == null || articleList.Count == 0)
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
                    catch (ThreadAbortException)
                    {
                        return;
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
                
                if (both)
                    SetStatusMessage("쓴 글 - 갤로그도 삭제 완료");
                else
                    SetStatusMessage("쓴 글 - 갤러리만 삭제 완료");
            }));

            if (both)
                SetStatusMessage("쓴 글 - 갤로그도 삭제중...");
            else
                SetStatusMessage("쓴 글 - 갤러리만 삭제중...");

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
            if (commentList == null || commentList.Count == 0)
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
                    catch (ThreadAbortException)
                    {
                        return;
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

                if (both)
                    SetStatusMessage("쓴 리플 - 갤로그도 삭제 완료");
                else
                    SetStatusMessage("쓴 리플 - 갤러리만 삭제 완료");
            }));

            if (both)
                SetStatusMessage("쓴 리플 - 갤로그도 삭제중...");
            else
                SetStatusMessage("쓴 리플 - 갤러리만 삭제중...");

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
                try
                {
                    conn.DeleteArticle(target, true);
                }
                catch
                {
                    return;
                }

                // 갤로그와 갤러리 둘다 삭제 되었을 경우
                if (target.ActualDelete && target.GallogDelete)
                {
                    articleList.RemoveAt(selectedIdx);

                    this.Invoke(new Action(() =>
                    {
                        dgv_ArticleList.Rows.RemoveAt(selectedIdx);
                        gb_ArticleGroup.Text = "내가 쓴 글 [" + articleList.Count.ToString() + "]";
                        SetStatusMessage("글을 삭제하였습니다.");
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
                        SetStatusMessage(rmErrMsg);
                    }));
                }
            }));

            SetStatusMessage("글을 삭제하는 중입니다...");

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
                try
                {
                    conn.DeleteComment(target, true);
                }
                catch
                {
                    return;
                }

                // 갤로그와 갤러리 둘다 삭제 되었을 경우
                if (target.ActualDelete && target.GallogDelete)
                {
                    commentList.RemoveAt(selectedIdx);

                    this.Invoke(new Action(() =>
                    {
                        dgv_CommentList.Rows.RemoveAt(selectedIdx);
                        gb_CommentGroup.Text = "내가 쓴 리플 [" + commentList.Count.ToString() + "]";
                        SetStatusMessage("리플을 삭제하였습니다.");
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
                        SetStatusMessage(rmErrMsg);
                    }));
                }
            }));

            SetStatusMessage("리플을 삭제하는 중입니다...");

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

        private void tc_CleanerTabContainer_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (loadingThread != null && loadingThread.IsAlive)
            {
                e.Cancel = true;

                return;
            }

            if (e.TabPage == tp_GallogBase && !conn.IsLogin)
            {
                SetStatusMessage("유동닉은 갤로그 기반 삭제를 할 수 없습니다.");
                e.Cancel = true;
            }
        }

        private void btn_SearchArticle_Click(object sender, EventArgs e)
        {
            if (loadingThread != null && loadingThread.IsAlive)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(tb_SearchGalleryID.Text))
            {
                tb_SearchGalleryID.Focus();
                SetStatusMessage("검색할 갤러리 ID를 입력해주세요.");
                return;
            }

            if (string.IsNullOrWhiteSpace(tb_SearchNickName.Text))
            {
                tb_SearchNickName.Focus();
                SetStatusMessage("검색할 닉네임을 입력해주세요.");
                return;
            }

            // 기존 검색목록 삭제
            dgv_SearchArticle.Rows.Clear();
            if (searchedList != null)
                searchedList.Clear();
            else
                searchedList = new List<SearchedArticleInfo>();

            string gall_id, nickname;
            gall_id = tb_SearchGalleryID.Text.Trim();
            nickname = tb_SearchNickName.Text.Trim();
            GalleryType gallType = GalleryType.Normal;
            if (rb_NormalGallery.Checked)
                gallType = GalleryType.Normal;
            else if (rb_MinorGallery.Checked)
                gallType = GalleryType.Minor;

            lock (lockObject)
            {
                isSearching = true;
            }

            loadingThread = new Thread(new ThreadStart(delegate ()
            {
                int delay = 50;
                int pos = 0;
                int page = 1;
                bool cont = false;
                List<SearchedArticleInfo> newSearchedList;

                while (pos != -1)
                {
                    try
                    {
                        newSearchedList = conn.SearchArticles(gall_id, gallType, nickname, ref pos, ref page, out cont);
                    }
                    catch (ThreadAbortException)
                    {
                        lock (lockObject)
                        {
                            isSearching = false;
                            SetStatusMessage("검색된 글 목록을 불러왔습니다 - 총 " + dgv_SearchArticle.Rows.Count.ToString() + "개");
                        }

                        return;
                    }
                    catch (Exception ex)
                    {
                        lock (lockObject)
                        {
                            isSearching = false;
                            SetStatusMessage(ex.Message);
                        }

                        return;
                    }

                    searchedList.AddRange(newSearchedList);

                    this.Invoke(new Action(() =>
                    {
                        LoadSearchedList(newSearchedList);
                    }));

                    Thread.Sleep(delay);
                }

                this.Invoke(new Action(() =>
                {
                    lock (lockObject)
                    {
                        isSearching = false;
                        SetStatusMessage("검색된 글 목록을 불러왔습니다 - 총 " + dgv_SearchArticle.Rows.Count.ToString() + "개");
                    }
                }));
            }));

            SetStatusMessage("글 목록을 검색하는 중입니다...");

            loadingThread.Start();
        }

        private void btn_AbortSearch_Click(object sender, EventArgs e)
        {
            if (loadingThread == null || !loadingThread.IsAlive)
            {
                return;
            }

            lock (lockObject)
            {
                if (isSearching == false)
                    return;
            }

            SetStatusMessage("검색을 중단하는 중입니다...");

            loadingThread.Abort();
        }

        private void LoadSearchedList(List<SearchedArticleInfo> searchedList)
        {
            foreach (SearchedArticleInfo info in searchedList)
            {
                dgv_SearchArticle.Rows.Add(info.Title, info.Date);
            }

            string loadedCnt = dgv_SearchArticle.Rows.Count.ToString();
            gb_SearchedArticleList.Text = "검색된 글 [" + loadedCnt + "]";
        }

        private void btn_DeleteSearchedArticle_Click(object sender, EventArgs e)
        {
            if (searchedList == null || searchedList.Count == 0)
                return;

            if (loadingThread != null && loadingThread.IsAlive)
            {
                return;
            }

            if(!conn.IsLogin)
            {
                if(string.IsNullOrWhiteSpace(tb_DeletePassword.Text))
                {
                    tb_DeletePassword.Focus();
                    SetStatusMessage("삭제할 비밀번호를 입력해주세요.");
                    return;
                }
            }

            string password = tb_DeletePassword.Text.Trim();
            GalleryType gallType = GalleryType.Normal;
            if (rb_NormalGallery.Checked)
                gallType = GalleryType.Normal;
            else if (rb_MinorGallery.Checked)
                gallType = GalleryType.Minor;

            loadingThread = new Thread(new ThreadStart(delegate ()
            {
                int rmIdx = 0;  // 삭제 인덱스. 0부터 위로
                int delCnt = searchedList.Count;

                for (int i = 0; i < delCnt; i++)
                {
                    SearchedArticleInfo info = searchedList[rmIdx];
                    ArticleInfo res = null;
                    try
                    {
                        if (!conn.IsLogin)
                            res = conn.DeleteArticle(info, info.Gallery, null, info.ArticleID, password, gallType, false);
                        else
                            res = conn.DeleteArticle(info, info.Gallery, null, info.ArticleID, null, gallType, false);
                    }
                    catch (ThreadAbortException)
                    {
                        return;
                    }
                    catch
                    {
                        // 삭제 못한 글은 무시
                        rmIdx++;
                        continue;
                    }

                    if (!res.ActualDelete)
                        for (int j = 0; j < 1; j++)
                        {
                            // 실패시, Sleep 후 1회 재시도
                            Thread.Sleep(100);
                            if (!conn.IsLogin)
                                res = conn.DeleteArticle(info, info.Gallery, null, info.ArticleID, password, gallType, false);
                            else
                                res = conn.DeleteArticle(info, info.Gallery, null, info.ArticleID, null, gallType, false);
                            if (res.ActualDelete)
                                break;
                        }

                    // 재시도에도 삭제 실패했을 경우,
                    if (!res.ActualDelete)
                    {
                        rmIdx++;
                        continue;   // 무시
                    }

                    info.ActualDelete = res.ActualDelete;
                    info.GallogDelete = res.GallogDelete;
                    info.DeleteMessage = res.DeleteMessage;

                    searchedList[rmIdx] = info;

                    searchedList.RemoveAt(rmIdx);
                    this.Invoke(new Action(() =>
                    {
                        dgv_SearchArticle.Rows.RemoveAt(rmIdx);
                        gb_SearchedArticleList.Text = "검색된 글 [" + searchedList.Count.ToString() + "]";
                    }));

                    Thread.Sleep(50);
                }
                
                SetStatusMessage("검색된 글 삭제 완료");
            }));
            
            SetStatusMessage("검색된 글 삭제중...");

            loadingThread.Start();
        }

        private void dgv_SearchArticle_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            int currentMouseOverRow = dgv_SearchArticle.HitTest(e.X, e.Y).RowIndex;

            if (currentMouseOverRow >= 0)
            {
                dgv_SearchArticle.ClearSelection();

                dgv_SearchArticle.Rows[currentMouseOverRow].Selected = true;
            }

            if (dgv_SearchArticle.SelectedRows == null)
                return;

            int selectedIdx = dgv_SearchArticle.SelectedRows[0].Index;
            SearchedArticleInfo target = searchedList[selectedIdx];

            string msg = "상태 : " + (target.ActualDelete ? "삭제됨" : "삭제안됨") + Environment.NewLine
                       + "메시지 : " + (target.DeleteMessage);

            MessageBox.Show(msg, "글 삭제 정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void dgv_SearchArticle_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            ContextMenu m = new ContextMenu();
            m.MenuItems.Add("삭제(&R)").Click += menu_DeleteSearchedArticle_Clicked;

            int currentMouseOverRow = dgv_SearchArticle.HitTest(e.X, e.Y).RowIndex;

            if (currentMouseOverRow >= 0)
            {
                dgv_SearchArticle.ClearSelection();

                dgv_SearchArticle.Rows[currentMouseOverRow].Selected = true;

                m.Show(dgv_SearchArticle, new Point(e.X, e.Y));
            }
        }

        private void menu_DeleteSearchedArticle_Clicked(object sender, EventArgs e)
        {
            if (dgv_SearchArticle.SelectedRows == null)
                return;

            int selectedIdx = dgv_SearchArticle.SelectedRows[0].Index;
            SearchedArticleInfo target = searchedList[selectedIdx];

            if (loadingThread != null && loadingThread.IsAlive)
            {
                return;
            }

            if (!conn.IsLogin)
            {
                if (string.IsNullOrWhiteSpace(tb_DeletePassword.Text))
                {
                    tb_DeletePassword.Focus();
                    SetStatusMessage("삭제할 비밀번호를 입력해주세요.");
                    return;
                }
            }

            string password = tb_DeletePassword.Text.Trim();
            GalleryType gallType = GalleryType.Normal;
            if (rb_NormalGallery.Checked)
                gallType = GalleryType.Normal;
            else if (rb_MinorGallery.Checked)
                gallType = GalleryType.Minor;


            loadingThread = new Thread(new ThreadStart(delegate ()
            {
                try
                {
                    if(!conn.IsLogin)
                        conn.DeleteArticle(target, target.Gallery, null, target.ArticleID, password, gallType, false);
                    else
                        conn.DeleteArticle(target, target.Gallery, null, target.ArticleID, "", gallType, false);
                }
                catch
                {
                    return;
                }

                // 갤로그와 갤러리 둘다 삭제 되었을 경우
                if (target.ActualDelete)
                {
                    searchedList.RemoveAt(selectedIdx);

                    this.Invoke(new Action(() =>
                    {
                        dgv_SearchArticle.Rows.RemoveAt(selectedIdx);
                        gb_SearchedArticleList.Text = "검색된 글 [" + searchedList.Count.ToString() + "]";
                        SetStatusMessage("글을 삭제하였습니다.");
                    }));
                }
                else
                {
                    string rmErrMsg = "";
                    if (!target.ActualDelete)
                        rmErrMsg = "글을 삭제하는데 실패하였습니다.";

                    SetStatusMessage(rmErrMsg);
                }
            }));

            SetStatusMessage("글을 삭제하는 중입니다...");

            loadingThread.Start();
        }

        /// <summary>
        /// 상태바의 메시지를 설정하는 함수
        /// </summary>
        /// <param name="msg">설정할 메시지</param>
        private void SetStatusMessage(string msg)
        {
            try
            {
                this.Invoke(new Action(() =>
                {
                    lbl_Status.Text = msg;
                }));
            }
            catch { return; }
        }
    }
}
