using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using DCAdapter;
using System.Threading.Tasks;

namespace DCCleaner
{
    public partial class Frm_Cleaner : Form
    {
        #region Variables
        DCConnector conn;
        CleanerTask currentTask = CleanerTask.None;
        int deleteStartCnt = 0;
        int deleteEndCnt = 0;
        CancellationTokenSource loadingToken;

        private readonly int deleteRetryCnt = 20;
        private readonly int deleteRetryTime = 500;
        #endregion

        #region Properties
        #endregion

        #region Constructor
        public Frm_Cleaner(DCConnector _conn)
        {
            InitializeComponent();
            this.conn = _conn;
        }
        #endregion

        #region Events
        private void Frm_Cleaner_Load(object sender, EventArgs e)
        {
            if(!conn.LoginInfo.IsLoggedIn)
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

        private async void btn_LoadArticles_Click(object sender, EventArgs e)
        {
            if (currentTask == CleanerTask.None)
            {
                List<ArticleInformation> articleList = new List<ArticleInformation>();
                loadingToken = new CancellationTokenSource();

                dgv_ArticleList.Rows.Clear();
                btn_LoadArticles.Text = "취소";
                SetStatusMessage("쓴 글 목록을 불러오는 중입니다...");

                currentTask = CleanerTask.LoadGallogArticles;

                bool cont = true, hasExecption = false;

                for (int page = 1; cont; page++)
                { 
                    try
                    {
                        var loadResult = await conn.LoadGallogItemAsync<ArticleInformation>(page, loadingToken.Token);
                        articleList.AddRange(loadResult.Item1);
                        cont = loadResult.Item2;
                        if (loadResult.Item1 != null)
                            LoadArticleList(loadResult.Item1);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        hasExecption = true;
                        SetStatusMessage(ex.Message);
                        break;
                    }
                }

                currentTask = CleanerTask.None;
                btn_LoadArticles.Text = "불러오기";
                btn_LoadArticles.Enabled = true;
                if (!hasExecption)
                    SetStatusMessage("쓴 글 목록을 불러왔습니다 - 총 " + articleList.Count.ToString() + "개");
                loadingToken = null;
            }
            else if (currentTask == CleanerTask.LoadGallogArticles)
            {
                btn_LoadArticles.Enabled = false;
                SetStatusMessage("취소하는 중입니다...");
                if (loadingToken != null)
                    loadingToken.Cancel();
            }
        }

        private async void btn_LoadComments_Click(object sender, EventArgs e)
        {
            if (currentTask == CleanerTask.None)
            {
                List<CommentInformation>  commentList = new List<CommentInformation>();
                loadingToken = new CancellationTokenSource();

                dgv_CommentList.Rows.Clear();
                btn_LoadComments.Text = "취소";
                SetStatusMessage("쓴 리플 목록을 불러오는 중입니다...");

                currentTask = CleanerTask.LoadGallogComments;

                bool cont = true, hasExecption = false;

                for (int page = 1; cont; page++)
                {
                    try
                    {
                        var loadResult = await conn.LoadGallogItemAsync<CommentInformation>(page, loadingToken.Token);
                        commentList.AddRange(loadResult.Item1);
                        cont = loadResult.Item2;
                        if (loadResult.Item1 != null)
                            LoadCommentList(loadResult.Item1);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        hasExecption = true;
                        SetStatusMessage(ex.Message);
                        break;
                    }
                }

                currentTask = CleanerTask.None;
                btn_LoadComments.Text = "불러오기";
                btn_LoadComments.Enabled = true;
                if (!hasExecption)
                    SetStatusMessage("쓴 리플 목록을 불러왔습니다 - 총 " + commentList.Count.ToString() + "개");
                loadingToken = null;
            }
            else if (currentTask == CleanerTask.LoadGallogComments)
            {
                btn_LoadComments.Enabled = false;
                SetStatusMessage("취소하는 중입니다...");
                if (loadingToken != null)
                    loadingToken.Cancel();
            }
        }

        private void btn_RemoveGallArticle_Click(object sender, EventArgs e)
        {
            RemoveArticles(false);
        }

        private void btn_RemoveAllArticle_Click(object sender, EventArgs e)
        {
            RemoveArticles(true);
        }

        private void btn_RemoveGallComment_Click(object sender, EventArgs e)
        {
            RemoveComments(false);
        }

        private void btn_RemoveAllComment_Click(object sender, EventArgs e)
        {
            RemoveComments(true);
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

            if (dgv_ArticleList.SelectedRows == null || dgv_ArticleList.SelectedRows.Count == 0)
                return;

            DeleteInformationRow row = (dgv_ArticleList.SelectedRows[0] as DeleteInformationRow);
            ArticleInformation target = row.ArticleInformation;

            string msg = "갤러리 삭제 : " + (target.IsGalleryDeleted ? "삭제됨" : "삭제안됨") + Environment.NewLine
                       + "갤로그 삭제 : " + (target.IsGallogDeleted ? "삭제됨" : "삭제안됨") + Environment.NewLine
                       + "메시지 : " + (target.DeleteMessage);

            MessageBox.Show(msg, "글 삭제 정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void dgv_CommentList_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
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

        private void dgv_CommentList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            int currentMouseOverRow = dgv_CommentList.HitTest(e.X, e.Y).RowIndex;

            if (currentMouseOverRow >= 0)
            {
                dgv_CommentList.ClearSelection();

                dgv_CommentList.Rows[currentMouseOverRow].Selected = true;
            }

            if (dgv_CommentList.SelectedRows == null || dgv_CommentList.SelectedRows.Count == 0)
                return;

            DeleteInformationRow row = (dgv_CommentList.SelectedRows[0] as DeleteInformationRow);
            CommentInformation target = row.CommentInformation;

            string msg = "갤러리 삭제 : " + (target.IsGalleryDeleted ? "삭제됨" : "삭제안됨") + Environment.NewLine
                       + "갤로그 삭제 : " + (target.IsGallogDeleted ? "삭제됨" : "삭제안됨") + Environment.NewLine
                       + "메시지 : " + (target.DeleteMessage);

            MessageBox.Show(msg, "리플 삭제 정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void menu_DeleteArticle_Clicked(object sender, EventArgs e)
        {
            if (dgv_ArticleList.SelectedRows == null || dgv_ArticleList.SelectedRows.Count == 0)
                return;

            DeleteInformationRow row = (dgv_ArticleList.SelectedRows[0] as DeleteInformationRow);
            ArticleInformation target = row.ArticleInformation;

            if (currentTask != CleanerTask.None)
                return;

            SetStatusMessage("글을 삭제하는 중입니다...");

            currentTask = CleanerTask.DeleteGallogArticles;

            try
            {
                await conn.DeleteArticle(target, true);
            }
            catch
            {
                return;
            }

            // 갤로그와 갤러리 둘다 삭제 되었을 경우
            if (target.IsGalleryDeleted && target.IsGallogDeleted)
            {
                if (row.DataGridView != null)
                    row.DataGridView.Rows.Remove(row);
                gb_ArticleGroup.Text = "내가 쓴 글 [" + dgv_ArticleList.Rows.Count.ToString() + "]";
                SetStatusMessage("글을 삭제하였습니다.");
            }
            else
            {
                string rmErrMsg = "";
                if (!target.IsGalleryDeleted)
                    rmErrMsg = "글을 삭제하는데 실패하였습니다. - 갤러리 삭제 실패";
                else
                    rmErrMsg = "글을 삭제하는데 실패하였습니다. - 갤로그 삭제 실패";

                SetStatusMessage(rmErrMsg);
            }

            currentTask = CleanerTask.None;
        }

        private async void menu_DeleteComment_Clicked(object sender, EventArgs e)
        {
            if (dgv_CommentList.SelectedRows == null || dgv_CommentList.SelectedRows.Count == 0)
                return;

            DeleteInformationRow row = (dgv_CommentList.SelectedRows[0] as DeleteInformationRow);
            CommentInformation target = row.CommentInformation;

            if (currentTask != CleanerTask.None)
                return;

            SetStatusMessage("리플을 삭제하는 중입니다...");

            currentTask = CleanerTask.DeleteGallogComments;

            try
            {
                await conn.DeleteComment(target, true);
            }
            catch
            {
                return;
            }

            // 갤로그와 갤러리 둘다 삭제 되었을 경우
            if (target.IsGalleryDeleted && target.IsGallogDeleted)
            {
                if (row.DataGridView != null)
                    row.DataGridView.Rows.Remove(row);
                gb_CommentGroup.Text = "내가 쓴 리플 [" + dgv_CommentList.Rows.Count.ToString() + "]";
                SetStatusMessage("리플을 삭제하였습니다.");
            }
            else
            {
                string rmErrMsg = "";
                if (!target.IsGalleryDeleted)
                    rmErrMsg = "리플을 삭제하는데 실패하였습니다. - 갤러리 삭제 실패";
                else
                    rmErrMsg = "리플을 삭제하는데 실패하였습니다. - 갤로그 삭제 실패";

                SetStatusMessage(rmErrMsg);
            }

            currentTask = CleanerTask.None;
        }

        private void tc_CleanerTabContainer_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (currentTask != CleanerTask.None)
            {
                e.Cancel = true;

                return;
            }

            if (e.TabPage == tp_GallogBase && !conn.LoginInfo.IsLoggedIn)
            {
                SetStatusMessage("유동닉은 갤로그 기반 삭제를 할 수 없습니다.");
                e.Cancel = true;
            }
        }

        private async void btn_SearchArticle_Click(object sender, EventArgs e)
        {
            if (currentTask == CleanerTask.None)
            {
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

                string gall_id, nickname;
                gall_id = tb_SearchGalleryID.Text.Trim();
                nickname = tb_SearchNickName.Text.Trim();
                GalleryType gallType = GalleryType.Normal;
                if (rb_NormalGallery.Checked)
                    gallType = GalleryType.Normal;
                else if (rb_MinorGallery.Checked)
                    gallType = GalleryType.Minor;

                btn_SearchArticle.Text = "검색중지";
                SetStatusMessage("글 목록을 검색하는 중입니다...");

                int delay = 50;
                int pos = 0;
                int page = 1;
                bool cont = false, hasExecption = false;
                List<ArticleInformation> newSearchedList;
                Tuple<List<ArticleInformation>, int, int, bool> req = null;

                currentTask = CleanerTask.SearchGalleryArticles;

                if (loadingToken == null)
                    loadingToken = new CancellationTokenSource();

                while (pos != -1)
                {
                    try
                    {
                        req = await conn.SearchArticleAsync(gall_id, gallType, nickname, pos, page, loadingToken.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        hasExecption = true;
                        SetStatusMessage(ex.Message);
                        break;
                    }
                    finally
                    {
                        if (req != null)
                        {
                            newSearchedList = req.Item1;
                            pos = req.Item2;
                            page = req.Item3;
                            cont = req.Item4;
                            
                            LoadSearchedList(newSearchedList);

                            newSearchedList.Clear();
                            newSearchedList = null;
                        }

                        await Task.Delay(delay);
                    }

                    if (!cont || req == null)
                        break;
                }

                currentTask = CleanerTask.None;
                btn_SearchArticle.Text = "검색하기";
                if (!hasExecption)
                    SetStatusMessage("검색된 글 목록을 불러왔습니다 - 총 " + dgv_SearchArticle.Rows.Count.ToString() + "개");

                loadingToken = null;
            }
            else if (currentTask == CleanerTask.SearchGalleryArticles)
            {
                SetStatusMessage("검색을 중단하는 중입니다...");

                if (loadingToken != null)
                    loadingToken.Cancel();
            }
        }

        private void btn_RemoveSearchedArticle_Click(object sender, EventArgs e)
        {
            if (dgv_SearchArticle.Rows.Count == 0)
                return;

            if (currentTask != CleanerTask.None)
                return;

            if (!conn.LoginInfo.IsLoggedIn)
            {
                if (string.IsNullOrWhiteSpace(tb_DeletePassword.Text))
                {
                    tb_DeletePassword.Focus();
                    SetStatusMessage("삭제할 비밀번호를 입력해주세요.");
                    return;
                }
            }

            SetStatusMessage("검색된 글 삭제중...");

            currentTask = CleanerTask.DeleteGalleryArticles;

            string password = tb_DeletePassword.Text.Trim();
            GalleryType gallType = GalleryType.Normal;
            if (rb_NormalGallery.Checked)
                gallType = GalleryType.Normal;
            else if (rb_MinorGallery.Checked)
                gallType = GalleryType.Minor;

            deleteStartCnt = dgv_SearchArticle.Rows.Count;
            deleteEndCnt = 0;

            for (int i = deleteStartCnt - 1; i >= 0; i--)
            {
                DeleteInformationRow row = (dgv_SearchArticle.Rows[i] as DeleteInformationRow);
                DeleteSearchedArticleAsync(row, gallType, password);
            }
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

            if (dgv_SearchArticle.SelectedRows == null || dgv_SearchArticle.SelectedRows.Count == 0)
                return;

            DeleteInformationRow row = (dgv_SearchArticle.SelectedRows[0] as DeleteInformationRow);
            ArticleInformation target = row.ArticleInformation;

            string msg = "상태 : " + (target.IsGalleryDeleted ? "삭제됨" : "삭제안됨") + Environment.NewLine
                       + "메시지 : " + (target.DeleteMessage);

            MessageBox.Show(msg, "글 삭제 정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void menu_DeleteSearchedArticle_Clicked(object sender, EventArgs e)
        {
            if (dgv_SearchArticle.SelectedRows == null || dgv_SearchArticle.SelectedRows.Count == 0)
                return;
            
            DeleteInformationRow row = (dgv_SearchArticle.SelectedRows[0] as DeleteInformationRow);
            ArticleInformation target = row.ArticleInformation;

            if (currentTask != CleanerTask.None)
                return;

            if (!conn.LoginInfo.IsLoggedIn)
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

            SetStatusMessage("글을 삭제하는 중입니다...");

            currentTask = CleanerTask.DeleteGalleryArticles;

            try
            {
                if (!conn.LoginInfo.IsLoggedIn)
                    target.GalleryDeleteParameter.Password = password;
                await conn.DeleteArticle(target, gallType, false);
            }
            catch
            {
                return;
            }

            // 갤로그와 갤러리 둘다 삭제 되었을 경우
            if (target.IsGalleryDeleted)
            {
                this.Invoke(new Action(() =>
                {
                    if (row.DataGridView != null)
                        row.DataGridView.Rows.Remove(row);
                    gb_SearchedArticleList.Text = "검색된 글 [" + dgv_SearchArticle.Rows.Count.ToString() + "]";
                    SetStatusMessage("글을 삭제하였습니다.");
                }));
            }
            else
            {
                string rmErrMsg = "";
                if (!target.IsGalleryDeleted)
                    rmErrMsg = "글을 삭제하는데 실패하였습니다.";

                SetStatusMessage(rmErrMsg);
            }

            currentTask = CleanerTask.None;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 글 목록 삭제 함수
        /// </summary>
        /// <param name="both">True : 갤로그도, False : 갤러리만</param>
        private void RemoveArticles(bool both)
        {
            if (dgv_ArticleList.Rows.Count <= 0)
                return;

            if (currentTask != CleanerTask.None)
                return;

            if (both)
                SetStatusMessage("쓴 글 - 갤로그도 삭제중...");
            else
                SetStatusMessage("쓴 글 - 갤러리만 삭제중...");

            currentTask = CleanerTask.DeleteGallogArticles;
            deleteStartCnt = dgv_ArticleList.Rows.Count;
            deleteEndCnt = 0;

            for (int i = deleteStartCnt - 1; i >= 0; i--)
            {
                DeleteInformationRow row = (dgv_ArticleList.Rows[i] as DeleteInformationRow);
                DeleteArticleAsync(row, both);
            }
        }

        /// <summary>
        /// 댓글 목록 삭제 함수
        /// </summary>
        /// <param name="both">True : 갤로그도 False : 갤러리만</param>
        private void RemoveComments(bool both)
        {
            if (dgv_CommentList.Rows.Count <= 0)
                return;

            if (currentTask != CleanerTask.None)
                return;

            if (both)
                SetStatusMessage("쓴 리플 - 갤로그도 삭제중...");
            else
                SetStatusMessage("쓴 리플 - 갤러리만 삭제중...");

            currentTask = CleanerTask.DeleteGallogComments;
            deleteStartCnt = dgv_CommentList.Rows.Count;
            deleteEndCnt = 0;

            for (int i = deleteStartCnt - 1; i >= 0; i--)
            {
                DeleteInformationRow row = (dgv_CommentList.Rows[i] as DeleteInformationRow);
                DeleteCommentAsync(row, both);
            }
        }

        private async void DeleteArticleAsync(DeleteInformationRow row, bool both)
        {
            ArticleInformation info = row.ArticleInformation;
            ArticleInformation res = null;
            try
            {
                res = await conn.DeleteArticle(info, both);
            }
            catch { }

            if (!res.IsGalleryDeleted || (both && !res.IsGallogDeleted))
                for (int j = 0; j < deleteRetryCnt; j++)
                {
                    // 실패시, Sleep 후 재시도
                    await Task.Delay(deleteRetryTime);
                    res = await conn.DeleteArticle(info, both);
                    if (res.IsGalleryDeleted && (!both || res.IsGallogDeleted))
                        break;
                }

            info.IsGalleryDeleted = res.IsGalleryDeleted;
            info.IsGallogDeleted = res.IsGallogDeleted;
            info.DeleteMessage = res.DeleteMessage;

            deleteEndCnt++;

            if (!info.IsGalleryDeleted || (both && !info.IsGallogDeleted))
            {
                if (deleteStartCnt <= deleteEndCnt)
                {
                    currentTask = CleanerTask.None;

                    if (both)
                        SetStatusMessage("쓴 글 - 갤로그도 삭제 완료");
                    else
                        SetStatusMessage("쓴 글 - 갤러리만 삭제 완료");
                }
                return;
            }

            // 갤로그도 삭제일 경우에만 화면 지움
            if (both)
            {
                if (row.DataGridView != null)
                    row.DataGridView.Rows.Remove(row);
                gb_ArticleGroup.Text = "내가 쓴 글 [" + dgv_ArticleList.Rows.Count + "]";
            }

            if (deleteStartCnt <= deleteEndCnt)
            {
                currentTask = CleanerTask.None;

                if (both)
                    SetStatusMessage("쓴 글 - 갤로그도 삭제 완료");
                else
                    SetStatusMessage("쓴 글 - 갤러리만 삭제 완료");
            }
        }

        private async void DeleteCommentAsync(DeleteInformationRow row, bool both)
        {
            CommentInformation info = row.CommentInformation;
            CommentInformation res = null;
            try
            {
                res = await conn.DeleteComment(info, both);
            }
            catch { }

            if (!res.IsGalleryDeleted || (both && !res.IsGallogDeleted))
                for (int j = 0; j < deleteRetryCnt; j++)
                {
                    // 실패시, Sleep 후 재시도
                    await Task.Delay(deleteRetryTime);
                    res = await conn.DeleteComment(info, both);
                    if (res.IsGalleryDeleted && (!both || res.IsGallogDeleted))
                        break;
                }

            info.IsGalleryDeleted = res.IsGalleryDeleted;
            info.IsGallogDeleted = res.IsGallogDeleted;
            info.DeleteMessage = res.DeleteMessage;

            deleteEndCnt++;

            if (!info.IsGalleryDeleted || (both && !info.IsGallogDeleted))
            {
                if (deleteStartCnt <= deleteEndCnt)
                {
                    currentTask = CleanerTask.None;

                    if (both)
                        SetStatusMessage("쓴 리플 - 갤로그도 삭제 완료");
                    else
                        SetStatusMessage("쓴 리플 - 갤러리만 삭제 완료");
                }
                return;
            }

            // 갤로그도 삭제일 경우에만 화면 지움
            if (both)
            {
                if (row.DataGridView != null)
                    row.DataGridView.Rows.Remove(row);
                gb_CommentGroup.Text = "내가 쓴 리플 [" + dgv_CommentList.Rows.Count.ToString() + "]";
            }

            if (deleteStartCnt <= deleteEndCnt)
            {
                currentTask = CleanerTask.None;

                if (both)
                    SetStatusMessage("쓴 리플 - 갤로그도 삭제 완료");
                else
                    SetStatusMessage("쓴 리플 - 갤러리만 삭제 완료");
            }
        }

        private async void DeleteSearchedArticleAsync(DeleteInformationRow row, GalleryType gallType, string password)
        {
            ArticleInformation info = row.ArticleInformation;
            ArticleInformation res = null;
            try
            {
                if (!conn.LoginInfo.IsLoggedIn)
                    info.GalleryDeleteParameter.Password = password;
                res = await conn.DeleteArticle(info, gallType, false);
            }
            catch { }

            if (!res.IsGalleryDeleted)
                for (int j = 0; j < deleteRetryCnt; j++)
                {
                    // 실패시, Sleep 후 재시도
                    await Task.Delay(deleteRetryTime);
                    res = await conn.DeleteArticle(info, gallType, false);
                    if (res.IsGalleryDeleted)
                        break;
                }

            info.IsGalleryDeleted = res.IsGalleryDeleted;
            info.IsGallogDeleted = res.IsGallogDeleted;
            info.DeleteMessage = res.DeleteMessage;

            deleteEndCnt++;

            if (!info.IsGalleryDeleted)
            {
                if (deleteStartCnt <= deleteEndCnt)
                {
                    currentTask = CleanerTask.None;

                    SetStatusMessage("검색된 글 삭제 완료");
                }
                return;
            }

            if (row.DataGridView != null)
                row.DataGridView.Rows.Remove(row);
            gb_SearchedArticleList.Text = "검색된 글 [" + dgv_SearchArticle.Rows.Count.ToString() + "]";
            
            if (deleteStartCnt <= deleteEndCnt)
            {
                currentTask = CleanerTask.None;

                SetStatusMessage("검색된 글 삭제 완료");
            }
        }

        private void LoadArticleList(List<ArticleInformation> newArticleList)
        {
            foreach (ArticleInformation info in newArticleList)
            {
                DeleteInformationRow nRow = new DeleteInformationRow(info, dgv_ArticleList, false);
                dgv_ArticleList.Rows.Add(nRow);
            }

            string loadedCnt = dgv_ArticleList.Rows.Count.ToString();
            gb_ArticleGroup.Text = "내가 쓴 글 [" + loadedCnt + "]";
        }

        private void LoadCommentList(List<CommentInformation> newArticleList)
        {
            foreach (CommentInformation info in newArticleList)
            {
                DeleteInformationRow nRow = new DeleteInformationRow(info, dgv_CommentList);
                dgv_CommentList.Rows.Add(nRow);
            }

            string loadedCnt = dgv_CommentList.Rows.Count.ToString();
            gb_CommentGroup.Text = "내가 쓴 리플 [" + loadedCnt + "]";
        }

        private void LoadSearchedList(List<ArticleInformation> searchedList)
        {
            foreach (ArticleInformation info in searchedList)
            {
                DeleteInformationRow nRow = new DeleteInformationRow(info, dgv_SearchArticle, true);
                dgv_SearchArticle.Rows.Add(nRow);
            }

            string loadedCnt = dgv_SearchArticle.Rows.Count.ToString();
            gb_SearchedArticleList.Text = "검색된 글 [" + loadedCnt + "]";
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
        #endregion
    }
}
