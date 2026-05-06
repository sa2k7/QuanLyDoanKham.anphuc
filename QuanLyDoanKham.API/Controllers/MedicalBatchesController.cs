using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDoanKham.API.Authorization;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Services.MedicalBatch;

namespace QuanLyDoanKham.API.Controllers
{
    /// <summary>
    /// Manages MedicalBatch entities — lightweight batch-based health check tracking.
    /// NEW controller — does NOT modify any existing endpoint.
    /// </summary>
    [Route("api/medical-batches")]
    [ApiController]
    [Authorize]
    public class MedicalBatchesController : ControllerBase
    {
        private readonly IMedicalBatchService _batchService;

        public MedicalBatchesController(IMedicalBatchService batchService)
        {
            _batchService = batchService;
        }

        // ── POST /api/medical-batches ─────────────────────────────────────────
        /// <summary>
        /// Create a new MedicalBatch.
        /// Set autoGenerate=true to bulk-create EstimatedCount records atomically.
        /// </summary>
        [HttpPost]
        [AuthorizePermission("DoanKham.Create")]
        public async Task<IActionResult> CreateBatch([FromBody] CreateMedicalBatchDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdBy = User.Identity?.Name ?? "system";
            var (batch, error) = await _batchService.CreateBatchAsync(dto, createdBy);

            if (error != null)
                return BadRequest(new { message = error });

            return StatusCode(201, batch);
        }

        // ── GET /api/medical-batches/{id} ─────────────────────────────────────
        /// <summary>Get a single batch with its current record count.</summary>
        [HttpGet("{id:guid}")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> GetBatch(Guid id)
        {
            var batch = await _batchService.GetBatchByIdAsync(id);
            if (batch == null) return NotFound(new { message = $"Không tìm thấy lô khám với Id = {id}" });
            return Ok(batch);
        }

        // ── GET /api/medical-batches?contractId={id} ──────────────────────────
        /// <summary>List all batches for a contract. Returns [] when none exist.</summary>
        [HttpGet]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> GetBatchesByContract([FromQuery] int contractId)
        {
            if (contractId <= 0)
                return BadRequest(new { message = "contractId phải lớn hơn 0." });

            var batches = await _batchService.GetBatchesByContractAsync(contractId);
            return Ok(batches);
        }
    }
}
