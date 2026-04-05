using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Định nghĩa bước phê duyệt đa cấp theo hợp đồng</summary>
    public class ApprovalStep
    {
        [Key]
        public int Id { get; set; }

        public int HealthContractId { get; set; }
        [ForeignKey("HealthContractId")]
        public HealthContract HealthContract { get; set; }

        public int Order { get; set; }

        public int RoleId { get; set; }
        [ForeignKey("RoleId")]
        public AppRole Role { get; set; }

        [MaxLength(100)]
        public string RequiredPermission { get; set; }
        public bool IsFinal { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }
}
