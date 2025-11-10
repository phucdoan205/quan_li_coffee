using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        SqlConnection conn = new SqlConnection(@"Data Source=DESKTOP-60ETTE4\SQLEXPRESS;Initial Catalog=QL_Cafe;Integrated Security=True;TrustServerCertificate=True");
        Dictionary<string, int> DonGiaGoc = new Dictionary<string, int>();

        public Form1()
        {
            InitializeComponent();
        }
        void LoadMenu()
        {
            foreach (var mon in dsMon)
            {
                AddMenuItem(mon.Ten, mon.Anh, mon.Gia);
                DonGiaGoc[mon.Ten] = mon.Gia;
            }
        }

        List<(string Ten, string Anh, int Gia)> dsMon = new List<(string, string, int)>
        {
            ("Cà phê sữa", "caphesua.jpg", 20000),
            ("Cà phê đen", "caphedenda.jpg", 20000),
            ("Cà phê muối", "caphemuoi.jpg", 25000),
            ("Cà phê trứng", "caphetrung.jpg", 28000),
            ("Cà phê cốt dừa", "caphecotdua.jpg", 27000),
            ("Cà phê đá xay", "caphedaxay.jpg", 25000),
            ("Bạc xỉu", "bacxiu.jpg", 20000),
            ("Latte đá", "latteda.jpg", 26000),
            ("Latte hạt nhân", "lattehatnhan.jpg", 27000),
            ("Latte matcha", "lattematcha.jpg", 28000),
            ("Lattle chocolate", "chocolatte.jpg", 26000),
            ("Trà đào", "tradao.jpg", 20000),
            ("Trà đá", "trada.jpg", 10000),
            ("Trà sữa", "trasua.jpg", 25000),
            ("Trà chanh mật ong", "trachanhmo.jpg", 20000),
            ("Trà matcha", "tramatcha.jpg", 25000)
        };

        void AddMenuItem(string ten, string anh, int gia)
        {
            Panel panelMon = new Panel
            {
                Size = new Size(130, 170),
                Margin = new Padding(10)
            };

            PictureBox pic = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.StretchImage,
                Size = new Size(120, 120),
                Location = new Point(5, 5)
            };

            string path = @"D:\c#\vs tím\cuối kì windowform\WindowsFormsApp1\Resources\" + anh;
            if (File.Exists(path))
                pic.Image = Image.FromFile(path);
            else
                MessageBox.Show("Không tìm thấy ảnh: " + path);

            Label lblTen = new Label
            {
                Text = ten,
                Size = new Size(120, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(5, 130)
            };

            Label lblGia = new Label
            {
                Text = gia.ToString("N0") + " đ",
                Size = new Size(120, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.DarkRed,
                Location = new Point(5, 150)
            };

            panelMon.Controls.Add(pic);
            panelMon.Controls.Add(lblTen);
            panelMon.Controls.Add(lblGia);

            panelMon.Click += (s, e) => ThemVaoHoaDon(ten, gia);
            pic.Click += (s, e) => ThemVaoHoaDon(ten, gia);
            lblTen.Click += (s, e) => ThemVaoHoaDon(ten, gia);
            lblGia.Click += (s, e) => ThemVaoHoaDon(ten, gia);

            flowMenu1.Controls.Add(panelMon);
        }
      
        private void MakeButtonRound(Button btn)
        {
            GraphicsPath gp = new GraphicsPath();
            gp.AddEllipse(0, 0, btn.Width - 1, btn.Height - 1);
            btn.Region = new Region(gp);
        }
        private void MakeButtonHCNT(Button btn, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            Rectangle rect = new Rectangle(0, 0, btn.Width, btn.Height);

            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseAllFigures();

            btn.Region = new Region(path);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MakeButtonRound(btnThoat);
            MakeButtonHCNT(btnThanhToan, 5);
            // Tắt thêm dòng trống cuối
            dgvorder.AllowUserToAddRows = false;
            LoadMenu();
        }

        private void btnThanhToan_Click(object sender, EventArgs e)
        {
            if (dgvorder.Rows.Count == 0)
            {
                MessageBox.Show("Chưa có món nào!");
                return;
            }

            if (MessageBox.Show("Xác nhận thanh toán?",
                 "Thanh toán", MessageBoxButtons.YesNo) == DialogResult.No)
                return;

            int tongTien = 0;

            foreach (DataGridViewRow row in dgvorder.Rows)
                tongTien += Convert.ToInt32(row.Cells["GiaSP"].Value);

            conn.Open();

            // ✅ 1) Lưu hóa đơn trước
            SqlCommand cmdHD = new SqlCommand(
                "INSERT INTO HoaDon (MaNV,NgayLapHD, Tong) OUTPUT INSERTED.MaHD " +
                "VALUES (@manv,GETDATE(), @tong)", conn);

            cmdHD.Parameters.AddWithValue("@manv", Program.MaNV_DangNhap);
            cmdHD.Parameters.AddWithValue("@tong", tongTien);

            int maHD = (int)cmdHD.ExecuteScalar();

            // ✅ 2) Lưu chi tiết hóa đơn (không lưu thành tiền)
            foreach (DataGridViewRow row in dgvorder.Rows)
            {
                string ten = row.Cells["TenSP"].Value.ToString();
                int sl = Convert.ToInt32(row.Cells["SoLuong"].Value);
                int giaGoc = DonGiaGoc[ten]; // giá gốc

                SqlCommand cmdCT = new SqlCommand("INSERT INTO CTHD (MaHD, TenSP, SoLuong, GiaSP) " + "VALUES (@mahd, @ten, @sl, @gia)", conn);

                cmdCT.Parameters.AddWithValue("@mahd", maHD);
                cmdCT.Parameters.AddWithValue("@ten", ten);
                cmdCT.Parameters.AddWithValue("@sl", sl);
                cmdCT.Parameters.AddWithValue("@gia", giaGoc);

                cmdCT.ExecuteNonQuery();
            }

            conn.Close();

            // ✅ 3) Reset order sau khi lưu
            dgvorder.Rows.Clear();
            TinhTongTien();

            MessageBox.Show("Đã lưu hóa đơn thành công!");
        }
        void TinhTongTien()
        {
            int tong = 0;

            foreach (DataGridViewRow row in dgvorder.Rows)
            {
                tong += Convert.ToInt32(row.Cells["GiaSP"].Value);
            }

            lblTongTien.Text = tong.ToString("N0") + " đ";
        }


        private void btnThoat_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnQuanly_Click(object sender, EventArgs e)
        {
            Form ql = new FrmADMIN();
            ql.Show();
        }
        void ThemVaoHoaDon(string ten, int gia)
        {
            bool found = false;

            foreach (DataGridViewRow row in dgvorder.Rows)
            {
                if (row.Cells["TenSP"].Value.ToString() == ten)
                {
                    int sl = Convert.ToInt32(row.Cells["SoLuong"].Value) + 1;
                    row.Cells["SoLuong"].Value = sl;

                    int giaGoc = DonGiaGoc[ten];
                    row.Cells["GiaSP"].Value = sl * giaGoc;

                    found = true;
                    break;
                }
            }

            if (!found)
            {
                dgvorder.Rows.Add(ten, 1, gia);
            }

            TinhTongTien();
        }

        private void dgvorder_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string ten = dgvorder.Rows[e.RowIndex].Cells["TenSP"].Value.ToString();
            int sl = Convert.ToInt32(dgvorder.Rows[e.RowIndex].Cells["SoLuong"].Value);
            int giaGoc = DonGiaGoc[ten];

            if (sl > 1)
            {
                sl--;
                dgvorder.Rows[e.RowIndex].Cells["SoLuong"].Value = sl;
                dgvorder.Rows[e.RowIndex].Cells["GiaSP"].Value = sl * giaGoc;
            }
            else
            {
                dgvorder.Rows.RemoveAt(e.RowIndex);
            }

            TinhTongTien();
        }
    }
}
