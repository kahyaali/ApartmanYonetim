namespace ApartmanYonetim.Models.Entities
{
    public class Apartment
    {
        public int Id { get; set; }
        public int DaireNo { get; set; }
        public int Kat { get; set; }
        public string Tipi { get; set; } = string.Empty;
        public decimal MetreKare { get; set; }
        public bool Dolu { get; set; } = false;
        public int? BlockId { get; set; }

        public Block? Block { get; set; }
        public ICollection<Resident> Residents { get; set; } = new List<Resident>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}