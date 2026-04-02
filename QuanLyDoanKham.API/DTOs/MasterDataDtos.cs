using System;
using System.Collections.Generic;

namespace QuanLyDoanKham.API.DTOs
{
    // ================================================================
    // COMPANY DTOs
    // ================================================================
    public class CompanyDto
    {
        public int CompanyId { get; set; }
        public string ShortName { get; set; }
        public string CompanyName { get; set; }
        public string TaxCode { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string ContactPerson { get; set; }
        public string ContactPhone { get; set; }
        public int? CompanySize { get; set; }
        public string Industry { get; set; }
        public string Notes { get; set; }
    }

    // ================================================================
    // CONTRACT DTOs
    // ================================================================
    public class HealthContractDto
    {
        public int HealthContractId { get; set; }
        public int CompanyId { get; set; }
        public string ContractCode { get; set; }
        public string ShortName { get; set; }
        public string CompanyName { get; set; }
        public DateTime SigningDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal UnitPrice { get; set; }
        public int ExpectedQuantity { get; set; }
        public string UnitName { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public int CurrentApprovalStep { get; set; }
        public string FilePath { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Notes { get; set; }
        public int TotalGroups { get; set; }
        public List<ContractStatusHistoryDto> StatusHistories { get; set; } = new();
        public List<ContractApprovalHistoryDto> ApprovalHistories { get; set; } = new();
        public List<ContractAttachmentDto> Attachments { get; set; } = new();
    }

    public class ContractStatusHistoryDto
    {
        public int Id { get; set; }
        public string OldStatus { get; set; }
        public string NewStatus { get; set; }
        public string Note { get; set; }
        public DateTime ChangedAt { get; set; }
        public string ChangedBy { get; set; }
    }

    public class ContractApprovalHistoryDto
    {
        public int Id { get; set; }
        public int StepOrder { get; set; }
        public string StepName { get; set; }
        public string Action { get; set; }
        public string Note { get; set; }
        public string ApprovedByName { get; set; }
        public DateTime ActionDate { get; set; }
    }

    public class ContractAttachmentDto
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public DateTime UploadedAt { get; set; }
        public string UploadedBy { get; set; }
    }

    /// <summary>Body cho approve/reject hợp đồng</summary>
    public class ApprovalActionDto
    {
        public string Note { get; set; }
    }

    // ================================================================
    // MEDICAL GROUP DTOs
    // ================================================================
    public class MedicalGroupDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public DateTime ExamDate { get; set; }
        public string Slot { get; set; }
        public string TeamCode { get; set; }
        public int HealthContractId { get; set; }
        public string ContractCode { get; set; }
        public string ShortName { get; set; }
        public string CompanyName { get; set; }
        public string Status { get; set; }
        public string ImportFilePath { get; set; }
        public string ManagerName { get; set; }
        public string GroupLeaderName { get; set; }
        public int TotalPositions { get; set; }
        public int TotalAssigned { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
    }

    public class MedicalGroupPositionDto
    {
        public int PositionId { get; set; }
        public int GroupId { get; set; }
        public string PositionName { get; set; }
        public int RequiredCount { get; set; }
        public int AssignedCount { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
    }

    public class AssignStaffDto
    {
        public int StaffId { get; set; }
        public int? PositionId { get; set; }
        public string WorkPosition { get; set; }
        public double ShiftType { get; set; } = 1.0;
    }

    public class GenerateGroupsDto
    {
        public int HealthContractId { get; set; }
        /// <summary>Null = tạo 1 đoàn/ngày theo range hợp đồng. Có value = tạo đoàn cho ngày cụ thể.</summary>
        public List<DateTime> SpecificDates { get; set; }
        public string DefaultSlot { get; set; } = "FullDay";
    }

    // ================================================================
    // DEPARTMENT DTOs
    // ================================================================
    public class DepartmentDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentCode { get; set; }
        public string Description { get; set; }
        public int TotalStaff { get; set; }
        public int TotalUsers { get; set; }
    }

    // ================================================================
    // ATTENDANCE (CHẤM CÔNG) DTOs
    // ================================================================
    public class CheckInOutDto
    {
        /// <summary>GroupId của đoàn khám</summary>
        public int GroupId { get; set; }
        /// <summary>StaffId để check-in/out thủ công</summary>
        public int? StaffId { get; set; }
        /// <summary>Token từ QR code (mã hóa GroupId + timestamp)</summary>
        public string QrToken { get; set; }
        public string Note { get; set; }
    }

    public class AttendanceSummaryDto
    {
        public int StaffId { get; set; }
        public string StaffName { get; set; }
        public string EmployeeCode { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public double TotalActualDays { get; set; }
        public List<AttendanceDetailDto> Details { get; set; } = new();
    }

    public class AttendanceDetailDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public DateTime ExamDate { get; set; }
        public double ShiftType { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string WorkStatus { get; set; }
    }

    // ================================================================
    // GROUP COST DTOs
    // ================================================================
    public class GroupCostDto
    {
        public int CostId { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public DateTime ExamDate { get; set; }
        public decimal StaffCost { get; set; }
        public decimal SupplyCost { get; set; }
        public decimal OtherCost { get; set; }
        public decimal TotalCost { get; set; }
        public string Note { get; set; }
        public DateTime CalculatedAt { get; set; }
    }

    // ================================================================
    // SCHEDULE/CALENDAR DTO
    // ================================================================
    public class ScheduleCalendarDto
    {
        public int CalendarId { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string CompanyName { get; set; }
        public string Slot { get; set; }
        public DateTime ExamDate { get; set; }
        public int? StaffId { get; set; }
        public string StaffName { get; set; }
        public bool IsConfirmed { get; set; }
    }

    // ================================================================
    // SHARED DTOs
    // ================================================================
    public class StatusUpdateDto
    {
        public string Status { get; set; }
        public string Note { get; set; }
    }
}
