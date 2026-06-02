using ApartmanYonetim.Data;
using ApartmanYonetim.Models.Entities;

namespace ApartmanYonetim.Services
{
    public interface IAuditService
    {
        Task LogAsync(string kullaniciId, string kullaniciAdi,
            string islem, string entity,
            string? eskiDeger = null, string? yeniDeger = null,
            string? ipAdresi = null);
    }

    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string kullaniciId, string kullaniciAdi,
            string islem, string entity,
            string? eskiDeger = null, string? yeniDeger = null,
            string? ipAdresi = null)
        {
            var log = new AuditLog
            {
                KullaniciId = kullaniciId,
                KullaniciAdi = kullaniciAdi,
                Islem = islem,
                Entity = entity,
                EskiDeger = eskiDeger,
                YeniDeger = yeniDeger,
                IpAdresi = ipAdresi,
                Tarih = DateTime.Now
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}