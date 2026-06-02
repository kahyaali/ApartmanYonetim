namespace ApartmanYonetim.Models.ViewModels
{
    public class KullaniciDetayViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roller { get; set; } = new();
        public string? DaireNo { get; set; }
        public string? BlokAd { get; set; }
    }
}
