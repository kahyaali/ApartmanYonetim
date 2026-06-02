using System.ComponentModel.DataAnnotations;

namespace ApartmanYonetim.Models.ViewModels
{
    public class AidatTanimiViewModel
    {
        [Required(ErrorMessage = "Başlık zorunludur.")]
        [Display(Name = "Aidat Türü")]
        public string Baslik { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tutar zorunludur.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Tutar 0'dan büyük olmalı.")]
        [Display(Name = "Tutar (₺)")]
        public decimal Tutar { get; set; }

        [Required(ErrorMessage = "Ay zorunludur.")]
        [Range(1, 12)]
        public int Ay { get; set; }

        [Required(ErrorMessage = "Yıl zorunludur.")]
        public int Yil { get; set; }

        [Required(ErrorMessage = "Son ödeme tarihi zorunludur.")]
        [Display(Name = "Son Ödeme Tarihi")]
        [DataType(DataType.Date)]
        public DateTime SonOdemeTarihi { get; set; }

        [Display(Name = "Tüm Dolu Dairelere Uygula")]
        public bool TumDairelereUygula { get; set; } = true;
    }
}