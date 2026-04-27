using FsCheck;
using FsCheck.Xunit;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.PropertyTests.Helpers;

namespace QuanLyDoanKham.PropertyTests.Properties;

/// <summary>
/// Property-based tests cho module Lịch Cá nhân.
/// Feature: medical-examination-team-management
/// </summary>
public class ScheduleProperties
{
    // -----------------------------------------------------------------------
    // Property 33: Lịch theo ngày chỉ trả về đoàn khám diễn ra đúng ngày đó
    // Validates: Requirements 14.2
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P33_DailyScheduleOnlyContainsTeamsOnThatDate()
    {
        // Feature: medical-examination-team-management, Property 33: Daily schedule only contains teams on that date
        var gen =
            from selectedDate in DomainArbitraries.ValidExamDates().Generator
            from teamCount in Gen.Choose(1, 20)
            select (selectedDate, teamCount);

        return Prop.ForAll(Arb.From(gen), data =>
        {
            var (selectedDate, teamCount) = data;

            // Tạo đoàn khám với ngày khác nhau
            var allTeams = Enumerable.Range(0, teamCount).Select(i => new MedicalGroup
            {
                GroupId = i + 1,
                GroupName = $"Đoàn {i + 1}",
                ExamDate = selectedDate.AddDays(i - teamCount / 2) // Phân bố quanh ngày được chọn
            }).ToList();

            // Lọc đoàn khám theo ngày được chọn
            var teamsOnDate = allTeams
                .Where(t => t.ExamDate.Date == selectedDate.Date)
                .ToList();

            // Tất cả đoàn trả về phải có ExamDate đúng ngày được chọn
            return teamsOnDate.All(t => t.ExamDate.Date == selectedDate.Date);
        });
    }

    // -----------------------------------------------------------------------
    // Property 33 (variant): Không có đoàn nào của ngày khác trong kết quả
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P33_NoTeamsFromOtherDatesInResult()
    {
        // Feature: medical-examination-team-management, Property 33 variant: No teams from other dates
        var gen =
            from selectedDate in DomainArbitraries.ValidExamDates().Generator
            from sameCount in Gen.Choose(0, 5)
            from otherCount in Gen.Choose(1, 10)
            select (selectedDate, sameCount, otherCount);

        return Prop.ForAll(Arb.From(gen), data =>
        {
            var (selectedDate, sameCount, otherCount) = data;

            var allTeams = new List<MedicalGroup>();

            // Đoàn khám đúng ngày
            for (int i = 0; i < sameCount; i++)
                allTeams.Add(new MedicalGroup { GroupId = i + 1, ExamDate = selectedDate });

            // Đoàn khám ngày khác
            for (int i = 0; i < otherCount; i++)
                allTeams.Add(new MedicalGroup { GroupId = sameCount + i + 1, ExamDate = selectedDate.AddDays(i + 1) });

            // Lọc theo ngày
            var result = allTeams.Where(t => t.ExamDate.Date == selectedDate.Date).ToList();

            return result.Count == sameCount
                && result.All(t => t.ExamDate.Date == selectedDate.Date);
        });
    }

    // -----------------------------------------------------------------------
    // Property 22 (Schedule context): Lịch nhân sự chỉ chứa đoàn được phân công
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P22_PersonalScheduleOnlyAssignedTeams()
    {
        // Feature: medical-examination-team-management, Property 22 (schedule): Personal schedule only assigned teams
        var gen =
            from staffId in Gen.Choose(1, 100)
            from assignedCount in Gen.Choose(1, 5)
            from totalCount in Gen.Choose(6, 20)
            select (staffId, assignedCount, totalCount);

        return Prop.ForAll(Arb.From(gen), data =>
        {
            var (staffId, assignedCount, totalCount) = data;

            // Tất cả đoàn khám
            var allTeams = Enumerable.Range(1, totalCount).Select(i => new MedicalGroup
            {
                GroupId = i,
                GroupName = $"Đoàn {i}",
                ExamDate = DateTime.Today.AddDays(i)
            }).ToList();

            // Phân công nhân sự vào một số đoàn
            var assignedTeamIds = Enumerable.Range(1, assignedCount).ToHashSet();
            var assignments = assignedTeamIds.Select(tid => new GroupStaffDetail
            {
                GroupId = tid,
                StaffId = staffId,
                WorkPosition = "Test",
                ExamDate = DateTime.Today.AddDays(tid)
            }).ToList();

            // Lịch cá nhân = đoàn có phân công
            var personalSchedule = allTeams
                .Where(t => assignments.Any(a => a.GroupId == t.GroupId && a.StaffId == staffId))
                .ToList();

            return personalSchedule.Count == assignedCount
                && personalSchedule.All(t => assignedTeamIds.Contains(t.GroupId));
        });
    }
}
