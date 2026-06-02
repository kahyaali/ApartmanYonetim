namespace ApartmanYonetim.Models.ViewModels
{
    public class OdemeBaslatViewModel
    {
        public int PaymentId { get; set; }
        public string KartSahibi { get; set; } = string.Empty;
        public string KartNumarasi { get; set; } = string.Empty;
        public string SonKullanmaAy { get; set; } = string.Empty;
        public string SonKullanmaYil { get; set; } = string.Empty;
        public string Cvv { get; set; } = string.Empty;
        public int Taksit { get; set; } = 1;
        public decimal Tutar { get; set; }
    }
}
