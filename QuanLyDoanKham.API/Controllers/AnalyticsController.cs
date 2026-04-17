using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDoanKham.API.Authorization;
using QuanLyDoanKham.API.Services;
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

        public AnalyticsController(IGroupAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

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
    }
}
