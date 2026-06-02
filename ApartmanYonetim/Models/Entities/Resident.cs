namespace ApartmanYonetim.Models.Entities
{
    public class Resident
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public int ApartmentId { get; set; }
        public DateTime TasinmaTarihi { get; set; }
        public DateTime? AyrilmaTarihi { get; set; }
        public bool AktifMi { get; set; } = true;

        // Navigation
        public Apartment Apartment { get; set; } = null!;
        public string? UserId { get; set; }
        public AppUser? User { get; set; }
    }
}
