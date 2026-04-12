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
        private readonly ApplicationDbContext _context;
        private readonly IMedicalRecordService _medicalRecordService;

        public MedicalRecordsController(ApplicationDbContext context, IMedicalRecordService medicalRecordService)
        {
            _context = context;
            _medicalRecordService = medicalRecordService;
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

        // GET: api/MedicalRecords/group/{groupId}
        [HttpGet("group/{groupId}")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<ActionResult<IEnumerable<object>>> GetByGroup(int groupId)
        {
            var records = await _context.MedicalRecords
                .Where(m => m.GroupId == groupId)
                .OrderBy(m => m.FullName)
                .Select(m => new {
                    m.MedicalRecordId,
                    m.FullName,
                    m.DateOfBirth,
                    m.Gender,
                    m.IDCardNumber,
                    m.Department,
                    m.Status,
                    m.CheckInAt,
                    m.QueueNo
                })
                .ToListAsync();

            return Ok(records);
        }

        // GET: api/MedicalRecords/{id}
        [HttpGet("{id}")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> GetById(int id)
        {
            var record = await _context.MedicalRecords
                .Include(m => m.StationTasks)
                .FirstOrDefaultAsync(m => m.MedicalRecordId == id);

            if (record == null) return NotFound();

            return Ok(record);
        }
        // GET: api/MedicalRecords/queue/{stationCode}
        // Returns active tasks for a specific station, ordered by QueueNo (FIFO).
        // Excludes STATION_DONE and SKIPPED tasks so the station coordinator only sees actionable items.
        [HttpGet("queue/{stationCode}")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> GetQueueByStation(string stationCode)
        {
            var queue = await _context.RecordStationTasks
                .Include(t => t.MedicalRecord)
                .Where(t => t.StationCode == stationCode
                         && t.Status != StationTaskStatus.StationDone
                         && t.Status != StationTaskStatus.Skipped)
                .OrderBy(t => t.MedicalRecord.QueueNo)
                .Select(t => new {
                    t.TaskId,
                    t.MedicalRecordId,
                    t.MedicalRecord.FullName,
                    t.MedicalRecord.Gender,
                    t.MedicalRecord.QueueNo,
                    t.Status,
                    t.WaitingSince,
                    t.StartedAt
                })
                .ToListAsync();

            return Ok(queue);
        }

        // GET: api/MedicalRecords/queue/{stationCode}/summary
        // Lightweight count-only endpoint for the QueueDashboard overview cards.
        [HttpGet("queue/{stationCode}/summary")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> GetStationQueueSummary(string stationCode)
        {
            var waitingCount   = await _context.RecordStationTasks
                .CountAsync(t => t.StationCode == stationCode && t.Status == StationTaskStatus.Waiting);
            var inProgressCount = await _context.RecordStationTasks
                .CountAsync(t => t.StationCode == stationCode && t.Status == StationTaskStatus.StationInProgress);
            var doneToday      = await _context.RecordStationTasks
                .CountAsync(t => t.StationCode == stationCode
                              && t.Status == StationTaskStatus.StationDone
                              && t.CompletedAt.HasValue
                              && t.CompletedAt.Value.Date == DateTime.Today);

            return Ok(new { stationCode, waitingCount, inProgressCount, doneToday });
        }

        // GET: api/MedicalRecords/queue/overview?groupId={groupId}
        // Returns aggregated queue state across ALL stations for a medical group.
        // Used by the QueueDashboard to render the station-load heatmap in one request.
        [HttpGet("queue/overview")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> GetGroupQueueOverview([FromQuery] int groupId)
        {
            var activeTasks = await _context.RecordStationTasks
                .Include(t => t.MedicalRecord)
                .Include(t => t.Station)
                .Where(t => t.MedicalRecord.GroupId == groupId
                         && t.Status != StationTaskStatus.StationDone
                         && t.Status != StationTaskStatus.Skipped)
                .GroupBy(t => new { t.StationCode, t.Station.StationName, t.Station.SortOrder })
                .Select(g => new {
                    StationCode  = g.Key.StationCode,
                    StationName  = g.Key.StationName,
                    SortOrder    = g.Key.SortOrder,
                    WaitingCount    = g.Count(t => t.Status == StationTaskStatus.Waiting),
                    InProgressCount = g.Count(t => t.Status == StationTaskStatus.StationInProgress),
                })
                .OrderBy(s => s.SortOrder)
                .ToListAsync();

            return Ok(activeTasks);
        }
    }
}
