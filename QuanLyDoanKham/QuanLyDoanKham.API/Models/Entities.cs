using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    // === AUTH & CATEGORY ===
    public class AppRole
    {
        [Key]
        public int RoleId { get; set; }
        [Required]
        [MaxLength(50)]
        public string RoleName { get; set; }
    }

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

        public int RoleId { get; set; }
        [ForeignKey("RoleId")]
        public AppRole Role { get; set; }

        public int? CompanyId { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
        public string AvatarPath { get; set; }
        [ForeignKey("CompanyId")]
        public Company Company { get; set; }
    }

    public class Company
    {
        [Key]
        public int CompanyId { get; set; }
        [Required]
        [MaxLength(200)]
        public string CompanyName { get; set; }
        [MaxLength(50)]
        public string TaxCode { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
    }

    // === BUSINESS ===
    public class HealthContract
    {
        [Key]
        public int HealthContractId { get; set; }
        public int CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public Company Company { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }
        public int PatientCount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsFinished { get; set; }
    }

    public class MedicalGroup
    {
        [Key]
        public int GroupId { get; set; }
        [Required]
        [MaxLength(200)]
        public string GroupName { get; set; }
        public DateTime ExamDate { get; set; }
        
        public int HealthContractId { get; set; }
        [ForeignKey("HealthContractId")]
        public HealthContract HealthContract { get; set; }

        public bool IsFinished { get; set; } = false;
    }

    public class Staff
    {
        [Key]
        public int StaffId { get; set; }

        [MaxLength(20)]
        public string EmployeeCode { get; set; } // Tương ứng MaNV

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [MaxLength(100)]
        public string FullNameUnsigned { get; set; } // HoTenKhongDau

        public int? BirthYear { get; set; } // NamSinh

        [MaxLength(10)]
        public string Gender { get; set; } // GioiTinh

        [MaxLength(20)]
        public string IDCardNumber { get; set; } // CMND

        [MaxLength(20)]
        public string TaxCode { get; set; } // MaSoThue

        [MaxLength(50)]
        public string BankAccountNumber { get; set; } // SoTaiKhoan

        [MaxLength(100)]
        public string BankAccountName { get; set; } // TenTaiKhoan

        [MaxLength(100)]
        public string BankName { get; set; } // NganHang

        [MaxLength(15)]
        public string PhoneNumber { get; set; } // SoDienThoai

        [MaxLength(100)]
        public string JobTitle { get; set; } // ChucDanh

        [MaxLength(100)]
        public string Department { get; set; } // DonVi (Đơn vị/Phòng ban)

        [MaxLength(20)]
        public string EmployeeType { get; set; } // LoaiNhanVien (Nội bộ/Thuê ngoài)

        // Path to images
        public string IDCardFrontPath { get; set; } // CMND_MatTruoc
        public string IDCardBackPath { get; set; } // CMND_MatSau
        public string PracticeCertificatePath { get; set; } // ChungChiHanhNghe
        public string AvatarPath { get; set; } // AnhChanDung

        [Column(TypeName = "decimal(18, 2)")]
        public decimal BaseSalary { get; set; }

        public bool IsActive { get; set; } = true; // TrangThai
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }

        public ICollection<GroupStaffDetail> GroupStaffDetails { get; set; } = new List<GroupStaffDetail>();
    }

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

        public double ShiftType { get; set; } // 0.5 or 1.0
        [Column(TypeName = "decimal(18, 2)")]
        public decimal CalculatedSalary { get; set; }
    }

    public class Supply
    {
        [Key]
        public int SupplyId { get; set; }
        [Required]
        [MaxLength(200)]
        public string SupplyName { get; set; }
        public bool IsFixedAsset { get; set; } // True = Co dinh, False = Tieu hao
        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }
        public int StockQuantity { get; set; }
    }

    public class GroupSupplyDetail
    {
        [Key]
        public int Id { get; set; }
        public int GroupId { get; set; }
        [ForeignKey("GroupId")]
        public MedicalGroup MedicalGroup { get; set; }

        public int SupplyId { get; set; }
        [ForeignKey("SupplyId")]
        public Supply Supply { get; set; }

        public int QuantityUsed { get; set; }
        public int ReturnQuantity { get; set; }
    }

    // === PATIENT MANAGEMENT ===
    public class Patient
    {
        [Key]
        public int PatientId { get; set; }
        
        public int HealthContractId { get; set; }
        [ForeignKey("HealthContractId")]
        public HealthContract HealthContract { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }
        
        public DateTime DateOfBirth { get; set; }
        
        [MaxLength(10)]
        public string Gender { get; set; } // Nam, Nữ
        
        [MaxLength(20)]
        public string IDCardNumber { get; set; }
        
        [MaxLength(15)]
        public string PhoneNumber { get; set; }
        
        [MaxLength(100)]
        public string Department { get; set; } // Phòng ban trong công ty
        
        public DateTime CreatedDate { get; set; }
    }

    public class ExamResult
    {
        [Key]
        public int ExamResultId { get; set; }
        
        public int PatientId { get; set; }
        [ForeignKey("PatientId")]
        public Patient Patient { get; set; }
        
        public int? GroupId { get; set; }
        [ForeignKey("GroupId")]
        public MedicalGroup Group { get; set; }
        
        [MaxLength(100)]
        public string ExamType { get; set; } // Nội khoa, Ngoại khoa, Xét nghiệm máu...
        
        public string Result { get; set; } // Kết quả chi tiết (có thể dài)
        
        [MaxLength(500)]
        public string Diagnosis { get; set; } // Chẩn đoán
        
        public int? DoctorStaffId { get; set; }
        [ForeignKey("DoctorStaffId")]
        public Staff DoctorStaff { get; set; }
        
        public DateTime ExamDate { get; set; }
    }

    // === PAYROLL SYSTEM ===
    public class PayrollRecord
    {
        [Key]
        public int PayrollId { get; set; }
        
        public int StaffId { get; set; }
        [ForeignKey("StaffId")]
        public Staff Staff { get; set; }
        
        public int Month { get; set; }
        public int Year { get; set; }
        
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalSalary { get; set; }
        
        public int TotalShifts { get; set; }
        
        public string Details { get; set; } // JSON chi tiết từng đoàn khám
        
        public DateTime GeneratedDate { get; set; }
    }

    // === AUDIT LOGGING ===
    public class AuditLog
    {
        [Key]
        public int LogId { get; set; }
        
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public AppUser User { get; set; }
        
        [MaxLength(50)]
        public string Action { get; set; } // CREATE, UPDATE, DELETE
        
        [MaxLength(100)]
        public string EntityType { get; set; } // Patient, Staff, Supply...
        
        public int EntityId { get; set; }
        
        public string OldValue { get; set; } // JSON
        public string NewValue { get; set; } // JSON
        
        public DateTime Timestamp { get; set; }
        
        [MaxLength(50)]
        public string IPAddress { get; set; }
    }
    // === INVENTORY MANAGEMENT ===
    public class SupplyTransaction
    {
        [Key]
        public int TransactionId { get; set; }
        public int SupplyId { get; set; }
        [ForeignKey("SupplyId")]
        public Supply Supply { get; set; }
        
        public int Quantity { get; set; }
        [MaxLength(50)]
        public string Type { get; set; } // IMPORT, EXPORT, ADJUST
        
        [MaxLength(500)]
        public string Note { get; set; }
        
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        public int? ProcessedByUserId { get; set; }
        [ForeignKey("ProcessedByUserId")]
        public AppUser ProcessedUser { get; set; }
    }
    // === PASSWORD RESET REQUESTS ===
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
        public string NewPassword { get; set; } // Optional: Admin can set this here
    }
}
