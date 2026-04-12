using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Services.Settlement
{
    public class CostCalculationService : ICostCalculationService
    {
        private readonly ApplicationDbContext _context;

        public CostCalculationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CostSnapshot?> GetLatestSnapshotAsync(int contractId, string snapshotType)
        {
            var snap = await _context.Set<CostSnapshot>()
                .Where(s => s.HealthContractId == contractId && s.SnapshotType == snapshotType)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();
            return snap;
        }

        public async Task<CostSnapshot> CalculateAndSaveSnapshotAsync(int contractId, string snapshotType, string? note = null, int? userId = null)
        {
            var contract = await _context.Contracts.FindAsync(contractId);
            if (contract == null) throw new Exception("Contract not found");

            // Doanh thu = Số hồ sơ COMPLETED qua HợpĐồng × ĐơnGiá
            // Nhất quán với SettlementService.CalculateSettlementAsync()
            int actualQuantity = await _context.MedicalRecords
                .CountAsync(r => r.MedicalGroup != null
                              && r.MedicalGroup.HealthContractId == contractId
                              && r.Status == "COMPLETED");
            decimal revenue = actualQuantity * contract.UnitPrice;

            // Health groups involved in this contract
            var groups = await _context.MedicalGroups
                .Where(g => g.HealthContractId == contractId)
                .Select(g => g.GroupId)
                .ToListAsync();

            // 2. Labor Cost
            decimal laborCost = await _context.GroupStaffDetails
                .Where(gd => groups.Contains(gd.GroupId))
                .SumAsync(gd => (decimal?)gd.CalculatedSalary) ?? 0;

            // 3. Supply Cost (MaterialCost) from StockMovements
            decimal supplyCostOut = await _context.Set<StockMovement>()
                .Where(sm => sm.MedicalGroupId.HasValue && groups.Contains(sm.MedicalGroupId.Value) && sm.MovementType == "OUT")
                .SumAsync(sm => (decimal?)sm.TotalValue) ?? 0;
            
            decimal supplyCostIn = await _context.Set<StockMovement>()
                .Where(sm => sm.MedicalGroupId.HasValue && groups.Contains(sm.MedicalGroupId.Value) && sm.MovementType == "IN")
                .SumAsync(sm => (decimal?)sm.TotalValue) ?? 0;

            decimal supplyCost = supplyCostOut - supplyCostIn;

            // 4. Overhead Cost from Overheads table
            decimal overheadCost = await _context.Set<Overhead>()
                .Where(o => o.HealthContractId == contractId)
                .SumAsync(o => (decimal?)o.Amount) ?? 0;

            // Calculate Totals
            decimal totalCost = laborCost + supplyCost + overheadCost;
            decimal grossProfit = revenue - totalCost;

            // Create Snapshot
            var snapshot = new CostSnapshot
            {
                HealthContractId = contractId,
                SnapshotType = snapshotType,
                Revenue = revenue,
                LaborCost = laborCost,
                SupplyCost = supplyCost,
                OverheadCost = overheadCost,
                TotalCost = totalCost,
                GrossProfit = grossProfit,
                Note = note ?? string.Empty,
                CreatedByUserId = userId,
                CreatedAt = DateTime.Now
            };

            _context.Set<CostSnapshot>().Add(snapshot);
            await _context.SaveChangesAsync();

            return snapshot;
        }
    }
}
