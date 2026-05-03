using System.ComponentModel.DataAnnotations;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Cấu hình bưá»›c phê duyệt hợp đá»“ng (1..N cấp)</summary>
    public class ContractApprovalStep
    {
        [Key]
        public int StepId { get; set; }

        public int StepOrder { get; set; } // 1, 2, 3...

        [Required]
        [MaxLength(100)]
        public string StepName { get; set; } = null!; // "Trưởng phòng duyệt", "Ban giám đá»‘c duyệt"

        // Permission cần có để duyệt bưá»›c nĂ y
        [MaxLength(100)]
        public string RequiredPermission { get; set; } = null!; // "HopDong.Approve"

        public bool IsActive { get; set; } = true;
    }
}
