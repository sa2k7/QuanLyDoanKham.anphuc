using System;
using System.Collections.Generic;

namespace QuanLyDoanKham.API.DTOs
{
    // Cấu trúc cho Dashboard KPIs (CEO View)
    public class DashboardKpiDto
    {
        public int TotalPatients { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal NetProfit { get; set; }
        public double CompletionRate { get; set; }
        public int ActiveGroupsCount { get; set; }
        public int CriticalAlertsCount { get; set; }
        public List<ChartPointDto> RevenueTrend { get; set; }
    }

    // Cấu trúc cho Báo cáo Tài chính
    public class FinancialReportDto
    {
        public decimal Revenue { get; set; }
        public decimal StaffCost { get; set; }
        public decimal SupplyCost { get; set; }
        public decimal OtherCost { get; set; }
        public decimal Margin { get; set; }
        public List<ContractRevenueDto> TopContracts { get; set; }
    }

    // Cấu trúc cho Hiệu suất Nhân sự (HR View)
    public class StaffEfficiencyDto
    {
        public string StaffName { get; set; }
        public string Role { get; set; }
        public int TotalGroups { get; set; }
        public int DaysWorked { get; set; }
        public decimal TotalSalary { get; set; }
        public double EfficiencyScore { get; set; } // Lượt khám / ngày
    }

    // Cấu trúc cho Cảnh báo Kho (Warehouse View)
    public class InventoryAlertDto
    {
        public int SupplyId { get; set; }
        public string SupplyName { get; set; }
        public int CurrentStock { get; set; }
        public int MinStockLevel { get; set; }
        public string Status { get; set; } // Low, Critical, OutOfStock
        public int DaysUntilExpired { get; set; }
    }

    public class ChartPointDto
    {
        public string Label { get; set; }
        public decimal Value { get; set; }
    }

    public class ContractRevenueDto
    {
        public string CompanyName { get; set; }
        public decimal Amount { get; set; }
        public int Quantity { get; set; }
    }
}
