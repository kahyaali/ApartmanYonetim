namespace ApartmanYonetim.Models.Entities
{
    public enum DemirbasKategori
    {
        Mobilya = 0,
        Elektronik = 1,
        Temizlik = 2,
        Guvenlik = 3,
        Isitma = 4,
        Aydinlatma = 5,
        Diger = 6
    }

    public enum DemirbasDurum
    {
        Aktif = 0,
        Arizali = 1,
        Bakimda = 2,
        Hurda = 3
    }

    public class Demirbas
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string? Aciklama { get; set; }
        public DemirbasKategori Kategori { get; set; } = DemirbasKategori.Diger;
        public DemirbasDurum Durum { get; set; } = DemirbasDurum.Aktif;
        public string? Konum { get; set; }
        public string? SeriNo { get; set; }
        public DateTime? AlimTarihi { get; set; }
        public decimal? Fiyat { get; set; }
        public string? Marka { get; set; }
        public string? Model { get; set; }
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<DemirbasResim> Resimler { get; set; } = new List<DemirbasResim>();
    }
}