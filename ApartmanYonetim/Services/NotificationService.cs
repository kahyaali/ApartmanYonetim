using ApartmanYonetim.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ApartmanYonetim.Services
{
    public interface INotificationService
    {
        Task SendToAllAsync(string mesaj, string tip = "info");
        Task SendToUserAsync(string userId, string mesaj, string tip = "info");
        Task SendToAdminAsync(string mesaj, string tip = "info");

        Task RefreshUserDataAsync(string userId, string dataType);
        Task RefreshAllAsync(string dataType);
    }

    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hub;

        public NotificationService(IHubContext<NotificationHub> hub)
        {
            _hub = hub;
        }

        public async Task SendToAllAsync(string mesaj, string tip = "info")
        {
            await _hub.Clients.All.SendAsync("ReceiveNotification", mesaj, tip);
        }

        public async Task SendToUserAsync(string userId, string mesaj, string tip = "info")
        {
            await _hub.Clients.Group($"user_{userId}")
                .SendAsync("ReceiveNotification", mesaj, tip);
        }

        public async Task SendToAdminAsync(string mesaj, string tip = "info")
        {
            await _hub.Clients.Group("admins")
                .SendAsync("ReceiveNotification", mesaj, tip);
        }

        // Kullanıcının belirli bir verisini yenile
        public async Task RefreshUserDataAsync(string userId, string dataType)
        {
            await _hub.Clients.Group($"user_{userId}")
                .SendAsync("RefreshData", dataType);
        }

        // Tüm bağlı kullanıcıların verisini yenile
        public async Task RefreshAllAsync(string dataType)
        {
            await _hub.Clients.All.SendAsync("RefreshData", dataType);
        }
    }
}