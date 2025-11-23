using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace QuanLyDeCuong.FormQuanLy
{
    public partial class frmQLND : Form
    {
        public frmQLND()
        {
            InitializeComponent();
        }

        private void frmQLND_Load(object sender, EventArgs e)
        {
            LoadComboBoxKhoa();
            LoadComboBoxVaiTro();
            LoadDanhSachNguoiDung();
            ResetForm();
        }

        private void LoadComboBoxKhoa()
        {
            DbQLDC db = new DbQLDC();
            string query = "SELECT MaKhoa, TenKhoa FROM Khoa";
            DataTable dt = db.GetDataTable(query);

            DataRow row = dt.NewRow();
            row["MaKhoa"] = "";
            row["TenKhoa"] = "-- Chọn Khoa --";
            dt.Rows.InsertAt(row, 0);

            cbKhoa.DataSource = dt;
            cbKhoa.DisplayMember = "TenKhoa";
            cbKhoa.ValueMember = "MaKhoa";
            cbKhoa.SelectedIndex = 0;
        }

        private void LoadComboBoxVaiTro()
        {
            cbVaiTro.Items.Clear();
            cbVaiTro.Items.Add("Giảng viên");
            cbVaiTro.Items.Add("Sinh viên");
            cbVaiTro.SelectedIndex = 0;
        }

        private void LoadDanhSachNguoiDung(string role = null)
        {
            DbQLDC db = new DbQLDC();
            string query = $@"
                SELECT nd.TenDangNhap, nd.HoTen, nd.VaiTro, k.TenKhoa, nd.Email, nd.MaKhoa
                FROM NguoiDung nd
                LEFT JOIN Khoa k ON nd.MaKhoa = k.MaKhoa
                WHERE nd.VaiTro != 'Admin'";
            if(role != null)
            {
                query += $" AND nd.VaiTro = N'{role}'";
            }
            DataTable dt = db.GetDataTable(query);
            dgvNguoiDung.DataSource = dt;

            if (dgvNguoiDung.Columns["MaKhoa"] != null) dgvNguoiDung.Columns["MaKhoa"].Visible = false;

            if (dgvNguoiDung.Columns["TenDangNhap"] != null) dgvNguoiDung.Columns["TenDangNhap"].HeaderText = "Tài khoản";
            if (dgvNguoiDung.Columns["HoTen"] != null) dgvNguoiDung.Columns["HoTen"].HeaderText = "Họ tên";
            if (dgvNguoiDung.Columns["VaiTro"] != null) dgvNguoiDung.Columns["VaiTro"].HeaderText = "Vai trò";
            if (dgvNguoiDung.Columns["TenKhoa"] != null) dgvNguoiDung.Columns["TenKhoa"].HeaderText = "Tên khoa";
            dgvNguoiDung.RowHeadersVisible = false;
        }

        // Chức năng THÊM
        private void btnThem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtTenDangNhap.Text))
            {
                MessageBox.Show("Vui lòng nhập Tài khoản!");
                return;
            }

            DbQLDC db = new DbQLDC();
            string checkQuery = "SELECT COUNT(*) FROM NguoiDung WHERE TenDangNhap = @user";
            int count = Convert.ToInt32(db.ExecuteScalar(checkQuery, new Dictionary<string, object> { { "@user", txtTenDangNhap.Text } }));

            if (count > 0)
            {
                MessageBox.Show("Tên đăng nhập đã tồn tại!");
                return;
            }

            string query = @"INSERT INTO NguoiDung (TenDangNhap, MatKhau, HoTen, VaiTro, MaKhoa, Email) 
                             VALUES (@user, @pass, @ten, @vaitro, @khoa, @email)";

            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("@user", txtTenDangNhap.Text.Trim());
            p.Add("@pass", "admin");
            p.Add("@ten", txtHoTen.Text.Trim());
            p.Add("@vaitro", cbVaiTro.SelectedItem.ToString());

            string selectedKhoa = cbKhoa.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(selectedKhoa))
                p.Add("@khoa", DBNull.Value);
            else
                p.Add("@khoa", selectedKhoa);

            p.Add("@email", txtEmail.Text.Trim());

            if (db.ExecuteNonQuery(query, p))
            {
                MessageBox.Show("Thêm người dùng thành công!");
                LoadDanhSachNguoiDung();
                ResetForm();
            }
        }

        // Chức năng SỬA
        private void btnSua_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtTenDangNhap.Text)) return;

            DbQLDC db = new DbQLDC();
            string query = @"UPDATE NguoiDung 
                            SET HoTen=@ten, VaiTro=@vaitro, MaKhoa=@khoa, Email=@email 
                            WHERE TenDangNhap=@user";

            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("@user", txtTenDangNhap.Text);
            p.Add("@ten", txtHoTen.Text);
            p.Add("@vaitro", cbVaiTro.SelectedItem.ToString());

            string selectedKhoa = cbKhoa.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(selectedKhoa))
                p.Add("@khoa", DBNull.Value);
            else
                p.Add("@khoa", selectedKhoa);

            p.Add("@email", txtEmail.Text);

            if (db.ExecuteNonQuery(query, p))
            {
                MessageBox.Show("Cập nhật thành công!");
                LoadDanhSachNguoiDung();
                ResetForm();
            }
        }

        // Chức năng XÓA
        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtTenDangNhap.Text)) return;

            if (MessageBox.Show($"Bạn có chắc muốn xóa người dùng {txtTenDangNhap.Text}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                DbQLDC db = new DbQLDC();
                string query1 = "SELECT * FROM DeCuong WHERE TacGia = @name";
                if (db.ExecuteNonQuery(query1, new Dictionary<string, object> { { "@name", txtHoTen.Text } }))
                {
                    MessageBox.Show("Không thể xóa vì còn đề cương của " + txtHoTen.Text + "!");
                }
                string query = "DELETE FROM NguoiDung WHERE TenDangNhap = @user";

                if (db.ExecuteNonQuery(query, new Dictionary<string, object> { { "@user", txtTenDangNhap.Text } }))
                {
                    MessageBox.Show("Đã xóa!");
                    LoadDanhSachNguoiDung();
                    ResetForm();
                }
            }
        }

        private void dgvNguoiDung_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvNguoiDung.Rows[e.RowIndex];

                txtTenDangNhap.Text = row.Cells["TenDangNhap"].Value.ToString();
                txtTenDangNhap.ReadOnly = true;

                txtHoTen.Text = row.Cells["HoTen"].Value.ToString();
                txtEmail.Text = row.Cells["Email"].Value.ToString();

                cbVaiTro.SelectedItem = row.Cells["VaiTro"].Value.ToString();

                string maKhoa = row.Cells["MaKhoa"].Value.ToString();
                if (!string.IsNullOrEmpty(maKhoa))
                    cbKhoa.SelectedValue = maKhoa;
                else
                    cbKhoa.SelectedIndex = 0;
            }
        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            ResetForm();
        }

        private void ResetForm()
        {
            txtTenDangNhap.Text = "";
            txtTenDangNhap.ReadOnly = false;
            txtHoTen.Text = "";
            txtEmail.Text = "";
            cbVaiTro.SelectedIndex = 0;
            cbKhoa.SelectedIndex = 0;
        }

        // Tìm kiếm
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text;
            DbQLDC db = new DbQLDC();
            string query = $@"
                SELECT nd.TenDangNhap, nd.HoTen, nd.VaiTro, k.TenKhoa, nd.Email, nd.MaKhoa
                FROM NguoiDung nd
                LEFT JOIN Khoa k ON nd.MaKhoa = k.MaKhoa
                WHERE (nd.HoTen LIKE N'%{keyword}%' OR nd.TenDangNhap LIKE '%{keyword}%') AND nd.VaiTro != 'Admin' " ;

            dgvNguoiDung.DataSource = db.GetDataTable(query);
        }

        private void btnDSSV_Click(object sender, EventArgs e)
        {
            LoadDanhSachNguoiDung("Sinh viên");
            XuatBaoCao xuatBaoCao = new XuatBaoCao();
            xuatBaoCao.BaoCao(dgvNguoiDung);
        }

        private void btnDSGV_Click(object sender, EventArgs e)
        {
            LoadDanhSachNguoiDung("Giảng viên");
            XuatBaoCao xuatBaoCao = new XuatBaoCao();
            xuatBaoCao.BaoCao(dgvNguoiDung);
        }
    }
}
