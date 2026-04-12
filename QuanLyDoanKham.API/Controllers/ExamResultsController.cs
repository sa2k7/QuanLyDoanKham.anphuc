using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Services.MedicalRecords;
using System.Security.Claims;

namespace QuanLyDoanKham.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ExamResultsController : ControllerBase
    {
        private readonly IExamService _examService;

        public ExamResultsController(IExamService examService)
        {
            _examService = examService;
        }

        /// <summary>
        /// POST api/ExamResults/save — Lưu kết quả khám cho một trạm/mục khám
        /// </summary>
        [HttpPost("save")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("KetQua.Write")]
        public async Task<IActionResult> SaveResult([FromBody] SaveExamResultDto dto)
        {
            var userId = User.FindFirst("UserId")?.Value ?? "0";
            var result = await _examService.SaveExamResultAsync(dto, userId);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = "Lưu kết quả thành công." });
        }

        /// <summary>
        /// GET api/ExamResults/record/{recordId} — Lấy danh sách kết quả của hồ sơ
        /// </summary>
        [HttpGet("record/{recordId}")]
        public async Task<IActionResult> GetResults(int recordId)
        {
            var result = await _examService.GetResultsByRecordIdAsync(recordId);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Message });

            return Ok(result.Data);
        }
    }
}
