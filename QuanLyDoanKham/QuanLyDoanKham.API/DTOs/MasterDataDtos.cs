using System;

namespace QuanLyDoanKham.API.DTOs
{
    public class CompanyDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string TaxCode { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class HealthContractDto
    {
        public int HealthContractId { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } // Extra info for display
        public decimal TotalAmount { get; set; }
        public int PatientCount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsFinished { get; set; }
    }

    public class MedicalGroupDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public DateTime ExamDate { get; set; }
        public int HealthContractId { get; set; }
        public string CompanyName { get; set; } // Optional: to show which company this group belongs to
        public bool IsFinished { get; set; }
    }
}
