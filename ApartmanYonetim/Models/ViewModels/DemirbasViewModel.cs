using System.ComponentModel.DataAnnotations;
using ApartmanYonetim.Models.Entities;

namespace ApartmanYonetim.Models.ViewModels
{
    public class DemirbasViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad zorunludur.")]
        [MaxLength(200)]
        public string Ad { get; set; } = string.Empty;

        public string? Aciklama { get; set; }

        public DemirbasKategori Kategori { get; set; } = DemirbasKategori.Diger;

        public DemirbasDurum Durum { get; set; } = DemirbasDurum.Aktif;

        public string? Konum { get; set; }
        public string? SeriNo { get; set; }
        public string? Marka { get; set; }
        public string? Model { get; set; }

        [DataType(DataType.Date)]
        public DateTime? AlimTarihi { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Fiyat { get; set; }

        public List<IFormFile>? Resimler { get; set; }
    }
}