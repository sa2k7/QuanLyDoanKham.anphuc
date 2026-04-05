using System.ComponentModel.DataAnnotations;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Cấu hình bước phê duyệt hợp đồng (1..N cấp)</summary>
    public class ContractApprovalStep
    {
        [Key]
        public int StepId { get; set; }

        public int StepOrder { get; set; } // 1, 2, 3...

        [Required]
        [MaxLength(100)]
        public string StepName { get; set; } // "Trưởng phòng duyệt", "Ban giám đốc duyệt"

        // Permission cần có để duyệt bước này
        [MaxLength(100)]
        public string RequiredPermission { get; set; } // "HopDong.Approve"

        public bool IsActive { get; set; } = true;
    }
}
