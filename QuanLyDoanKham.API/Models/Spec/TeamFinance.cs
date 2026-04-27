using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models.Spec
{
    /// <summary>Tài chính của đoàn khám (doanh thu, chi phí, lợi nhuận)</summary>
    public class TeamFinance
    {
        [Key]
        public int Id { get; set; }

        public int TeamId { get; set; }
        [ForeignKey("TeamId")]
        public ExaminationTeam Team { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Revenue { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal StaffCost { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal MaterialCost { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal OtherCost { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Profit { get; set; } = 0;
    }
}
