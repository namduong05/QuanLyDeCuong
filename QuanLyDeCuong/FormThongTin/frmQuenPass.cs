using QuanLyDeCuong;
using QuanLyDeCuong.FormThongTin;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Windows.Forms;

namespace QuanLyDeCuong
{
    public partial class frmQuenPass : Form
    {
        int countdown = 60;
        public frmQuenPass()
        {
            InitializeComponent();
        }

        private string GenerateOTP()
        {
            Random rand = new Random();
            return rand.Next(100000, 999999).ToString(); // Mã 6 số
        }
        private void btnSendOTP_Click_1(object sender, EventArgs e)
        {
            if(txtEmail.Text == "")
            {
                MessageBox.Show("Vui lòng nhập email của bạn");
                return;
            }
            DbQLDC db = new DbQLDC();

            string query = "SELECT COUNT(*) FROM NguoiDung WHERE Email = @email";
            Dictionary<string, object> p = new Dictionary<string, object> {
                { "@email", txtEmail.Text }
            };

            int count = Convert.ToInt32(db.ExecuteScalar(query, p));
            if (count == 0)
            {
                MessageBox.Show("Không người dùng nào có email trên!");
                return;
            }

            string queryUser = "SELECT TenDangNhap FROM NguoiDung WHERE Email = @email";
            UserSession.TenDangNhap = db.ExecuteScalar(queryUser, new Dictionary<string, object> { { "@email", txtEmail.Text } }).ToString();

            string queryName = "SELECT HoTen FROM NguoiDung WHERE Email = @email";
            UserSession.HoTen = db.ExecuteScalar(queryName, new Dictionary<string, object> { { "@email", txtEmail.Text } }).ToString();

            string queryRole = "SELECT VaiTro FROM NguoiDung WHERE Email = @email";
            UserSession.VaiTro = db.ExecuteScalar(queryRole, new Dictionary<string, object> { { "@email", txtEmail.Text } }).ToString();

            string queryKhoa = "SELECT MaKhoa FROM NguoiDung WHERE Email = @email";
            object khoa = db.ExecuteScalar(queryKhoa, new Dictionary<string, object> { { "@email", txtEmail.Text } });
            UserSession.MaKhoa = khoa != null ? khoa.ToString() : "";

            // Khóa nút
            btnSendOTP.Enabled = false;

            // Hiển thị số giây
            btnSendOTP.Text = $"Gửi lại ({countdown}s)";

            // Bắt đầu đếm
            timer1.Start();
            try
            {
                string otp = GenerateOTP();
                string queryMail = @"UPDATE NguoiDung 
                            SET OTP = @otp 
                            WHERE Email=@email";

                Dictionary<string, object> pMail = new Dictionary<string, object>();
                pMail.Add("@otp", otp);
                pMail.Add("@email", txtEmail.Text);
                bool check = db.ExecuteNonQuery(queryMail, pMail);
                
                var fromAddress = new MailAddress("chiutoe123@gmail.com");
                var toAddress = new MailAddress(txtEmail.Text);
                const string frompass = "ckfh npyc vlzz xvwz";
                const string subject = "OTP code";
                string body = $"Mã OTP của bạn là: {otp}";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, frompass),
                    Timeout = 200000
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    smtp.Send(message);
                }
                MessageBox.Show("OTP đã được gửi qua mail!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "Hihi"); ;
            }

        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            DbQLDC db = new DbQLDC();
            string queryOTP = "SELECT OTP FROM NguoiDung WHERE Email = @email";
            string otp = db.ExecuteScalar(queryOTP, new Dictionary<string, object> { { "@email", txtEmail.Text } }).ToString();
            if (otp == txtOTP.Text)
            {
                MessageBox.Show("Xác nhận mail thành công");
                frmDoiMatKhau frmDoiMatKhau = new frmDoiMatKhau();
                this.Hide(); ;
                frmDoiMatKhau.ShowDialog();
                this.Close();
            }
            else
            {
                MessageBox.Show("Nhập sai OTP!");
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            countdown--;

            if (countdown <= 0)
            {
                timer1.Stop();
                btnSendOTP.Enabled = true;
                btnSendOTP.Text = "Gửi mã";
            }
            else
            {
                btnSendOTP.Text = $"Gửi lại ({countdown}s)";
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            frmLogin frmLogin = new frmLogin(); 
            this.Hide();
            frmLogin.ShowDialog();
            this.Close();
        }
    }
}