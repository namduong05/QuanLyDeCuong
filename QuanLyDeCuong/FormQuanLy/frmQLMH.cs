using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace QuanLyDeCuong.FormQuanLy
{
    public partial class frmQLMH : Form
    {
        public frmQLMH()
        {
            InitializeComponent();
        }

        private void frmQLMH_Load(object sender, EventArgs e)
        {
            LoadComboBoxKhoa();
            LoadDanhSachMonHoc();
            ResetForm();
        }

        private void LoadComboBoxKhoa()
        {
            DbQLDC db = new DbQLDC();
            string query = "SELECT MaKhoa, TenKhoa FROM Khoa";
            DataTable dt = db.GetDataTable(query);

            cbKhoa.DataSource = dt;
            cbKhoa.DisplayMember = "TenKhoa";
            cbKhoa.ValueMember = "MaKhoa";
        }

        private void LoadDanhSachMonHoc()
        {
            DbQLDC db = new DbQLDC();
            string query = @"
                SELECT mh.MaMonHoc, mh.TenMonHoc, k.TenKhoa, mh.MaKhoa
                FROM MonHoc mh
                LEFT JOIN Khoa k ON mh.MaKhoa = k.MaKhoa";

            DataTable dt = db.GetDataTable(query);
            dgvMonHoc.DataSource = dt;

            if (dgvMonHoc.Columns["MaKhoa"] != null) dgvMonHoc.Columns["MaKhoa"].Visible = false;

            if (dgvMonHoc.Columns["MaMonHoc"] != null) dgvMonHoc.Columns["MaMonHoc"].HeaderText = "Mã Môn";
            if (dgvMonHoc.Columns["TenMonHoc"] != null) dgvMonHoc.Columns["TenMonHoc"].HeaderText = "Tên Môn Học";
            if (dgvMonHoc.Columns["TenKhoa"] != null) dgvMonHoc.Columns["TenKhoa"].HeaderText = "Khoa";
            dgvMonHoc.RowHeadersVisible = false;
        }

        // Chức năng THÊM
        private void btnThem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtMaMon.Text) || string.IsNullOrEmpty(txtTenMon.Text))
            {
                MessageBox.Show("Vui lòng nhập Mã môn và Tên môn!");
                return;
            }


            DbQLDC db = new DbQLDC();
            string checkQuery = "SELECT COUNT(*) FROM MonHoc WHERE MaMonHoc = @ma";
            int count = Convert.ToInt32(db.ExecuteScalar(checkQuery, new Dictionary<string, object> { { "@ma", txtMaMon.Text } }));
            if (count > 0)
            {
                MessageBox.Show("Mã môn học này đã tồn tại!");
                return;
            }

            string query = "INSERT INTO MonHoc (MaMonHoc, TenMonHoc, MaKhoa) VALUES (@ma, @ten, @khoa)";

            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("@ma", txtMaMon.Text.Trim());
            p.Add("@ten", txtTenMon.Text.Trim());
            p.Add("@khoa", cbKhoa.SelectedValue);

            if (db.ExecuteNonQuery(query, p))
            {
                MessageBox.Show("Thêm môn học thành công!");
                LoadDanhSachMonHoc();
                ResetForm();
            }
        }

        // Chức năng SỬA
        private void btnSua_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtMaMon.Text)) return;

            DbQLDC db = new DbQLDC();
            string query = "UPDATE MonHoc SET TenMonHoc = @ten, MaKhoa = @khoa WHERE MaMonHoc = @ma";

            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("@ma", txtMaMon.Text);
            p.Add("@ten", txtTenMon.Text);
            p.Add("@khoa", cbKhoa.SelectedValue);

            if (db.ExecuteNonQuery(query, p))
            {
                MessageBox.Show("Cập nhật thành công!");
                LoadDanhSachMonHoc();
                ResetForm();
            }
        }

        // Chức năng XÓA
        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtMaMon.Text)) return;

            if (MessageBox.Show($"Bạn có chắc muốn xóa môn {txtTenMon.Text}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                DbQLDC db = new DbQLDC();

                string checkDeCuong = "SELECT COUNT(*) FROM DeCuong WHERE MaMonHoc = @ma";
                int count = Convert.ToInt32(db.ExecuteScalar(checkDeCuong, new Dictionary<string, object> { { "@ma", txtMaMon.Text } }));

                if (count > 0)
                {
                    MessageBox.Show("Môn học này đang có đề cương lưu trữ, không thể xóa! Hãy xóa đề cương trước.");
                    return;
                }

                string query = "DELETE FROM MonHoc WHERE MaMonHoc = @ma";
                if (db.ExecuteNonQuery(query, new Dictionary<string, object> { { "@ma", txtMaMon.Text } }))
                {
                    MessageBox.Show("Đã xóa môn học!");
                    LoadDanhSachMonHoc();
                    ResetForm();
                }
            }
        }

        private void dgvMonHoc_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvMonHoc.Rows[e.RowIndex];
                txtMaMon.Text = row.Cells["MaMonHoc"].Value.ToString();
                txtMaMon.ReadOnly = true;

                txtTenMon.Text = row.Cells["TenMonHoc"].Value.ToString();
                cbKhoa.SelectedValue = row.Cells["MaKhoa"].Value.ToString();
            }
        }

        // Tìm kiếm
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string key = txtSearch.Text;
            DbQLDC db = new DbQLDC();
            string query = $@"
                SELECT mh.MaMonHoc, mh.TenMonHoc, k.TenKhoa, mh.MaKhoa
                FROM MonHoc mh
                LEFT JOIN Khoa k ON mh.MaKhoa = k.MaKhoa
                WHERE mh.TenMonHoc LIKE N'%{key}%' OR mh.MaMonHoc LIKE '%{key}%'";

            dgvMonHoc.DataSource = db.GetDataTable(query);
        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            ResetForm();
        }

        private void ResetForm()
        {
            txtMaMon.Text = "";
            txtMaMon.ReadOnly = false;
            txtTenMon.Text = "";
            cbKhoa.SelectedIndex = 0;
            txtSearch.Text = "";
        }

    }
}
