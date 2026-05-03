using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace QuanLyDoanKham.API.Models
{
    /// <summary>Vị trí trong đoĂ n khám: Tiếp nhận, Khám ná»™i, Lấy máu...</summary>
    public class MedicalGroupPosition
    {
        [Key]
        public int Id { get; set; }

        public int? PositionId { get; set; }
        [ForeignKey("PositionId")]
        public virtual Position? Position { get; set; }

        public int GroupId { get; set; }
        [ForeignKey("GroupId")]
        public virtual MedicalGroup MedicalGroup { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string PositionName { get; set; } = null!; // "Tiếp nhận", "Khám ná»™i", "Siêu âm"...

        public int RequiredCount { get; set; } = 1; // Sá»‘ ngưá» i cần
        public int AssignedCount { get; set; } = 0; // Sá»‘ ngưá» i đã gán (tự tá»•ng hợp)

        [MaxLength(200)]
        public string Description { get; set; } = null!;

        public int SortOrder { get; set; } = 0;
    }
}
