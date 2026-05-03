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
        public string Unit { get; set; } = ""; // Cái, Há»™p, Lọ, Chai

        [MaxLength(100)]
        public string Category { get; set; } = ""; // Thuá»‘c, Vật tư y tế, Thiết bị...

        public int CurrentStock { get; set; } = 0;

        public int MinStockLevel { get; set; } = 10; // Ngưỡng báo đá»™ng

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TypicalUnitPrice { get; set; } // Giá nhập tham khảo

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedAt { get; set; }

        // Nhật ký biến đá»™ng kho liên quan
        public ICollection<StockMovement> Movements { get; set; } = new List<StockMovement>();
    }
}
