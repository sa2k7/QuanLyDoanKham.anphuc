using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyDoanKham.API.Models.Spec
{
    /// <summary>Vai trò trong hệ thống RBAC</summary>
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = null!;

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
