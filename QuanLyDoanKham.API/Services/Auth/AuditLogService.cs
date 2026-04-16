using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Models;
using System.Text.Json;

namespace QuanLyDoanKham.API.Services.Auth
{
    public interface IAuditLogService
    {
        Task LogAsync(string action, string module, string description, string? oldValue = null, string? newValue = null, string? metadata = null);
        Task<List<AuditLog>> GetLogsAsync(DateTime? start, DateTime? end, string? module = null, string? username = null);
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

            var log = new AuditLog
            {
                Username = username,
                Action = action,
                Module = module,
                Description = description,
                OldValue = oldValue,
                NewValue = newValue,
                Metadata = metadata,
                IpAddress = ipAddress,
                CreatedAt = DateTime.Now
            };

            _context.Set<AuditLog>().Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<List<AuditLog>> GetLogsAsync(DateTime? start, DateTime? end, string? module = null, string? username = null)
        {
            var query = _context.Set<AuditLog>().AsQueryable();

            if (start.HasValue) query = query.Where(l => l.CreatedAt >= start.Value);
            if (end.HasValue) query = query.Where(l => l.CreatedAt <= end.Value);
            if (!string.IsNullOrEmpty(module)) query = query.Where(l => l.Module == module);
            if (!string.IsNullOrEmpty(username)) query = query.Where(l => l.Username == username);

            return await query.OrderByDescending(l => l.CreatedAt).Take(1000).ToListAsync();
        }
    }

    // Model cho AuditLog (Nếu chưa có trong Models)
    public class AuditLog
    {
        public int AuditLogId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // CREATE, UPDATE, DELETE, LOGIN
        public string Module { get; set; } = string.Empty; // Hợp đồng, Đoàn khám, Nhân sự...
        public string Description { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? Metadata { get; set; } // Browser, OS...
        public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
