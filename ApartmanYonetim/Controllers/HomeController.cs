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
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IBlockAuthService _blockAuthService;

        public HomeController(ApplicationDbContext context, UserManager<AppUser> userManager, IBlockAuthService blockAuthService)
        {
            _context = context;
            _userManager = userManager;
            _blockAuthService = blockAuthService;
        }


      
        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var isAdmin = User.IsInRole("Admin");

                if (isAdmin && user != null)
                {
                    var yetkiliBloklar = await _blockAuthService
                        .GetYetkiliBlokIdsAsync(user.Id);

                    // Daire sayısı — blok filtreli
                    var dairelerQuery = _context.Apartments.AsQueryable();
                    if (yetkiliBloklar.Any())
                        dairelerQuery = dairelerQuery.Where(a =>
                            a.BlockId != null &&
                            yetkiliBloklar.Contains(a.BlockId.Value));

                    ViewBag.ToplamDaire = await dairelerQuery.CountAsync();

                    // Sakin sayısı — blok filtreli
                    var sakinlerQuery = _context.Residents
                        .Include(r => r.Apartment)
                        .Where(r => r.AktifMi)
                        .AsQueryable();
                    if (yetkiliBloklar.Any())
                        sakinlerQuery = sakinlerQuery.Where(r =>
                            r.Apartment != null &&
                            r.Apartment.BlockId != null &&
                            yetkiliBloklar.Contains(r.Apartment.BlockId.Value));

                    ViewBag.ToplamSakin = await sakinlerQuery.CountAsync();

                    // Ödeme istatistikleri — blok filtreli
                    var odemelerQuery = _context.Payments
                        .Include(p => p.Apartment)
                        .Include(p => p.Transactions)
                        .AsQueryable();
                    if (yetkiliBloklar.Any())
                        odemelerQuery = odemelerQuery.Where(p =>
                            p.Apartment != null &&
                            p.Apartment.BlockId != null &&
                            yetkiliBloklar.Contains(p.Apartment.BlockId.Value));

                    var odemeler = await odemelerQuery.ToListAsync();

                    ViewBag.BekleyenOdeme = odemeler.Count(p => !p.Odendi);
                    ViewBag.ToplamGelir = odemeler.Sum(p => p.OdenenTutar);
                    ViewBag.ToplamGider = await _context.Expenses
                        .SumAsync(e => (decimal?)e.Tutar) ?? 0m;

                    // Son ödemeler — blok filtreli
                    ViewBag.SonOdemeler = odemeler
                        .OrderByDescending(p => p.SonOdemeTarihi)
                        .Take(8)
                        .ToList();

                    // Yetkili blok bilgisi (dashboard'da göstermek için)
                    ViewBag.YetkiliBloklar = yetkiliBloklar.Any()
                        ? await _context.Blocks
                            .Where(b => yetkiliBloklar.Contains(b.Id))
                            .Select(b => b.Ad).ToListAsync()
                        : new List<string> { "Tüm Bloklar" };
                }
                else if (!isAdmin && user != null)
                {
                    // Sakin görünümü — değişmedi
                    ViewBag.ToplamDaire = 0;
                    ViewBag.ToplamSakin = 0;
                    ViewBag.BekleyenOdeme = await _context.Payments
                        .CountAsync(p => !p.Odendi &&
                            p.ApartmentId == user.ApartmentId);
                    ViewBag.ToplamGelir = 0m;
                    ViewBag.ToplamGider = 0m;

                    var sonOdemeler = await _context.Payments
                        .Include(p => p.Apartment).ThenInclude(a => a.Block)
                        .Include(p => p.Transactions)
                        .Where(p => p.ApartmentId == user.ApartmentId)
                        .OrderByDescending(p => p.SonOdemeTarihi)
                        .Take(8)
                        .ToListAsync();

                    ViewBag.SonOdemeler = sonOdemeler;
                }
                else
                {
                    // Fallback — hiçbir şey null olmasın
                    ViewBag.ToplamDaire = 0;
                    ViewBag.ToplamSakin = 0;
                    ViewBag.BekleyenOdeme = 0;
                    ViewBag.ToplamGelir = 0m;
                    ViewBag.ToplamGider = 0m;
                    ViewBag.SonOdemeler = new List<Payment>();
                }

                // Duyurular her zaman yüklenir
                ViewBag.SonDuyurular = await _context.Announcements
                    .Where(a => a.AktifMi)
                    .OrderByDescending(a => a.YayinTarihi)
                    .Take(5)
                    .ToListAsync();

                return View();
            }
            catch (Exception ex)
            {
                // Hata detayını logla, sayfayı boş göster
                ViewBag.ToplamDaire = 0;
                ViewBag.ToplamSakin = 0;
                ViewBag.BekleyenOdeme = 0;
                ViewBag.ToplamGelir = 0m;
                ViewBag.ToplamGider = 0m;
                ViewBag.ToplamBorc = 0m;
                ViewBag.SonOdemeler = new List<Payment>();
                ViewBag.SonDuyurular = new List<Announcement>();
                return View();
            }
        }


        [AllowAnonymous]
        public IActionResult Error()
        {
            TempData["Error"] = "Bir hata oluştu. Lütfen tekrar deneyin.";
            return RedirectToAction("Index");
        }
    }
}