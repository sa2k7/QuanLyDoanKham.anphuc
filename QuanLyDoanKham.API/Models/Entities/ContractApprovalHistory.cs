using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Lá»‹ch sá»­ duyá»‡t tá»«ng há»£p Ä‘á»“ng theo bÆ°á»›c</summary>
    public class ContractApprovalHistory
    {
        [Key]
        public int Id { get; set; }

        public int HealthContractId { get; set; }
        [ForeignKey("HealthContractId")]
        public HealthContract HealthContract { get; set; } = null!;

        public int StepOrder { get; set; }

        [MaxLength(100)]
        public string StepName { get; set; } = null!;

        [MaxLength(20)]
        public string Action { get; set; } = null!; // Approved, Rejected, Recalled

        public string Note { get; set; } = null!;

        public int ApprovedByUserId { get; set; }
        [ForeignKey("ApprovedByUserId")]
        public AppUser ApprovedByUser { get; set; } = null!;

        public DateTime ActionDate { get; set; } = DateTime.Now;
    }
}
