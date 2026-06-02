namespace ApartmanYonetim.Models.Entities
{
    public enum KargoDurum
    {
        Bekliyor = 0,
        Teslim = 1,
        Iade = 2
    }

    public class Kargo
    {
        public int Id { get; set; }
        public string AliciAd { get; set; } = string.Empty;
        public string? KargoFirma { get; set; }
        public string? TakipNo { get; set; }
        public string? Aciklama { get; set; }
        public KargoDurum Durum { get; set; } = KargoDurum.Bekliyor;
        public DateTime GelisTarihi { get; set; } = DateTime.Now;
        public DateTime? TeslimTarihi { get; set; }
        public string? TeslimAlan { get; set; }
        public int? ApartmentId { get; set; }

        // Navigation
        public Apartment? Apartment { get; set; }
    }
}