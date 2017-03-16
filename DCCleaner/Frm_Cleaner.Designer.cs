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
            this.btn_LoadComments = new System.Windows.Forms.Button();
            this.gb_ArticleGroup = new System.Windows.Forms.GroupBox();
            this.btn_LoadArticles = new System.Windows.Forms.Button();
            this.dgv_ArticleList = new System.Windows.Forms.DataGridView();
            this.dgv_CommentList = new System.Windows.Forms.DataGridView();
            this.col_Title = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_Name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_Content = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_Date = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btn_RemoveAllArticle = new System.Windows.Forms.Button();
            this.btn_RemoveGallArticle = new System.Windows.Forms.Button();
            this.btn_RemoveGallComment = new System.Windows.Forms.Button();
            this.btn_RemoveAllComent = new System.Windows.Forms.Button();
            this.tlp_CleanerSeprator.SuspendLayout();
            this.gb_CommentGroup.SuspendLayout();
            this.gb_ArticleGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_ArticleList)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_CommentList)).BeginInit();
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
            this.tlp_CleanerSeprator.Location = new System.Drawing.Point(12, 12);
            this.tlp_CleanerSeprator.Name = "tlp_CleanerSeprator";
            this.tlp_CleanerSeprator.RowCount = 1;
            this.tlp_CleanerSeprator.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlp_CleanerSeprator.Size = new System.Drawing.Size(628, 400);
            this.tlp_CleanerSeprator.TabIndex = 0;
            // 
            // gb_CommentGroup
            // 
            this.gb_CommentGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gb_CommentGroup.Controls.Add(this.btn_RemoveAllComent);
            this.gb_CommentGroup.Controls.Add(this.btn_RemoveGallComment);
            this.gb_CommentGroup.Controls.Add(this.dgv_CommentList);
            this.gb_CommentGroup.Controls.Add(this.btn_LoadComments);
            this.gb_CommentGroup.Location = new System.Drawing.Point(317, 3);
            this.gb_CommentGroup.Name = "gb_CommentGroup";
            this.gb_CommentGroup.Size = new System.Drawing.Size(308, 394);
            this.gb_CommentGroup.TabIndex = 1;
            this.gb_CommentGroup.TabStop = false;
            this.gb_CommentGroup.Text = "내가 쓴 리플";
            // 
            // btn_LoadComments
            // 
            this.btn_LoadComments.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_LoadComments.Location = new System.Drawing.Point(6, 367);
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
            this.gb_ArticleGroup.Size = new System.Drawing.Size(308, 394);
            this.gb_ArticleGroup.TabIndex = 0;
            this.gb_ArticleGroup.TabStop = false;
            this.gb_ArticleGroup.Text = "내가 쓴 글";
            // 
            // btn_LoadArticles
            // 
            this.btn_LoadArticles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_LoadArticles.Location = new System.Drawing.Point(7, 367);
            this.btn_LoadArticles.Name = "btn_LoadArticles";
            this.btn_LoadArticles.Size = new System.Drawing.Size(75, 23);
            this.btn_LoadArticles.TabIndex = 1;
            this.btn_LoadArticles.Text = "불러오기";
            this.btn_LoadArticles.UseVisualStyleBackColor = true;
            this.btn_LoadArticles.Click += new System.EventHandler(this.btn_LoadArticles_Click);
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
            this.dgv_ArticleList.Size = new System.Drawing.Size(295, 340);
            this.dgv_ArticleList.TabIndex = 2;
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
            this.dgv_CommentList.Size = new System.Drawing.Size(296, 341);
            this.dgv_CommentList.TabIndex = 3;
            // 
            // col_Title
            // 
            this.col_Title.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.col_Title.HeaderText = "제목";
            this.col_Title.Name = "col_Title";
            this.col_Title.ReadOnly = true;
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
            // btn_RemoveAllArticle
            // 
            this.btn_RemoveAllArticle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_RemoveAllArticle.Location = new System.Drawing.Point(227, 367);
            this.btn_RemoveAllArticle.Name = "btn_RemoveAllArticle";
            this.btn_RemoveAllArticle.Size = new System.Drawing.Size(75, 23);
            this.btn_RemoveAllArticle.TabIndex = 3;
            this.btn_RemoveAllArticle.Text = "갤로그도";
            this.btn_RemoveAllArticle.UseVisualStyleBackColor = true;
            this.btn_RemoveAllArticle.Click += new System.EventHandler(this.btn_RemoveAllArticle_Click);
            // 
            // btn_RemoveGallArticle
            // 
            this.btn_RemoveGallArticle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_RemoveGallArticle.Location = new System.Drawing.Point(146, 367);
            this.btn_RemoveGallArticle.Name = "btn_RemoveGallArticle";
            this.btn_RemoveGallArticle.Size = new System.Drawing.Size(75, 23);
            this.btn_RemoveGallArticle.TabIndex = 4;
            this.btn_RemoveGallArticle.Text = "갤만";
            this.btn_RemoveGallArticle.UseVisualStyleBackColor = true;
            this.btn_RemoveGallArticle.Click += new System.EventHandler(this.btn_RemoveGallArticle_Click);
            // 
            // btn_RemoveGallComment
            // 
            this.btn_RemoveGallComment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_RemoveGallComment.Location = new System.Drawing.Point(146, 367);
            this.btn_RemoveGallComment.Name = "btn_RemoveGallComment";
            this.btn_RemoveGallComment.Size = new System.Drawing.Size(75, 23);
            this.btn_RemoveGallComment.TabIndex = 5;
            this.btn_RemoveGallComment.Text = "갤만";
            this.btn_RemoveGallComment.UseVisualStyleBackColor = true;
            this.btn_RemoveGallComment.Click += new System.EventHandler(this.btn_RemoveGallComment_Click);
            // 
            // btn_RemoveAllComent
            // 
            this.btn_RemoveAllComent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_RemoveAllComent.Location = new System.Drawing.Point(227, 367);
            this.btn_RemoveAllComent.Name = "btn_RemoveAllComent";
            this.btn_RemoveAllComent.Size = new System.Drawing.Size(75, 23);
            this.btn_RemoveAllComent.TabIndex = 5;
            this.btn_RemoveAllComent.Text = "갤로그도";
            this.btn_RemoveAllComent.UseVisualStyleBackColor = true;
            this.btn_RemoveAllComent.Click += new System.EventHandler(this.btn_RemoveAllComent_Click);
            // 
            // Frm_Cleaner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(652, 424);
            this.Controls.Add(this.tlp_CleanerSeprator);
            this.Name = "Frm_Cleaner";
            this.Text = "디시 클리너";
            this.tlp_CleanerSeprator.ResumeLayout(false);
            this.gb_CommentGroup.ResumeLayout(false);
            this.gb_ArticleGroup.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_ArticleList)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_CommentList)).EndInit();
            this.ResumeLayout(false);

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
        private System.Windows.Forms.Button btn_RemoveAllComent;
        private System.Windows.Forms.Button btn_RemoveGallComment;
        private System.Windows.Forms.Button btn_RemoveGallArticle;
        private System.Windows.Forms.Button btn_RemoveAllArticle;
    }
}