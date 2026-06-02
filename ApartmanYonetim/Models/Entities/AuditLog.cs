namespace ApartmanYonetim.Models.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string KullaniciId { get; set; } = string.Empty;
        public string KullaniciAdi { get; set; } = string.Empty;
        public string Islem { get; set; } = string.Empty;
        public string Entity { get; set; } = string.Empty;
        public string? EskiDeger { get; set; }
        public string? YeniDeger { get; set; }
        public string? IpAdresi { get; set; }
        public DateTime Tarih { get; set; } = DateTime.Now;
    }
}