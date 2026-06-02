namespace ApartmanYonetim.Models.Entities
{
    public enum ToplantiDurum
    {
        Planlanıyor = 0,
        Tamamlandi = 1,
        Iptal = 2
    }

    public class Toplanti
    {
        public int Id { get; set; }
        public string Baslik { get; set; } = string.Empty;
        public string? Aciklama { get; set; }
        public DateTime Tarih { get; set; }
        public string? Konum { get; set; }
        public ToplantiDurum Durum { get; set; } = ToplantiDurum.Planlanıyor;
        public string OlusturanId { get; set; } = string.Empty;
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        public string? Notlar { get; set; }

        public AppUser? Olusturan { get; set; }
    }
}