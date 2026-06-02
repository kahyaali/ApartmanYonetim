namespace ApartmanYonetim.Models.Entities
{
    public class Anket
    {
        public int Id { get; set; }
        public string Baslik { get; set; } = string.Empty;
        public string? Aciklama { get; set; }
        public DateTime BaslangicTarihi { get; set; } = DateTime.Now;
        public DateTime BitisTarihi { get; set; }
        public bool AktifMi { get; set; } = true;
        public string OlusturanId { get; set; } = string.Empty;
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        public AppUser? Olusturan { get; set; }
        public ICollection<AnketSik> Siklar { get; set; } = new List<AnketSik>();
        public ICollection<AnketOy> Oylar { get; set; } = new List<AnketOy>();
    }

    public class AnketSik
    {
        public int Id { get; set; }
        public int AnketId { get; set; }
        public string Metin { get; set; } = string.Empty;
        public int SiraNo { get; set; } = 0;

        public Anket Anket { get; set; } = null!;
        public ICollection<AnketOy> Oylar { get; set; } = new List<AnketOy>();
    }

    public class AnketOy
    {
        public int Id { get; set; }
        public int AnketId { get; set; }
        public int AnketSikId { get; set; }
        public string KullaniciId { get; set; } = string.Empty;
        public DateTime OyTarihi { get; set; } = DateTime.Now;

        public Anket Anket { get; set; } = null!;
        public AnketSik Sik { get; set; } = null!;
        public AppUser? Kullanici { get; set; }
    }
}