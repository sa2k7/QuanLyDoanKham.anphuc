using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyDoanKham.API.DTOs
{
    // ================================================================
    // MEDICAL BATCH DTOs
    // health-check-domain-refactor — Batch 2, Task 8
    // Additive only: no existing DTO modified.
    // ================================================================

    // ── REQUEST DTOs ─────────────────────────────────────────────────

    /// <summary>
    /// Body for POST /api/medical-batches.
    /// AutoGenerate = true → bulk-create EstimatedCount records atomically.
    /// AutoGenerate = false (default) → lazy creation via separate endpoint.
    /// </summary>
    public class CreateMedicalBatchDto
    {
        [Required(ErrorMessage = "ContractId không được để trống.")]
        [Range(1, int.MaxValue, ErrorMessage = "ContractId phải lớn hơn 0.")]
        public int ContractId { get; set; }

        [Required]
        [Range(1, 10000, ErrorMessage = "EstimatedCount phải từ 1 đến 10,000.")]
        public int EstimatedCount { get; set; }

        /// <summary>
        /// true  → auto-generate EstimatedCount MedicalBatchRecords in one transaction.
        /// false → create batch only; records created on demand via lazy endpoint.
        /// </summary>
        public bool AutoGenerate { get; set; } = false;
    }

    /// <summary>Body for PATCH /api/medical-records/{id}/status</summary>
    public class UpdateBatchRecordStatusDto
    {
        /// <summary>Only "Done" is accepted as a target status.</summary>
        [Required(ErrorMessage = "Status không được để trống.")]
        public string Status { get; set; } = "Done";
    }

    /// <summary>Body for POST /api/campaigns/{campaignId}/role-requirements</summary>
    public class SetRoleRequirementDto
    {
        [Required(ErrorMessage = "Role không được để trống.")]
        [MaxLength(30)]
        public string Role { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "RequiredCount phải lớn hơn hoặc bằng 1.")]
        public int RequiredCount { get; set; }
    }

    // ── RESPONSE DTOs ─────────────────────────────────────────────────

    /// <summary>Response for GET /api/medical-batches/{id} and POST /api/medical-batches</summary>
    public class MedicalBatchDto
    {
        public Guid Id { get; set; }
        public int ContractId { get; set; }
        public int EstimatedCount { get; set; }

        /// <summary>Current count of MedicalBatchRecords linked to this batch.</summary>
        public int RecordCount { get; set; }

        /// <summary>
        /// Populated only on bulk-generation response (POST with AutoGenerate=true).
        /// Null on GET responses.
        /// </summary>
        public int? RecordsGenerated { get; set; }

        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response for GET /api/medical-batches/{batchId}/records (paginated list item).
    /// No personal information — code + status only.
    /// </summary>
    public class MedicalBatchRecordDto
    {
        public Guid Id { get; set; }
        public Guid BatchId { get; set; }

        /// <summary>Auto-generated sequential code: BN0001 → BNXXXX</summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>Pending | Done</summary>
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Response item for GET /api/campaigns/{campaignId}/role-requirements.
    /// Includes live assigned count alongside the required count.
    /// </summary>
    public class RoleRequirementWithCountDto
    {
        public Guid Id { get; set; }
        public int CampaignId { get; set; }

        /// <summary>StaffRole enum value stored as string (e.g. "TiepNhan")</summary>
        public string Role { get; set; } = string.Empty;

        public int RequiredCount { get; set; }

        /// <summary>
        /// Current number of staff assigned to this role in this campaign.
        /// Computed at query time — not stored.
        /// </summary>
        public int AssignedCount { get; set; }

        /// <summary>True when AssignedCount &lt; RequiredCount (under-assigned)</summary>
        public bool IsUnderAssigned => AssignedCount < RequiredCount;

        /// <summary>True when AssignedCount &gt; RequiredCount (over-assigned)</summary>
        public bool IsOverAssigned => AssignedCount > RequiredCount;
    }

    // ── ATTENDANCE (NEW SIMPLIFIED APIs) ─────────────────────────────
    // These DTOs are for the 3 new attendance endpoints added alongside
    // the existing attendance logic. The existing CheckInOutDto and
    // AttendanceSummaryDto in MasterDataDtos.cs are NOT modified.

    /// <summary>Response item for GET /api/attendance/today</summary>
    public class AttendanceTodayDto
    {
        public int CampaignId { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        public DateTime ExamDate { get; set; }

        /// <summary>True if the current user has already checked in for this campaign today.</summary>
        public bool CheckedIn { get; set; }

        /// <summary>True if the current user has already checked out from this campaign today.</summary>
        public bool CheckedOut { get; set; }
    }

    /// <summary>Body for POST /api/attendance/checkin</summary>
    public class AttendanceCheckInDto
    {
        [Required(ErrorMessage = "CampaignId không được để trống.")]
        [Range(1, int.MaxValue, ErrorMessage = "CampaignId phải lớn hơn 0.")]
        public int CampaignId { get; set; }

        /// <summary>Client-supplied timestamp; defaults to server time if not provided.</summary>
        public DateTime? Timestamp { get; set; }
    }

    /// <summary>Body for POST /api/attendance/checkout</summary>
    public class AttendanceCheckOutDto
    {
        [Required(ErrorMessage = "CampaignId không được để trống.")]
        [Range(1, int.MaxValue, ErrorMessage = "CampaignId phải lớn hơn 0.")]
        public int CampaignId { get; set; }

        /// <summary>Client-supplied timestamp; defaults to server time if not provided.</summary>
        public DateTime? Timestamp { get; set; }
    }

    // ── WAREHOUSE ─────────────────────────────────────────────────────

    /// <summary>Response item for GET /api/warehouse/campaigns</summary>
    public class WarehouseCampaignDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public DateTime ExamDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ContractCode { get; set; }
        public string? CompanyName { get; set; }
    }
}
