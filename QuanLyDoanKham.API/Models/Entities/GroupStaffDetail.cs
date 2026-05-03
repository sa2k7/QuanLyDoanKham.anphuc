using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>Phân công nhân sự vĂ o vị trí trong đoĂ n khám</summary>
    public class GroupStaffDetail
    {
        [Key]
        public int Id { get; set; }

        public int GroupId { get; set; }
        [ForeignKey("GroupId")]
        public MedicalGroup MedicalGroup { get; set; } = null!;

        public int StaffId { get; set; }
        [ForeignKey("StaffId")]
        public Staff Staff { get; set; } = null!;

        public int? PositionId { get; set; }
        [ForeignKey("PositionId")]
        public Position? Position { get; set; }

        public int? GroupPositionQuotaId { get; set; }
        [ForeignKey("GroupPositionQuotaId")]
        public GroupPositionQuota? GroupPositionQuota { get; set; }

        [MaxLength(100)]
        public string WorkPosition { get; set; } = null!; // Tên vị trí (denormalized để dá»… query)

        [MaxLength(50)]
        public string WorkStatus { get; set; } = "Pending"; // Pending, Joined, Absent, Leave

        public DateTime ExamDate { get; set; }

        // Chấm công
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }

        // 0.5 = nửa ngĂ y, 1.0 = má»™t ngĂ y
        public double ShiftType { get; set; } = 1.0;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal CalculatedSalary { get; set; }

        public int? AssignedByUserId { get; set; }
        public DateTime? AssignedAt { get; set; }

        [MaxLength(200)]
        public string? PickupLocation { get; set; } // Ä ón tại

        public string? Note { get; set; }
    }
}
