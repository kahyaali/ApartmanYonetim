namespace ApartmanYonetim.Models.Entities
{
    public class DemirbasResim
    {
        public int Id { get; set; }
        public int DemirbasId { get; set; }
        public string ResimYolu { get; set; } = string.Empty;
        public string? Aciklama { get; set; }
        public int SiraNo { get; set; } = 0;
        public DateTime EklemeTarihi { get; set; } = DateTime.Now;

        // Navigation
        public Demirbas Demirbas { get; set; } = null!;
    }
}