namespace QuanLyDoanKham.API.Services.Finance;

public interface IFinanceService
{
    /// <summary>Tính chi phí và doanh thu cho một đoàn khám</summary>
    Task<TeamFinanceDto> CalculateTeamFinanceAsync(int teamId);

    /// <summary>Tổng hợp tài chính cho một hợp đồng</summary>
    Task<ContractFinanceSummaryDto> GetContractSummaryAsync(int contractId);

    /// <summary>Thêm chi phí phát sinh cho đoàn khám</summary>
    Task<OtherExpenseDto> AddOtherExpenseAsync(int teamId, CreateExpenseRequest request);
}

public class TeamFinanceDto
{
    public int TeamId { get; set; }
    public string TeamName { get; set; } = null!;
    public decimal Revenue { get; set; }
    public decimal StaffCost { get; set; }
    public decimal MaterialCost { get; set; }
    public decimal OtherCost { get; set; }
    public decimal Profit { get; set; }
    public List<OtherExpenseDto> OtherExpenses { get; set; } = new();
}

public class ContractFinanceSummaryDto
{
    public int ContractId { get; set; }
    public string ContractCode { get; set; } = null!;
    public decimal TotalRevenue { get; set; }
    public decimal TotalStaffCost { get; set; }
    public decimal TotalMaterialCost { get; set; }
    public decimal TotalOtherCost { get; set; }
    public decimal TotalProfit { get; set; }
    public List<TeamFinanceDto> Teams { get; set; } = new();
}

public class OtherExpenseDto
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public string Description { get; set; } = null!;
    public decimal Amount { get; set; }
    public string? Category { get; set; }
}

public class CreateExpenseRequest
{
    public string Description { get; set; } = null!;
    public decimal Amount { get; set; }
    public string? Category { get; set; }
}
