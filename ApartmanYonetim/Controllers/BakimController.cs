// Controllers/BakimController.cs
using ApartmanYonetim.Data;
using ApartmanYonetim.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ApartmanYonetim.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BakimController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public BakimController(ApplicationDbContext context,
            UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(BakimDurum? durum, string? kategori)
        {
            try
            {
                var query = _context.BakimGorevler
                    .Include(b => b.Demirbas)
                    .Include(b => b.Olusturan)
                    .AsQueryable();

                if (durum.HasValue)
                    query = query.Where(b => b.Durum == durum);

                if (!string.IsNullOrEmpty(kategori))
                    query = query.Where(b => b.Kategori == kategori);

                var gorevler = await query
                    .OrderBy(b => b.PlanlananTarih)
                    .ToListAsync();

                var bugun = DateTime.Today;
                ViewBag.Bekleyen = gorevler.Count(b =>
                    b.Durum == BakimDurum.Bekliyor);
                ViewBag.Gecmis = gorevler.Count(b =>
                    b.Durum == BakimDurum.Bekliyor &&
                    b.PlanlananTarih.Date < bugun);
                ViewBag.BuHafta = gorevler.Count(b =>
                    b.Durum == BakimDurum.Bekliyor &&
                    b.PlanlananTarih.Date >= bugun &&
                    b.PlanlananTarih.Date <= bugun.AddDays(7));
                ViewBag.Tamamlanan = gorevler.Count(b =>
                    b.Durum == BakimDurum.Tamamlandi);

                ViewBag.SecilenDurum = durum;
                ViewBag.SecilenKategori = kategori;
                ViewBag.Kategoriler = gorevler
                    .Select(b => b.Kategori)
                    .Distinct()
                    .OrderBy(k => k)
                    .ToList();

                return View(gorevler);
            }
            catch
            {
                TempData["Error"] = "Bakım görevleri yüklenirken hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Ekle()
        {
            await DemirbasListesiYukle();
            return View(new BakimGorev
            {
                PlanlananTarih = DateTime.Now.AddDays(7)
            });
        }

        [HttpPost]
        public async Task<IActionResult> Ekle(BakimGorev model)
        {
            ModelState.Remove("OlusturanId");
            ModelState.Remove("Olusturan");
            ModelState.Remove("Demirbase");

            if (!ModelState.IsValid)
            {
                await DemirbasListesiYukle();
                return View(model);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                model.OlusturanId = user!.Id;
                model.Durum = BakimDurum.Bekliyor;
                model.OlusturmaTarihi = DateTime.Now;

                _context.BakimGorevler.Add(model);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Bakım görevi eklendi.";
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["Error"] = "Bakım görevi eklenirken hata oluştu.";
                await DemirbasListesiYukle();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Duzenle(int id)
        {
            var gorev = await _context.BakimGorevler.FindAsync(id);
            if (gorev == null)
            {
                TempData["Error"] = "Görev bulunamadı.";
                return RedirectToAction("Index");
            }
            await DemirbasListesiYukle(gorev.DemirbasId);
            return View(gorev);
        }

        [HttpPost]
        public async Task<IActionResult> Duzenle(BakimGorev model)
        {
            ModelState.Remove("OlusturanId");
            ModelState.Remove("Olusturan");
            ModelState.Remove("Demirbase");

            if (!ModelState.IsValid)
            {
                await DemirbasListesiYukle(model.DemirbasId);
                return View(model);
            }

            try
            {
                var gorev = await _context.BakimGorevler.FindAsync(model.Id);
                if (gorev == null)
                {
                    TempData["Error"] = "Görev bulunamadı.";
                    return RedirectToAction("Index");
                }

                gorev.Baslik = model.Baslik;
                gorev.Aciklama = model.Aciklama;
                gorev.Kategori = model.Kategori;
                gorev.Periyot = model.Periyot;
                gorev.Durum = model.Durum;
                gorev.PlanlananTarih = model.PlanlananTarih;
                gorev.Maliyet = model.Maliyet;
                gorev.Firma = model.Firma;
                gorev.Notlar = model.Notlar;
                gorev.DemirbasId = model.DemirbasId;

                if (model.Durum == BakimDurum.Tamamlandi &&
                    !gorev.TamamlanmaTarihi.HasValue)
                    gorev.TamamlanmaTarihi = DateTime.Now;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Bakım görevi güncellendi.";
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["Error"] = "Güncelleme sırasında hata oluştu.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Tamamla(int id)
        {
            try
            {
                var gorev = await _context.BakimGorevler.FindAsync(id);
                if (gorev == null)
                {
                    TempData["Error"] = "Görev bulunamadı.";
                    return RedirectToAction("Index");
                }
                gorev.Durum = BakimDurum.Tamamlandi;
                gorev.TamamlanmaTarihi = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Görev tamamlandı olarak işaretlendi.";
            }
            catch
            {
                TempData["Error"] = "İşlem gerçekleştirilemedi.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Sil(int id)
        {
            try
            {
                var gorev = await _context.BakimGorevler.FindAsync(id);
                if (gorev == null)
                {
                    TempData["Error"] = "Görev bulunamadı.";
                    return RedirectToAction("Index");
                }
                _context.BakimGorevler.Remove(gorev);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Bakım görevi silindi.";
            }
            catch
            {
                TempData["Error"] = "Silme işlemi başarısız.";
            }
            return RedirectToAction("Index");
        }

        private async Task DemirbasListesiYukle(int? secilenId = null)
        {
            var demirbaslar = await _context.Demirbaslar
                .OrderBy(d => d.Ad)
                .Select(d => new { d.Id, d.Ad })
                .ToListAsync();

            ViewBag.Demirbaslar = new SelectList(
                demirbaslar, "Id", "Ad", secilenId);
        }
    }
}