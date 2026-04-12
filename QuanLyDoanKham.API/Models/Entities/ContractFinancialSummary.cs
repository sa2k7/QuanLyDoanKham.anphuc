using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Tá»•ng há»£p tĂ i chĂ­nh snapshot cho há»£p Ä‘á»“ng</summary>
    public class ContractFinancialSummary
    {
        [Key]
        public int Id { get; set; }

        public int HealthContractId { get; set; }
        [ForeignKey("HealthContractId")]
        public HealthContract HealthContract { get; set; } = null!;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal StaffCost { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal SupplyCost { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal OtherCost { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Revenue { get; set; }

        [NotMapped]
        public decimal Profit => Revenue - (StaffCost + SupplyCost + OtherCost);

        public DateTime SnapshotAt { get; set; } = DateTime.Now;
    }
}
