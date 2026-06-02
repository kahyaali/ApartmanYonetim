namespace ApartmanYonetim.Models.Entities
{
    public class FavoriSayfa
    {
        public int Id { get; set; }
        public string KullaniciId { get; set; } = string.Empty;
        public string Baslik { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Ikon { get; set; } = "fa-bookmark";
        public int Sira { get; set; } = 0;
        public DateTime EklemeTarihi { get; set; } = DateTime.Now;

        public AppUser? Kullanici { get; set; }
    }
}