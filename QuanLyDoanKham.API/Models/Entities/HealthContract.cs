using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    public class HealthContract
    {
        [Key]
        public int HealthContractId { get; set; }

        public int CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public Company Company { get; set; }

        [MaxLength(50)]
        public string ContractCode { get; set; } // Số hợp đồng

        public DateTime SigningDate { get; set; }   // Ngày ký HĐ
        public DateTime StartDate { get; set; }     // Ngày bắt đầu khám
        public DateTime EndDate { get; set; }       // Ngày kết thúc khám

        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }

        public int ExpectedQuantity { get; set; }

        [MaxLength(50)]
        public string UnitName { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        // Trạng thái: Draft, PendingApproval, Approved, Rejected, Active, Finished, Locked
        [MaxLength(50)]
        public string Status { get; set; } = "Draft";

        public int CurrentApprovalStep { get; set; } = 0; // Bước duyệt hiện tại

        public string FilePath { get; set; }  // File hợp đồng đính kèm

        // Audit
        public int? CreatedByUserId { get; set; }
        [ForeignKey("CreatedByUserId")]
        public AppUser CreatedByUser { get; set; }

        public int? UpdatedByUserId { get; set; }
        [ForeignKey("UpdatedByUserId")]
        public AppUser UpdatedByUser { get; set; }

        public int? SignerId { get; set; }
        [ForeignKey("SignerId")]
        public AppUser Signer { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        [MaxLength(100)]
        public string UpdatedBy { get; set; }

        public string Notes { get; set; }

        public ICollection<ContractStatusHistory> StatusHistories { get; set; } = new List<ContractStatusHistory>();
        public ICollection<ContractApprovalHistory> ApprovalHistories { get; set; } = new List<ContractApprovalHistory>();
        public ICollection<ContractAttachment> Attachments { get; set; } = new List<ContractAttachment>();
        public ICollection<MedicalGroup> MedicalGroups { get; set; } = new List<MedicalGroup>();
    }
}
