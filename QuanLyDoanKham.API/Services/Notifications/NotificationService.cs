using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Hubs;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ApplicationDbContext context, IHubContext<NotificationHub> hubContext, ILogger<NotificationService> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<Notification> SendAsync(int userId, string type, string title, string message, string? actionUrl = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = type,
                Title = title,
                Message = message,
                ActionUrl = actionUrl,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group($"user_{userId}")
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

            _logger.LogInformation("Sent notification to user {UserId}: {Title}", userId, title);
            return notification;
        }

        public async Task BroadcastAsync(string type, string title, string message, string? actionUrl = null)
        {
            var notification = new Notification
            {
                UserId = null,
                Type = type,
                Title = title,
                Message = message,
                ActionUrl = actionUrl,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All
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

            _logger.LogInformation("Broadcast notification: {Title}", title);
        }

        public async Task SendToRoleAsync(string roleCode, string type, string title, string message, string? actionUrl = null)
        {
            // Get all users with this role
            var userIds = await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.Role != null && ur.Role.RoleName == roleCode)
                .Select(ur => ur.UserId)
                .Distinct()
                .ToListAsync();

            foreach (var userId in userIds)
            {
                await SendAsync(userId, type, title, message, actionUrl);
            }

            _logger.LogInformation("Sent role notification to {Count} users with role {Role}", userIds.Count, roleCode);
        }

        public async Task MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null) return;

            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetUnreadAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }
    }
}
