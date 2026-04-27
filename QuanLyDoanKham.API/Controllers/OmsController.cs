using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Controllers
{
    /// <summary>OMS (Outpatient Management System) - API cho check-in bệnh nhân ngoại trú</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OmsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OmsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>Check-in bệnh nhân bằng Medical Record ID</summary>
        [HttpPost("checkin/{recordId}")]
        public async Task<IActionResult> CheckInPatient(int recordId)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            
            // Tìm medical record
            var record = await _context.MedicalRecords
                .Include(r => r.Patient)
                .Include(r => r.MedicalGroup)
                .FirstOrDefaultAsync(r => r.MedicalRecordId == recordId);

            if (record == null)
                return NotFound(new { message = "Không tìm thấy hồ sơ khám" });

            // Kiểm tra trạng thái
            if (record.Status == "CheckedIn" || record.Status == "InProgress")
                return BadRequest(new { message = "Bệnh nhân đã check-in trước đó" });

            // Cập nhật trạng thái
            record.Status = "CheckedIn";
            record.CheckInAt = DateTime.Now;

            await _context.SaveChangesAsync();

            // Tạo số thứ tự (queue number) - giả lập
            var queueNo = new Random().Next(1, 999);

            return Ok(new
            {
                message = "Check-in thành công",
                data = new
                {
                    medicalRecordId = record.MedicalRecordId,
                    fullName = record.Patient?.FullName ?? "Không rõ",
                    queueNo = queueNo.ToString().PadLeft(3, '0'),
                    serviceName = "Khám tổng quát",
                    status = record.Status,
                    checkInAt = record.CheckInAt
                }
            });
        }
    }
}
