using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    public class AuditLog
    {
        [Key]
        public int LogId { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public AppUser User { get; set; } = null!;

        [MaxLength(50)]
        public string Action { get; set; } = null!;

        [MaxLength(100)]
        public string EntityType { get; set; } = null!;

        public int EntityId { get; set; }
        public string OldValue { get; set; } = null!;
        public string NewValue { get; set; } = null!;

        public DateTime Timestamp { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string IPAddress { get; set; } = null!;
    }
}
