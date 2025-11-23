using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace QuanLyDeCuong.FormThongTin
{
    public partial class frmThongTin : Form
    {
        // Thư mục lưu Avatar
        private string avatarFolder = @"C:\Syllabus\Avatars\";
        private string currentAvatarPath = "";

        public frmThongTin()
        {
            InitializeComponent();
            this.Text = $"Thông tin {UserSession.VaiTro}";
            // Tạo thư mục lưu ảnh nếu chưa có
            if (!Directory.Exists(avatarFolder))
            {
                Directory.CreateDirectory(avatarFolder);
            }
        }

        private void frmThongTin_Load(object sender, EventArgs e)
        {
            LoadComboBoxGioiTinh();
            LoadThongTin();
        }

        private void LoadComboBoxGioiTinh()
        {
            cbGioiTinh.Items.Clear();
            cbGioiTinh.Items.Add("Nam");
            cbGioiTinh.Items.Add("Nữ");
            cbGioiTinh.Items.Add("Khác");
            cbGioiTinh.SelectedIndex = 0;
        }

        private void LoadThongTin()
        {
            txtTenDangNhap.Text = UserSession.TenDangNhap;

            DbQLDC db = new DbQLDC();
            string query = "SELECT * FROM NguoiDung WHERE TenDangNhap = @user";

            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("@user", UserSession.TenDangNhap);
            DataTable dt = db.GetDataTable(query, p);
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                txtHoTen.Text = row["HoTen"].ToString();
                txtEmail.Text = row["Email"].ToString();
                txtSoDT.Text = row["SoDT"].ToString();
                txtDiaChi.Text = row["DiaChi"].ToString();
                cbGioiTinh.SelectedItem = row["GioiTinh"].ToString();

                if (row["NgaySinh"] != DBNull.Value)
                {
                    dtpNgaySinh.Value = Convert.ToDateTime(row["NgaySinh"]);
                }

                string gioitinh = row["GioiTinh"].ToString();
                if (!string.IsNullOrEmpty(gioitinh))
                {
                    cbGioiTinh.Text = gioitinh;
                }
                else
                {
                    cbGioiTinh.SelectedIndex = -1;
                }

                // Load Avatar
                string avatarDbPath = row["Avatar"].ToString();
                if (!string.IsNullOrEmpty(avatarDbPath) && File.Exists(avatarDbPath))
                {
                    pbAvatar.Image = Image.FromFile(avatarDbPath);
                    currentAvatarPath = avatarDbPath;
                }
            }
        }

        private void btnChonAnh_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Hiển thị
                    pbAvatar.Image = Image.FromFile(dlg.FileName);

                    // Lưu đường dẫn tạm
                    pbAvatar.Tag = dlg.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể đọc file ảnh: " + ex.Message);
                }
            }
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtHoTen.Text))
            {
                MessageBox.Show("Họ tên không được để trống!");
                return;
            }

            // Xử lý Avatar: Nếu người dùng có chọn ảnh mới (Tag chứa đường dẫn file gốc)
            string finalAvatarPath = currentAvatarPath;

            if (pbAvatar.Tag != null)
            {
                string sourceFile = pbAvatar.Tag.ToString();
                string fileName = Path.GetFileName(sourceFile);
                string destFile = Path.Combine(avatarFolder, UserSession.TenDangNhap + "_" + fileName);

                try
                {
                    // Giải phóng ảnh cũ khỏi PictureBox để tránh lỗi file đang được sử dụng nếu đè file
                    if (pbAvatar.Image != null) pbAvatar.Image.Dispose();

                    File.Copy(sourceFile, destFile, true);
                    finalAvatarPath = destFile;

                    // Load lại ảnh vào PictureBox từ đường dẫn mới
                    pbAvatar.Image = Image.FromFile(finalAvatarPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi lưu ảnh: " + ex.Message);
                    return;
                }
            }

            DbQLDC db = new DbQLDC();
            string query = @"UPDATE NguoiDung 
                             SET HoTen=@ten, Email=@email, SoDT=@sodt, DiaChi=@diachi, 
                                 NgaySinh=@ns, GioiTinh=@gt, Avatar=@avatar
                             WHERE TenDangNhap=@user";

            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("@user", UserSession.TenDangNhap);
            p.Add("@ten", txtHoTen.Text.Trim());
            p.Add("@email", txtEmail.Text.Trim());
            p.Add("@sodt", txtSoDT.Text.Trim());
            p.Add("@diachi", txtDiaChi.Text.Trim());
            p.Add("@ns", dtpNgaySinh.Value);
            p.Add("@gt", cbGioiTinh.Text);
            p.Add("@avatar", finalAvatarPath);

            if (db.ExecuteNonQuery(query, p))
            {
                MessageBox.Show("Cập nhật thông tin thành công!");
                UserSession.HoTen = txtHoTen.Text;

                pbAvatar.Tag = null;
                currentAvatarPath = finalAvatarPath;
            }
        }

        private void btnDoiMatKhau_Click(object sender, EventArgs e)
        {
            frmDoiMatKhau frmDoiMatKhau = new frmDoiMatKhau();
            frmDoiMatKhau.ShowDialog();
        }
    }
}
