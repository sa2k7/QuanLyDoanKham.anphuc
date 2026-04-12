using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    public class SupplyItem
    {
        [Key]
        public int SupplyId { get; set; }

        [Required]
        [MaxLength(200)]
        public string ItemName { get; set; } = "";

        [MaxLength(50)]
        public string Unit { get; set; } = ""; // CĂ¡i, Há»™p, Lá», Chai

        [MaxLength(100)]
        public string Category { get; set; } = ""; // Thuá»‘c, Váº­t tÆ° y táº¿, Thiáº¿t bá»‹...

        public int CurrentStock { get; set; } = 0;

        public int MinStockLevel { get; set; } = 10; // NgÆ°á»¡ng bĂ¡o Ä‘á»™ng

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TypicalUnitPrice { get; set; } // GiĂ¡ nháº­p tham kháº£o

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedAt { get; set; }

        // Nháº­t kĂ½ biáº¿n Ä‘á»™ng kho liĂªn quan
        public ICollection<StockMovement> Movements { get; set; } = new List<StockMovement>();
    }
}
