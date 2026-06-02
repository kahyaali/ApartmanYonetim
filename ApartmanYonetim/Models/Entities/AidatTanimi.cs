namespace ApartmanYonetim.Models.Entities
{
    public class AidatTanimi
    {
        public int Id { get; set; }
        public string Baslik { get; set; } = string.Empty;
        public decimal Tutar { get; set; }
        public int Ay { get; set; }
        public int Yil { get; set; }
        public DateTime SonOdemeTarihi { get; set; }
        public bool TumDairelereUygula { get; set; } = true;
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}