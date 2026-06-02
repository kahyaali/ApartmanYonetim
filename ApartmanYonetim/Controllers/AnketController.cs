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
    public class AnketController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly INotificationService _notificationService;

        public AnketController(ApplicationDbContext context,
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
                var user = await _userManager.GetUserAsync(User);
                var anketler = await _context.Anketler
                    .Include(a => a.Siklar)
                    .Include(a => a.Oylar)
                    .OrderByDescending(a => a.OlusturmaTarihi)
                    .ToListAsync();

                var oyVerilenAnketler = await _context.AnketOylari
                    .Where(o => o.KullaniciId == user!.Id)
                    .Select(o => o.AnketId)
                    .ToListAsync();

                ViewBag.OyVerilenAnketler = oyVerilenAnketler;
                return View(anketler);
            }
            catch
            {
                TempData["Error"] = "Anketler yüklenirken hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create() => View(new AnketViewModel
        {
            BitisTarihi = DateTime.Now.AddDays(7),
            Siklar = new List<string> { "", "" }
        });

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(AnketViewModel model)
        {
            ModelState.Remove("Siklar");
            var gecerliSiklar = model.Siklar?
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList() ?? new List<string>();

            if (gecerliSiklar.Count < 2)
            {
                ModelState.AddModelError("Siklar",
                    "En az 2 geçerli seçenek giriniz.");
                return View(model);
            }

            if (!ModelState.IsValid) return View(model);

            try
            {
                var user = await _userManager.GetUserAsync(User);
                var anket = new Anket
                {
                    Baslik = model.Baslik,
                    Aciklama = model.Aciklama,
                    BaslangicTarihi = DateTime.Now,
                    BitisTarihi = model.BitisTarihi,
                    AktifMi = true,
                    OlusturanId = user!.Id,
                    OlusturmaTarihi = DateTime.Now
                };

                _context.Anketler.Add(anket);
                await _context.SaveChangesAsync();

                for (int i = 0; i < gecerliSiklar.Count; i++)
                {
                    _context.AnketSiklari.Add(new AnketSik
                    {
                        AnketId = anket.Id,
                        Metin = gecerliSiklar[i],
                        SiraNo = i + 1
                    });
                }
                await _context.SaveChangesAsync();

                await _notificationService.SendToAllAsync(
                    $"🗳️ Yeni anket: {model.Baslik} — Oyunuzu kullanın!", "info");

                TempData["Success"] = "Anket oluşturuldu.";
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["Error"] = "Anket oluşturulurken hata oluştu.";
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> OyVer(int anketId, int sikId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var anket = await _context.Anketler.FindAsync(anketId);

                if (anket == null || !anket.AktifMi ||
                    anket.BitisTarihi < DateTime.Now)
                {
                    TempData["Error"] = "Bu anket artık aktif değil.";
                    return RedirectToAction("Index");
                }

                var oyVar = await _context.AnketOylari
                    .AnyAsync(o => o.AnketId == anketId
                               && o.KullaniciId == user!.Id);

                if (oyVar)
                {
                    TempData["Error"] = "Bu ankete zaten oy verdiniz.";
                    return RedirectToAction("Index");
                }

                var oy = new AnketOy
                {
                    AnketId = anketId,
                    AnketSikId = sikId,
                    KullaniciId = user!.Id,
                    OyTarihi = DateTime.Now
                };

                _context.AnketOylari.Add(oy);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Oyunuz kaydedildi.";
            }
            catch
            {
                TempData["Error"] = "Oy kaydedilemedi.";
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var anket = await _context.Anketler
                    .Include(a => a.Siklar)
                    .Include(a => a.Oylar)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (anket == null)
                {
                    TempData["Error"] = "Anket bulunamadı.";
                    return RedirectToAction("Index");
                }

                _context.AnketOylari.RemoveRange(anket.Oylar);
                _context.AnketSiklari.RemoveRange(anket.Siklar);
                _context.Anketler.Remove(anket);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Anket silindi.";
            }
            catch
            {
                TempData["Error"] = "Anket silinirken hata oluştu.";
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ToggleAktif(int id)
        {
            var anket = await _context.Anketler.FindAsync(id);
            if (anket == null)
            {
                TempData["Error"] = "Anket bulunamadı.";
                return RedirectToAction("Index");
            }
            anket.AktifMi = !anket.AktifMi;
            await _context.SaveChangesAsync();
            TempData["Success"] = anket.AktifMi
                ? "Anket aktifleştirildi."
                : "Anket kapatıldı.";
            return RedirectToAction("Index");
        }
    }
}