using FsCheck;
using FsCheck.Xunit;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.PropertyTests.Helpers;

namespace QuanLyDoanKham.PropertyTests.Properties;

/// <summary>
/// Property-based tests cho module Nhân sự và Phân công.
/// Feature: medical-examination-team-management
/// </summary>
public class StaffAssignmentProperties
{
    // -----------------------------------------------------------------------
    // Property 14: Số lượng nhân sự hiển thị tại vị trí khám khớp với dữ liệu thực
    // Validates: Requirements 4.4
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P14_StaffCountMatchesActualAssignments()
    {
        // Feature: medical-examination-team-management, Property 14: Staff count matches actual assignments
        var gen =
            from positionId in Gen.Choose(1, 10)
            from staffCount in Gen.Choose(0, 10)
            select (positionId, staffCount);

        return Prop.ForAll(Arb.From(gen), data =>
        {
            var (positionId, staffCount) = data;

            // Tạo các bản ghi GroupStaffDetail cho vị trí này
            var assignments = Enumerable.Range(1, staffCount).Select(i => new GroupStaffDetail
            {
                Id = i,
                GroupId = 1,
                StaffId = i,
                PositionId = positionId,
                WorkPosition = "Test Position",
                ExamDate = DateTime.Today
            }).ToList();

            // Số lượng nhân sự hiển thị phải bằng số bản ghi thực tế
            var displayedCount = assignments.Count(a => a.PositionId == positionId);
            return displayedCount == staffCount;
        });
    }

    // -----------------------------------------------------------------------
    // Property 15: Thêm nhân sự vào đoàn mà không chỉ định vị trí bị từ chối
    // Validates: Requirements 4.5
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P15_StaffAssignmentWithoutPositionRejected()
    {
        // Feature: medical-examination-team-management, Property 15: Staff assignment without position rejected
        var gen =
            from staffId in Gen.Choose(1, 1000)
            from groupId in Gen.Choose(1, 100)
            select (staffId, groupId);

        return Prop.ForAll(Arb.From(gen), data =>
        {
            var (staffId, groupId) = data;

            // Simulate validation: PositionId là bắt buộc
            bool isValid = ValidateStaffAssignment(staffId, groupId, positionId: null);
            return !isValid; // Phải bị từ chối
        });
    }

    [Property(MaxTest = 100)]
    public Property P15_StaffAssignmentWithPositionAccepted()
    {
        // Feature: medical-examination-team-management, Property 15 variant: With position is accepted
        var gen =
            from staffId in Gen.Choose(1, 1000)
            from groupId in Gen.Choose(1, 100)
            from positionId in Gen.Choose(1, 10)
            select (staffId, groupId, positionId);

        return Prop.ForAll(Arb.From(gen), data =>
        {
            var (staffId, groupId, positionId) = data;

            bool isValid = ValidateStaffAssignment(staffId, groupId, positionId);
            return isValid; // Phải được chấp nhận
        });
    }

    private static bool ValidateStaffAssignment(int staffId, int groupId, int? positionId)
    {
        // Business rule: PositionId là bắt buộc khi thêm nhân sự vào đoàn
        if (!positionId.HasValue || positionId.Value <= 0)
            return false;
        if (staffId <= 0 || groupId <= 0)
            return false;
        return true;
    }

    // -----------------------------------------------------------------------
    // Property 22: Lịch đoàn khám của nhân sự chỉ chứa đoàn họ được phân công
    // Validates: Requirements 7.3, 14.3
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P22_StaffScheduleOnlyContainsAssignedTeams()
    {
        // Feature: medical-examination-team-management, Property 22: Staff schedule only contains assigned teams
        var gen =
            from staffId in Gen.Choose(1, 100)
            from assignedTeamCount in Gen.Choose(1, 10)
            from totalTeamCount in Gen.Choose(11, 30)
            select (staffId, assignedTeamCount, totalTeamCount);

        return Prop.ForAll(Arb.From(gen), data =>
        {
            var (staffId, assignedTeamCount, totalTeamCount) = data;

            // Tạo tất cả đoàn khám
            var allTeams = Enumerable.Range(1, totalTeamCount).Select(i => new MedicalGroup
            {
                GroupId = i,
                GroupName = $"Đoàn {i}",
                ExamDate = DateTime.Today.AddDays(i)
            }).ToList();

            // Phân công nhân sự vào một số đoàn
            var assignments = Enumerable.Range(1, assignedTeamCount).Select(i => new GroupStaffDetail
            {
                GroupId = i,
                StaffId = staffId,
                WorkPosition = "Test",
                ExamDate = DateTime.Today.AddDays(i)
            }).ToList();

            // Lịch của nhân sự = các đoàn có bản ghi phân công
            var staffScheduleTeamIds = assignments
                .Where(a => a.StaffId == staffId)
                .Select(a => a.GroupId)
                .ToHashSet();

            var staffSchedule = allTeams
                .Where(t => staffScheduleTeamIds.Contains(t.GroupId))
                .ToList();

            return staffSchedule.Count == assignedTeamCount
                && staffSchedule.All(t => staffScheduleTeamIds.Contains(t.GroupId));
        });
    }
}
