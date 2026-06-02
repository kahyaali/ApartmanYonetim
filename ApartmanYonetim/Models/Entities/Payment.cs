namespace ApartmanYonetim.Models.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public int ApartmentId { get; set; }
        public string Aciklama { get; set; } = string.Empty;
        public decimal Tutar { get; set; }
        public DateTime SonOdemeTarihi { get; set; }
        public bool Odendi { get; set; } = false;
        public int? AidatTanimiId { get; set; }

        // Hesaplanan alanlar
        public decimal OdenenTutar => Transactions?.Sum(t => t.OdenenTutar) ?? 0;
        public decimal KalanTutar => Tutar - OdenenTutar;

        // Navigation
        public Apartment Apartment { get; set; } = null!;
        public AidatTanimi? AidatTanimi { get; set; }
        public ICollection<PaymentTransaction> Transactions { get; set; }
            = new List<PaymentTransaction>();
    }
}
