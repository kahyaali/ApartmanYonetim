using ApartmanYonetim.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApartmanYonetim.Models.Entities;

namespace ApartmanYonetim.Controllers
{
    [Authorize]
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public SearchController(ApplicationDbContext context,
            UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            {
                ViewBag.Query = q;
                return View(new SearchResultViewModel());
            }

            var isAdmin = User.IsInRole("Admin");
            var user = await _userManager.GetUserAsync(User);
            var aranan = q.ToLower().Trim();

            var sonuc = new SearchResultViewModel { Query = q };

            if (isAdmin)
            {
                // Sakinler
                sonuc.Sakinler = await _context.Residents
                    .Include(r => r.Apartment).ThenInclude(a => a.Block)
                    .Where(r => r.Ad.ToLower().Contains(aranan)
                             || r.Soyad.ToLower().Contains(aranan)
                             || (r.Ad + " " + r.Soyad).ToLower().Contains(aranan))
                    .Take(5).ToListAsync();

                // Daireler
                sonuc.Daireler = await _context.Apartments
                    .Include(a => a.Block)
                    .Where(a => a.DaireNo.ToString().Contains(aranan)
                             || (a.Block != null &&
                                 a.Block.Ad.ToLower().Contains(aranan)))
                    .Take(5).ToListAsync();

                // Araçlar
                sonuc.Araclar = await _context.Vehicles
                    .Include(v => v.Apartment).ThenInclude(a => a!.Block)
                    .Where(v => v.Plaka.ToLower().Contains(aranan)
                             || v.Marka.ToLower().Contains(aranan))
                    .Take(5).ToListAsync();
            }

            // Ödemeler
            var odemeQuery = _context.Payments
                .Include(p => p.Apartment).ThenInclude(a => a.Block)
                .Include(p => p.Transactions)
                .Where(p => p.Aciklama.ToLower().Contains(aranan));

            if (!isAdmin)
                odemeQuery = odemeQuery.Where(p => p.ApartmentId == user!.ApartmentId);

            sonuc.Odemeler = await odemeQuery.Take(5).ToListAsync();

            // Duyurular
            sonuc.Duyurular = await _context.Announcements
                .Where(a => a.AktifMi
                         && (a.Baslik.ToLower().Contains(aranan)
                          || a.Icerik.ToLower().Contains(aranan)))
                .Take(5).ToListAsync();

            // Talepler
            var ticketQuery = _context.Tickets
                .Where(t => t.Baslik.ToLower().Contains(aranan)
                          || t.Aciklama.ToLower().Contains(aranan));

            if (!isAdmin)
                ticketQuery = ticketQuery.Where(t => t.OlusturanId == user!.Id);

            sonuc.Talepler = await ticketQuery.Take(5).ToListAsync();

            ViewBag.Query = q;
            return View(sonuc);
        }
    }

    public class SearchResultViewModel
    {
        public string Query { get; set; } = string.Empty;
        public List<ApartmanYonetim.Models.Entities.Resident> Sakinler { get; set; } = new();
        public List<ApartmanYonetim.Models.Entities.Apartment> Daireler { get; set; } = new();
        public List<ApartmanYonetim.Models.Entities.Payment> Odemeler { get; set; } = new();
        public List<ApartmanYonetim.Models.Entities.Announcement> Duyurular { get; set; } = new();
        public List<ApartmanYonetim.Models.Entities.Ticket> Talepler { get; set; } = new();
        public List<ApartmanYonetim.Models.Entities.Vehicle> Araclar { get; set; } = new();

        public int ToplamSonuc =>
            Sakinler.Count + Daireler.Count + Odemeler.Count +
            Duyurular.Count + Talepler.Count + Araclar.Count;
    }
}