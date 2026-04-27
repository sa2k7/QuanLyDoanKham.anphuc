using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models.Spec
{
    /// <summary>Phiếu xuất kho vật tư cho đoàn khám</summary>
    public class InventoryIssue
    {
        [Key]
        public int Id { get; set; }

        public int ItemId { get; set; }
        [ForeignKey("ItemId")]
        public InventoryItem Item { get; set; } = null!;

        public int TeamId { get; set; }
        [ForeignKey("TeamId")]
        public ExaminationTeam Team { get; set; } = null!;

        [Column(TypeName = "decimal(18,4)")]
        public decimal Quantity { get; set; }

        public DateOnly IssueDate { get; set; }
    }
}
