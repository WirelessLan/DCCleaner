namespace DCCleaner
{
    partial class Frm_Login
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.tb_ID = new System.Windows.Forms.TextBox();
            this.tb_PW = new System.Windows.Forms.TextBox();
            this.lbl_ID = new System.Windows.Forms.Label();
            this.lbl_PW = new System.Windows.Forms.Label();
            this.btn_Login = new System.Windows.Forms.Button();
            this.lbl_Error = new System.Windows.Forms.Label();
            this.gb_Login = new System.Windows.Forms.GroupBox();
            this.gb_Login.SuspendLayout();
            this.SuspendLayout();
            // 
            // tb_ID
            // 
            this.tb_ID.Location = new System.Drawing.Point(71, 22);
            this.tb_ID.Name = "tb_ID";
            this.tb_ID.Size = new System.Drawing.Size(179, 21);
            this.tb_ID.TabIndex = 0;
            this.tb_ID.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tb_ID_KeyPress);
            // 
            // tb_PW
            // 
            this.tb_PW.Location = new System.Drawing.Point(71, 54);
            this.tb_PW.Name = "tb_PW";
            this.tb_PW.PasswordChar = '*';
            this.tb_PW.Size = new System.Drawing.Size(179, 21);
            this.tb_PW.TabIndex = 1;
            this.tb_PW.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tb_PW_KeyPress);
            // 
            // lbl_ID
            // 
            this.lbl_ID.AutoSize = true;
            this.lbl_ID.Location = new System.Drawing.Point(12, 27);
            this.lbl_ID.Name = "lbl_ID";
            this.lbl_ID.Size = new System.Drawing.Size(16, 12);
            this.lbl_ID.TabIndex = 2;
            this.lbl_ID.Text = "ID";
            // 
            // lbl_PW
            // 
            this.lbl_PW.AutoSize = true;
            this.lbl_PW.Location = new System.Drawing.Point(12, 59);
            this.lbl_PW.Name = "lbl_PW";
            this.lbl_PW.Size = new System.Drawing.Size(53, 12);
            this.lbl_PW.TabIndex = 3;
            this.lbl_PW.Text = "비밀번호";
            // 
            // btn_Login
            // 
            this.btn_Login.Location = new System.Drawing.Point(210, 138);
            this.btn_Login.Name = "btn_Login";
            this.btn_Login.Size = new System.Drawing.Size(75, 23);
            this.btn_Login.TabIndex = 3;
            this.btn_Login.Text = "로그인";
            this.btn_Login.UseVisualStyleBackColor = true;
            this.btn_Login.Click += new System.EventHandler(this.btn_Login_Click);
            // 
            // lbl_Error
            // 
            this.lbl_Error.AutoSize = true;
            this.lbl_Error.Location = new System.Drawing.Point(26, 116);
            this.lbl_Error.Name = "lbl_Error";
            this.lbl_Error.Size = new System.Drawing.Size(65, 12);
            this.lbl_Error.TabIndex = 2;
            this.lbl_Error.Text = "에러메시지";
            // 
            // gb_Login
            // 
            this.gb_Login.Controls.Add(this.lbl_ID);
            this.gb_Login.Controls.Add(this.tb_PW);
            this.gb_Login.Controls.Add(this.tb_ID);
            this.gb_Login.Controls.Add(this.lbl_PW);
            this.gb_Login.Location = new System.Drawing.Point(23, 12);
            this.gb_Login.Name = "gb_Login";
            this.gb_Login.Size = new System.Drawing.Size(262, 91);
            this.gb_Login.TabIndex = 0;
            this.gb_Login.TabStop = false;
            this.gb_Login.Text = "로그인";
            // 
            // Frm_Login
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(303, 173);
            this.Controls.Add(this.gb_Login);
            this.Controls.Add(this.lbl_Error);
            this.Controls.Add(this.btn_Login);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Frm_Login";
            this.Text = "디시 클리너";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Frm_Login_FormClosing);
            this.gb_Login.ResumeLayout(false);
            this.gb_Login.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tb_ID;
        private System.Windows.Forms.TextBox tb_PW;
        private System.Windows.Forms.Label lbl_ID;
        private System.Windows.Forms.Label lbl_PW;
        private System.Windows.Forms.Button btn_Login;
        private System.Windows.Forms.Label lbl_Error;
        private System.Windows.Forms.GroupBox gb_Login;
    }
}

