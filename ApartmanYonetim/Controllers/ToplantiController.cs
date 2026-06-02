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
    public class ToplantiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly INotificationService _notificationService;

        public ToplantiController(ApplicationDbContext context,
            UserManager<AppUser> userManager,
            INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var toplantilar = await _context.Toplantilar
                    .Include(t => t.Olusturan)
                    .OrderByDescending(t => t.Tarih)
                    .ToListAsync();

                ViewBag.Planlanan = toplantilar
                    .Count(t => t.Durum == ToplantiDurum.Planlanıyor
                             && t.Tarih >= DateTime.Now);
                ViewBag.Tamamlanan = toplantilar
                    .Count(t => t.Durum == ToplantiDurum.Tamamlandi);

                return View(toplantilar);
            }
            catch
            {
                TempData["Error"] = "Toplantılar yüklenirken hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create() => View(new ToplantiViewModel
        {
            Tarih = DateTime.Now.AddDays(7)
        });

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(ToplantiViewModel model)
        {
            ModelState.Remove("Durum");
            if (!ModelState.IsValid) return View(model);

            try
            {
                var user = await _userManager.GetUserAsync(User);
                var toplanti = new Toplanti
                {
                    Baslik = model.Baslik,
                    Aciklama = model.Aciklama,
                    Tarih = model.Tarih,
                    Konum = model.Konum,
                    Notlar = model.Notlar,
                    Durum = ToplantiDurum.Planlanıyor,
                    OlusturanId = user!.Id,
                    OlusturmaTarihi = DateTime.Now
                };

                _context.Toplantilar.Add(toplanti);
                await _context.SaveChangesAsync();

                // Tüm sakinlere bildirim
                await _notificationService.SendToAllAsync(
                    $"📅 Yeni toplantı: {model.Baslik} — " +
                    $"{model.Tarih:dd.MM.yyyy HH:mm}", "info");

                TempData["Success"] = "Toplantı oluşturuldu.";
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["Error"] = "Toplantı oluşturulurken hata oluştu.";
                return View(model);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var toplanti = await _context.Toplantilar.FindAsync(id);
            if (toplanti == null)
            {
                TempData["Error"] = "Toplantı bulunamadı.";
                return RedirectToAction("Index");
            }

            var model = new ToplantiViewModel
            {
                Id = toplanti.Id,
                Baslik = toplanti.Baslik,
                Aciklama = toplanti.Aciklama,
                Tarih = toplanti.Tarih,
                Konum = toplanti.Konum,
                Notlar = toplanti.Notlar,
                Durum = toplanti.Durum
            };
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(ToplantiViewModel model)
        {
            ModelState.Remove("Durum");
            if (!ModelState.IsValid) return View(model);

            try
            {
                var toplanti = await _context.Toplantilar.FindAsync(model.Id);
                if (toplanti == null)
                {
                    TempData["Error"] = "Toplantı bulunamadı.";
                    return RedirectToAction("Index");
                }

                toplanti.Baslik = model.Baslik;
                toplanti.Aciklama = model.Aciklama;
                toplanti.Tarih = model.Tarih;
                toplanti.Konum = model.Konum;
                toplanti.Notlar = model.Notlar;
                toplanti.Durum = model.Durum;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Toplantı güncellendi.";
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["Error"] = "Toplantı güncellenirken hata oluştu.";
                return View(model);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var toplanti = await _context.Toplantilar.FindAsync(id);
                if (toplanti == null)
                {
                    TempData["Error"] = "Toplantı bulunamadı.";
                    return RedirectToAction("Index");
                }

                _context.Toplantilar.Remove(toplanti);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Toplantı silindi.";
            }
            catch
            {
                TempData["Error"] = "Toplantı silinirken hata oluştu.";
            }
            return RedirectToAction("Index");
        }
    }
}