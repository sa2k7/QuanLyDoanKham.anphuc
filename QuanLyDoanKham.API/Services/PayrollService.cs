using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Services
{
    public class PayrollService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PayrollService> _logger;

        public PayrollService(ApplicationDbContext context, ILogger<PayrollService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool Success, string Message)> CalculateMonthlyPayrollAsync(int staffId, int month, int year)
        {
            var staff = await _context.Staffs
                .Include(s => s.GroupStaffDetails)
                .ThenInclude(gsd => gsd.MedicalGroup)
                .FirstOrDefaultAsync(s => s.StaffId == staffId && s.IsActive);

            if (staff == null) return (false, "Không tìm thấy nhân viên.");

            var monthStart = new DateTime(year, month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var detailsInMonth = staff.GroupStaffDetails?
                .Where(gsd => gsd.MedicalGroup != null
                    && gsd.MedicalGroup.ExamDate.Date >= monthStart.Date
                    && gsd.MedicalGroup.ExamDate.Date <= monthEnd.Date)
                .ToList() ?? new List<GroupStaffDetail>();

            if (!detailsInMonth.Any()) return (false, "Không có dữ liệu chấm công cho tháng này.");

            // ── FIX: Only count attendance records that have BOTH check-in AND check-out ──
            // Fetch ScheduleCalendar records for this staff in the month to determine validity.
            var groupIds = detailsInMonth.Select(d => d.GroupId).ToList();
            var completeAttendance = await _context.ScheduleCalendars
                .AsNoTracking()
                .Where(sc => sc.StaffId == staffId
                    && groupIds.Contains(sc.GroupId)
                    && sc.CheckInTime != null
                    && sc.CheckOutTime != null)
                .Select(sc => sc.GroupId)
                .ToListAsync();

            var completeSet = completeAttendance.ToHashSet();

            int skipped = 0;
            foreach (var detail in detailsInMonth)
            {
                if (!completeAttendance.Contains(detail.GroupId))
                {
                    // Incomplete attendance (check-in only or no attendance) — exclude from payroll
                    _logger.LogWarning(
                        "Payroll: Staff {StaffId} GroupId {GroupId} on {Date} excluded — missing check-out",
                        staffId, detail.GroupId, detail.MedicalGroup?.ExamDate.ToString("dd/MM/yyyy"));
                    skipped++;
                    continue;
                }

                detail.CalculatedSalary = CalculateDetailSalary(staff, detail);
            }

            await _context.SaveChangesAsync();

            var processed = detailsInMonth.Count - skipped;
            return (true, $"Đã cập nhật lương cho {processed} công trong tháng {month}/{year}." +
                (skipped > 0 ? $" ({skipped} bản ghi bị bỏ qua do chưa check-out.)" : ""));
        }

        public async Task<(bool Success, string Message)> CalculateAllMonthlyPayrollAsync(int month, int year)
        {
            var staffs = await _context.Staffs
                .Include(s => s.GroupStaffDetails)
                .ThenInclude(gsd => gsd.MedicalGroup)
                .Where(s => s.IsActive)
                .ToListAsync();

            var monthStart = new DateTime(year, month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            // ── FIX: Load all complete attendance records for the month in one query ──
            var completeAttendance = await _context.ScheduleCalendars
                .AsNoTracking()
                .Where(sc => sc.ExamDate.Month == month
                    && sc.ExamDate.Year == year
                    && sc.CheckInTime != null
                    && sc.CheckOutTime != null)
                .Select(sc => new { sc.StaffId, sc.GroupId })
                .ToListAsync();

            // Build a HashSet for O(1) lookup: (staffId, groupId)
            var completeSet = completeAttendance
                .Where(x => x.StaffId.HasValue)
                .Select(x => (x.StaffId!.Value, x.GroupId))
                .ToHashSet();

            int count = 0;
            int skipped = 0;

            foreach (var staff in staffs)
            {
                var details = staff.GroupStaffDetails?
                    .Where(gsd => gsd.MedicalGroup != null
                        && gsd.MedicalGroup.ExamDate.Date >= monthStart.Date
                        && gsd.MedicalGroup.ExamDate.Date <= monthEnd.Date)
                    .ToList();

                if (details == null || !details.Any()) continue;

                foreach (var detail in details)
                {
                    if (!completeSet.Contains((staff.StaffId, detail.GroupId)))
                    {
                        _logger.LogWarning(
                            "Payroll bulk: Staff {StaffId} GroupId {GroupId} excluded — missing check-out",
                            staff.StaffId, detail.GroupId);
                        skipped++;
                        continue;
                    }

                    detail.CalculatedSalary = CalculateDetailSalary(staff, detail);
                    count++;
                }
            }

            if (count == 0 && skipped == 0)
                return (false, "Không tìm thấy dữ liệu chấm công nào để tính toán trong tháng này.");

            await _context.SaveChangesAsync();

            return (true, $"Đã tính toán thành công lương cho toàn diện nhân sự. " +
                $"Tổng cộng {count} lượt công đã được xử lý." +
                (skipped > 0 ? $" ({skipped} bản ghi bị bỏ qua do chưa check-out.)" : ""));
        }

        private decimal CalculateDetailSalary(Staff staff, GroupStaffDetail detail)
        {
            // Logic tính toán:
            // 1. Nếu theo ngày: DailyRate * ShiftType
            // 2. Nếu theo tháng: (BaseSalary / StandardWorkDays) * ShiftType

            decimal rate = 0;
            if (staff.SalaryType == "ByMonth")
            {
                int stdDays = staff.StandardWorkDays > 0 ? staff.StandardWorkDays : 26;
                rate = staff.BaseSalary / stdDays;
            }
            else // Default or "ByDay"
            {
                rate = staff.DailyRate;
            }

            return rate * (decimal)detail.ShiftType;
        }
    }
}
