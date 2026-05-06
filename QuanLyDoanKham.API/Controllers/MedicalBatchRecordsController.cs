using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDoanKham.API.Authorization;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Services.MedicalBatch;

namespace QuanLyDoanKham.API.Controllers
{
    /// <summary>
    /// Manages MedicalBatchRecord entities — lazy creation and status tracking.
    /// NEW controller — does NOT modify the existing MedicalRecordsController.
    /// </summary>
    [ApiController]
    [Authorize]
    public class MedicalBatchRecordsController : ControllerBase
    {
        private readonly IMedicalBatchRecordService _recordService;

        public MedicalBatchRecordsController(IMedicalBatchRecordService recordService)
        {
            _recordService = recordService;
        }

        // ── POST /api/medical-batches/{batchId}/records ───────────────────────
        /// <summary>
        /// Lazily create a single MedicalBatchRecord for the given batch.
        /// Returns 400 when the batch is already at EstimatedCount capacity.
        /// </summary>
        [HttpPost("api/medical-batches/{batchId:guid}/records")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> CreateRecord(Guid batchId)
        {
            var (record, error) = await _recordService.CreateRecordAsync(batchId);

            if (error != null)
                return BadRequest(new { message = error });

            return StatusCode(201, record);
        }

        // ── PATCH /api/medical-records/{id}/status ────────────────────────────
        /// <summary>
        /// Update a record's status to Done.
        /// Returns 409 Conflict if the record is already Done (idempotency guard).
        /// </summary>
        [HttpPatch("api/medical-records/{id:guid}/status")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateBatchRecordStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, error) = await _recordService.UpdateStatusToDoneAsync(id);

            if (!success)
            {
                // "Bản ghi đã ở trạng thái Done" → 409 Conflict
                if (error != null && error.Contains("Done"))
                    return Conflict(new { message = error });

                return BadRequest(new { message = error });
            }

            return Ok(new { message = "Cập nhật trạng thái thành công.", id });
        }

        // ── GET /api/medical-batches/{batchId}/records ────────────────────────
        /// <summary>
        /// Paginated list of records for a batch.
        /// Returns empty array when no records exist (never 404).
        /// </summary>
        [HttpGet("api/medical-batches/{batchId:guid}/records")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> GetRecords(
            Guid batchId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var result = await _recordService.GetRecordsByBatchAsync(batchId, page, pageSize);
            return Ok(result);
        }
    }
}
