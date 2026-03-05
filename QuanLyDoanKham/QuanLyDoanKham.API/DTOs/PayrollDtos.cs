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
        public decimal TotalSalary { get; set; }
        public double TotalShifts { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankName { get; set; }
        public string BankAccountName { get; set; }
        public DateTime GeneratedDate { get; set; }
    }

    public class PayrollSummaryDto
    {
        public int StaffId { get; set; }
        public string FullName { get; set; }
        public string EmployeeCode { get; set; }
        public string JobTitle { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankName { get; set; }
        public string BankAccountName { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal GroupEarnings { get; set; }
        public decimal TotalSalary { get; set; }
        public double TotalShifts { get; set; }
        public List<PayrollDetailDto> Details { get; set; } = new List<PayrollDetailDto>();
    }

    public class PayrollDetailDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public DateTime ExamDate { get; set; }
        public double ShiftType { get; set; }
        public decimal CalculatedSalary { get; set; }
    }
}
