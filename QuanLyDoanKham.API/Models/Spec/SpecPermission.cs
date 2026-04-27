using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyDoanKham.API.Models.Spec
{
    /// <summary>Quyền hạn trong hệ thống RBAC</summary>
    public class SpecPermission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Feature { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = null!;

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
