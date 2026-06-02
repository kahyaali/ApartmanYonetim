using ApartmanYonetim.Data;
using ApartmanYonetim.Models.Entities;
using ApartmanYonetim.Models.ViewModels;
using ApartmanYonetim.Services;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApartmanYonetim.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly ApplicationDbContext _context;

        public AuthController(UserManager<AppUser> userManager,
                             SignInManager<AppUser> signInManager,
                             IEmailService emailService,
                             ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        // ───── SIGNIN ─────
        [HttpGet]
        public IActionResult SignIn(string? returnUrl = null)
        {
            if (User.Identity!.IsAuthenticated) return RedirectToAction("Index", "Home");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(SignInViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "E-posta veya şifre hatalı.");
            return View(model);
        }

        // ───── SIGNUP ─────
        [HttpGet]
        public IActionResult SignUp()
        {
            if (User.Identity!.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                // 1. Email zaten kayıtlı mı?
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Bu e-posta adresi zaten kayıtlı.");
                    return View(model);
                }

                // 2. Daire no ile apartment bul
                var apartment = await _context.Apartments
                    .Include(a => a.Block)
                    .FirstOrDefaultAsync(a => a.DaireNo == model.DaireNo);

                if (apartment == null)
                {
                    ModelState.AddModelError("DaireNo",
                        "Bu daire numarası sistemde bulunamadı.");
                    return View(model);
                }

                // 3. Bu daireye ait aktif sakin var mı? Ad Soyad eşleşiyor mu?
                var sakin = await _context.Residents
                    .Where(r => r.ApartmentId == apartment.Id
                             && r.AktifMi
                             && r.Ad.ToLower() == model.Ad.ToLower().Trim()
                             && r.Soyad.ToLower() == model.Soyad.ToLower().Trim())
                    .FirstOrDefaultAsync();

                if (sakin == null)
                {
                    ModelState.AddModelError("",
                        "Sistemde bu bilgilere ait kayıtlı bir sakin bulunamadı. " +
                        "Lütfen yönetici ile iletişime geçin.");
                    return View(model);
                }

                // 4. Bu sakin için zaten kullanıcı var mı?
                var mevcutKullanici = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.ApartmentId == apartment.Id);

                if (mevcutKullanici != null)
                {
                    ModelState.AddModelError("",
                        "Bu daireye ait zaten bir kullanıcı hesabı mevcut. " +
                        "Şifrenizi mi unuttunuz?");
                    return View(model);
                }

                // 5. Kullanıcı oluştur
                var user = new AppUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Ad = model.Ad,
                    Soyad = model.Soyad,
                    ApartmentId = apartment.Id,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Sakin kaydına userId bağla
                    sakin.UserId = user.Id;
                    await _context.SaveChangesAsync();

                    await _userManager.AddToRoleAsync(user, "Sakin");
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    TempData["Success"] = $"Hoş geldiniz {user.Ad}! Hesabınız başarıyla oluşturuldu.";
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }
            catch
            {
                ModelState.AddModelError("",
                    "Kayıt sırasında beklenmedik bir hata oluştu. Lütfen tekrar deneyin.");
            }

            return View(model);
        }

        // ───── SIGNOUT ─────
        [HttpGet]
        [Authorize]
        public new async Task<IActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("SignIn");
        }

        // ───── ŞİFREMİ UNUTTUM ─────
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ViewBag.Message = "success";
                return View(model);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action("ResetPassword", "Auth",
                new { token, email = model.Email }, Request.Scheme);

            var htmlBody = $@"
                <div style='font-family:Segoe UI,sans-serif;max-width:480px;margin:auto;'>
                    <h2 style='color:#1e3a5f;'>Şifre Sıfırlama</h2>
                    <p>Merhaba <strong>{user.Ad}</strong>,</p>
                    <p>Şifrenizi sıfırlamak için aşağıdaki butona tıklayın.</p>
                    <a href='{resetLink}' style='display:inline-block;padding:12px 28px;
                        background:#1e3a5f;color:#fff;border-radius:8px;text-decoration:none;
                        font-weight:600;margin:16px 0;'>Şifremi Sıfırla</a>
                    <p style='color:#888;font-size:13px;'>Bu link 1 saat geçerlidir.</p>
                </div>";

            await _emailService.SendAsync(model.Email, "Şifre Sıfırlama", htmlBody);

            ViewBag.Message = "success";
            return View(model);
        }

        // ───── ŞİFRE SIFIRLA ─────
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return RedirectToAction("SignIn");

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
            {
                TempData["Success"] = "Şifreniz başarıyla güncellendi.";
                return RedirectToAction("SignIn");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        public IActionResult AccessDenied() => View();
    }
}
