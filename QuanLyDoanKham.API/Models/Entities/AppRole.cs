using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Role cÆ¡ báº£n cá»§a há»‡ thá»‘ng</summary>
    public class AppRole
    {
        [Key]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "TÄ‚Âªn vai trÄ‚Â² khÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c Ă„â€˜Ă¡Â»Æ’ trĂ¡Â»â€˜ng.")]
        [MaxLength(50)]
        public string RoleName { get; set; } = null!;

        [MaxLength(200)]
        public string Description { get; set; } = null!;

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
