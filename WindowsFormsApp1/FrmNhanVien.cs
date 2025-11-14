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
    public partial class FrmNhanVien : Form
    {
        SqlConnection conn = new SqlConnection (@"Data Source=NGOCHONG\SQLEXPRESS;Initial Catalog=QL_Cafe;Integrated Security=True;TrustServerCertificate=True");
        public FrmNhanVien()
        {
            InitializeComponent();
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            Form themnv = new FrmThemNV(this);
            themnv.Show();
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (dgvNhanVien.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần sửa!");
                return;
            }

            int id = Convert.ToInt32(dgvNhanVien.CurrentRow.Cells["MaNV"].Value);
            string ten = dgvNhanVien.CurrentRow.Cells["HoTen"].Value.ToString();
            string sdt = dgvNhanVien.CurrentRow.Cells["SDT"].Value.ToString();
            DateTime ngay = Convert.ToDateTime(dgvNhanVien.CurrentRow.Cells["NgayVaoLam"].Value);
            Form suanv = new FrmSuaNv(this,id, ten,sdt,ngay);
            suanv.Show();
        }
        public void LoadData()
        {
            string query = "SELECT MaNV, HoTen, SDT, NgayVaoLam, TinhTrang, Username, Password FROM NhanVien";

            SqlDataAdapter da = new SqlDataAdapter(query, conn);
            DataTable dt = new DataTable();
            da.Fill(dt);

            dgvNhanVien.DataSource = dt;

            // Ẩn cột ID khỏi giao diện nhưng vẫn dùng được
            dgvNhanVien.Columns["MaNV"].Visible = false;
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (dgvNhanVien.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần xóa!");
                return;
            }
            int id = Convert.ToInt32(dgvNhanVien.CurrentRow.Cells["MaNV"].Value);

            if (MessageBox.Show("Bạn có chắc muốn xóa nhân viên này?",
                "Xóa", MessageBoxButtons.YesNo) == DialogResult.No)
                return;

            string query = "DELETE FROM NhanVien WHERE MaNV = @id";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);

            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            LoadData();
            MessageBox.Show("Xóa nhân viên thành công!");
        }

        private void TxtbTimKiem_TextChanged(object sender, EventArgs e)
        {
            SqlCommand cmd = new SqlCommand("SELECT MaNV, HoTen, SDT, NgayVaoLam, TinhTrang FROM NhanVien WHERE HoTen LIKE '%' + @key + '%'", conn);
            cmd.Parameters.AddWithValue("@key", TxtbTimKiem.Text);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            dgvNhanVien.DataSource = dt;
            dgvNhanVien.Columns["MaNV"].Visible = false;
        }

        private void FrmNhanVien_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void btnthongke_Click(object sender, EventArgs e)
        {
            Form thongke = new FrmDoanhThu();
            thongke.Show();
        }

        private void btntrove_Click(object sender, EventArgs e)
        {
            Form trove = new FrmADMIN();
            trove.Show();
            this.Hide();
        }
    }
}
