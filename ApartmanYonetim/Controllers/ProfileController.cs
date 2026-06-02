using ApartmanYonetim.Data;
using ApartmanYonetim.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApartmanYonetim.Models.Entities;

namespace ApartmanYonetim.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProfileController(UserManager<AppUser> userManager, ApplicationDbContext context, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.Users
                .Include(u => u.Apartment)
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

            if (user == null) return NotFound();

            var roller = await _userManager.GetRolesAsync(user);

            var model = new ProfileViewModel
            {
                Ad = user.Ad,
                Soyad = user.Soyad,
                Email = user.Email ?? "",
                DaireNo = user.Apartment?.DaireNo,
                Rol = roller.FirstOrDefault() ?? "-"
            };

            ViewBag.ProfilFoto = user.ProfilFoto;
            ViewBag.AdSoyad = $"{user.Ad} {user.Soyad}";
            ViewBag.Email = user.Email;

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var model = new ProfileEditViewModel
            {
                Ad = user.Ad,
                Soyad = user.Soyad,
                Email = user.Email ?? ""
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ProfileEditViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.Ad = model.Ad;
            user.Soyad = model.Soyad;

            await _userManager.UpdateAsync(user);
            TempData["Success"] = "Profil güncellendi.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(
                user, model.EskiSifre, model.YeniSifre);

            if (result.Succeeded)
            {
                TempData["Success"] = "Şifreniz başarıyla güncellendi.";
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

   
        [HttpPost]
        public async Task<IActionResult> FotoYukle(IFormFile foto)
        {
            try
            {
                if (foto == null || foto.Length == 0)
                {
                    TempData["Error"] = "Lütfen bir fotoğraf seçiniz.";
                    return RedirectToAction("Index");
                }

                var uzanti = Path.GetExtension(foto.FileName).ToLower();
                if (!new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(uzanti))
                {
                    TempData["Error"] = "Sadece JPG, PNG veya WEBP yükleyebilirsiniz.";
                    return RedirectToAction("Index");
                }

                if (foto.Length > 5 * 1024 * 1024)
                {
                    TempData["Error"] = "Fotoğraf 5MB'dan büyük olamaz.";
                    return RedirectToAction("Index");
                }

                var user = await _userManager.GetUserAsync(User);

                // Eski fotoğrafı sil
                if (!string.IsNullOrEmpty(user!.ProfilFoto))
                {
                    var eskiYol = Path.Combine(
                        _env.WebRootPath,
                        user.ProfilFoto.TrimStart('/')
                            .Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(eskiYol))
                        System.IO.File.Delete(eskiYol);
                }

                // Yeni fotoğrafı kaydet
                var uploadPath = Path.Combine(
                    _env.WebRootPath, "uploads", "profil");
                Directory.CreateDirectory(uploadPath);

                var dosyaAdi = $"{user.Id}{uzanti}";
                var dosyaYolu = Path.Combine(uploadPath, dosyaAdi);

                using var stream = new FileStream(dosyaYolu, FileMode.Create);
                await foto.CopyToAsync(stream);

                user.ProfilFoto = $"/uploads/profil/{dosyaAdi}";
                await _userManager.UpdateAsync(user);

                TempData["Success"] = "Profil fotoğrafı güncellendi.";
            }
            catch
            {
                TempData["Error"] = "Fotoğraf yüklenirken hata oluştu.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> FotoSil()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (!string.IsNullOrEmpty(user!.ProfilFoto))
                {
                    var dosyaYolu = Path.Combine(
                        _env.WebRootPath,
                        user.ProfilFoto.TrimStart('/')
                            .Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(dosyaYolu))
                        System.IO.File.Delete(dosyaYolu);

                    user.ProfilFoto = null;
                    await _userManager.UpdateAsync(user);
                }
                TempData["Success"] = "Profil fotoğrafı kaldırıldı.";
            }
            catch
            {
                TempData["Error"] = "İşlem gerçekleştirilemedi.";
            }
            return RedirectToAction("Index");
        }
    }
}