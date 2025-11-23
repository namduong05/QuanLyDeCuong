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

        // Xem File
        private void btnXem_Click(object sender, EventArgs e)
        {
            if (dgvDeCuong.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn một đề cương để xem!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string filePath = dgvDeCuong.CurrentRow.Cells["DuongDanFile"].Value.ToString();

            if (File.Exists(filePath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể mở file: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("File không tồn tại hoặc đã bị xóa khỏi hệ thống!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
