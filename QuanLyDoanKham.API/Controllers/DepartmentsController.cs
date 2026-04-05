using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Authorization;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DepartmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DepartmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Departments
        [HttpGet]
        [AuthorizePermission("NhanSu.View")]
        public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetDepartments()
        {
            var deps = await _context.Departments
                .Include(d => d.Staffs)
                .Include(d => d.Users)
                .OrderBy(d => d.DepartmentName)
                .Select(d => new DepartmentDto
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    DepartmentCode = d.DepartmentCode,
                    Description = d.Description,
                    TotalStaff = d.Staffs.Count(s => s.IsActive),
                    TotalUsers = d.Users.Count(u => u.IsActive)
                })
                .ToListAsync();

            return Ok(deps);
        }

        // GET: api/Departments/{id}
        [HttpGet("{id}")]
        [AuthorizePermission("NhanSu.View")]
        public async Task<IActionResult> GetDepartment(int id)
        {
            var dep = await _context.Departments
                .Include(d => d.Staffs)
                .Include(d => d.Users).ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (dep == null) return NotFound();

            return Ok(new
            {
                dep.DepartmentId,
                dep.DepartmentName,
                dep.DepartmentCode,
                dep.Description,
                staffs = dep.Staffs.Where(s => s.IsActive).Select(s => new
                {
                    s.StaffId, s.FullName, s.EmployeeCode, s.JobTitle
                }),
                users = dep.Users.Where(u => u.IsActive).Select(u => new
                {
                    u.UserId, u.Username, u.FullName, RoleName = u.Role?.RoleName
                })
            });
        }

        // POST: api/Departments
        [HttpPost]
        [AuthorizePermission("NhanSu.Manage")]
        public async Task<IActionResult> PostDepartment([FromBody] DepartmentDto dto)
        {
            if (await _context.Departments.AnyAsync(d => d.DepartmentCode == dto.DepartmentCode))
                return BadRequest("Mã phòng ban đã tồn tại.");

            var dept = new Department
            {
                DepartmentName = dto.DepartmentName,
                DepartmentCode = dto.DepartmentCode?.ToUpper(),
                Description = dto.Description,
                CreatedAt = DateTime.Now
            };

            _context.Departments.Add(dept);
            await _context.SaveChangesAsync();
            return Ok(dept);
        }

        // PUT: api/Departments/{id}
        [HttpPut("{id}")]
        [AuthorizePermission("NhanSu.Manage")]
        public async Task<IActionResult> PutDepartment(int id, [FromBody] DepartmentDto dto)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return NotFound();

            dept.DepartmentName = dto.DepartmentName;
            dept.DepartmentCode = dto.DepartmentCode?.ToUpper();
            dept.Description = dto.Description;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật phòng ban thành công." });
        }

        // DELETE: api/Departments/{id}
        [HttpDelete("{id}")]
        [AuthorizePermission("NhanSu.Manage")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return NotFound();

            var hasStaff = await _context.Staffs.AnyAsync(s => s.DepartmentId == id && s.IsActive);
            if (hasStaff) return BadRequest("Không thể xóa phòng ban đang có nhân sự.");

            _context.Departments.Remove(dept);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa phòng ban thành công." });
        }
    }
}
