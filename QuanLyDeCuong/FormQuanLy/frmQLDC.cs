using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace QuanLyDeCuong.FormQuanLy
{
    public partial class frmQLDC : Form
    {
        private string saveFolder = Path.Combine(Application.StartupPath, "SyllabusData");
        private string currentSourceFilePath = ""; // Lưu đường dẫn file tạm khi chọn

        public frmQLDC()
        {
            InitializeComponent();
            // Tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(saveFolder))
            {
                Directory.CreateDirectory(saveFolder);
            }
        }

        private void frmQLDC_Load(object sender, EventArgs e)
        {
            LoadComboBoxMonHoc();
            LoadDanhSachDeCuong();
            ResetForm();
        }

        private void LoadComboBoxMonHoc()
        {
            DbQLDC db = new DbQLDC();
            string query = "";

            if (UserSession.VaiTro == "Admin")
            {
                // Admin: Thấy tất cả môn
                query = "SELECT MaMonHoc, TenMonHoc FROM MonHoc";
            }
            else
            {
                // Giảng viên: Chỉ thấy môn thuộc khoa của mình
                query = $"SELECT MaMonHoc, TenMonHoc FROM MonHoc WHERE MaKhoa = '{UserSession.MaKhoa}'";
            }

            DataTable dt = db.GetDataTable(query);
            cbMonHoc.DataSource = dt;
            cbMonHoc.DisplayMember = "TenMonHoc";
            cbMonHoc.ValueMember = "MaMonHoc";
            cbMonHoc.SelectedIndex = -1;
        }

        private void LoadDanhSachDeCuong()
        {
            DbQLDC db = new DbQLDC();
            string query = @"
                SELECT dc.MaDeCuong, mh.TenMonHoc, dc.TacGia, dc.TenFile, dc.NgayCapNhat, dc.DuongDanFile, dc.MaMonHoc
                FROM DeCuong dc
                JOIN MonHoc mh ON dc.MaMonHoc = mh.MaMonHoc";
            if(UserSession.VaiTro == "Giảng viên")
            {
                query += $" WHERE dc.TacGia LIKE N'{UserSession.HoTen}'";
            }

            DataTable dt = db.GetDataTable(query);
            dgvDeCuong.DataSource = dt;

            if (dgvDeCuong.Columns["MaDeCuong"] != null) dgvDeCuong.Columns["MaDeCuong"].HeaderText = "STT";
            if (dgvDeCuong.Columns["TenMonHoc"] != null) dgvDeCuong.Columns["TenMonHoc"].HeaderText = "Tên môn học";
            if (dgvDeCuong.Columns["TacGia"] != null) dgvDeCuong.Columns["TacGia"].HeaderText = "Tác giả";
            if (dgvDeCuong.Columns["TenFile"] != null) dgvDeCuong.Columns["TenFile"].HeaderText = "Tên File";
            if (dgvDeCuong.Columns["NgayCapNhat"] != null) dgvDeCuong.Columns["NgayCapNhat"].HeaderText = "Ngày cập nhật";

            if (dgvDeCuong.Columns["DuongDanFile"] != null) dgvDeCuong.Columns["DuongDanFile"].Visible = false;
            if (dgvDeCuong.Columns["MaMonHoc"] != null) dgvDeCuong.Columns["MaMonHoc"].Visible = false;
            dgvDeCuong.RowHeadersVisible = false;
        }

        private void btnChonFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Document Files|*.pdf;*.doc;*.docx;*.xls;*.xlsx|All Files|*.*";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                currentSourceFilePath = dlg.FileName;
                txtTenFile.Text = Path.GetFileName(currentSourceFilePath);
            }
        }

        // Chức năng THÊM
        private void btnThem_Click(object sender, EventArgs e)
        {
            if (cbMonHoc.SelectedIndex == -1 || string.IsNullOrEmpty(txtTenFile.Text))
            {
                MessageBox.Show("Vui lòng chọn môn học và file đề cương!");
                return;
            }

            string fileName = Path.GetFileName(currentSourceFilePath);
            // Thêm timestamp để tránh trùng tên file: DeCuong_20231122_abc.pdf
            string uniqueFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{UserSession.TenDangNhap}{Path.GetExtension(fileName)}";
            string destPath = Path.Combine(saveFolder, uniqueFileName);

            try
            {
                if (!string.IsNullOrEmpty(currentSourceFilePath))
                {
                    File.Copy(currentSourceFilePath, destPath, true);
                }
                else
                {
                    MessageBox.Show("File không hợp lệ!"); return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi upload file: " + ex.Message);
                return;
            }

            DbQLDC db = new DbQLDC();
            string query = @"INSERT INTO DeCuong (MaMonHoc, TacGia, TenFile, DuongDanFile, NgayCapNhat) 
                             VALUES (@maMon, @tacGia, @tenFile, @duongDan, GETDATE())";

            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("@maMon", cbMonHoc.SelectedValue);
            p.Add("@tacGia", UserSession.HoTen);
            p.Add("@tenFile", uniqueFileName);
            p.Add("@duongDan", destPath);

            if (db.ExecuteNonQuery(query, p))
            {
                MessageBox.Show("Thêm đề cương thành công!");
                LoadDanhSachDeCuong();
                ResetForm();
            }
        }

        // Chức năng SỬA
        private void btnSua_Click(object sender, EventArgs e)
        {
            if (dgvDeCuong.CurrentRow == null) return;
            string maDeCuong = dgvDeCuong.CurrentRow.Cells["MaDeCuong"].Value.ToString();
            string oldFilePath = dgvDeCuong.CurrentRow.Cells["DuongDanFile"].Value.ToString();

            string finalPath = oldFilePath;
            string finalName = dgvDeCuong.CurrentRow.Cells["TenFile"].Value.ToString();

            if (!string.IsNullOrEmpty(currentSourceFilePath)) // Có chọn file mới
            {
                string fileName = Path.GetFileName(currentSourceFilePath);
                string uniqueFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{UserSession.TenDangNhap}{Path.GetExtension(fileName)}";
                string destPath = Path.Combine(saveFolder, uniqueFileName);
                try
                {
                    if (File.Exists(oldFilePath)) File.Delete(oldFilePath);
                    File.Copy(currentSourceFilePath, destPath, true);
                    finalPath = destPath;
                    finalName = uniqueFileName;
                }
                catch (Exception ex) { MessageBox.Show("Lỗi file: " + ex.Message); return; }
            }

            DbQLDC db = new DbQLDC();
            string query = @"UPDATE DeCuong 
                             SET MaMonHoc=@maMon, TenFile=@tenFile, DuongDanFile=@duongDan, NgayCapNhat=GETDATE()
                             WHERE MaDeCuong=@id";

            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("@maMon", cbMonHoc.SelectedValue);
            p.Add("@tenFile", finalName);
            p.Add("@duongDan", finalPath);
            p.Add("@id", maDeCuong);

            if (db.ExecuteNonQuery(query, p))
            {
                MessageBox.Show("Cập nhật thành công!");
                LoadDanhSachDeCuong();
                ResetForm();
            }
        }

        // Chức năng XÓA
        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (dgvDeCuong.CurrentRow == null) return;

            if (MessageBox.Show("Bạn có chắc muốn xóa đề cương này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                string maDeCuong = dgvDeCuong.CurrentRow.Cells["MaDeCuong"].Value.ToString();

                string filePath = dgvDeCuong.CurrentRow.Cells["DuongDanFile"].Value.ToString();
                if (File.Exists(filePath)) File.Delete(filePath);

                DbQLDC db = new DbQLDC();
                if (db.ExecuteNonQuery("DELETE FROM DeCuong WHERE MaDeCuong = @id", new Dictionary<string, object> { { "@id", maDeCuong } }))
                {
                    MessageBox.Show("Đã xóa!");
                    LoadDanhSachDeCuong();
                    ResetForm();
                }
            }
        }

        // Chức năng XEM FILE
        private void btnXem_Click(object sender, EventArgs e)
        {
            if (dgvDeCuong.CurrentRow == null) return;
            string path = dgvDeCuong.CurrentRow.Cells["DuongDanFile"].Value.ToString();

            if (File.Exists(path))
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
            else
            {
                MessageBox.Show("File không tồn tại trên hệ thống (Có thể đã bị xóa hoặc đường dẫn sai)!");
            }
        }

        private void dgvDeCuong_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvDeCuong.Rows[e.RowIndex];
                cbMonHoc.SelectedValue = row.Cells["MaMonHoc"].Value.ToString();
                txtTenFile.Text = row.Cells["TenFile"].Value.ToString();

                currentSourceFilePath = "";
            }
        }

        // Tìm kiếm
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string key = txtSearch.Text;
            DbQLDC db = new DbQLDC();
            string query = $@"
                SELECT dc.MaDeCuong, mh.TenMonHoc, dc.TacGia, dc.TenFile, dc.NgayCapNhat, dc.DuongDanFile, dc.MaMonHoc
                FROM DeCuong dc
                JOIN MonHoc mh ON dc.MaMonHoc = mh.MaMonHoc
                WHERE mh.TenMonHoc LIKE N'%{key}%' OR dc.TacGia LIKE N'%{key}%'";

            dgvDeCuong.DataSource = db.GetDataTable(query);
        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            ResetForm();
            LoadDanhSachDeCuong();
        }

        private void ResetForm()
        {
            txtTenFile.Text = "";
            cbMonHoc.SelectedIndex = -1;
            currentSourceFilePath = "";
            txtSearch.Text = "";
        }
    }
}
