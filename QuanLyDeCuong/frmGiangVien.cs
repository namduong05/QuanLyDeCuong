using System;
using System.Windows.Forms;
using QuanLyDeCuong.FormQuanLy;
using QuanLyDeCuong.FormThongTin;

namespace QuanLyDeCuong
{
    public partial class frmGiangVien : Form
    {
        public frmGiangVien()
        {
            InitializeComponent();
            this.Text = $"Giảng viên {UserSession.HoTen}";
        }
        private Form currentFormChild;
        private void OpenChildForm(Form childForm)
        {
            if (currentFormChild != null)
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

        private void btnQLDC_Click(object sender, EventArgs e)
        {
            OpenChildForm(new frmQLDC());
        }

        private void btnTTCN_Click(object sender, EventArgs e)
        {
            OpenChildForm(new frmThongTin());
        }

        private void btnDangXuat_Click(object sender, EventArgs e)
        {
            this.Hide();
            frmLogin frmLogin = new frmLogin();
            frmLogin.ShowDialog();
            this.Close();
        }

        private void frmGiangVien_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}
