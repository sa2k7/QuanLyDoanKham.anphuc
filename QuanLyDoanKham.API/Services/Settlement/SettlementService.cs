using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Services.Settlement
{
    public class SettlementService : ISettlementService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICostCalculationService _costCalculationService;

        public SettlementService(ApplicationDbContext context, ICostCalculationService costCalculationService)
        {
            _context = context;
            _costCalculationService = costCalculationService;
        }

        public async Task<SettlementResultDto?> CalculateSettlementAsync(int contractId)
        {
            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.HealthContractId == contractId);

            if (contract == null) return null;

            // 1. Plan figures
            decimal expectedValue = contract.TotalAmount;
            int expectedQuantity = contract.ExpectedQuantity;

            // 2. Actual figures — count MedicalRecords that were truly COMPLETED
            //    (not Patients, which is the master record, not per-exam)
            int actualQuantity = await _context.MedicalRecords
                .Where(r => r.MedicalGroup != null
                         && r.MedicalGroup.HealthContractId == contractId
                         && r.Status == "COMPLETED")
                .CountAsync();

            // Actual revenue = actual exams × unit price from contract
            decimal actualPackageValue = actualQuantity * contract.UnitPrice;

            // 3. Phân tích Chi phí (Cost Analysis)
            CostSnapshot? costSnapshot = await _costCalculationService.GetLatestSnapshotAsync(contractId, "ACT");
            if (costSnapshot == null)
            {
                // Nếu chưa có snapshot ACT, tự động tính on-the-fly
                costSnapshot = await _costCalculationService.CalculateAndSaveSnapshotAsync(contractId, "ACT", "Auto calculated on Settlement load");
            }

            // 5. Dịch vụ ngoài gói
            decimal extraServiceValue = contract.ExtraServiceRevenue;

            // 6. Tổng hợp chi phí & Lợi nhuận
            decimal totalCost = costSnapshot.TotalCost;
            decimal baseRevenue = actualPackageValue + extraServiceValue;
            decimal vatAmount = baseRevenue * (contract.VATRate / 100);
            decimal revenueWithVat = baseRevenue + vatAmount;

            decimal grossProfit = baseRevenue - totalCost; // Profit is usually calculated on Net Revenue
            decimal marginPct = baseRevenue > 0 ? (grossProfit / baseRevenue * 100) : 0;

            return new SettlementResultDto
            {
                ContractId = contract.HealthContractId,
                ContractName = contract.ContractName,
                ContractValue = expectedValue,
                ActualValue = actualPackageValue,
                ExpectedQuantity = expectedQuantity,
                ActualQuantity = actualQuantity,
                ExtraServiceValue = extraServiceValue,
                ExtraServiceDetails = contract.ExtraServiceDetails,
                VATRate = contract.VATRate,
                TotalSettlement = revenueWithVat,
                
                // Mới: Chi tiết Cost Breakdown
                LaborCost = costSnapshot.LaborCost,
                SupplyCost = costSnapshot.SupplyCost,
                OverheadCost = costSnapshot.OverheadCost,
                TotalCost = totalCost,

                // Mới: Phân tích Tài chính (Tính trên Net Revenue)
                Revenue = baseRevenue,
                GrossProfit = grossProfit,
                GrossMarginPercent = marginPct,
                IsMarginWarning = marginPct < 20 // Cảnh báo đỏ nếu biên lợi nhuận < 20%
            };
        }
    }
}
