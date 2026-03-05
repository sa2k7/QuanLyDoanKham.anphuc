using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;

namespace QuanLyDoanKham.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,ContractManager,PayrollManager")] // Admin và các role quản lý liên quan được xem báo cáo
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try {
                var stats = await CalculateStats();
                return Ok(stats);
            } catch (Exception ex) {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("export-dashboard")]
        public async Task<IActionResult> ExportDashboard()
        {
            var stats = await CalculateStats();

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Tong quan Tai chinh");
                var currentRow = 1;

                // Header
                worksheet.Cell(currentRow, 1).Value = "BÁO CÁO TỔNG QUAN HIỆU QUẢ VẬN HÀNH";
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 1).Style.Font.FontSize = 16;
                worksheet.Range(currentRow, 1, currentRow, 2).Merge();
                currentRow += 2;

                // Key metrics
                worksheet.Cell(currentRow, 1).Value = "Tổng Doanh thu";
                worksheet.Cell(currentRow, 2).Value = stats.totalRevenue;
                worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "#,##0 \"₫\"";
                currentRow++;

                worksheet.Cell(currentRow, 1).Value = "Chi phí Lương";
                worksheet.Cell(currentRow, 2).Value = stats.totalSalaryCost;
                worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "#,##0 \"₫\"";
                currentRow++;

                worksheet.Cell(currentRow, 1).Value = "Chi phí Vật tư";
                worksheet.Cell(currentRow, 2).Value = stats.totalSupplyCost;
                worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "#,##0 \"₫\"";
                currentRow++;

                worksheet.Cell(currentRow, 1).Value = "Lợi nhuận ròng";
                worksheet.Cell(currentRow, 2).Value = stats.netProfit;
                worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "#,##0 \"₫\"";
                worksheet.Cell(currentRow, 2).Style.Font.Bold = true;
                currentRow += 2;

                // Groups sheet
                var groupSheet = workbook.Worksheets.Add("Hieu qua Doan kham");
                var gRow = 1;
                groupSheet.Cell(gRow, 1).Value = "Đoàn khám";
                groupSheet.Cell(gRow, 2).Value = "Công ty";
                groupSheet.Cell(gRow, 3).Value = "Số nhân sự";
                groupSheet.Cell(gRow, 4).Value = "Tổng chi phí";
                groupSheet.Range(gRow, 1, gRow, 4).Style.Font.Bold = true;

                foreach (var g in stats.groupStats)
                {
                    gRow++;
                    groupSheet.Cell(gRow, 1).Value = g.GroupName;
                    groupSheet.Cell(gRow, 2).Value = g.CompanyName;
                    groupSheet.Cell(gRow, 3).Value = g.StaffCount;
                    groupSheet.Cell(gRow, 4).Value = g.TotalCost;
                    groupSheet.Cell(gRow, 4).Style.NumberFormat.Format = "#,##0 \"₫\"";
                }

                worksheet.Columns().AdjustToContents();
                groupSheet.Columns().AdjustToContents();

                using (var stream = new System.IO.MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BaoCao_HeThong.xlsx");
                }
            }
        }

        private async Task<dynamic> CalculateStats()
        {
            var totalContracts = await _context.Contracts.CountAsync();
            var totalRevenue = await _context.Contracts.SumAsync(c => c.TotalAmount);
            var activeGroups = await _context.MedicalGroups.CountAsync();
            
            var staffParticipating = await _context.GroupStaffDetails.Select(d => d.StaffId).Distinct().CountAsync();
            var suppliesExported = await _context.GroupSupplyDetails.SumAsync(d => d.QuantityUsed);

            var totalSalaryCost = await _context.GroupStaffDetails.SumAsync(d => d.CalculatedSalary);
            var supplyCost = await _context.GroupSupplyDetails
                .Include(d => d.Supply)
                .Where(d => !d.Supply.IsFixedAsset)
                .SumAsync(d => (d.QuantityUsed - d.ReturnQuantity) * d.Supply.UnitPrice);

            var profit = totalRevenue - totalSalaryCost - supplyCost;

            var topSupplies = await _context.GroupSupplyDetails
                .Include(d => d.Supply)
                .GroupBy(d => d.Supply.SupplyName)
                .Select(g => new { 
                    Name = g.Key, 
                    Qty = g.Sum(x => x.QuantityUsed),
                    Cost = g.Sum(x => x.QuantityUsed * x.Supply.UnitPrice)
                })
                .OrderByDescending(x => x.Qty)
                .Take(5)
                .ToListAsync();

            var groupStats = await _context.MedicalGroups
                .Include(g => g.HealthContract)
                .ThenInclude(c => c.Company)
                .Select(g => new {
                    g.GroupName,
                    CompanyName = g.HealthContract.Company.CompanyName,
                    StaffCount = _context.GroupStaffDetails.Count(d => d.GroupId == g.GroupId),
                    TotalCost = _context.GroupStaffDetails.Where(d => d.GroupId == g.GroupId).Sum(d => d.CalculatedSalary) +
                                _context.GroupSupplyDetails.Where(d => d.GroupId == g.GroupId && !d.Supply.IsFixedAsset)
                                    .Sum(d => (d.QuantityUsed - d.ReturnQuantity) * d.Supply.UnitPrice)
                })
                .OrderByDescending(g => g.TotalCost)
                .ToListAsync();

            var lowStock = await _context.Supplies
                .Where(s => !s.IsFixedAsset && s.StockQuantity < 20)
                .Select(s => new { s.SupplyName, s.StockQuantity })
                .ToListAsync();

            var topStaff = await _context.GroupStaffDetails
                .Include(d => d.Staff)
                .GroupBy(d => d.Staff.FullName)
                .Select(g => new { Name = g.Key, Sessions = g.Count() })
                .OrderByDescending(x => x.Sessions)
                .Take(5)
                .ToListAsync();

            return new
            {
                totalContracts,
                totalRevenue,
                totalSalaryCost,
                totalSupplyCost = supplyCost,
                netProfit = profit,
                activeGroups,
                staffParticipating,
                suppliesExported,
                topSupplies,
                groupStats,
                lowStock,
                topStaff
            };
        }
    }
}
