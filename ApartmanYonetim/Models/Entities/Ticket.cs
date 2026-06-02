namespace ApartmanYonetim.Models.Entities
{
    public enum TicketDurum
    {
        Bekliyor = 0,
        Inceleniyor = 1,
        Tamamlandi = 2,
        Iptal = 3
    }

    public enum TicketOncelik
    {
        Dusuk = 0,
        Normal = 1,
        Yuksek = 2,
        Acil = 3
    }

    public class Ticket
    {
        public int Id { get; set; }
        public string Baslik { get; set; } = string.Empty;
        public string Aciklama { get; set; } = string.Empty;
        public TicketDurum Durum { get; set; } = TicketDurum.Bekliyor;
        public TicketOncelik Oncelik { get; set; } = TicketOncelik.Normal;
        public string Kategori { get; set; } = string.Empty;
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        public DateTime? KapanmaTarihi { get; set; }
        public string? AdminNotu { get; set; }
        public string OlusturanId { get; set; } = string.Empty;
        public int? ApartmentId { get; set; }

        // Navigation
        public AppUser? Olusturan { get; set; }
        public Apartment? Apartment { get; set; }
    }
}