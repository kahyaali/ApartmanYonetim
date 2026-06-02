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
    public class BelgeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public BelgeController(ApplicationDbContext context,
            UserManager<AppUser> userManager,
            IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // ───── LİSTE ─────
        public async Task<IActionResult> Index(BelgeKategori? kategori, string? ara)
        {
            try
            {
                var isAdmin = User.IsInRole("Admin");

                var query = _context.Belgeler
                    .Include(b => b.Yukleyen)
                    .Where(b => isAdmin || b.HerkesGorebilir)
                    .AsQueryable();

                if (kategori.HasValue)
                    query = query.Where(b => b.Kategori == kategori);

                if (!string.IsNullOrWhiteSpace(ara))
                    query = query.Where(b => b.Ad.Contains(ara) ||
                        (b.Aciklama != null && b.Aciklama.Contains(ara)));

                var belgeler = await query
                    .OrderByDescending(b => b.YuklemeTarihi)
                    .ToListAsync();

                ViewBag.SecilenKategori = kategori;
                ViewBag.AramaMetni = ara;
                ViewBag.ToplamBelge = belgeler.Count;

                return View(belgeler);
            }
            catch
            {
                TempData["Error"] = "Belgeler yüklenirken hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }

        // ───── YÜKLE GET ─────
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Upload() => View(new BelgeViewModel());

        // ───── YÜKLE POST ─────
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Upload(BelgeViewModel model)
        {
            ModelState.Remove("Dosya");

            if (model.Dosya == null || model.Dosya.Length == 0)
            {
                ModelState.AddModelError("Dosya", "Lütfen bir dosya seçiniz.");
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.Ad))
            {
                ModelState.AddModelError("Ad", "Belge adı zorunludur.");
                return View(model);
            }

            try
            {
                var uzanti = Path.GetExtension(model.Dosya.FileName).ToLower();
                var izinli = new[] { ".pdf", ".doc", ".docx", ".xlsx",
                    ".xls", ".jpg", ".jpeg", ".png", ".txt", ".zip" };

                if (!izinli.Contains(uzanti))
                {
                    ModelState.AddModelError("Dosya",
                        "Desteklenmeyen dosya formatı. " +
                        "İzin verilenler: PDF, Word, Excel, JPG, PNG, TXT, ZIP");
                    return View(model);
                }

                // Max 20MB
                if (model.Dosya.Length > 20 * 1024 * 1024)
                {
                    ModelState.AddModelError("Dosya",
                        "Dosya boyutu 20MB'dan büyük olamaz.");
                    return View(model);
                }

                var user = await _userManager.GetUserAsync(User);
                var uploadPath = Path.Combine(
                    _env.WebRootPath, "uploads", "belgeler");
                Directory.CreateDirectory(uploadPath);

                var dosyaAdi = $"{Guid.NewGuid()}{uzanti}";
                var dosyaYolu = Path.Combine(uploadPath, dosyaAdi);

                using var stream = new FileStream(dosyaYolu, FileMode.Create);
                await model.Dosya.CopyToAsync(stream);

                var belge = new Belge
                {
                    Ad = model.Ad.Trim(),
                    Aciklama = model.Aciklama?.Trim(),
                    Kategori = model.Kategori,
                    DosyaYolu = $"/uploads/belgeler/{dosyaAdi}",
                    DosyaAdi = model.Dosya.FileName,
                    DosyaTipi = uzanti.TrimStart('.').ToUpper(),
                    DosyaBoyutu = model.Dosya.Length,
                    YuklemeTarihi = DateTime.Now,
                    YukleyenId = user!.Id,
                    HerkesGorebilir = model.HerkesGorebilir
                };

                _context.Belgeler.Add(belge);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Belge başarıyla yüklendi.";
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["Error"] = "Belge yüklenirken hata oluştu.";
                return View(model);
            }
        }

        // ───── DÜZENLE GET ─────
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var belge = await _context.Belgeler.FindAsync(id);
                if (belge == null)
                {
                    TempData["Error"] = "Belge bulunamadı.";
                    return RedirectToAction("Index");
                }

                var model = new BelgeViewModel
                {
                    Id = belge.Id,
                    Ad = belge.Ad,
                    Aciklama = belge.Aciklama,
                    Kategori = belge.Kategori,
                    HerkesGorebilir = belge.HerkesGorebilir
                };

                ViewBag.MevcutDosyaAdi = belge.DosyaAdi;
                ViewBag.MevcutDosyaTipi = belge.DosyaTipi;
                ViewBag.MevcutDosyaBoyutu = belge.DosyaBoyutu < 1024 * 1024
                    ? $"{belge.DosyaBoyutu / 1024} KB"
                    : $"{belge.DosyaBoyutu / (1024 * 1024)} MB";

                return View(model);
            }
            catch
            {
                TempData["Error"] = "Belge yüklenirken hata oluştu.";
                return RedirectToAction("Index");
            }
        }

        // ───── DÜZENLE POST ─────
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(BelgeViewModel model)
        {
            ModelState.Remove("Dosya");

            if (string.IsNullOrWhiteSpace(model.Ad))
            {
                ModelState.AddModelError("Ad", "Belge adı zorunludur.");
                return View(model);
            }

            try
            {
                var belge = await _context.Belgeler.FindAsync(model.Id);
                if (belge == null)
                {
                    TempData["Error"] = "Belge bulunamadı.";
                    return RedirectToAction("Index");
                }

                belge.Ad = model.Ad.Trim();
                belge.Aciklama = model.Aciklama?.Trim();
                belge.Kategori = model.Kategori;
                belge.HerkesGorebilir = model.HerkesGorebilir;

                // Yeni dosya yüklendiyse güncelle
                if (model.Dosya != null && model.Dosya.Length > 0)
                {
                    var uzanti = Path.GetExtension(model.Dosya.FileName).ToLower();
                    var izinli = new[] { ".pdf", ".doc", ".docx", ".xlsx",
                        ".xls", ".jpg", ".jpeg", ".png", ".txt", ".zip" };

                    if (!izinli.Contains(uzanti))
                    {
                        ModelState.AddModelError("Dosya",
                            "Desteklenmeyen dosya formatı.");
                        return View(model);
                    }

                    // Eski dosyayı sil
                    EskiDosyaSil(belge.DosyaYolu);

                    // Yeni dosyayı kaydet
                    var uploadPath = Path.Combine(
                        _env.WebRootPath, "uploads", "belgeler");
                    Directory.CreateDirectory(uploadPath);

                    var dosyaAdi = $"{Guid.NewGuid()}{uzanti}";
                    var dosyaYolu = Path.Combine(uploadPath, dosyaAdi);

                    using var stream = new FileStream(dosyaYolu, FileMode.Create);
                    await model.Dosya.CopyToAsync(stream);

                    belge.DosyaYolu = $"/uploads/belgeler/{dosyaAdi}";
                    belge.DosyaAdi = model.Dosya.FileName;
                    belge.DosyaTipi = uzanti.TrimStart('.').ToUpper();
                    belge.DosyaBoyutu = model.Dosya.Length;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Belge güncellendi.";
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["Error"] = "Belge güncellenirken hata oluştu.";
                return View(model);
            }
        }

        // ───── İNDİR ─────
        public async Task<IActionResult> Indir(int id)
        {
            try
            {
                var isAdmin = User.IsInRole("Admin");
                var belge = await _context.Belgeler.FindAsync(id);

                if (belge == null)
                {
                    TempData["Error"] = "Belge bulunamadı.";
                    return RedirectToAction("Index");
                }

                if (!isAdmin && !belge.HerkesGorebilir)
                {
                    TempData["Error"] = "Bu belgeye erişim yetkiniz yok.";
                    return RedirectToAction("Index");
                }

                var dosyaYolu = Path.Combine(
                    _env.WebRootPath,
                    belge.DosyaYolu.TrimStart('/')
                        .Replace('/', Path.DirectorySeparatorChar));

                if (!System.IO.File.Exists(dosyaYolu))
                {
                    TempData["Error"] = "Dosya sunucuda bulunamadı.";
                    return RedirectToAction("Index");
                }

                var bytes = await System.IO.File.ReadAllBytesAsync(dosyaYolu);
                var contentType = belge.DosyaTipi.ToLower() switch
                {
                    "pdf" => "application/pdf",
                    "docx" => "application/vnd.openxmlformats-officedocument" +
                              ".wordprocessingml.document",
                    "doc" => "application/msword",
                    "xlsx" => "application/vnd.openxmlformats-officedocument" +
                              ".spreadsheetml.sheet",
                    "xls" => "application/vnd.ms-excel",
                    "jpg" or "jpeg" => "image/jpeg",
                    "png" => "image/png",
                    "txt" => "text/plain",
                    "zip" => "application/zip",
                    _ => "application/octet-stream"
                };

                return File(bytes, contentType, belge.DosyaAdi);
            }
            catch
            {
                TempData["Error"] = "Dosya indirilirken hata oluştu.";
                return RedirectToAction("Index");
            }
        }

        // ───── ÖNIZLEME (PDF/Resim için) ─────
        public async Task<IActionResult> Onizle(int id)
        {
            try
            {
                var isAdmin = User.IsInRole("Admin");
                var belge = await _context.Belgeler.FindAsync(id);

                if (belge == null || (!isAdmin && !belge.HerkesGorebilir))
                    return NotFound();

                var dosyaYolu = Path.Combine(
                    _env.WebRootPath,
                    belge.DosyaYolu.TrimStart('/')
                        .Replace('/', Path.DirectorySeparatorChar));

                if (!System.IO.File.Exists(dosyaYolu))
                    return NotFound();

                var bytes = await System.IO.File.ReadAllBytesAsync(dosyaYolu);
                var contentType = belge.DosyaTipi.ToLower() switch
                {
                    "pdf" => "application/pdf",
                    "jpg" or "jpeg" => "image/jpeg",
                    "png" => "image/png",
                    _ => "application/octet-stream"
                };

                // inline — tarayıcıda aç
                Response.Headers.Append("Content-Disposition",
                    $"inline; filename=\"{belge.DosyaAdi}\"");
                return File(bytes, contentType);
            }
            catch
            {
                return NotFound();
            }
        }

        // ───── SİL ─────
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var belge = await _context.Belgeler.FindAsync(id);
                if (belge == null)
                {
                    TempData["Error"] = "Belge bulunamadı.";
                    return RedirectToAction("Index");
                }

                EskiDosyaSil(belge.DosyaYolu);
                _context.Belgeler.Remove(belge);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Belge silindi.";
            }
            catch
            {
                TempData["Error"] = "Belge silinirken hata oluştu.";
            }
            return RedirectToAction("Index");
        }

        // ───── YARDIMCI ─────
        private void EskiDosyaSil(string dosyaYolu)
        {
            try
            {
                var fullPath = Path.Combine(
                    _env.WebRootPath,
                    dosyaYolu.TrimStart('/')
                        .Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }
            catch { }
        }
    }
}