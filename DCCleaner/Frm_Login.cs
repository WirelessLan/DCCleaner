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
            if (string.IsNullOrWhiteSpace(tb_ID.Text) || string.IsNullOrWhiteSpace(tb_PW.Text))
            {
                MessageBox.Show("ID 또는 비밀번호를 입력해주세요.", "로그인 실패", MessageBoxButtons.OK, MessageBoxIcon.Hand);
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
                catch
                {
                    try
                    {
                        this.Invoke(new Action(() =>
                        {
                            this.lbl_Error.Text = "서버 오류로 로그인에 실패하였습니다.";
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
                    if (result)
                    {
                        Frm_Cleaner cleaner = new Frm_Cleaner(this.connector);
                        cleaner.FormClosed += (s, argv) => this.Close();
                        this.Hide();
                        cleaner.Show();
                    }
                    else
                    {
                        lbl_Error.Text = connector.ErrorMessage;
                    }
                }));
            }));

            this.lbl_Error.Text = "로그인중입니다.";

            loginThread.Start();
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
