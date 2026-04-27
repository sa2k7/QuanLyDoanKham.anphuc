using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models.Spec
{
    /// <summary>Phân công nhân sự vào đoàn khám tại vị trí cụ thể</summary>
    public class TeamStaff
    {
        [Key]
        public int Id { get; set; }

        public int TeamId { get; set; }
        [ForeignKey("TeamId")]
        public ExaminationTeam Team { get; set; } = null!;

        public int StaffId { get; set; }
        [ForeignKey("StaffId")]
        public SpecStaff Staff { get; set; } = null!;

        public int ExamPositionId { get; set; }
        [ForeignKey("ExamPositionId")]
        public ExamPosition ExamPosition { get; set; } = null!;

        [MaxLength(100)]
        public string? Role { get; set; }
    }
}
