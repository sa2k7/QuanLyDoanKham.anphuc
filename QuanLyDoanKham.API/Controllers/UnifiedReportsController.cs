using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDoanKham.API.Authorization;
using QuanLyDoanKham.API.Services.Reports;

namespace QuanLyDoanKham.API.Controllers
{
    /// <summary>Controller thống kê tổng hợp - Unified Reports</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UnifiedReportsController : ControllerBase
    {
        private readonly IUnifiedReportService _reportService;
        private readonly IReportExportService _exportService;

        public UnifiedReportsController(
            IUnifiedReportService reportService,
            IReportExportService exportService)
        {
            _reportService = reportService;
            _exportService = exportService;
        }

        /// <summary>Lấy dashboard tổng quan</summary>
        [HttpGet("dashboard")]
        [AuthorizePermission("BaoCao.View")]
        public async Task<IActionResult> GetDashboard(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var data = await _reportService.GetUnifiedDashboardAsync(from, to);
            return Ok(data);
        }

        /// <summary>Lấy biểu đồ doanh thu</summary>
        [HttpGet("charts/revenue")]
        [AuthorizePermission("BaoCao.View")]
        public async Task<IActionResult> GetRevenueChart(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            var data = await _reportService.GetRevenueChartAsync(from, to);
            return Ok(data);
        }

        /// <summary>Lấy biểu đồ hợp đồng</summary>
        [HttpGet("charts/contracts")]
        [AuthorizePermission("BaoCao.View")]
        public async Task<IActionResult> GetContractChart(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            var data = await _reportService.GetContractChartAsync(from, to);
            return Ok(data);
        }

        /// <summary>Lấy biểu đồ đoàn khám</summary>
        [HttpGet("charts/medical-groups")]
        [AuthorizePermission("BaoCao.View")]
        public async Task<IActionResult> GetMedicalGroupChart(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            var data = await _reportService.GetMedicalGroupChartAsync(from, to);
            return Ok(data);
        }

        /// <summary>Lấy top thống kê (công ty, nhân sự, đoàn khám)</summary>
        [HttpGet("top-stats")]
        [AuthorizePermission("BaoCao.View")]
        public async Task<IActionResult> GetTopStats(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var data = await _reportService.GetTopStatsAsync(from, to);
            return Ok(data);
        }

        /// <summary>Lấy danh sách vật tư sắp hết</summary>
        [HttpGet("low-stock")]
        [AuthorizePermission("Kho.View")]
        public async Task<IActionResult> GetLowStockItems()
        {
            var data = await _reportService.GetLowStockItemsAsync();
            return Ok(data);
        }

        /// <summary>Xuất báo cáo PDF</summary>
        [HttpGet("export/pdf")]
        [AuthorizePermission("BaoCao.View")]
        public async Task<IActionResult> ExportPdf(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var pdfBytes = await _exportService.GenerateDashboardPdfAsync(from, to);
            var fileName = $"BaoCao_ThongKe_{DateTime.Now:yyyyMMdd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        /// <summary>Xuất báo cáo Excel</summary>
        [HttpGet("export/excel")]
        [AuthorizePermission("BaoCao.View")]
        public async Task<IActionResult> ExportExcel(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var excelBytes = await _exportService.GenerateDashboardExcelAsync(from, to);
            var fileName = $"BaoCao_ThongKe_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
