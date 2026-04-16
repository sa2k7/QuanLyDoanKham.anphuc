using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDoanKham.API.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuanLyDoanKham.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SettlementController : ControllerBase
    {
        private readonly IReportingServiceEnhanced _reportingService;

        public SettlementController(IReportingServiceEnhanced reportingService)
        {
            _reportingService = reportingService;
        }

        /// <summary>
        /// Lấy chi tiết quyết toán cho một hợp đồng cụ thể
        /// </summary>
        [HttpGet("detail/{healthContractId}")]
        public async Task<IActionResult> GetSettlementDetail(int healthContractId)
        {
            try
            {
                var result = await _reportingService.GetSettlementDetailAsync(healthContractId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy thống kê tổng hợp (Master Stats) cho toàn bộ hệ thống
        /// </summary>
        [HttpGet("master-stats")]
        public async Task<IActionResult> GetMasterStats([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var result = await _reportingService.GetMasterStatsAsync(startDate, endDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách các ca khám "lệch" (Outliers) để đối soát
        /// </summary>
        [HttpGet("reconciliation-list")]
        public async Task<IActionResult> GetReconciliationList([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var result = await _reportingService.GetReconciliationListAsync(startDate, endDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
