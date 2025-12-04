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

            if (ten == "" || sdt == ""||user==""||pass=="")
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            // KIỂM TRA SỐ ĐIỆN THOẠI PHẢI LÀ 10 SỐ
            if (sdt.Length != 10 || !sdt.All(char.IsDigit))
            {
                MessageBox.Show("Số điện thoại phải gồm đúng 10 chữ số!",
                                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtbMatKhau.Focus();
                return;
            }
            // KIỂM TRA TÊN NHÂN VIÊN TRÙNG
            string queryCheck = "SELECT COUNT(*) FROM NhanVien WHERE HoTen = @ten";
            SqlCommand cmdCheck = new SqlCommand(queryCheck, conn);
            cmdCheck.Parameters.AddWithValue("@ten", txtbTaiKhoan.Text.Trim());

            conn.Open();
            int count = (int)cmdCheck.ExecuteScalar();
            conn.Close();

            if (count > 0)
            {
                MessageBox.Show("Tên nhân viên đã tồn tại! Vui lòng nhập tên khác.",
                                "Trùng tên", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string queryUser = "SELECT COUNT(*) FROM NhanVien WHERE Username = @user";
            SqlCommand cmdUser = new SqlCommand(queryUser, conn);
            cmdUser.Parameters.AddWithValue("@user", txtUser.Text.Trim());

            conn.Open();
            int existUser = (int)cmdUser.ExecuteScalar();
            conn.Close();

            if (existUser > 0)
            {
                MessageBox.Show("Username đã tồn tại! Vui lòng chọn username khác.",
                                "Trùng Username", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            txtbTaiKhoan.Clear();
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
