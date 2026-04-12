using QuanLyDoanKham.API.Models;
using System.ComponentModel.DataAnnotations;

namespace QuanLyDoanKham.API.DTOs
{
    public class AutoCreateGroupWithStaffRequestDto
    {
        [Required]
        public int HealthContractId { get; set; }

        [Required]
        [StringLength(255)]
        public required string GroupName { get; set; }

        [Required]
        public DateTime ExamDate { get; set; }

        /// <summary>
        /// Mode: "strict" (fail if min requirements not met), "partial" (assign as many as possible)
        /// </summary>
        [Required]
        public string AssignmentMode { get; set; } = "partial";

        /// <summary>
        /// Target ratio: items/staff (default 18)
        /// </summary>
        public int TargetRatio { get; set; } = 18;

        /// <summary>
        /// Minimum doctors needed for health checking
        /// </summary>
        public int MinimumDoctors { get; set; } = 2;

        public bool AllowReuseAiSuggestion { get; set; } = false;

        [Required]
        public required string IdempotencyKey { get; set; }
    }

    public class AutoCreateGroupWithStaffResponseDto
    {
        public required MedicalGroupDto Group { get; set; }
        public required AssignmentSummaryDto Summary { get; set; }
        public List<AssignedStaffDto> AssignedStaff { get; set; } = new();
        public List<UnassignedReasonDto> UnassignedReasons { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    public class AssignmentSummaryDto
    {
        public int RequiredHeadcount { get; set; }
        public int AssignedCount { get; set; }
        public int MissingCount { get; set; }
        public string? Mode { get; set; }
    }

    public class AssignedStaffDto
    {
        public int StaffId { get; set; }
        public string? StaffName { get; set; }
        public string? WorkPosition { get; set; }
        public double ShiftType { get; set; }
        public string? Reason { get; set; }
    }

    public class UnassignedReasonDto
    {
        public string? Position { get; set; }
        public string? Reason { get; set; }
    }
}
