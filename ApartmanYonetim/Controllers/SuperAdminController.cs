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
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SuperAdminController(ApplicationDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            return RedirectToAction("Yoneticiler");
        }

        // Tüm kullanıcıları listele
        public async Task<IActionResult> TumKullanicilar()
        {
            try
            {
                var kullanicilar = _userManager.Users
                    .Include(u => u.Apartment).ThenInclude(a => a!.Block)
                    .OrderBy(u => u.Ad)
                    .ToList();

                var model = new List<KullaniciDetayViewModel>();
                foreach (var u in kullanicilar)
                {
                    var roller = await _userManager.GetRolesAsync(u);
                    model.Add(new KullaniciDetayViewModel
                    {
                        Id = u.Id,
                        Ad = u.Ad,
                        Soyad = u.Soyad,
                        Email = u.Email ?? "",
                        Roller = roller.ToList(),
                        DaireNo = u.Apartment?.DaireNo.ToString(),
                        BlokAd = u.Apartment?.Block?.Ad
                    });
                }

                return View(model);
            }
            catch
            {
                TempData["Error"] = "Kullanıcılar yüklenirken hata oluştu.";
                return RedirectToAction("Yoneticiler");
            }
        }

        // Kullanıcıya rol ata/kaldır
        [HttpPost]
        public async Task<IActionResult> RolDegistir(string userId, string rol,
            bool ekle)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    TempData["Error"] = "Kullanıcı bulunamadı.";
                    return RedirectToAction("TumKullanicilar");
                }

                if (ekle)
                    await _userManager.AddToRoleAsync(user, rol);
                else
                    await _userManager.RemoveFromRoleAsync(user, rol);

                TempData["Success"] = $"Rol güncellendi.";
            }
            catch
            {
                TempData["Error"] = "Rol güncellenirken hata oluştu.";
            }
            return RedirectToAction("TumKullanicilar");
        }


        // ───── YÖNETİCİ LİSTESİ ─────
        public async Task<IActionResult> Yoneticiler()
        {
            try
            {
                var adminlar = await _userManager
                    .GetUsersInRoleAsync("Admin");

                var model = new List<YoneticiListViewModel>();
                foreach (var u in adminlar)
                {
                    var roller = await _userManager.GetRolesAsync(u);
                    var bloklar = await _context.YoneticiBlocklar
                        .Include(y => y.Block)
                        .Where(y => y.YoneticiId == u.Id)
                        .ToListAsync();

                    model.Add(new YoneticiListViewModel
                    {
                        Id = u.Id,
                        Ad = u.Ad,
                        Soyad = u.Soyad,
                        Email = u.Email ?? "",
                        Roller = roller.ToList(),
                        AtandigiBloklar = bloklar
                            .Select(b => b.Block?.Ad ?? "")
                            .ToList()
                    });
                }

                return View(model);
            }
            catch
            {
                TempData["Error"] = "Yöneticiler yüklenirken hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }

        // ───── YÖNETİCİ EKLE GET ─────
        [HttpGet]
        public async Task<IActionResult> YoneticiEkle()
        {
            ViewBag.Bloklar = await _context.Blocks
                .OrderBy(b => b.Ad).ToListAsync();
            return View(new YoneticiEkleViewModel());
        }

        // ───── YÖNETİCİ EKLE POST ─────
        [HttpPost]
        public async Task<IActionResult> YoneticiEkle(YoneticiEkleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Bloklar = await _context.Blocks
                    .OrderBy(b => b.Ad).ToListAsync();
                return View(model);
            }

            try
            {
                // Email zaten var mı?
                var varMi = await _userManager.FindByEmailAsync(model.Email);
                if (varMi != null)
                {
                    ModelState.AddModelError("Email",
                        "Bu e-posta adresi zaten kullanımda.");
                    ViewBag.Bloklar = await _context.Blocks
                        .OrderBy(b => b.Ad).ToListAsync();
                    return View(model);
                }

                var user = new AppUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Ad = model.Ad,
                    Soyad = model.Soyad,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Sifre);
                if (!result.Succeeded)
                {
                    foreach (var e in result.Errors)
                        ModelState.AddModelError("", e.Description);
                    ViewBag.Bloklar = await _context.Blocks
                        .OrderBy(b => b.Ad).ToListAsync();
                    return View(model);
                }

                await _userManager.AddToRoleAsync(user, "Admin");

                // Blok atamaları
                if (model.BlokIds != null && model.BlokIds.Any())
                {
                    foreach (var blokId in model.BlokIds)
                    {
                        _context.YoneticiBlocklar.Add(new YoneticiBlock
                        {
                            YoneticiId = user.Id,
                            BlockId = blokId,
                            AtamaTarihi = DateTime.Now
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] =
                    $"Yönetici {user.Ad} {user.Soyad} başarıyla eklendi.";
                return RedirectToAction("Yoneticiler");
            }
            catch
            {
                TempData["Error"] = "Yönetici eklenirken hata oluştu.";
                ViewBag.Bloklar = await _context.Blocks
                    .OrderBy(b => b.Ad).ToListAsync();
                return View(model);
            }
        }

        // ───── YÖNETİCİ DÜZENLE GET ─────
        [HttpGet]
        public async Task<IActionResult> YoneticiDuzenle(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "Yönetici bulunamadı.";
                    return RedirectToAction("Yoneticiler");
                }

                var atandigiBloklar = await _context.YoneticiBlocklar
                    .Where(y => y.YoneticiId == id)
                    .Select(y => y.BlockId)
                    .ToListAsync();

                var model = new YoneticiDuzenleViewModel
                {
                    Id = user.Id,
                    Ad = user.Ad,
                    Soyad = user.Soyad,
                    Email = user.Email ?? "",
                    SecilenBlokIds = atandigiBloklar
                };

                ViewBag.Bloklar = await _context.Blocks
                    .OrderBy(b => b.Ad).ToListAsync();
                return View(model);
            }
            catch
            {
                TempData["Error"] = "Yönetici bilgileri yüklenemedi.";
                return RedirectToAction("Yoneticiler");
            }
        }

        // ───── YÖNETİCİ DÜZENLE POST ─────
        [HttpPost]
        public async Task<IActionResult> YoneticiDuzenle(
            YoneticiDuzenleViewModel model)
        {
            ModelState.Remove("YeniSifre");
            if (!ModelState.IsValid)
            {
                ViewBag.Bloklar = await _context.Blocks
                    .OrderBy(b => b.Ad).ToListAsync();
                return View(model);
            }

            try
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    TempData["Error"] = "Yönetici bulunamadı.";
                    return RedirectToAction("Yoneticiler");
                }

                user.Ad = model.Ad;
                user.Soyad = model.Soyad;
                await _userManager.UpdateAsync(user);

                // Şifre değiştir
                if (!string.IsNullOrWhiteSpace(model.YeniSifre))
                {
                    var token = await _userManager
                        .GeneratePasswordResetTokenAsync(user);
                    await _userManager.ResetPasswordAsync(
                        user, token, model.YeniSifre);
                }

                // Blok atamalarını güncelle
                var eskiAtamalar = await _context.YoneticiBlocklar
                    .Where(y => y.YoneticiId == model.Id)
                    .ToListAsync();
                _context.YoneticiBlocklar.RemoveRange(eskiAtamalar);

                if (model.SecilenBlokIds != null && model.SecilenBlokIds.Any())
                {
                    foreach (var blokId in model.SecilenBlokIds)
                    {
                        _context.YoneticiBlocklar.Add(new YoneticiBlock
                        {
                            YoneticiId = model.Id,
                            BlockId = blokId,
                            AtamaTarihi = DateTime.Now
                        });
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Yönetici güncellendi.";
                return RedirectToAction("Yoneticiler");
            }
            catch
            {
                TempData["Error"] = "Güncelleme sırasında hata oluştu.";
                ViewBag.Bloklar = await _context.Blocks
                    .OrderBy(b => b.Ad).ToListAsync();
                return View(model);
            }
        }

        // ───── YÖNETİCİ SİL ─────
        [HttpPost]
        public async Task<IActionResult> YoneticiSil(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "Yönetici bulunamadı.";
                    return RedirectToAction("Yoneticiler");
                }

                var roller = await _userManager.GetRolesAsync(user);
                if (roller.Contains("SuperAdmin"))
                {
                    TempData["Error"] = "SuperAdmin silinemez.";
                    return RedirectToAction("Yoneticiler");
                }

                var atamalar = await _context.YoneticiBlocklar
                    .Where(y => y.YoneticiId == id).ToListAsync();
                _context.YoneticiBlocklar.RemoveRange(atamalar);
                await _context.SaveChangesAsync();

                await _userManager.DeleteAsync(user);
                TempData["Success"] = "Yönetici silindi.";
            }
            catch
            {
                TempData["Error"] = "Silme işlemi sırasında hata oluştu.";
            }
            return RedirectToAction("Yoneticiler");
        }
    }

    // ───── ViewModels ─────
    public class YoneticiListViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roller { get; set; } = new();
        public List<string> AtandigiBloklar { get; set; } = new();
    }

    public class YoneticiEkleViewModel
    {
        [System.ComponentModel.DataAnnotations.Required(
            ErrorMessage = "Ad zorunludur.")]
        public string Ad { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(
            ErrorMessage = "Soyad zorunludur.")]
        public string Soyad { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(
            ErrorMessage = "E-posta zorunludur.")]
        [System.ComponentModel.DataAnnotations.EmailAddress]
        public string Email { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(
            ErrorMessage = "Şifre zorunludur.")]
        [System.ComponentModel.DataAnnotations.MinLength(6)]
        public string Sifre { get; set; } = string.Empty;

        public List<int> BlokIds { get; set; } = new();
    }

    public class YoneticiDuzenleViewModel
    {
        public string Id { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required]
        public string Ad { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required]
        public string Soyad { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public string? YeniSifre { get; set; }
        public List<int> SecilenBlokIds { get; set; } = new();
    }
}