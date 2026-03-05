using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Helpers;

namespace QuanLyDoanKham.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StaffsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StaffsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Staffs — Admin và Staff được xem
        [HttpGet]
        [Authorize(Roles = "Admin,Staff,PersonnelManager")] // Cho phép PersonnelManager xem danh sách
        public async Task<ActionResult<IEnumerable<StaffDto>>> GetStaffs()
        {
            var staffs = await _context.Staffs
                .Where(s => s.IsActive)
                .Include(s => s.GroupStaffDetails)
                    .ThenInclude(gsd => gsd.MedicalGroup)
                .ToListAsync();

            var users = await _context.Users.Include(u => u.Role).ToListAsync();

            var result = staffs.Select(s => {
                // Tìm đoàn khám hiện tại (chưa kết thúc) mà nhân viên này tham gia
                var activeGroupDetail = s.GroupStaffDetails
                    .FirstOrDefault(gsd => gsd.MedicalGroup != null && !gsd.MedicalGroup.IsFinished);

                // Tìm thông tin vai trò từ tài khoản tương ứng
                var userAccount = users.FirstOrDefault(u => u.Username.Equals(s.EmployeeCode, StringComparison.OrdinalIgnoreCase));

                return new StaffDto
                {
                    StaffId = s.StaffId,
                    EmployeeCode = s.EmployeeCode,
                    FullName = s.FullName,
                    FullNameUnsigned = s.FullNameUnsigned,
                    BirthYear = s.BirthYear,
                    Gender = s.Gender,
                    IDCardNumber = s.IDCardNumber,
                    TaxCode = s.TaxCode,
                    BankAccountNumber = s.BankAccountNumber,
                    BankAccountName = s.BankAccountName,
                    BankName = s.BankName,
                    PhoneNumber = s.PhoneNumber,
                    JobTitle = s.JobTitle,
                    Department = s.Department,
                    EmployeeType = s.EmployeeType,
                    AvatarPath = s.AvatarPath,
                    BaseSalary = s.BaseSalary,
                    IsActive = s.IsActive,
                    IsWorking = activeGroupDetail != null,
                    CurrentGroupName = activeGroupDetail?.MedicalGroup?.GroupName,
                    SystemRole = userAccount?.Role?.RoleName ?? "MedicalStaff"
                };
            }).ToList();

            return Ok(result);
        }

        // GET: api/Staffs/5 — Admin và Staff được xem
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult<StaffDetailDto>> GetStaff(int id)
        {
            var staff = await _context.Staffs
                .Include(s => s.GroupStaffDetails)
                    .ThenInclude(gsd => gsd.MedicalGroup)
                .FirstOrDefaultAsync(s => s.StaffId == id);

            if (staff == null || !staff.IsActive)
            {
                return NotFound();
            }

            var dto = new StaffDetailDto
            {
                StaffId = staff.StaffId,
                EmployeeCode = staff.EmployeeCode,
                FullName = staff.FullName,
                FullNameUnsigned = staff.FullNameUnsigned,
                BirthYear = staff.BirthYear,
                Gender = staff.Gender,
                IDCardNumber = staff.IDCardNumber,
                TaxCode = staff.TaxCode,
                BankAccountNumber = staff.BankAccountNumber,
                BankAccountName = staff.BankAccountName,
                BankName = staff.BankName,
                PhoneNumber = staff.PhoneNumber,
                JobTitle = staff.JobTitle,
                Department = staff.Department,
                EmployeeType = staff.EmployeeType,
                IDCardFrontPath = staff.IDCardFrontPath,
                IDCardBackPath = staff.IDCardBackPath,
                PracticeCertificatePath = staff.PracticeCertificatePath,
                AvatarPath = staff.AvatarPath,
                BaseSalary = staff.BaseSalary,
                IsActive = staff.IsActive,
                Workdays = staff.GroupStaffDetails.Select(gsd => new StaffWorkdayDto
                {
                    Date = gsd.MedicalGroup.ExamDate,
                    GroupName = gsd.MedicalGroup.GroupName
                }).ToList(),
                Shifts = staff.GroupStaffDetails.Select(gsd => new StaffShiftDto
                {
                    GroupName = gsd.MedicalGroup.GroupName,
                    ShiftType = gsd.ShiftType,
                    CalculatedSalary = gsd.CalculatedSalary
                }).ToList()
            };

            return dto;
        }

        // POST: api/Staffs — Admin và PersonnelManager được tạo mới
        [HttpPost]
        [Authorize(Roles = "Admin,PersonnelManager")]
        public async Task<ActionResult<Staff>> PostStaff(StaffDto dto)
        {
            var staff = new Staff
            {
                EmployeeCode = string.IsNullOrEmpty(dto.EmployeeCode) ? $"NV{DateTime.Now:yyyyMMddHHmmss}" : dto.EmployeeCode,
                FullName = dto.FullName,
                FullNameUnsigned = StringHelper.RemoveVietnameseAccents(dto.FullName).ToUpper(),
                BirthYear = dto.BirthYear,
                Gender = dto.Gender,
                IDCardNumber = dto.IDCardNumber,
                TaxCode = dto.TaxCode,
                BankAccountNumber = dto.BankAccountNumber,
                BankAccountName = dto.BankAccountName,
                BankName = dto.BankName,
                PhoneNumber = dto.PhoneNumber,
                JobTitle = dto.JobTitle,
                Department = dto.Department,
                EmployeeType = dto.EmployeeType,
                IDCardFrontPath = dto.IDCardFrontPath,
                IDCardBackPath = dto.IDCardBackPath,
                PracticeCertificatePath = dto.PracticeCertificatePath,
                AvatarPath = dto.AvatarPath,
                BaseSalary = dto.BaseSalary,
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            _context.Staffs.Add(staff);
            await _context.SaveChangesAsync();

            // Tự động tạo tài khoản người dùng cho nhân viên với vai trò được chọn
            string roleToAssign = string.IsNullOrEmpty(dto.SystemRole) ? "MedicalStaff" : dto.SystemRole;
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleToAssign);
            
            if (userRole != null)
            {
                var newUser = new AppUser
                {
                    Username = staff.EmployeeCode.ToLower(),
                    FullName = staff.FullName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password@123"),
                    RoleId = userRole.RoleId,
                    AvatarPath = staff.AvatarPath
                };

                // Kiểm tra xem username đã tồn tại chưa
                if (!await _context.Users.AnyAsync(u => u.Username == newUser.Username))
                {
                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync();
                }
            }

            dto.StaffId = staff.StaffId;
            return CreatedAtAction(nameof(GetStaff), new { id = staff.StaffId }, dto);
        }

        // PUT: api/Staffs/5 — Admin và PersonnelManager được sửa
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,PersonnelManager")]
        public async Task<IActionResult> PutStaff(int id, StaffDto dto)
        {
            if (id != dto.StaffId)
            {
                return BadRequest();
            }

            var staff = await _context.Staffs.FindAsync(id);
            if (staff == null)
            {
                return NotFound();
            }

            staff.EmployeeCode = dto.EmployeeCode;
            staff.FullName = dto.FullName;
            staff.FullNameUnsigned = StringHelper.RemoveVietnameseAccents(dto.FullName).ToUpper();
            staff.BirthYear = dto.BirthYear;
            staff.Gender = dto.Gender;
            staff.IDCardNumber = dto.IDCardNumber;
            staff.TaxCode = dto.TaxCode;
            staff.BankAccountNumber = dto.BankAccountNumber;
            staff.BankAccountName = dto.BankAccountName;
            staff.BankName = dto.BankName;
            staff.PhoneNumber = dto.PhoneNumber;
            staff.JobTitle = dto.JobTitle;
            staff.Department = dto.Department;
            staff.EmployeeType = dto.EmployeeType;
            staff.AvatarPath = dto.AvatarPath;
            staff.BaseSalary = dto.BaseSalary;
            staff.ModifiedDate = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();

                // Cập nhật vai trò hệ thống nếu có thay đổi
                if (User.IsInRole("Admin") && !string.IsNullOrEmpty(dto.SystemRole))
                {
                    var userAccount = await _context.Users.FirstOrDefaultAsync(u => u.Username.Equals(staff.EmployeeCode, StringComparison.OrdinalIgnoreCase));
                    if (userAccount != null)
                    {
                        var newRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == dto.SystemRole);
                        if (newRole != null && userAccount.RoleId != newRole.RoleId)
                        {
                            userAccount.RoleId = newRole.RoleId;
                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StaffExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Staffs/5 (Soft Delete) — Admin và PersonnelManager được xóa
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,PersonnelManager")]
        public async Task<IActionResult> DeleteStaff(int id)
        {
            var staff = await _context.Staffs.FindAsync(id);
            if (staff == null)
            {
                return NotFound();
            }

            staff.IsActive = false;
            staff.ModifiedDate = DateTime.Now;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Staffs/upload-avatar
        [HttpPost("upload-avatar")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadAvatar([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"uploads/avatars/{fileName}";
            return Ok(new { path = relativePath });
        }

        private bool StaffExists(int id)
        {
            return _context.Staffs.Any(e => e.StaffId == id);
        }
    }
}
