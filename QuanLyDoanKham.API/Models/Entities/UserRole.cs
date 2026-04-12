using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Mapping nhiá»u-nhiá»u: User â†” Role</summary>
    public class UserRole
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public AppUser User { get; set; } = null!;

        // Primary role (backward compat) - optional khi Ä‘Ă£ dĂ¹ng multi-role
        public int? RoleId { get; set; }
        [ForeignKey("RoleId")]
        public AppRole Role { get; set; } = null!;

        public DateTime AssignedAt { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string AssignedBy { get; set; } = "";
    }
}
