
namespace ApartmanYonetim.Models.Entities
{
    public enum BakimPeriyot
    {
        Teksefer = 0,
        Haftalik = 1,
        Aylik = 2,
        Ucaylik = 3,
        Yillik = 4
    }

    public enum BakimDurum
    {
        Bekliyor = 0,
        Devam = 1,
        Tamamlandi = 2,
        Iptal = 3
    }

    public class BakimGorev
    {
        public int Id { get; set; }
        public string Baslik { get; set; } = string.Empty;
        public string? Aciklama { get; set; }
        public string Kategori { get; set; } = string.Empty;
        public BakimPeriyot Periyot { get; set; } = BakimPeriyot.Teksefer;
        public BakimDurum Durum { get; set; } = BakimDurum.Bekliyor;
        public DateTime PlanlananTarih { get; set; }
        public DateTime? TamamlanmaTarihi { get; set; }
        public decimal? Maliyet { get; set; }
        public string? Firma { get; set; }
        public string? Notlar { get; set; }
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        public string OlusturanId { get; set; } = string.Empty;

        // Demirbaşa bağlı mı?
        public int? DemirbasId { get; set; }
        public Demirbas? Demirbas { get; set; }
        public AppUser? Olusturan { get; set; }
    }
}