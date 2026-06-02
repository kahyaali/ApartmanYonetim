
using System.ComponentModel.DataAnnotations;
namespace ApartmanYonetim.Models.ViewModels
{
    public class ResidentViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad zorunludur.")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur.")]
        public string Soyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Daire seçiniz.")]
        [Display(Name = "Daire")]
        public int ApartmentId { get; set; }

        [Required(ErrorMessage = "Taşınma tarihi zorunludur.")]
        [Display(Name = "Taşınma Tarihi")]
        [DataType(DataType.Date)]
        public DateTime TasinmaTarihi { get; set; } = DateTime.Today;

        [Display(Name = "Ayrılma Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? AyrilmaTarihi { get; set; }

        [Display(Name = "Aktif")]
        public bool AktifMi { get; set; } = true;
    }
    public class UserWithRoleViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int? DaireNo { get; set; }
        public string? BlokAdi { get; set; }
        public string Rol { get; set; } = string.Empty;
    }
}
