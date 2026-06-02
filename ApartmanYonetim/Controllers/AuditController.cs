using ApartmanYonetim.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApartmanYonetim.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuditController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var logs = await _context.AuditLogs
                .OrderByDescending(l => l.Tarih)
                .Take(500)
                .ToListAsync();
            return View(logs);
        }

        [HttpPost]
        public async Task<IActionResult> Clear()
        {
            var eskiKayitlar = _context.AuditLogs
                .Where(l => l.Tarih < DateTime.Now.AddMonths(-3));
            _context.AuditLogs.RemoveRange(eskiKayitlar);
            await _context.SaveChangesAsync();
            TempData["Success"] = "3 aydan eski loglar temizlendi.";
            return RedirectToAction("Index");
        }
    }
}