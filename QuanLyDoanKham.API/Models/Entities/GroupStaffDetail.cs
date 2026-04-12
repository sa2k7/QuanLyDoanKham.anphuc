using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>PhĂ¢n cĂ´ng nhĂ¢n sá»± vĂ o vá»‹ trĂ­ trong Ä‘oĂ n khĂ¡m</summary>
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
        public string WorkPosition { get; set; } = null!; // TĂªn vá»‹ trĂ­ (denormalized Ä‘á»ƒ dá»… query)

        [MaxLength(50)]
        public string WorkStatus { get; set; } = "Pending"; // Pending, Joined, Absent, Leave

        public DateTime ExamDate { get; set; }

        // Cháº¥m cĂ´ng
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }

        // 0.5 = ná»­a ngĂ y, 1.0 = má»™t ngĂ y
        public double ShiftType { get; set; } = 1.0;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal CalculatedSalary { get; set; }

        public int? AssignedByUserId { get; set; }
        public DateTime? AssignedAt { get; set; }

        public string? Note { get; set; }
    }
}
