using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace QuanLyDeCuong
{
    public class DbQLDC
    {
        private string connectionString = $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + Path.Combine(Application.StartupPath, "Data", "DatabaseQLDC.mdf") + ";Integrated Security=True;Connect Timeout=30";

        private SqlConnection conn;

        public DbQLDC()
        {
            conn = new SqlConnection(connectionString);
        }

        // 1. Mở kết nối
        public void OpenConnection()
        {
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
        }

        // 2. Đóng kết nối
        public void CloseConnection()
        {
            if (conn.State != ConnectionState.Closed)
            {
                conn.Close();
            }
        }

        // 3. Lấy dữ liệu
        public DataTable GetDataTable(string query, Dictionary<string, object> parameters = null)
        {
            DataTable dt = new DataTable();
            try
            {
                OpenConnection();
                SqlCommand cmd = new SqlCommand(query, conn);

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đọc dữ liệu: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }
            return dt;
        }

        // 4. Thực thi lệnh INSERT, UPDATE, DELETE
        public bool ExecuteNonQuery(string query, Dictionary<string, object> parameters = null)
        {
            try
            {
                OpenConnection();
                SqlCommand cmd = new SqlCommand(query, conn);

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }

                int rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thực thi: " + ex.Message);
                return false;
            }
            finally
            {
                CloseConnection();
            }
        }

        // 5. Thực thi lệnh lấy 1 giá trị duy nhất
        public object ExecuteScalar(string query, Dictionary<string, object> parameters = null)
        {
            object result = null;
            try
            {
                OpenConnection();
                SqlCommand cmd = new SqlCommand(query, conn);
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }
                result = cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi truy vấn giá trị: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }
            return result;
        }
    }

    public static class UserSession
    {
        public static string TenDangNhap { get; set; }
        public static string HoTen { get; set; }
        public static string VaiTro { get; set; }
        public static string MaKhoa { get; set; }
    }
}