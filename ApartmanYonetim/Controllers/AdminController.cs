using ApartmanYonetim.Data;
using ApartmanYonetim.Models.Entities;
using ApartmanYonetim.Models.ViewModels;
using ApartmanYonetim.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ApartmanYonetim.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IBlockAuthService _blockAuthService;

        public AdminController(ApplicationDbContext context,
            UserManager<AppUser> userManager, IBlockAuthService blockAuthService)
        {
            _context = context;
            _userManager = userManager;
            _blockAuthService = blockAuthService;
        }

        // ───── DAİRELER ─────
        public async Task<IActionResult> Apartments()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var yetkiliBloklar = await _blockAuthService
                    .GetYetkiliBlokIdsAsync(userId!);

                var query = _context.Apartments
                    .Include(a => a.Block)
                    .AsQueryable();

                if (yetkiliBloklar.Any())
                    query = query.Where(a =>
                        a.BlockId != null &&
                        yetkiliBloklar.Contains(a.BlockId.Value));

                var apartments = await query
                    .OrderBy(a => a.Block!.Ad)
                    .ThenBy(a => a.DaireNo)
                    .ToListAsync();

                return View(apartments);
            }
            catch
            {
                TempData["Error"] = "Daireler yüklenirken hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ApartmentCreate()
        {
            await LoadBloklarAsync();
            return View(new Apartment());
        }

        [HttpPost]
        public async Task<IActionResult> ApartmentCreate(Apartment model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await LoadBloklarAsync();
                    return View(model);
                }

                var varMi = await _context.Apartments
                    .AnyAsync(a => a.DaireNo == model.DaireNo
                                && a.BlockId == model.BlockId);
                if (varMi)
                {
                    ModelState.AddModelError("DaireNo",
                        "Bu blokta aynı daire numarası zaten mevcut.");
                    await LoadBloklarAsync();
                    return View(model);
                }

                _context.Apartments.Add(model);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Daire başarıyla eklendi.";
                return RedirectToAction("Apartments");
            }
            catch
            {
                TempData["Error"] = "Daire eklenirken hata oluştu.";
                return RedirectToAction("Apartments");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ApartmentEdit(int id)
        {
            try
            {
                var daire = await _context.Apartments
                    .Include(a => a.Block)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (daire == null)
                {
                    TempData["Error"] = "Daire bulunamadı.";
                    return RedirectToAction("Apartments");
                }

                await LoadBloklarAsync(daire.BlockId);
                return View(daire);
            }
            catch
            {
                TempData["Error"] = "Daire yüklenirken hata oluştu.";
                return RedirectToAction("Apartments");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApartmentEdit(Apartment model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await LoadBloklarAsync(model.BlockId);
                    return View(model);
                }

                var daire = await _context.Apartments.FindAsync(model.Id);
                if (daire == null)
                {
                    TempData["Error"] = "Daire bulunamadı.";
                    return RedirectToAction("Apartments");
                }

                daire.DaireNo = model.DaireNo;
                daire.Kat = model.Kat;
                daire.Tipi = model.Tipi;
                daire.MetreKare = model.MetreKare;
                daire.Dolu = model.Dolu;
                daire.BlockId = model.BlockId;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Daire güncellendi.";
                return RedirectToAction("Apartments");
            }
            catch
            {
                TempData["Error"] = "Daire güncellenirken hata oluştu.";
                return RedirectToAction("Apartments");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApartmentDelete(int id)
        {
            try
            {
                var daire = await _context.Apartments
                    .Include(a => a.Residents)
                    .Include(a => a.Payments)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (daire == null)
                {
                    TempData["Error"] = "Daire bulunamadı.";
                    return RedirectToAction("Apartments");
                }

                if (daire.Residents.Any(r => r.AktifMi))
                {
                    TempData["Error"] = "Bu dairede aktif sakin var. Önce sakini çıkartın.";
                    return RedirectToAction("Apartments");
                }

                _context.Apartments.Remove(daire);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Daire silindi.";
            }
            catch
            {
                TempData["Error"] = "Daire silinirken hata oluştu.";
            }
            return RedirectToAction("Apartments");
        }

        // ───── SAKİNLER ─────
        public async Task<IActionResult> Residents()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var yetkiliBloklar = await _blockAuthService
                    .GetYetkiliBlokIdsAsync(userId!);

                var query = _context.Residents
                    .Include(r => r.Apartment).ThenInclude(a => a.Block)
                    .AsQueryable();

                // Blok kısıtlaması uygula
                if (yetkiliBloklar.Any())
                {
                    query = query.Where(r =>
                        r.Apartment != null &&
                        r.Apartment.BlockId != null &&
                        yetkiliBloklar.Contains(r.Apartment.BlockId.Value));
                }

                var residents = await query
                    .OrderBy(r => r.Apartment!.Block!.Ad)
                    .ThenBy(r => r.Apartment!.DaireNo)
                    .ToListAsync();

                return View(residents);
            }
            catch
            {
                TempData["Error"] = "Sakinler yüklenirken hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }


        [HttpGet]
        public async Task<IActionResult> ResidentCreate()
        {
            await LoadDairelerAsync();
            return View(new ResidentViewModel
            {
                TasinmaTarihi = DateTime.Today
            });
        }

        [HttpPost]
        public async Task<IActionResult> ResidentCreate(ResidentViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await LoadDairelerAsync();
                    return View(model);
                }

                var sakin = new Resident
                {
                    Ad = model.Ad,
                    Soyad = model.Soyad,
                    ApartmentId = model.ApartmentId,
                    TasinmaTarihi = model.TasinmaTarihi,
                    AktifMi = true
                };

                _context.Residents.Add(sakin);

                var daire = await _context.Apartments.FindAsync(model.ApartmentId);
                if (daire != null) daire.Dolu = true;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Sakin başarıyla eklendi.";
                return RedirectToAction("Residents");
            }
            catch
            {
                TempData["Error"] = "Sakin eklenirken hata oluştu.";
                await LoadDairelerAsync();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ResidentEdit(int id)
        {
            try
            {
                var sakin = await _context.Residents
                    .Include(r => r.Apartment).ThenInclude(a => a.Block)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (sakin == null)
                {
                    TempData["Error"] = "Sakin bulunamadı.";
                    return RedirectToAction("Residents");
                }

                await LoadDairelerAsync(sakin.ApartmentId);

                var model = new ResidentViewModel
                {
                    Id = sakin.Id,
                    Ad = sakin.Ad,
                    Soyad = sakin.Soyad,
                    ApartmentId = sakin.ApartmentId,
                    TasinmaTarihi = sakin.TasinmaTarihi,
                    AyrilmaTarihi = sakin.AyrilmaTarihi,
                    AktifMi = sakin.AktifMi
                };
                return View(model);
            }
            catch
            {
                TempData["Error"] = "Sakin yüklenirken hata oluştu.";
                return RedirectToAction("Residents");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResidentEdit(ResidentViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await LoadDairelerAsync(model.ApartmentId);
                    return View(model);
                }

                var sakin = await _context.Residents.FindAsync(model.Id);
                if (sakin == null)
                {
                    TempData["Error"] = "Sakin bulunamadı.";
                    return RedirectToAction("Residents");
                }

                sakin.Ad = model.Ad;
                sakin.Soyad = model.Soyad;
                sakin.ApartmentId = model.ApartmentId;
                sakin.TasinmaTarihi = model.TasinmaTarihi;
                sakin.AyrilmaTarihi = model.AyrilmaTarihi;
                sakin.AktifMi = model.AktifMi;

                if (!model.AktifMi && model.AyrilmaTarihi.HasValue)
                {
                    var daire = await _context.Apartments
                        .FindAsync(model.ApartmentId);
                    if (daire != null) daire.Dolu = false;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Sakin güncellendi.";
                return RedirectToAction("Residents");
            }
            catch
            {
                TempData["Error"] = "Sakin güncellenirken hata oluştu.";
                await LoadDairelerAsync(model.ApartmentId);
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResidentDelete(int id)
        {
            try
            {
                var sakin = await _context.Residents.FindAsync(id);
                if (sakin == null)
                {
                    TempData["Error"] = "Sakin bulunamadı.";
                    return RedirectToAction("Residents");
                }

                _context.Residents.Remove(sakin);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Sakin silindi.";
            }
            catch
            {
                TempData["Error"] = "Sakin silinirken hata oluştu.";
            }
            return RedirectToAction("Residents");
        }

        // ───── KULLANICILAR ─────
        public async Task<IActionResult> Users()
        {
            try
            {
                var kullanicilar = await _userManager.Users
                    .Include(u => u.Apartment).ThenInclude(a => a!.Block)
                    .ToListAsync();

                var model = new List<UserWithRoleViewModel>();
                foreach (var u in kullanicilar)
                {
                    var roller = await _userManager.GetRolesAsync(u);
                    model.Add(new UserWithRoleViewModel
                    {
                        Id = u.Id,
                        Ad = u.Ad,
                        Soyad = u.Soyad,
                        Email = u.Email ?? "",
                        DaireNo = u.Apartment?.DaireNo,
                        BlokAdi = u.Apartment?.Block?.Ad,
                        Rol = roller.FirstOrDefault() ?? "-"
                    });
                }
                return View(model);
            }
            catch
            {
                TempData["Error"] = "Kullanıcılar yüklenirken hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UserDelete(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "Kullanıcı bulunamadı.";
                    return RedirectToAction("Users");
                }

                var roller = await _userManager.GetRolesAsync(user);
                if (roller.Contains("Admin"))
                {
                    TempData["Error"] = "Admin kullanıcısı silinemez.";
                    return RedirectToAction("Users");
                }

                await _userManager.DeleteAsync(user);
                TempData["Success"] = "Kullanıcı silindi.";
            }
            catch(Exception ex) 
            {
                TempData["Error"] = "Kullanıcı silinirken hata oluştu.";          
            }
            return RedirectToAction("Users");
        }

        // ───── YARDIMCI METODLAR ─────
        private async Task LoadBloklarAsync(int? secilenId = null)
        {
            var bloklar = await _context.Blocks
                .Where(b => b.AktifMi)
                .OrderBy(b => b.Ad)
                .ToListAsync();

            ViewBag.Bloklar = new SelectList(bloklar, "Id", "Ad", secilenId);
        }

        private async Task LoadDairelerAsync(int? secilenId = null)
        {
            var daireler = await _context.Apartments
                .Include(a => a.Block)
                .OrderBy(a => a.Block!.Ad)
                .ThenBy(a => a.Kat)
                .ThenBy(a => a.DaireNo)
                .ToListAsync();

            var liste = daireler.Select(d => new
            {
                d.Id,
                Tanim = $"{(d.Block != null ? d.Block.Ad + " — " : "")}Kat {d.Kat} — Daire {d.DaireNo} ({d.Tipi})"
            }).ToList();

            ViewBag.Daireler = new SelectList(liste, "Id", "Tanim", secilenId);
        }
    }
}