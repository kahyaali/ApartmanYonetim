using ApartmanYonetim.Data;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApartmanYonetim.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ExportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ───── ÖDEMELER ─────
        public async Task<IActionResult> Payments(int? ay, int? yil)
        {
            var query = _context.Payments
                .Include(p => p.Apartment).ThenInclude(a => a.Block)
                .Include(p => p.Transactions)
                .AsQueryable();

            if (ay.HasValue && yil.HasValue)
                query = query.Where(p => p.SonOdemeTarihi.Month == ay
                                      && p.SonOdemeTarihi.Year == yil);

            var odemeler = await query
                .OrderBy(p => p.Apartment.Block!.Ad)
                .ThenBy(p => p.Apartment.DaireNo)
                .ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Ödemeler");

            // Başlık satırı
            ws.Cell(1, 1).Value = "Blok";
            ws.Cell(1, 2).Value = "Daire No";
            ws.Cell(1, 3).Value = "Açıklama";
            ws.Cell(1, 4).Value = "Toplam Tutar";
            ws.Cell(1, 5).Value = "Ödenen Tutar";
            ws.Cell(1, 6).Value = "Kalan Tutar";
            ws.Cell(1, 7).Value = "Son Ödeme Tarihi";
            ws.Cell(1, 8).Value = "Durum";
            ws.Cell(1, 9).Value = "Ödeme Hareketi Sayısı";

            // Başlık stili
            var baslikAraligi = ws.Range("A1:I1");
            baslikAraligi.Style.Font.Bold = true;
            baslikAraligi.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3a5f");
            baslikAraligi.Style.Font.FontColor = XLColor.White;
            baslikAraligi.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            baslikAraligi.Style.Border.BottomBorder = XLBorderStyleValues.Medium;

            int row = 2;
            foreach (var p in odemeler)
            {
                var odenen = p.OdenenTutar;
                var kalan = p.KalanTutar;
                var bugun = DateTime.Today;
                var fark = (p.SonOdemeTarihi.Date - bugun).TotalDays;

                string durum;
                XLColor renk;

                if (p.Odendi || kalan <= 0)
                {
                    durum = "Ödendi";
                    renk = XLColor.FromHtml("#d4edda");
                }
                else if (fark < 0)
                {
                    durum = "Gecikmiş";
                    renk = XLColor.FromHtml("#ffd6d6");
                }
                else if (fark <= 3)
                {
                    durum = "Acil";
                    renk = XLColor.FromHtml("#ffd6d6");
                }
                else if (fark <= 5)
                {
                    durum = "Yaklaşıyor";
                    renk = XLColor.FromHtml("#fff9c4");
                }
                else
                {
                    durum = "Bekliyor";
                    renk = XLColor.FromHtml("#fde8cc");
                }

                ws.Cell(row, 1).Value = p.Apartment?.Block?.Ad ?? "-";
                ws.Cell(row, 2).Value = p.Apartment?.DaireNo.ToString() ?? "-";
                ws.Cell(row, 3).Value = p.Aciklama;
                ws.Cell(row, 4).Value = p.Tutar;
                ws.Cell(row, 5).Value = odenen;
                ws.Cell(row, 6).Value = kalan;
                ws.Cell(row, 7).Value = p.SonOdemeTarihi.ToString("dd.MM.yyyy");
                ws.Cell(row, 8).Value = durum;
                ws.Cell(row, 9).Value = p.Transactions.Count;

                // Para formatı
                ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00 ₺";
                ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00 ₺";
                ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00 ₺";

                // Satır rengi
                var satirAraligi = ws.Range(row, 1, row, 9);
                satirAraligi.Style.Fill.BackgroundColor = renk;

                // Alternatif satır için hafif border
                satirAraligi.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                satirAraligi.Style.Border.BottomBorderColor = XLColor.FromHtml("#e2e8f0");

                row++;
            }

            // Özet satırı
            ws.Cell(row, 1).Value = "TOPLAM";
            ws.Cell(row, 4).Value = odemeler.Sum(p => p.Tutar);
            ws.Cell(row, 5).Value = odemeler.Sum(p => p.OdenenTutar);
            ws.Cell(row, 6).Value = odemeler.Sum(p => p.KalanTutar);
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00 ₺";
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00 ₺";
            ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00 ₺";

            var toplamAraligi = ws.Range(row, 1, row, 9);
            toplamAraligi.Style.Font.Bold = true;
            toplamAraligi.Style.Fill.BackgroundColor = XLColor.FromHtml("#e8f4fd");
            toplamAraligi.Style.Border.TopBorder = XLBorderStyleValues.Medium;

            // Kolon genişlikleri
            ws.Column(1).Width = 15;
            ws.Column(2).Width = 12;
            ws.Column(3).Width = 20;
            ws.Column(4).Width = 16;
            ws.Column(5).Width = 16;
            ws.Column(6).Width = 16;
            ws.Column(7).Width = 18;
            ws.Column(8).Width = 14;
            ws.Column(9).Width = 20;

            // Freeze başlık
            ws.SheetView.FreezeRows(1);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            ms.Position = 0;

            var dosyaAdi = ay.HasValue
                ? $"Odemeler_{yil}_{ay:D2}.xlsx"
                : $"Odemeler_{DateTime.Now:yyyyMMdd}.xlsx";

            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                dosyaAdi);
        }

        // ───── SAKİNLER ─────
        public async Task<IActionResult> Residents()
        {
            var sakinler = await _context.Residents
                .Include(r => r.Apartment).ThenInclude(a => a.Block)
                .OrderBy(r => r.Apartment.Block!.Ad)
                .ThenBy(r => r.Apartment.DaireNo)
                .ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Sakinler");

            ws.Cell(1, 1).Value = "Ad";
            ws.Cell(1, 2).Value = "Soyad";
            ws.Cell(1, 3).Value = "Blok";
            ws.Cell(1, 4).Value = "Daire No";
            ws.Cell(1, 5).Value = "Taşınma Tarihi";
            ws.Cell(1, 6).Value = "Ayrılma Tarihi";
            ws.Cell(1, 7).Value = "Durum";

            var baslik = ws.Range("A1:G1");
            baslik.Style.Font.Bold = true;
            baslik.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3a5f");
            baslik.Style.Font.FontColor = XLColor.White;
            baslik.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int row = 2;
            foreach (var s in sakinler)
            {
                ws.Cell(row, 1).Value = s.Ad;
                ws.Cell(row, 2).Value = s.Soyad;
                ws.Cell(row, 3).Value = s.Apartment?.Block?.Ad ?? "-";
                ws.Cell(row, 4).Value = s.Apartment?.DaireNo.ToString() ?? "-";
                ws.Cell(row, 5).Value = s.TasinmaTarihi.ToString("dd.MM.yyyy");
                ws.Cell(row, 6).Value = s.AyrilmaTarihi.HasValue
                    ? s.AyrilmaTarihi.Value.ToString("dd.MM.yyyy") : "-";
                ws.Cell(row, 7).Value = s.AktifMi ? "Aktif" : "Ayrıldı";

                var satirAraligi = ws.Range(row, 1, row, 7);
                satirAraligi.Style.Fill.BackgroundColor = s.AktifMi
                    ? XLColor.FromHtml("#f0fff4")
                    : XLColor.FromHtml("#fff5f5");
                satirAraligi.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                satirAraligi.Style.Border.BottomBorderColor = XLColor.FromHtml("#e2e8f0");

                row++;
            }

            ws.Column(1).Width = 16;
            ws.Column(2).Width = 16;
            ws.Column(3).Width = 14;
            ws.Column(4).Width = 12;
            ws.Column(5).Width = 18;
            ws.Column(6).Width = 18;
            ws.Column(7).Width = 12;
            ws.SheetView.FreezeRows(1);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            ms.Position = 0;

            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Sakinler_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        // ───── GİDERLER ─────
        public async Task<IActionResult> Expenses(int? ay, int? yil)
        {
            var query = _context.Expenses.AsQueryable();

            if (ay.HasValue && yil.HasValue)
                query = query.Where(e => e.GiderTarihi.Month == ay
                                      && e.GiderTarihi.Year == yil);

            var giderler = await query
                .OrderByDescending(e => e.GiderTarihi)
                .ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Giderler");

            ws.Cell(1, 1).Value = "Firma Adı";
            ws.Cell(1, 2).Value = "Çalışma Türü";
            ws.Cell(1, 3).Value = "Açıklama";
            ws.Cell(1, 4).Value = "Tutar";
            ws.Cell(1, 5).Value = "Gider Tarihi";
            ws.Cell(1, 6).Value = "Fatura No";

            var baslik = ws.Range("A1:F1");
            baslik.Style.Font.Bold = true;
            baslik.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3a5f");
            baslik.Style.Font.FontColor = XLColor.White;
            baslik.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int row = 2;
            foreach (var g in giderler)
            {
                ws.Cell(row, 1).Value = g.FirmaAdi;
                ws.Cell(row, 2).Value = g.CalismaTuru;
                ws.Cell(row, 3).Value = g.Aciklama;
                ws.Cell(row, 4).Value = g.Tutar;
                ws.Cell(row, 5).Value = g.GiderTarihi.ToString("dd.MM.yyyy");
                ws.Cell(row, 6).Value = g.FaturaNo ?? "-";

                ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00 ₺";

                var satirAraligi = ws.Range(row, 1, row, 6);
                satirAraligi.Style.Fill.BackgroundColor = row % 2 == 0
                    ? XLColor.FromHtml("#f8fafc")
                    : XLColor.White;
                satirAraligi.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                satirAraligi.Style.Border.BottomBorderColor = XLColor.FromHtml("#e2e8f0");

                row++;
            }

            // Toplam satırı
            ws.Cell(row, 3).Value = "TOPLAM";
            ws.Cell(row, 4).Value = giderler.Sum(g => g.Tutar);
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00 ₺";

            var toplamAraligi = ws.Range(row, 1, row, 6);
            toplamAraligi.Style.Font.Bold = true;
            toplamAraligi.Style.Fill.BackgroundColor = XLColor.FromHtml("#fdf2f2");
            toplamAraligi.Style.Border.TopBorder = XLBorderStyleValues.Medium;

            ws.Column(1).Width = 20;
            ws.Column(2).Width = 18;
            ws.Column(3).Width = 30;
            ws.Column(4).Width = 16;
            ws.Column(5).Width = 16;
            ws.Column(6).Width = 16;
            ws.SheetView.FreezeRows(1);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            ms.Position = 0;

            var dosyaAdi = ay.HasValue
                ? $"Giderler_{yil}_{ay:D2}.xlsx"
                : $"Giderler_{DateTime.Now:yyyyMMdd}.xlsx";

            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                dosyaAdi);
        }

        // ───── AYLIK ÖZET RAPOR ─────
        public async Task<IActionResult> MonthlyReport(int ay, int yil)
        {
            using var wb = new XLWorkbook();

            // ── Sayfa 1: Gelirler ──
            var wsGelir = wb.Worksheets.Add("Gelirler");

            var odemeler = await _context.Payments
                .Include(p => p.Apartment).ThenInclude(a => a.Block)
                .Include(p => p.Transactions)
                .Where(p => p.SonOdemeTarihi.Month == ay
                         && p.SonOdemeTarihi.Year == yil)
                .OrderBy(p => p.Apartment.Block!.Ad)
                .ThenBy(p => p.Apartment.DaireNo)
                .ToListAsync();

            var ayAdi = new DateTime(yil, ay, 1)
                .ToString("MMMM yyyy", new System.Globalization.CultureInfo("tr-TR"));

            // Rapor başlığı
            wsGelir.Cell(1, 1).Value = $"AYLIK GELİR RAPORU — {ayAdi.ToUpper()}";
            wsGelir.Range("A1:I1").Merge();
            wsGelir.Cell(1, 1).Style.Font.Bold = true;
            wsGelir.Cell(1, 1).Style.Font.FontSize = 14;
            wsGelir.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3a5f");
            wsGelir.Cell(1, 1).Style.Font.FontColor = XLColor.White;
            wsGelir.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            wsGelir.Cell(2, 1).Value = "Blok";
            wsGelir.Cell(2, 2).Value = "Daire";
            wsGelir.Cell(2, 3).Value = "Açıklama";
            wsGelir.Cell(2, 4).Value = "Toplam";
            wsGelir.Cell(2, 5).Value = "Ödenen";
            wsGelir.Cell(2, 6).Value = "Kalan";
            wsGelir.Cell(2, 7).Value = "Son Tarih";
            wsGelir.Cell(2, 8).Value = "Durum";

            var baslik2 = wsGelir.Range("A2:H2");
            baslik2.Style.Font.Bold = true;
            baslik2.Style.Fill.BackgroundColor = XLColor.FromHtml("#e8f4fd");
            baslik2.Style.Border.BottomBorder = XLBorderStyleValues.Medium;

            int row = 3;
            foreach (var p in odemeler)
            {
                wsGelir.Cell(row, 1).Value = p.Apartment?.Block?.Ad ?? "-";
                wsGelir.Cell(row, 2).Value = $"Daire {p.Apartment?.DaireNo}";
                wsGelir.Cell(row, 3).Value = p.Aciklama;
                wsGelir.Cell(row, 4).Value = p.Tutar;
                wsGelir.Cell(row, 5).Value = p.OdenenTutar;
                wsGelir.Cell(row, 6).Value = p.KalanTutar;
                wsGelir.Cell(row, 7).Value = p.SonOdemeTarihi.ToString("dd.MM.yyyy");
                wsGelir.Cell(row, 8).Value = p.Odendi ? "Ödendi" : "Bekliyor";

                wsGelir.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00 ₺";
                wsGelir.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00 ₺";
                wsGelir.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00 ₺";

                var renk = p.Odendi
                    ? XLColor.FromHtml("#d4edda")
                    : XLColor.FromHtml("#fde8cc");
                wsGelir.Range(row, 1, row, 8).Style.Fill.BackgroundColor = renk;

                row++;
            }

            // Toplam
            wsGelir.Cell(row, 3).Value = "TOPLAM";
            wsGelir.Cell(row, 4).Value = odemeler.Sum(p => p.Tutar);
            wsGelir.Cell(row, 5).Value = odemeler.Sum(p => p.OdenenTutar);
            wsGelir.Cell(row, 6).Value = odemeler.Sum(p => p.KalanTutar);
            wsGelir.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00 ₺";
            wsGelir.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00 ₺";
            wsGelir.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00 ₺";
            wsGelir.Range(row, 1, row, 8).Style.Font.Bold = true;
            wsGelir.Range(row, 1, row, 8).Style.Fill.BackgroundColor = XLColor.FromHtml("#e8f4fd");
            wsGelir.Range(row, 1, row, 8).Style.Border.TopBorder = XLBorderStyleValues.Medium;

            wsGelir.Columns().AdjustToContents();
            wsGelir.SheetView.FreezeRows(2);

            // ── Sayfa 2: Giderler ──
            var wsGider = wb.Worksheets.Add("Giderler");

            var giderler = await _context.Expenses
                .Where(e => e.GiderTarihi.Month == ay && e.GiderTarihi.Year == yil)
                .OrderByDescending(e => e.GiderTarihi)
                .ToListAsync();

            wsGider.Cell(1, 1).Value = $"AYLIK GİDER RAPORU — {ayAdi.ToUpper()}";
            wsGider.Range("A1:F1").Merge();
            wsGider.Cell(1, 1).Style.Font.Bold = true;
            wsGider.Cell(1, 1).Style.Font.FontSize = 14;
            wsGider.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3a5f");
            wsGider.Cell(1, 1).Style.Font.FontColor = XLColor.White;
            wsGider.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            wsGider.Cell(2, 1).Value = "Firma";
            wsGider.Cell(2, 2).Value = "Çalışma Türü";
            wsGider.Cell(2, 3).Value = "Açıklama";
            wsGider.Cell(2, 4).Value = "Tutar";
            wsGider.Cell(2, 5).Value = "Tarih";
            wsGider.Cell(2, 6).Value = "Fatura No";

            wsGider.Range("A2:F2").Style.Font.Bold = true;
            wsGider.Range("A2:F2").Style.Fill.BackgroundColor = XLColor.FromHtml("#fdf2f2");
            wsGider.Range("A2:F2").Style.Border.BottomBorder = XLBorderStyleValues.Medium;

            int gRow = 3;
            foreach (var g in giderler)
            {
                wsGider.Cell(gRow, 1).Value = g.FirmaAdi;
                wsGider.Cell(gRow, 2).Value = g.CalismaTuru;
                wsGider.Cell(gRow, 3).Value = g.Aciklama;
                wsGider.Cell(gRow, 4).Value = g.Tutar;
                wsGider.Cell(gRow, 5).Value = g.GiderTarihi.ToString("dd.MM.yyyy");
                wsGider.Cell(gRow, 6).Value = g.FaturaNo ?? "-";
                wsGider.Cell(gRow, 4).Style.NumberFormat.Format = "#,##0.00 ₺";

                wsGider.Range(gRow, 1, gRow, 6).Style.Fill.BackgroundColor =
                    gRow % 2 == 0 ? XLColor.FromHtml("#fff5f5") : XLColor.White;

                gRow++;
            }

            wsGider.Cell(gRow, 3).Value = "TOPLAM";
            wsGider.Cell(gRow, 4).Value = giderler.Sum(g => g.Tutar);
            wsGider.Cell(gRow, 4).Style.NumberFormat.Format = "#,##0.00 ₺";
            wsGider.Range(gRow, 1, gRow, 6).Style.Font.Bold = true;
            wsGider.Range(gRow, 1, gRow, 6).Style.Fill.BackgroundColor = XLColor.FromHtml("#fdf2f2");
            wsGider.Range(gRow, 1, gRow, 6).Style.Border.TopBorder = XLBorderStyleValues.Medium;

            wsGider.Columns().AdjustToContents();
            wsGider.SheetView.FreezeRows(2);

            // ── Sayfa 3: Özet ──
            var wsOzet = wb.Worksheets.Add("Özet");

            var toplamGelir = odemeler.Sum(p => p.OdenenTutar);
            var toplamGider = giderler.Sum(g => g.Tutar);
            var netBakiye = toplamGelir - toplamGider;

            wsOzet.Cell(1, 1).Value = $"AYLIK ÖZET — {ayAdi.ToUpper()}";
            wsOzet.Range("A1:C1").Merge();
            wsOzet.Cell(1, 1).Style.Font.Bold = true;
            wsOzet.Cell(1, 1).Style.Font.FontSize = 14;
            wsOzet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3a5f");
            wsOzet.Cell(1, 1).Style.Font.FontColor = XLColor.White;
            wsOzet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            wsOzet.Cell(3, 1).Value = "Toplam Tahsilat";
            wsOzet.Cell(3, 2).Value = toplamGelir;
            wsOzet.Cell(3, 2).Style.NumberFormat.Format = "#,##0.00 ₺";
            wsOzet.Cell(3, 2).Style.Font.FontColor = XLColor.FromHtml("#27ae60");
            wsOzet.Cell(3, 2).Style.Font.Bold = true;

            wsOzet.Cell(4, 1).Value = "Toplam Gider";
            wsOzet.Cell(4, 2).Value = toplamGider;
            wsOzet.Cell(4, 2).Style.NumberFormat.Format = "#,##0.00 ₺";
            wsOzet.Cell(4, 2).Style.Font.FontColor = XLColor.FromHtml("#e74c3c");
            wsOzet.Cell(4, 2).Style.Font.Bold = true;

            wsOzet.Cell(5, 1).Value = "Net Bakiye";
            wsOzet.Cell(5, 2).Value = netBakiye;
            wsOzet.Cell(5, 2).Style.NumberFormat.Format = "#,##0.00 ₺";
            wsOzet.Cell(5, 2).Style.Font.FontColor = netBakiye >= 0
                ? XLColor.FromHtml("#27ae60")
                : XLColor.FromHtml("#e74c3c");
            wsOzet.Cell(5, 2).Style.Font.Bold = true;
            wsOzet.Cell(5, 2).Style.Font.FontSize = 14;

            wsOzet.Cell(7, 1).Value = "Ödeme Yapan Daire Sayısı";
            wsOzet.Cell(7, 2).Value = odemeler.Count(p => p.Odendi);

            wsOzet.Cell(8, 1).Value = "Ödeme Bekleyen Daire Sayısı";
            wsOzet.Cell(8, 2).Value = odemeler.Count(p => !p.Odendi);

            wsOzet.Cell(9, 1).Value = "Toplam Gider Kalemi";
            wsOzet.Cell(9, 2).Value = giderler.Count;

            wsOzet.Column(1).Width = 30;
            wsOzet.Column(2).Width = 20;

            // Sayfayı en başa al
            wb.Worksheets.First(w => w.Name == "Özet").Position = 1;

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            ms.Position = 0;

            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"AylikRapor_{yil}_{ay:D2}.xlsx");
        }
    }
}