using ApartmanYonetim.Data;
using ApartmanYonetim.Models.Entities;
using ApartmanYonetim.Models.ViewModels;
using ApartmanYonetim.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApartmanYonetim.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AidatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public AidatController(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var tanimlar = await _context.AidatTanimlari
                    .Include(a => a.Payments)
                    .OrderByDescending(a => a.Yil)
                    .ThenByDescending(a => a.Ay)
                    .ToListAsync();
                return View(tanimlar);
            }
            catch
            {
                TempData["Error"] = "Aidat tanımları yüklenirken hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new AidatTanimiViewModel
            {
                Ay = DateTime.Now.Month,
                Yil = DateTime.Now.Year,
                SonOdemeTarihi = new DateTime(
                    DateTime.Now.Year,
                    DateTime.Now.Month,
                    DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month))
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(AidatTanimiViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var varMi = await _context.AidatTanimlari
                    .AnyAsync(a => a.Baslik == model.Baslik
                                && a.Ay == model.Ay
                                && a.Yil == model.Yil);

                if (varMi)
                {
                    ModelState.AddModelError("",
                        $"{model.Yil}/{model.Ay} için '{model.Baslik}' zaten tanımlanmış.");
                    return View(model);
                }

                var tanimla = new AidatTanimi
                {
                    Baslik = model.Baslik,
                    Tutar = model.Tutar,
                    Ay = model.Ay,
                    Yil = model.Yil,
                    SonOdemeTarihi = model.SonOdemeTarihi,
                    TumDairelereUygula = model.TumDairelereUygula,
                    OlusturmaTarihi = DateTime.Now
                };

                _context.AidatTanimlari.Add(tanimla);
                await _context.SaveChangesAsync();

                if (model.TumDairelereUygula)
                {
                    var doluDaireler = await _context.Apartments
                        .Where(a => a.Dolu)
                        .ToListAsync();

                    foreach (var daire in doluDaireler)
                    {
                        var payment = new Payment
                        {
                            ApartmentId = daire.Id,
                            AidatTanimiId = tanimla.Id,
                            Aciklama = $"{model.Baslik} - {model.Yil}/{model.Ay:D2}",
                            Tutar = model.Tutar,
                            SonOdemeTarihi = model.SonOdemeTarihi,
                            Odendi = false
                        };
                        _context.Payments.Add(payment);
                    }

                    await _context.SaveChangesAsync();

                    // ── SignalR: Her dairedeki kullanıcıya bildirim gönder ──
                    var daireSakinleri = await _context.Users
                        .Where(u => u.ApartmentId != null && doluDaireler
                            .Select(d => (int?)d.Id)
                            .Contains(u.ApartmentId))
                        .ToListAsync();

                    foreach (var sakin in daireSakinleri)
                    {
                        await _notificationService.SendToUserAsync(
                            sakin.Id,
                            $"📋 Yeni aidat tanımlandı: {model.Baslik} — {model.Tutar:N2} ₺ " +
                            $"(Son tarih: {model.SonOdemeTarihi:dd.MM.yyyy})",
                            "warning"
                        );

                        // Sayfayı yenilemeden veriyi güncelle komutu gönder
                        await _notificationService.RefreshUserDataAsync(sakin.Id, "payments");
                    }


                    TempData["Success"] =
                        $"Aidat tanımlandı. {doluDaireler.Count} daireye otomatik ödeme kaydı oluşturuldu.";
                }
                else
                {
                    TempData["Success"] = "Aidat tanımı oluşturuldu.";
                }

                return RedirectToAction("Index");
            }
            catch
            {
                TempData["Error"] = "Aidat tanımlanırken hata oluştu.";
                return View(model);
            }
        }

        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var tanimla = await _context.AidatTanimlari
                    .Include(a => a.Payments)
                        .ThenInclude(p => p.Apartment)
                            .ThenInclude(a => a!.Block)
                    .Include(a => a.Payments)
                        .ThenInclude(p => p.Transactions)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (tanimla == null)
                {
                    TempData["Error"] = "Aidat tanımı bulunamadı.";
                    return RedirectToAction("Index");
                }

                ViewBag.OdenenSayi = tanimla.Payments.Count(p => p.Odendi);
                ViewBag.OdenmeyenSayi = tanimla.Payments.Count(p => !p.Odendi);
                ViewBag.ToplamTahsilat = tanimla.Payments.Sum(p => p.OdenenTutar);
                ViewBag.ToplamBeklenen = tanimla.Payments.Sum(p => p.Tutar);

                return View(tanimla);
            }
            catch
            {
                TempData["Error"] = "Aidat detayı yüklenirken hata oluştu.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var tanimla = await _context.AidatTanimlari
                    .Include(a => a.Payments)
                        .ThenInclude(p => p.Transactions)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (tanimla == null)
                {
                    TempData["Error"] = "Aidat tanımı bulunamadı.";
                    return RedirectToAction("Index");
                }

                foreach (var p in tanimla.Payments)
                    _context.PaymentTransactions.RemoveRange(p.Transactions);

                _context.Payments.RemoveRange(tanimla.Payments);
                _context.AidatTanimlari.Remove(tanimla);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Aidat tanımı ve bağlı ödemeler silindi.";
            }
            catch
            {
                TempData["Error"] = "Aidat silinirken hata oluştu.";
            }
            return RedirectToAction("Index");
        }
    }
}