using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Services.Auth
{
    public class AuditLogDto
    {
        public int AuditLogId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public interface IAuditLogService
    {
        Task LogAsync(string action, string module, string description, string? oldValue = null, string? newValue = null, string? metadata = null);
        Task<List<AuditLogDto>> GetLogsAsync(DateTime? start, DateTime? end, string? module = null, string? username = null, string? action = null);
    }

    public class AuditLogService : IAuditLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string action, string module, string description, string? oldValue = null, string? newValue = null, string? metadata = null)
        {
            var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "system";
            var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            // Tìm UserId từ username
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return; // Không log nếu không tìm thấy user

            var log = new AuditLog
            {
                UserId = user.UserId,
                Action = action,
                EntityType = module,
                EntityId = 0,
                OldValue = oldValue ?? string.Empty,
                NewValue = newValue ?? description,
                IPAddress = ipAddress ?? string.Empty,
                Timestamp = DateTime.Now
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<List<AuditLogDto>> GetLogsAsync(DateTime? start, DateTime? end, string? module = null, string? username = null, string? action = null)
        {
            var query = _context.AuditLogs
                .Include(l => l.User)
                .AsQueryable();

            if (start.HasValue) query = query.Where(l => l.Timestamp >= start.Value);
            if (end.HasValue) query = query.Where(l => l.Timestamp <= end.Value.AddDays(1).AddTicks(-1));
            if (!string.IsNullOrEmpty(module)) query = query.Where(l => l.EntityType.Contains(module));
            if (!string.IsNullOrEmpty(action)) query = query.Where(l => l.Action == action);
            if (!string.IsNullOrEmpty(username)) query = query.Where(l => l.User.Username.Contains(username));

            var logs = await query.OrderByDescending(l => l.Timestamp).Take(1000).ToListAsync();

            return logs.Select(l => new AuditLogDto
            {
                AuditLogId = l.LogId,
                Username = l.User?.Username ?? "system",
                Action = l.Action,
                Module = l.EntityType,
                Description = l.NewValue,
                OldValue = l.OldValue,
                NewValue = l.NewValue,
                IpAddress = l.IPAddress,
                CreatedAt = l.Timestamp
            }).ToList();
        }
    }
}
