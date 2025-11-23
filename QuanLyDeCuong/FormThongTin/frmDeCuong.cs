using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;

namespace QuanLyDeCuong.FormThongTin
{
    public partial class frmDeCuong : Form
    {
        private string projectFolder = Path.Combine(Application.StartupPath, "SyllabusData");
        public frmDeCuong()
        {
            InitializeComponent();
        }
        private void frmDeCuong_Load(object sender, EventArgs e)
        {
            LoadDanhSachDeCuong();
        }

        private void LoadDanhSachDeCuong()
        {
            DbQLDC db = new DbQLDC();

            //Sinh viên chỉ xem đề cương các môn thuộc Khoa của mình
            string query = @"
                SELECT dc.MaDeCuong, mh.TenMonHoc, dc.TacGia, dc.TenFile, dc.NgayCapNhat, dc.DuongDanFile
                FROM DeCuong dc
                JOIN MonHoc mh ON dc.MaMonHoc = mh.MaMonHoc
                WHERE mh.MaKhoa = @maKhoa OR @maKhoa IS NULL OR @maKhoa = ''";

            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("@maKhoa", UserSession.MaKhoa);

            DataTable dt = db.GetDataTable(query, p);
            dgvDeCuong.DataSource = dt;

            // Ẩn cột không cần thiết
            if (dgvDeCuong.Columns["DuongDanFile"] != null) dgvDeCuong.Columns["DuongDanFile"].Visible = false;
            if (dgvDeCuong.Columns["MaDeCuong"] != null) dgvDeCuong.Columns["MaDeCuong"].Visible = false;

            // Đặt tên tiếng Việt cho cột
            if (dgvDeCuong.Columns["TenMonHoc"] != null) dgvDeCuong.Columns["TenMonHoc"].HeaderText = "Môn Học";
            if (dgvDeCuong.Columns["TacGia"] != null) dgvDeCuong.Columns["TacGia"].HeaderText = "Giảng Viên";
            if (dgvDeCuong.Columns["TenFile"] != null) dgvDeCuong.Columns["TenFile"].HeaderText = "Tên File";
            if (dgvDeCuong.Columns["NgayCapNhat"] != null) dgvDeCuong.Columns["NgayCapNhat"].HeaderText = "Ngày Cập Nhật";

            dgvDeCuong.RowHeadersVisible = false;
        }

        // Chức năng Tìm Kiếm
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim();
            DbQLDC db = new DbQLDC();

            string query = @"
                SELECT dc.MaDeCuong, mh.TenMonHoc, dc.TacGia, dc.TenFile, dc.NgayCapNhat, dc.DuongDanFile
                FROM DeCuong dc
                JOIN MonHoc mh ON dc.MaMonHoc = mh.MaMonHoc
                WHERE (mh.MaKhoa = @maKhoa OR @maKhoa = '') 
                AND (mh.TenMonHoc LIKE @key OR dc.TacGia LIKE @key)";

            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("@maKhoa", UserSession.MaKhoa ?? "");
            p.Add("@key", "%" + keyword + "%");

            dgvDeCuong.DataSource = db.GetDataTable(query, p);
        }

        // tải File
        private void btnTaiVe_Click(object sender, EventArgs e)
        {
            if (dgvDeCuong.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn đề cương cần tải!", "Thông báo");
                return;
            }

            string fileName = dgvDeCuong.CurrentRow.Cells["TenFile"].Value.ToString();

            // SỬA ĐỔI: Tìm file nguồn tương tự như nút Xem
            string localPath = Path.Combine(projectFolder, fileName);
            string dbPath = dgvDeCuong.CurrentRow.Cells["DuongDanFile"].Value.ToString();
            string sourcePath = "";

            if (File.Exists(localPath)) sourcePath = localPath;
            else if (File.Exists(dbPath)) sourcePath = dbPath;

            if (string.IsNullOrEmpty(sourcePath))
            {
                MessageBox.Show("File gốc không tồn tại trên hệ thống!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Mở hộp thoại Save File cho người dùng chọn nơi lưu
            SaveFileDialog saveDlg = new SaveFileDialog();
            saveDlg.FileName = fileName;
            saveDlg.Filter = "All Files|*.*";

            string ext = Path.GetExtension(fileName);
            if (!string.IsNullOrEmpty(ext))
            {
                saveDlg.Filter = $"{ext.ToUpper()} File|*{ext}|All Files|*.*";
            }

            if (saveDlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.Copy(sourcePath, saveDlg.FileName, true);
                    MessageBox.Show("Tải về thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    if (MessageBox.Show("Bạn có muốn mở file vừa tải không?", "Mở file", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo(saveDlg.FileName) { UseShellExecute = true });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi lưu file: " + ex.Message);
                }
            }
        }

        private void dgvDeCuong_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvDeCuong.Rows[e.RowIndex];
                txtTenFile.Text = row.Cells["TenFile"].Value.ToString();
            }
        }

    }
}
