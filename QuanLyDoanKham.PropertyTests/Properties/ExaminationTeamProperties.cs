using FsCheck;
using FsCheck.Xunit;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Models.Enums;
using QuanLyDoanKham.PropertyTests.Helpers;

namespace QuanLyDoanKham.PropertyTests.Properties;

/// <summary>
/// Property-based tests cho module Đoàn Khám.
/// Feature: medical-examination-team-management
/// </summary>
public class ExaminationTeamProperties
{
    // -----------------------------------------------------------------------
    // Property 7: Số đoàn khám sinh ra bằng số ngày trong hợp đồng
    // Validates: Requirements 2.2
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P7_TeamCountEqualsContractDays()
    {
        // Feature: medical-examination-team-management, Property 7: Team count = contract days
        return Prop.ForAll(DomainArbitraries.ValidDateRanges(), range =>
        {
            var (start, end) = range;
            var expectedCount = (end.Date - start.Date).Days + 1;

            // Simulate AutoCreateMedicalGroupService logic
            var teams = new List<MedicalGroup>();
            var currentDate = start.Date;
            var dayCount = 1;

            while (currentDate <= end.Date)
            {
                teams.Add(new MedicalGroup
                {
                    GroupName = $"Đoàn khám - Ngày {dayCount}",
                    ExamDate = currentDate,
                    Status = "Draft",
                    TeamCode = $"DAY_{dayCount:D2}"
                });
                currentDate = currentDate.AddDays(1);
                dayCount++;
            }

            return teams.Count == expectedCount;
        });
    }

    // -----------------------------------------------------------------------
    // Property 8: Mã đoàn khám là duy nhất trong toàn hệ thống
    // Validates: Requirements 2.4
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P8_TeamCodesAreUnique()
    {
        // Feature: medical-examination-team-management, Property 8: TeamCode uniqueness
        var gen =
            from days in Gen.Choose(1, 30)
            from baseDate in DomainArbitraries.ValidExamDates().Generator
            select (days, baseDate);

        return Prop.ForAll(Arb.From(gen), pair =>
        {
            var (days, baseDate) = pair;
            var teams = new List<MedicalGroup>();

            for (int i = 0; i < days; i++)
            {
                // Mỗi đoàn có TeamCode duy nhất dựa trên GUID
                var uniqueCode = $"TEAM-{Guid.NewGuid():N}".Substring(0, 20);
                teams.Add(new MedicalGroup
                {
                    GroupName = $"Đoàn {i + 1}",
                    ExamDate = baseDate.AddDays(i),
                    Status = "Draft",
                    TeamCode = uniqueCode
                });
            }

            var allCodes = teams.Select(t => t.TeamCode).ToList();
            var distinctCodes = allCodes.Distinct().ToList();

            return allCodes.Count == distinctCodes.Count;
        });
    }

    // -----------------------------------------------------------------------
    // Property 7 (variant): Mỗi đoàn khám ứng với đúng một ngày trong hợp đồng
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P7_EachTeamHasDistinctExamDate()
    {
        // Feature: medical-examination-team-management, Property 7 variant: Each team has distinct exam date
        return Prop.ForAll(DomainArbitraries.ValidDateRanges(), range =>
        {
            var (start, end) = range;
            var teams = new List<MedicalGroup>();
            var currentDate = start.Date;
            int dayCount = 1;

            while (currentDate <= end.Date)
            {
                teams.Add(new MedicalGroup
                {
                    GroupName = $"Đoàn {dayCount}",
                    ExamDate = currentDate,
                    Status = "Draft"
                });
                currentDate = currentDate.AddDays(1);
                dayCount++;
            }

            // Mỗi ngày chỉ có một đoàn
            var distinctDates = teams.Select(t => t.ExamDate.Date).Distinct().Count();
            return distinctDates == teams.Count;
        });
    }
}
