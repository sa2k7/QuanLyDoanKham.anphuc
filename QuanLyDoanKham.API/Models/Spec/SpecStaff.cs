using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models.Spec
{
    /// <summary>Nhân sự tham gia đoàn khám (spec-aligned)</summary>
    public class SpecStaff
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [MaxLength(100)]
        public string? ProfessionalRole { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DailyRate { get; set; }

        public int? DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }

        public ICollection<TeamStaff> TeamStaffs { get; set; } = new List<TeamStaff>();
        public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    }
}
