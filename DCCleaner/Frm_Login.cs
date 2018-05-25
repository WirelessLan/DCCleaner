using System;
using System.Windows.Forms;
using System.Threading;
using DCAdapter;
using System.Threading.Tasks;

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
            int loginAttemptCnt = 0;

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

            string id = tb_ID.Text.Trim().ToLower();
            string pw = tb_PW.Text.Trim();

            tb_ID.Enabled = false;
            tb_PW.Enabled = false;
            btn_Login.Enabled = false;
            btn_NoAccn.Enabled = false;

            bool result = false;
            this.lbl_Error.Text = "로그인중입니다.";

            for (loginAttemptCnt = 0; loginAttemptCnt < 5; loginAttemptCnt++)
            {
                result = false;

                try
                {
                    result = await connector.Login(id, pw);
                    break;
                }
                catch
                {
                    await Task.Delay(1000);
                }
            }

            if (loginAttemptCnt >= 5 || !result)
            {
                tb_ID.Enabled = true;
                tb_PW.Enabled = true;
                btn_Login.Enabled = true;
                btn_NoAccn.Enabled = true;
                if (connector.LoginInfo.ErrorMessage != null)
                    this.lbl_Error.Text = connector.LoginInfo.ErrorMessage;
                else
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
                tb_ID.Enabled = true;
                tb_PW.Enabled = true;
                btn_Login.Enabled = true;
                btn_NoAccn.Enabled = true;
                lbl_Error.Text = connector.LoginInfo.ErrorMessage;
                btn_Login.Focus();
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
