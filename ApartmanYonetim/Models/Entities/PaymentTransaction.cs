namespace ApartmanYonetim.Models.Entities
{
    public class PaymentTransaction
    {
        public int Id { get; set; }
        public int PaymentId { get; set; }
        public decimal OdenenTutar { get; set; }
        public DateTime OdemeTarihi { get; set; } = DateTime.Now;
        public string? Aciklama { get; set; }
        public string? OdeyenKisi { get; set; }

        // Navigation
        public Payment Payment { get; set; } = null!;
    }
}
