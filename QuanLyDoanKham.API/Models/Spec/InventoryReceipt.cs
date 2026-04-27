using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models.Spec
{
    /// <summary>Phiếu nhập kho vật tư</summary>
    public class InventoryReceipt
    {
        [Key]
        public int Id { get; set; }

        public int ItemId { get; set; }
        [ForeignKey("ItemId")]
        public InventoryItem Item { get; set; } = null!;

        [Column(TypeName = "decimal(18,4)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        public DateOnly ReceiptDate { get; set; }

        [MaxLength(200)]
        public string? Supplier { get; set; }
    }
}
