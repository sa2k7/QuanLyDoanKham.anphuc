using System;
using System.Collections.Generic;
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

        public DateTime SigningDate { get; set; } // Ngày ký HD
        public DateTime StartDate { get; set; } // Ngày bắt đầu
        public DateTime EndDate { get; set; } // Ngày kết thúc

        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; } // Đơn giá 1 người
        public int ExpectedQuantity { get; set; } // Số lượng người dự kiến
        [MaxLength(50)]
        public string UnitName { get; set; } // Đơn vị tính (người, gói...)

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; } // Tổng giá trị = UnitPrice * ExpectedQuantity

        [MaxLength(50)]
        public string Status { get; set; } // Pending, Active, Finished, Locked
        public string FilePath { get; set; } // Đường dẫn file HĐ (Excel/PDF)
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

        [MaxLength(50)]
        public string Status { get; set; } // Open, Finished, Locked
        public string ImportFilePath { get; set; } // File dữ liệu nhập từ ngoài
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

        public string IDCardFrontPath { get; set; } 
        public string IDCardBackPath { get; set; } 
        public string PracticeCertificatePath { get; set; } 
        public string AvatarPath { get; set; } 

        [Column(TypeName = "decimal(18, 2)")]
        public decimal BaseSalary { get; set; }

        public bool IsActive { get; set; } = true; 
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
        public DateTime ExamDate { get; set; } // Ngày đi khám cụ thể

        [MaxLength(100)]
        public string WorkPosition { get; set; } // Tiếp nhận, Cân đo...
        [MaxLength(50)]
        public string WorkStatus { get; set; } // Đang làm, Đang chờ...
    }

    public class Supply
    {
        [Key]
        public int SupplyId { get; set; }
        [Required]
        [MaxLength(200)]
        public string SupplyName { get; set; }
        public string Unit { get; set; } // Đơn vị tính (Cái, Hộp, Túi...)
        public bool IsFixedAsset { get; set; } // True = Có định, False = Tiêu hao
        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }
        public int TotalStock { get; set; } // Tổng tồn kho thực tế
    }

    public class SupplyInventoryVoucher
    {
        [Key]
        public int VoucherId { get; set; }
        [MaxLength(20)]
        public string VoucherCode { get; set; } // Mã phiếu (NK001, XK001)
        public DateTime CreateDate { get; set; }
        [MaxLength(50)]
        public string Type { get; set; } // IMPORT (Nhập kho), EXPORT (Xuất cho đoàn)
        
        public int? GroupId { get; set; } // Nếu là EXPORT thì xuất cho đoàn nào
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
        public decimal Price { get; set; } // Giá tại thời điểm nhập/xuất
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
        
        public int TotalDays { get; set; } // Tổng số ngày đi khám trong tháng
        
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
        public string EntityType { get; set; } 
        
        public int EntityId { get; set; }
        
        public string OldValue { get; set; } 
        public string NewValue { get; set; } 
        
        public DateTime Timestamp { get; set; }
        
        [MaxLength(50)]
        public string IPAddress { get; set; }
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
        public string NewPassword { get; set; } 
    }
}
