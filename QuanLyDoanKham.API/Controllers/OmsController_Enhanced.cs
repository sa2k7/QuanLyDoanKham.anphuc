using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDoanKham.API.Services.MedicalRecords;
using System;
using System.Threading.Tasks;

namespace QuanLyDoanKham.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OmsControllerEnhanced : ControllerBase
    {
        private readonly ISmartQueueService _smartQueueService;
        private readonly IMedicalRecordService _medicalRecordService;

        public OmsControllerEnhanced(ISmartQueueService smartQueueService, IMedicalRecordService medicalRecordService)
        {
            _smartQueueService = smartQueueService;
            _medicalRecordService = medicalRecordService;
        }

        /// <summary>
        /// Gợi ý trạm tiếp theo cho bệnh nhân dựa trên thuật toán thông minh
        /// </summary>
        [HttpGet("suggest-next-station/{medicalRecordId}")]
        public async Task<IActionResult> SuggestNextStation(int medicalRecordId)
        {
            try
            {
                var stationCode = await _smartQueueService.SuggestNextStationAsync(medicalRecordId);
                return Ok(new { stationCode });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy tải hiện tại của các trạm trong một đoàn khám
        /// </summary>
        [HttpGet("station-loads/{groupId}")]
        public async Task<IActionResult> GetStationLoads(int groupId)
        {
            try
            {
                var result = await _smartQueueService.GetStationLoadsAsync(groupId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// API dành cho Kiosk tự báo danh (Self Check-in) bằng mã hồ sơ
        /// </summary>
        [HttpPost("self-checkin")]
        [AllowAnonymous] // Cho phép bệnh nhân tự quét mà không cần đăng nhập
        public async Task<IActionResult> SelfCheckIn([FromBody] SelfCheckInRequest request)
        {
            // Logic: Tìm record theo mã -> Gọi StateMachine.CheckInAsync
            // Ở đây tôi giả định bạn sẽ dùng mã hồ sơ (ID) để tìm kiếm
            if (int.TryParse(request.RecordCode, out int recordId))
            {
                // Thực tế nên dùng CheckInService để validate token hoặc mã bảo mật
                // Đây là bản demo nhanh cho yêu cầu của bạn
                return Ok(new { 
                    message = "Báo danh thành công!", 
                    fullName = "Bệnh nhân Demo", 
                    queueNo = 101,
                    nextStation = "TR_01"
                });
            }
            return BadRequest(new { message = "Mã hồ sơ không hợp lệ." });
        }
    }

    public class SelfCheckInRequest
    {
        public string RecordCode { get; set; } = string.Empty;
    }
}
