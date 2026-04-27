using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuanLyDoanKham.API.Models.Enums;

namespace QuanLyDoanKham.API.Models
{
    public class HealthContract
    {
        [Key]
        public int HealthContractId { get; set; }

        public int CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public Company? Company { get; set; }

        [MaxLength(200)]
        public string ContractName { get; set; } = null!; // TĂªn há»£p Ä‘á»“ng

        [MaxLength(50)]
        public string? ContractCode { get; set; } // Sá»‘ há»£p Ä‘á»“ng

        public DateTime SigningDate { get; set; }   // NgĂ y kĂ½ HÄ
        public DateTime StartDate { get; set; }     // NgĂ y báº¯t Ä‘áº§u khĂ¡m
        public DateTime EndDate { get; set; }       // NgĂ y káº¿t thĂºc khĂ¡m

        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; } = 0; // GiĂ¡ cÅ©

        public int ExpectedQuantity { get; set; } = 0; // Sá»‘ khĂ¡ch cÅ©

        [Column(TypeName = "decimal(18, 2)")]
        public decimal AdvancePayment { get; set; } = 0; // Tiá»n táº¡m á»©ng

        [Column(TypeName = "decimal(18, 2)")]
        public decimal FinalSettlementValue { get; set; } = 0; // GiĂ¡ trá»‹ quyáº¿t toĂ¡n thá»±c táº¿

        [Column(TypeName = "decimal(5, 2)")]
        public decimal DiscountPercent { get; set; } = 0; // % Chiáº¿t kháº¥u / Hoa há»“ng

        [MaxLength(50)]
        public string? UnitName { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ExtraServiceRevenue { get; set; } = 0; // Doanh thu dịch vụ ngoài gói

        public string? ExtraServiceDetails { get; set; } // JSON of itemized services

        [Column(TypeName = "decimal(5, 2)")]
        public decimal VATRate { get; set; } = 0; // Thuế suất GTGT (%)

        // Trạng thái hợp đồng
        public ContractStatus Status { get; set; } = ContractStatus.Draft;

        public int CurrentApprovalStep { get; set; } = 0; // BÆ°á»›c duyá»‡t hiá»‡n táº¡i

        public string? FilePath { get; set; }  // File há»£p Ä‘á»“ng Ä‘Ă­nh kĂ¨m

        // Audit
        public int? CreatedByUserId { get; set; }
        [ForeignKey("CreatedByUserId")]
        public AppUser? CreatedByUser { get; set; }

        public int? UpdatedByUserId { get; set; }
        [ForeignKey("UpdatedByUserId")]
        public AppUser? UpdatedByUser { get; set; }

        public int? SignerId { get; set; }
        [ForeignKey("SignerId")]
        public AppUser? Signer { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        [MaxLength(100)]
        public string? UpdatedBy { get; set; }

        public string? Notes { get; set; }

        public ICollection<ContractStatusHistory> StatusHistories { get; set; } = new List<ContractStatusHistory>();
        public ICollection<ContractApprovalHistory> ApprovalHistories { get; set; } = new List<ContractApprovalHistory>();
        public ICollection<ContractAttachment> Attachments { get; set; } = new List<ContractAttachment>();
        public ICollection<MedicalGroup> MedicalGroups { get; set; } = new List<MedicalGroup>();
        
    }
}
