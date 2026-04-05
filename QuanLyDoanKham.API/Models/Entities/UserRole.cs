using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Mapping nhiều-nhiều: User ↔ Role</summary>
    public class UserRole
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public AppUser User { get; set; }

        // Primary role (backward compat) - optional khi đã dùng multi-role
        public int? RoleId { get; set; }
        [ForeignKey("RoleId")]
        public AppRole Role { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string AssignedBy { get; set; }
    }
}
