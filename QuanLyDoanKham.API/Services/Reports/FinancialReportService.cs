using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Services.Reports;

public class FinancialReportService
{
    private readonly ApplicationDbContext _db;
    public FinancialReportService(ApplicationDbContext db) => _db = db;

    public async Task<ContractFinancialSummary> BuildContractSummaryAsync(int contractId)
    {
        var staffCost = await _db.GroupStaffDetails
            .Include(g => g.MedicalGroup)
            .Include(g => g.Staff)
            .Where(g => g.MedicalGroup.HealthContractId == contractId)
            .SumAsync(g =>
                (decimal)(g.ShiftType <= 0 ? 1 : g.ShiftType) *
                (g.Staff != null ? g.Staff.BaseSalary : 0) /
                (g.Staff != null && g.Staff.StandardWorkDays > 0 ? g.Staff.StandardWorkDays : 26));

        var supplyCost = await _db.SupplyInventoryDetails
            .Include(d => d.Voucher)
            .ThenInclude(v => v.MedicalGroup)
            .Where(d => d.Voucher.Type == "EXPORT" && d.Voucher.GroupId != null &&
                        d.Voucher.MedicalGroup.HealthContractId == contractId)
            .SumAsync(d => d.Price * d.Quantity);

        var revenue = await _db.Contracts
            .Where(c => c.HealthContractId == contractId)
            .Select(c => c.TotalAmount)
            .FirstOrDefaultAsync();

        var summary = new ContractFinancialSummary
        {
            HealthContractId = contractId,
            StaffCost = staffCost,
            SupplyCost = supplyCost,
            OtherCost = 0,
            Revenue = revenue,
            SnapshotAt = DateTime.UtcNow
        };

        _db.ContractFinancialSummaries.Add(summary);
        await _db.SaveChangesAsync();
        return summary;
    }
}
