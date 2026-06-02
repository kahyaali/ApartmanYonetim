using ApartmanYonetim.Data;
using ApartmanYonetim.Models.Entities;
using ApartmanYonetim.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApartmanYonetim.Controllers
{
    [Authorize]
    public class AnnouncementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AnnouncementController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var duyurular = await _context.Announcements
                .OrderByDescending(a => a.YayinTarihi)
                .ToListAsync();
            return View(duyurular);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View(new AnnouncementViewModel());

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(AnnouncementViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);

            var duyuru = new Announcement
            {
                Baslik = model.Baslik,
                Icerik = model.Icerik,
                YayinTarihi = DateTime.Now,
                AktifMi = model.AktifMi,
                OlusturanId = user!.Id
            };

            _context.Announcements.Add(duyuru);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Duyuru yayınlandı.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var duyuru = await _context.Announcements.FindAsync(id);
            if (duyuru == null) return NotFound();

            var model = new AnnouncementViewModel
            {
                Id = duyuru.Id,
                Baslik = duyuru.Baslik,
                Icerik = duyuru.Icerik,
                AktifMi = duyuru.AktifMi
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(AnnouncementViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var duyuru = await _context.Announcements.FindAsync(model.Id);
            if (duyuru == null) return NotFound();

            duyuru.Baslik = model.Baslik;
            duyuru.Icerik = model.Icerik;
            duyuru.AktifMi = model.AktifMi;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Duyuru güncellendi.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var duyuru = await _context.Announcements.FindAsync(id);
            if (duyuru == null) return NotFound();

            _context.Announcements.Remove(duyuru);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Duyuru silindi.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var duyuru = await _context.Announcements.FindAsync(id);
            if (duyuru == null) return NotFound();

            duyuru.AktifMi = !duyuru.AktifMi;
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}