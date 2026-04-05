using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    public class ContractPackage
    {
        [Key]
        public int PackageId { get; set; }

        public int HealthContractId { get; set; }
        [ForeignKey("HealthContractId")]
        public HealthContract HealthContract { get; set; }

        [Required]
        [MaxLength(200)]
        public string PackageName { get; set; } // e.g: Gói cơ bản, Gói Nữ, Gói Chuyên sâu

        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; } = 0;

        public int ExpectedQuantity { get; set; } = 0;
    }
}
