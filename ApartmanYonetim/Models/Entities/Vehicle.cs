namespace ApartmanYonetim.Models.Entities
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string Plaka { get; set; } = string.Empty;
        public string Marka { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Renk { get; set; } = string.Empty;
        public string AracTipi { get; set; } = "Otomobil";
        public bool AktifMi { get; set; } = true;
        public DateTime KayitTarihi { get; set; } = DateTime.Now;
        public string? OtoparkNo { get; set; }
        public bool ZiyaretciMi { get; set; } = false;
        public DateTime? ZiyaretGiris { get; set; }
        public DateTime? ZiyaretCikis { get; set; }
        public string? Aciklama { get; set; }

        // Navigation
        public int? ApartmentId { get; set; }
        public Apartment? Apartment { get; set; }
        public string? KaydedenId { get; set; }
        public AppUser? Kaydeden { get; set; }
    }
}