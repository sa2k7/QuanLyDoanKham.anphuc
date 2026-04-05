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

        public ReportsController(IReportingService reportingService)
        {
            _reportingService = reportingService;
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

        // Cảnh báo Kho - Dùng cho Admin và WarehouseManager/MedicalGroupManager
        [HttpGet("inventory-alerts")]
        [AuthorizePermission("BaoCao.View")]
        public async Task<IActionResult> GetInventoryAlerts()
        {
            try
            {
                var alerts = await _reportingService.GetInventoryAlertsAsync();
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy thông tin cảnh báo kho", error = ex.Message });
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
