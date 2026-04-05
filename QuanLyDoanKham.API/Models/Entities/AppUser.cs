using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Người dùng hệ thống</summary>
    public class AppUser
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required(ErrorMessage = " Máº­t kháº©u khĂ´ng Ä‘Æ°á»£c Ä‘á»ƒ trá»‘ng.")]
        public string PasswordHash { get; set; }

        [MaxLength(100)]
        public string FullName { get; set; }

        // Primary role (backward compat) - vẫn giữ để JWT đơn giản hơn
        public int RoleId { get; set; }
        [ForeignKey("RoleId")]
        public AppRole Role { get; set; }

        // Phòng ban
        public int? DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }

        public int? CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public Company Company { get; set; }

        public int? StaffId { get; set; }
        [ForeignKey("StaffId")]
        public Staff Staff { get; set; }

        public string RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }

        [MaxLength(200)]
        public string Email { get; set; }

        public string AvatarPath { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Multi-role support
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
