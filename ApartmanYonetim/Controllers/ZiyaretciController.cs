
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
    public class ZiyaretciController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly INotificationService _notificationService;

        public ZiyaretciController(ApplicationDbContext context,
            UserManager<AppUser> userManager,
            INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        //public async Task<IActionResult> Index()
        //{
        //    try
        //    {
        //        var user = await _userManager.GetUserAsync(User);
        //        var isAdmin = User.IsInRole("Admin");

        //        var query = _context.Ziyaretciler
        //            .Include(z => z.Apartment)
        //                .ThenInclude(a => a!.Block)
        //            .Include(z => z.DavetEden)
        //            .AsQueryable();

        //        if (!isAdmin)
        //            query = query.Where(z => z.DavetEdenId == user!.Id);

        //        var ziyaretciler = await query
        //            .OrderByDescending(z => z.OlusturmaTarihi)
        //            .ToListAsync();

        //        ViewBag.Bekleyen = ziyaretciler
        //            .Count(z => z.Durum == ZiyaretciDurum.Bekliyor);
        //        ViewBag.Icerde = ziyaretciler
        //            .Count(z => z.Durum == ZiyaretciDurum.Icerde);
        //        ViewBag.BugunCikan = ziyaretciler
        //            .Count(z => z.Durum == ZiyaretciDurum.Cikti &&
        //                z.CikisSaati?.Date == DateTime.Today);

        //        return View(ziyaretciler);
        //    }
        //    catch
        //    {
        //        TempData["Error"] = "Ziyaretçiler yüklenirken hata oluştu.";
        //        return RedirectToAction("Index", "Home");
        //    }
        //}


        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var isAdmin = User.IsInRole("Admin");

                IQueryable<Ziyaretci> query;

                if (isAdmin)
                {
                    // Admin tümünü görür
                    query = _context.Ziyaretciler
                        .Include(z => z.Apartment).ThenInclude(a => a!.Block)
                        .Include(z => z.DavetEden)
                        .Include(z => z.Onaylayan);
                }
                else
                {
                    // Sakin: kendi dairesine gelen TÜM ziyaretçileri görür
                    // (kendisi eklesin ya da admin/güvenlik eklesin fark etmez)
                    query = _context.Ziyaretciler
                        .Include(z => z.Apartment).ThenInclude(a => a!.Block)
                        .Include(z => z.DavetEden)
                        .Include(z => z.Onaylayan)
                        .Where(z => z.ApartmentId == user!.ApartmentId);
                }

                var ziyaretciler = await query
                    .OrderByDescending(z => z.OlusturmaTarihi)
                    .ToListAsync();

                ViewBag.Bekleyen = ziyaretciler
                    .Count(z => z.Durum == ZiyaretciDurum.Bekliyor);
                ViewBag.Icerde = ziyaretciler
                    .Count(z => z.Durum == ZiyaretciDurum.Icerde);
                ViewBag.BugunCikan = ziyaretciler
                    .Count(z => z.Durum == ZiyaretciDurum.Cikti &&
                        z.CikisSaati?.Date == DateTime.Today);

                return View(ziyaretciler);
            }
            catch
            {
                TempData["Error"] = "Ziyaretçiler yüklenirken hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }


        //[HttpGet]
        //public async Task<IActionResult> Ekle()
        //{
        //    var user = await _userManager.GetUserAsync(User);
        //    var isAdmin = User.IsInRole("Admin");

        //    if (isAdmin)
        //    {
        //        // Admin tüm daireleri görebilir
        //        var daireler = await _context.Apartments
        //            .Include(a => a.Block)
        //            .OrderBy(a => a.Block!.Ad)
        //            .ThenBy(a => a.DaireNo)
        //            .ToListAsync();

        //        ViewBag.Daireler = daireler.Select(d => new
        //        {
        //            d.Id,
        //            Tanim = $"{(d.Block != null ? d.Block.Ad + " — " : "")}Daire {d.DaireNo}"
        //        }).ToList();

        //        ViewBag.IsAdmin = true;
        //    }
        //    else
        //    {
        //        // Sakin kendi dairesini görür
        //        var daire = await _context.Apartments
        //            .Include(a => a.Block)
        //            .FirstOrDefaultAsync(a => a.Id == user!.ApartmentId);

        //        ViewBag.DaireBilgisi = daire != null
        //            ? $"{daire.Block?.Ad} — Daire {daire.DaireNo}"
        //            : "Daire bilgisi bulunamadı";
        //        ViewBag.IsAdmin = false;
        //    }

        //    return View(new Ziyaretci
        //    {
        //        BeklenenGirisSaati = DateTime.Now.AddHours(1),
        //        ApartmentId = user!.ApartmentId
        //    });
        //}


        [HttpGet]
        public async Task<IActionResult> Ekle()
        {
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            if (isAdmin)
            {
                // Tüm daireleri + sakin adlarını yükle
                var daireler = await _context.Apartments
                    .Include(a => a.Block)
                    .Include(a => a.Residents.Where(r => r.AktifMi))
                    .OrderBy(a => a.Block!.Ad)
                    .ThenBy(a => a.DaireNo)
                    .ToListAsync();

                ViewBag.Daireler = daireler.Select(d => new
                {
                    d.Id,
                    Tanim = $"{(d.Block != null ? d.Block.Ad + " - " : "")}Daire {d.DaireNo}" +
             (d.Residents.Any()
                 ? $" ({string.Join(", ", d.Residents.Select(r => r.Ad + " " + r.Soyad))})"
                 : " (Boş)")
                }).ToList();

                ViewBag.IsAdmin = true;
            }
            else
            {
                // Sakin: kendi daire bilgisini göster
                var daire = await _context.Apartments
                    .Include(a => a.Block)
                    .FirstOrDefaultAsync(a => a.Id == user!.ApartmentId);

                ViewBag.DaireBilgisi = daire != null
                    ? $"{daire.Block?.Ad} - Daire {daire.DaireNo}"
                    : "Daire bulunamadı";
                ViewBag.IsAdmin = false;
            }

            return View(new Ziyaretci
            {
                BeklenenGirisSaati = DateTime.Now.AddHours(1),
                ApartmentId = isAdmin ? null : user!.ApartmentId
            });
        }

        [HttpPost]
        public async Task<IActionResult> Ekle(Ziyaretci model)
        {
            ModelState.Remove("DavetEdenId");
            ModelState.Remove("DavetEden");
            ModelState.Remove("Apartment");
            ModelState.Remove("Onaylayan");

            if (!ModelState.IsValid)
            {
                // View için tekrar yükle
                var u = await _userManager.GetUserAsync(User);
                var isAdm = User.IsInRole("Admin");
                if (isAdm)
                {
                    var daireler = await _context.Apartments
                        .Include(a => a.Block)
                        .OrderBy(a => a.Block!.Ad).ThenBy(a => a.DaireNo)
                        .ToListAsync();
                    ViewBag.Daireler = daireler.Select(d => new
                    {
                        d.Id,
                        Tanim = $"{(d.Block != null ? d.Block.Ad + " — " : "")}Daire {d.DaireNo}"
                    }).ToList();
                    ViewBag.IsAdmin = true;
                }
                else
                {
                    var daire = await _context.Apartments
                        .Include(a => a.Block)
                        .FirstOrDefaultAsync(a => a.Id == u!.ApartmentId);
                    ViewBag.DaireBilgisi = daire != null
                        ? $"{daire.Block?.Ad} — Daire {daire.DaireNo}" : "";
                    ViewBag.IsAdmin = false;
                }
                return View(model);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                var isAdmin = User.IsInRole("Admin");

                model.DavetEdenId = user!.Id;
                model.Durum = ZiyaretciDurum.Bekliyor;
                model.OlusturmaTarihi = DateTime.Now;

                // Sakin ise kendi dairesini ata
                if (!isAdmin)
                    model.ApartmentId = user.ApartmentId;

                _context.Ziyaretciler.Add(model);
                await _context.SaveChangesAsync();

                await _notificationService.SendToAdminAsync(
                    $"🧑‍🤝‍🧑 Yeni ziyaretçi: {model.AdSoyad}", "info");

                TempData["Success"] = "Ziyaretçi kaydedildi.";
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["Error"] = "Ziyaretçi eklenirken hata oluştu.";
                return View(model);
            }
        }

        // Giriş yap — Admin yapar
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> GirisYap(int id)
        {
            try
            {
                var z = await _context.Ziyaretciler
                    .Include(x => x.DavetEden)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (z == null)
                {
                    TempData["Error"] = "Ziyaretçi bulunamadı.";
                    return RedirectToAction("Index");
                }

                var user = await _userManager.GetUserAsync(User);
                z.Durum = ZiyaretciDurum.Icerde;
                z.GercekGirisSaati = DateTime.Now;
                z.OnaylayanId = user!.Id;
                await _context.SaveChangesAsync();

                // Davet edene bildirim
                await _notificationService.SendToUserAsync(
                    z.DavetEdenId,
                    $"✅ Ziyaretçiniz {z.AdSoyad} binaya giriş yaptı.",
                    "success");

                TempData["Success"] = $"{z.AdSoyad} girişi onaylandı.";
            }
            catch
            {
                TempData["Error"] = "İşlem gerçekleştirilemedi.";
            }
            return RedirectToAction("Index");
        }

        // Çıkış yap — Admin yapar
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CikisYap(int id)
        {
            try
            {
                var z = await _context.Ziyaretciler
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (z == null)
                {
                    TempData["Error"] = "Ziyaretçi bulunamadı.";
                    return RedirectToAction("Index");
                }

                z.Durum = ZiyaretciDurum.Cikti;
                z.CikisSaati = DateTime.Now;
                await _context.SaveChangesAsync();

                TempData["Success"] = $"{z.AdSoyad} çıkışı kaydedildi.";
            }
            catch
            {
                TempData["Error"] = "Çıkış kaydedilemedi.";
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Reddet(int id)
        {
            try
            {
                var z = await _context.Ziyaretciler.FindAsync(id);
                if (z == null)
                {
                    TempData["Error"] = "Ziyaretçi bulunamadı.";
                    return RedirectToAction("Index");
                }

                z.Durum = ZiyaretciDurum.Reddedildi;
                await _context.SaveChangesAsync();

                await _notificationService.SendToUserAsync(
                    z.DavetEdenId,
                    $"❌ Ziyaretçi {z.AdSoyad} girişi reddedildi.",
                    "danger");

                TempData["Success"] = "Ziyaretçi reddedildi.";
            }
            catch
            {
                TempData["Error"] = "İşlem gerçekleştirilemedi.";
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Sil(int id)
        {
            try
            {
                var z = await _context.Ziyaretciler.FindAsync(id);
                if (z == null)
                {
                    TempData["Error"] = "Ziyaretçi bulunamadı.";
                    return RedirectToAction("Index");
                }
                _context.Ziyaretciler.Remove(z);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Kayıt silindi.";
            }
            catch
            {
                TempData["Error"] = "Silme işlemi başarısız.";
            }
            return RedirectToAction("Index");
        }
    }
}