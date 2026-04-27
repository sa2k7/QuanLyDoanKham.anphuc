using Microsoft.AspNetCore.SignalR;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task JoinUserGroup(int userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        public async Task LeaveUserGroup(int userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        public async Task SendNotification(int userId, Notification notification)
        {
            await Clients.Group($"user_{userId}")
                .SendAsync("ReceiveNotification", new
                {
                    notification.Id,
                    notification.Title,
                    notification.Message,
                    notification.Type,
                    notification.CreatedAt,
                    notification.IsRead,
                    notification.ActionUrl
                });
        }
    }
}
