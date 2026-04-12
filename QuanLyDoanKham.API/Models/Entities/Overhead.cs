using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    public class Overhead
    {
        [Key]
        public int OverheadId { get; set; }

        public int HealthContractId { get; set; }
        [ForeignKey("HealthContractId")]
        public HealthContract HealthContract { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = ""; // ThuĂª xe, KhĂ¡ch sáº¡n, Tiá»n Äƒn

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        public string Description { get; set; } = "";

        public DateTime IncurredAt { get; set; } = DateTime.Now;

        public int? RecordedByUserId { get; set; }
        [ForeignKey("RecordedByUserId")]
        public AppUser RecordedByUser { get; set; } = null!;
    }
}
