namespace ApartmanYonetim.Models.Entities
{
    // Bir yönetici birden fazla bloğa atanabilir
    public class YoneticiBlock
    {
        public int Id { get; set; }
        public string YoneticiId { get; set; } = string.Empty;
        public int BlockId { get; set; }
        public DateTime AtamaTarihi { get; set; } = DateTime.Now;

        public AppUser? Yonetici { get; set; }
        public Block? Block { get; set; }
    }
}