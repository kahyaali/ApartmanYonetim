namespace ApartmanYonetim.Models.Entities
{
    public class Announcement
    {
        public int Id { get; set; }
        public string Baslik { get; set; } = string.Empty;
        public string Icerik { get; set; } = string.Empty;
        public DateTime YayinTarihi { get; set; }
        public bool AktifMi { get; set; } = true;
        public string OlusturanId { get; set; } = string.Empty;

        // Navigation
        public AppUser? Olusturan { get; set; }
    }
}
