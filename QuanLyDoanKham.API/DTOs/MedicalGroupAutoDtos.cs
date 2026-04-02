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
        public string GroupName { get; set; }

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
        public string IdempotencyKey { get; set; }
    }

    public class AutoCreateGroupWithStaffResponseDto
    {
        public MedicalGroupDto Group { get; set; }
        public AssignmentSummaryDto Summary { get; set; }
        public List<AssignedStaffDto> AssignedStaff { get; set; }
        public List<UnassignedReasonDto> UnassignedReasons { get; set; }
        public List<string> Warnings { get; set; }
    }

    public class AssignmentSummaryDto
    {
        public int RequiredHeadcount { get; set; }
        public int AssignedCount { get; set; }
        public int MissingCount { get; set; }
        public string Mode { get; set; }
    }

    public class AssignedStaffDto
    {
        public int StaffId { get; set; }
        public string StaffName { get; set; }
        public string WorkPosition { get; set; }
        public double ShiftType { get; set; }
        public string Reason { get; set; }
    }

    public class UnassignedReasonDto
    {
        public string Position { get; set; }
        public string Reason { get; set; }
    }
}
