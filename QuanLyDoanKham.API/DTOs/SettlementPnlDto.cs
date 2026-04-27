using System;

namespace QuanLyDoanKham.API.DTOs
{
    /// <summary>
    /// Data Transfer Object cho Báo cáo Tối ưu hóa Lợi nhuận (P&amp;L - Profit and Loss).
    /// Tổng hợp doanh thu thực tế, các khoản khấu trừ, VAT và các loại chi phí (Lương, Vật tư)
    /// </summary>
    public class PnlReportDto
    {
        public int HealthContractId { get; set; }
        public string ContractName { get; set; } = null!;
        public string CompanyName { get; set; } = null!;

        // --- 1. REVENUE BREAKDOWN ---
        public int CompletedRecords { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal PackageRevenue { get; set; }       // CompletedRecords * UnitPrice
        public decimal ExtraServiceRevenue { get; set; }
        public decimal GrossRevenue { get; set; }         // PackageRevenue + ExtraServiceRevenue
        
        // --- 2. DEDUCTIONS & TAXES ---
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }       // GrossRevenue * (DiscountPercent / 100)
        
        public decimal NetRevenue { get; set; }           // GrossRevenue - DiscountAmount

        public decimal VATRate { get; set; }
        public decimal VATAmount { get; set; }            // NetRevenue * (VATRate / 100)

        // --- 3. COST BREAKDOWN ---
        public decimal LaborCost { get; set; }           // Tổng CalculatedSalary từ GroupStaffDetail
        public decimal MaterialCost { get; set; }        // StockMovement OUT - IN
        public decimal OverheadCost { get; set; }        // Chi phí phát sinh từ Overheads
        public decimal TotalCost { get; set; }           // LaborCost + MaterialCost + OverheadCost

        // --- 4. PROFITABILITY ---
        public decimal GrossProfit { get; set; }         // NetRevenue - TotalCost
        public decimal ProfitMargin { get; set; }        // (GrossProfit / NetRevenue) * 100
        
        // --- 5. SETTLEMENT BALANCE ---
        public decimal TotalReceivable { get; set; }     // NetRevenue + VATAmount
        public decimal AdvancePayment { get; set; }
        public decimal RemainingBalance { get; set; }    // TotalReceivable - AdvancePayment
    }
}
