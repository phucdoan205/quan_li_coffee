using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace WindowsFormsApp1
{
    public partial class FrmSuaNv : Form
    {
        SqlConnection conn = new SqlConnection(@"Data Source=DESKTOP-60ETTE4\SQLEXPRESS;Initial Catalog=QL_Cafe;Integrated Security=True;TrustServerCertificate=True");
        private FrmNhanVien formCha;
        private int NVID;
        public FrmSuaNv(FrmNhanVien parent, int id, string ten, string sdt, DateTime ngay)
        {
            InitializeComponent();
            formCha = parent;
            NVID = id;     // ID LƯU TẠI ĐÂY - KHÔNG CHO NGƯỜI DÙNG SỬA

            txtbTaiKhoan.Text = ten;
            txtbMatKhau.Text = sdt;
            dateTimePicker1.Value = ngay;
        }

        private void btnHuy_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            string ten = txtbTaiKhoan.Text.Trim();
            string sdt = txtbMatKhau.Text.Trim();
            DateTime ngay = dateTimePicker1.Value;

            if (sdt.Length != 10 || !sdt.All(char.IsDigit))
            {
                MessageBox.Show("Số điện thoại phải gồm đúng 10 chữ số!",
                                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtbMatKhau.Focus();
                return;
            }

            string query = "UPDATE NhanVien SET HoTen = @ten, SDT = @sdt, NgayVaoLam = @ngay WHERE MaNV = @id";

            SqlCommand cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@ten", ten);
            cmd.Parameters.AddWithValue("@sdt", sdt);
            cmd.Parameters.AddWithValue("@ngay", ngay);
            cmd.Parameters.AddWithValue("@id", NVID);   //GIỮ NGUYÊN ID

            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            MessageBox.Show("Cập nhật thông tin thành công!");

            formCha.LoadData();  // reload dữ liệu
            this.Close();
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
