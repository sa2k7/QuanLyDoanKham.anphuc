using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Authorization;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Models.Enums;
using QuanLyDoanKham.API.Services.Auth;
using System.Security.Claims;

namespace QuanLyDoanKham.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IAuthService _authService;

        public AuthController(ApplicationDbContext context, IConfiguration configuration, IAuthService authService)
        {
            _context = context;
            _configuration = configuration;
            _authService = authService;
        }

        // GET: api/Auth/roles — Danh sách roles
        [HttpGet("roles")]
        [AuthorizePermission("HeThong.RoleManage")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _context.Roles.OrderBy(r => r.RoleId).ToListAsync();
            return Ok(roles);
        }

        // GET: api/Auth/permissions — Danh sách permissions theo module
        [HttpGet("permissions")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HeThong.RoleManage")]
        public async Task<IActionResult> GetPermissions()
        {
            var perms = await _context.Permissions
                .OrderBy(p => p.Module).ThenBy(p => p.PermissionId)
                .ToListAsync();
            return Ok(perms);
        }

        // GET: api/Auth/users
        [HttpGet("users")]
        [AuthorizePermission("HeThong.UserManage")]
        public async Task<ActionResult<IEnumerable<UserProfileDto>>> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Company)
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var list = users.Select(u => new UserProfileDto
            {
                UserId = u.UserId,
                Username = u.Username,
                FullName = u.FullName,
                RoleName = u.Role?.RoleName,
                RoleId = u.RoleId,
                Roles = u.UserRoles.Select(ur => ur.Role?.RoleName).OfType<string>().ToList(),
                CompanyId = u.CompanyId,
                CompanyName = u.Company?.CompanyName,
                Email = u.Email,
                AvatarPath = u.AvatarPath,
                IsActive = u.IsActive
            }).ToList();

            return Ok(list);
        }

        // GET: api/Auth/profile
        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<UserProfileDto>> GetProfile()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            var user = await _context.Users
                .Include(u => u.Role)
                    .ThenInclude(r => r!.RolePermissions).ThenInclude(rp => rp.Permission)
                .Include(u => u.Company)
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r!.RolePermissions).ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Username == username || u.Email == username);

            if (user == null) return NotFound();

            // Tổng hợp tất cả roles
            var allRoles = new HashSet<string>();
            if (user.Role != null) allRoles.Add(user.Role.RoleName);
            foreach (var ur in user.UserRoles) if (ur.Role != null) allRoles.Add(ur.Role.RoleName);

            // Tổng hợp tất cả permissions
            var allPerms = new HashSet<string>();
            if (user.Role?.RolePermissions != null)
                foreach (var rp in user.Role.RolePermissions)
                    if (rp.Permission != null) allPerms.Add(rp.Permission.PermissionKey);

            foreach (var ur in user.UserRoles ?? new List<UserRole>())
                if (ur.Role?.RolePermissions != null)
                    foreach (var rp in ur.Role.RolePermissions)
                        if (rp.Permission != null) allPerms.Add(rp.Permission.PermissionKey);

            return Ok(new UserProfileDto
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                RoleName = user.Role?.RoleName,
                RoleId = user.RoleId,
                Roles = allRoles.ToList(),
                Permissions = allPerms.ToList(),
                CompanyId = user.CompanyId,
                CompanyName = user.Company?.CompanyName,
                Email = user.Email,
                AvatarPath = user.AvatarPath,
                IsActive = user.IsActive
            });
        }

        // PUT: api/Auth/users/{username}
        [HttpPut("users/{username}")]
        [AuthorizePermission("HeThong.UserManage")]
        public async Task<IActionResult> UpdateUser(string username, UpdateUserDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return NotFound();

            var oldRoleId = user.RoleId;
            user.FullName = dto.FullName ?? user.FullName;
            user.RoleId = (dto.RoleId.HasValue && dto.RoleId.Value > 0) ? dto.RoleId.Value : user.RoleId;
            user.CompanyId = dto.CompanyId;
            user.Email = dto.Email;
            user.AvatarPath = dto.AvatarPath ?? user.AvatarPath;
            if (dto.IsActive.HasValue) user.IsActive = dto.IsActive.Value;

            if (!string.IsNullOrEmpty(dto.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // Cập nhật multi-roles nếu có
            if (dto.RoleIds != null && dto.RoleIds.Count > 0)
            {
                var existing = _context.UserRoles.Where(ur => ur.UserId == user.UserId);
                _context.UserRoles.RemoveRange(existing);
                foreach (var rid in dto.RoleIds.Distinct())
                {
                    _context.UserRoles.Add(new UserRole
                    {
                        UserId = user.UserId,
                        RoleId = rid,
                        AssignedAt = DateTime.Now,
                        AssignedBy = User.Identity?.Name ?? "system"
                    });
                }
            }

            await _context.SaveChangesAsync();

            if (oldRoleId != user.RoleId)
            {
                try
                {
                    var newRole = await _context.Roles.FindAsync(user.RoleId);
                    if (newRole != null)
                    {
                        _context.Notifications.Add(new Notification
                        {
                            UserId = user.UserId,
                            Message = $"Vai trò của bạn đã được cập nhật thành: {newRole.RoleName}.",
                            Link = "/profile",
                            CreatedAt = DateTime.Now,
                            IsRead = false
                        });
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[Notification Error] UpdateUser: {ex.Message}");
                }
            }

            return Ok(new { message = "Cập nhật tài khoản thành công" });
        }

        // POST: api/Auth/assign-roles — Gán roles cho user
        [HttpPost("assign-roles")]
        [AuthorizePermission("HeThong.RoleManage")]
        public async Task<IActionResult> AssignRoles([FromBody] AssignRolesDto dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null) return NotFound("Không tìm thấy user.");

            // Xóa tất cả roles cũ
            var existingRoles = _context.UserRoles.Where(ur => ur.UserId == dto.UserId);
            _context.UserRoles.RemoveRange(existingRoles);

            // Gán roles mới
            foreach (var roleId in dto.RoleIds.Distinct())
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = dto.UserId,
                    RoleId = roleId,
                    AssignedAt = DateTime.Now,
                    AssignedBy = User.Identity?.Name ?? "system"
                });
            }

            // Đặt primary role là role đầu tiên trong danh sách
            if (dto.RoleIds.Count > 0)
                user.RoleId = dto.RoleIds[0];

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Đã gán {dto.RoleIds.Count} vai trò cho {user.Username}." });
        }

        // GET: api/Auth/role-permissions/{roleId} — Lấy permissions của role
        [HttpGet("role-permissions/{roleId}")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HeThong.RoleManage")]
        public async Task<IActionResult> GetRolePermissions(int roleId)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null) return NotFound();

            var perms = await _context.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => new { rp.Permission!.PermissionId, rp.Permission.PermissionKey, rp.Permission.PermissionName, rp.Permission.Module })
                .ToListAsync();

            return Ok(perms);
        }

        // PUT: api/Auth/role-permissions/{roleId} — Cập nhật permissions cho role
        [HttpPut("role-permissions/{roleId}")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HeThong.RoleManage")]
        public async Task<IActionResult> UpdateRolePermissions(int roleId, [FromBody] AssignPermissionsDto dto)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null) return NotFound("Không tìm thấy role.");

            // Removed Admin restriction to allow testing/full control.

            // Xóa cũ bằng ExecuteDeleteAsync để tránh lỗi UNIQUE constraint của EF Core khi thêm mới trùng key
            await _context.RolePermissions.Where(rp => rp.RoleId == roleId).ExecuteDeleteAsync();

            // Thêm mới
            foreach (var permId in dto.PermissionIds.Distinct())
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permId
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Đã cập nhật {dto.PermissionIds.Count} quyền cho role {role.RoleName}." });
        }

        // POST: api/Auth/restore-default-permissions/{roleId}
        [HttpPost("restore-default-permissions/{roleId}")]
        [QuanLyDoanKham.API.Authorization.AuthorizePermission("HeThong.RoleManage")]
        public async Task<IActionResult> RestoreDefaultPermissions(int roleId)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null) return NotFound("Không tìm thấy role.");

            var permissionsToRestore = new List<int>();

            if (role.RoleName == "Admin")
            {
                // Admin gets ALL permissions by default
                permissionsToRestore = await _context.Permissions.Select(p => p.PermissionId).ToListAsync();
            }
            else
            {
                // Define default permission keys per role based on seed data
                var defaultKeys = new List<string>();

                switch (role.RoleName)
                {
                    case "ContractManager":
                        defaultKeys = new List<string> { "HopDong.View", "HopDong.Create", "HopDong.Edit", "HopDong.Approve", "HopDong.Reject", "HopDong.Upload" };
                        break;
                    case "MedicalGroupManager":
                        defaultKeys = new List<string> { "DoanKham.View", "DoanKham.Create", "DoanKham.Edit", "DoanKham.SetPosition", "DoanKham.AssignStaff", "DoanKham.ManageOwn", "LichKham.ViewAll" };
                        break;
                    case "PersonnelManager":
                        defaultKeys = new List<string> { "NhanSu.View", "NhanSu.Manage", "DoanKham.AssignStaff", "LichKham.ViewAll" };
                        break;
                    case "PayrollManager":
                        defaultKeys = new List<string> { "Luong.View", "Luong.Manage", "NhanSu.View", "LichKham.ViewAll", "ChamCong.ViewAll" };
                        break;
                    case "WarehouseManager":
                        defaultKeys = new List<string> { "Kho.View", "Kho.Import", "Kho.Export" };
                        break;
                    case "GroupLeader":
                        defaultKeys = new List<string> { "DoanKham.View", "ChamCong.QR", "ChamCong.CheckInOut", "LichKham.ViewAll" };
                        break;
                    case "MedicalStaff":
                        defaultKeys = new List<string> { "LichKham.ViewOwn" };
                        break;
                    case "Accountant":
                        defaultKeys = new List<string> { "BaoCao.View", "BaoCao.Export", "Luong.View", "HopDong.View" };
                        break;
                    default:
                        return BadRequest("Vai trò này không có bộ quyền mặc định.");
                }

                permissionsToRestore = await _context.Permissions
                    .Where(p => defaultKeys.Contains(p.PermissionKey))
                    .Select(p => p.PermissionId)
                    .ToListAsync();
            }

            // Clear existing using ExecuteDeleteAsync directly on DB to avoid UNIQUE constraint issue
            await _context.RolePermissions.Where(rp => rp.RoleId == roleId).ExecuteDeleteAsync();

            // Add defaults
            foreach (var permId in permissionsToRestore)
            {
                _context.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = permId });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Đã khôi phục {permissionsToRestore.Count} quyền mặc định cho vai trò {role.RoleName}." });
        }



        // DELETE: api/Auth/users/{username}

        [HttpDelete("users/{username}")]
        [AuthorizePermission("HeThong.UserManage")]
        public async Task<IActionResult> DeleteUser(string username)
        {
            if (username == "admin") return BadRequest("Không thể xóa tài khoản Admin mặc định.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa tài khoản thành công" });
        }

        // POST: api/Auth/logout
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            if (!string.IsNullOrEmpty(username))
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user != null)
                {
                    user.RefreshToken = null;
                    user.RefreshTokenExpiry = null;
                    await _context.SaveChangesAsync();
                }
            }
            return Ok(new { message = "Đăng xuất thành công" });
        }

        [HttpPost("login")]
        [EnableRateLimiting("LoginPolicy")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto request)
        {
            var result = await _authService.LoginAsync(request);
            if (!result.IsSuccess)
                return Unauthorized(new { message = result.Message });

            return Ok(result.Data!);
        }

        // GET: api/Auth/verify-token
        [HttpGet("verify-token")]
        [Authorize]
        public IActionResult VerifyToken()
        {
            // Nếu qua được [Authorize] nghĩa là token hợp lệ
            return Ok(new { valid = true, message = "Token hợp lệ" });
        }

        [HttpPost("register")]
        [AuthorizePermission("HeThong.UserManage")]
        public async Task<ActionResult<UserProfileDto>> Register(RegisterDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest("Username already exists.");

            var newUser = new AppUser
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName,
                RoleId = request.RoleId,
                CompanyId = request.CompanyId,
                Email = request.Email,
                AvatarPath = request.AvatarPath,
                StaffId = request.StaffId // Nhận liên kết từ FE nếu có
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            if (request.AdditionalRoleIds != null && request.AdditionalRoleIds.Count > 0)
            {
                foreach (var rid in request.AdditionalRoleIds.Distinct())
                {
                    _context.UserRoles.Add(new UserRole
                    {
                        UserId = newUser.UserId,
                        RoleId = rid,
                        AssignedAt = DateTime.Now,
                        AssignedBy = User.Identity?.Name ?? "system"
                    });
                }
                await _context.SaveChangesAsync();
            }

            // Return DTO to avoid circular reference and match GET responses
            return Ok(new UserProfileDto
            {
                UserId = newUser.UserId,
                Username = newUser.Username,
                FullName = newUser.FullName,
                RoleId = newUser.RoleId,
                RoleName = (await _context.Roles.FindAsync(newUser.RoleId))?.RoleName,
                Email = newUser.Email,
                CompanyId = newUser.CompanyId
            });
        }

        // POST: api/Auth/upload-avatar
        [HttpPost("upload-avatar")]
        [Authorize]
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

        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken(RefreshTokenDto request)
        {
            var result = await _authService.RefreshTokenAsync(request);
            if (!result.IsSuccess)
                return Unauthorized(new { message = result.Message });

            return Ok(result.Data!);
        }

        // POST: api/Auth/change-password
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto request)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            var result = await _authService.ChangePasswordAsync(username, request);
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        // POST: api/Auth/request-reset
        [HttpPost("request-reset")]
        public async Task<IActionResult> RequestReset([FromBody] ResetRequestDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.Username)) 
                return BadRequest("Vui lòng cung cấp tên đăng nhập.");

            var result = await _authService.RequestResetAsync(request);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        // GET: api/Auth/reset-requests
        [HttpGet("reset-requests")]
        [AuthorizePermission("HeThong.UserManage")]
        public async Task<IActionResult> GetResetRequests()
        {
            var list = await _context.PasswordResetRequests
                .Where(r => !r.IsProcessed)
                .OrderByDescending(r => r.RequestedDate)
                .Take(50)
                .ToListAsync();
            return Ok(list);
        }

        // POST: api/Auth/process-reset
        [HttpPost("process-reset")]
        [AuthorizePermission("HeThong.UserManage")]
        public async Task<IActionResult> ProcessReset([FromBody] ProcessResetDto dto)
        {
            var result = await _authService.ProcessResetAsync(dto);
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }
        // GET: api/Auth/seed-mock-data
        [HttpGet("seed-mock-data")]
        [AllowAnonymous] // Cho phép chạy Tool Seed mà không cần login (Dành cho Dev)
        public async Task<IActionResult> SeedMockData()
        {
            try
            {
                await _context.SaveChangesAsync();

                // 2. Seed Staff (Admin)
                var adminStaff = await _context.Staffs.FirstOrDefaultAsync(s => s.EmployeeCode == "admin");
                if (adminStaff == null)
                {
                    adminStaff = new Staff
                    {
                        FullName = "Bác sĩ Admin",
                        EmployeeCode = "admin",
                        Email = "admin@anphuc.vn",
                        PhoneNumber = "0901234567",
                        DepartmentName = "Trạm Siêu Âm",
                        BaseSalary = 15000000,
                        CreatedDate = DateTime.Now.Date,
                        IsActive = true
                    };
                    _context.Staffs.Add(adminStaff);
                    await _context.SaveChangesAsync();
                }

                // 3. Map AppUser and Repair existing data
                var allUsers = await _context.Users.ToListAsync();
                var allStaffs = await _context.Staffs.ToListAsync();

                foreach (var user in allUsers)
                {
                    if (user.StaffId == null)
                    {
                        var staff = allStaffs.FirstOrDefault(s => string.Equals(s.EmployeeCode, user.Username, StringComparison.OrdinalIgnoreCase));
                        if (staff != null)
                        {
                            user.StaffId = staff.StaffId;
                        }
                    }
                }
                await _context.SaveChangesAsync();

                // 4. Seed HealthContract for MedicalGroup
                var dummyCompany = await _context.Companies.FirstOrDefaultAsync(c => c.TaxCode == "000000000");
                if (dummyCompany == null)
                {
                    dummyCompany = new Company { CompanyName = "Công ty Mẫu Sài Gòn", TaxCode = "000000000", Address = "HCM" };
                    _context.Companies.Add(dummyCompany);
                    await _context.SaveChangesAsync();
                }

                var dummyContract = await _context.Contracts.FirstOrDefaultAsync(c => c.ContractCode == "HD_MAU_2026");
                if (dummyContract == null)
                {
                    dummyContract = new HealthContract { ContractCode = "HD_MAU_2026", ContractName = "Hợp đồng Mẫu 2026", CompanyId = dummyCompany.CompanyId, ExpectedQuantity = 100, StartDate = DateTime.Now.Date.AddDays(-30), EndDate = DateTime.Now.Date.AddDays(30), Status = ContractStatus.Active, TotalAmount = 100000000 };
                    _context.Contracts.Add(dummyContract);
                    await _context.SaveChangesAsync();
                }

                // 5. Seed MedicalGroups
                var group1 = await _context.MedicalGroups.FirstOrDefaultAsync(g => g.GroupName == "Đoàn khám Điện lực SG");
                if (group1 == null)
                {
                    group1 = new MedicalGroup { GroupName = "Đoàn khám Điện lực SG", ExamDate = DateTime.Now.Date, HealthContractId = dummyContract.HealthContractId, Status = "Approved", CreatedAt = DateTime.Now };
                    _context.MedicalGroups.Add(group1);
                }

                var group2 = await _context.MedicalGroups.FirstOrDefaultAsync(g => g.GroupName == "Đoàn khám GV Bình Thạnh");
                if (group2 == null)
                {
                    group2 = new MedicalGroup { GroupName = "Đoàn khám GV Bình Thạnh", ExamDate = DateTime.Now.Date.AddDays(-3), HealthContractId = dummyContract.HealthContractId, Status = "Finished", CreatedAt = DateTime.Now.AddDays(-5) };
                    _context.MedicalGroups.Add(group2);
                }
                
                await _context.SaveChangesAsync();

                // 6. Seed GroupStaffDetails
                if (!await _context.GroupStaffDetails.AnyAsync(g => g.GroupId == group1.GroupId && g.StaffId == adminStaff.StaffId))
                {
                    _context.GroupStaffDetails.Add(new GroupStaffDetail { GroupId = group1.GroupId, StaffId = adminStaff.StaffId, WorkPosition = "Bác sĩ Siêu âm", WorkStatus = "Pending", ExamDate = group1.ExamDate, ShiftType = 1.0, CalculatedSalary = 300000 });
                }

                if (!await _context.GroupStaffDetails.AnyAsync(g => g.GroupId == group2.GroupId && g.StaffId == adminStaff.StaffId))
                {
                    _context.GroupStaffDetails.Add(new GroupStaffDetail { GroupId = group2.GroupId, StaffId = adminStaff.StaffId, WorkPosition = "Bác sĩ Siêu âm", WorkStatus = "Joined", ExamDate = group2.ExamDate, ShiftType = 1.0, CalculatedSalary = 300000 });
                }
                
                // 7. Seed ScheduleCalendars
                if (!await _context.ScheduleCalendars.AnyAsync(c => c.GroupId == group1.GroupId && c.StaffId == adminStaff.StaffId && c.ExamDate == group1.ExamDate))
                {
                    _context.ScheduleCalendars.Add(new ScheduleCalendar { GroupId = group1.GroupId, StaffId = adminStaff.StaffId, ExamDate = group1.ExamDate, CheckInTime = group1.ExamDate.AddHours(8), IsConfirmed = false, Note = "Check-in QR" });
                }

                if (!await _context.ScheduleCalendars.AnyAsync(c => c.GroupId == group2.GroupId && c.StaffId == adminStaff.StaffId && c.ExamDate == group2.ExamDate))
                {
                    _context.ScheduleCalendars.Add(new ScheduleCalendar { GroupId = group2.GroupId, StaffId = adminStaff.StaffId, ExamDate = group2.ExamDate, CheckInTime = group2.ExamDate.AddHours(8), CheckOutTime = group2.ExamDate.AddHours(17), IsConfirmed = true, Note = "Đủ công" });
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Seeding data successfully via EF Core!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, detail = ex.InnerException?.Message });
            }
        }
    }
}
