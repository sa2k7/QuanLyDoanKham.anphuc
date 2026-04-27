using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;

namespace QuanLyDoanKham.API.Services.AI;

public class AISchedulerService : IAISchedulerService
{
    private readonly ApplicationDbContext _context;

    // Bảng điểm khớp vai trò - vị trí khám
    private static readonly Dictionary<string, List<string>> RolePositionMap = new()
    {
        ["Bác sĩ nội"]              = new() { "Khám nội" },
        ["Bác sĩ ngoại"]            = new() { "Khám ngoại" },
        ["Bác sĩ sản"]              = new() { "Khám sản" },
        ["Kỹ thuật viên siêu âm"]   = new() { "Siêu âm" },
        ["Điều dưỡng"]              = new() { "Lấy máu", "Tiếp nhận" },
        ["Nhân viên hành chính"]    = new() { "Tiếp nhận" },
    };

    public AISchedulerService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ScheduleProposal> ProposeAsync(int teamId)
    {
        var group = await _context.MedicalGroups.FindAsync(teamId);
        if (group == null) throw new InvalidOperationException($"Team {teamId} not found");

        // Lấy các vị trí cần phân công từ GroupPositionQuota
        var quotas = await _context.GroupPositionQuotas
            .Include(q => q.Position)
            .Where(q => q.MedicalGroupId == teamId)
            .ToListAsync();

        var positionNames = quotas.Any()
            ? quotas.Select(q => new { Id = q.PositionId, Name = q.Position?.Name ?? "Nhân viên" }).ToList()
            : new[] { "Tiếp nhận", "Khám nội", "Khám ngoại", "Lấy máu", "Khám sản", "Siêu âm" }
                .Select((name, i) => new { Id = i + 1, Name = name }).ToList();

        // Lấy nhân sự đã được phân công vào ngày khám (tránh trùng lịch)
        var assignedOnDate = await _context.GroupStaffDetails
            .Where(gsd => gsd.ExamDate.Date == group.ExamDate.Date && gsd.GroupId != teamId)
            .Select(gsd => gsd.StaffId)
            .ToListAsync();

        var availableStaff = await _context.Staffs
            .Where(s => s.IsActive && !assignedOnDate.Contains(s.StaffId))
            .ToListAsync();

        var proposal = new ScheduleProposal { TeamId = teamId };
        var usedStaffIds = new HashSet<int>();

        foreach (var position in positionNames)
        {
            var scored = availableStaff
                .Where(s => !usedStaffIds.Contains(s.StaffId))
                .Select(s => new { Staff = s, Score = CalculateScore(s.JobTitle ?? "", position.Name) })
                .OrderByDescending(x => x.Score)
                .ToList();

            var best = scored.FirstOrDefault();
            if (best == null || best.Score == 0)
            {
                proposal.Warnings.Add($"Không tìm được nhân sự phù hợp cho vị trí '{position.Name}'");
                var fallback = availableStaff.FirstOrDefault(s => !usedStaffIds.Contains(s.StaffId));
                if (fallback != null)
                {
                    proposal.Assignments.Add(new Assignment
                    {
                        PositionId = position.Id,
                        PositionName = position.Name,
                        StaffId = fallback.StaffId,
                        StaffName = fallback.FullName ?? "",
                        IsFallback = true
                    });
                    usedStaffIds.Add(fallback.StaffId);
                }
            }
            else
            {
                proposal.Assignments.Add(new Assignment
                {
                    PositionId = position.Id,
                    PositionName = position.Name,
                    StaffId = best.Staff.StaffId,
                    StaffName = best.Staff.FullName ?? "",
                    IsFallback = false
                });
                usedStaffIds.Add(best.Staff.StaffId);
            }
        }

        return proposal;
    }

    private int CalculateScore(string professionalRole, string positionName)
    {
        if (RolePositionMap.TryGetValue(professionalRole, out var positions))
            return positions.Contains(positionName) ? 100 : 10;
        return 5;
    }
}
