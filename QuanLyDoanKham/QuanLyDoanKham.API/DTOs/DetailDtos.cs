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
    }

    public class StaffShiftDto
    {
        public string GroupName { get; set; }
        public double ShiftType { get; set; } // 0.5 or 1.0
        public decimal CalculatedSalary { get; set; }
    }

    public class AddStaffToGroupDto
    {
        public int GroupId { get; set; }
        public int StaffId { get; set; }
        public double ShiftType { get; set; } // 0.5 (Morning/Afternoon) or 1.0 (Full day)
    }

    public class GroupStaffItemDto
    {
        public int Id { get; set; } // Detail ID
        public int StaffId { get; set; }
        public string FullName { get; set; }
        public string JobTitle { get; set; }
        public double ShiftType { get; set; }
        public decimal CalculatedSalary { get; set; } // Kết quả tính "1 củ còn 500k"
    }

    // === SUPPLY & WAREHOUSE ===
    public class SupplyDto
    {
        public int SupplyId { get; set; }
        public string SupplyName { get; set; }
        public bool IsFixedAsset { get; set; }
        public decimal UnitPrice { get; set; }
        public int StockQuantity { get; set; }
    }

    public class AddSupplyToGroupDto
    {
        public int GroupId { get; set; }
        public int SupplyId { get; set; }
        public int QuantityUsed { get; set; }
    }

    public class ReturnSupplyDto
    {
        public int ReturnQuantity { get; set; }
    }

    public class ImportStockDto
    {
        public int Quantity { get; set; }
        public string Note { get; set; }
    }

    public class SupplyTransactionDto
    {
        public int TransactionId { get; set; }
        public int SupplyId { get; set; }
        public string SupplyName { get; set; }
        public int Quantity { get; set; }
        public string Type { get; set; }
        public string Note { get; set; }
        public DateTime TransactionDate { get; set; }
        public string ProcessedBy { get; set; }
    }

    public class GroupSupplyItemDto
    {
        public int Id { get; set; }
        public int SupplyId { get; set; }
        public string SupplyName { get; set; }
        public int QuantityUsed { get; set; }
        public int ReturnQuantity { get; set; }
        public decimal TotalCost { get; set; } // (Used - Return) * Price
    }
}
