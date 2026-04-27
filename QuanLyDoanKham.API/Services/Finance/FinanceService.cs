using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Services.Finance;

public class FinanceService : IFinanceService
{
    private readonly ApplicationDbContext _context;

    public FinanceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TeamFinanceDto> CalculateTeamFinanceAsync(int teamId)
    {
        var group = await _context.MedicalGroups
            .Include(g => g.HealthContract)
            .FirstOrDefaultAsync(g => g.GroupId == teamId);

        if (group == null) throw new InvalidOperationException($"Team {teamId} not found");

        // Revenue = TotalAmount / number of days in contract (proportional per day)
        decimal revenue = 0;
        if (group.HealthContract != null)
        {
            var contractDays = Math.Max(1, (group.HealthContract.EndDate - group.HealthContract.StartDate).Days + 1);
            revenue = group.HealthContract.TotalAmount / contractDays;
        }

        // StaffCost = Σ CalculatedSalary per staff in this group
        var staffCost = await _context.GroupStaffDetails
            .Where(gsd => gsd.GroupId == teamId)
            .SumAsync(gsd => (decimal?)gsd.CalculatedSalary) ?? 0;

        // MaterialCost = Σ stock movements OUT linked to this group
        var materialCost = await _context.StockMovements
            .Where(sm => sm.MedicalGroupId == teamId && sm.MovementType == "OUT")
            .SumAsync(sm => (decimal?)sm.TotalValue) ?? 0;

        // OtherCost = Σ Overhead linked to this group's contract
        var otherCost = await _context.Overheads
            .Where(o => o.HealthContractId == group.HealthContractId)
            .SumAsync(o => (decimal?)o.Amount) ?? 0;

        // Profit = Revenue - StaffCost - MaterialCost - OtherCost
        var profit = revenue - staffCost - materialCost - otherCost;

        // Other expenses (Overhead items for this contract)
        var overheads = await _context.Overheads
            .Where(o => o.HealthContractId == group.HealthContractId)
            .Select(o => new OtherExpenseDto
            {
                Id = o.OverheadId,
                TeamId = teamId,
                Description = o.Description ?? o.Name,
                Amount = o.Amount,
                Category = o.Name
            })
            .ToListAsync();

        return new TeamFinanceDto
        {
            TeamId = teamId,
            TeamName = group.GroupName,
            Revenue = revenue,
            StaffCost = staffCost,
            MaterialCost = materialCost,
            OtherCost = otherCost,
            Profit = profit,
            OtherExpenses = overheads
        };
    }

    public async Task<ContractFinanceSummaryDto> GetContractSummaryAsync(int contractId)
    {
        var contract = await _context.Contracts.FindAsync(contractId);
        if (contract == null) throw new InvalidOperationException($"Contract {contractId} not found");

        var groupIds = await _context.MedicalGroups
            .Where(g => g.HealthContractId == contractId)
            .Select(g => g.GroupId)
            .ToListAsync();

        var teams = new List<TeamFinanceDto>();
        foreach (var gid in groupIds)
        {
            try { teams.Add(await CalculateTeamFinanceAsync(gid)); }
            catch { /* skip groups with data issues */ }
        }

        return new ContractFinanceSummaryDto
        {
            ContractId = contractId,
            ContractCode = contract.ContractCode ?? contract.ContractName,
            TotalRevenue = teams.Sum(t => t.Revenue),
            TotalStaffCost = teams.Sum(t => t.StaffCost),
            TotalMaterialCost = teams.Sum(t => t.MaterialCost),
            TotalOtherCost = teams.Sum(t => t.OtherCost),
            TotalProfit = teams.Sum(t => t.Profit),
            Teams = teams
        };
    }

    public async Task<OtherExpenseDto> AddOtherExpenseAsync(int teamId, CreateExpenseRequest request)
    {
        var group = await _context.MedicalGroups.FindAsync(teamId);
        if (group == null) throw new InvalidOperationException($"Team {teamId} not found");

        var overhead = new Overhead
        {
            HealthContractId = group.HealthContractId,
            Name = request.Category ?? "Chi phí phát sinh",
            Description = request.Description,
            Amount = request.Amount,
            IncurredAt = DateTime.Now
        };

        _context.Overheads.Add(overhead);
        await _context.SaveChangesAsync();

        return new OtherExpenseDto
        {
            Id = overhead.OverheadId,
            TeamId = teamId,
            Description = overhead.Description,
            Amount = overhead.Amount,
            Category = overhead.Name
        };
    }
}
