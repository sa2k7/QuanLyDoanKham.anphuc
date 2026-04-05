using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;
using System.Security.Claims;

namespace QuanLyDoanKham.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PayrollController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PayrollController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================================================================
        // GET api/Payroll/monthly?month=4&year=2026
        // Tính lương theo công thức chuẩn:
        //   Lương theo tháng: (BaseSalary / StandardWorkDays) × ActualDays
        //   Lương theo ngày:  DailyRate × ActualDays
        // ================================================================
        [HttpGet("monthly")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("Luong.View")]
        public async Task<ActionResult<IEnumerable<PayrollSummaryDto>>> GetMonthlyPayroll(
            [FromQuery] int month, [FromQuery] int year)
        {
            if (month == 0) month = DateTime.Now.Month;
            if (year == 0) year = DateTime.Now.Year;

            var staffs = await _context.Staffs
                .Where(s => s.IsActive)
                .OrderBy(s => s.FullName)
                .ToListAsync();

            // Lấy toàn bộ chấm công tháng này — 1 query
            var allAttendance = await _context.ScheduleCalendars
                .Include(sc => sc.MedicalGroup)
                .Where(sc => sc.ExamDate.Month == month && sc.ExamDate.Year == year && sc.IsConfirmed)
                .ToListAsync();

            // Lấy GroupStaffDetails tháng này
            var allGroupDetails = await _context.GroupStaffDetails
                .Include(gd => gd.MedicalGroup)
                .Where(gd => gd.MedicalGroup.ExamDate.Month == month && gd.MedicalGroup.ExamDate.Year == year)
                .ToListAsync();

            var result = new List<PayrollSummaryDto>();

            foreach (var staff in staffs)
            {
                // ── Tính công thực tế từ bảng chấm công ──────────────────────
                var attendances = allAttendance.Where(sc => sc.StaffId == staff.StaffId).ToList();
                var groupDetails = allGroupDetails.Where(gd => gd.StaffId == staff.StaffId).ToList();

                // Tổng công: ưu tiên từ chấm công QR, fallback về ShiftType trong GroupStaffDetail
                double totalActualDays = 0;
                var payrollDetails = new List<PayrollDetailDto>();

                // Nhóm theo GroupId để tránh tính trùng
                var groupIds = groupDetails.Select(gd => gd.GroupId).Distinct().ToList();

                foreach (var gid in groupIds)
                {
                    var gd = groupDetails.First(g => g.GroupId == gid);
                    var attRecord = attendances.FirstOrDefault(a => a.GroupId == gid);

                    // ShiftType: từ chấm công QR nếu đã confirmed, không thì từ GroupStaffDetail
                    double shiftType = attRecord?.IsConfirmed == true
                        ? (gd.ShiftType > 0 ? gd.ShiftType : 1.0)
                        : (gd.WorkStatus == "Đã tham gia" ? gd.ShiftType : 0);

                    if (shiftType <= 0) continue;

                    totalActualDays += shiftType;

                    // Tính lương từng buổi
                    decimal shiftSalary;
                    if (staff.SalaryType == "Daily" && staff.DailyRate > 0)
                        shiftSalary = staff.DailyRate * (decimal)shiftType;
                    else
                    {
                        int stdDays = staff.StandardWorkDays > 0 ? staff.StandardWorkDays : 26;
                        shiftSalary = (staff.BaseSalary / stdDays) * (decimal)shiftType;
                    }

                    payrollDetails.Add(new PayrollDetailDto
                    {
                        GroupId = gid,
                        GroupName = gd.MedicalGroup?.GroupName ?? $"Đoàn #{gid}",
                        ExamDate = gd.MedicalGroup?.ExamDate ?? DateTime.Today,
                        ShiftType = shiftType,
                        CalculatedSalary = shiftSalary
                    });
                }

                // ── Tính tổng lương ───────────────────────────────────────────
                decimal calculatedTotal;
                if (staff.SalaryType == "Daily" && staff.DailyRate > 0)
                    calculatedTotal = staff.DailyRate * (decimal)totalActualDays;
                else
                {
                    int stdDays = staff.StandardWorkDays > 0 ? staff.StandardWorkDays : 26;
                    calculatedTotal = (staff.BaseSalary / stdDays) * (decimal)totalActualDays;
                }

                result.Add(new PayrollSummaryDto
                {
                    StaffId = staff.StaffId,
                    FullName = staff.FullName,
                    EmployeeCode = staff.EmployeeCode,
                    JobTitle = staff.JobTitle,
                    DepartmentName = staff.DepartmentName,
                    BankAccountNumber = staff.BankAccountNumber,
                    BankName = staff.BankName,
                    BankAccountName = staff.BankAccountName,
                    BaseSalary = staff.BaseSalary,
                    SalaryType = staff.SalaryType ?? "Monthly",
                    StandardWorkDays = staff.StandardWorkDays > 0 ? staff.StandardWorkDays : 26,
                    TotalActualDays = totalActualDays,
                    GroupEarnings = calculatedTotal,
                    TotalSalary = calculatedTotal,
                    Details = payrollDetails
                });
            }

            return Ok(result.Where(r => r.TotalActualDays > 0).ToList());
        }

        // ================================================================
        // GET api/Payroll/my-salary — Nhân viên xem lương cá nhân
        // ================================================================
        [HttpGet("my-salary")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("Luong.View")]
        public async Task<IActionResult> GetMySalary([FromQuery] int? month, [FromQuery] int? year)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            var staff = await _context.Staffs
                .FirstOrDefaultAsync(s => s.EmployeeCode.ToLower() == username.ToLower()
                    || s.FullName.ToLower() == username.ToLower());
            if (staff == null) return NotFound(new { message = "Không tìm thấy hồ sơ nhân sự." });

            int m = month ?? DateTime.Now.Month;
            int y = year ?? DateTime.Now.Year;

            var groupDetails = await _context.GroupStaffDetails
                .Include(gd => gd.MedicalGroup)
                .Where(gd => gd.StaffId == staff.StaffId
                    && gd.MedicalGroup.ExamDate.Month == m
                    && gd.MedicalGroup.ExamDate.Year == y
                    && gd.WorkStatus == "Đã tham gia")
                .ToListAsync();

            double totalActualDays = groupDetails.Sum(gd => gd.ShiftType);
            int stdDays = staff.StandardWorkDays > 0 ? staff.StandardWorkDays : 26;

            decimal calcTotal = staff.SalaryType == "Daily" && staff.DailyRate > 0
                ? staff.DailyRate * (decimal)totalActualDays
                : (staff.BaseSalary / stdDays) * (decimal)totalActualDays;

            var details = groupDetails.Select(gd => new PayrollDetailDto
            {
                GroupId = gd.GroupId,
                GroupName = gd.MedicalGroup?.GroupName ?? $"Đoàn #{gd.GroupId}",
                ExamDate = gd.MedicalGroup?.ExamDate ?? DateTime.Today,
                ShiftType = gd.ShiftType,
                CalculatedSalary = staff.SalaryType == "Daily"
                    ? staff.DailyRate * (decimal)gd.ShiftType
                    : (staff.BaseSalary / stdDays) * (decimal)gd.ShiftType
            }).ToList();

            return Ok(new PayrollSummaryDto
            {
                StaffId = staff.StaffId,
                FullName = staff.FullName,
                EmployeeCode = staff.EmployeeCode,
                JobTitle = staff.JobTitle,
                BaseSalary = staff.BaseSalary,
                SalaryType = staff.SalaryType ?? "Monthly",
                StandardWorkDays = stdDays,
                TotalActualDays = totalActualDays,
                GroupEarnings = calcTotal,
                TotalSalary = calcTotal,
                Details = details
            });
        }

        // ================================================================
        // POST api/Payroll/generate?month=4&year=2026 — Tạo bản ghi lương
        // ================================================================
        [HttpPost("generate")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("Luong.Manage")]
        public async Task<IActionResult> GeneratePayroll([FromQuery] int month, [FromQuery] int year)
        {
            var username = User.Identity?.Name ?? "system";
            var existing = await _context.PayrollRecords
                .Where(p => p.Month == month && p.Year == year && p.Status != "Cancelled")
                .ToListAsync();
            if (existing.Any())
                return BadRequest($"Đã có bảng lương tháng {month}/{year}. Vui lòng hủy trước.");

            var staffs = await _context.Staffs.Where(s => s.IsActive).ToListAsync();
            var allGroupDetails = await _context.GroupStaffDetails
                .Include(gd => gd.MedicalGroup)
                .Where(gd => gd.MedicalGroup.ExamDate.Month == month
                          && gd.MedicalGroup.ExamDate.Year == year
                          && gd.WorkStatus == "Đã tham gia")
                .ToListAsync();

            int count = 0;
            foreach (var staff in staffs)
            {
                var details = allGroupDetails.Where(gd => gd.StaffId == staff.StaffId).ToList();
                if (!details.Any()) continue;

                double totalDays = details.Sum(d => d.ShiftType);
                int stdDays = staff.StandardWorkDays > 0 ? staff.StandardWorkDays : 26;
                decimal dailyRate = staff.SalaryType == "Daily" && staff.DailyRate > 0
                    ? staff.DailyRate
                    : (staff.BaseSalary / stdDays);
                decimal totalAmount = dailyRate * (decimal)totalDays;

                _context.PayrollRecords.Add(new PayrollRecord
                {
                    StaffId = staff.StaffId,
                    Month = month,
                    Year = year,
                    BaseSalary = staff.BaseSalary,
                    DailyRate = dailyRate,
                    SalaryType = staff.SalaryType ?? "Monthly",
                    StandardWorkDays = stdDays,
                    TotalActualDays = totalDays,
                    TotalAmount = totalAmount,
                    Status = "Draft",
                    GeneratedBy = username,
                    GeneratedAt = DateTime.Now
                });
                count++;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Đã tạo bảng lương cho {count} nhân viên tháng {month}/{year}." });
        }

        // ================================================================
        // POST api/Payroll/confirm?month=4&year=2026 — Duyệt bảng lương
        // ================================================================
        [HttpPost("confirm")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("Luong.Manage")]
        public async Task<IActionResult> ConfirmPayroll([FromQuery] int month, [FromQuery] int year)
        {
            var records = await _context.PayrollRecords
                .Where(p => p.Month == month && p.Year == year && p.Status == "Draft")
                .ToListAsync();
            if (!records.Any()) return NotFound("Không có bảng lương Draft để duyệt.");

            foreach (var r in records) r.Status = "Confirmed";
            await _context.SaveChangesAsync();

            var username = User.Identity?.Name ?? "system";
            _context.AuditLogs.Add(new AuditLog
            {
                Action = "CONFIRM_PAYROLL",
                EntityType = "PayrollRecord",
                EntityId = 0,
                NewValue = $"Chốt lương tháng {month}/{year} - {records.Count} nhân viên",
                Timestamp = DateTime.Now,
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            });
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Đã duyệt bảng lương tháng {month}/{year} ({records.Count} nhân viên)." });
        }

        // ================================================================
        // GET api/Payroll/export-monthly?month=4&year=2026
        // ================================================================
        [HttpGet("export-monthly")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("Luong.View")]
        public async Task<IActionResult> ExportMonthlyPayroll([FromQuery] int month, [FromQuery] int year)
        {
            if (month == 0) month = DateTime.Now.Month;
            if (year == 0) year = DateTime.Now.Year;

            var payroll = await GetPayrollData(month, year);

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add($"Luong_T{month}_{year}");
            int row = 1;

            ws.Cell(row, 1).Value = $"BẢNG LƯƠNG NHÂN VIÊN - THÁNG {month}/{year}";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontSize = 14;
            ws.Range(row, 1, row, 8).Merge().Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            row += 2;

            string[] headers = ["Mã NV", "Họ và Tên", "Phòng ban", "Lương cơ bản", "Loại lương", "Công chuẩn", "Công thực tế", "Lương thực lĩnh"];
            for (int c = 1; c <= headers.Length; c++)
            {
                ws.Cell(row, c).Value = headers[c - 1];
                ws.Cell(row, c).Style.Font.Bold = true;
                ws.Cell(row, c).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightSteelBlue;
            }
            row++;

            foreach (var item in payroll)
            {
                ws.Cell(row, 1).Value = item.EmployeeCode;
                ws.Cell(row, 2).Value = item.FullName;
                ws.Cell(row, 3).Value = item.DepartmentName ?? "";
                ws.Cell(row, 4).Value = (double)item.BaseSalary;
                ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0";
                ws.Cell(row, 5).Value = item.SalaryType == "Daily" ? "Theo ngày" : "Theo tháng";
                ws.Cell(row, 6).Value = item.StandardWorkDays;
                ws.Cell(row, 7).Value = item.TotalActualDays;
                ws.Cell(row, 8).Value = (double)item.TotalSalary;
                ws.Cell(row, 8).Style.NumberFormat.Format = "#,##0";
                ws.Cell(row, 8).Style.Font.Bold = true;
                row++;
            }

            // Totals
            ws.Cell(row, 7).Value = "TỔNG";
            ws.Cell(row, 7).Style.Font.Bold = true;
            ws.Cell(row, 8).Value = (double)payroll.Sum(p => p.TotalSalary);
            ws.Cell(row, 8).Style.NumberFormat.Format = "#,##0";
            ws.Cell(row, 8).Style.Font.Bold = true;

            ws.Columns().AdjustToContents();

            using var stream = new System.IO.MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"BangLuong_T{month}_{year}.xlsx");
        }

        // ── Private helpers ───────────────────────────────────────────────
        private async Task<List<PayrollSummaryDto>> GetPayrollData(int month, int year)
        {
            // Ưu tiên dùng PayrollRecord đã generate; fallback tính real-time
            var records = await _context.PayrollRecords
                .Include(p => p.Staff)
                .Where(p => p.Month == month && p.Year == year && p.Status != "Cancelled")
                .ToListAsync();

            if (records.Any())
            {
                return records.Select(r => new PayrollSummaryDto
                {
                    StaffId = r.StaffId,
                    FullName = r.Staff?.FullName ?? "",
                    EmployeeCode = r.Staff?.EmployeeCode ?? "",
                    JobTitle = r.Staff?.JobTitle ?? "",
                    DepartmentName = r.Staff?.DepartmentName,
                    BankAccountNumber = r.Staff?.BankAccountNumber,
                    BankName = r.Staff?.BankName,
                    BankAccountName = r.Staff?.BankAccountName,
                    BaseSalary = r.BaseSalary,
                    SalaryType = r.SalaryType ?? "Monthly",
                    StandardWorkDays = r.StandardWorkDays,
                    TotalActualDays = r.TotalActualDays,
                    GroupEarnings = r.TotalAmount,
                    TotalSalary = r.TotalAmount
                }).ToList();
            }

            // Real-time fallback
            var staffs = await _context.Staffs.Where(s => s.IsActive).ToListAsync();
            var result = new List<PayrollSummaryDto>();
            foreach (var staff in staffs)
            {
                var details = await _context.GroupStaffDetails
                    .Include(gd => gd.MedicalGroup)
                    .Where(gd => gd.StaffId == staff.StaffId
                              && gd.MedicalGroup.ExamDate.Month == month
                              && gd.MedicalGroup.ExamDate.Year == year
                              && gd.WorkStatus == "Đã tham gia")
                    .ToListAsync();

                if (!details.Any()) continue;

                double totalDays = details.Sum(d => d.ShiftType);
                int stdDays = staff.StandardWorkDays > 0 ? staff.StandardWorkDays : 26;
                decimal calcTotal = staff.SalaryType == "Daily"
                    ? staff.DailyRate * (decimal)totalDays
                    : (staff.BaseSalary / stdDays) * (decimal)totalDays;

                result.Add(new PayrollSummaryDto
                {
                    StaffId = staff.StaffId,
                    FullName = staff.FullName,
                    EmployeeCode = staff.EmployeeCode,
                    JobTitle = staff.JobTitle,
                    DepartmentName = staff.DepartmentName,
                    BaseSalary = staff.BaseSalary,
                    SalaryType = staff.SalaryType ?? "Monthly",
                    StandardWorkDays = stdDays,
                    TotalActualDays = totalDays,
                    GroupEarnings = calcTotal,
                    TotalSalary = calcTotal
                });
            }
            return result;
        }
    }
}
