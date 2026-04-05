using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Lưu vết phê duyệt theo bước</summary>
    public class ApprovalHistory
    {
        [Key]
        public int Id { get; set; }

        public int ApprovalStepId { get; set; }
        [ForeignKey("ApprovalStepId")]
        public ApprovalStep ApprovalStep { get; set; }

        public int HealthContractId { get; set; }
        [ForeignKey("HealthContractId")]
        public HealthContract HealthContract { get; set; }

        public int ApproverId { get; set; }
        [ForeignKey("ApproverId")]
        public AppUser Approver { get; set; }

        [MaxLength(20)]
        public string Decision { get; set; } // Approved / Rejected

        public string Note { get; set; }
        public DateTime ApprovedAt { get; set; } = DateTime.Now;
    }
}
