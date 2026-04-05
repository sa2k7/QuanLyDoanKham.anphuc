using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Chi phí từng đoàn khám</summary>
    public class GroupCost
    {
        [Key]
        public int CostId { get; set; }

        public int GroupId { get; set; }
        [ForeignKey("GroupId")]
        public MedicalGroup MedicalGroup { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal StaffCost { get; set; }      // Chi phí nhân sự

        [Column(TypeName = "decimal(18, 2)")]
        public decimal SupplyCost { get; set; }     // Chi phí vật tư

        [Column(TypeName = "decimal(18, 2)")]
        public decimal OtherCost { get; set; }      // Chi phí khác

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalCost { get; set; }      // Tổng chi phí

        public string Note { get; set; }
        public DateTime CalculatedAt { get; set; } = DateTime.Now;

        public int? CalculatedByUserId { get; set; }
        [ForeignKey("CalculatedByUserId")]
        public AppUser CalculatedByUser { get; set; }
    }
}
