using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Vị trí trong đoàn khám: Tiếp nhận, Khám nội, Lấy máu...</summary>
    public class MedicalGroupPosition
    {
        [Key]
        public int PositionId { get; set; }

        public int GroupId { get; set; }
        [ForeignKey("GroupId")]
        public MedicalGroup MedicalGroup { get; set; }

        [Required]
        [MaxLength(100)]
        public string PositionName { get; set; } // "Tiếp nhận", "Khám nội", "Siêu âm"...

        public int RequiredCount { get; set; } = 1; // Số người cần
        public int AssignedCount { get; set; } = 0; // Số người đã gán (tự tổng hợp)

        [MaxLength(200)]
        public string Description { get; set; }

        public int SortOrder { get; set; } = 0;
    }
}
