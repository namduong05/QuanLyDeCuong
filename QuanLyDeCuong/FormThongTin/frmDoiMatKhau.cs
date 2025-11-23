using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace QuanLyDeCuong.FormThongTin
{
    public partial class frmDoiMatKhau : Form
    {
        public frmDoiMatKhau()
        {
            InitializeComponent();
        }

        private void btnDoiMatKhau_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtPassCu.Text) ||
                string.IsNullOrEmpty(txtPassMoi.Text) ||
                string.IsNullOrEmpty(txtXacNhan.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtPassMoi.Text != txtXacNhan.Text)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                DbQLDC db = new DbQLDC();

                string checkQuery = "SELECT COUNT(*) FROM NguoiDung WHERE TenDangNhap = @user AND MatKhau = @passCu";

                Dictionary<string, object> pCheck = new Dictionary<string, object>();
                pCheck.Add("@user", UserSession.TenDangNhap);
                pCheck.Add("@passCu", txtPassCu.Text);

                int count = Convert.ToInt32(db.ExecuteScalar(checkQuery, pCheck));

                if (count > 0)
                {
                    string updateQuery = "UPDATE NguoiDung SET MatKhau = @passMoi WHERE TenDangNhap = @user";

                    Dictionary<string, object> pUpdate = new Dictionary<string, object>();
                    pUpdate.Add("@user", UserSession.TenDangNhap);
                    pUpdate.Add("@passMoi", txtPassMoi.Text);

                    if (db.ExecuteNonQuery(updateQuery, pUpdate))
                    {
                        MessageBox.Show("Đổi mật khẩu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        txtPassCu.Text = "";
                        txtPassMoi.Text = "";
                        txtXacNhan.Text = "";
                        Application.Restart();
                    }
                }
                else
                {
                    MessageBox.Show("Mật khẩu cũ không chính xác!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message);
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
