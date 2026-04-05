using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Phân công nhân sự vào vị trí trong đoàn khám</summary>
    public class GroupStaffDetail
    {
        [Key]
        public int Id { get; set; }

        public int GroupId { get; set; }
        [ForeignKey("GroupId")]
        public MedicalGroup MedicalGroup { get; set; }

        public int StaffId { get; set; }
        [ForeignKey("StaffId")]
        public Staff Staff { get; set; }

        // Vị trí được phân công
        public int? PositionId { get; set; }
        [ForeignKey("PositionId")]
        public Position Position { get; set; }

        public int? GroupPositionQuotaId { get; set; }
        [ForeignKey("GroupPositionQuotaId")]
        public GroupPositionQuota GroupPositionQuota { get; set; }

        [MaxLength(100)]
        public string WorkPosition { get; set; } // Tên vị trí (denormalized để dễ query)

        [MaxLength(50)]
        public string WorkStatus { get; set; } = "Pending"; // Pending, Joined, Absent, Leave

        public DateTime ExamDate { get; set; }

        // Chấm công
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }

        // 0.5 = nửa ngày, 1.0 = một ngày
        public double ShiftType { get; set; } = 1.0;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal CalculatedSalary { get; set; }

        public int? AssignedByUserId { get; set; }
        public DateTime? AssignedAt { get; set; }

        public string Note { get; set; }
    }
}
