using System.ComponentModel.DataAnnotations;
using ApartmanYonetim.Models.Entities;

namespace ApartmanYonetim.Models.ViewModels
{
    public class TicketCreateViewModel
    {
        [Required(ErrorMessage = "Başlık zorunludur.")]
        [MaxLength(200)]
        public string Baslik { get; set; } = string.Empty;

        [Required(ErrorMessage = "Açıklama zorunludur.")]
        public string Aciklama { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kategori seçiniz.")]
        public string Kategori { get; set; } = string.Empty;

        public TicketOncelik Oncelik { get; set; } = TicketOncelik.Normal;
    }

    public class TicketAdminViewModel
    {
        public int Id { get; set; }
        public TicketDurum Durum { get; set; }
        public TicketOncelik Oncelik { get; set; }
        public string? AdminNotu { get; set; }
    }
}