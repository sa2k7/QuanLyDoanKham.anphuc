using System;
using System.Collections.Generic;

namespace QuanLyDoanKham.API.DTOs
{
    public class PayrollRecordDto
    {
        public int PayrollId { get; set; }
        public int StaffId { get; set; }
        public string FullName { get; set; }
        public string EmployeeCode { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalSalary { get; set; }  // alias
        public double TotalActualDays { get; set; }
        public string Status { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankName { get; set; }
        public string BankAccountName { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string GeneratedBy { get; set; }
    }

    public class PayrollSummaryDto
    {
        public int StaffId { get; set; }
        public string FullName { get; set; }
        public string EmployeeCode { get; set; }
        public string JobTitle { get; set; }
        public string DepartmentName { get; set; }

        // Ngân hàng
        public string BankAccountNumber { get; set; }
        public string BankName { get; set; }
        public string BankAccountName { get; set; }

        // Công thức lương
        public decimal BaseSalary { get; set; }
        public string SalaryType { get; set; } = "Monthly";   // Monthly | Daily
        public int StandardWorkDays { get; set; } = 26;
        public double TotalActualDays { get; set; }            // công thực tế (0.5 | 1.0)

        // Tổng tiền
        public decimal GroupEarnings { get; set; }             // = calculated total
        public decimal TotalSalary { get; set; }               // net pay

        // Chi tiết từng buổi/đoàn
        public List<PayrollDetailDto> Details { get; set; } = new();
    }

    public class PayrollDetailDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public DateTime ExamDate { get; set; }
        public double ShiftType { get; set; }          // 0.5 | 1.0
        public decimal CalculatedSalary { get; set; }
    }

    /// <summary>Request body cho tính lương thủ công</summary>
    public class GeneratePayrollDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public bool Overwrite { get; set; } = false;
    }
}
