using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Permission granular - vĂ­ dá»¥: HopDong.Approve, DoanKham.Create</summary>
    public class Permission
    {
        [Key]
        public int PermissionId { get; set; }

        [Required(ErrorMessage = "MÄ‚Â£ quyĂ¡Â» n khÄ‚Â´ng Ă„â€˜Ă†Â°Ă¡Â»Â£c Ă„â€˜Ă¡Â»Æ’ trĂ¡Â»â€˜ng.")]
        [MaxLength(100)]
        public string PermissionKey { get; set; } = ""; // "HopDong.Approve"
        [MaxLength(200)]
        public string PermissionName { get; set; } = ""; // "PhĂª duyá»‡t há»£p Ä‘á»“ng"
        [MaxLength(50)]
        public string Module { get; set; } = ""; // "HopDong", "DoanKham", "Kho"...

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
