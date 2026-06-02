using ApartmanYonetim.Data;
using ApartmanYonetim.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ApartmanYonetim.Controllers
{
    [Authorize]
    public class KargoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public KargoController(ApplicationDbContext context,
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

                var query = _context.Kargolar
                    .Include(k => k.Apartment).ThenInclude(a => a!.Block)
                    .AsQueryable();

                if (!isAdmin)
                    query = query.Where(k => k.ApartmentId == user!.ApartmentId);

                var kargolar = await query
                    .OrderByDescending(k => k.GelisTarihi)
                    .ToListAsync();

                ViewBag.Bekleyen = kargolar.Count(k => k.Durum == KargoDurum.Bekliyor);
                ViewBag.Teslim = kargolar.Count(k => k.Durum == KargoDurum.Teslim);

                return View(kargolar);
            }
            catch
            {
                TempData["Error"] = "Kargolar yüklenirken hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDairelerAsync();
            return View(new Kargo { GelisTarihi = DateTime.Now });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(Kargo model)
        {
            try
            {
                model.Durum = KargoDurum.Bekliyor;
                model.GelisTarihi = DateTime.Now;
                _context.Kargolar.Add(model);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Kargo kaydedildi.";
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["Error"] = "Kargo eklenirken hata oluştu.";
                await LoadDairelerAsync();
                return View(model);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Teslim(int id, string teslimAlan)
        {
            try
            {
                var kargo = await _context.Kargolar.FindAsync(id);
                if (kargo == null)
                {
                    TempData["Error"] = "Kargo bulunamadı.";
                    return RedirectToAction("Index");
                }

                kargo.Durum = KargoDurum.Teslim;
                kargo.TeslimTarihi = DateTime.Now;
                kargo.TeslimAlan = teslimAlan;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Kargo teslim edildi olarak işaretlendi.";
            }
            catch
            {
                TempData["Error"] = "İşlem gerçekleştirilemedi.";
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var kargo = await _context.Kargolar.FindAsync(id);
                if (kargo == null)
                {
                    TempData["Error"] = "Kargo bulunamadı.";
                    return RedirectToAction("Index");
                }

                _context.Kargolar.Remove(kargo);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Kargo silindi.";
            }
            catch
            {
                TempData["Error"] = "Kargo silinirken hata oluştu.";
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