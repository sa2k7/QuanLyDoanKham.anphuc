using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDoanKham.API.Authorization;
using QuanLyDoanKham.API.Services;

namespace QuanLyDoanKham.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportingService _reportingService;
        private readonly QuanLyDoanKham.API.Services.Reports.IReportExportService _exportService;

        public ReportsController(IReportingService reportingService, QuanLyDoanKham.API.Services.Reports.IReportExportService exportService)
        {
            _reportingService = reportingService;
            _exportService = exportService;
        }

        [HttpGet("export-pdf")]
        [AuthorizePermission("BaoCao.View")]
        public async Task<IActionResult> ExportPdf([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var pdfBytes = await _exportService.GenerateDashboardPdfAsync(startDate, endDate);
            var fileName = $"BaoCao_HeThong_{DateTime.Now:yyyyMMdd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        [HttpGet("export-excel")]
        [AuthorizePermission("BaoCao.View")]
        public async Task<IActionResult> ExportExcel([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var excelBytes = await _exportService.GenerateDashboardExcelAsync(startDate, endDate);
            var fileName = $"BaoCao_HeThong_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // Dashboard KPIs - Dùng cho trang Tổng quan (Cho phép tất cả Managers xem chỉ số chung)
        [HttpGet("dashboard-kpis")]
        [AuthorizePermission("BaoCao.View")]
        public async Task<IActionResult> GetDashboardKpis([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var kpis = await _reportingService.GetDashboardKpisAsync(startDate, endDate);
                return Ok(kpis);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy dữ liệu KPI báo cáo", error = ex.Message });
            }
        }

        // Báo cáo Tài chính - Dùng cho Admin và PayrollManager/ContractManager
        [HttpGet("financial")]
        [AuthorizePermission("BaoCao.View")]
        public async Task<IActionResult> GetFinancialReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var report = await _reportingService.GetFinancialReportAsync(startDate, endDate);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy báo cáo tài chính", error = ex.Message });
            }
        }

        // Hiệu suất Nhân sự - Dùng cho Admin và PersonnelManager
        [HttpGet("staff-efficiency")]
        [AuthorizePermission("BaoCao.View")]
        public async Task<IActionResult> GetStaffEfficiency([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var efficiency = await _reportingService.GetStaffEfficiencyAsync(startDate, endDate);
                return Ok(efficiency);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy dữ liệu hiệu suất nhân sự", error = ex.Message });
            }
        }

        // Báo cáo Vận hành
        [HttpGet("operational-summary")]
        [AuthorizePermission("BaoCao.View")]
        public async Task<IActionResult> GetOperationalSummary([FromQuery] int? year, [FromQuery] int? month)
        {
            try
            {
                var summary = await _reportingService.GetOperationalSummaryAsync(year, month);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy dữ liệu báo cáo vận hành", error = ex.Message });
            }
        }

        // Báo cáo Lương tổng hợp
        [HttpGet("payroll-summary")]
        [AuthorizePermission("Luong.View")] // Chỉ Kế toán hoặc Admin
        public async Task<IActionResult> GetPayrollSummary([FromQuery] int? year, [FromQuery] int? month)
        {
            try
            {
                var summary = await _reportingService.GetPayrollSummaryAsync(year, month);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy bảng lương tổng hợp", error = ex.Message });
            }
        }
        // P&L theo hợp đồng
        [HttpGet("pnl/contract/{contractId}")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("BaoCao.View")]
        public async Task<IActionResult> GetContractPnL(int contractId, [FromServices] QuanLyDoanKham.API.Services.Reports.FinancialReportService finSvc)
        {
            var data = await finSvc.BuildContractSummaryAsync(contractId);
            return Ok(data);
        }
    }
}
