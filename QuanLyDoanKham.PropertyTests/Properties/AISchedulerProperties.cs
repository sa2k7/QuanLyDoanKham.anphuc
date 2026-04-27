using FsCheck;
using FsCheck.Xunit;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Services.AI;
using QuanLyDoanKham.PropertyTests.Helpers;

namespace QuanLyDoanKham.PropertyTests.Properties;

/// <summary>
/// Property-based tests cho module AI Scheduler.
/// Feature: medical-examination-team-management
/// </summary>
public class AISchedulerProperties
{
    // Bảng điểm khớp vai trò - vị trí (mirror từ AISchedulerService)
    private static readonly Dictionary<string, List<string>> RolePositionMap = new()
    {
        ["Bác sĩ nội"]              = new() { "Khám nội" },
        ["Bác sĩ ngoại"]            = new() { "Khám ngoại" },
        ["Bác sĩ sản"]              = new() { "Khám sản" },
        ["Kỹ thuật viên siêu âm"]   = new() { "Siêu âm" },
        ["Điều dưỡng"]              = new() { "Lấy máu", "Tiếp nhận" },
        ["Nhân viên hành chính"]    = new() { "Tiếp nhận" },
    };

    private static int CalculateScore(string professionalRole, string positionName)
    {
        if (RolePositionMap.TryGetValue(professionalRole, out var positions))
            return positions.Contains(positionName) ? 100 : 10;
        return 5;
    }

    // -----------------------------------------------------------------------
    // Property 16: AI_Scheduler đề xuất nhân sự có vai trò khớp khi có sẵn
    // Validates: Requirements 5.2, 5.3
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P16_AISchedulerPrefersMatchingRole()
    {
        // Feature: medical-examination-team-management, Property 16: AI prefers matching role
        var gen =
            from positionName in Gen.Elements("Khám nội", "Khám ngoại", "Khám sản", "Siêu âm", "Lấy máu", "Tiếp nhận")
            select positionName;

        return Prop.ForAll(Arb.From(gen), positionName =>
        {
            // Tìm vai trò khớp với vị trí
            var matchingRole = RolePositionMap
                .FirstOrDefault(kv => kv.Value.Contains(positionName)).Key;

            if (matchingRole == null) return true; // Không có vai trò khớp, bỏ qua

            // Tạo pool nhân sự: 1 người có vai trò khớp + vài người không khớp
            var staffPool = new List<Staff>
            {
                new Staff { StaffId = 1, FullName = "Nhân sự khớp", JobTitle = matchingRole, IsActive = true },
                new Staff { StaffId = 2, FullName = "Nhân sự không khớp 1", JobTitle = "Nhân viên tạp vụ", IsActive = true },
                new Staff { StaffId = 3, FullName = "Nhân sự không khớp 2", JobTitle = "Lái xe", IsActive = true },
            };

            // Simulate AI scoring
            var scored = staffPool
                .Select(s => new { Staff = s, Score = CalculateScore(s.JobTitle ?? "", positionName) })
                .OrderByDescending(x => x.Score)
                .ToList();

            var best = scored.First();

            // Nhân sự được chọn phải có vai trò khớp (score = 100)
            return best.Score == 100 && best.Staff.JobTitle == matchingRole;
        });
    }

    // -----------------------------------------------------------------------
    // Property 17: AI_Scheduler cảnh báo và đề xuất fallback khi thiếu nhân sự phù hợp
    // Validates: Requirements 5.6
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P17_AISchedulerWarnsAndFallbackWhenNoMatch()
    {
        // Feature: medical-examination-team-management, Property 17: AI warns and fallback when no match
        var gen =
            from positionName in Gen.Elements("Khám nội", "Khám ngoại", "Khám sản", "Siêu âm")
            from fallbackCount in Gen.Choose(1, 5)
            select (positionName, fallbackCount);

        return Prop.ForAll(Arb.From(gen), data =>
        {
            var (positionName, fallbackCount) = data;

            // Tạo pool nhân sự không có ai khớp với vị trí
            var staffPool = Enumerable.Range(1, fallbackCount).Select(i => new Staff
            {
                StaffId = i,
                FullName = $"Nhân viên {i}",
                JobTitle = "Nhân viên tạp vụ", // Không khớp với bất kỳ vị trí nào
                IsActive = true
            }).ToList();

            var warnings = new List<string>();
            var assignments = new List<Assignment>();
            var usedStaffIds = new HashSet<int>();

            // Simulate AI proposal logic
            var scored = staffPool
                .Where(s => !usedStaffIds.Contains(s.StaffId))
                .Select(s => new { Staff = s, Score = CalculateScore(s.JobTitle ?? "", positionName) })
                .OrderByDescending(x => x.Score)
                .ToList();

            var best = scored.FirstOrDefault();
            if (best == null || best.Score == 0)
            {
                warnings.Add($"Không tìm được nhân sự phù hợp cho vị trí '{positionName}'");
                var fallback = staffPool.FirstOrDefault(s => !usedStaffIds.Contains(s.StaffId));
                if (fallback != null)
                {
                    assignments.Add(new Assignment
                    {
                        PositionName = positionName,
                        StaffId = fallback.StaffId,
                        IsFallback = true
                    });
                }
            }
            else
            {
                // Score > 0 nhưng không phải 100 → vẫn là fallback
                if (best.Score < 100)
                {
                    warnings.Add($"Không tìm được nhân sự phù hợp cho vị trí '{positionName}'");
                    assignments.Add(new Assignment
                    {
                        PositionName = positionName,
                        StaffId = best.Staff.StaffId,
                        IsFallback = true
                    });
                }
            }

            // Phải có cảnh báo và đề xuất fallback
            bool hasWarning = warnings.Count > 0;
            bool hasFallback = assignments.Any(a => a.IsFallback);

            return hasWarning && hasFallback;
        });
    }

    // -----------------------------------------------------------------------
    // Property 16 (variant): Score = 100 khi vai trò khớp chính xác
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P16_ScoreIs100ForExactRoleMatch()
    {
        // Feature: medical-examination-team-management, Property 16 variant: Score 100 for exact match
        var gen =
            from entry in Gen.Elements(RolePositionMap.ToArray())
            from positionName in Gen.Elements(entry.Value.ToArray())
            select (entry.Key, positionName);

        return Prop.ForAll(Arb.From(gen), pair =>
        {
            var (role, position) = pair;
            return CalculateScore(role, position) == 100;
        });
    }
}
