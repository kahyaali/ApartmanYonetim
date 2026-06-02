using ApartmanYonetim.Data;
using ApartmanYonetim.Models.Entities;
using ApartmanYonetim.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace ApartmanYonetim.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ExpenseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExpenseController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var giderler = await _context.Expenses
                .OrderByDescending(e => e.GiderTarihi)
                .ToListAsync();

            ViewBag.ToplamGider = giderler.Sum(e => e.Tutar);
            ViewBag.BuAyGider = giderler
                .Where(e => e.GiderTarihi.Month == DateTime.Now.Month
                         && e.GiderTarihi.Year == DateTime.Now.Year)
                .Sum(e => e.Tutar);

            return View(giderler);
        }
        [HttpGet]
        public IActionResult Create() => View(new ExpenseViewModel());

        [HttpPost]
        public async Task<IActionResult> Create(ExpenseViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var expense = new Expense
            {
                FirmaAdi = model.FirmaAdi,
                CalismaTuru = model.CalismaTuru,
                Aciklama = model.Aciklama,
                Tutar = model.Tutar,
                GiderTarihi = model.GiderTarihi,
                FaturaNo = model.FaturaNo
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Gider kaydı eklendi.";
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null) return NotFound();

            var model = new ExpenseViewModel
            {
                Id = expense.Id,
                FirmaAdi = expense.FirmaAdi,
                CalismaTuru = expense.CalismaTuru,
                Aciklama = expense.Aciklama,
                Tutar = expense.Tutar,
                GiderTarihi = expense.GiderTarihi,
                FaturaNo = expense.FaturaNo
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(ExpenseViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var expense = await _context.Expenses.FindAsync(model.Id);
            if (expense == null) return NotFound();

            expense.FirmaAdi = model.FirmaAdi;
            expense.CalismaTuru = model.CalismaTuru;
            expense.Aciklama = model.Aciklama;
            expense.Tutar = model.Tutar;
            expense.GiderTarihi = model.GiderTarihi;
            expense.FaturaNo = model.FaturaNo;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Gider güncellendi.";
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null) return NotFound();

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Gider silindi.";
            return RedirectToAction("Index");
        }
    }
}
