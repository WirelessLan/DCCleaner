using System;
using System.Windows.Forms;
using System.Threading;
using DCAdapter;

namespace DCCleaner
{
    public partial class Frm_Login : Form
    {
        DCConnector connector;

        public Frm_Login()
        {
            InitializeComponent();
            lbl_Error.Text = "";

            connector = new DCConnector();
        }

        private async void btn_Login_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tb_ID.Text))
            {
                tb_ID.Focus();
                this.lbl_Error.Text = "ID를 입력해주세요.";
                return;
            }
            if (string.IsNullOrWhiteSpace(tb_PW.Text))
            {
                tb_PW.Focus();
                this.lbl_Error.Text = "비밀번호를 입력해주세요.";
                return;
            }

            string id = tb_ID.Text.Trim();
            string pw = tb_PW.Text.Trim();

            btn_Login.Enabled = false;
            btn_NoAccn.Enabled = false;
            tb_ID.Enabled = false;
            tb_PW.Enabled = false;

            bool result = false;
            this.lbl_Error.Text = "로그인중입니다.";

            try
            {
                result = await connector.Login(id, pw);
            }
            catch
            {
                btn_Login.Enabled = true;
                btn_NoAccn.Enabled = true;
                tb_ID.Enabled = true;
                tb_PW.Enabled = true;
                this.lbl_Error.Text = "서버 오류로 로그인에 실패하였습니다.";

                return;
            }

            if (result)
            {
                Frm_Cleaner cleaner = new Frm_Cleaner(this.connector);
                cleaner.FormClosed += (s, argv) => this.Close();
                this.Hide();
                cleaner.Show();
            }
            else
            {
                btn_Login.Enabled = true;
                btn_NoAccn.Enabled = true;
                tb_ID.Enabled = true;
                tb_PW.Enabled = true;
                lbl_Error.Text = connector.LoginInfo.ErrorMessage;
            }
        }

        private void btn_NoAccn_Click(object sender, EventArgs e)
        {
            Frm_Cleaner cleaner = new Frm_Cleaner(this.connector);
            cleaner.FormClosed += (s, argv) => this.Close();
            this.Hide();
            cleaner.Show();
        }

        private void tb_PW_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == 13)
            {
                btn_Login.PerformClick();
            }
        }

        private void tb_ID_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                btn_Login.PerformClick();
            }
        }
    }
}
