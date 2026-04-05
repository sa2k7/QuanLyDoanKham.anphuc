using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyDoanKham.API.Models
{
    public class PasswordResetRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        public DateTime RequestedDate { get; set; } = DateTime.Now;
        public bool IsProcessed { get; set; } = false;

        [MaxLength(100)]
        public string NewPassword { get; set; }
    }
}
