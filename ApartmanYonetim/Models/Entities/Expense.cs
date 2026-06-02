namespace ApartmanYonetim.Models.Entities
{
    public class Expense
    {
        public int Id { get; set; }
        public string FirmaAdi { get; set; } = string.Empty;
        public string CalismaTuru { get; set;  } = string.Empty;
        public string Aciklama { get; set; } = string.Empty;
        public decimal Tutar { get; set;  }
        public DateTime GiderTarihi { get; set; }
        public string? FaturaNo { get; set; }

    }
}
