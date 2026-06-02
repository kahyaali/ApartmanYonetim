using ApartmanYonetim.Data;
using ApartmanYonetim.Models.Entities;
using ApartmanYonetim.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ApartmanYonetim.Controllers
{
    [Authorize]
    public class VehicleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public VehicleController(ApplicationDbContext context,
            UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var isAdmin = User.IsInRole("Admin");
                var user = await _userManager.GetUserAsync(User);

                var query = _context.Vehicles
                    .Include(v => v.Apartment).ThenInclude(a => a!.Block)
                    .AsQueryable();

                if (!isAdmin)
                    query = query.Where(v => v.ApartmentId == user!.ApartmentId);

                var araclar = await query
                    .OrderBy(v => v.ZiyaretciMi)
                    .ThenBy(v => v.Apartment!.Block!.Ad)
                    .ThenBy(v => v.Apartment!.DaireNo)
                    .ToListAsync();

                ViewBag.ToplamArac = araclar.Count(v => !v.ZiyaretciMi && v.AktifMi);
                ViewBag.ToplamZiyaretci = araclar.Count(v => v.ZiyaretciMi && v.AktifMi);
                ViewBag.DoluOtopark = araclar.Count(v => !string.IsNullOrEmpty(v.OtoparkNo) && v.AktifMi);

                return View(araclar);
            }
            catch
            {
                TempData["Error"] = "Araçlar yüklenirken hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDairelerAsync();
            return View(new VehicleViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(VehicleViewModel aracModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await LoadDairelerAsync();
                    return View(aracModel);
                }

                // Plaka zaten var mı?
                var plakaVar = await _context.Vehicles
                    .AnyAsync(v => v.Plaka.ToUpper() == aracModel.Plaka.ToUpper()
                                && v.AktifMi);
                if (plakaVar)
                {
                    ModelState.AddModelError("Plaka",
                        "Bu plaka zaten sistemde kayıtlı.");
                    await LoadDairelerAsync();
                    return View(aracModel);
                }

                var user = await _userManager.GetUserAsync(User);
                var isAdmin = User.IsInRole("Admin");

                var vehicle = new Vehicle
                {
                    Plaka = aracModel.Plaka.ToUpper().Trim(),
                    Marka = aracModel.Marka,
                    Model = aracModel.Model,
                    Renk = aracModel.Renk,
                    AracTipi = aracModel.AracTipi,
                    OtoparkNo = aracModel.OtoparkNo,
                    ZiyaretciMi = aracModel.ZiyaretciMi,
                    ApartmentId = isAdmin ? aracModel.ApartmentId : user!.ApartmentId,
                    KaydedenId = user!.Id,
                    Aciklama = aracModel.Aciklama,
                    KayitTarihi = DateTime.Now,
                    AktifMi = true
                };

                if (aracModel.ZiyaretciMi)
                    vehicle.ZiyaretGiris = DateTime.Now;

                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Araç kaydedildi.";
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["Error"] = "Araç eklenirken hata oluştu.";
                await LoadDairelerAsync();
                return View(aracModel);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ZiyaretciCikis(int id)
        {
            try
            {
                var arac = await _context.Vehicles.FindAsync(id);
                if (arac == null)
                {
                    TempData["Error"] = "Araç bulunamadı.";
                    return RedirectToAction("Index");
                }

                arac.ZiyaretCikis = DateTime.Now;
                arac.AktifMi = false;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"{arac.Plaka} plakalı araç çıkışı kaydedildi.";
            }
            catch
            {
                TempData["Error"] = "Çıkış kaydedilirken hata oluştu.";
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var arac = await _context.Vehicles.FindAsync(id);
                if (arac == null)
                {
                    TempData["Error"] = "Araç bulunamadı.";
                    return RedirectToAction("Index");
                }

                _context.Vehicles.Remove(arac);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Araç silindi.";
            }
            catch
            {
                TempData["Error"] = "Araç silinirken hata oluştu.";
            }
            return RedirectToAction("Index");
        }

        private async Task LoadDairelerAsync(int? secilenId = null)
        {
            var daireler = await _context.Apartments
                .Include(a => a.Block)
                .OrderBy(a => a.Block!.Ad)
                .ThenBy(a => a.DaireNo)
                .ToListAsync();

            var liste = daireler.Select(d => new
            {
                d.Id,
                Tanim = $"{(d.Block != null ? d.Block.Ad + " — " : "")}Daire {d.DaireNo}"
            }).ToList();

            ViewBag.Daireler = new SelectList(liste, "Id", "Tanim", secilenId);
        }
    }
}