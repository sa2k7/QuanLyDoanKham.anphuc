using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyDoanKham.API.DTOs
{
    // === STAFF & SALARY ===
    public class StaffDto
    {
        public int StaffId { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string FullNameUnsigned { get; set; }
        public int? BirthYear { get; set; }
        public string Gender { get; set; }
        [StringLength(20, MinimumLength = 9, ErrorMessage = "Số CMND/CCCD phải từ 9 đến 12 số")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Số CMND/CCCD chỉ được chứa số")]
        public string IDCardNumber { get; set; }
        public string TaxCode { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankAccountName { get; set; }
        public string BankName { get; set; }
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại không đúng định dạng (10-11 số, bắt đầu bằng 0)")]
        public string PhoneNumber { get; set; }
        public string JobTitle { get; set; }
        public string Department { get; set; }
        public string EmployeeType { get; set; }
        public string IDCardFrontPath { get; set; }
        public string IDCardBackPath { get; set; }
        public string PracticeCertificatePath { get; set; }
        public string AvatarPath { get; set; }
        public decimal BaseSalary { get; set; }
        public bool IsActive { get; set; }
        
        // Trạng thái vận hành hiện tại
        public string CurrentGroupName { get; set; }
        public bool IsWorking { get; set; }

        // Vai trò hệ thống được cấp (Manager, Staff, etc.)
        public string SystemRole { get; set; }
    }

    public class StaffDetailDto : StaffDto
    {
        public List<StaffWorkdayDto> Workdays { get; set; } = new List<StaffWorkdayDto>();
        public List<StaffShiftDto> Shifts { get; set; } = new List<StaffShiftDto>();
    }

    public class StaffWorkdayDto
    {
        public DateTime Date { get; set; }
        public string GroupName { get; set; }
        public string WorkPosition { get; set; }
    }

    public class StaffShiftDto
    {
        public string GroupName { get; set; }
        public double ShiftType { get; set; } // 0.5 or 1.0
        public decimal CalculatedSalary { get; set; }
        public string WorkPosition { get; set; }
    }

    public class AddStaffToGroupDto
    {
        public int GroupId { get; set; }
        public int StaffId { get; set; }
        public double ShiftType { get; set; } // 0.5 (Morning/Afternoon) or 1.0 (Full day)
        public string WorkPosition { get; set; }
        public string WorkStatus { get; set; }
    }

    public class GroupStaffItemDto
    {
        public int Id { get; set; } // Detail ID
        public int StaffId { get; set; }
        public string FullName { get; set; }
        public string JobTitle { get; set; }
        public double ShiftType { get; set; }
        public decimal CalculatedSalary { get; set; } // Kết quả tính "1 củ còn 500k"
        public string WorkPosition { get; set; }
        public string WorkStatus { get; set; }
    }

    // === SUPPLY & WAREHOUSE ===
    public class SupplyDto
    {
        public int SupplyId { get; set; }
        public string SupplyName { get; set; }
        public string Unit { get; set; }
        public bool IsFixedAsset { get; set; }
        public decimal UnitPrice { get; set; }
        public int TotalStock { get; set; }
    }

    public class SupplyInventoryVoucherDto
    {
        public int VoucherId { get; set; }
        public string VoucherCode { get; set; }
        public DateTime CreateDate { get; set; }
        public string Type { get; set; } // IMPORT, EXPORT
        public int? GroupId { get; set; }
        public string GroupName { get; set; }
        public string CreatedBy { get; set; }
        public string Note { get; set; }
        public List<SupplyInventoryDetailDto> Details { get; set; } = new List<SupplyInventoryDetailDto>();
    }

    public class SupplyInventoryDetailDto
    {
        public int Id { get; set; }
        public int SupplyId { get; set; }
        public string SupplyName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public class CreateVoucherDto
    {
        public string Type { get; set; }
        public int? GroupId { get; set; }
        public string Note { get; set; }
        public List<CreateVoucherDetailDto> Details { get; set; }
    }

    public class CreateVoucherDetailDto
    {
        public int SupplyId { get; set; }
        public int Quantity { get; set; }
    }
}
