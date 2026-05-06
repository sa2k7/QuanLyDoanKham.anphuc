using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Authorization;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Controllers
{
    /// <summary>
    /// Manages the BatchMedicalRecord bridge: assigns Patients (Bệnh Án) to MedicalBatches.
    ///
    /// Architecture:
    ///   Patient (Bệnh Án) ──── BatchMedicalRecord ──── MedicalBatch
    ///
    /// Rules:
    ///   - A Patient belongs to exactly ONE Contract
    ///   - A Patient can appear in MULTIPLE batches (via this bridge)
    ///   - Duplicate assignment in the same batch is rejected (HTTP 409)
    ///   - EstimatedCount on MedicalBatch is NOT a hard constraint
    /// </summary>
    [Route("api/batch-medical-records")]
    [ApiController]
    [Authorize]
    public class BatchMedicalRecordsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BatchMedicalRecordsController> _logger;

        public BatchMedicalRecordsController(
            ApplicationDbContext context,
            ILogger<BatchMedicalRecordsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ── 1. Create Patient (Bệnh Án) by Contract ───────────────────────────
        // POST /api/batch-medical-records/patients
        // Creates a new Patient record linked to a Contract.
        // If patient with same IDCardNumber already exists in the contract → returns existing.
        [HttpPost("patients")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> CreatePatient([FromBody] CreateBenhAnDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Validate contract exists
            var contract = await _context.Contracts
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.HealthContractId == dto.ContractId);
            if (contract == null)
                return BadRequest(new { message = $"Hợp đồng #{dto.ContractId} không tồn tại." });

            // Idempotency: if same IDCard already registered in this contract, return existing
            if (!string.IsNullOrWhiteSpace(dto.IDCardNumber))
            {
                var existing = await _context.Patients
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p =>
                        p.HealthContractId == dto.ContractId &&
                        p.IDCardNumber == dto.IDCardNumber.Trim());

                if (existing != null)
                    return Ok(new { message = "Bệnh án đã tồn tại.", patientId = existing.PatientId, isExisting = true });
            }

            var patient = new Patient
            {
                HealthContractId = dto.ContractId,
                FullName         = dto.FullName.Trim(),
                DateOfBirth      = dto.DateOfBirth ?? DateTime.MinValue,
                Gender           = dto.Gender,
                IDCardNumber     = dto.IDCardNumber?.Trim(),
                PhoneNumber      = dto.PhoneNumber?.Trim(),
                Department       = dto.Department?.Trim(),
                Source           = "Manual",
                CreatedDate      = DateTime.Now
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Bệnh án #{PatientId} created for contract #{ContractId} by {User}",
                patient.PatientId, dto.ContractId, User.Identity?.Name);

            return StatusCode(201, new
            {
                message    = "Tạo bệnh án thành công.",
                patientId  = patient.PatientId,
                isExisting = false
            });
        }

        // ── 2. Assign Patient to Batch ────────────────────────────────────────
        // POST /api/batch-medical-records/assign
        // Assigns an existing Patient to a MedicalBatch.
        // If patient doesn't exist yet → creates it first (walk-in flow).
        // Prevents duplicate assignment in the same batch (HTTP 409).
        [HttpPost("assign")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> AssignToBatch([FromBody] AssignToBatchDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Validate batch exists
            var batch = await _context.MedicalBatches
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == dto.MedicalBatchId);
            if (batch == null)
                return NotFound(new { message = $"Lô khám {dto.MedicalBatchId} không tồn tại." });

            int patientId;

            if (dto.PatientId.HasValue)
            {
                // Existing patient — validate it belongs to the same contract as the batch
                var patient = await _context.Patients
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PatientId == dto.PatientId.Value);
                if (patient == null)
                    return NotFound(new { message = $"Bệnh án #{dto.PatientId} không tồn tại." });

                if (patient.HealthContractId != batch.ContractId)
                    return BadRequest(new
                    {
                        message = "Bệnh án không thuộc hợp đồng của lô khám này. " +
                                  "Một bệnh án chỉ được phân công vào lô khám cùng hợp đồng."
                    });

                patientId = patient.PatientId;
            }
            else if (dto.WalkIn != null)
            {
                // Walk-in: create patient on the fly, then assign
                var walkIn = dto.WalkIn;

                // Check for existing by IDCard in same contract
                Patient? existing = null;
                if (!string.IsNullOrWhiteSpace(walkIn.IDCardNumber))
                {
                    existing = await _context.Patients
                        .FirstOrDefaultAsync(p =>
                            p.HealthContractId == batch.ContractId &&
                            p.IDCardNumber == walkIn.IDCardNumber.Trim());
                }

                if (existing != null)
                {
                    patientId = existing.PatientId;
                }
                else
                {
                    var newPatient = new Patient
                    {
                        HealthContractId = batch.ContractId,
                        FullName         = walkIn.FullName.Trim(),
                        DateOfBirth      = walkIn.DateOfBirth ?? DateTime.MinValue,
                        Gender           = walkIn.Gender,
                        IDCardNumber     = walkIn.IDCardNumber?.Trim(),
                        PhoneNumber      = walkIn.PhoneNumber?.Trim(),
                        Department       = walkIn.Department?.Trim(),
                        Source           = "WalkIn",
                        CreatedDate      = DateTime.Now
                    };
                    _context.Patients.Add(newPatient);
                    await _context.SaveChangesAsync();
                    patientId = newPatient.PatientId;

                    _logger.LogInformation(
                        "Walk-in bệnh án #{PatientId} created for contract #{ContractId}",
                        patientId, batch.ContractId);
                }
            }
            else
            {
                return BadRequest(new { message = "Phải cung cấp patientId hoặc thông tin walk-in." });
            }

            // Duplicate assignment guard
            var alreadyAssigned = await _context.BatchMedicalRecords
                .AnyAsync(b => b.MedicalBatchId == dto.MedicalBatchId && b.PatientId == patientId);
            if (alreadyAssigned)
                return Conflict(new
                {
                    message    = "Bệnh án đã được phân công vào lô khám này.",
                    patientId,
                    batchId    = dto.MedicalBatchId
                });

            var userId = User.FindFirst("UserId")?.Value;
            int? assignedByUserId = int.TryParse(userId, out var uid) ? uid : null;

            var bridge = new BatchMedicalRecord
            {
                Id                = Guid.NewGuid(),
                MedicalBatchId    = dto.MedicalBatchId,
                PatientId         = patientId,
                Status            = "Waiting",
                CreatedAt         = DateTime.UtcNow,
                AssignedByUserId  = assignedByUserId,
                Note              = dto.Note
            };

            _context.BatchMedicalRecords.Add(bridge);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Patient #{PatientId} assigned to batch {BatchId} by {User}",
                patientId, dto.MedicalBatchId, User.Identity?.Name);

            return StatusCode(201, new
            {
                message         = "Phân công bệnh án vào lô khám thành công.",
                id              = bridge.Id,
                patientId,
                medicalBatchId  = dto.MedicalBatchId,
                status          = bridge.Status
            });
        }

        // ── 3. Get Patients (Bệnh Án) by Contract ────────────────────────────
        // GET /api/batch-medical-records/by-contract/{contractId}
        // Returns all patients registered under a contract with their batch assignment summary.
        [HttpGet("by-contract/{contractId:int}")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> GetByContract(
            int contractId,
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var query = _context.Patients
                .AsNoTracking()
                .Where(p => p.HealthContractId == contractId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(p =>
                    p.FullName.ToLower().Contains(s) ||
                    (p.IDCardNumber != null && p.IDCardNumber.Contains(s)) ||
                    (p.PhoneNumber  != null && p.PhoneNumber.Contains(s)));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.PatientId,
                    p.FullName,
                    p.DateOfBirth,
                    p.Gender,
                    p.IDCardNumber,
                    p.PhoneNumber,
                    p.Department,
                    p.Source,
                    p.CreatedDate,
                    // Batch assignment summary
                    BatchCount = _context.BatchMedicalRecords
                        .Count(b => b.PatientId == p.PatientId),
                    LatestStatus = _context.BatchMedicalRecords
                        .Where(b => b.PatientId == p.PatientId)
                        .OrderByDescending(b => b.CreatedAt)
                        .Select(b => b.Status)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(new { total, page, pageSize, items });
        }

        // ── 4. Get Patients (Bệnh Án) by MedicalBatch ────────────────────────
        // GET /api/batch-medical-records/by-batch/{batchId}
        // Returns all patients assigned to a specific batch with their current status.
        [HttpGet("by-batch/{batchId:guid}")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> GetByBatch(Guid batchId)
        {
            var batch = await _context.MedicalBatches
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == batchId);
            if (batch == null)
                return NotFound(new { message = $"Lô khám {batchId} không tồn tại." });

            var records = await _context.BatchMedicalRecords
                .AsNoTracking()
                .Where(b => b.MedicalBatchId == batchId)
                .OrderBy(b => b.CreatedAt)
                .Select(b => new
                {
                    b.Id,
                    b.Status,
                    b.CreatedAt,
                    b.Note,
                    Patient = b.Patient == null ? null : new
                    {
                        b.Patient.PatientId,
                        b.Patient.FullName,
                        b.Patient.DateOfBirth,
                        b.Patient.Gender,
                        b.Patient.IDCardNumber,
                        b.Patient.PhoneNumber,
                        b.Patient.Department
                    }
                })
                .ToListAsync();

            // Progress summary
            var waiting    = records.Count(r => r.Status == "Waiting");
            var inProgress = records.Count(r => r.Status == "InProgress");
            var done       = records.Count(r => r.Status == "Done");

            return Ok(new
            {
                batchId,
                estimatedCount = batch.EstimatedCount,
                assignedCount  = records.Count,
                progress       = new { waiting, inProgress, done },
                records
            });
        }

        // ── 5. Update Status ──────────────────────────────────────────────────
        // PATCH /api/batch-medical-records/{id}/status
        // Updates the status of a single bridge record (Waiting → InProgress → Done).
        [HttpPatch("{id:guid}/status")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateBatchRecordStatusDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var record = await _context.BatchMedicalRecords.FindAsync(id);
            if (record == null)
                return NotFound(new { message = $"Bản ghi {id} không tồn tại." });

            var allowed = new[] { "Waiting", "InProgress", "Done" };
            if (!allowed.Contains(dto.Status))
                return BadRequest(new { message = $"Trạng thái không hợp lệ. Cho phép: {string.Join(", ", allowed)}" });

            // Idempotency: already Done → 409
            if (record.Status == "Done" && dto.Status == "Done")
                return Conflict(new { message = "Bản ghi đã ở trạng thái Done." });

            record.Status = dto.Status;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật trạng thái thành công.", id, status = record.Status });
        }

        // ── 6. Remove Assignment ──────────────────────────────────────────────
        // DELETE /api/batch-medical-records/{id}
        // Removes a patient from a batch (only if status is Waiting).
        [HttpDelete("{id:guid}")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> RemoveAssignment(Guid id)
        {
            var record = await _context.BatchMedicalRecords.FindAsync(id);
            if (record == null)
                return NotFound(new { message = $"Bản ghi {id} không tồn tại." });

            if (record.Status != "Waiting")
                return BadRequest(new
                {
                    message = $"Không thể gỡ bệnh án đã bắt đầu khám (trạng thái: {record.Status})."
                });

            _context.BatchMedicalRecords.Remove(record);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã gỡ bệnh án khỏi lô khám." });
        }
    }

    // ── DTOs ──────────────────────────────────────────────────────────────────

    /// <summary>Create a new Patient (Bệnh Án) linked to a Contract.</summary>
    public class CreateBenhAnDto
    {
        public int ContractId { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(100)]
        public string FullName { get; set; } = null!;

        public DateTime? DateOfBirth { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(10)]
        public string? Gender { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(20)]
        public string? IDCardNumber { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(15)]
        public string? PhoneNumber { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(100)]
        public string? Department { get; set; }
    }

    /// <summary>Assign an existing Patient (or walk-in) to a MedicalBatch.</summary>
    public class AssignToBatchDto
    {
        public Guid MedicalBatchId { get; set; }

        /// <summary>Provide this if the patient already exists in the system.</summary>
        public int? PatientId { get; set; }

        /// <summary>Provide this for walk-in patients (creates patient on the fly).</summary>
        public WalkInPatientDto? WalkIn { get; set; }

        public string? Note { get; set; }
    }

    /// <summary>Walk-in patient data — used when patient is not pre-registered.</summary>
    public class WalkInPatientDto
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(100)]
        public string FullName { get; set; } = null!;

        public DateTime? DateOfBirth { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(10)]
        public string? Gender { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(20)]
        public string? IDCardNumber { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(15)]
        public string? PhoneNumber { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(100)]
        public string? Department { get; set; }
    }

    /// <summary>Update the status of a BatchMedicalRecord.</summary>
    public class UpdateBatchRecordStatusDto
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string Status { get; set; } = null!; // Waiting | InProgress | Done
    }
}
