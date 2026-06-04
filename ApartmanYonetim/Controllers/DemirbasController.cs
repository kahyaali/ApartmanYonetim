using ApartmanYonetim.Data;
using ApartmanYonetim.Models.Entities;
using ApartmanYonetim.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApartmanYonetim.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DemirbasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DemirbasController(ApplicationDbContext context,
            IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(DemirbasKategori? kategori,
            DemirbasDurum? durum, string? ara)
        {
            try
            {
                var query = _context.Demirbaslar
                    .Include(d => d.Resimler)
                    .AsQueryable();

                if (kategori.HasValue)
                    query = query.Where(d => d.Kategori == kategori);

                if (durum.HasValue)
                    query = query.Where(d => d.Durum == durum);

                if (!string.IsNullOrWhiteSpace(ara))
                    query = query.Where(d =>
                        d.Ad.Contains(ara) ||
                        (d.SeriNo != null && d.SeriNo.Contains(ara)) ||
                        (d.Marka != null && d.Marka.Contains(ara)));

                var liste = await query
                    .OrderBy(d => d.Kategori)
                    .ThenBy(d => d.Ad)
                    .ToListAsync();

                ViewBag.ToplamAktif = await _context.Demirbaslar
                    .CountAsync(d => d.Durum == DemirbasDurum.Aktif);
                ViewBag.ToplamArizali = await _context.Demirbaslar
                    .CountAsync(d => d.Durum == DemirbasDurum.Arizali);
                ViewBag.ToplamBakim = await _context.Demirbaslar
                    .CountAsync(d => d.Durum == DemirbasDurum.Bakimda);
                ViewBag.ToplamDeger = await _context.Demirbaslar
                    .SumAsync(d => (decimal?)d.Fiyat ?? 0);

                ViewBag.SecilenKategori = kategori;
                ViewBag.SecilenDurum = durum;
                ViewBag.AramaMetni = ara;

                return View(liste);
            }
            catch
            {
                TempData["Error"] = "Demirbaşlar yüklenirken hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult Create() => View(new DemirbasViewModel());



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( DemirbasViewModel viewModel)
        {

            //ModelState.Remove("Resimler");
            if (!ModelState.IsValid)
            {
                var hatalar = ModelState.Values
           .SelectMany(v => v.Errors)
           .Select(e => e.ErrorMessage)
           .ToList();
                TempData["Error"] = string.Join(", ", hatalar);
                return View(viewModel);
            }

            try
            {
                var demirbas = new Demirbas
                {
                    Ad = viewModel.Ad,
                    Aciklama = viewModel.Aciklama,
                    Kategori = viewModel.Kategori,
                    Durum = viewModel.Durum,
                    Konum = viewModel.Konum,
                    SeriNo = viewModel.SeriNo,
                    Marka = viewModel.Marka,
                    Model = viewModel.Model,
                    AlimTarihi = viewModel.AlimTarihi,
                    Fiyat = viewModel.Fiyat,
                    OlusturmaTarihi = DateTime.Now
                };

                _context.Demirbaslar.Add(demirbas);
                await _context.SaveChangesAsync();

                // Resimleri kaydet
                if (viewModel.Resimler != null && viewModel.Resimler.Any(r => r.Length > 0))
                {
                    await ResimKaydet(demirbas.Id, viewModel.Resimler);
                }

                TempData["Success"] = "Demirbaş başarıyla eklendi.";
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["Error"] = "Demirbaş eklenirken hata oluştu.";
                return View(viewModel);
            }
        }



        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var d = await _context.Demirbaslar
                    .Include(x => x.Resimler.OrderBy(r => r.SiraNo))
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (d == null)
                {
                    TempData["Error"] = "Demirbaş bulunamadı.";
                    return RedirectToAction("Index");
                }

                var model = new DemirbasViewModel
                {
                    Id = d.Id,
                    Ad = d.Ad,
                    Aciklama = d.Aciklama,
                    Kategori = d.Kategori,
                    Durum = d.Durum,
                    Konum = d.Konum,
                    SeriNo = d.SeriNo,
                    Marka = d.Marka,
                    Model = d.Model,
                    AlimTarihi = d.AlimTarihi,
                    Fiyat = d.Fiyat ?? 0
                };

                ViewBag.MevcutResimler = d.Resimler.ToList();
                return View(model);
            }
            catch
            {
                TempData["Error"] = "Demirbaş yüklenirken hata oluştu.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DemirbasViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MevcutResimler = await _context.DemirbasResimleri
                    .Where(r => r.DemirbasId == viewModel.Id)
                    .ToListAsync();
                return View(viewModel);
            }

            try
            {
                var demirbase = await _context.Demirbaslar.FindAsync(viewModel.Id);
                if (demirbase == null)
                {
                    TempData["Error"] = "Demirbaş bulunamadı.";
                    return RedirectToAction("Index");
                }

                demirbase.Ad = viewModel.Ad;
                demirbase.Aciklama = viewModel.Aciklama;
                demirbase.Kategori = viewModel.Kategori;
                demirbase.Durum = viewModel.Durum;
                demirbase.Konum = viewModel.Konum;
                demirbase.SeriNo = viewModel.SeriNo;
                demirbase.Marka = viewModel.Marka;
                demirbase.Model = viewModel.Model;
                demirbase.AlimTarihi = viewModel.AlimTarihi;
                demirbase.Fiyat = viewModel.Fiyat;

                if (viewModel.Resimler != null && viewModel.Resimler.Any())
                {
                    await ResimKaydet(demirbase.Id, viewModel.Resimler);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Demirbaş güncellendi.";
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["Error"] = "Demirbaş güncellenirken hata oluştu.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var demirbas = await _context.Demirbaslar
                    .Include(d => d.Resimler)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (demirbas == null)
                {
                    TempData["Error"] = "Demirbaş bulunamadı.";
                    return RedirectToAction("Index");
                }

                // Resimleri diskten sil
                foreach (var resim in demirbas.Resimler)
                {
                    ResimSil(resim.ResimYolu);
                }

                _context.Demirbaslar.Remove(demirbas);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Demirbaş silindi.";
            }
            catch
            {
                TempData["Error"] = "Demirbaş silinirken hata oluştu.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Demirbas/ResimSilAsync")]
        public async Task<IActionResult> ResimSilAsync([FromQuery] int id)
        {
            try
            {
                var resim = await _context.DemirbasResimleri.FindAsync(id);
                if (resim == null)
                    return Json(new { success = false, mesaj = "Resim bulunamadı." });

                ResimSil(resim.ResimYolu);
                _context.DemirbasResimleri.Remove(resim);
                await _context.SaveChangesAsync();

                return Json(new { success = true, mesaj = "Resim silindi." });
            }
            catch
            {
                return Json(new { success = false, mesaj = "Resim silinirken hata oluştu." });
            }
        }

        // Resimleri getir (lightbox için)
        [HttpGet]
        public async Task<IActionResult> GetResimler(int id)
        {
            var resimler = await _context.DemirbasResimleri
                .Where(r => r.DemirbasId == id)
                .OrderBy(r => r.SiraNo)
                .Select(r => new { r.Id, r.ResimYolu, r.Aciklama })
                .ToListAsync();

            return Json(resimler);
        }

        // ── Yardımcı metodlar ──
        private async Task ResimKaydet(int demirbasId, List<IFormFile> dosyalar)
        {
            var uploadPath = Path.Combine(_env.WebRootPath, "uploads", "demirbaslar");
            Directory.CreateDirectory(uploadPath);

            var mevcutSira = await _context.DemirbasResimleri
                .Where(r => r.DemirbasId == demirbasId)
                .MaxAsync(r => (int?)r.SiraNo) ?? 0;

            foreach (var dosya in dosyalar)
            {
                if (dosya.Length == 0) continue;

                var uzanti = Path.GetExtension(dosya.FileName).ToLower();
                var izinliUzantilar = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                if (!izinliUzantilar.Contains(uzanti)) continue;

                var dosyaAdi = $"{Guid.NewGuid()}{uzanti}";
                var dosyaYolu = Path.Combine(uploadPath, dosyaAdi);

                using var stream = new FileStream(dosyaYolu, FileMode.Create);
                await dosya.CopyToAsync(stream);

                mevcutSira++;
                var resim = new DemirbasResim
                {
                    DemirbasId = demirbasId,
                    ResimYolu = $"/uploads/demirbaslar/{dosyaAdi}",
                    SiraNo = mevcutSira,
                    EklemeTarihi = DateTime.Now
                };
                _context.DemirbasResimleri.Add(resim);
            }

            await _context.SaveChangesAsync();
        }

        private void ResimSil(string resimYolu)
        {
            try
            {
                var fullPath = Path.Combine(_env.WebRootPath,
                    resimYolu.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }
            catch { }
        }
    }
}