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
        public AppUser User { get; set; }

        [MaxLength(50)]
        public string Action { get; set; }

        [MaxLength(100)]
        public string EntityType { get; set; }

        public int EntityId { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string IPAddress { get; set; }
    }
}
