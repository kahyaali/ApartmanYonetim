using ApartmanYonetim.Data;
using ApartmanYonetim.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApartmanYonetim.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BlockController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BlockController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var bloklar = await _context.Blocks
                .Include(b => b.Apartments)
                .OrderBy(b => b.Ad)
                .ToListAsync();

            ViewBag.ToplamDaire = bloklar.Sum(b => b.Apartments.Count);
            ViewBag.DoluDaire = bloklar.Sum(b => b.Apartments.Count(a => a.Dolu));

            return View(bloklar);
        }

        [HttpGet]
        public IActionResult Create() => View(new Block());

        [HttpPost]
        public async Task<IActionResult> Create(Block model)
        {
            if (!ModelState.IsValid) return View(model);

            var varMi = await _context.Blocks.AnyAsync(b => b.Ad == model.Ad);
            if (varMi)
            {
                ModelState.AddModelError("Ad", "Bu blok adı zaten mevcut.");
                return View(model);
            }

            _context.Blocks.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"{model.Ad} başarıyla eklendi.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var blok = await _context.Blocks.FindAsync(id);
            if (blok == null) return NotFound();
            return View(blok);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Block model)
        {
            if (!ModelState.IsValid) return View(model);

            _context.Blocks.Update(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Blok güncellendi.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var blok = await _context.Blocks
                .Include(b => b.Apartments)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (blok == null) return NotFound();

            if (blok.Apartments.Any())
            {
                TempData["Error"] = "Bu bloğa ait daireler var. Önce daireleri silin.";
                return RedirectToAction("Index");
            }

            _context.Blocks.Remove(blok);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Blok silindi.";
            return RedirectToAction("Index");
        }

        // Blok detay — o bloğa ait tüm daireler
        public async Task<IActionResult> Detail(int id)
        {
            var blok = await _context.Blocks
                .Include(b => b.Apartments)
                    .ThenInclude(a => a.Residents.Where(r => r.AktifMi))
                .FirstOrDefaultAsync(b => b.Id == id);

            if (blok == null) return NotFound();
            return View(blok);
        }
    }
}