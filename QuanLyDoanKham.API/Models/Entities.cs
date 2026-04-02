using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    // ================================================================
    // SECTION 1: AUTH, USERS, ROLES, PERMISSIONS
    // ================================================================

    /// <summary>Role cơ bản của hệ thống</summary>
    public class AppRole
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RoleName { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }

    /// <summary>Permission granular - ví dụ: HopDong.Approve, DoanKham.Create</summary>
    public class Permission
    {
        [Key]
        public int PermissionId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PermissionKey { get; set; } // "HopDong.Approve"

        [MaxLength(200)]
        public string PermissionName { get; set; } // "Phê duyệt hợp đồng"

        [MaxLength(50)]
        public string Module { get; set; } // "HopDong", "DoanKham", "Kho"...

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }

    /// <summary>Mapping nhiều-nhiều: User ↔ Role</summary>
    public class UserRole
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public AppUser User { get; set; }

        public int RoleId { get; set; }
        [ForeignKey("RoleId")]
        public AppRole Role { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string AssignedBy { get; set; }
    }

    /// <summary>Mapping nhiều-nhiều: Role ↔ Permission</summary>
    public class RolePermission
    {
        [Key]
        public int Id { get; set; }

        public int RoleId { get; set; }
        [ForeignKey("RoleId")]
        public AppRole Role { get; set; }

        public int PermissionId { get; set; }
        [ForeignKey("PermissionId")]
        public Permission Permission { get; set; }
    }

    /// <summary>Người dùng hệ thống</summary>
    public class AppUser
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [MaxLength(100)]
        public string FullName { get; set; }

        // Primary role (backward compat) - vẫn giữ để JWT đơn giản hơn
        public int RoleId { get; set; }
        [ForeignKey("RoleId")]
        public AppRole Role { get; set; }

        // Phòng ban
        public int? DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }

        public int? CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public Company Company { get; set; }

        public string RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }

        [MaxLength(200)]
        public string Email { get; set; }

        public string AvatarPath { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Multi-role support
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }

    // ================================================================
    // SECTION 2: DEPARTMENTS (PHÒNG BAN)
    // ================================================================

    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required]
        [MaxLength(100)]
        public string DepartmentName { get; set; }

        [MaxLength(20)]
        public string DepartmentCode { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
        public ICollection<Staff> Staffs { get; set; } = new List<Staff>();
    }

    // ================================================================
    // SECTION 3: COMPANIES
    // ================================================================

    public class Company
    {
        [Key]
        public int CompanyId { get; set; }

        [MaxLength(100)]
        public string ShortName { get; set; }

        [Required]
        [MaxLength(200)]
        public string CompanyName { get; set; }

        [MaxLength(50)]
        public string TaxCode { get; set; }

        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }

        [MaxLength(200)]
        public string ContactPerson { get; set; }

        [MaxLength(15)]
        public string ContactPhone { get; set; }

        public int? CompanySize { get; set; }

        [MaxLength(100)]
        public string Industry { get; set; }

        public string Notes { get; set; }
    }

    // ================================================================
    // SECTION 4: CONTRACTS & APPROVAL WORKFLOW
    // ================================================================

    public class HealthContract
    {
        [Key]
        public int HealthContractId { get; set; }

        public int CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public Company Company { get; set; }

        [MaxLength(50)]
        public string ContractCode { get; set; } // Số hợp đồng

        public DateTime SigningDate { get; set; }   // Ngày ký HĐ
        public DateTime StartDate { get; set; }     // Ngày bắt đầu khám
        public DateTime EndDate { get; set; }       // Ngày kết thúc khám

        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }

        public int ExpectedQuantity { get; set; }

        [MaxLength(50)]
        public string UnitName { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        // Trạng thái: Draft, PendingApproval, Approved, Rejected, Active, Finished, Locked
        [MaxLength(50)]
        public string Status { get; set; } = "Draft";

        public int CurrentApprovalStep { get; set; } = 0; // Bước duyệt hiện tại

        public string FilePath { get; set; }  // File hợp đồng đính kèm

        // Audit
        public int? CreatedByUserId { get; set; }
        [ForeignKey("CreatedByUserId")]
        public AppUser CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        [MaxLength(100)]
        public string UpdatedBy { get; set; }

        public string Notes { get; set; }

        public ICollection<ContractStatusHistory> StatusHistories { get; set; } = new List<ContractStatusHistory>();
        public ICollection<ContractApprovalHistory> ApprovalHistories { get; set; } = new List<ContractApprovalHistory>();
        public ICollection<ContractAttachment> Attachments { get; set; } = new List<ContractAttachment>();
        public ICollection<MedicalGroup> MedicalGroups { get; set; } = new List<MedicalGroup>();
    }

    /// <summary>Cấu hình bước phê duyệt hợp đồng (1..N cấp)</summary>
    public class ContractApprovalStep
    {
        [Key]
        public int StepId { get; set; }

        public int StepOrder { get; set; } // 1, 2, 3...

        [Required]
        [MaxLength(100)]
        public string StepName { get; set; } // "Trưởng phòng duyệt", "Ban giám đốc duyệt"

        // Permission cần có để duyệt bước này
        [MaxLength(100)]
        public string RequiredPermission { get; set; } // "HopDong.Approve"

        public bool IsActive { get; set; } = true;
    }

    /// <summary>Lịch sử duyệt từng hợp đồng theo bước</summary>
    public class ContractApprovalHistory
    {
        [Key]
        public int Id { get; set; }

        public int HealthContractId { get; set; }
        [ForeignKey("HealthContractId")]
        public HealthContract HealthContract { get; set; }

        public int StepOrder { get; set; }

        [MaxLength(100)]
        public string StepName { get; set; }

        [MaxLength(20)]
        public string Action { get; set; } // Approved, Rejected, Recalled

        public string Note { get; set; }

        public int ApprovedByUserId { get; set; }
        [ForeignKey("ApprovedByUserId")]
        public AppUser ApprovedByUser { get; set; }

        public DateTime ActionDate { get; set; } = DateTime.Now;
    }

    /// <summary>File đính kèm hợp đồng</summary>
    public class ContractAttachment
    {
        [Key]
        public int Id { get; set; }

        public int HealthContractId { get; set; }
        [ForeignKey("HealthContractId")]
        public HealthContract HealthContract { get; set; }

        [MaxLength(200)]
        public string FileName { get; set; }

        public string FilePath { get; set; }

        [MaxLength(50)]
        public string FileType { get; set; } // PDF, DOCX, IMG

        public DateTime UploadedAt { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string UploadedBy { get; set; }
    }

    /// <summary>Lịch sử thay đổi trạng thái hợp đồng</summary>
    public class ContractStatusHistory
    {
        [Key]
        public int Id { get; set; }

        public int HealthContractId { get; set; }
        [ForeignKey("HealthContractId")]
        public HealthContract HealthContract { get; set; }

        [MaxLength(50)]
        public string OldStatus { get; set; }

        [MaxLength(50)]
        public string NewStatus { get; set; }

        public string Note { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string ChangedBy { get; set; }
    }

    // ================================================================
    // SECTION 5: MEDICAL GROUPS (ĐOÀN KHÁM)
    // ================================================================

    public class MedicalGroup
    {
        [Key]
        public int GroupId { get; set; }

        [Required]
        [MaxLength(200)]
        public string GroupName { get; set; }

        public DateTime ExamDate { get; set; }

        // Ca / Slot (sáng/chiều/tối - cho phép nhiều đoàn/ngày)
        [MaxLength(20)]
        public string Slot { get; set; } = "FullDay"; // Morning, Afternoon, Evening, FullDay

        [MaxLength(50)]
        public string TeamCode { get; set; } // Phân biệt team A/B nếu nhiều đoàn/ngày

        public int HealthContractId { get; set; }
        [ForeignKey("HealthContractId")]
        public HealthContract HealthContract { get; set; }

        // Trạng thái: Draft, Open, InProgress, Finished, Locked
        [MaxLength(50)]
        public string Status { get; set; } = "Draft";

        public string ImportFilePath { get; set; }

        // Người quản lý đoàn khám (AppUser)
        public int? ManagerUserId { get; set; }
        [ForeignKey("ManagerUserId")]
        public AppUser ManagerUser { get; set; }

        // Trưởng đoàn (Staff - người có thể mở QR)
        public int? GroupLeaderStaffId { get; set; }
        [ForeignKey("GroupLeaderStaffId")]
        public Staff GroupLeaderStaff { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [MaxLength(100)]
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(100)]
        public string UpdatedBy { get; set; }

        public ICollection<MedicalGroupPosition> Positions { get; set; } = new List<MedicalGroupPosition>();
        public ICollection<GroupStaffDetail> StaffDetails { get; set; } = new List<GroupStaffDetail>();
    }

    /// <summary>Vị trí trong đoàn khám: Tiếp nhận, Khám nội, Lấy máu...</summary>
    public class MedicalGroupPosition
    {
        [Key]
        public int PositionId { get; set; }

        public int GroupId { get; set; }
        [ForeignKey("GroupId")]
        public MedicalGroup MedicalGroup { get; set; }

        [Required]
        [MaxLength(100)]
        public string PositionName { get; set; } // "Tiếp nhận", "Khám nội", "Siêu âm"...

        public int RequiredCount { get; set; } = 1; // Số người cần
        public int AssignedCount { get; set; } = 0; // Số người đã gán (tự tổng hợp)

        [MaxLength(200)]
        public string Description { get; set; }

        public int SortOrder { get; set; } = 0;
    }

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
        public MedicalGroupPosition Position { get; set; }

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

        public string Note { get; set; }
    }

    // ================================================================
    // SECTION 6: STAFF (NHÂN SỰ)
    // ================================================================

    public class Staff
    {
        [Key]
        public int StaffId { get; set; }

        [MaxLength(20)]
        public string EmployeeCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [MaxLength(100)]
        public string FullNameUnsigned { get; set; }

        public int? BirthYear { get; set; }

        [MaxLength(10)]
        public string Gender { get; set; }

        [MaxLength(20)]
        public string IDCardNumber { get; set; }

        [MaxLength(20)]
        public string TaxCode { get; set; }

        [MaxLength(50)]
        public string BankAccountNumber { get; set; }

        [MaxLength(100)]
        public string BankAccountName { get; set; }

        [MaxLength(100)]
        public string BankName { get; set; }

        [MaxLength(15)]
        public string PhoneNumber { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(100)]
        public string JobTitle { get; set; }

        // BacSi, DieuDuong, KyThuatVien, NhanVienHoTro
        [MaxLength(50)]
        public string StaffType { get; set; }

        [MaxLength(100)]
        public string Specialty { get; set; } // Chuyên khoa

        // Phòng ban
        public int? DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }

        [MaxLength(100)]
        public string DepartmentName { get; set; } // Tên phòng ban (denormalized, không cần join)

        [MaxLength(20)]
        public string EmployeeType { get; set; } // NoiBo, ThueNgoai

        [Column(TypeName = "decimal(18, 2)")]
        public decimal DailyRate { get; set; } // Đơn giá ngày

        [Column(TypeName = "decimal(18, 2)")]
        public decimal BaseSalary { get; set; } // Lương cơ bản tháng

        public int StandardWorkDays { get; set; } = 26; // Công chuẩn mặc định

        // Loại lương: ByDay (tính theo DailyRate), ByMonth (BaseSalary ÷ StdDays × actual)
        [MaxLength(20)]
        public string SalaryType { get; set; } = "ByDay";

        public string IDCardFrontPath { get; set; }
        public string IDCardBackPath { get; set; }
        public string PracticeCertificatePath { get; set; }
        public string AvatarPath { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }

        public ICollection<GroupStaffDetail> GroupStaffDetails { get; set; } = new List<GroupStaffDetail>();
    }

    // ================================================================
    // SECTION 7: SCHEDULE & CALENDAR
    // ================================================================

    /// <summary>Lịch khám - sinh tự động khi tạo đoàn</summary>
    public class ScheduleCalendar
    {
        [Key]
        public int CalendarId { get; set; }

        public int GroupId { get; set; }
        [ForeignKey("GroupId")]
        public MedicalGroup MedicalGroup { get; set; }

        public DateTime ExamDate { get; set; }

        // NULL = toàn đoàn, có StaffId = lịch riêng nhân viên
        public int? StaffId { get; set; }
        [ForeignKey("StaffId")]
        public Staff Staff { get; set; }

        // ---- Chấm công QR ----
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }

        [MaxLength(200)]
        public string Note { get; set; }

        public bool IsConfirmed { get; set; } = false;
    }

    // ================================================================
    // SECTION 8: SUPPLIES & WAREHOUSE
    // ================================================================

    public class Supply
    {
        [Key]
        public int SupplyId { get; set; }

        [Required]
        [MaxLength(200)]
        public string SupplyName { get; set; }

        public string Unit { get; set; }
        public bool IsFixedAsset { get; set; }

        [MaxLength(100)]
        public string Category { get; set; }

        [MaxLength(50)]
        public string LotNumber { get; set; }

        public DateTime? ExpirationDate { get; set; }
        public DateTime? ManufactureDate { get; set; }
        public int MinStockLevel { get; set; } = 10;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }

        public int TotalStock { get; set; }
    }

    public class SupplyInventoryVoucher
    {
        [Key]
        public int VoucherId { get; set; }

        [MaxLength(20)]
        public string VoucherCode { get; set; }

        public DateTime CreateDate { get; set; }

        [MaxLength(50)]
        public string Type { get; set; } // IMPORT, EXPORT

        public int? GroupId { get; set; }
        [ForeignKey("GroupId")]
        public MedicalGroup MedicalGroup { get; set; }

        public int CreatedByUserId { get; set; }
        [ForeignKey("CreatedByUserId")]
        public AppUser CreatedByUser { get; set; }

        public string Note { get; set; }

        public ICollection<SupplyInventoryDetail> Details { get; set; } = new List<SupplyInventoryDetail>();
    }

    public class SupplyInventoryDetail
    {
        [Key]
        public int Id { get; set; }

        public int VoucherId { get; set; }
        [ForeignKey("VoucherId")]
        public SupplyInventoryVoucher Voucher { get; set; }

        public int SupplyId { get; set; }
        [ForeignKey("SupplyId")]
        public Supply Supply { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }
    }

    // ================================================================
    // SECTION 9: COST & REVENUE TRACKING
    // ================================================================

    /// <summary>Chi phí từng đoàn khám</summary>
    public class GroupCost
    {
        [Key]
        public int CostId { get; set; }

        public int GroupId { get; set; }
        [ForeignKey("GroupId")]
        public MedicalGroup MedicalGroup { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal StaffCost { get; set; }      // Chi phí nhân sự

        [Column(TypeName = "decimal(18, 2)")]
        public decimal SupplyCost { get; set; }     // Chi phí vật tư

        [Column(TypeName = "decimal(18, 2)")]
        public decimal OtherCost { get; set; }      // Chi phí khác

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalCost { get; set; }      // Tổng chi phí

        public string Note { get; set; }
        public DateTime CalculatedAt { get; set; } = DateTime.Now;

        public int? CalculatedByUserId { get; set; }
        [ForeignKey("CalculatedByUserId")]
        public AppUser CalculatedByUser { get; set; }
    }

    /// <summary>Tổng hợp doanh thu/lợi nhuận theo hợp đồng</summary>
    public class ContractRevenueSummary
    {
        [Key]
        public int SummaryId { get; set; }

        public int HealthContractId { get; set; }
        [ForeignKey("HealthContractId")]
        public HealthContract HealthContract { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalContractValue { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalGroupCosts { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Revenue { get; set; } // = TotalContractValue - TotalGroupCosts

        public int TotalGroups { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string GeneratedBy { get; set; }
    }

    // ================================================================
    // SECTION 10: PAYROLL
    // ================================================================

    public class PayrollRecord
    {
        [Key]
        public int PayrollId { get; set; }

        public int StaffId { get; set; }
        [ForeignKey("StaffId")]
        public Staff Staff { get; set; }

        public int Month { get; set; }
        public int Year { get; set; }

        public double TotalActualDays { get; set; }  // Ngày công thực tế (0.5/1.0)
        public int StandardWorkDays { get; set; } = 26;

        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DailyRate { get; set; }

        /// <summary>Monthly | Daily</summary>
        [MaxLength(20)]
        public string SalaryType { get; set; } = "Monthly";

        /// <summary>Công thức: Monthly = (BaseSalary / StandardWorkDays) × TotalActualDays
        ///                   Daily   = DailyRate × TotalActualDays</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // Backward compat alias
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalSalary { get; set; }

        public DateTime GeneratedAt { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string GeneratedBy { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Draft"; // Draft | Confirmed | Paid
    }

    // ================================================================
    // SECTION 11: AUDIT, NOTIFICATIONS, PASSWORD RESET
    // ================================================================

    public class AuditLog
    {
        [Key]
        public int LogId { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public AppUser User { get; set; }

        [MaxLength(50)]
        public string Action { get; set; }

        [MaxLength(100)]
        public string EntityType { get; set; }

        public int EntityId { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string IPAddress { get; set; }
    }

    public class PasswordResetRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        public DateTime RequestedDate { get; set; } = DateTime.Now;
        public bool IsProcessed { get; set; } = false;

        [MaxLength(100)]
        public string NewPassword { get; set; }
    }

    public class Notification
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public AppUser User { get; set; }

        [Required]
        public string Message { get; set; }

        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Link { get; set; }
    }
}
