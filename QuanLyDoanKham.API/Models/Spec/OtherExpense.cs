using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models.Spec
{
    /// <summary>Chi phí phát sinh khác của đoàn khám (di chuyển, ăn uống, ...)</summary>
    public class OtherExpense
    {
        [Key]
        public int Id { get; set; }

        public int TeamId { get; set; }
        [ForeignKey("TeamId")]
        public ExaminationTeam Team { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }
    }
}
