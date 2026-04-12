using System;
using System.Collections.Generic;

namespace QuanLyDoanKham.API.DTOs
{
    // Cấu trúc cho Dashboard KPIs (CEO View)
    public class DashboardKpiDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal NetProfit { get; set; }
        public double CompletionRate { get; set; }
        public int ActiveGroupsCount { get; set; }
        public double HrPerformance { get; set; } // Percentage of staff utilization or efficiency
        public decimal MaterialDeviation { get; set; } // Variance between expected and actual material costs
        public List<ChartPointDto> RevenueTrend { get; set; } = new();
    }

    // Cấu trúc cho Báo cáo Tài chính
    public class FinancialReportDto
    {
        /// <summary>Doanh thu thực tế từ các ca khám đã COMPLETED</summary>
        public decimal Revenue { get; set; }
        /// <summary>Doanh thu kế hoạch (từ TotalAmount hợp đồng)</summary>
        public decimal PlannedRevenue { get; set; }
        /// <summary>Số ca đã khám thực tế (COMPLETED)</summary>
        public int ActualQuantity { get; set; }
        /// <summary>Số ca kế hoạch (ExpectedQuantity hợp đồng)</summary>
        public int PlannedQuantity { get; set; }
        /// <summary>Chênh lệch số lượng (thực - kế hoạch)</summary>
        public int VarianceQuantity { get; set; }
        /// <summary>Chênh lệch doanh thu (thực - kế hoạch)</summary>
        public decimal VarianceRevenue { get; set; }
        public decimal StaffCost { get; set; }
        public decimal OtherCost { get; set; }
        public decimal Margin { get; set; }
        public List<ContractRevenueDto> TopContracts { get; set; } = new();
    }

    // Cấu trúc cho Hiệu suất Nhân sự (HR View)
    public class StaffEfficiencyDto
    {
        public string? StaffName { get; set; }
        public string? Role { get; set; }
        public int TotalGroups { get; set; }
        public int DaysWorked { get; set; }
        public decimal TotalSalary { get; set; }
    }

    // Cấu trúc cho Báo cáo Vận hành
    public class OperationalSummaryDto
    {
        public int TotalMedicalGroupsThisMonth { get; set; }
        public int TotalPatientsThisMonth { get; set; }
        public int TotalStaffDeployedThisMonth { get; set; }
        public int PendingContractsCount { get; set; }
    }

    // Cấu trúc cho Bảng lương tổng hợp
    public class StaffPayrollSummaryDto
    {
        public int StaffId { get; set; }
        public string? StaffName { get; set; }
        public string? JobTitle { get; set; }
        public int TotalShifts { get; set; }
        public int TotalDays { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal TotalSalary { get; set; }
    }

    public class ChartPointDto
    {
        public required string Label { get; set; }
        public decimal Value { get; set; }
    }

    public class ContractRevenueDto
    {
        public string? CompanyName { get; set; }
        public decimal Amount { get; set; }
        public int Quantity { get; set; }
    }
}
