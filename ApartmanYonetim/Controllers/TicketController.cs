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
    public class TicketController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly INotificationService _notificationService;

        public TicketController(ApplicationDbContext context,
            UserManager<AppUser> userManager, INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        // ───── LİSTE ─────
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            var query = _context.Tickets
                .Include(t => t.Olusturan)
                .Include(t => t.Apartment).ThenInclude(a => a!.Block)
                .AsQueryable();

            if (!isAdmin)
                query = query.Where(t => t.OlusturanId == user!.Id);

            var tickets = await query
                .OrderByDescending(t => t.OlusturmaTarihi)
                .ToListAsync();

            ViewBag.ToplamAcik = tickets.Count(t =>
                t.Durum == TicketDurum.Bekliyor ||
                t.Durum == TicketDurum.Inceleniyor);
            ViewBag.ToplamKapali = tickets.Count(t =>
                t.Durum == TicketDurum.Tamamlandi);

            return View(tickets);
        }

        // ───── YENİ TALEBİ ─────
        [HttpGet]
        public IActionResult Create() => View(new TicketCreateViewModel());

        [HttpPost]
        public async Task<IActionResult> Create(TicketCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);

            var ticket = new Ticket
            {
                Baslik = model.Baslik,
                Aciklama = model.Aciklama,
                Kategori = model.Kategori,
                Oncelik = model.Oncelik,
                OlusturanId = user!.Id,
                ApartmentId = user.ApartmentId,
                OlusturmaTarihi = DateTime.Now,
                Durum = TicketDurum.Bekliyor
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Talebiniz iletildi. En kısa sürede değerlendireceğiz.";
            return RedirectToAction("Index");
        }

        // ───── DETAY ─────
        public async Task<IActionResult> Detail(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            var ticket = await _context.Tickets
                .Include(t => t.Olusturan)
                .Include(t => t.Apartment).ThenInclude(a => a!.Block)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null) return NotFound();
            if (!isAdmin && ticket.OlusturanId != user!.Id) return Forbid();

            return View(ticket);
        }

        // ───── ADMİN: DURUM GÜNCELLE ─────
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(TicketAdminViewModel model)
        {
            var ticket = await _context.Tickets.FindAsync(model.Id);
            if (ticket == null) return NotFound();

            ticket.Durum = model.Durum;
            ticket.Oncelik = model.Oncelik;
            ticket.AdminNotu = model.AdminNotu;

            if (model.Durum == TicketDurum.Tamamlandi ||
                model.Durum == TicketDurum.Iptal)
                ticket.KapanmaTarihi = DateTime.Now;
            else
                ticket.KapanmaTarihi = null;

            await _context.SaveChangesAsync();

            // Talebi açan kişiye bildirim gönder
            var durumMesaj = model.Durum switch
            {
                TicketDurum.Inceleniyor => "Talebiniz incelemeye alındı.",
                TicketDurum.Tamamlandi => "Talebiniz tamamlandı.",
                TicketDurum.Iptal => "Talebiniz iptal edildi.",
                _ => "Talebiniz güncellendi."
            };

            await _notificationService.SendToUserAsync(
                ticket.OlusturanId,
                $"Talep #{ticket.Id}: {durumMesaj}",
                model.Durum == TicketDurum.Tamamlandi ? "success" : "info"
            );

            TempData["Success"] = "Talep güncellendi.";
            return RedirectToAction("Detail", new { id = model.Id });
        }

        // ───── SİL ─────
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Talep silindi.";
            return RedirectToAction("Index");
        }
    }
}