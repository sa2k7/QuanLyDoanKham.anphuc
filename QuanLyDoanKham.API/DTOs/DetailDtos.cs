using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyDoanKham.API.DTOs
{
    // === STAFF & SALARY ===
    public class StaffDto
    {
        public int StaffId { get; set; }
        public string? EmployeeCode { get; set; }
        public required string FullName { get; set; }
        public string? FullNameUnsigned { get; set; }
        public int? BirthYear { get; set; }
        public string? Gender { get; set; }
        [StringLength(20, MinimumLength = 9, ErrorMessage = "Số CMND/CCCD phải từ 9 đến 12 số")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Số CMND/CCCD chỉ được chứa số")]
        public string? IDCardNumber { get; set; }
        public string? TaxCode { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankAccountName { get; set; }
        public string? BankName { get; set; }
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại không đúng định dạng (10-11 số, bắt đầu bằng 0)")]
        public string? PhoneNumber { get; set; }
        
        [EmailAddress(ErrorMessage = "Địa chỉ Email không đúng định dạng")]
        public string? Email { get; set; }

        public string? JobTitle { get; set; }
        public string? StaffType { get; set; } // BacSi, DieuDuong, KyThuatVien, NhanVienHoTro
        public string? Specialty { get; set; } // Chuyen khoa
        public decimal DailyRate { get; set; } // Don gia thu lao ca ngay
        public string? Department { get; set; }
        public string? EmployeeType { get; set; }
        public string? IDCardFrontPath { get; set; }
        public string? IDCardBackPath { get; set; }
        public string? PracticeCertificatePath { get; set; }
        public string? AvatarPath { get; set; }
        public decimal BaseSalary { get; set; }
        public bool IsActive { get; set; }
        
        // Trang thai van hanh
        public string? CurrentGroupName { get; set; }
        public bool IsWorking { get; set; }
        public string? SystemRole { get; set; }
    }

    public class StaffDetailDto : StaffDto
    {
        public List<StaffWorkdayDto> Workdays { get; set; } = new List<StaffWorkdayDto>();
        public List<StaffShiftDto> Shifts { get; set; } = new List<StaffShiftDto>();
    }

    public class StaffWorkdayDto
    {
        public DateTime Date { get; set; }
        public string? GroupName { get; set; }
        public string? WorkPosition { get; set; }
    }

    public class StaffShiftDto
    {
        public string? GroupName { get; set; }
        public double ShiftType { get; set; } // 0.5 or 1.0
        public decimal CalculatedSalary { get; set; }
        public string? WorkPosition { get; set; }
    }

    public class AddStaffToGroupDto
    {
        public int GroupId { get; set; }
        public int StaffId { get; set; }
        public double ShiftType { get; set; } // 0.5 (Morning/Afternoon) or 1.0 (Full day)
        public required string WorkPosition { get; set; }
        public string? WorkStatus { get; set; }
        public string? PickupLocation { get; set; }
        public int? PositionId { get; set; }
    }

    public class GroupStaffItemDto
    {
        public int Id { get; set; } // Detail ID
        public int StaffId { get; set; }
        public string? EmployeeCode { get; set; }
        public required string FullName { get; set; }
        public string? JobTitle { get; set; }
        public string? StaffType { get; set; } // BacSi, DieuDuong...
        public double ShiftType { get; set; }
        public decimal CalculatedSalary { get; set; }
        public string? WorkPosition { get; set; }
        public int? PositionId { get; set; }
        public string? WorkStatus { get; set; }
        public DateTime? CheckInTime { get; set; } // Gio check-in thuc te
        public DateTime? CheckOutTime { get; set; } // Gio check-out thuc te
        public string? Note { get; set; }
        public string? PickupLocation { get; set; }

        // ThĂ´ng tin thĂªm cho bĂ¡o cĂ¡o Excel
        public int? BirthYear { get; set; }
        public string? PhoneNumber { get; set; }
        public string? EmployeeType { get; set; } // NoiBo, ThueNgoai
    }

    // === DIEM DANH / CHECK-IN ===
    public class UpdateWorkStatusDto
    {
        public required string WorkStatus { get; set; } // "Đang chờ" / "Đã tham gia" / "Vắng mặt" / "Xin nghỉ"
        public string? Note { get; set; } // Ghi chu ly do nghi, v.v.
    }

    // === KẾT QUẢ KHÁM (EXAMINATION RESULTS) ===
    public class SaveExamResultDto
    {
        public int MedicalRecordId { get; set; }
        public string StationCode { get; set; } = null!;
        public string ExamType { get; set; } = null!;
        public System.Text.Json.JsonElement ResultData { get; set; } // JSON Object
        public string Diagnosis { get; set; } = null!;
        public int? DoctorStaffId { get; set; }
    }

    public class ExamResultResponseDto
    {
        public int ExamResultId { get; set; }
        public string? ExamType { get; set; }
        public object? ResultData { get; set; } // JSON Object returns as object for automatic serialization
        public string? Diagnosis { get; set; }
        public DateTime ExamDate { get; set; }
        public string? DoctorName { get; set; }
    }
}
