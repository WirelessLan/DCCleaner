using System;
using System.Windows.Forms;
using System.Threading;
using DCAdapter;

namespace DCCleaner
{
    public partial class Frm_Login : Form
    {
        DCConnector connector;
        Thread loginThread;

        public Frm_Login()
        {
            InitializeComponent();
            lbl_Error.Text = "";

            connector = new DCConnector();
        }

        private void btn_Login_Click(object sender, EventArgs e)
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

            if(loginThread != null && loginThread.IsAlive)
            {
                return;
            }

            string id = tb_ID.Text.Trim();
            string pw = tb_PW.Text.Trim();
            
            loginThread = new Thread(new ThreadStart(delegate ()
            {
                bool result = false;

                try
                {
                    result = connector.LoginDCInside(id, pw);
                }
                catch(ThreadAbortException) { throw; }
                catch
                {
                    this.Invoke(new Action(() =>
                    {
                        this.lbl_Error.Text = "서버 오류로 로그인에 실패하였습니다.";
                    }));

                    return;
                }

                this.Invoke(new Action(() =>
                {
                    if (result)
                    {
                        Frm_Cleaner cleaner = new Frm_Cleaner(this.connector);
                        cleaner.FormClosed += (s, argv) => this.Close();
                        this.Hide();
                        cleaner.Show();
                    }
                    else
                    {
                        lbl_Error.Text = connector.LoginErrorMessage;
                    }
                }));
            }));

            this.lbl_Error.Text = "로그인중입니다.";

            loginThread.Start();
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

        private void ExitForm()
        {
            if (this.loginThread != null && this.loginThread.IsAlive)
            {
                this.loginThread.Abort();
            }
            else
            {
                this.loginThread = null;
            }
        }

        private void Frm_Login_FormClosing(object sender, FormClosingEventArgs e)
        {
            ExitForm();
        }
    }
}
