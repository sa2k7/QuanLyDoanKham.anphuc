using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    public class StockMovement
    {
        [Key]
        public int MovementId { get; set; }

        public int? MedicalGroupId { get; set; } // Thuá»™c vá» Ä‘oĂ n khĂ¡m nĂ o
        [ForeignKey("MedicalGroupId")]
        public MedicalGroup MedicalGroup { get; set; } = null!;

        public int? SupplyId { get; set; } // LiĂªn káº¿t vá»›i danh má»¥c váº­t tÆ°
        [ForeignKey("SupplyId")]
        public SupplyItem SupplyItem { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string ItemName { get; set; } = ""; // TĂªn váº­t tÆ° y táº¿

        [MaxLength(50)]
        public string Unit { get; set; } = ""; // CĂ¡i, Há»™p, Lá»

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
