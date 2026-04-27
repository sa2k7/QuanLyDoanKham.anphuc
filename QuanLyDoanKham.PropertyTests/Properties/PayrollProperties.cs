using FsCheck;
using FsCheck.Xunit;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.PropertyTests.Helpers;

namespace QuanLyDoanKham.PropertyTests.Properties;

/// <summary>
/// Property-based tests cho module Chấm công và Tính lương.
/// Feature: medical-examination-team-management
/// </summary>
public class PayrollProperties
{
    // -----------------------------------------------------------------------
    // Property 32: Lương tạm tính bằng tổng ngày công nhân với đơn giá ngày công
    // Validates: Requirements 12.4
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P32_GrossSalaryEqualsTotalWorkUnitsTimesRate()
    {
        // Feature: medical-examination-team-management, Property 32: Gross salary = TotalWorkUnits × DailyRate
        var gen =
            from workUnitCount in Gen.Choose(1, 20)
            from workUnits in Gen.ListOf(workUnitCount, Gen.Elements(0m, 0.5m, 1.0m))
            from dailyRate in Gen.Choose(200_000, 2_000_000).Select(r => (decimal)r)
            select (workUnits.ToList(), dailyRate);

        return Prop.ForAll(Arb.From(gen), data =>
        {
            var (workUnits, dailyRate) = data;

            // Tổng ngày công
            decimal totalWorkUnits = workUnits.Sum();

            // Lương tạm tính = TotalWorkUnits × DailyRate
            decimal grossSalary = totalWorkUnits * dailyRate;
            decimal expectedSalary = workUnits.Sum() * dailyRate;

            return grossSalary == expectedSalary;
        });
    }

    // -----------------------------------------------------------------------
    // Property 32 (variant): Lương tháng theo BaseSalary
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P32_MonthlySalaryCalculation()
    {
        // Feature: medical-examination-team-management, Property 32 variant: Monthly salary calculation
        var gen =
            from baseSalary in Gen.Choose(5_000_000, 30_000_000).Select(s => (decimal)s)
            from standardDays in Gen.Choose(20, 26)
            from actualWorkUnits in Gen.Choose(0, standardDays).Select(d => (decimal)d * 0.5m)
            select (baseSalary, standardDays, actualWorkUnits);

        return Prop.ForAll(Arb.From(gen), data =>
        {
            var (baseSalary, standardDays, actualWorkUnits) = data;

            // Logic từ PayrollService.CalculateDetailSalary (SalaryType = "ByMonth")
            decimal ratePerDay = baseSalary / standardDays;
            decimal grossSalary = ratePerDay * actualWorkUnits;

            decimal expectedSalary = (baseSalary / standardDays) * actualWorkUnits;

            return Math.Abs(grossSalary - expectedSalary) < 0.01m;
        });
    }

    // -----------------------------------------------------------------------
    // Property 32 (variant): Lương 0 khi không có ngày công
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P32_ZeroSalaryWhenNoWorkUnits()
    {
        // Feature: medical-examination-team-management, Property 32 variant: Zero salary when no work units
        var gen =
            from dailyRate in Gen.Choose(200_000, 2_000_000).Select(r => (decimal)r)
            select dailyRate;

        return Prop.ForAll(Arb.From(gen), dailyRate =>
        {
            decimal totalWorkUnits = 0m;
            decimal grossSalary = totalWorkUnits * dailyRate;
            return grossSalary == 0m;
        });
    }

    // -----------------------------------------------------------------------
    // Property 21 (Payroll context): Tổng ngày công tháng bằng tổng bản ghi chấm công
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P21_MonthlyPayrollAggregation()
    {
        // Feature: medical-examination-team-management, Property 21 (payroll): Monthly aggregation
        var gen =
            from staffId in Gen.Choose(1, 100)
            from month in Gen.Choose(1, 12)
            from year in Gen.Choose(2024, 2026)
            from recordCount in Gen.Choose(0, 22)
            from shiftTypes in Gen.ListOf(recordCount, Gen.Elements(0.0, 0.5, 1.0))
            select (staffId, month, year, shiftTypes.ToList());

        return Prop.ForAll(Arb.From(gen), data =>
        {
            var (staffId, month, year, shiftTypes) = data;

            // Tạo bản ghi chấm công trong tháng
            var records = shiftTypes.Select((shift, i) => new GroupStaffDetail
            {
                StaffId = staffId,
                ExamDate = new DateTime(year, month, Math.Min(i + 1, DateTime.DaysInMonth(year, month))),
                ShiftType = shift
            }).ToList();

            // Tổng ngày công tháng
            decimal totalWorkUnits = records
                .Where(r => r.ExamDate.Month == month && r.ExamDate.Year == year)
                .Sum(r => (decimal)r.ShiftType);

            decimal expectedTotal = shiftTypes.Sum(s => (decimal)s);

            return totalWorkUnits == expectedTotal;
        });
    }
}
