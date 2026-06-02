using ApartmanYonetim.Data;
using ApartmanYonetim.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApartmanYonetim.Controllers
{
    [Authorize]
    public class FavoriController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public FavoriController(ApplicationDbContext context,
            UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Toggle([FromBody] FavoriToggleRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Json(new { success = false, mesaj = "Oturum bulunamadı." });

                var mevcut = await _context.FavoriSayfalar
                    .FirstOrDefaultAsync(f => f.KullaniciId == user.Id
                                           && f.Url == request.Url);

                if (mevcut != null)
                {
                    _context.FavoriSayfalar.Remove(mevcut);
                    await _context.SaveChangesAsync();
                    return Json(new
                    {
                        success = true,
                        pinned = false,
                        mesaj = "Favorilerden kaldırıldı."
                    });
                }
                else
                {
                    var favori = new FavoriSayfa
                    {
                        KullaniciId = user.Id,
                        Baslik = request.Baslik,
                        Url = request.Url,
                        Ikon = request.Ikon ?? "fa-bookmark",
                        EklemeTarihi = DateTime.Now
                    };
                    _context.FavoriSayfalar.Add(favori);
                    await _context.SaveChangesAsync();
                    return Json(new
                    {
                        success = true,
                        pinned = true,
                        mesaj = "Favorilere eklendi."
                    });
                }
            }
            catch
            {
                return Json(new
                {
                    success = false,
                    mesaj = "İşlem gerçekleştirilemedi."
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Json(new List<object>());

                var favoriler = await _context.FavoriSayfalar
                    .Where(f => f.KullaniciId == user.Id)
                    .OrderBy(f => f.Sira)
                    .ThenBy(f => f.EklemeTarihi)
                    .Select(f => new
                    {
                        id = f.Id,
                        baslik = f.Baslik,
                        url = f.Url,
                        ikon = f.Ikon
                    })
                    .ToListAsync();

                return Json(favoriler);
            }
            catch
            {
                return Json(new List<object>());
            }
        }
    }

    public class FavoriToggleRequest
    {
        public string Baslik { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? Ikon { get; set; }
    }
}