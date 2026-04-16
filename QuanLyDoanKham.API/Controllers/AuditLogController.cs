using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDoanKham.API.Authorization;
using QuanLyDoanKham.API.Services.Auth;
using System;
using System.Threading.Tasks;

namespace QuanLyDoanKham.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        /// <summary>
        /// Lấy danh sách nhật ký thao tác hệ thống
        /// </summary>
        [HttpGet]
        [AuthorizePermission("HeThong.AuditLog")]
        public async Task<IActionResult> GetLogs([FromQuery] DateTime? start, [FromQuery] DateTime? end, [FromQuery] string? module, [FromQuery] string? username)
        {
            try
            {
                var logs = await _auditLogService.GetLogsAsync(start, end, module, username);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
