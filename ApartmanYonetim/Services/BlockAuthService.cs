// Services/BlockAuthService.cs — yeni dosya oluştur
using ApartmanYonetim.Data;
using ApartmanYonetim.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ApartmanYonetim.Services
{
    public interface IBlockAuthService
    {
        // Bu adminin görebileceği blok ID'lerini döndür
        // Boş liste = tüm blokları görebilir (SuperAdmin)
        Task<List<int>> GetYetkiliBlokIdsAsync(string userId);

        // Bu admin bu bloğu görebilir mi?
        Task<bool> CanAccessBlockAsync(string userId, int blockId);

        // Query'e blok filtresi uygula
        Task<IQueryable<Apartment>> FilterApartmentsAsync(
            IQueryable<Apartment> query, string userId);
    }

    public class BlockAuthService : IBlockAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public BlockAuthService(ApplicationDbContext context,
            UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<int>> GetYetkiliBlokIdsAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return new List<int>();

            // SuperAdmin her şeyi görür
            if (await _userManager.IsInRoleAsync(user, "SuperAdmin"))
                return new List<int>(); // boş = kısıtsız

            // Atanmış blokları getir
            var blokIds = await _context.YoneticiBlocklar
                .Where(y => y.YoneticiId == userId)
                .Select(y => y.BlockId)
                .ToListAsync();

            // Blok atanmamışsa (eski admin) tüm blokları görebilir
            if (!blokIds.Any())
                return new List<int>(); // boş = kısıtsız

            return blokIds;
        }

        public async Task<bool> CanAccessBlockAsync(string userId, int blockId)
        {
            var yetkiliBloklar = await GetYetkiliBlokIdsAsync(userId);
            if (!yetkiliBloklar.Any()) return true; // kısıtsız
            return yetkiliBloklar.Contains(blockId);
        }

        public async Task<IQueryable<Apartment>> FilterApartmentsAsync(
            IQueryable<Apartment> query, string userId)
        {
            var yetkiliBloklar = await GetYetkiliBlokIdsAsync(userId);
            if (!yetkiliBloklar.Any()) return query; // kısıtsız
            return query.Where(a => a.BlockId != null &&
                yetkiliBloklar.Contains(a.BlockId.Value));
        }
    }
}