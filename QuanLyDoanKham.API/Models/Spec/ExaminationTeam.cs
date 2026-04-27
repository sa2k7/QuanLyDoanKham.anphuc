using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models.Spec
{
    /// <summary>Đoàn khám sức khỏe</summary>
    public class ExaminationTeam
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string TeamCode { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string TeamName { get; set; } = null!;

        public int ContractId { get; set; }
        [ForeignKey("ContractId")]
        public Contract Contract { get; set; } = null!;

        public DateOnly ExamDate { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        public string? Notes { get; set; }

        /// <summary>Draft | Open | InProgress | Finished | Locked</summary>
        [MaxLength(20)]
        public string Status { get; set; } = "Draft";

        public int? CreatedByUserId { get; set; }
        [ForeignKey("CreatedByUserId")]
        public User? CreatedByUser { get; set; }

        public int? LeaderStaffId { get; set; }
        [ForeignKey("LeaderStaffId")]
        public SpecStaff? LeaderStaff { get; set; }

        public ICollection<ExamPosition> ExamPositions { get; set; } = new List<ExamPosition>();
        public ICollection<TeamStaff> TeamStaffs { get; set; } = new List<TeamStaff>();
        public ICollection<SpecPatient> Patients { get; set; } = new List<SpecPatient>();
        public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
        public ICollection<QRSession> QRSessions { get; set; } = new List<QRSession>();
        public ICollection<InventoryIssue> InventoryIssues { get; set; } = new List<InventoryIssue>();
        public ICollection<OtherExpense> OtherExpenses { get; set; } = new List<OtherExpense>();
        public TeamFinance? TeamFinance { get; set; }
    }
}
