using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Lịch sử duyệt từng hợp đồng theo bước</summary>
    public class ContractApprovalHistory
    {
        [Key]
        public int Id { get; set; }

        public int HealthContractId { get; set; }
        [ForeignKey("HealthContractId")]
        public HealthContract HealthContract { get; set; }

        public int StepOrder { get; set; }

        [MaxLength(100)]
        public string StepName { get; set; }

        [MaxLength(20)]
        public string Action { get; set; } // Approved, Rejected, Recalled

        public string Note { get; set; }

        public int ApprovedByUserId { get; set; }
        [ForeignKey("ApprovedByUserId")]
        public AppUser ApprovedByUser { get; set; }

        public DateTime ActionDate { get; set; } = DateTime.Now;
    }
}
