using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        public int? UserId { get; set; }
        [ForeignKey("UserId")]
        public AppUser? User { get; set; }

        [Required]
        public string Title { get; set; } = null!;

        [Required]
        public string Message { get; set; } = null!;

        [MaxLength(50)]
        public string Type { get; set; } = "General";

        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? Link { get; set; }
        public string? ActionUrl { get; set; }
    }
}
