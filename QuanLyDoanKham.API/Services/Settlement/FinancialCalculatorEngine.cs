using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Services.Settlement
{
    public interface IFinancialCalculatorEngine
    {
        Task<PnlReportDto> CalculatePnlAsync(int contractId);
    }

    public class FinancialCalculatorEngine : IFinancialCalculatorEngine
    {
        private readonly ApplicationDbContext _context;

        public FinancialCalculatorEngine(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PnlReportDto> CalculatePnlAsync(int contractId)
        {
            var contract = await _context.Contracts
                .Include(c => c.Company)
                .FirstOrDefaultAsync(c => c.HealthContractId == contractId);

            if (contract == null) throw new Exception("Contract not found");

            // 1. REVENUE
            // Tổng hợp doanh thu từ hồ sơ có trạng thái COMPLETED
            int completedCount = await _context.MedicalRecords
                .CountAsync(r => r.MedicalGroup != null
                              && r.MedicalGroup.HealthContractId == contractId
                              && r.Status == RecordStatus.Completed);
            
            decimal packageRevenue = completedCount * contract.UnitPrice;
            decimal extraRevenue = contract.ExtraServiceRevenue;
            decimal grossRevenue = packageRevenue + extraRevenue;

            // 2. CONCEPT: DISCOUNT & VAT
            // Chiết khấu được tính trên tổng doanh thu
            decimal discountAmount = grossRevenue * (contract.DiscountPercent / 100m);
            decimal netRevenue = grossRevenue - discountAmount;
            
            // VAT tính trên doanh thu sau chiết khấu
            decimal vatAmount = netRevenue * (contract.VATRate / 100m);
            decimal totalReceivable = netRevenue + vatAmount;

            // 3. COSTS
            var groupIds = await _context.MedicalGroups
                .Where(g => g.HealthContractId == contractId)
                .Select(g => g.GroupId)
                .ToListAsync();

            // Trừ chi phí lương từ GroupStaffDetail
            decimal laborCost = await _context.GroupStaffDetails
                .Where(gd => groupIds.Contains(gd.GroupId))
                .SumAsync(gd => (decimal?)gd.CalculatedSalary) ?? 0;

            // Chi phí vật tư từ StockMovement (OUT)
            decimal supplyCostOut = await _context.Set<StockMovement>()
                .Where(sm => sm.MedicalGroupId.HasValue 
                          && groupIds.Contains(sm.MedicalGroupId.Value) 
                          && sm.MovementType == "OUT")
                .SumAsync(sm => (decimal?)sm.TotalValue) ?? 0;
                
            // Hàng kho trả về (IN) nếu có, sẽ được cấn trừ
            decimal supplyCostIn = await _context.Set<StockMovement>()
                .Where(sm => sm.MedicalGroupId.HasValue 
                          && groupIds.Contains(sm.MedicalGroupId.Value) 
                          && sm.MovementType == "IN")
                .SumAsync(sm => (decimal?)sm.TotalValue) ?? 0;
            
            decimal materialCost = supplyCostOut - supplyCostIn;

            // Chi phí chung khác
            decimal overheadCost = await _context.Set<Overhead>()
                .Where(o => o.HealthContractId == contractId)
                .SumAsync(o => (decimal?)o.Amount) ?? 0;

            decimal totalCost = laborCost + materialCost + overheadCost;

            // 4. PROFIT RATIOS
            decimal grossProfit = netRevenue - totalCost;
            decimal margin = netRevenue > 0 ? (grossProfit / netRevenue) * 100m : 0;
            decimal remainingBalance = totalReceivable - contract.AdvancePayment;

            return new PnlReportDto
            {
                HealthContractId = contract.HealthContractId,
                ContractName = contract.ContractName,
                CompanyName = contract.Company?.CompanyName ?? "Unknown",
                CompletedRecords = completedCount,
                UnitPrice = contract.UnitPrice,
                PackageRevenue = packageRevenue,
                ExtraServiceRevenue = extraRevenue,
                GrossRevenue = grossRevenue,
                DiscountPercent = contract.DiscountPercent,
                DiscountAmount = discountAmount,
                NetRevenue = netRevenue,
                VATRate = contract.VATRate,
                VATAmount = vatAmount,
                LaborCost = laborCost,
                MaterialCost = materialCost,
                OverheadCost = overheadCost,
                TotalCost = totalCost,
                GrossProfit = grossProfit,
                ProfitMargin = margin,
                TotalReceivable = totalReceivable,
                AdvancePayment = contract.AdvancePayment,
                RemainingBalance = remainingBalance
            };
        }
    }
}
