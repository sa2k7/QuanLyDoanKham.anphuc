using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    public class Supply
    {
        [Key]
        public int SupplyId { get; set; }

        [Required]
        [MaxLength(200)]
        public string SupplyName { get; set; }

        public string Unit { get; set; }
        public bool IsFixedAsset { get; set; }

        [MaxLength(100)]
        public string Category { get; set; }

        [MaxLength(50)]
        public string LotNumber { get; set; }

        public DateTime? ExpirationDate { get; set; }
        public DateTime? ManufactureDate { get; set; }
        public int MinStockLevel { get; set; } = 10;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }

        public int TotalStock { get; set; }
    }
}
