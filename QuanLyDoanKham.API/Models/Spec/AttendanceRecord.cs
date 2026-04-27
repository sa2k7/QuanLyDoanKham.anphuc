using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models.Spec
{
    /// <summary>Bản ghi chấm công nhân sự trong đoàn khám</summary>
    public class AttendanceRecord
    {
        [Key]
        public int Id { get; set; }

        public int StaffId { get; set; }
        [ForeignKey("StaffId")]
        public SpecStaff Staff { get; set; } = null!;

        public int TeamId { get; set; }
        [ForeignKey("TeamId")]
        public ExaminationTeam Team { get; set; } = null!;

        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        public decimal WorkUnits { get; set; } = 0;

        /// <summary>Hoàn chỉnh | Chưa hoàn chỉnh</summary>
        [MaxLength(30)]
        public string Status { get; set; } = "Chưa hoàn chỉnh";
    }
}
