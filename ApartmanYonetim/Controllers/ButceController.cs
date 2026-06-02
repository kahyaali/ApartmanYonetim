using ApartmanYonetim.Data;
using ApartmanYonetim.Models.Entities;
using ApartmanYonetim.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApartmanYonetim.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ButceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ButceController(ApplicationDbContext context,
            UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var planlar = await _context.ButcePlanlar
                    .Include(b => b.Kalemler)
                    .OrderByDescending(b => b.Yil)
                    .ToListAsync();

                return View(planlar);
            }
            catch
            {
                TempData["Error"] = "Bütçe planları yüklenirken hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult Create() => View(new ButceViewModel
        {
            Yil = DateTime.Now.Year
        });

        [HttpPost]
        public async Task<IActionResult> Create(ButceViewModel model)
        {
            ModelState.Remove("Kalemler");
            if (!ModelState.IsValid) return View(model);

            try
            {
                var user = await _userManager.GetUserAsync(User);
                var plan = new ButcePlan
                {
                    Yil = model.Yil,
                    Aciklama = model.Aciklama,
                    OlusturmaTarihi = DateTime.Now,
                    OlusturanId = user!.Id
                };

                _context.ButcePlanlar.Add(plan);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Bütçe planı oluşturuldu.";
                return RedirectToAction("Detail", new { id = plan.Id });
            }
            catch
            {
                TempData["Error"] = "Bütçe planı oluşturulurken hata oluştu.";
                return View(model);
            }
        }

        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var plan = await _context.ButcePlanlar
                    .Include(b => b.Kalemler.OrderBy(k => k.Ay).ThenBy(k => k.Tip))
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (plan == null)
                {
                    TempData["Error"] = "Bütçe planı bulunamadı.";
                    return RedirectToAction("Index");
                }

                // Aylık özet hesapla
                var aylikOzet = plan.Kalemler
                    .GroupBy(k => k.Ay)
                    .Select(g => new
                    {
                        Ay = g.Key,
                        PlanlananGelir = g.Where(k => k.Tip == ButceKalemTipi.Gelir)
                            .Sum(k => k.PlanlananTutar),
                        PlanlananGider = g.Where(k => k.Tip == ButceKalemTipi.Gider)
                            .Sum(k => k.PlanlananTutar),
                        GerceklesenGelir = g.Where(k => k.Tip == ButceKalemTipi.Gelir)
                            .Sum(k => k.GerceklesenTutar),
                        GerceklesenGider = g.Where(k => k.Tip == ButceKalemTipi.Gider)
                            .Sum(k => k.GerceklesenTutar)
                    }).OrderBy(a => a.Ay).ToList();

                ViewBag.AylikOzet = aylikOzet;
                ViewBag.ToplamPlanlananGelir = plan.Kalemler
                    .Where(k => k.Tip == ButceKalemTipi.Gelir)
                    .Sum(k => k.PlanlananTutar);
                ViewBag.ToplamPlanlananGider = plan.Kalemler
                    .Where(k => k.Tip == ButceKalemTipi.Gider)
                    .Sum(k => k.PlanlananTutar);
                ViewBag.ToplamGerceklesenGelir = plan.Kalemler
                    .Where(k => k.Tip == ButceKalemTipi.Gelir)
                    .Sum(k => k.GerceklesenTutar);
                ViewBag.ToplamGerceklesenGider = plan.Kalemler
                    .Where(k => k.Tip == ButceKalemTipi.Gider)
                    .Sum(k => k.GerceklesenTutar);

                return View(plan);
            }
            catch
            {
                TempData["Error"] = "Bütçe detayı yüklenirken hata oluştu.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> KalemEkle(ButceKalemViewModel model, int planId)
        {
            try
            {
                var kalem = new ButceKalem
                {
                    ButcePlanId = planId,
                    Kategori = model.Kategori,
                    Aciklama = model.Aciklama,
                    Tip = model.Tip,
                    PlanlananTutar = model.PlanlananTutar,
                    GerceklesenTutar = model.GerceklesenTutar,
                    Ay = model.Ay
                };

                _context.ButceKalemler.Add(kalem);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Bütçe kalemi eklendi.";
            }
            catch
            {
                TempData["Error"] = "Kalem eklenirken hata oluştu.";
            }
            return RedirectToAction("Detail", new { id = planId });
        }

        [HttpPost]
        public async Task<IActionResult> KalemSil(int id, int planId)
        {
            try
            {
                var kalem = await _context.ButceKalemler.FindAsync(id);
                if (kalem != null)
                {
                    _context.ButceKalemler.Remove(kalem);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Kalem silindi.";
                }
            }
            catch
            {
                TempData["Error"] = "Kalem silinirken hata oluştu.";
            }
            return RedirectToAction("Detail", new { id = planId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var plan = await _context.ButcePlanlar
                    .Include(b => b.Kalemler)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (plan == null)
                {
                    TempData["Error"] = "Plan bulunamadı.";
                    return RedirectToAction("Index");
                }

                _context.ButceKalemler.RemoveRange(plan.Kalemler);
                _context.ButcePlanlar.Remove(plan);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Bütçe planı silindi.";
            }
            catch
            {
                TempData["Error"] = "Plan silinirken hata oluştu.";
            }
            return RedirectToAction("Index");
        }
    }
}