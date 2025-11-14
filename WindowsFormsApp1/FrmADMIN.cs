using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace WindowsFormsApp1
{
    public partial class FrmADMIN : Form
    {
        SqlConnection conn = new SqlConnection(@"Data Source=NGOCHONG\SQLEXPRESS;Initial Catalog=QL_Cafe;Integrated Security=True;TrustServerCertificate=True");
        public FrmADMIN()
        {
            InitializeComponent();
        }
        private void MakeButtonRound(Button btn)
        {
            GraphicsPath gp = new GraphicsPath();
            gp.AddEllipse(0, 0, btn.Width - 1, btn.Height - 1);
            btn.Region = new Region(gp);
        }

        private void btnDangNhap_Click(object sender, EventArgs e)
        {
            string user = txtbTaiKhoan.Text.Trim();
            string pass = txtbMatKhau.Text.Trim();

            SqlCommand cmd = new SqlCommand(
                "SELECT MaNV, HoTen FROM NhanVien WHERE Username=@u AND Password=@p", conn);

            cmd.Parameters.AddWithValue("@u", user);
            cmd.Parameters.AddWithValue("@p", pass);

            conn.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                int maNV = dr.GetInt32(0);
                string tenNV = dr.GetString(1);

                // ✅ Lưu thông tin nhân viên đăng nhập vào biến toàn cục
                Program.MaNV_DangNhap = maNV;
                Program.TenNV_DangNhap = tenNV;

                conn.Close();

                Form menu = new Form1();
                menu.Show();
                this.Hide();
            }
            else if (txtbTaiKhoan.Text == "admin" && txtbMatKhau.Text == "123")
            {
                Form admin = new FrmNhanVien();
                admin.Show();
                this.Hide();
            }
            else
            {
                conn.Close();
                MessageBox.Show("Sai tài khoản hoặc mật khẩu");
            }
        }
    }
}
