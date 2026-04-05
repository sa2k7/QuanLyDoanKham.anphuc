using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Permission granular - ví dụ: HopDong.Approve, DoanKham.Create</summary>
    public class Permission
    {
        [Key]
        public int PermissionId { get; set; }

        [Required(ErrorMessage = "MĂ£ quyá» n khĂ´ng Ä‘Æ°á»£c Ä‘á»ƒ trá»‘ng.")]
        [MaxLength(100)]
        public string PermissionKey { get; set; } // "HopDong.Approve"

        [MaxLength(200)]
        public string PermissionName { get; set; } // "Phê duyệt hợp đồng"

        [MaxLength(50)]
        public string Module { get; set; } // "HopDong", "DoanKham", "Kho"...

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
