using ApartmanYonetim.Data;
using ApartmanYonetim.Models.Entities;
using ApartmanYonetim.Models.ViewModels;
using ApartmanYonetim.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApartmanYonetim.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notificationService;
        private readonly IBlockAuthService _blockAuthService;

        public PaymentController(ApplicationDbContext context, UserManager<AppUser> userManager, IAuditService auditService, INotificationService notificationService, IBlockAuthService blockAuthService)
        {
            _context = context;
            _userManager = userManager;
            _auditService = auditService;
            _notificationService = notificationService;
            _blockAuthService = blockAuthService;
        }

        // ───── Renk yardımcı metot ─────
        private static (string rowClass, string durum, string durumClass) GetStyle(
            bool odendi, decimal kalan, DateTime sonTarih)
        {
            if (odendi || kalan <= 0)
                return ("row-yesil", "Ödendi", "badge bg-success");

            var fark = (sonTarih.Date - DateTime.Today).TotalDays;

            if (fark < 0)
                return ("row-kirmizi", "Gecikmiş", "badge bg-danger");
            if (fark <= 3)
                return ("row-kirmizi", "Acil", "badge bg-danger");
            if (fark <= 5)
                return ("row-sari", "Yaklaşıyor", "badge bg-warning text-dark");
            // kalan < toplam demek kısmi ödeme yapılmış
            return ("row-turuncu", "Bekliyor", "badge bg-secondary");

        }

        // ───── ADMİN: TÜM ÖDEMELER ─────
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var isAdmin = User.IsInRole("Admin");
                var user = await _userManager.GetUserAsync(User);

                if (!isAdmin)
                    return RedirectToAction("MyPayments");

                var userId = user!.Id;
                var yetkiliBloklar = await _blockAuthService
                    .GetYetkiliBlokIdsAsync(userId);

                var query = _context.Payments
                    .Include(p => p.Apartment).ThenInclude(a => a.Block)
                    .Include(p => p.Transactions)
                    .AsQueryable();

                // Blok kısıtlaması
                if (yetkiliBloklar.Any())
                {
                    query = query.Where(p =>
                        p.Apartment != null &&
                        p.Apartment.BlockId != null &&
                        yetkiliBloklar.Contains(p.Apartment.BlockId.Value));
                }

                var odemeler = await query
                    .OrderByDescending(p => p.SonOdemeTarihi)
                    .ToListAsync();


                var model = odemeler.Select(p =>
                {
                    var odenen = p.OdenenTutar;
                    var kalan = p.KalanTutar;

                    var (rc, d, dc) = GetStyle(p.Odendi, kalan, p.SonOdemeTarihi);

                    return new PaymentListViewModel
                    {
                        Id = p.Id,
                        BlokAdi = p.Apartment?.Block?.Ad,
                        DaireNo = p.Apartment?.DaireNo ?? 0,
                        Aciklama = p.Aciklama,
                        Tutar = p.Tutar,
                        OdenenTutar = odenen,
                        Kalan = kalan,
                        SonOdemeTarihi = p.SonOdemeTarihi,
                        Odendi = p.Odendi,
                        RowClass = rc,
                        Durum = d,
                        DurumClass = dc,
                        TransactionSayisi = p.Transactions?.Count ?? 0
                    };
                }).ToList();


                // ViewBag istatistikleri...
                ViewBag.ToplamBorc = odemeler
                    .Where(p => !p.Odendi).Sum(p => p.KalanTutar);
                ViewBag.ToplamTahsilat = odemeler.Sum(p => p.OdenenTutar);
                ViewBag.OdenmeyenSayi = odemeler.Count(p => !p.Odendi);
                ViewBag.OdenenSayi = odemeler.Count(p => p.Odendi);

                return View(model);
            }
            catch
            {
                TempData["Error"] = "Ödemeler yüklenirken hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }

        // ───── SAKİN: KENDİ ÖDEMELERİ ─────
        public async Task<IActionResult> MyPayments()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.ApartmentId == null)
            {
                ViewBag.Mesaj = "Hesabınıza tanımlı bir daire bulunamadı.";
                return View(new List<PaymentListViewModel>());
            }

            var odemeler = await _context.Payments
                .Include(p => p.Apartment).ThenInclude(a => a.Block)
                .Include(p => p.Transactions)
                .Where(p => p.ApartmentId == user.ApartmentId)
                .OrderByDescending(p => p.SonOdemeTarihi)
                .ToListAsync();

            var model = odemeler.Select(p =>
            {
                var odenen = p.OdenenTutar;
                var kalan = p.KalanTutar;
                var (rc, d, dc) = GetStyle(p.Odendi, kalan, p.SonOdemeTarihi);
                return new PaymentListViewModel
                {
                    Id = p.Id,
                    BlokAdi = p.Apartment?.Block?.Ad,
                    DaireNo = p.Apartment.DaireNo,
                    Aciklama = p.Aciklama,
                    Tutar = p.Tutar,
                    OdenenTutar = odenen,
                    Kalan = kalan,
                    SonOdemeTarihi = p.SonOdemeTarihi,
                    Odendi = p.Odendi,
                    RowClass = rc,
                    Durum = d,
                    DurumClass = dc,
                    TransactionSayisi = p.Transactions.Count
                };
            }).ToList();

            ViewBag.ToplamBorc = odemeler
            .Where(p => !p.Odendi)
            .Sum(p => p.KalanTutar);

            ViewBag.ToplamTahsilat = odemeler.Sum(p => p.OdenenTutar);

            return View(model);
        }

        // ───── ÖDEME DETAY (transaction listesi) ─────
        public async Task<IActionResult> Detail(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            var payment = await _context.Payments
                .Include(p => p.Apartment).ThenInclude(a => a.Block)
                .Include(p => p.Transactions)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null) return NotFound();

            // Sakin sadece kendi dairesini görebilir
            if (!isAdmin && payment.ApartmentId != user!.ApartmentId)
                return Forbid();

            var odenen = payment.OdenenTutar;
            var kalan = payment.KalanTutar;
            var (rc, d, _) = GetStyle(payment.Odendi, kalan, payment.SonOdemeTarihi);

            var model = new PaymentDetailViewModel
            {
                Id = payment.Id,
                BlokAdi = payment.Apartment?.Block?.Ad ?? "-",
                DaireNo = payment.Apartment.DaireNo,
                Aciklama = payment.Aciklama,
                Tutar = payment.Tutar,
                OdenenTutar = odenen,
                KalanTutar = kalan,
                SonOdemeTarihi = payment.SonOdemeTarihi,
                Odendi = payment.Odendi,
                RowClass = rc,
                Durum = d,
                Transactions = payment.Transactions
                    .OrderByDescending(t => t.OdemeTarihi)
                    .Select(t => new TransactionViewModel
                    {
                        Id = t.Id,
                        PaymentId = t.PaymentId,
                        OdenenTutar = t.OdenenTutar,
                        OdemeTarihi = t.OdemeTarihi,
                        Aciklama = t.Aciklama,
                        OdeyenKisi = t.OdeyenKisi
                    }).ToList()
            };

            return View(model);
        }

        // ───── ÖDEME EKLE (transaction) ─────
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> AddTransaction(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Apartment).ThenInclude(a => a.Block)
                .Include(p => p.Transactions)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null) return NotFound();

            ViewBag.Payment = payment;
            ViewBag.KalanTutar = payment.KalanTutar;

            var model = new TransactionViewModel
            {
                PaymentId = id,
                OdemeTarihi = DateTime.Today,
                OdenenTutar = payment.KalanTutar // varsayılan: kalan tutar
            };
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddTransaction(TransactionViewModel model)
        {
            var payment = await _context.Payments
                .Include(p => p.Transactions)
                .FirstOrDefaultAsync(p => p.Id == model.PaymentId);

            if (payment == null) return NotFound();

            // Kalan tutardan fazla ödeme yapılamaz
            var kalan = payment.KalanTutar;
            if (model.OdenenTutar > kalan)
            {
                ModelState.AddModelError("OdenenTutar",
                    $"Ödeme tutarı kalan borçtan ({kalan:N2} ₺) fazla olamaz.");
                ViewBag.Payment = payment;
                ViewBag.KalanTutar = kalan;
                return View(model);
            }

            var transaction = new PaymentTransaction
            {
                PaymentId = model.PaymentId,
                OdenenTutar = model.OdenenTutar,
                OdemeTarihi = model.OdemeTarihi,
                Aciklama = model.Aciklama,
                OdeyenKisi = model.OdeyenKisi
            };

            _context.PaymentTransactions.Add(transaction);

            // Tüm ödemeler tamamlandıysa Odendi = true
            var yeniOdenen = payment.OdenenTutar + model.OdenenTutar;
            if (yeniOdenen >= payment.Tutar)
                payment.Odendi = true;

            await _context.SaveChangesAsync();


            // Sakine bildirim gönder
            var sakinUser = await _context.Users
                .FirstOrDefaultAsync(u => u.ApartmentId == payment.ApartmentId);

            if (sakinUser != null)
            {
                await _notificationService.SendToUserAsync(
                    sakinUser.Id,
                    $"{model.OdenenTutar:N2} ₺ ödemeniz kaydedildi. Kalan: {payment.KalanTutar:N2} ₺",
                    "success"
                );

                // Sakin sayfasını yenile
                await _notificationService.RefreshUserDataAsync(sakinUser.Id, "payments");
            }

            // Admine bildirim gönder
            await _notificationService.SendToAdminAsync(
                $"Daire {payment.Apartment?.DaireNo} — {model.OdenenTutar:N2} ₺ ödeme alındı.",
                "info"
            );

            var user = await _userManager.GetUserAsync(User);
            await _auditService.LogAsync(
                user!.Id,
                $"{user.Ad} {user.Soyad}",
                "Ödeme Eklendi",
                "Payment",
                null,
                $"Daire {payment.Apartment?.DaireNo} — {model.OdenenTutar:N2} ₺",
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            TempData["Success"] = $"{model.OdenenTutar:N2} ₺ ödeme kaydedildi.";
            return RedirectToAction("Detail", new { id = model.PaymentId });
        }

        // ───── TRANSACTION SİL ─────
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var transaction = await _context.PaymentTransactions
                .Include(t => t.Payment)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null) return NotFound();

            var paymentId = transaction.PaymentId;
            _context.PaymentTransactions.Remove(transaction);

            // Odendi durumunu güncelle
            var payment = await _context.Payments
                .Include(p => p.Transactions)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment != null)
            {
                var yeniOdenen = payment.Transactions
                    .Where(t => t.Id != id)
                    .Sum(t => t.OdenenTutar);
                payment.Odendi = yeniOdenen >= payment.Tutar;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Ödeme hareketi silindi.";
            return RedirectToAction("Detail", new { id = paymentId });
        }

        // ───── ANA BORÇ EKLE ─────
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Daireler = await _context.Apartments
                .Include(a => a.Block)
                .OrderBy(a => a.Block!.Ad)
                .ThenBy(a => a.DaireNo)
                .ToListAsync();
            return View(new PaymentViewModel());
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(PaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Daireler = await _context.Apartments
                    .Include(a => a.Block).OrderBy(a => a.DaireNo).ToListAsync();
                return View(model);
            }

            var payment = new Payment
            {
                ApartmentId = model.ApartmentId,
                Aciklama = model.Aciklama,
                Tutar = model.Tutar,
                SonOdemeTarihi = model.SonOdemeTarihi,
                Odendi = false
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Ödeme kaydı oluşturuldu.";
            return RedirectToAction("Index");
        }

        // ───── ANA BORÇ SİL ─────
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Transactions)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null) return NotFound();

            _context.PaymentTransactions.RemoveRange(payment.Transactions);
            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Ödeme ve tüm hareketleri silindi.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();

            ViewBag.Daireler = await _context.Apartments
                .Include(a => a.Block)
                .OrderBy(a => a.Block!.Ad)
                .ThenBy(a => a.DaireNo)
                .ToListAsync();

            var model = new PaymentViewModel
            {
                Id = payment.Id,
                ApartmentId = payment.ApartmentId,
                Aciklama = payment.Aciklama,
                Tutar = payment.Tutar,
                SonOdemeTarihi = payment.SonOdemeTarihi
            };
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(PaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Daireler = await _context.Apartments
                    .Include(a => a.Block)
                    .OrderBy(a => a.DaireNo)
                    .ToListAsync();
                return View(model);
            }

            var payment = await _context.Payments.FindAsync(model.Id);
            if (payment == null) return NotFound();

            payment.ApartmentId = model.ApartmentId;
            payment.Aciklama = model.Aciklama;
            payment.Tutar = model.Tutar;
            payment.SonOdemeTarihi = model.SonOdemeTarihi;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Ödeme güncellendi.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Calendar()
        {
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            var query = _context.Payments
                .Include(p => p.Apartment).ThenInclude(a => a.Block)
                .Include(p => p.Transactions)
                .AsQueryable();

            if (!isAdmin)
                query = query.Where(p => p.ApartmentId == user!.ApartmentId);

            var odemeler = await query
                .Where(p => p.SonOdemeTarihi.Year == DateTime.Now.Year)
                .ToListAsync();

            var events = odemeler.Select(p => new
            {
                id = p.Id,
                title = $"Daire {p.Apartment?.DaireNo} — {p.Aciklama}",
                start = p.SonOdemeTarihi.ToString("yyyy-MM-dd"),
                color = p.Odendi || p.KalanTutar <= 0 ? "#27ae60" :
                        (p.SonOdemeTarihi.Date - DateTime.Today).TotalDays <= 3 ? "#e74c3c" :
                        (p.SonOdemeTarihi.Date - DateTime.Today).TotalDays <= 5 ? "#f39c12" :
                        "#2d6a9f",
                tutar = p.Tutar.ToString("N2"),
                kalan = p.KalanTutar.ToString("N2"),
                odendi = p.Odendi
            }).ToList();

            ViewBag.Events = System.Text.Json.JsonSerializer.Serialize(events);
            return View();
        }

        // Sakin için güncel ödeme listesini JSON olarak döndür
        [HttpGet]
        public async Task<IActionResult> GetMyPaymentsJson()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.ApartmentId == null)
                return Json(new { success = false });

            var odemeler = await _context.Payments
                .Include(p => p.Apartment).ThenInclude(a => a.Block)
                .Include(p => p.Transactions)
                .Where(p => p.ApartmentId == user.ApartmentId)
                .OrderByDescending(p => p.SonOdemeTarihi)
                .ToListAsync();

            var bugun = DateTime.Today;

            var result = odemeler.Select(p =>
            {
                var odenen = p.OdenenTutar;
                var kalan = p.KalanTutar;
                var fark = (p.SonOdemeTarihi.Date - bugun).TotalDays;

                string durum, durumClass, rowClass;
                if (p.Odendi || kalan <= 0)
                {
                    durum = "Ödendi"; durumClass = "badge bg-success";
                    rowClass = "row-yesil";
                }
                else if (fark < 0)
                {
                    durum = "Gecikmiş"; durumClass = "badge bg-danger";
                    rowClass = "row-kirmizi";
                }
                else if (fark <= 3)
                {
                    durum = "Acil"; durumClass = "badge bg-danger";
                    rowClass = "row-kirmizi";
                }
                else if (fark <= 5)
                {
                    durum = "Yaklaşıyor"; durumClass = "badge bg-warning text-dark";
                    rowClass = "row-sari";
                }
                else
                {
                    durum = "Bekliyor"; durumClass = "badge bg-secondary";
                    rowClass = "row-turuncu";
                }

                return new
                {
                    id = p.Id,
                    aciklama = p.Aciklama,
                    tutar = p.Tutar.ToString("N2"),
                    odenenTutar = odenen.ToString("N2"),
                    kalanTutar = kalan.ToString("N2"),
                    sonOdemeTarihi = p.SonOdemeTarihi.ToString("dd.MM.yyyy"),
                    odendi = p.Odendi,
                    transactionSayisi = p.Transactions.Count,
                    durum,
                    durumClass,
                    rowClass
                };
            }).ToList();

            return Json(new
            {
                success = true,
                toplamBorc = odemeler.Where(p => !p.Odendi).Sum(p => p.KalanTutar),
                toplamTahsilat = odemeler.Sum(p => p.OdenenTutar),
                odemeler = result
            });
        }

        // Admin için tüm ödemeleri JSON döndür
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllPaymentsJson()
        {
            var odemeler = await _context.Payments
                .Include(p => p.Apartment).ThenInclude(a => a.Block)
                .Include(p => p.Transactions)
                .OrderByDescending(p => p.SonOdemeTarihi)
                .ToListAsync();

            return Json(new
            {
                success = true,
                toplamBorc = odemeler.Where(p => !p.Odendi).Sum(p => p.KalanTutar),
                toplamTahsilat = odemeler.Sum(p => p.OdenenTutar),
                odenmeyenSayi = odemeler.Count(p => !p.Odendi),
                odenenSayi = odemeler.Count(p => p.Odendi)
            });
        }

    }
}