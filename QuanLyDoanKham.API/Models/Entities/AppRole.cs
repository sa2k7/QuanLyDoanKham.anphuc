using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Role cơ bản của hệ thống</summary>
    public class AppRole
    {
        [Key]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Tên vai trò không được để trống.")]
        [MaxLength(50)]
        public string RoleName { get; set; } = null!;

        [MaxLength(200)]
        public string Description { get; set; } = null!;

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
