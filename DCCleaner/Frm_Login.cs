using System;
using System.Windows.Forms;
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

        private void btn_Login_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tb_ID.Text) || string.IsNullOrWhiteSpace(tb_PW.Text))
            {
                MessageBox.Show("ID 또는 비밀번호를 입력해주세요.", "입력 오류");
                return;
            }

            if(connector.LoginDCInside(tb_ID.Text.Trim(), tb_PW.Text.Trim()))
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
