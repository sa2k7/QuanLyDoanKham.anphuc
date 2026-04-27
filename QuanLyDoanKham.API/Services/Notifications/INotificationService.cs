using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Services.Notifications
{
    public interface INotificationService
    {
        Task<Notification> SendAsync(int userId, string type, string title, string message, string? actionUrl = null);
        Task BroadcastAsync(string type, string title, string message, string? actionUrl = null);
        Task SendToRoleAsync(string roleCode, string type, string title, string message, string? actionUrl = null);
        Task MarkAsReadAsync(int notificationId, int userId);
        Task<List<Notification>> GetUnreadAsync(int userId);
    }
}
