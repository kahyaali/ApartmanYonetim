using ApartmanYonetim.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ApartmanYonetim.Controllers
{
    [Authorize]
    public class PdfController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PdfController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Makbuz(int id)
        {
            try
            {
                var payment = await _context.Payments
                    .Include(p => p.Apartment).ThenInclude(a => a.Block)
                    .Include(p => p.Transactions)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (payment == null)
                {
                    TempData["Error"] = "Ödeme kaydı bulunamadı.";
                    return RedirectToAction("Index", "Payment");
                }

                var isAdmin = User.IsInRole("Admin");
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (!isAdmin)
                {
                    var user = await _context.Users
                        .FirstOrDefaultAsync(u => u.Id == userId);

                    if (user?.ApartmentId != payment.ApartmentId)
                    {
                        TempData["Error"] = "Bu makbuza erişim yetkiniz yok.";
                        return RedirectToAction("MyPayments", "Payment");
                    }
                }

                var pdf = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontSize(11));
                        page.Header().Element(ComposeHeader);
                        page.Content().Element(content =>
                            ComposeContent(content, payment));
                        page.Footer().Element(ComposeFooter);
                    });
                });

                var bytes = pdf.GeneratePdf();
                var dosyaAdi = $"Makbuz_Daire{payment.Apartment?.DaireNo}_{payment.Id}.pdf";

                return File(bytes, "application/pdf", dosyaAdi);
            }

            catch (Exception ex)
            {
                TempData["Error"] = $"PDF hatası: {ex.Message} - {ex.InnerException?.Message}";
               // TempData["Error"] = "PDF oluşturulurken hata oluştu.";
                return RedirectToAction("Index", "Payment");
            }
        }

     
        private void ComposeHeader(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("APARTMAN YÖNETİM SİSTEMİ")
                            .FontSize(18).Bold().FontColor("#1e3a5f");
                        c.Item().Text("Ödeme Makbuzu")
                            .FontSize(12).FontColor("#718096");
                    });

                    row.ConstantItem(100).Column(c =>
                    {
                        c.Item().AlignRight().Text($"Tarih: {DateTime.Now:dd.MM.yyyy}")
                            .FontSize(10).FontColor("#718096");
                        c.Item().AlignRight().Text($"Saat: {DateTime.Now:HH:mm}")
                            .FontSize(10).FontColor("#718096");
                    });
                });

                col.Item().PaddingTop(8).LineHorizontal(2).LineColor("#1e3a5f");
            });
        }

      
        private void ComposeContent(IContainer container, ApartmanYonetim.Models.Entities.Payment payment)
        {
            container.PaddingVertical(20).Column(col =>
            {
                // Daire Bilgisi (DÜZELTİLDİ)
                col.Item().Background("#f8fafc").Padding(12).Element(x =>
                {
                    x.Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("DAİRE BİLGİSİ").Bold().FontSize(10).FontColor("#718096");
                            c.Item().PaddingTop(4).Text($"{payment.Apartment?.Block?.Ad ?? ""} — Daire {payment.Apartment?.DaireNo}")
                                .Bold().FontSize(14).FontColor("#1e3a5f");
                        });

                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("MAKBUZ NO").Bold().FontSize(10).FontColor("#718096");
                            c.Item().PaddingTop(4).Text($"#{payment.Id:D6}").Bold().FontSize(14).FontColor("#1e3a5f");
                        });
                    });
                });

                col.Item().PaddingTop(16).Text("BORÇ BİLGİSİ").Bold().FontSize(10).FontColor("#718096");

                // Borç tablosu
                col.Item().PaddingTop(8).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(3);
                        cols.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background("#1e3a5f").Padding(8).Text("Açıklama").Bold().FontColor("#ffffff");
                        header.Cell().Background("#1e3a5f").Padding(8).AlignRight().Text("Tutar").Bold().FontColor("#ffffff");
                    });

                    table.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(8).Text(payment.Aciklama);
                    table.Cell().BorderBottom(1).BorderColor("#e2e8f0").Padding(8).AlignRight().Text($"{payment.Tutar:N2} ₺").Bold();

                    table.Cell().Background("#f0fff4").Padding(8).Text("Ödenen Toplam").Bold().FontColor("#27ae60");
                    table.Cell().Background("#f0fff4").Padding(8).AlignRight().Text($"{payment.OdenenTutar:N2} ₺").Bold().FontColor("#27ae60");

                    if (payment.KalanTutar > 0)
                    {
                        table.Cell().Background("#fff5f5").Padding(8).Text("Kalan Borç").Bold().FontColor("#e74c3c");
                        table.Cell().Background("#fff5f5").Padding(8).AlignRight().Text($"{payment.KalanTutar:N2} ₺").Bold().FontColor("#e74c3c");
                    }
                    else
                    {
                        table.Cell().Background("#f0fff4").Padding(8).Text("BORÇ YOK ✓").Bold().FontColor("#27ae60");
                        table.Cell().Background("#f0fff4").Padding(8).AlignRight().Text("—").FontColor("#27ae60");
                    }
                });

                // Ödeme hareketleri
                if (payment.Transactions.Any())
                {
                    col.Item().PaddingTop(20).Text("ÖDEME HAREKETLERİ").Bold().FontSize(10).FontColor("#718096");

                    col.Item().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(30);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background("#e8f4fd").Padding(6).Text("#").Bold().FontColor("#2d6a9f");
                            header.Cell().Background("#e8f4fd").Padding(6).Text("Tarih").Bold().FontColor("#2d6a9f");
                            header.Cell().Background("#e8f4fd").Padding(6).AlignRight().Text("Tutar").Bold().FontColor("#2d6a9f");
                            header.Cell().Background("#e8f4fd").Padding(6).Text("Ödeyen").Bold().FontColor("#2d6a9f");
                            header.Cell().Background("#e8f4fd").Padding(6).Text("Not").Bold().FontColor("#2d6a9f");
                        });

                        int sira = 1;
                        foreach (var t in payment.Transactions.OrderBy(t => t.OdemeTarihi))
                        {
                            var bg = sira % 2 == 0 ? "#f8fafc" : "#ffffff";
                            table.Cell().Background(bg).Padding(6).Text(sira.ToString()).FontColor("#718096");
                            table.Cell().Background(bg).Padding(6).Text(t.OdemeTarihi.ToString("dd.MM.yyyy"));
                            table.Cell().Background(bg).Padding(6).AlignRight().Text($"{t.OdenenTutar:N2} ₺").Bold().FontColor("#27ae60");
                            table.Cell().Background(bg).Padding(6).Text(t.OdeyenKisi ?? "-").FontColor("#718096");
                            table.Cell().Background(bg).Padding(6).Text(t.Aciklama ?? "-").FontColor("#718096");
                            sira++;
                        }
                    });
                }

                // Son ödeme tarihi
                col.Item().PaddingTop(16).Background(payment.Odendi ? "#f0fff4" : "#fff5f5").Padding(12).Element(x =>
                {
                    x.Row(row =>
                    {
                        row.RelativeItem().Text($"Son Ödeme Tarihi: {payment.SonOdemeTarihi:dd.MM.yyyy}")
                            .Bold().FontColor(payment.Odendi ? "#27ae60" : "#e74c3c");

                        row.RelativeItem().AlignRight().Text(payment.Odendi ? "✓ ÖDEME TAMAMLANDI" : "⚠ ÖDEME BEKLİYOR")
                            .Bold().FontColor(payment.Odendi ? "#27ae60" : "#e74c3c");
                    });
                });
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().LineHorizontal(1).LineColor("#e2e8f0");

                col.Item().PaddingTop(8).Row(row =>
                {
                    row.RelativeItem().Text("Bu makbuz Apartman Yönetim Sistemi tarafından otomatik oluşturulmuştur.")
                        .FontSize(9).FontColor("#a0aec0");

                    row.ConstantItem(80).AlignRight().Text(text =>
                    {
                        text.Span("Sayfa ").FontSize(9).FontColor("#a0aec0");
                        text.CurrentPageNumber().FontSize(9).FontColor("#a0aec0");
                    });
                });
            });
        }
    }
}