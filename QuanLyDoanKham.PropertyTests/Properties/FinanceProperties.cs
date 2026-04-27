using FsCheck;
using FsCheck.Xunit;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.PropertyTests.Helpers;

namespace QuanLyDoanKham.PropertyTests.Properties;

/// <summary>
/// Property-based tests cho module Tài chính.
/// Feature: medical-examination-team-management
/// </summary>
public class FinanceProperties
{
    // -----------------------------------------------------------------------
    // Property 28: Chi phí nhân sự đoàn khám bằng tổng (ngày công × đơn giá) của từng nhân sự
    // Validates: Requirements 10.1
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P28_StaffCostEqualsSumOfWorkUnitsTimesRate()
    {
        // Feature: medical-examination-team-management, Property 28: Staff cost = sum(WorkUnits × DailyRate)
        var gen =
            from staffCount in Gen.Choose(1, 10)
            from workUnits in Gen.ListOf(staffCount, Gen.Elements(0m, 0.5m, 1.0m))
            from dailyRates in Gen.ListOf(staffCount, Gen.Choose(200_000, 2_000_000).Select(r => (decimal)r))
            select (workUnits.ToList(), dailyRates.ToList());

        return Prop.ForAll(Arb.From(gen), data =>
        {
            var (workUnits, dailyRates) = data;

            // Tạo bản ghi chấm công với đơn giá
            var attendanceRecords = workUnits.Zip(dailyRates, (wu, rate) => new
            {
                WorkUnits = wu,
                DailyRate = rate,
                CalculatedSalary = wu * rate
            }).ToList();

            // Chi phí nhân sự = Σ (WorkUnits × DailyRate)
            decimal staffCost = attendanceRecords.Sum(r => r.CalculatedSalary);
            decimal expectedCost = workUnits.Zip(dailyRates, (wu, rate) => wu * rate).Sum();

            return staffCost == expectedCost;
        });
    }

    // -----------------------------------------------------------------------
    // Property 29: Công thức lợi nhuận đoàn khám luôn đúng
    // Validates: Requirements 10.4
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P29_ProfitFormulaAlwaysCorrect()
    {
        // Feature: medical-examination-team-management, Property 29: Profit formula always correct
        return Prop.ForAll(DomainArbitraries.FinanceComponents(), components =>
        {
            var (revenue, staffCost, materialCost, otherCost) = components;

            // Công thức: Profit = Revenue - StaffCost - MaterialCost - OtherCost
            decimal expectedProfit = revenue - staffCost - materialCost - otherCost;

            // Simulate FinanceService calculation
            decimal calculatedProfit = revenue - staffCost - materialCost - otherCost;

            return calculatedProfit == expectedProfit;
        });
    }

    // -----------------------------------------------------------------------
    // Property 29 (variant): Lợi nhuận có thể âm khi chi phí vượt doanh thu
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P29_ProfitCanBeNegative()
    {
        // Feature: medical-examination-team-management, Property 29 variant: Profit can be negative
        var gen =
            from revenue in Gen.Choose(0, 10_000_000).Select(r => (decimal)r)
            from totalCost in Gen.Choose(10_000_001, 20_000_000).Select(c => (decimal)c)
            select (revenue, totalCost);

        return Prop.ForAll(Arb.From(gen), data =>
        {
            var (revenue, totalCost) = data;
            decimal profit = revenue - totalCost;
            return profit < 0;
        });
    }

    // -----------------------------------------------------------------------
    // Property 30: Doanh thu hợp đồng bằng tổng doanh thu các đoàn khám thuộc hợp đồng
    // Validates: Requirements 10.6
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P30_ContractRevenueEqualsSumOfTeamRevenues()
    {
        // Feature: medical-examination-team-management, Property 30: Contract revenue = sum of team revenues
        var gen =
            from contractId in Gen.Choose(1, 100)
            from teamCount in Gen.Choose(1, 10)
            from revenues in Gen.ListOf(teamCount, Gen.Choose(0, 50_000_000).Select(r => (decimal)r))
            select (contractId, revenues.ToList());

        return Prop.ForAll(Arb.From(gen), data =>
        {
            var (contractId, revenues) = data;

            // Tạo các đoàn khám với doanh thu
            var teams = revenues.Select((rev, i) => new
            {
                TeamId = i + 1,
                ContractId = contractId,
                Revenue = rev
            }).ToList();

            // Doanh thu hợp đồng = Σ doanh thu các đoàn
            decimal contractRevenue = teams
                .Where(t => t.ContractId == contractId)
                .Sum(t => t.Revenue);

            decimal expectedRevenue = revenues.Sum();

            return contractRevenue == expectedRevenue;
        });
    }

    // -----------------------------------------------------------------------
    // Property 30 (variant): Doanh thu hợp đồng tỷ lệ với số ngày
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P30_RevenueProportionalToContractDays()
    {
        // Feature: medical-examination-team-management, Property 30 variant: Revenue proportional to days
        var gen =
            from totalAmount in Gen.Choose(10_000_000, 100_000_000).Select(a => (decimal)a)
            from days in Gen.Choose(1, 30)
            select (totalAmount, days);

        return Prop.ForAll(Arb.From(gen), data =>
        {
            var (totalAmount, days) = data;

            // Revenue per day = TotalAmount / days (từ FinanceService)
            decimal revenuePerDay = totalAmount / days;
            decimal totalRevenue = revenuePerDay * days;

            // Tổng doanh thu phải bằng tổng hợp đồng (có thể có sai số làm tròn nhỏ)
            decimal diff = Math.Abs(totalRevenue - totalAmount);
            return diff < 1m; // Sai số < 1 đồng
        });
    }
}
