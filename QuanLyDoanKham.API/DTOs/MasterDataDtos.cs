using System;

namespace QuanLyDoanKham.API.DTOs
{
    public class CompanyDto
    {
        public int CompanyId { get; set; }
        public string ShortName { get; set; }
        public string CompanyName { get; set; }
        public string TaxCode { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class HealthContractDto
    {
        public int HealthContractId { get; set; }
        public int CompanyId { get; set; }
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
        public string FilePath { get; set; }
        public List<ContractStatusHistoryDto> StatusHistories { get; set; }
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

    public class MedicalGroupDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public DateTime ExamDate { get; set; }
        public int HealthContractId { get; set; }
        public string ShortName { get; set; }
        public string CompanyName { get; set; }
        public string Status { get; set; }
        public string ImportFilePath { get; set; }
    }

    public class StatusUpdateDto
    {
        public string Status { get; set; }
        public string Note { get; set; }
    }
}
