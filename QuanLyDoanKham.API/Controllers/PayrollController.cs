using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,PayrollManager")]
    public class PayrollController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PayrollController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Payroll/monthly?month=2&year=2026
        [HttpGet("monthly")]
        public async Task<ActionResult<IEnumerable<PayrollSummaryDto>>> GetMonthlyPayroll(int month, int year)
        {
            var staffs = await _context.Staffs
                .Where(s => s.IsActive)
                .ToListAsync();

            var result = new List<PayrollSummaryDto>();

            foreach (var staff in staffs)
            {
                var details = await _context.GroupStaffDetails
                    .Include(gsd => gsd.MedicalGroup)
                    .Where(gsd => gsd.StaffId == staff.StaffId && 
                                  gsd.MedicalGroup.ExamDate.Month == month && 
                                  gsd.MedicalGroup.ExamDate.Year == year &&
                                  gsd.WorkStatus == "Đã tham gia") // CHỈ TÍNH LƯƠNG NHỮNG NGƯỜI THỰC SỰ ĐI LÀM
                    .Select(gsd => new PayrollDetailDto
                    {
                        GroupId = gsd.GroupId,
                        GroupName = gsd.MedicalGroup.GroupName,
                        ExamDate = gsd.MedicalGroup.ExamDate,
                        ShiftType = gsd.ShiftType,
                        CalculatedSalary = gsd.CalculatedSalary
                    })
                    .ToListAsync();

                var groupEarnings = details.Sum(d => d.CalculatedSalary);
                var totalDays = details.Count; 
                
                result.Add(new PayrollSummaryDto
                {
                    StaffId = staff.StaffId,
                    FullName = staff.FullName,
                    EmployeeCode = staff.EmployeeCode,
                    JobTitle = staff.JobTitle,
                    BankAccountNumber = staff.BankAccountNumber,
                    BankName = staff.BankName,
                    BankAccountName = staff.BankAccountName,
                    BaseSalary = staff.BaseSalary,
                    GroupEarnings = groupEarnings,
                    TotalSalary = staff.BaseSalary + groupEarnings,
                    TotalDays = totalDays, 
                    Details = details
                });
            }

            return result;
        }

        // POST: api/Payroll/confirm
        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmPayroll(int month, int year)
        {
            // Logic to mark as finished or save records
            return Ok(new { message = $"Đã chốt lương tháng {month}/{year} thành công!" });
        }

        // GET: api/Payroll/export-monthly
        [HttpGet("export-monthly")]
        public async Task<IActionResult> ExportMonthlyPayroll(int month, int year)
        {
            var payroll = await GetMonthlyPayrollData(month, year);

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add($"Luong_Th{month}_{year}");
                var currentRow = 1;

                // Header
                var title = worksheet.Cell(currentRow, 1);
                title.Value = $"BẢNG LƯƠNG NHÂN VIÊN - THÁNG {month}/{year}";
                title.Style.Font.Bold = true;
                title.Style.Font.FontSize = 16;
                worksheet.Range(currentRow, 1, currentRow, 7).Merge().Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                currentRow += 2;

                // Table Header
                worksheet.Cell(currentRow, 1).Value = "Mã NV";
                worksheet.Cell(currentRow, 2).Value = "Họ và tên";
                worksheet.Cell(currentRow, 3).Value = "Chức vụ";
                worksheet.Cell(currentRow, 4).Value = "Lương định mức";
                worksheet.Cell(currentRow, 5).Value = "Số ngày đi khám";
                worksheet.Cell(currentRow, 6).Value = "Thù lao đoàn";
                worksheet.Cell(currentRow, 7).Value = "Thực lĩnh (NET)";

                var headerRange = worksheet.Range(currentRow, 1, currentRow, 7);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
                headerRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;

                foreach (var item in payroll)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item.EmployeeCode;
                    worksheet.Cell(currentRow, 2).Value = item.FullName;
                    worksheet.Cell(currentRow, 3).Value = item.JobTitle;
                    worksheet.Cell(currentRow, 4).Value = item.BaseSalary;
                    worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "#,##0 \"₫\"";
                    worksheet.Cell(currentRow, 5).Value = item.TotalDays;
                    worksheet.Cell(currentRow, 6).Value = item.GroupEarnings;
                    worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "#,##0 \"₫\"";
                    worksheet.Cell(currentRow, 7).Value = item.TotalSalary;
                    worksheet.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0 \"₫\"";
                    worksheet.Cell(currentRow, 7).Style.Font.Bold = true;

                    worksheet.Range(currentRow, 1, currentRow, 7).Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new System.IO.MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BangLuong_{month}_{year}.xlsx");
                }
            }
        }

        private async Task<List<PayrollSummaryDto>> GetMonthlyPayrollData(int month, int year)
        {
            var staffs = await _context.Staffs.Where(s => s.IsActive).ToListAsync();
            var result = new List<PayrollSummaryDto>();

            foreach (var staff in staffs)
            {
                var details = await _context.GroupStaffDetails
                    .Include(gsd => gsd.MedicalGroup)
                    .Where(gsd => gsd.StaffId == staff.StaffId && 
                                  gsd.MedicalGroup.ExamDate.Month == month && 
                                  gsd.MedicalGroup.ExamDate.Year == year &&
                                  gsd.WorkStatus == "Đã tham gia") // CHỈ XUẤT EXCEL CHO NHỮNG DÒNG HỢP LỆ
                    .Select(gsd => new PayrollDetailDto
                    {
                        GroupId = gsd.GroupId,
                        GroupName = gsd.MedicalGroup.GroupName,
                        ExamDate = gsd.MedicalGroup.ExamDate,
                        ShiftType = gsd.ShiftType,
                        CalculatedSalary = gsd.CalculatedSalary
                    })
                    .ToListAsync();

                var groupEarnings = details.Sum(d => d.CalculatedSalary);
                var totalDays = details.Count;
                
                result.Add(new PayrollSummaryDto
                {
                    StaffId = staff.StaffId,
                    FullName = staff.FullName,
                    EmployeeCode = staff.EmployeeCode,
                    JobTitle = staff.JobTitle,
                    BaseSalary = staff.BaseSalary,
                    GroupEarnings = groupEarnings,
                    TotalSalary = staff.BaseSalary + groupEarnings,
                    TotalDays = totalDays
                });
            }
            return result;
        }
    }
}
