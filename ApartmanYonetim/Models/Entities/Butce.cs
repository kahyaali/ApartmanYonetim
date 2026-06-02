namespace ApartmanYonetim.Models.Entities
{
    public enum ButceKalemTipi
    {
        Gelir = 0,
        Gider = 1
    }

    public class ButcePlan
    {
        public int Id { get; set; }
        public int Yil { get; set; }
        public string? Aciklama { get; set; }
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        public string OlusturanId { get; set; } = string.Empty;

        public AppUser? Olusturan { get; set; }
        public ICollection<ButceKalem> Kalemler { get; set; } = new List<ButceKalem>();
    }

    public class ButceKalem
    {
        public int Id { get; set; }
        public int ButcePlanId { get; set; }
        public string Kategori { get; set; } = string.Empty;
        public string Aciklama { get; set; } = string.Empty;
        public ButceKalemTipi Tip { get; set; }
        public decimal PlanlananTutar { get; set; }
        public decimal GerceklesenTutar { get; set; } = 0;
        public int Ay { get; set; }

        public ButcePlan ButcePlan { get; set; } = null!;
    }
}