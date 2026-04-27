using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;

namespace QuanLyDoanKham.API.Services.Calendar
{
    public class CalendarService : ICalendarService
    {
        private readonly ApplicationDbContext _context;

        public CalendarService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CalendarDayDto>> GetCalendarAsync(int month, int year)
        {
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1).AddDays(-1);

            var groups = await _context.MedicalGroups
                .Include(g => g.HealthContract)
                .ThenInclude(c => c!.Company)
                .Include(g => g.StaffDetails)
                .Where(g => g.ExamDate >= start && g.ExamDate <= end && g.Status != "Cancelled")
                .ToListAsync();

            var result = new List<CalendarDayDto>();

            for (var date = start; date <= end; date = date.AddDays(1))
            {
                var dayGroups = groups
                    .Where(g => g.ExamDate.Date == date.Date)
                    .Select(g => new CalendarGroupDto
                    {
                        GroupId = g.GroupId,
                        GroupName = g.GroupName,
                        CompanyName = g.HealthContract?.Company?.CompanyName,
                        Status = g.Status,
                        AssignedStaffCount = g.StaffDetails?.Count ?? 0
                    })
                    .ToList();

                if (dayGroups.Any())
                {
                    result.Add(new CalendarDayDto
                    {
                        Date = date,
                        Groups = dayGroups
                    });
                }
            }

            return result;
        }

        public async Task<List<MyScheduleDto>> GetMyScheduleAsync(int staffId, int month, int year)
        {
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1).AddDays(-1);

            var assignments = await _context.GroupStaffDetails
                .Include(g => g.MedicalGroup)
                .ThenInclude(mg => mg!.HealthContract)
                .ThenInclude(c => c!.Company)
                .Include(g => g.Position)
                .Where(g => g.StaffId == staffId
                         && g.MedicalGroup.ExamDate >= start
                         && g.MedicalGroup.ExamDate <= end
                         && g.MedicalGroup.Status != "Cancelled")
                .ToListAsync();

            return assignments.Select(a => new MyScheduleDto
            {
                GroupId = a.GroupId,
                GroupName = a.MedicalGroup?.GroupName ?? "",
                ExamDate = a.MedicalGroup?.ExamDate ?? DateTime.MinValue,
                PositionName = a.Position?.Name ?? a.WorkPosition,
                CompanyName = a.MedicalGroup?.HealthContract?.Company?.CompanyName
            }).OrderBy(s => s.ExamDate).ToList();
        }
    }
}
