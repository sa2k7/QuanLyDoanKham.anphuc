using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Authorization;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;
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

        public MedicalRecordsController(IMedicalRecordService medicalRecordService, IMedicalReportPdfGenerator pdfGenerator)
        {
            _medicalRecordService = medicalRecordService;
            _pdfGenerator = pdfGenerator;
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

        // GET: api/MedicalRecords/queue/{stationCode}
        [HttpGet("queue/{stationCode}")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> GetQueueByStation(string stationCode)
        {
            var queue = await _medicalRecordService.GetQueueByStationAsync(stationCode);
            return Ok(queue);
        }

        // GET: api/MedicalRecords/queue/{stationCode}/summary
        [HttpGet("queue/{stationCode}/summary")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> GetStationQueueSummary(string stationCode)
        {
            var summary = await _medicalRecordService.GetStationQueueSummaryAsync(stationCode);
            return Ok(summary);
        }

        // GET: api/MedicalRecords/queue/overview?groupId={groupId}
        [HttpGet("queue/overview")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> GetGroupQueueOverview([FromQuery] int groupId)
        {
            var overview = await _medicalRecordService.GetGroupQueueOverviewAsync(groupId);
            return Ok(overview);
        }

        // GET: api/MedicalRecords/qc-pending
        [HttpGet("qc-pending")]
        [AuthorizePermission("BaoCao.QC")]
        public async Task<IActionResult> GetQcPendingRecords()
        {
            var records = await _medicalRecordService.GetQcPendingRecordsAsync();
            return Ok(records);
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
    }
}
