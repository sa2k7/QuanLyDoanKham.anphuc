using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Định nghĩa bưá»›c phê duyệt đa cấp theo hợp đá»“ng</summary>
    public class ApprovalStep
    {
        [Key]
        public int Id { get; set; }

        public int HealthContractId { get; set; }
        [ForeignKey("HealthContractId")]
        public HealthContract HealthContract { get; set; } = null!;

        public int Order { get; set; }

        public int RoleId { get; set; }
        [ForeignKey("RoleId")]
        public AppRole Role { get; set; } = null!;

        [MaxLength(100)]
        public string RequiredPermission { get; set; } = null!;
        public bool IsFinal { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }
}
