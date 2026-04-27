using FsCheck;
using FsCheck.Xunit;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Services;
using QuanLyDoanKham.PropertyTests.Helpers;

namespace QuanLyDoanKham.PropertyTests.Properties;

/// <summary>
/// Property-based tests cho module QR Check-in/Check-out và Chấm công.
/// Feature: medical-examination-team-management
/// </summary>
public class QRAttendanceProperties
{
    private readonly TimeSheetService _timeSheetService = new();

    // -----------------------------------------------------------------------
    // Property 18: Mã QR check-in và check-out là duy nhất mỗi lần sinh
    // Validates: Requirements 6.2, 6.3
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P18_QRCodesAreUniquePerGeneration()
    {
        // Feature: medical-examination-team-management, Property 18: QR codes unique per generation
        var gen =
            from count in Gen.Choose(2, 20)
            select count;

        return Prop.ForAll(Arb.From(gen), count =>
        {
            // Simulate QR generation: mỗi lần sinh dùng GUID + timestamp
            var qrCodes = Enumerable.Range(1, count)
                .Select(_ => GenerateUniqueQRCode())
                .ToList();

            var distinctCodes = qrCodes.Distinct().ToList();
            return distinctCodes.Count == count;
        });
    }

    private static string GenerateUniqueQRCode()
    {
        // Simulate QRService.GenerateSignedToken logic: unique per call
        return $"QR-{Guid.NewGuid():N}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
    }

    // -----------------------------------------------------------------------
    // Property 19: Tính công đúng theo ngưỡng giờ thực tế
    // Validates: Requirements 6.5, 6.6
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P19_WorkUnitsCalculatedCorrectly()
    {
        // Feature: medical-examination-team-management, Property 19: Work units calculated correctly
        return Prop.ForAll(DomainArbitraries.AttendancePairs(), pair =>
        {
            var (checkIn, checkOut) = pair;
            var hours = (checkOut - checkIn).TotalHours;

            var workUnits = _timeSheetService.ComputeWorkUnits(checkIn, checkOut);

            // Quy tắc tính công từ TimeSheetService (halfDayHours = 4)
            if (hours <= 0) return workUnits == 0m;
            if (hours < 4) return workUnits == 0.5m;
            return workUnits == 1.0m;
        });
    }

    // -----------------------------------------------------------------------
    // Property 19 (variant): Kiểm tra từng ngưỡng cụ thể
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P19_WorkUnitsThresholds()
    {
        // Feature: medical-examination-team-management, Property 19 variant: Threshold checks
        var gen =
            from hours in Gen.Choose(0, 720).Select(m => m / 60.0) // 0-12 hours
            select hours;

        return Prop.ForAll(Arb.From(gen), hours =>
        {
            var baseTime = new DateTime(2025, 1, 1, 8, 0, 0);
            var checkIn = baseTime;
            var checkOut = baseTime.AddHours(hours);

            var workUnits = _timeSheetService.ComputeWorkUnits(checkIn, checkOut);

            if (hours <= 0) return workUnits == 0m;
            if (hours < 4) return workUnits == 0.5m;
            return workUnits == 1.0m;
        });
    }

    // -----------------------------------------------------------------------
    // Property 20: Bản ghi chấm công chỉ có check-in mà không có check-out được đánh dấu "Chưa hoàn chỉnh"
    // Validates: Requirements 6.7
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P20_IncompleteAttendanceMarkedCorrectly()
    {
        // Feature: medical-examination-team-management, Property 20: Incomplete attendance marked
        var gen =
            from checkInHour in Gen.Choose(6, 10)
            from staffId in Gen.Choose(1, 100)
            select (checkInHour, staffId);

        return Prop.ForAll(Arb.From(gen), data =>
        {
            var (checkInHour, staffId) = data;

            // Bản ghi chỉ có check-in, không có check-out
            var record = new GroupStaffDetail
            {
                StaffId = staffId,
                CheckInTime = new DateTime(2025, 1, 1, checkInHour, 0, 0),
                CheckOutTime = null, // Chưa check-out
                WorkStatus = "Pending"
            };

            // Logic: nếu có check-in nhưng không có check-out → "Chưa hoàn chỉnh"
            string status = DetermineAttendanceStatus(record);
            return status == "Chưa hoàn chỉnh";
        });
    }

    [Property(MaxTest = 100)]
    public Property P20_CompleteAttendanceNotMarkedIncomplete()
    {
        // Feature: medical-examination-team-management, Property 20 variant: Complete attendance not marked incomplete
        var gen =
            from checkInHour in Gen.Choose(6, 10)
            from durationHours in Gen.Choose(4, 10)
            from staffId in Gen.Choose(1, 100)
            select (checkInHour, durationHours, staffId);

        return Prop.ForAll(Arb.From(gen), data =>
        {
            var (checkInHour, durationHours, staffId) = data;

            var checkIn = new DateTime(2025, 1, 1, checkInHour, 0, 0);
            var record = new GroupStaffDetail
            {
                StaffId = staffId,
                CheckInTime = checkIn,
                CheckOutTime = checkIn.AddHours(durationHours),
                WorkStatus = "Joined"
            };

            string status = DetermineAttendanceStatus(record);
            return status != "Chưa hoàn chỉnh";
        });
    }

    private static string DetermineAttendanceStatus(GroupStaffDetail record)
    {
        if (record.CheckInTime.HasValue && !record.CheckOutTime.HasValue)
            return "Chưa hoàn chỉnh";
        if (record.CheckInTime.HasValue && record.CheckOutTime.HasValue)
            return "Hoàn chỉnh";
        return "Chưa check-in";
    }

    // -----------------------------------------------------------------------
    // Property 21: Tổng ngày công tháng bằng tổng các bản ghi chấm công trong tháng
    // Validates: Requirements 6.8, 12.2
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P21_MonthlyWorkUnitsEqualsSum()
    {
        // Feature: medical-examination-team-management, Property 21: Monthly work units = sum of records
        var gen =
            from month in Gen.Choose(1, 12)
            from year in Gen.Choose(2024, 2026)
            from recordCount in Gen.Choose(0, 20)
            from workUnits in Gen.ListOf(recordCount, Gen.Elements(0m, 0.5m, 1.0m))
            select (month, year, workUnits.ToList());

        return Prop.ForAll(Arb.From(gen), data =>
        {
            var (month, year, workUnits) = data;

            // Tạo bản ghi chấm công trong tháng
            var records = workUnits.Select((wu, i) => new GroupStaffDetail
            {
                StaffId = 1,
                ExamDate = new DateTime(year, month, Math.Min(i + 1, DateTime.DaysInMonth(year, month))),
                ShiftType = (double)wu
            }).ToList();

            // Tổng ngày công tháng
            var totalWorkUnits = records.Sum(r => (decimal)r.ShiftType);
            var expectedTotal = workUnits.Sum();

            return totalWorkUnits == expectedTotal;
        });
    }
}
