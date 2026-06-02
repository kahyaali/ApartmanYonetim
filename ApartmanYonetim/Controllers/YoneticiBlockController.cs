using ApartmanYonetim.Data;
using ApartmanYonetim.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApartmanYonetim.Controllers
{
    [Authorize(Roles = "Admin")]
    public class YoneticiBlockController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public YoneticiBlockController(ApplicationDbContext context,
            UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var atamalar = await _context.YoneticiBlocklar
                    .Include(y => y.Yonetici)
                    .Include(y => y.Block)
                    .OrderBy(y => y.Block!.Ad)
                    .ToListAsync();

                ViewBag.Yoneticiler = await _userManager.GetUsersInRoleAsync("Admin");
                ViewBag.Bloklar = await _context.Blocks
                    .OrderBy(b => b.Ad).ToListAsync();

                return View(atamalar);
            }
            catch
            {
                TempData["Error"] = "Yükleme hatası oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Ata(string yoneticiId, int blockId)
        {
            try
            {
                var varMi = await _context.YoneticiBlocklar
                    .AnyAsync(y => y.YoneticiId == yoneticiId
                               && y.BlockId == blockId);

                if (varMi)
                {
                    TempData["Error"] = "Bu yönetici zaten bu bloğa atanmış.";
                    return RedirectToAction("Index");
                }

                var atama = new YoneticiBlock
                {
                    YoneticiId = yoneticiId,
                    BlockId = blockId,
                    AtamaTarihi = DateTime.Now
                };

                _context.YoneticiBlocklar.Add(atama);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Yönetici blok ataması yapıldı.";
            }
            catch
            {
                TempData["Error"] = "Atama yapılırken hata oluştu.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AtamaKaldir(int id)
        {
            try
            {
                var atama = await _context.YoneticiBlocklar.FindAsync(id);
                if (atama == null)
                {
                    TempData["Error"] = "Atama bulunamadı.";
                    return RedirectToAction("Index");
                }

                _context.YoneticiBlocklar.Remove(atama);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Atama kaldırıldı.";
            }
            catch
            {
                TempData["Error"] = "Atama kaldırılırken hata oluştu.";
            }
            return RedirectToAction("Index");
        }
    }
}