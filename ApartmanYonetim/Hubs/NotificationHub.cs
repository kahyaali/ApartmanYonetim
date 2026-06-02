using Microsoft.AspNetCore.SignalR;

namespace ApartmanYonetim.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        // sayfayı yenile komutu gönder
        public async Task RefreshPage(string groupName)
        {
            await Clients.Group(groupName).SendAsync("RefreshData");
        }
    }
}