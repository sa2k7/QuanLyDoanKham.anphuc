using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Tổng hợp doanh thu/lợi nhuận theo hợp đồng</summary>
    public class ContractRevenueSummary
    {
        [Key]
        public int SummaryId { get; set; }

        public int HealthContractId { get; set; }
        [ForeignKey("HealthContractId")]
        public HealthContract HealthContract { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalContractValue { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalGroupCosts { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Revenue { get; set; } // = TotalContractValue - TotalGroupCosts

        public int TotalGroups { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string GeneratedBy { get; set; }
    }
}
