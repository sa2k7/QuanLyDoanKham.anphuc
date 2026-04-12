using System.ComponentModel.DataAnnotations;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Cáº¥u hĂ¬nh bÆ°á»›c phĂª duyá»‡t há»£p Ä‘á»“ng (1..N cáº¥p)</summary>
    public class ContractApprovalStep
    {
        [Key]
        public int StepId { get; set; }

        public int StepOrder { get; set; } // 1, 2, 3...

        [Required]
        [MaxLength(100)]
        public string StepName { get; set; } = null!; // "TrÆ°á»Ÿng phĂ²ng duyá»‡t", "Ban giĂ¡m Ä‘á»‘c duyá»‡t"

        // Permission cáº§n cĂ³ Ä‘á»ƒ duyá»‡t bÆ°á»›c nĂ y
        [MaxLength(100)]
        public string RequiredPermission { get; set; } = null!; // "HopDong.Approve"

        public bool IsActive { get; set; } = true;
    }
}
