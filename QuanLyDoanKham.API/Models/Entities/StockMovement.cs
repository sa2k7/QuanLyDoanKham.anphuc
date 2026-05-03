using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    public class StockMovement
    {
        [Key]
        public int MovementId { get; set; }

        public int? MedicalGroupId { get; set; } // Thuá»™c về đoĂ n khám nĂ o
        [ForeignKey("MedicalGroupId")]
        public MedicalGroup MedicalGroup { get; set; } = null!;

        public int? SupplyId { get; set; } // Liên kết vá»›i danh mục vật tư
        [ForeignKey("SupplyId")]
        public SupplyItem SupplyItem { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string ItemName { get; set; } = ""; // Tên vật tư y tế

        [MaxLength(50)]
        public string Unit { get; set; } = ""; // Cái, Há»™p, Lọ

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalValue { get; set; } // Quantity * UnitPrice

        [MaxLength(20)]
        public string MovementType { get; set; } = ""; // IN, OUT

        public DateTime MovementDate { get; set; } = DateTime.Now;

        public int? RecordedByUserId { get; set; }
        [ForeignKey("RecordedByUserId")]
        public AppUser RecordedByUser { get; set; } = null!;
        
        public string Note { get; set; } = "";
    }
}
