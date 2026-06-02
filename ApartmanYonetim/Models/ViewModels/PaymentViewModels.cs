using System.ComponentModel.DataAnnotations;

namespace ApartmanYonetim.Models.ViewModels
{
    public class PaymentViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Daire seçiniz.")]
        [Display(Name = "Daire")]
        public int ApartmentId { get; set; }

        [Required(ErrorMessage = "Açıklama zorunludur.")]
        public string Aciklama { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tutar zorunludur.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Tutar 0'dan büyük olmalı.")]
        public decimal Tutar { get; set; }

        [Required(ErrorMessage = "Son ödeme tarihi zorunludur.")]
        [DataType(DataType.Date)]
        public DateTime SonOdemeTarihi { get; set; } = DateTime.Today.AddDays(30);
    }

    public class PaymentListViewModel
    {
        public int Id { get; set; }
        public int DaireNo { get; set; }
        public string? BlokAdi { get; set; }
        public string Aciklama { get; set; } = string.Empty;
        public decimal Tutar { get; set; }
        public decimal OdenenTutar { get; set; }
        public decimal Kalan { get; set; }
        public DateTime SonOdemeTarihi { get; set; }
        public bool Odendi { get; set; }
        public string RowClass { get; set; } = string.Empty;
        public string Durum { get; set; } = string.Empty;
        public string DurumClass { get; set; } = string.Empty;
        public int TransactionSayisi { get; set; }
    }

    public class PaymentDetailViewModel
    {
        public int Id { get; set; }
        public string BlokAdi { get; set; } = string.Empty;
        public int DaireNo { get; set; }
        public string Aciklama { get; set; } = string.Empty;
        public decimal Tutar { get; set; }
        public decimal OdenenTutar { get; set; }
        public decimal KalanTutar { get; set; }
        public DateTime SonOdemeTarihi { get; set; }
        public bool Odendi { get; set; }
        public string RowClass { get; set; } = string.Empty;
        public string Durum { get; set; } = string.Empty;
        public List<TransactionViewModel> Transactions { get; set; } = new();
    }

    public class TransactionViewModel
    {
        public int Id { get; set; }
        public int PaymentId { get; set; }

        [Required(ErrorMessage = "Tutar zorunludur.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Tutar 0'dan büyük olmalı.")]
        [Display(Name = "Ödenen Tutar")]
        public decimal OdenenTutar { get; set; }

        [Required(ErrorMessage = "Tarih zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Ödeme Tarihi")]
        public DateTime OdemeTarihi { get; set; } = DateTime.Today;

        [Display(Name = "Açıklama / Not")]
        public string? Aciklama { get; set; }

        [Display(Name = "Ödeyen Kişi")]
        public string? OdeyenKisi { get; set; }
    }
}