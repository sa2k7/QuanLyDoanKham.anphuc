using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Chi phĂ­ tá»«ng Ä‘oĂ n khĂ¡m</summary>
    public class GroupCost
    {
        [Key]
        public int CostId { get; set; }

        public int GroupId { get; set; }
        [ForeignKey("GroupId")]
        public MedicalGroup MedicalGroup { get; set; } = null!;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal StaffCost { get; set; }      // Chi phĂ­ nhĂ¢n sá»±

        [Column(TypeName = "decimal(18, 2)")]
        public decimal SupplyCost { get; set; }     // Chi phĂ­ váº­t tÆ°

        [Column(TypeName = "decimal(18, 2)")]
        public decimal OtherCost { get; set; }      // Chi phĂ­ khĂ¡c

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalCost { get; set; }      // Tá»•ng chi phĂ­

        public string Note { get; set; } = null!;
        public DateTime CalculatedAt { get; set; } = DateTime.Now;

        public int? CalculatedByUserId { get; set; }
        [ForeignKey("CalculatedByUserId")]
        public AppUser CalculatedByUser { get; set; } = null!;
    }
}
