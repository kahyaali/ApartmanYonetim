namespace ApartmanYonetim.Models.Entities
{
    public enum BelgeKategori
    {
        Sozlesme = 0,
        Fatura = 1,
        Karar = 2,
        Rapor = 3,
        Diger = 4
    }

    public class Belge
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string? Aciklama { get; set; }
        public BelgeKategori Kategori { get; set; } = BelgeKategori.Diger;
        public string DosyaYolu { get; set; } = string.Empty;
        public string DosyaAdi { get; set; } = string.Empty;
        public string DosyaTipi { get; set; } = string.Empty;
        public long DosyaBoyutu { get; set; }
        public DateTime YuklemeTarihi { get; set; } = DateTime.Now;
        public string YukleyenId { get; set; } = string.Empty;
        public bool HerkesGorebilir { get; set; } = true;

        public AppUser? Yukleyen { get; set; }
    }
}