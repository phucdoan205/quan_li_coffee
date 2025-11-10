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

namespace WindowsFormsApp1
{
    public partial class FrmDoanhThu : Form
    {
        SqlConnection conn = new SqlConnection(@"Data Source=DESKTOP-60ETTE4\SQLEXPRESS;Initial Catalog=QL_Cafe;Integrated Security=True;TrustServerCertificate=True");
        public FrmDoanhThu()
        {
            InitializeComponent();
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            DateTime ngay = dateTimePicker1.Value.Date;

            string query = @"SELECT HD.MaHD,HD.NgayLapHD,HD.Tong,NV.HoTen AS NhanVien FROM HoaDon HD
               LEFT JOIN NhanVien NV ON HD.MaNV = NV.MaNV
               WHERE CONVERT(date, HD.NgayLapHD) = @ngay";

            SqlDataAdapter da = new SqlDataAdapter(query, conn);
            da.SelectCommand.Parameters.AddWithValue("@ngay", ngay);

            DataTable dt = new DataTable();
            da.Fill(dt);

            string thongbao = "Tổng số hóa đơn là: ";
            string thongbao1 = "Tổng tiền hóa đơn là: ";
            string thongbao2 = "Tổng lợi nhuận là: ";
            dgvHD.DataSource = dt;
            // Tổng số hóa đơn
            label3.Text = thongbao + dgvHD.Rows.Count.ToString();

            // Tổng tiền hóa đơn
            int tong = 0;
            foreach (DataRow row in dt.Rows)
            {
                tong += Convert.ToInt32(row["Tong"]);
            }
            label1.Text = thongbao1 + tong.ToString("N0") + " đ";

            // Lợi nhuận = 20% tổng tiền (ví dụ)
            label2.Text = thongbao2 + (tong * 0.2).ToString("N0") + " đ";

        }

        private void FrmDoanhThu_Load(object sender, EventArgs e)
        {
            dgvHD.CellClick += dgvHD_CellClick;
        }

        private void dgvHD_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            int maHD = Convert.ToInt32(dgvHD.Rows[e.RowIndex].Cells["MaHD"].Value);

            string query = @"SELECT TenSP, SoLuong, GiaSP, (SoLuong * GiaSP) AS ThanhTien FROM CTHD WHERE MaHD = @mahd";

            SqlDataAdapter da = new SqlDataAdapter(query, conn);
            da.SelectCommand.Parameters.AddWithValue("@mahd", maHD);

            DataTable dt = new DataTable();
            da.Fill(dt);

            dgvCTHD.DataSource = dt;
        }
    }
}
