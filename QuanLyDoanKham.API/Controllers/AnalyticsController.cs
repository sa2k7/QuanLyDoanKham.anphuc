using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDoanKham.API.Authorization;
using QuanLyDoanKham.API.Services;
using QuanLyDoanKham.API.Services.Reports;
using System;
using System.Threading.Tasks;

namespace QuanLyDoanKham.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AnalyticsController : ControllerBase
    {
        private readonly IGroupAnalyticsService _analyticsService;
        private readonly IAnalyticsService _reportAnalytics;

        public AnalyticsController(
            IGroupAnalyticsService analyticsService,
            IAnalyticsService reportAnalytics)
        {
            _analyticsService = analyticsService;
            _reportAnalytics = reportAnalytics;
        }

        // ==================== EXISTING ENDPOINTS ====================

        /// <summary>GET api/Analytics/groups — Danh sách tóm tắt tất cả đoàn khám</summary>
        [HttpGet("groups")]
        [AuthorizePermission("BaoCao.View")]
        public async Task<IActionResult> GetGroupSummaries(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var result = await _analyticsService.GetAllGroupSummariesAsync(from, to);
            return Ok(result);
        }

        /// <summary>GET api/Analytics/groups/{id} — Phân tích chi tiết một đoàn khám</summary>
        [HttpGet("groups/{groupId}")]
        [AuthorizePermission("BaoCao.View")]
        public async Task<IActionResult> GetGroupAnalytics(int groupId)
        {
            var result = await _analyticsService.GetGroupAnalyticsAsync(groupId);
            if (result == null) return NotFound(new { message = "Không tìm thấy đoàn khám." });
            return Ok(result);
        }

        // ==================== NEW DASHBOARD ENDPOINTS ====================

        /// <summary>GET api/Analytics/dashboard — Tổng quan dashboard</summary>
        [HttpGet("dashboard")]
        [AuthorizePermission("BaoCao.View")]
        public async Task<IActionResult> GetDashboard(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var result = await _reportAnalytics.GetDashboardSummaryAsync(from, to);
            return Ok(result);
        }

        /// <summary>GET api/Analytics/contracts — Phân tích hợp đồng</summary>
        [HttpGet("contracts")]
        [AuthorizePermission("BaoCao.View")]
        public async Task<IActionResult> GetContractAnalytics(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var startDate = from?.Date ?? DateTime.Today.AddMonths(-6);
            var endDate = to?.Date ?? DateTime.Today;
            var result = await _reportAnalytics.GetContractAnalyticsAsync(startDate, endDate);
            return Ok(result);
        }

        /// <summary>GET api/Analytics/medical-groups — Phân tích đoàn khám</summary>
        [HttpGet("medical-groups")]
        [AuthorizePermission("BaoCao.View")]
        public async Task<IActionResult> GetMedicalGroupAnalytics(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var startDate = from?.Date ?? DateTime.Today.AddMonths(-6);
            var endDate = to?.Date ?? DateTime.Today;
            var result = await _reportAnalytics.GetMedicalGroupAnalyticsAsync(startDate, endDate);
            return Ok(result);
        }

        /// <summary>GET api/Analytics/staff — Phân tích nhân sự</summary>
        [HttpGet("staff")]
        [AuthorizePermission("BaoCao.View")]
        public async Task<IActionResult> GetStaffAnalytics(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var startDate = from?.Date ?? DateTime.Today.AddMonths(-1);
            var endDate = to?.Date ?? DateTime.Today;
            var result = await _reportAnalytics.GetStaffAnalyticsAsync(startDate, endDate);
            return Ok(result);
        }

        /// <summary>GET api/Analytics/financial — Phân tích tài chính</summary>
        [HttpGet("financial")]
        [AuthorizePermission("BaoCao.ViewFinance")]
        public async Task<IActionResult> GetFinancialAnalytics(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var startDate = from?.Date ?? DateTime.Today.AddMonths(-6);
            var endDate = to?.Date ?? DateTime.Today;
            var result = await _reportAnalytics.GetFinancialAnalyticsAsync(startDate, endDate);
            return Ok(result);
        }

        /// <summary>GET api/Analytics/top-companies — Top công ty theo doanh thu</summary>
        [HttpGet("top-companies")]
        [AuthorizePermission("BaoCao.View")]
        public async Task<IActionResult> GetTopCompanies(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int top = 10)
        {
            var result = await _reportAnalytics.GetTopCompaniesAsync(top, from, to);
            return Ok(new { top, companies = result });
        }

        /// <summary>GET api/Analytics/kpi-summary — KPI Summary cho dashboard chính</summary>
        [HttpGet("kpi-summary")]
        [AuthorizePermission("BaoCao.View")]
        public async Task<IActionResult> GetKpiSummary()
        {
            var today = DateTime.Today;
            var thisMonth = new DateTime(today.Year, today.Month, 1);
            var lastMonth = thisMonth.AddMonths(-1);

            var thisMonthData = await _reportAnalytics.GetDashboardSummaryAsync(thisMonth, today);
            var lastMonthData = await _reportAnalytics.GetDashboardSummaryAsync(lastMonth, lastMonth.AddMonths(1).AddDays(-1));

            return Ok(new
            {
                thisMonth = thisMonthData,
                lastMonth = lastMonthData,
                comparison = new
                {
                    revenueChange = thisMonthData.Financial.Revenue - lastMonthData.Financial.Revenue,
                    revenueChangePercent = lastMonthData.Financial.Revenue > 0
                        ? ((thisMonthData.Financial.Revenue - lastMonthData.Financial.Revenue) / lastMonthData.Financial.Revenue) * 100
                        : 0,
                    profitChange = thisMonthData.Financial.Profit - lastMonthData.Financial.Profit,
                    contractChange = thisMonthData.Contracts.NewInPeriod - lastMonthData.Contracts.NewInPeriod,
                    groupChange = thisMonthData.MedicalGroups.InPeriod - lastMonthData.MedicalGroups.InPeriod
                }
            });
        }

        /// <summary>GET api/Analytics/group-pnl/{groupId} — P&amp;L chi tiết của một đoàn khám</summary>
        [HttpGet("group-pnl/{groupId:int}")]
        [AuthorizePermission("BaoCao.ViewFinance")]
        public async Task<IActionResult> GetGroupPnl(int groupId)
        {
            var pnl = await _analyticsService.GetGroupPnlAsync(groupId);
            if (pnl == null) return NotFound(new { message = "Không tìm thấy đoàn khám." });
            return Ok(pnl);
        }

        /// <summary>GET api/Analytics/period-summary?from=&amp;to= — Tổng hợp P&amp;L theo kỳ</summary>
        [HttpGet("period-summary")]
        [AuthorizePermission("BaoCao.View")]
        public async Task<IActionResult> GetPeriodSummary(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var fromDate = from?.Date ?? DateTime.Today.AddMonths(-1);
            var toDate   = to?.Date   ?? DateTime.Today;
            var result = await _analyticsService.GetPeriodSummaryAsync(fromDate, toDate);
            return Ok(result);
        }
    }
}
