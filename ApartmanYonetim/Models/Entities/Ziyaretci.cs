// Models/Entities/Ziyaretci.cs
namespace ApartmanYonetim.Models.Entities
{
    public enum ZiyaretciDurum
    {
        Bekliyor = 0,
        Icerde = 1,
        Cikti = 2,
        Reddedildi = 3
    }

    public class Ziyaretci
    {
        public int Id { get; set; }
        public string AdSoyad { get; set; } = string.Empty;
        public string? Telefon { get; set; }
        public string? AracPlaka { get; set; }
        public string? Aciklama { get; set; }
        public ZiyaretciDurum Durum { get; set; } = ZiyaretciDurum.Bekliyor;
        public DateTime BeklenenGirisSaati { get; set; } = DateTime.Now;
        public DateTime? GercekGirisSaati { get; set; }
        public DateTime? CikisSaati { get; set; }
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        // Hangi daireyi ziyaret ediyor
        public int? ApartmentId { get; set; }
        public Apartment? Apartment { get; set; }

        // Kim davet etti
        public string DavetEdenId { get; set; } = string.Empty;
        public AppUser? DavetEden { get; set; }

        // Kapıcı/Admin kim onayladı
        public string? OnaylayanId { get; set; }
        public AppUser? Onaylayan { get; set; }
    }
}