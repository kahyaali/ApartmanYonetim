using System.ComponentModel.DataAnnotations;

namespace ApartmanYonetim.Models.ViewModels
{
    public class ToplantiViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Başlık zorunludur.")]
        [MaxLength(200)]
        public string Baslik { get; set; } = string.Empty;

        public string? Aciklama { get; set; }

        [Required(ErrorMessage = "Tarih zorunludur.")]
        public DateTime Tarih { get; set; } = DateTime.Now.AddDays(7);

        public string? Konum { get; set; }
        public string? Notlar { get; set; }
        public ApartmanYonetim.Models.Entities.ToplantiDurum Durum { get; set; }
    }

    public class AnketViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Başlık zorunludur.")]
        [MaxLength(200)]
        public string Baslik { get; set; } = string.Empty;

        public string? Aciklama { get; set; }

        [Required]
        public DateTime BitisTarihi { get; set; } = DateTime.Now.AddDays(7);

        [Required(ErrorMessage = "En az 2 seçenek gereklidir.")]
        public List<string> Siklar { get; set; } = new() { "", "" };
    }

    public class BelgeViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad zorunludur.")]
        [MaxLength(200)]
        public string Ad { get; set; } = string.Empty;

        public string? Aciklama { get; set; }
        public ApartmanYonetim.Models.Entities.BelgeKategori Kategori { get; set; }
        public bool HerkesGorebilir { get; set; } = true;
        public IFormFile? Dosya { get; set; }
    }

    public class ButceViewModel
    {
        public int Id { get; set; }

        [Required]
        public int Yil { get; set; } = DateTime.Now.Year;

        public string? Aciklama { get; set; }
        public List<ButceKalemViewModel> Kalemler { get; set; } = new();
    }

    public class ButceKalemViewModel
    {
        public int Id { get; set; }
        public string Kategori { get; set; } = string.Empty;
        public string Aciklama { get; set; } = string.Empty;
        public ApartmanYonetim.Models.Entities.ButceKalemTipi Tip { get; set; }
        public decimal PlanlananTutar { get; set; }
        public decimal GerceklesenTutar { get; set; }
        public int Ay { get; set; } = DateTime.Now.Month;
    }
}