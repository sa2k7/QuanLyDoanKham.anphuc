using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDoanKham.API.Services.MedicalRecords;

namespace QuanLyDoanKham.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CheckInController : ControllerBase
    {
        private readonly ICheckInService _checkInService;

        public CheckInController(ICheckInService checkInService)
        {
            _checkInService = checkInService;
        }

        [HttpPost("scan")]
        public async Task<IActionResult> ScanQr([FromBody] CheckInRequestDto request)
        {
            if (string.IsNullOrEmpty(request.Token))
                return BadRequest(new { message = "Token không được để trống." });

            var actorUserId = User.FindFirst("UserId")?.Value ?? "0";
            var result = await _checkInService.ProcessCheckInTokenAsync(request.Token, actorUserId);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });

            return Ok(result.Data);
        }
    }

    public class CheckInRequestDto
    {
        public string Token { get; set; } = null!;
    }
}
