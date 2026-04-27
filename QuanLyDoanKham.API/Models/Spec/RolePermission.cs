using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models.Spec
{
    /// <summary>Bảng trung gian Role - Permission (many-to-many)</summary>
    public class RolePermission
    {
        public int RoleId { get; set; }
        [ForeignKey("RoleId")]
        public Role Role { get; set; } = null!;

        public int PermissionId { get; set; }
        [ForeignKey("PermissionId")]
        public SpecPermission Permission { get; set; } = null!;
    }
}
