
using ApartmanYonetim.Data;
using ApartmanYonetim.Models.Entities;
using ApartmanYonetim.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApartmanYonetim.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BildirimController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;

        public BildirimController(ApplicationDbContext context,
            UserManager<AppUser> userManager,
            INotificationService notificationService,
            IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> Gonder()
        {
            var sakinler = await _userManager
                .GetUsersInRoleAsync("Sakin");

            ViewBag.ToplamSakin = sakinler.Count;
            return View(new TopluBildirimViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Gonder(TopluBildirimViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Mesaj))
            {
                ModelState.AddModelError("Mesaj", "Mesaj zorunludur.");
                ViewBag.ToplamSakin = (await _userManager
                    .GetUsersInRoleAsync("Sakin")).Count;
                return View(model);
            }

            try
            {
                var sakinler = await _userManager.GetUsersInRoleAsync("Sakin");

                if (!sakinler.Any())
                {
                    TempData["Error"] = "Sistemde Sakin rolünde kullanıcı bulunamadı.";
                    ViewBag.ToplamSakin = 0;
                    return View(model);
                }

                int signalrGonderilen = 0;
                int mailGonderilen = 0;
                var hatalar = new List<string>();

                foreach (var sakin in sakinler)
                {
                    // SignalR
                    if (model.SignalRGonder)
                    {
                        try
                        {
                            await _notificationService.SendToUserAsync(
                                sakin.Id,
                                $"📢 {model.Mesaj}",
                                model.Tip ?? "info");
                            signalrGonderilen++;
                        }
                        catch (Exception ex)
                        {
                            hatalar.Add($"SignalR [{sakin.Email}]: {ex.Message}");
                        }
                    }

                    // Mail
                    if (model.MailGonder && !string.IsNullOrEmpty(sakin.Email))
                    {
                        try
                        {
                            await _emailService.SendAsync(
                                sakin.Email,
                                model.MailBaslik ?? "Apartman Bildirimi",
                                $"<div style='font-family:sans-serif;padding:20px;'>" +
                                $"<h3 style='color:#1e3a5f;'>" +
                                $"{model.MailBaslik ?? "Apartman Bildirimi"}</h3>" +
                                $"<p style='font-size:15px;color:#333;'>{model.Mesaj}</p>" +
                                $"<hr/>" +
                                $"<small style='color:#999;'>" +
                                $"Apartman Yönetim Sistemi</small></div>");
                            mailGonderilen++;
                        }
                        catch (Exception ex)
                        {
                            hatalar.Add($"Mail [{sakin.Email}]: {ex.Message}");
                        }
                    }
                }

                if (hatalar.Any())
                {
                    TempData["Error"] = $"Bazı hatalar: {string.Join(", ", hatalar.Take(3))}";
                }

                TempData["Success"] =
                    $"Bildirim gönderildi! " +
                    $"SignalR: {signalrGonderilen} kişi" +
                    (model.MailGonder ? $", E-posta: {mailGonderilen} kişi." : ".");

                ViewBag.ToplamSakin = sakinler.Count;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hata: {ex.Message}";
                ViewBag.ToplamSakin = (await _userManager
                    .GetUsersInRoleAsync("Sakin")).Count;
                return View(model);
            }
        }
    }

    public class TopluBildirimViewModel
    {
        [System.ComponentModel.DataAnnotations.Required(
            ErrorMessage = "Mesaj zorunludur.")]
        public string Mesaj { get; set; } = string.Empty;

        public string Tip { get; set; } = "info";
        public bool SignalRGonder { get; set; } = true;
        public bool MailGonder { get; set; } = false;
        public string? MailBaslik { get; set; }
    }
}