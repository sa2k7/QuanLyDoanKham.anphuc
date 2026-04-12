using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Services
{
    public class PayrollService
    {
        private readonly ApplicationDbContext _context;

        public PayrollService(ApplicationDbContext context)
        {
            _context = context;
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
                .Where(gsd => gsd.MedicalGroup != null && gsd.MedicalGroup.ExamDate.Date >= monthStart.Date && gsd.MedicalGroup.ExamDate.Date <= monthEnd.Date)
                .ToList() ?? new List<GroupStaffDetail>();

            if (!detailsInMonth.Any()) return (false, "Không có dữ liệu chấm công cho tháng này.");

            foreach (var detail in detailsInMonth)
            {
                detail.CalculatedSalary = CalculateDetailSalary(staff, detail);
            }

            await _context.SaveChangesAsync();
            return (true, $"Đã cập nhật lương cho {detailsInMonth.Count} công trong tháng {month}/{year}.");
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
            int count = 0;

            foreach (var staff in staffs)
            {
                var details = staff.GroupStaffDetails?
                    .Where(gsd => gsd.MedicalGroup != null && gsd.MedicalGroup.ExamDate.Date >= monthStart.Date && gsd.MedicalGroup.ExamDate.Date <= monthEnd.Date)
                    .ToList();

                if (details == null || !details.Any()) continue;

                foreach (var detail in details)
                {
                    detail.CalculatedSalary = CalculateDetailSalary(staff, detail);
                    count++;
                }
            }

            if (count == 0) return (false, "Không tìm thấy dữ liệu chấm công nào để tính toán trong tháng này.");

            await _context.SaveChangesAsync();
            return (true, $"Đã tính toán thành công lương cho toàn diện nhân sự. Tổng cộng {count} lượt công đã được xử lý.");
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
