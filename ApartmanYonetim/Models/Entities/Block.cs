namespace ApartmanYonetim.Models.Entities
{
    public class Block
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string? Aciklama { get; set; }
        public bool AktifMi { get; set; } = true;

        // Navigation
        public ICollection<Apartment> Apartments { get; set; } = new List<Apartment>();
    }
}
