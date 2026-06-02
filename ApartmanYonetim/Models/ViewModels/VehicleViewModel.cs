using System.ComponentModel.DataAnnotations;

namespace ApartmanYonetim.Models.ViewModels
{
    public class VehicleViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Plaka zorunludur.")]
        [MaxLength(20)]
        public string Plaka { get; set; } = string.Empty;

        [Required(ErrorMessage = "Marka zorunludur.")]
        public string Marka { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;
        public string Renk { get; set; } = string.Empty;

        [Required]
        public string AracTipi { get; set; } = "Otomobil";

        public string? OtoparkNo { get; set; }
        public bool ZiyaretciMi { get; set; } = false;
        public int? ApartmentId { get; set; }
        public string? Aciklama { get; set; }
    }
}