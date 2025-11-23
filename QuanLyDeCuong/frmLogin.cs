using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace QuanLyDeCuong
{
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();
            foreach (Form f in Application.OpenForms.Cast<Form>().ToList())
            {
                if (f != this)
                    f.Close();
            }
        }


        private void btnLogin_Click(object sender, EventArgs e)
        {
            string user = txtUser.Text.Trim();
            string pass = txtPass.Text.Trim();

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (CheckLogin(user, pass))
                {
                    // Đăng nhập thành công, lấy thông tin chi tiết lưu vào Session
                    LoadUserSession(user);

                    MessageBox.Show($"Đăng nhập thành công!\nXin chào {UserSession.VaiTro}: {UserSession.HoTen}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    this.Hide(); 

                    switch (UserSession.VaiTro)
                    {
                        case "Admin":
                            frmAdmin frmAdmin = new frmAdmin();
                            frmAdmin.ShowDialog();
                            break;
                        case "Giảng viên":
                            frmGiangVien frmGiangVien = new frmGiangVien();
                            frmGiangVien.ShowDialog();
                            break;
                        case "Sinh viên":
                            frmSinhVien frmSinhVien = new frmSinhVien();
                            frmSinhVien.ShowDialog();
                            break;
                    }

                    this.Close();
                }
                else
                {
                    MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối: " + ex.Message);
            }
        }

        private bool CheckLogin(string user, string pass)
        {
            DbQLDC db = new DbQLDC();
            string query = "SELECT COUNT(*) FROM NguoiDung WHERE TenDangNhap = @user AND MatKhau = @pass";
            Dictionary<string, object> p = new Dictionary<string, object> {
                { "@user", user },
                { "@pass", pass }
            };

            int count = Convert.ToInt32(db.ExecuteScalar(query, p));
            return count > 0;
        }

        private void LoadUserSession(string user)
        {
            DbQLDC db = new DbQLDC();

            string queryName = "SELECT HoTen FROM NguoiDung WHERE TenDangNhap = @user";
            UserSession.HoTen = db.ExecuteScalar(queryName, new Dictionary<string, object> { { "@user", user } }).ToString();

            string queryRole = "SELECT VaiTro FROM NguoiDung WHERE TenDangNhap = @user";
            UserSession.VaiTro = db.ExecuteScalar(queryRole, new Dictionary<string, object> { { "@user", user } }).ToString();

            string queryKhoa = "SELECT MaKhoa FROM NguoiDung WHERE TenDangNhap = @user";
            object khoa = db.ExecuteScalar(queryKhoa, new Dictionary<string, object> { { "@user", user } });
            UserSession.MaKhoa = khoa != null ? khoa.ToString() : "";

            UserSession.TenDangNhap = user;
        }

        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void label3_Click(object sender, EventArgs e)
        {
            frmQuenPass frmQuenPass = new frmQuenPass();
            this.Hide();
            frmQuenPass.ShowDialog();
            this.Close();
        }
    }
}
