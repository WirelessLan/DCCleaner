namespace DCCleaner
{
    partial class Frm_Cleaner
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tlp_CleanerSeprator = new System.Windows.Forms.TableLayoutPanel();
            this.gb_CommentGroup = new System.Windows.Forms.GroupBox();
            this.btn_RemoveAllComment = new System.Windows.Forms.Button();
            this.btn_RemoveGallComment = new System.Windows.Forms.Button();
            this.dgv_CommentList = new System.Windows.Forms.DataGridView();
            this.col_Name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_Content = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_Date = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btn_LoadComments = new System.Windows.Forms.Button();
            this.gb_ArticleGroup = new System.Windows.Forms.GroupBox();
            this.btn_RemoveGallArticle = new System.Windows.Forms.Button();
            this.btn_RemoveAllArticle = new System.Windows.Forms.Button();
            this.dgv_ArticleList = new System.Windows.Forms.DataGridView();
            this.col_Title = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btn_LoadArticles = new System.Windows.Forms.Button();
            this.ststp_Status = new System.Windows.Forms.StatusStrip();
            this.lbl_Status = new System.Windows.Forms.ToolStripStatusLabel();
            this.tc_CleanerTabContainer = new System.Windows.Forms.TabControl();
            this.tp_GallogBase = new System.Windows.Forms.TabPage();
            this.tp_SearchBase = new System.Windows.Forms.TabPage();
            this.pl_SearchBasePanel = new System.Windows.Forms.Panel();
            this.gb_SearchedArticleList = new System.Windows.Forms.GroupBox();
            this.btn_AbortSearch = new System.Windows.Forms.Button();
            this.btn_SearchArticle = new System.Windows.Forms.Button();
            this.dgv_SearchArticle = new System.Windows.Forms.DataGridView();
            this.dgvCol_Title = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvCol_Date = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btn_DeleteSearchedArticle = new System.Windows.Forms.Button();
            this.gb_SearchInfo = new System.Windows.Forms.GroupBox();
            this.lbl_Password = new System.Windows.Forms.Label();
            this.lbl_SearchID = new System.Windows.Forms.Label();
            this.tb_DeletePassword = new System.Windows.Forms.TextBox();
            this.tb_SearchNickName = new System.Windows.Forms.TextBox();
            this.lbl_SearchBaseWarn = new System.Windows.Forms.Label();
            this.gb_GalleryInfo = new System.Windows.Forms.GroupBox();
            this.tb_SearchGalleryID = new System.Windows.Forms.TextBox();
            this.lbl_GalleryID = new System.Windows.Forms.Label();
            this.lbl_GalleryType = new System.Windows.Forms.Label();
            this.rb_MinorGallery = new System.Windows.Forms.RadioButton();
            this.rb_NormalGallery = new System.Windows.Forms.RadioButton();
            this.tlp_CleanerSeprator.SuspendLayout();
            this.gb_CommentGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_CommentList)).BeginInit();
            this.gb_ArticleGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_ArticleList)).BeginInit();
            this.ststp_Status.SuspendLayout();
            this.tc_CleanerTabContainer.SuspendLayout();
            this.tp_GallogBase.SuspendLayout();
            this.tp_SearchBase.SuspendLayout();
            this.pl_SearchBasePanel.SuspendLayout();
            this.gb_SearchedArticleList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_SearchArticle)).BeginInit();
            this.gb_SearchInfo.SuspendLayout();
            this.gb_GalleryInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlp_CleanerSeprator
            // 
            this.tlp_CleanerSeprator.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlp_CleanerSeprator.ColumnCount = 3;
            this.tlp_CleanerSeprator.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlp_CleanerSeprator.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlp_CleanerSeprator.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlp_CleanerSeprator.Controls.Add(this.gb_CommentGroup, 2, 0);
            this.tlp_CleanerSeprator.Controls.Add(this.gb_ArticleGroup, 0, 0);
            this.tlp_CleanerSeprator.Location = new System.Drawing.Point(0, 0);
            this.tlp_CleanerSeprator.Name = "tlp_CleanerSeprator";
            this.tlp_CleanerSeprator.RowCount = 1;
            this.tlp_CleanerSeprator.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlp_CleanerSeprator.Size = new System.Drawing.Size(776, 507);
            this.tlp_CleanerSeprator.TabIndex = 0;
            // 
            // gb_CommentGroup
            // 
            this.gb_CommentGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gb_CommentGroup.Controls.Add(this.btn_RemoveAllComment);
            this.gb_CommentGroup.Controls.Add(this.btn_RemoveGallComment);
            this.gb_CommentGroup.Controls.Add(this.dgv_CommentList);
            this.gb_CommentGroup.Controls.Add(this.btn_LoadComments);
            this.gb_CommentGroup.Location = new System.Drawing.Point(391, 3);
            this.gb_CommentGroup.Name = "gb_CommentGroup";
            this.gb_CommentGroup.Size = new System.Drawing.Size(382, 501);
            this.gb_CommentGroup.TabIndex = 1;
            this.gb_CommentGroup.TabStop = false;
            this.gb_CommentGroup.Text = "내가 쓴 리플";
            // 
            // btn_RemoveAllComment
            // 
            this.btn_RemoveAllComment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_RemoveAllComment.Location = new System.Drawing.Point(286, 474);
            this.btn_RemoveAllComment.Name = "btn_RemoveAllComment";
            this.btn_RemoveAllComment.Size = new System.Drawing.Size(91, 23);
            this.btn_RemoveAllComment.TabIndex = 5;
            this.btn_RemoveAllComment.Text = "갤로그도 삭제";
            this.btn_RemoveAllComment.UseVisualStyleBackColor = true;
            this.btn_RemoveAllComment.Click += new System.EventHandler(this.btn_RemoveAllComment_Click);
            // 
            // btn_RemoveGallComment
            // 
            this.btn_RemoveGallComment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_RemoveGallComment.Location = new System.Drawing.Point(185, 474);
            this.btn_RemoveGallComment.Name = "btn_RemoveGallComment";
            this.btn_RemoveGallComment.Size = new System.Drawing.Size(95, 23);
            this.btn_RemoveGallComment.TabIndex = 5;
            this.btn_RemoveGallComment.Text = "갤러리만 삭제";
            this.btn_RemoveGallComment.UseVisualStyleBackColor = true;
            this.btn_RemoveGallComment.Click += new System.EventHandler(this.btn_RemoveGallComment_Click);
            // 
            // dgv_CommentList
            // 
            this.dgv_CommentList.AllowUserToAddRows = false;
            this.dgv_CommentList.AllowUserToDeleteRows = false;
            this.dgv_CommentList.AllowUserToResizeRows = false;
            this.dgv_CommentList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgv_CommentList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_CommentList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.col_Name,
            this.col_Content,
            this.col_Date});
            this.dgv_CommentList.Location = new System.Drawing.Point(6, 20);
            this.dgv_CommentList.MultiSelect = false;
            this.dgv_CommentList.Name = "dgv_CommentList";
            this.dgv_CommentList.ReadOnly = true;
            this.dgv_CommentList.RowHeadersVisible = false;
            this.dgv_CommentList.RowTemplate.Height = 23;
            this.dgv_CommentList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv_CommentList.Size = new System.Drawing.Size(370, 448);
            this.dgv_CommentList.TabIndex = 3;
            this.dgv_CommentList.MouseClick += new System.Windows.Forms.MouseEventHandler(this.dgv_CommentList_MouseClick);
            this.dgv_CommentList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.dgv_CommentList_MouseDoubleClick);
            // 
            // col_Name
            // 
            this.col_Name.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.col_Name.FillWeight = 20F;
            this.col_Name.HeaderText = "이름";
            this.col_Name.Name = "col_Name";
            this.col_Name.ReadOnly = true;
            // 
            // col_Content
            // 
            this.col_Content.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.col_Content.FillWeight = 70F;
            this.col_Content.HeaderText = "내용";
            this.col_Content.Name = "col_Content";
            this.col_Content.ReadOnly = true;
            // 
            // col_Date
            // 
            this.col_Date.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.col_Date.FillWeight = 20F;
            this.col_Date.HeaderText = "날짜";
            this.col_Date.Name = "col_Date";
            this.col_Date.ReadOnly = true;
            // 
            // btn_LoadComments
            // 
            this.btn_LoadComments.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_LoadComments.Location = new System.Drawing.Point(6, 474);
            this.btn_LoadComments.Name = "btn_LoadComments";
            this.btn_LoadComments.Size = new System.Drawing.Size(75, 23);
            this.btn_LoadComments.TabIndex = 2;
            this.btn_LoadComments.Text = "불러오기";
            this.btn_LoadComments.UseVisualStyleBackColor = true;
            this.btn_LoadComments.Click += new System.EventHandler(this.btn_LoadComments_Click);
            // 
            // gb_ArticleGroup
            // 
            this.gb_ArticleGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gb_ArticleGroup.Controls.Add(this.btn_RemoveGallArticle);
            this.gb_ArticleGroup.Controls.Add(this.btn_RemoveAllArticle);
            this.gb_ArticleGroup.Controls.Add(this.dgv_ArticleList);
            this.gb_ArticleGroup.Controls.Add(this.btn_LoadArticles);
            this.gb_ArticleGroup.Location = new System.Drawing.Point(3, 3);
            this.gb_ArticleGroup.Name = "gb_ArticleGroup";
            this.gb_ArticleGroup.Size = new System.Drawing.Size(382, 501);
            this.gb_ArticleGroup.TabIndex = 0;
            this.gb_ArticleGroup.TabStop = false;
            this.gb_ArticleGroup.Text = "내가 쓴 글";
            // 
            // btn_RemoveGallArticle
            // 
            this.btn_RemoveGallArticle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_RemoveGallArticle.Location = new System.Drawing.Point(184, 474);
            this.btn_RemoveGallArticle.Name = "btn_RemoveGallArticle";
            this.btn_RemoveGallArticle.Size = new System.Drawing.Size(95, 23);
            this.btn_RemoveGallArticle.TabIndex = 4;
            this.btn_RemoveGallArticle.Text = "갤러리만 삭제";
            this.btn_RemoveGallArticle.UseVisualStyleBackColor = true;
            this.btn_RemoveGallArticle.Click += new System.EventHandler(this.btn_RemoveGallArticle_Click);
            // 
            // btn_RemoveAllArticle
            // 
            this.btn_RemoveAllArticle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_RemoveAllArticle.Location = new System.Drawing.Point(285, 474);
            this.btn_RemoveAllArticle.Name = "btn_RemoveAllArticle";
            this.btn_RemoveAllArticle.Size = new System.Drawing.Size(91, 23);
            this.btn_RemoveAllArticle.TabIndex = 3;
            this.btn_RemoveAllArticle.Text = "갤로그도 삭제";
            this.btn_RemoveAllArticle.UseVisualStyleBackColor = true;
            this.btn_RemoveAllArticle.Click += new System.EventHandler(this.btn_RemoveAllArticle_Click);
            // 
            // dgv_ArticleList
            // 
            this.dgv_ArticleList.AllowUserToAddRows = false;
            this.dgv_ArticleList.AllowUserToDeleteRows = false;
            this.dgv_ArticleList.AllowUserToResizeRows = false;
            this.dgv_ArticleList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgv_ArticleList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_ArticleList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.col_Title});
            this.dgv_ArticleList.Location = new System.Drawing.Point(7, 21);
            this.dgv_ArticleList.MultiSelect = false;
            this.dgv_ArticleList.Name = "dgv_ArticleList";
            this.dgv_ArticleList.ReadOnly = true;
            this.dgv_ArticleList.RowHeadersVisible = false;
            this.dgv_ArticleList.RowTemplate.Height = 23;
            this.dgv_ArticleList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv_ArticleList.Size = new System.Drawing.Size(369, 447);
            this.dgv_ArticleList.TabIndex = 2;
            this.dgv_ArticleList.MouseClick += new System.Windows.Forms.MouseEventHandler(this.dgv_ArticleList_MouseClick);
            this.dgv_ArticleList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.dgv_ArticleList_MouseDoubleClick);
            // 
            // col_Title
            // 
            this.col_Title.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.col_Title.HeaderText = "제목";
            this.col_Title.Name = "col_Title";
            this.col_Title.ReadOnly = true;
            // 
            // btn_LoadArticles
            // 
            this.btn_LoadArticles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_LoadArticles.Location = new System.Drawing.Point(7, 474);
            this.btn_LoadArticles.Name = "btn_LoadArticles";
            this.btn_LoadArticles.Size = new System.Drawing.Size(75, 23);
            this.btn_LoadArticles.TabIndex = 1;
            this.btn_LoadArticles.Text = "불러오기";
            this.btn_LoadArticles.UseVisualStyleBackColor = true;
            this.btn_LoadArticles.Click += new System.EventHandler(this.btn_LoadArticles_Click);
            // 
            // ststp_Status
            // 
            this.ststp_Status.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lbl_Status});
            this.ststp_Status.Location = new System.Drawing.Point(0, 539);
            this.ststp_Status.Name = "ststp_Status";
            this.ststp_Status.Size = new System.Drawing.Size(784, 22);
            this.ststp_Status.TabIndex = 1;
            // 
            // lbl_Status
            // 
            this.lbl_Status.Name = "lbl_Status";
            this.lbl_Status.Size = new System.Drawing.Size(0, 17);
            // 
            // tc_CleanerTabContainer
            // 
            this.tc_CleanerTabContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tc_CleanerTabContainer.Controls.Add(this.tp_GallogBase);
            this.tc_CleanerTabContainer.Controls.Add(this.tp_SearchBase);
            this.tc_CleanerTabContainer.Location = new System.Drawing.Point(0, 3);
            this.tc_CleanerTabContainer.Name = "tc_CleanerTabContainer";
            this.tc_CleanerTabContainer.SelectedIndex = 0;
            this.tc_CleanerTabContainer.Size = new System.Drawing.Size(784, 533);
            this.tc_CleanerTabContainer.TabIndex = 0;
            this.tc_CleanerTabContainer.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.tc_CleanerTabContainer_Selecting);
            // 
            // tp_GallogBase
            // 
            this.tp_GallogBase.Controls.Add(this.tlp_CleanerSeprator);
            this.tp_GallogBase.Location = new System.Drawing.Point(4, 22);
            this.tp_GallogBase.Name = "tp_GallogBase";
            this.tp_GallogBase.Size = new System.Drawing.Size(776, 507);
            this.tp_GallogBase.TabIndex = 0;
            this.tp_GallogBase.Text = "갤로그 기반";
            this.tp_GallogBase.UseVisualStyleBackColor = true;
            // 
            // tp_SearchBase
            // 
            this.tp_SearchBase.Controls.Add(this.pl_SearchBasePanel);
            this.tp_SearchBase.Location = new System.Drawing.Point(4, 22);
            this.tp_SearchBase.Name = "tp_SearchBase";
            this.tp_SearchBase.Size = new System.Drawing.Size(776, 507);
            this.tp_SearchBase.TabIndex = 0;
            this.tp_SearchBase.Text = "검색 기반";
            this.tp_SearchBase.UseVisualStyleBackColor = true;
            // 
            // pl_SearchBasePanel
            // 
            this.pl_SearchBasePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pl_SearchBasePanel.Controls.Add(this.gb_SearchedArticleList);
            this.pl_SearchBasePanel.Controls.Add(this.gb_SearchInfo);
            this.pl_SearchBasePanel.Controls.Add(this.lbl_SearchBaseWarn);
            this.pl_SearchBasePanel.Controls.Add(this.gb_GalleryInfo);
            this.pl_SearchBasePanel.Location = new System.Drawing.Point(0, 0);
            this.pl_SearchBasePanel.Name = "pl_SearchBasePanel";
            this.pl_SearchBasePanel.Size = new System.Drawing.Size(780, 511);
            this.pl_SearchBasePanel.TabIndex = 0;
            // 
            // gb_SearchedArticleList
            // 
            this.gb_SearchedArticleList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gb_SearchedArticleList.Controls.Add(this.btn_AbortSearch);
            this.gb_SearchedArticleList.Controls.Add(this.btn_SearchArticle);
            this.gb_SearchedArticleList.Controls.Add(this.dgv_SearchArticle);
            this.gb_SearchedArticleList.Controls.Add(this.btn_DeleteSearchedArticle);
            this.gb_SearchedArticleList.Location = new System.Drawing.Point(8, 208);
            this.gb_SearchedArticleList.Name = "gb_SearchedArticleList";
            this.gb_SearchedArticleList.Size = new System.Drawing.Size(760, 295);
            this.gb_SearchedArticleList.TabIndex = 3;
            this.gb_SearchedArticleList.TabStop = false;
            this.gb_SearchedArticleList.Text = "검색된 글";
            // 
            // btn_AbortSearch
            // 
            this.btn_AbortSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_AbortSearch.Location = new System.Drawing.Point(87, 266);
            this.btn_AbortSearch.Name = "btn_AbortSearch";
            this.btn_AbortSearch.Size = new System.Drawing.Size(75, 23);
            this.btn_AbortSearch.TabIndex = 2;
            this.btn_AbortSearch.Text = "검색 중지";
            this.btn_AbortSearch.UseVisualStyleBackColor = true;
            this.btn_AbortSearch.Click += new System.EventHandler(this.btn_AbortSearch_Click);
            // 
            // btn_SearchArticle
            // 
            this.btn_SearchArticle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_SearchArticle.Location = new System.Drawing.Point(6, 266);
            this.btn_SearchArticle.Name = "btn_SearchArticle";
            this.btn_SearchArticle.Size = new System.Drawing.Size(75, 23);
            this.btn_SearchArticle.TabIndex = 1;
            this.btn_SearchArticle.Text = "검색하기";
            this.btn_SearchArticle.UseVisualStyleBackColor = true;
            this.btn_SearchArticle.Click += new System.EventHandler(this.btn_SearchArticle_Click);
            // 
            // dgv_SearchArticle
            // 
            this.dgv_SearchArticle.AllowUserToAddRows = false;
            this.dgv_SearchArticle.AllowUserToDeleteRows = false;
            this.dgv_SearchArticle.AllowUserToResizeRows = false;
            this.dgv_SearchArticle.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgv_SearchArticle.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_SearchArticle.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dgvCol_Title,
            this.dgvCol_Date});
            this.dgv_SearchArticle.Location = new System.Drawing.Point(6, 20);
            this.dgv_SearchArticle.MultiSelect = false;
            this.dgv_SearchArticle.Name = "dgv_SearchArticle";
            this.dgv_SearchArticle.ReadOnly = true;
            this.dgv_SearchArticle.RowHeadersVisible = false;
            this.dgv_SearchArticle.RowTemplate.Height = 23;
            this.dgv_SearchArticle.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv_SearchArticle.Size = new System.Drawing.Size(748, 240);
            this.dgv_SearchArticle.TabIndex = 0;
            this.dgv_SearchArticle.MouseClick += new System.Windows.Forms.MouseEventHandler(this.dgv_SearchArticle_MouseClick);
            this.dgv_SearchArticle.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.dgv_SearchArticle_MouseDoubleClick);
            // 
            // dgvCol_Title
            // 
            this.dgvCol_Title.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dgvCol_Title.HeaderText = "제목";
            this.dgvCol_Title.Name = "dgvCol_Title";
            this.dgvCol_Title.ReadOnly = true;
            // 
            // dgvCol_Date
            // 
            this.dgvCol_Date.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dgvCol_Date.FillWeight = 30F;
            this.dgvCol_Date.HeaderText = "날짜";
            this.dgvCol_Date.Name = "dgvCol_Date";
            this.dgvCol_Date.ReadOnly = true;
            // 
            // btn_DeleteSearchedArticle
            // 
            this.btn_DeleteSearchedArticle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_DeleteSearchedArticle.Location = new System.Drawing.Point(679, 266);
            this.btn_DeleteSearchedArticle.Name = "btn_DeleteSearchedArticle";
            this.btn_DeleteSearchedArticle.Size = new System.Drawing.Size(75, 23);
            this.btn_DeleteSearchedArticle.TabIndex = 3;
            this.btn_DeleteSearchedArticle.Text = "전체 삭제";
            this.btn_DeleteSearchedArticle.UseVisualStyleBackColor = true;
            this.btn_DeleteSearchedArticle.Click += new System.EventHandler(this.btn_DeleteSearchedArticle_Click);
            // 
            // gb_SearchInfo
            // 
            this.gb_SearchInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gb_SearchInfo.Controls.Add(this.lbl_Password);
            this.gb_SearchInfo.Controls.Add(this.lbl_SearchID);
            this.gb_SearchInfo.Controls.Add(this.tb_DeletePassword);
            this.gb_SearchInfo.Controls.Add(this.tb_SearchNickName);
            this.gb_SearchInfo.Location = new System.Drawing.Point(8, 101);
            this.gb_SearchInfo.Name = "gb_SearchInfo";
            this.gb_SearchInfo.Size = new System.Drawing.Size(760, 83);
            this.gb_SearchInfo.TabIndex = 1;
            this.gb_SearchInfo.TabStop = false;
            this.gb_SearchInfo.Text = "검색 정보";
            // 
            // lbl_Password
            // 
            this.lbl_Password.AutoSize = true;
            this.lbl_Password.Location = new System.Drawing.Point(23, 54);
            this.lbl_Password.Name = "lbl_Password";
            this.lbl_Password.Size = new System.Drawing.Size(53, 12);
            this.lbl_Password.TabIndex = 2;
            this.lbl_Password.Text = "비밀번호";
            // 
            // lbl_SearchID
            // 
            this.lbl_SearchID.AutoSize = true;
            this.lbl_SearchID.Location = new System.Drawing.Point(23, 27);
            this.lbl_SearchID.Name = "lbl_SearchID";
            this.lbl_SearchID.Size = new System.Drawing.Size(41, 12);
            this.lbl_SearchID.TabIndex = 0;
            this.lbl_SearchID.Text = "닉네임";
            // 
            // tb_DeletePassword
            // 
            this.tb_DeletePassword.Location = new System.Drawing.Point(128, 48);
            this.tb_DeletePassword.Name = "tb_DeletePassword";
            this.tb_DeletePassword.Size = new System.Drawing.Size(207, 21);
            this.tb_DeletePassword.TabIndex = 3;
            // 
            // tb_SearchNickName
            // 
            this.tb_SearchNickName.Location = new System.Drawing.Point(128, 21);
            this.tb_SearchNickName.Name = "tb_SearchNickName";
            this.tb_SearchNickName.Size = new System.Drawing.Size(207, 21);
            this.tb_SearchNickName.TabIndex = 1;
            // 
            // lbl_SearchBaseWarn
            // 
            this.lbl_SearchBaseWarn.AutoSize = true;
            this.lbl_SearchBaseWarn.ForeColor = System.Drawing.Color.Red;
            this.lbl_SearchBaseWarn.Location = new System.Drawing.Point(8, 192);
            this.lbl_SearchBaseWarn.Name = "lbl_SearchBaseWarn";
            this.lbl_SearchBaseWarn.Size = new System.Drawing.Size(365, 12);
            this.lbl_SearchBaseWarn.TabIndex = 2;
            this.lbl_SearchBaseWarn.Text = "※ 주의 : 유동닉은 갤로그 기반 삭제 및 리플 삭제가 불가능합니다.";
            // 
            // gb_GalleryInfo
            // 
            this.gb_GalleryInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gb_GalleryInfo.Controls.Add(this.tb_SearchGalleryID);
            this.gb_GalleryInfo.Controls.Add(this.lbl_GalleryID);
            this.gb_GalleryInfo.Controls.Add(this.lbl_GalleryType);
            this.gb_GalleryInfo.Controls.Add(this.rb_MinorGallery);
            this.gb_GalleryInfo.Controls.Add(this.rb_NormalGallery);
            this.gb_GalleryInfo.Location = new System.Drawing.Point(8, 8);
            this.gb_GalleryInfo.Name = "gb_GalleryInfo";
            this.gb_GalleryInfo.Size = new System.Drawing.Size(760, 87);
            this.gb_GalleryInfo.TabIndex = 0;
            this.gb_GalleryInfo.TabStop = false;
            this.gb_GalleryInfo.Text = "갤러리 정보";
            // 
            // tb_SearchGalleryID
            // 
            this.tb_SearchGalleryID.Location = new System.Drawing.Point(128, 51);
            this.tb_SearchGalleryID.Name = "tb_SearchGalleryID";
            this.tb_SearchGalleryID.Size = new System.Drawing.Size(207, 21);
            this.tb_SearchGalleryID.TabIndex = 4;
            // 
            // lbl_GalleryID
            // 
            this.lbl_GalleryID.AutoSize = true;
            this.lbl_GalleryID.Location = new System.Drawing.Point(23, 56);
            this.lbl_GalleryID.Name = "lbl_GalleryID";
            this.lbl_GalleryID.Size = new System.Drawing.Size(56, 12);
            this.lbl_GalleryID.TabIndex = 3;
            this.lbl_GalleryID.Text = "갤러리 ID";
            // 
            // lbl_GalleryType
            // 
            this.lbl_GalleryType.AutoSize = true;
            this.lbl_GalleryType.Location = new System.Drawing.Point(23, 22);
            this.lbl_GalleryType.Name = "lbl_GalleryType";
            this.lbl_GalleryType.Size = new System.Drawing.Size(69, 12);
            this.lbl_GalleryType.TabIndex = 0;
            this.lbl_GalleryType.Text = "갤러리 형식";
            // 
            // rb_MinorGallery
            // 
            this.rb_MinorGallery.AutoSize = true;
            this.rb_MinorGallery.Location = new System.Drawing.Point(236, 20);
            this.rb_MinorGallery.Name = "rb_MinorGallery";
            this.rb_MinorGallery.Size = new System.Drawing.Size(99, 16);
            this.rb_MinorGallery.TabIndex = 2;
            this.rb_MinorGallery.Text = "마이너 갤러리";
            this.rb_MinorGallery.UseVisualStyleBackColor = true;
            // 
            // rb_NormalGallery
            // 
            this.rb_NormalGallery.AutoSize = true;
            this.rb_NormalGallery.Checked = true;
            this.rb_NormalGallery.Location = new System.Drawing.Point(128, 20);
            this.rb_NormalGallery.Name = "rb_NormalGallery";
            this.rb_NormalGallery.Size = new System.Drawing.Size(87, 16);
            this.rb_NormalGallery.TabIndex = 1;
            this.rb_NormalGallery.TabStop = true;
            this.rb_NormalGallery.Text = "일반 갤러리";
            this.rb_NormalGallery.UseVisualStyleBackColor = true;
            // 
            // Frm_Cleaner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.tc_CleanerTabContainer);
            this.Controls.Add(this.ststp_Status);
            this.Name = "Frm_Cleaner";
            this.Text = "디시 클리너";
            this.Load += new System.EventHandler(this.Frm_Cleaner_Load);
            this.tlp_CleanerSeprator.ResumeLayout(false);
            this.gb_CommentGroup.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_CommentList)).EndInit();
            this.gb_ArticleGroup.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_ArticleList)).EndInit();
            this.ststp_Status.ResumeLayout(false);
            this.ststp_Status.PerformLayout();
            this.tc_CleanerTabContainer.ResumeLayout(false);
            this.tp_GallogBase.ResumeLayout(false);
            this.tp_SearchBase.ResumeLayout(false);
            this.pl_SearchBasePanel.ResumeLayout(false);
            this.pl_SearchBasePanel.PerformLayout();
            this.gb_SearchedArticleList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_SearchArticle)).EndInit();
            this.gb_SearchInfo.ResumeLayout(false);
            this.gb_SearchInfo.PerformLayout();
            this.gb_GalleryInfo.ResumeLayout(false);
            this.gb_GalleryInfo.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlp_CleanerSeprator;
        private System.Windows.Forms.GroupBox gb_ArticleGroup;
        private System.Windows.Forms.GroupBox gb_CommentGroup;
        private System.Windows.Forms.Button btn_LoadComments;
        private System.Windows.Forms.Button btn_LoadArticles;
        private System.Windows.Forms.DataGridView dgv_CommentList;
        private System.Windows.Forms.DataGridView dgv_ArticleList;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_Title;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_Name;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_Content;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_Date;
        private System.Windows.Forms.Button btn_RemoveAllComment;
        private System.Windows.Forms.Button btn_RemoveGallComment;
        private System.Windows.Forms.Button btn_RemoveGallArticle;
        private System.Windows.Forms.Button btn_RemoveAllArticle;
        private System.Windows.Forms.StatusStrip ststp_Status;
        private System.Windows.Forms.ToolStripStatusLabel lbl_Status;
        private System.Windows.Forms.TabControl tc_CleanerTabContainer;
        private System.Windows.Forms.TabPage tp_GallogBase;
        private System.Windows.Forms.TabPage tp_SearchBase;
        private System.Windows.Forms.Panel pl_SearchBasePanel;
        private System.Windows.Forms.GroupBox gb_GalleryInfo;
        private System.Windows.Forms.RadioButton rb_MinorGallery;
        private System.Windows.Forms.RadioButton rb_NormalGallery;
        private System.Windows.Forms.TextBox tb_SearchGalleryID;
        private System.Windows.Forms.Label lbl_GalleryID;
        private System.Windows.Forms.Label lbl_GalleryType;
        private System.Windows.Forms.Label lbl_SearchBaseWarn;
        private System.Windows.Forms.GroupBox gb_SearchInfo;
        private System.Windows.Forms.Label lbl_Password;
        private System.Windows.Forms.Label lbl_SearchID;
        private System.Windows.Forms.TextBox tb_DeletePassword;
        private System.Windows.Forms.TextBox tb_SearchNickName;
        private System.Windows.Forms.DataGridView dgv_SearchArticle;
        private System.Windows.Forms.Button btn_DeleteSearchedArticle;
        private System.Windows.Forms.GroupBox gb_SearchedArticleList;
        private System.Windows.Forms.Button btn_SearchArticle;
        private System.Windows.Forms.Button btn_AbortSearch;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvCol_Title;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvCol_Date;
    }
}