namespace ApartmanYonetim.Models.Entities
{
    public class Mesaj
    {
        public int Id { get; set; }
        public string GonderenId { get; set; } = string.Empty;
        public string AliciId { get; set; } = string.Empty;
        public string Icerik { get; set; } = string.Empty;
        public DateTime GonderimTarihi { get; set; } = DateTime.Now;
        public bool Okundu { get; set; } = false;
        public DateTime? OkunmaTarihi { get; set; }
        public bool GonderenSildi { get; set; } = false;
        public bool AliciSildi { get; set; } = false;

        // Navigation
        public AppUser? Gonderen { get; set; }
        public AppUser? Alici { get; set; }
    }
}