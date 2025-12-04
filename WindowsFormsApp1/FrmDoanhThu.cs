using iText.IO.Font;
using iText.IO.Font.Constants;   // ← CẦN ĐỂ DÙNG StandardFonts.HELVETICA
using iText.Kernel.Font;         // ← CẦN ĐỂ DÙNG PdfFont, PdfFontFactory
using iText.Kernel.Geom;          // ← CẦN ĐỂ DÙNG PageSize.A4
using iText.Kernel.Pdf;
using iText.Layout;              // ← CẦN ĐỂ DÙNG Document, Paragraph, Table
using iText.Layout.Element;      // ← CẦN ĐỂ DÙNG Paragraph, Table, Cell
using iText.Layout.Properties;
using Path = System.IO.Path;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
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

        private void button1_Click(object sender, EventArgs e)
        {
            DateTime ngayChon = dateTimePicker1.Value.Date;

            // LẤY DỮ LIỆU
            string queryHD = @"SELECT HD.MaHD, HD.NgayLapHD, HD.Tong, ISNULL(NV.HoTen, 'Không rõ') AS NhanVien 
                       FROM HoaDon HD LEFT JOIN NhanVien NV ON HD.MaNV = NV.MaNV
                       WHERE CONVERT(date, HD.NgayLapHD) = @ngay";

            DataTable dtHD = new DataTable();
            DataTable dtCT = new DataTable();

            try
            {
                using (SqlConnection con = new SqlConnection(conn.ConnectionString))
                {
                    con.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(queryHD, con))
                    {
                        da.SelectCommand.Parameters.AddWithValue("@ngay", ngayChon);
                        da.Fill(dtHD);
                    }

                    string queryCT = @"SELECT MaHD, TenSP, SoLuong, GiaSP, (SoLuong * GiaSP) AS ThanhTien 
                               FROM CTHD WHERE MaHD IN (SELECT MaHD FROM HoaDon WHERE CONVERT(date, NgayLapHD) = @ngay)";
                    using (SqlDataAdapter da = new SqlDataAdapter(queryCT, con))
                    {
                        da.SelectCommand.Parameters.AddWithValue("@ngay", ngayChon);
                        da.Fill(dtCT);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối CSDL: " + ex.Message);
                return;
            }

            if (dtHD.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất báo cáo!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal tongDoanhThu = dtHD.AsEnumerable().Sum(r => r.Field<decimal>("Tong"));
            decimal loiNhuan = tongDoanhThu * 0.2m;

            SaveFileDialog save = new SaveFileDialog
            {
                Filter = "PDF File|*.pdf",
                FileName = $"BaoCao_DoanhThu_{ngayChon:dd-MM-yyyy}.pdf"
            };

            if (save.ShowDialog() != DialogResult.OK) return;

            try
            {
                using (FileStream stream = new FileStream(save.FileName, FileMode.Create))
                {
                    PdfWriter writer = new PdfWriter(stream);
                    PdfDocument pdf = new PdfDocument(writer);
                    Document document = new Document(pdf, PageSize.A4);
                    document.SetMargins(40, 40, 40, 40);

                    // FONT AN TOÀN
                    PdfFont fontNormal = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                    PdfFont fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                    PdfFont fontItalic = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE);

                    // dùng Arial thật (nếu có)
                    try
                    {
                        string arial = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                        string arialBold = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arialbd.ttf");
                        string arialItal = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "ariali.ttf");

                        if (File.Exists(arial)) fontNormal = PdfFontFactory.CreateFont(arial, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
                        if (File.Exists(arialBold)) fontBold = PdfFontFactory.CreateFont(arialBold, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
                        if (File.Exists(arialItal)) fontItalic = PdfFontFactory.CreateFont(arialItal, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
                    }
                    catch { /* dùng font mặc định */ }

                    // NỘI DUNG PDF
                    document.Add(new Paragraph("BÁO CÁO DOANH THU QUÁN CAFE")
                        .SetFont(fontBold).SetFontSize(22).SetTextAlignment(TextAlignment.CENTER));

                    document.Add(new Paragraph($"Ngày: {ngayChon:dd/MM/yyyy}")
                        .SetFont(fontNormal).SetFontSize(16).SetTextAlignment(TextAlignment.CENTER)
                        .SetMarginBottom(20));

                    document.Add(new Paragraph("1. DANH SÁCH HÓA ĐƠN")
                        .SetFont(fontBold).SetFontSize(14).SetMarginBottom(8));

                    Table tableHD = new Table(4).UseAllAvailableWidth();
                    tableHD.SetFont(fontNormal).SetFontSize(11);

                    tableHD.AddHeaderCell(new Cell().Add(new Paragraph("Mã HD").SetFont(fontBold)));
                    tableHD.AddHeaderCell(new Cell().Add(new Paragraph("Ngày lập").SetFont(fontBold)));
                    tableHD.AddHeaderCell(new Cell().Add(new Paragraph("Tổng tiền").SetFont(fontBold)));
                    tableHD.AddHeaderCell(new Cell().Add(new Paragraph("Nhân viên").SetFont(fontBold)));

                    foreach (DataRow row in dtHD.Rows)
                    {
                        tableHD.AddCell(row["MaHD"].ToString());
                        tableHD.AddCell(((DateTime)row["NgayLapHD"]).ToString("dd/MM/yyyy HH:mm"));
                        tableHD.AddCell(decimal.Parse(row["Tong"].ToString()).ToString("N0") + " đ");
                        tableHD.AddCell(row["NhanVien"].ToString());
                    }
                    document.Add(tableHD);

                    document.Add(new Paragraph("\n2. CHI TIẾT SẢN PHẨM ĐÃ BÁN")
                        .SetFont(fontBold).SetFontSize(14).SetMarginTop(20));

                    Table tableSP = new Table(5).UseAllAvailableWidth();
                    tableSP.SetFont(fontNormal).SetFontSize(11);

                    tableSP.AddHeaderCell(new Cell().Add(new Paragraph("Mã HD").SetFont(fontBold)));
                    tableSP.AddHeaderCell(new Cell().Add(new Paragraph("Tên SP").SetFont(fontBold)));
                    tableSP.AddHeaderCell(new Cell().Add(new Paragraph("SL").SetFont(fontBold)));
                    tableSP.AddHeaderCell(new Cell().Add(new Paragraph("Giá").SetFont(fontBold)));
                    tableSP.AddHeaderCell(new Cell().Add(new Paragraph("Thành tiền").SetFont(fontBold)));

                    foreach (DataRow row in dtCT.Rows)
                    {
                        tableSP.AddCell(row["MaHD"].ToString());
                        tableSP.AddCell(row["TenSP"].ToString());
                        tableSP.AddCell(row["SoLuong"].ToString());
                        tableSP.AddCell(decimal.Parse(row["GiaSP"].ToString()).ToString("N0") + " đ");
                        tableSP.AddCell(decimal.Parse(row["ThanhTien"].ToString()).ToString("N0") + " đ");
                    }
                    document.Add(tableSP);

                    document.Add(new Paragraph($"\nTỔNG DOANH THU: {tongDoanhThu:N0} đ")
                        .SetFont(fontBold).SetFontSize(16).SetTextAlignment(TextAlignment.RIGHT));

                    document.Add(new Paragraph($"TỔNG LỢI NHUẬN (20%): {loiNhuan:N0} đ")
                        .SetFont(fontBold).SetFontSize(14).SetTextAlignment(TextAlignment.RIGHT));

                    document.Add(new Paragraph($"\nIn lúc: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                        .SetFont(fontItalic).SetFontSize(10).SetTextAlignment(TextAlignment.RIGHT));

                    document.Close();
                }

                MessageBox.Show("Xuất báo cáo PDF thành công!\nĐã lưu tại: " + save.FileName, "Thành công",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xuất PDF: " + ex.Message + "\n\nChi tiết: " + ex.ToString(), "Lỗi",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
