using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Permission granular - ví dụ: HopDong.Approve, DoanKham.Create</summary>
    public class Permission
    {
        [Key]
        public int PermissionId { get; set; }

        [Required(ErrorMessage = "MÄ‚Â£ quyáÂ» n khÄ‚Â´ng Ă„â€˜Ă†Â°áÂ»Â£c Ă„â€˜áÂ»Æ’ tráÂ»â€˜ng.")]
        [MaxLength(100)]
        public string PermissionKey { get; set; } = ""; // "HopDong.Approve"
        [MaxLength(200)]
        public string PermissionName { get; set; } = ""; // "Phê duyệt hợp đá»“ng"
        [MaxLength(50)]
        public string Module { get; set; } = ""; // "HopDong", "DoanKham", "Kho"...

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
