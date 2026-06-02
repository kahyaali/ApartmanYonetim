using Microsoft.AspNetCore.Identity;

namespace ApartmanYonetim.Models.Entities
{
    public class AppUser : IdentityUser
    {
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public string? ProfilFoto { get; set; }
        public int? ApartmentId { get; set; }

        // Navigation
        public Apartment? Apartment { get; set; }
        public Resident? Resident { get; set; }
    }
}
