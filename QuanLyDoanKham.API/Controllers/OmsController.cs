using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Authorization;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Services.MedicalRecords;

namespace QuanLyDoanKham.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OmsController : ControllerBase
    {
        private readonly IMedicalRecordStateMachine _stateMachine;
        private readonly IMedicalRecordService _medicalRecordService;
        private readonly ApplicationDbContext _context;

        public OmsController(IMedicalRecordStateMachine stateMachine, IMedicalRecordService medicalRecordService, ApplicationDbContext context)
        {
            _stateMachine = stateMachine;
            _medicalRecordService = medicalRecordService;
            _context = context;
        }

        [HttpPost("checkin/{medicalRecordId}")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> ManualCheckIn(int medicalRecordId)
        {
            var userId = User.Identity?.Name ?? "system";
            var result = await _stateMachine.CheckInAsync(medicalRecordId, userId);
            if (!result.IsSuccess) return BadRequest(new { message = result.Message });
            return Ok(result);
        }

        [HttpPost("station/{medicalRecordId}/start")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> StartStation(int medicalRecordId, [FromQuery] string stationCode)
        {
            var userId = User.Identity?.Name ?? "system";
            var result = await _stateMachine.StartStationAsync(medicalRecordId, stationCode, userId);
            if (!result.IsSuccess) return BadRequest(new { message = result.Message });
            return Ok(result);
        }

        [HttpPost("station/{medicalRecordId}/complete")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> CompleteStation(int medicalRecordId, [FromQuery] string stationCode, [FromQuery] string? notes = null)
        {
            var userId = User.Identity?.Name ?? "system";
            var result = await _stateMachine.CompleteStationAsync(medicalRecordId, stationCode, userId, notes);
            if (!result.IsSuccess) return BadRequest(new { message = result.Message });
            return Ok(result);
        }

        [HttpPost("finalize/{medicalRecordId}")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> FinalizeRecord(int medicalRecordId)
        {
            var userId = User.Identity?.Name ?? "system";
            var result = await _stateMachine.FinalizeRecordAsync(medicalRecordId, userId);
            if (!result.IsSuccess) return BadRequest(new { message = result.Message });
            return Ok(result);
        }

        [HttpPost("station/{medicalRecordId}/assign-extra")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> AssignExtraStation(int medicalRecordId, [FromQuery] string stationCode, [FromQuery] string? notes = null)
        {
            var userId = User.Identity?.Name ?? "system";
            var result = await _stateMachine.AddExtraStationAsync(medicalRecordId, stationCode, userId, notes);
            if (!result.IsSuccess) return BadRequest(new { message = result.Message });
            return Ok(result);
        }

        // ─── QC Endpoints ────────────────────────────────────────────────────

        /// <summary>GET api/Oms/qc-pending — Danh sách hồ sơ đang chờ QC</summary>
        [HttpGet("qc-pending")]
        [AuthorizePermission("KetQua.QCApprove")]
        public async Task<IActionResult> GetQcPendingRecords()
        {
            var records = await _medicalRecordService.GetQcPendingRecordsAsync();
            return Ok(records);
        }

        /// <summary>POST api/Oms/qc/{id}/pass — QC Duyệt hồ sơ</summary>
        [HttpPost("qc/{medicalRecordId}/pass")]
        [AuthorizePermission("KetQua.QCApprove")]
        public async Task<IActionResult> QcPass(int medicalRecordId)
        {
            var userId = User.Identity?.Name ?? "system";
            var result = await _stateMachine.QCPassAsync(medicalRecordId, userId);
            if (!result.IsSuccess) return BadRequest(new { message = result.Message });
            return Ok(new { message = "Hồ sơ đã được duyệt QC thành công." });
        }

        /// <summary>POST api/Oms/qc/{id}/rework — QC Trả lại hồ sơ để bổ sung</summary>
        [HttpPost("qc/{medicalRecordId}/rework")]
        [AuthorizePermission("KetQua.QCApprove")]
        public async Task<IActionResult> QcRework(int medicalRecordId, [FromBody] QcReworkRequest request)
        {
            var userId = User.Identity?.Name ?? "system";
            var result = await _stateMachine.QCReworkAsync(medicalRecordId, userId, request.Reason);
            if (!result.IsSuccess) return BadRequest(new { message = result.Message });
            return Ok(new { message = "Đã trả hồ sơ về bác sĩ để chỉnh sửa." });
        }

        [HttpPost("cancel/{medicalRecordId}")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> CancelRecord(int medicalRecordId, [FromBody] CancelRecordRequest request)
        {
            var userId = User.Identity?.Name ?? "system";
            var result = await _stateMachine.CancelRecordAsync(medicalRecordId, userId, request.Reason);
            if (!result.IsSuccess) return BadRequest(new { message = result.Message });
            return Ok(new { message = "Hồ sơ đã được hủy/bỏ cuộc thành công." });
        }
    }

    public record QcReworkRequest(string Reason);
    public record CancelRecordRequest(string Reason);
}

