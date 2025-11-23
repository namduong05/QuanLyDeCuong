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
        private string avatarFolder = Path.Combine(Application.StartupPath, "Avatars");
        private string currentAvatarFileName = "";

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
                string avatarDbValue = row["Avatar"].ToString();

                if (!string.IsNullOrEmpty(avatarDbValue))
                {
                    string fileName = Path.GetFileName(avatarDbValue);
                    string localPath = Path.Combine(avatarFolder, fileName);

                    // Ưu tiên load từ thư mục Avatars trong project
                    if (File.Exists(localPath))
                    {
                        LoadImageSafe(localPath);
                        currentAvatarFileName = fileName;
                    }
                    // Dự phòng cho dữ liệu cũ (đường dẫn tuyệt đối)
                    else if (File.Exists(avatarDbValue))
                    {
                        LoadImageSafe(avatarDbValue);
                    }
                    else
                    {
                        pbAvatar.Image = null;
                    }
                }
            }
        }

        // Hàm tiện ích load ảnh an toàn không khóa file
        private void LoadImageSafe(string path)
        {
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    pbAvatar.Image = Image.FromStream(fs);
                }
            }
            catch { pbAvatar.Image = null; }
        }

        // 2. Chọn ảnh đại diện
        private void btnChonAnh_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Hiển thị preview dùng stream để không khóa file gốc
                    using (FileStream fs = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read))
                    {
                        pbAvatar.Image = Image.FromStream(fs);
                    }
                    pbAvatar.Tag = dlg.FileName; // Lưu đường dẫn tạm
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể đọc file ảnh: " + ex.Message);
                }
            }
        }

        // 3. Lưu cập nhật
        private void btnLuu_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtHoTen.Text))
            {
                MessageBox.Show("Họ tên không được để trống!");
                return;
            }

            string finalFileName = currentAvatarFileName;

            // XỬ LÝ LƯU ẢNH VÀ XÓA ẢNH CŨ
            if (pbAvatar.Tag != null)
            {
                string sourceFile = pbAvatar.Tag.ToString();
                string ext = Path.GetExtension(sourceFile);
                string newFileName = $"{UserSession.TenDangNhap}_{UserSession.TenDangNhap}{ext}";
                string destPath = Path.Combine(avatarFolder, newFileName);

                try
                {
                    // 1. Giải phóng ảnh đang hiện trên PictureBox (quan trọng để xóa được nếu trùng tên)
                    if (pbAvatar.Image != null)
                    {
                        pbAvatar.Image.Dispose();
                        pbAvatar.Image = null;
                    }

                    // 2. Xóa ảnh cũ (nếu có và khác null)
                    if (!string.IsNullOrEmpty(currentAvatarFileName))
                    {
                        string oldFullPath = Path.Combine(avatarFolder, currentAvatarFileName);
                        if (File.Exists(oldFullPath))
                        {
                            try { File.Delete(oldFullPath); }
                            catch { /* Nếu không xóa được file rác thì bỏ qua, không crash app */ }
                        }
                    }

                    // 3. Copy ảnh mới
                    File.Copy(sourceFile, destPath, true);
                    finalFileName = newFileName;

                    // 4. Load lại ảnh mới vào PictureBox để hiển thị tiếp (Dùng Stream để không khóa file mới)
                    LoadImageSafe(destPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xử lý file ảnh: " + ex.Message);
                    return;
                }
            }

            // Cập nhật Database
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
            p.Add("@avatar", finalFileName); // Chỉ lưu tên file

            if (db.ExecuteNonQuery(query, p))
            {
                MessageBox.Show("Cập nhật thông tin thành công!");
                UserSession.HoTen = txtHoTen.Text;

                pbAvatar.Tag = null;
                currentAvatarFileName = finalFileName;
            }
        }

        private void btnDoiMatKhau_Click(object sender, EventArgs e)
        {
            frmDoiMatKhau frmDoiMatKhau = new frmDoiMatKhau();
            frmDoiMatKhau.ShowDialog();
        }
    }
}
