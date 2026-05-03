using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    public class CostSnapshot
    {
        [Key]
        public int SnapshotId { get; set; }

        public int HealthContractId { get; set; }
        [ForeignKey("HealthContractId")]
        public HealthContract HealthContract { get; set; } = null!;

        [MaxLength(20)]
        public string SnapshotType { get; set; } = null!; // EST (Dự kiến), ACT (Thực tế), SETTLE (Quyết toán)

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Revenue { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal LaborCost { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal SupplyCost { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal OverheadCost { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalCost { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal GrossProfit { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int? CreatedByUserId { get; set; }
        [ForeignKey("CreatedByUserId")]
        public AppUser CreatedByUser { get; set; } = null!;
        
        public string Note { get; set; } = null!;
    }
}
