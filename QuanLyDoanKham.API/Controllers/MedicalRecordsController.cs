using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Authorization;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Services;
using QuanLyDoanKham.API.Services.MedicalRecords;

namespace QuanLyDoanKham.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MedicalRecordsController : ControllerBase
    {
        private readonly IMedicalRecordService _medicalRecordService;
        private readonly IMedicalReportPdfGenerator _pdfGenerator;
        private readonly IMedicalRecordStateMachine _stateMachine;

        public MedicalRecordsController(
            IMedicalRecordService medicalRecordService, 
            IMedicalReportPdfGenerator pdfGenerator,
            IMedicalRecordStateMachine stateMachine)
        {
            _medicalRecordService = medicalRecordService;
            _pdfGenerator = pdfGenerator;
            _stateMachine = stateMachine;
        }

        // POST: api/MedicalRecords/batch-ingest
        [HttpPost("batch-ingest")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> BatchIngest([FromBody] MedicalRecordBatchIngestRequestDto request)
        {
            var username = User.Identity?.Name ?? "system";
            var result = await _medicalRecordService.BatchIngestAsync(request, username);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });

            return Ok(new { 
                message = "Đã nhập thành công hồ sơ y tế.", 
                count = result.Data?.Count ?? 0,
                groupId = request.GroupId
            });
        }

        // GET: api/MedicalRecords/by-group/{groupId}
        [HttpGet("by-group/{groupId}")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<ActionResult<IEnumerable<object>>> GetByGroup(int groupId)
        {
            var records = await _medicalRecordService.GetByGroupAsync(groupId);
            return Ok(records);
        }

        // POST: api/MedicalRecords/batch-ingest-excel
        [HttpPost("batch-ingest-excel")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> BatchIngestExcel([FromQuery] int groupId, [FromQuery] string filePath)
        {
            var username = User.Identity?.Name ?? "system";
            var result = await _medicalRecordService.BatchIngestFromExcelAsync(groupId, filePath, username);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = "Đã nhập dữ liệu từ Excel thành công.", count = result.Data?.Count ?? 0 });
        }

        // DELETE: api/MedicalRecords/{id}
        [HttpDelete("{id}")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _medicalRecordService.DeleteAsync(id);
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = "Đã xóa hồ sơ y tế thành công." });
        }

        // GET: api/MedicalRecords/{id}
        [HttpGet("{id}")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> GetById(int id)
        {
            var record = await _medicalRecordService.GetByIdAsync(id);
            if (record == null) return NotFound();
            return Ok(record);
        }


        // GET: api/MedicalRecords/{id}/export-pdf
        [HttpGet("{id}/export-pdf")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> ExportPdf(int id)
        {
            try
            {
                var pdfBytes = await _pdfGenerator.GenerateReportAsync(id);
                return File(pdfBytes, "application/pdf", $"KhamSucKhoe_{id}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/MedicalRecords/{id}/ai-clinical-summary
        [HttpPost("{id}/ai-clinical-summary")]
        [AuthorizePermission("KetQua.QCApprove")]
        public async Task<IActionResult> GetAiClinicalSummary(int id)
        {
            var result = await _medicalRecordService.GenerateAiClinicalSummaryAsync(id);
            if (!result.IsSuccess) return BadRequest(new { message = result.Message });
            return Ok(new { summary = result.Data });
        }

        // ─── Lifecycle Endpoints (Simplified) ────────────────────────────────

        [HttpPost("{id}/checkin")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> CheckIn(int id)
        {
            var userId = User.Identity?.Name ?? "system";
            var result = await _stateMachine.CheckInAsync(id, userId);
            if (!result.IsSuccess) return BadRequest(new { message = result.Message });
            return Ok(result);
        }

        [HttpPost("{id}/finalize")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> FinalizeRecord(int id)
        {
            var userId = User.Identity?.Name ?? "system";
            var result = await _stateMachine.FinalizeRecordAsync(id, userId);
            if (!result.IsSuccess) return BadRequest(new { message = result.Message });
            return Ok(result);
        }

        [HttpPost("{id}/qc-pass")]
        [AuthorizePermission("KetQua.QCApprove")]
        public async Task<IActionResult> QcPass(int id)
        {
            var userId = User.Identity?.Name ?? "system";
            var result = await _stateMachine.QCPassAsync(id, userId);
            if (!result.IsSuccess) return BadRequest(new { message = result.Message });
            return Ok(new { message = "Hồ sơ đã được duyệt QC thành công." });
        }

        [HttpPost("{id}/qc-rework")]
        [AuthorizePermission("KetQua.QCApprove")]
        public async Task<IActionResult> QcRework(int id, [FromBody] string reason)
        {
            var userId = User.Identity?.Name ?? "system";
            var result = await _stateMachine.QCReworkAsync(id, userId, reason);
            if (!result.IsSuccess) return BadRequest(new { message = result.Message });
            return Ok(new { message = "Đã trả hồ sơ về bác sĩ để chỉnh sửa." });
        }

        [HttpPost("{id}/cancel")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> CancelRecord(int id, [FromBody] string reason)
        {
            var userId = User.Identity?.Name ?? "system";
            var result = await _stateMachine.CancelRecordAsync(id, userId, reason);
            if (!result.IsSuccess) return BadRequest(new { message = result.Message });
            return Ok(new { message = "Hồ sơ đã được hủy thành công." });
        }

        [HttpGet("qc-pending")]
        [AuthorizePermission("KetQua.QCApprove")]
        public async Task<IActionResult> GetQcPending()
        {
            var records = await _medicalRecordService.GetQcPendingRecordsAsync();
            return Ok(records);
        }
    }
}
