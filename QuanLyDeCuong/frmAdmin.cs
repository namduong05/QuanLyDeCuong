using QuanLyDeCuong.FormQuanLy;
using System;
using System.Windows.Forms;

namespace QuanLyDeCuong
{
    public partial class frmAdmin : Form
    {
        public frmAdmin()
        {
            InitializeComponent();
        }

        private Form currentFormChild;
        private void OpenChildForm ( Form childForm)
        {
            if(currentFormChild != null)
            {
                currentFormChild.Close();
            }
            currentFormChild = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            panel_Body.Controls.Add(childForm);
            panel_Body.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }

        private void btnQLMH_Click(object sender, EventArgs e)
        {
            OpenChildForm(new frmQLMH());
        }
        private void btnQLDC_Click(object sender, EventArgs e)
        {
            OpenChildForm(new frmQLDC());
        }

        private void btnQLND_Click(object sender, EventArgs e)
        {
            OpenChildForm(new frmQLND());
        }

        private void btnDangXuat_Click(object sender, EventArgs e)
        {
            this.Hide();
            frmLogin frmLogin = new frmLogin();
            frmLogin.ShowDialog();
            this.Close();
        }

        private void frmAdmin_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}
