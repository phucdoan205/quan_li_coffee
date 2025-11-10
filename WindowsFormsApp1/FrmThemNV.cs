using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace WindowsFormsApp1
{
    public partial class FrmThemNV : Form
    {
        SqlConnection conn = new SqlConnection(@"Data Source=DESKTOP-60ETTE4\SQLEXPRESS;Initial Catalog=QL_Cafe;Integrated Security=True;TrustServerCertificate=True");
        private FrmNhanVien formCha;
        public FrmThemNV(FrmNhanVien parent)
        {
            InitializeComponent();
            formCha = parent;
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            string ten = txtbTaiKhoan.Text.Trim();
            string sdt = txtbMatKhau.Text.Trim();
            string user = txtUser.Text.Trim();
            string pass = txtPass.Text.Trim();
            DateTime ngay = dateTimePicker1.Value;

            if (ten == "" || sdt == "")
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            string query = "INSERT INTO NhanVien (HoTen, SDT, NgayVaoLam, TinhTrang, Username, Password) VALUES(@ten, @sdt, @ngay, N'Đang làm',@user,@pass)";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@ten", ten);
            cmd.Parameters.AddWithValue("@sdt", sdt);
            cmd.Parameters.AddWithValue("@ngay", ngay);
            cmd.Parameters.AddWithValue("@user", user);
            cmd.Parameters.AddWithValue("@pass", pass);

            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            MessageBox.Show("Thêm nhân viên thành công!");

            formCha.LoadData();  // refresh list

            // Xóa để nhập tiếp
            txtbMatKhau.Clear();
            txtbMatKhau.Clear();
            txtUser.Clear();
            txtPass.Clear();
            txtbTaiKhoan.Focus();
        }

        private void txtbMatKhau_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(e.KeyChar >= '0' && e.KeyChar <= '9' || e.KeyChar == (char)8))
            {
                e.Handled = true;
            }
        }
    }
}
