using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models.Spec
{
    /// <summary>Vật tư y tế trong kho</summary>
    public class InventoryItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        [MaxLength(50)]
        public string? Unit { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Quantity { get; set; } = 0;

        [Column(TypeName = "decimal(18,4)")]
        public decimal MinThreshold { get; set; } = 0;

        public ICollection<InventoryReceipt> Receipts { get; set; } = new List<InventoryReceipt>();
        public ICollection<InventoryIssue> Issues { get; set; } = new List<InventoryIssue>();
    }
}
