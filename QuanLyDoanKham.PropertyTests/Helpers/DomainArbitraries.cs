using FsCheck;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Models.Enums;

namespace QuanLyDoanKham.PropertyTests.Helpers;

/// <summary>Custom FsCheck Arbitraries cho domain objects</summary>
public static class DomainArbitraries
{
    // ---- Date helpers ----

    public static Arbitrary<DateTime> ValidExamDates() =>
        Arb.From(
            from year in Gen.Choose(2024, 2026)
            from month in Gen.Choose(1, 12)
            from day in Gen.Choose(1, 28)
            select new DateTime(year, month, day)
        );

    /// <summary>Cặp (StartDate, EndDate) hợp lệ với 1-30 ngày</summary>
    public static Arbitrary<(DateTime Start, DateTime End)> ValidDateRanges() =>
        Arb.From(
            from start in ValidExamDates().Generator
            from days in Gen.Choose(0, 29)
            select (start, start.AddDays(days))
        );

    // ---- Contract ----

    public static Arbitrary<HealthContract> DraftContracts() =>
        Arb.From(
            from start in ValidExamDates().Generator
            from days in Gen.Choose(0, 29)
            from total in Gen.Choose(1_000_000, 100_000_000)
            select new HealthContract
            {
                ContractName = "Test Contract",
                ContractCode = $"HD-{Guid.NewGuid():N}".Substring(0, 20),
                SigningDate = start.AddDays(-1),
                StartDate = start,
                EndDate = start.AddDays(days),
                TotalAmount = total,
                Status = ContractStatus.Draft,
                CreatedAt = DateTime.Now
            }
        );

    public static Arbitrary<HealthContract> ApprovedContracts() =>
        Arb.From(
            from start in ValidExamDates().Generator
            from days in Gen.Choose(0, 29)
            from total in Gen.Choose(1_000_000, 100_000_000)
            select new HealthContract
            {
                ContractName = "Approved Contract",
                ContractCode = $"HD-{Guid.NewGuid():N}".Substring(0, 20),
                SigningDate = start.AddDays(-1),
                StartDate = start,
                EndDate = start.AddDays(days),
                TotalAmount = total,
                Status = ContractStatus.Approved,
                CreatedAt = DateTime.Now
            }
        );

    public static Arbitrary<ContractStatus> NonApprovedStatuses() =>
        Arb.From(
            Gen.Elements(
                ContractStatus.Draft,
                ContractStatus.PendingApproval,
                ContractStatus.Rejected,
                ContractStatus.Cancelled
            )
        );

    // ---- Staff ----

    public static Arbitrary<Staff> ActiveStaff() =>
        Arb.From(
            from name in Gen.Elements("Nguyễn Văn A", "Trần Thị B", "Lê Văn C", "Phạm Thị D")
            from rate in Gen.Choose(200_000, 2_000_000)
            select new Staff
            {
                FullName = name,
                DailyRate = rate,
                IsActive = true,
                SalaryType = "ByDay",
                CreatedDate = DateTime.Now
            }
        );

    public static Arbitrary<Staff> StaffWithRole(string role) =>
        Arb.From(
            from name in Gen.Elements("Bác sĩ A", "Bác sĩ B", "Bác sĩ C")
            from rate in Gen.Choose(300_000, 1_500_000)
            select new Staff
            {
                FullName = name,
                JobTitle = role,
                DailyRate = rate,
                IsActive = true,
                SalaryType = "ByDay",
                CreatedDate = DateTime.Now
            }
        );

    // ---- Attendance / Work units ----

    /// <summary>Cặp (CheckIn, CheckOut) với khoảng giờ ngẫu nhiên 0-12h</summary>
    public static Arbitrary<(DateTime CheckIn, DateTime CheckOut)> AttendancePairs() =>
        Arb.From(
            from baseDate in ValidExamDates().Generator
            from checkInHour in Gen.Choose(6, 10)
            from durationMinutes in Gen.Choose(0, 720) // 0-12 hours
            let checkIn = baseDate.AddHours(checkInHour)
            let checkOut = checkIn.AddMinutes(durationMinutes)
            select (checkIn, checkOut)
        );

    // ---- Inventory ----

    public static Arbitrary<(int InitialStock, List<int> Receipts, List<int> Issues)> InventorySequences() =>
        Arb.From(
            from initial in Gen.Choose(0, 1000)
            from receiptCount in Gen.Choose(0, 5)
            from issueCount in Gen.Choose(0, 5)
            from receipts in Gen.ListOf(receiptCount, Gen.Choose(1, 100))
            from issues in Gen.ListOf(issueCount, Gen.Choose(1, 50))
            select (initial, receipts.ToList(), issues.ToList())
        );

    // ---- Finance ----

    public static Arbitrary<(decimal Revenue, decimal StaffCost, decimal MaterialCost, decimal OtherCost)> FinanceComponents() =>
        Arb.From(
            from revenue in Gen.Choose(0, 100_000_000)
            from staffCost in Gen.Choose(0, 50_000_000)
            from materialCost in Gen.Choose(0, 20_000_000)
            from otherCost in Gen.Choose(0, 10_000_000)
            select ((decimal)revenue, (decimal)staffCost, (decimal)materialCost, (decimal)otherCost)
        );
}
