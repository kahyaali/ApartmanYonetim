using ApartmanYonetim.Data;
using ApartmanYonetim.Models.Entities;
using ApartmanYonetim.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApartmanYonetim.Controllers
{
    [Authorize]
    public class MesajController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly INotificationService _notificationService;

        public MesajController(ApplicationDbContext context,
            UserManager<AppUser> userManager,
            INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        // Gelen kutusu
        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var isAdmin = User.IsInRole("Admin");

                // Konuşmalar — son mesaja göre grupla
                var mesajlar = await _context.Mesajlar
                    .Include(m => m.Gonderen)
                    .Include(m => m.Alici)
                    .Where(m => (m.AliciId == user!.Id && !m.AliciSildi)
                             || (m.GonderenId == user!.Id && !m.GonderenSildi))
                    .OrderByDescending(m => m.GonderimTarihi)
                    .ToListAsync();

                // Konuşma partnerleri
                var konusmalar = mesajlar
                    .GroupBy(m => m.GonderenId == user!.Id ? m.AliciId : m.GonderenId)
                    .Select(g => new
                    {
                        PartnerId = g.Key,
                        SonMesaj = g.OrderByDescending(m => m.GonderimTarihi).First(),
                        OkunmamişSayi = g.Count(m => m.AliciId == user!.Id && !m.Okundu)
                    })
                    .ToList();

                // Partner bilgilerini yükle
                var partnerIds = konusmalar.Select(k => k.PartnerId).ToList();
                var partnerler = await _userManager.Users
                    .Where(u => partnerIds.Contains(u.Id))
                    .ToListAsync();

                ViewBag.Konusmalar = konusmalar.Select(k => new
                {
                    k.PartnerId,
                    k.SonMesaj,
                    k.OkunmamişSayi,
                    Partner = partnerler.FirstOrDefault(p => p.Id == k.PartnerId)
                }).ToList();

                ViewBag.ToplamOkunmamis = mesajlar.Count(m =>
                    m.AliciId == user!.Id && !m.Okundu);

                // Admin ise kullanıcı listesi
                if (isAdmin)
                {
                    ViewBag.Kullanicilar = await _userManager.Users
                        .Where(u => u.Id != user!.Id)
                        .Include(u => u.Apartment)
                        .ToListAsync();
                }

                return View();
            }
            catch
            {
                TempData["Error"] = "Mesajlar yüklenirken hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }

        // Konuşma detayı
        public async Task<IActionResult> Konusma(string partnerId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var partner = await _userManager.FindByIdAsync(partnerId);

                if (partner == null)
                {
                    TempData["Error"] = "Kullanıcı bulunamadı.";
                    return RedirectToAction("Index");
                }



                var mesajlar = await _context.Mesajlar
    .Include(m => m.Gonderen)
    .Where(m =>
        (m.GonderenId == user!.Id &&
         m.AliciId == partner.Id &&
         !m.GonderenSildi)
        ||
        (m.GonderenId == partner.Id &&
         m.AliciId == user!.Id &&
         !m.AliciSildi))
    .OrderBy(m => m.GonderimTarihi)
    .ToListAsync();

                // Okunmamışları okundu yap
                var okunmamıslar = mesajlar
                    .Where(m => m.AliciId == user!.Id && !m.Okundu)
                    .ToList();

                foreach (var m in okunmamıslar)
                {
                    m.Okundu = true;
                    m.OkunmaTarihi = DateTime.Now;
                }
                if (okunmamıslar.Any())
                    await _context.SaveChangesAsync();

                ViewBag.Partner = partner;
                ViewBag.BenimId = user!.Id;
                return View(mesajlar);
            }
            catch
            {
                TempData["Error"] = "Konuşma yüklenirken hata oluştu.";
                return RedirectToAction("Index");
            }
        }

        // Mesaj gönder
        [HttpPost]
        public async Task<IActionResult> Gonder(string aliciId, string icerik)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(icerik))
                    return RedirectToAction("Konusma", new { partnerId = aliciId });

                var user = await _userManager.GetUserAsync(User);
                var alici = await _userManager.FindByIdAsync(aliciId);

                if (alici == null)
                {
                    TempData["Error"] = "Alıcı bulunamadı.";
                    return RedirectToAction("Index");
                }

                var mesaj = new Mesaj
                {
                    GonderenId = user!.Id,
                    AliciId = aliciId,
                    Icerik = icerik.Trim(),
                    GonderimTarihi = DateTime.Now,
                    Okundu = false
                };

                _context.Mesajlar.Add(mesaj);
                await _context.SaveChangesAsync();

                // SignalR ile anlık bildirim
                await _notificationService.SendToUserAsync(
                    aliciId,
                    $"📨 {user.Ad} {user.Soyad}: {icerik.Substring(0, Math.Min(icerik.Length, 50))}",
                    "info"
                );

                return RedirectToAction("Konusma", new { partnerId = aliciId });
            }
            catch
            {
                TempData["Error"] = "Mesaj gönderilemedi.";
                return RedirectToAction("Konusma", new { partnerId = aliciId });
            }
        }

        // Yeni konuşma başlat (Admin → Sakin)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> YeniKonusma(string aliciId)
        {
            return RedirectToAction("Konusma", new { partnerId = aliciId });
        }

        // Okunmamış mesaj sayısı (AJAX)
        [HttpGet]
        public async Task<IActionResult> OkunmamisSayi()
        {
            var user = await _userManager.GetUserAsync(User);
            var sayi = await _context.Mesajlar
                .CountAsync(m => m.AliciId == user!.Id && !m.Okundu);
            return Json(new { sayi });
        }


        //======= Sayfayı refresh etmeden mesaj güncelleniyor ======//

        // AJAX mesaj gönder
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> GonderAjax(string aliciId, string icerik)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(icerik))
                    return Json(new { success = false, mesaj = "Mesaj boş olamaz." });

                var user = await _userManager.GetUserAsync(User);
                var alici = await _userManager.FindByIdAsync(aliciId);

                if (alici == null)
                    return Json(new { success = false, mesaj = "Alıcı bulunamadı." });

                var mesaj = new Mesaj
                {
                    GonderenId = user!.Id,
                    AliciId = aliciId,
                    Icerik = icerik.Trim(),
                    GonderimTarihi = DateTime.Now,
                    Okundu = false
                };

                _context.Mesajlar.Add(mesaj);
                await _context.SaveChangesAsync();

                // SignalR ile anlık bildirim
                await _notificationService.SendToUserAsync(
                    aliciId,
                    $"📨 {user.Ad} {user.Soyad}: {icerik.Substring(0, Math.Min(icerik.Length, 50))}",
                    "info"
                );

                return Json(new
                {
                    success = true,
                    id = mesaj.Id,
                    icerik = mesaj.Icerik,
                    gonderimTarihi = mesaj.GonderimTarihi.ToString("dd.MM.yyyy HH:mm"),
                    gonderenId = mesaj.GonderenId
                });
            }
            catch
            {
                return Json(new { success = false, mesaj = "Mesaj gönderilemedi." });
            }
        }


        // Yeni mesajları getir (polling)
        [HttpGet]
        public async Task<IActionResult> YeniMesajlar(string partnerId, int sonMesajId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                var mesajlar = await _context.Mesajlar
                    .Where(m => m.Id > sonMesajId &&
                        ((m.GonderenId == partnerId && m.AliciId == user!.Id && !m.AliciSildi) ||
                         (m.GonderenId == user!.Id && m.AliciId == partnerId && !m.GonderenSildi)))
                    .OrderBy(m => m.GonderimTarihi)
                    .ToListAsync();

                // Gelen mesajları okundu yap
                var okunmamıslar = mesajlar
                    .Where(m => m.AliciId == user!.Id && !m.Okundu)
                    .ToList();

                foreach (var m in okunmamıslar)
                {
                    m.Okundu = true;
                    m.OkunmaTarihi = DateTime.Now;
                }
                if (okunmamıslar.Any())
                    await _context.SaveChangesAsync();

                var result = mesajlar.Select(m => new
                {
                    id = m.Id,
                    icerik = m.Icerik,
                    gonderimTarihi = m.GonderimTarihi.ToString("dd.MM.yyyy HH:mm"),
                    gonderenId = m.GonderenId,
                    okundu = m.Okundu,
                    gonderenSildi = m.GonderenSildi, 
                    aliciSildi = m.AliciSildi       
                }).ToList();

                return Json(result);
            }
            catch
            {
                return Json(new List<object>());
            }
        }

        // Tek mesaj sil
        [HttpPost]
        public async Task<IActionResult> MesajSil(int mesajId, string partnerId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var mesaj = await _context.Mesajlar.FindAsync(mesajId);

                if (mesaj == null)
                {
                    TempData["Error"] = "Mesaj bulunamadı.";
                    return RedirectToAction("Konusma", new { partnerId });
                }

                // Sadece gönderen veya alıcı silebilir
                if (mesaj.GonderenId == user!.Id)
                    mesaj.GonderenSildi = true;
                else if (mesaj.AliciId == user.Id)
                    mesaj.AliciSildi = true;
                else
                    return Forbid();

                // Her iki taraf da sildiyse DB'den tamamen kaldır
                if (mesaj.GonderenSildi && mesaj.AliciSildi)
                    _context.Mesajlar.Remove(mesaj);

                await _context.SaveChangesAsync();
                return RedirectToAction("Konusma", new { partnerId });
            }
            catch
            {
                TempData["Error"] = "Mesaj silinemedi.";
                return RedirectToAction("Konusma", new { partnerId });
            }
        }

        // Tüm konuşmayı sil
        [HttpPost]
        public async Task<IActionResult> KonusmaSil(string partnerId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                var mesajlar = await _context.Mesajlar
                    .Where(m =>
                        (m.GonderenId == user!.Id && m.AliciId == partnerId) ||
                        (m.GonderenId == partnerId && m.AliciId == user!.Id))
                    .ToListAsync();

                foreach (var m in mesajlar)
                {
                    if (m.GonderenId == user!.Id)
                        m.GonderenSildi = true;
                    else
                        m.AliciSildi = true;
                }

                // Her iki tarafça silinen mesajları DB'den kaldır
                var tamamenSilincekler = mesajlar
                    .Where(m => m.GonderenSildi && m.AliciSildi)
                    .ToList();

                if (tamamenSilincekler.Any())
                    _context.Mesajlar.RemoveRange(tamamenSilincekler);

                await _context.SaveChangesAsync();
                TempData["Success"] = "Konuşma silindi.";
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["Error"] = "Konuşma silinemedi.";
                return RedirectToAction("Index");
            }
        }
    }
}