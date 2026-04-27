using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Authorization;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Services.Calendar;

namespace QuanLyDoanKham.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CalendarController : ControllerBase
    {
        private readonly ICalendarService _calendarService;
        private readonly ApplicationDbContext _context;

        public CalendarController(ICalendarService calendarService, ApplicationDbContext context)
        {
            _calendarService = calendarService;
            _context = context;
        }

        // ================================================================
        // GET: api/Calendar?month=4&year=2026 — Lịch đoàn khám tháng
        // ================================================================
        [HttpGet]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> GetCalendar(int month, int year)
        {
            var data = await _calendarService.GetCalendarAsync(month, year);
            return Ok(data);
        }

        // ================================================================
        // GET: api/Calendar/my-schedule?month=4&year=2026 — Lịch cá nhân
        // ================================================================
        [HttpGet("my-schedule")]
        public async Task<IActionResult> GetMySchedule(int month, int year)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var user = await _context.Users
                .Include(u => u.Staff)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user?.Staff == null)
                return BadRequest(new { message = "Tai khoan khong lien ket nhan su" });

            var data = await _calendarService.GetMyScheduleAsync(user.Staff.StaffId, month, year);
            return Ok(data);
        }
    }
}
