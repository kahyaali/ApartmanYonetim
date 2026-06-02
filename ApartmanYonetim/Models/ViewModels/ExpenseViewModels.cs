
using System.ComponentModel.DataAnnotations;
namespace ApartmanYonetim.Models.ViewModels
{
    public class ExpenseViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Firma adı zorunludur.")]
        [Display(Name = "Firma Adı")]
        public string FirmaAdi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Çalışma türü zorunludur.")]
        [Display(Name = "Çalışma Türü")]
        public string CalismaTuru { get; set; } = string.Empty;

        [Required(ErrorMessage = "Açıklama zorunludur.")]
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tutar zorunludur.")]
        [Display(Name = "Tutar")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Tutar 0'dan büyük olmalı.")]
        public decimal Tutar { get; set; }

        [Required(ErrorMessage = "Tarih zorunludur.")]
        [Display(Name = "Gider Tarihi")]
        [DataType(DataType.Date)]
        public DateTime GiderTarihi { get; set; } = DateTime.Today;

        [Display(Name = "Fatura No")]
        public string? FaturaNo { get; set; }
    }
}
