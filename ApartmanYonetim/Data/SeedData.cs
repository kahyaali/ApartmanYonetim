using ApartmanYonetim.Models.Entities;
using Microsoft.AspNetCore.Identity;


namespace ApartmanYonetim.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider
                .GetRequiredService<UserManager<AppUser>>();

            // Rolleri oluştur — SuperAdmin eklendi
            string[] roles = { "SuperAdmin", "Admin", "Sakin" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // SuperAdmin kullanıcısı oluştur
            var superAdminEmail = "superadmin@apartman.com";
            var superAdmin = await userManager.FindByEmailAsync(superAdminEmail);
            if (superAdmin == null)
            {
                superAdmin = new AppUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    Ad = "Super",
                    Soyad = "Admin",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(superAdmin, "SuperAdmin123!");
                await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
                await userManager.AddToRoleAsync(superAdmin, "Admin");
            }

            // Eski Admin kullanıcısını SuperAdmin yap
            var adminEmail = "admin@apartman.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Ad = "Sistem",
                    Soyad = "Admin",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(adminUser, "Admin123!");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
