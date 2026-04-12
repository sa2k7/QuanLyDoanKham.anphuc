using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>LÆ°u váº¿t phĂª duyá»‡t theo bÆ°á»›c</summary>
    public class ApprovalHistory
    {
        [Key]
        public int Id { get; set; }

        public int ApprovalStepId { get; set; }
        [ForeignKey("ApprovalStepId")]
        public ApprovalStep ApprovalStep { get; set; } = null!;

        public int HealthContractId { get; set; }
        [ForeignKey("HealthContractId")]
        public HealthContract HealthContract { get; set; } = null!;

        public int ApproverId { get; set; }
        [ForeignKey("ApproverId")]
        public AppUser Approver { get; set; } = null!;

        [MaxLength(20)]
        public string Decision { get; set; } = null!; // Approved / Rejected

        public string Note { get; set; } = null!;
        public DateTime ApprovedAt { get; set; } = DateTime.Now;
    }
}
