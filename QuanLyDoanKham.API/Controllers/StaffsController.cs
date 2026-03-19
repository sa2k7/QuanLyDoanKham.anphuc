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

        // GET: api/Staffs — Admin, MedicalStaff và PersonnelManager được xem
        [HttpGet]
        [Authorize(Roles = "Admin,MedicalStaff,PersonnelManager")] // Fix role name 'Staff' -> 'MedicalStaff' bh pk bh
        public async Task<ActionResult<IEnumerable<StaffDto>>> GetStaffs()
        {
            var staffs = await _context.Staffs
                .Where(s => s.IsActive)
                .Include(s => s.GroupStaffDetails)
                    .ThenInclude(gsd => gsd.MedicalGroup)
                .ToListAsync();

            var users = await _context.Users.Include(u => u.Role).ToListAsync();

            var result = staffs.Select(s => {
                // Tìm đoàn khám hiện tại (chưa kết thúc hoặc chưa khóa) mà nhân viên này tham gia
                var activeGroupDetail = s.GroupStaffDetails
                    .FirstOrDefault(gsd => gsd.MedicalGroup != null && (gsd.MedicalGroup.Status == "Open"));

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
                    Email = s.Email,
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

        // GET: api/Staffs/5 — Admin, MedicalStaff, PersonnelManager và MedicalGroupManager được xem
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,MedicalStaff,PersonnelManager,MedicalGroupManager")] // Chuẩn hóa vai trò bh pk bh
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

            var userAccount = await _context.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == staff.EmployeeCode.ToLower());

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
                Email = staff.Email,
                JobTitle = staff.JobTitle,
                Department = staff.Department,
                EmployeeType = staff.EmployeeType,
                IDCardFrontPath = staff.IDCardFrontPath,
                IDCardBackPath = staff.IDCardBackPath,
                PracticeCertificatePath = staff.PracticeCertificatePath,
                AvatarPath = staff.AvatarPath,
                BaseSalary = staff.BaseSalary,
                IsActive = staff.IsActive,
                SystemRole = userAccount?.Role?.RoleName ?? "MedicalStaff",
                Workdays = staff.GroupStaffDetails.Select(gsd => new StaffWorkdayDto
                {
                    Date = gsd.MedicalGroup.ExamDate,
                    GroupName = gsd.MedicalGroup.GroupName,
                    WorkPosition = gsd.WorkPosition
                }).ToList(),
                Shifts = staff.GroupStaffDetails.Select(gsd => new StaffShiftDto
                {
                    GroupName = gsd.MedicalGroup.GroupName,
                    ShiftType = gsd.ShiftType,
                    CalculatedSalary = gsd.CalculatedSalary,
                    WorkPosition = gsd.WorkPosition
                }).ToList()
            };

            return dto;
        }

        // POST: api/Staffs — Admin và PersonnelManager được tạo mới
        [HttpPost]
        [Authorize(Roles = "Admin,PersonnelManager")]
        public async Task<ActionResult<Staff>> PostStaff(StaffDto dto)
        {
            // Tự động sinh mã nếu trống
            if (string.IsNullOrEmpty(dto.EmployeeCode))
            {
                dto.EmployeeCode = await GenerateNextEmployeeCode();
            }

            // Kiểm tra trùng lặp
            if (!string.IsNullOrEmpty(dto.EmployeeCode) && await _context.Staffs.AnyAsync(s => s.EmployeeCode == dto.EmployeeCode && s.IsActive))
                return BadRequest("Mã nhân viên đã tồn tại.");
            if (!string.IsNullOrEmpty(dto.IDCardNumber) && await _context.Staffs.AnyAsync(s => s.IDCardNumber == dto.IDCardNumber && s.IsActive))
                return BadRequest("Số CCCD/CMND đã được đăng ký cho nhân sự khác.");
            if (!string.IsNullOrEmpty(dto.TaxCode) && await _context.Staffs.AnyAsync(s => s.TaxCode == dto.TaxCode && s.IsActive))
                return BadRequest("Mã số thuế cá nhân đã tồn tại.");
            if (!string.IsNullOrEmpty(dto.PhoneNumber) && await _context.Staffs.AnyAsync(s => s.PhoneNumber == dto.PhoneNumber && s.IsActive))
                return BadRequest("Số điện thoại đã được đăng ký cho nhân sự khác.");

            var staff = new Staff
            {
                EmployeeCode = dto.EmployeeCode,
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
                Email = dto.Email,
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
                var username = staff.EmployeeCode.ToLower();
                var initialPassword = "Password@123";
                var newUser = new AppUser
                {
                    Username = username,
                    FullName = staff.FullName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(initialPassword),
                    RoleId = userRole.RoleId,
                    AvatarPath = staff.AvatarPath
                };

                // Kiểm tra xem username đã tồn tại chưa
                if (!await _context.Users.AnyAsync(u => u.Username == newUser.Username))
                {
                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync();

                    // Gửi thông báo thông tin tài khoản qua Email nêú có
                    if (!string.IsNullOrEmpty(staff.Email))
                    {
                        EmailHelper.SendAccountCredentialNotification(staff.Email, staff.FullName, username, initialPassword);
                    }
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

            // Kiểm tra trùng lặp (loại trừ chính nó)
            if (!string.IsNullOrEmpty(dto.EmployeeCode) && await _context.Staffs.AnyAsync(s => s.EmployeeCode == dto.EmployeeCode && s.StaffId != id && s.IsActive))
                return BadRequest("Mã nhân viên đã tồn tại.");
            if (!string.IsNullOrEmpty(dto.IDCardNumber) && await _context.Staffs.AnyAsync(s => s.IDCardNumber == dto.IDCardNumber && s.StaffId != id && s.IsActive))
                return BadRequest("Số CCCD/CMND đã tồn tại.");
            if (!string.IsNullOrEmpty(dto.TaxCode) && await _context.Staffs.AnyAsync(s => s.TaxCode == dto.TaxCode && s.StaffId != id && s.IsActive))
                return BadRequest("Mã số thuế đã tồn tại.");
            if (!string.IsNullOrEmpty(dto.PhoneNumber) && await _context.Staffs.AnyAsync(s => s.PhoneNumber == dto.PhoneNumber && s.StaffId != id && s.IsActive))
                return BadRequest("Số điện thoại đã tồn tại.");

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
            staff.Email = dto.Email;
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
                if (!string.IsNullOrEmpty(dto.SystemRole))
                {
                    var userAccount = await _context.Users.FirstOrDefaultAsync(u => u.Username == staff.EmployeeCode.ToLower());
                    var newRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == dto.SystemRole);
                    
                    if (userAccount != null && newRole != null)
                    {
                        bool isRoleChanged = userAccount.RoleId != newRole.RoleId;
                        userAccount.RoleId = newRole.RoleId;
                        userAccount.FullName = staff.FullName;
                        userAccount.AvatarPath = staff.AvatarPath;
                        await _context.SaveChangesAsync();

                        // Nếu đổi vai trò và có email thì thông báo
                        if (isRoleChanged && !string.IsNullOrEmpty(staff.Email))
                        {
                            EmailHelper.SendRoleChangeNotification(staff.Email, staff.FullName, newRole.RoleName);
                        }
                    }
                    else if (userAccount == null && newRole != null)
                    {
                        // Nếu chưa có tài khoản, tự động tạo mới khi Update Staff
                        var initialPassword = "Password@123";
                        var newUser = new AppUser
                        {
                            Username = staff.EmployeeCode.ToLower(),
                            FullName = staff.FullName,
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword(initialPassword),
                            RoleId = newRole.RoleId,
                            AvatarPath = staff.AvatarPath
                        };
                        _context.Users.Add(newUser);
                        await _context.SaveChangesAsync();

                        // Thông báo tài khoản mới
                        if (!string.IsNullOrEmpty(staff.Email))
                        {
                            EmailHelper.SendAccountCredentialNotification(staff.Email, staff.FullName, newUser.Username, initialPassword);
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

            return Ok(staff);
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
        [Authorize(Roles = "Admin,PersonnelManager")]
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

        // GET: api/Staffs/export
        [HttpGet("export")]
        [Authorize(Roles = "Admin,PersonnelManager")]
        public async Task<IActionResult> ExportStaffExcel()
        {
            var staffs = await _context.Staffs.Where(s => s.IsActive).ToListAsync();

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("NhanSu");
                
                // Header
                worksheet.Cell(1, 1).Value = "Mã NV";
                worksheet.Cell(1, 2).Value = "Họ và Tên";
                worksheet.Cell(1, 3).Value = "Chức vụ";
                worksheet.Cell(1, 4).Value = "Lương cơ bản";
                
                var headerRange = worksheet.Range(1, 1, 1, 4);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.AliceBlue;

                int row = 2;
                foreach (var s in staffs)
                {
                    worksheet.Cell(row, 1).Value = s.EmployeeCode;
                    worksheet.Cell(row, 2).Value = s.FullName;
                    worksheet.Cell(row, 3).Value = s.JobTitle;
                    worksheet.Cell(row, 4).Value = s.BaseSalary;
                    row++;
                }
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DanhSachNhanSu.xlsx");
                }
            }
        }

        // POST: api/Staffs/import — Admin và PersonnelManager được tạo mới
        [HttpPost("import")]
        [Authorize(Roles = "Admin,PersonnelManager")]
        public async Task<IActionResult> ImportStaffExcel(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("File không hợp lệ.");

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var workbook = new ClosedXML.Excel.XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Bỏ qua header

                    var medicalStaffRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "MedicalStaff");

                    foreach (var r in rows)
                    {
                        var employeeCode = r.Cell(1).Value.ToString();
                        var fullName = r.Cell(2).Value.ToString();
                        if (string.IsNullOrEmpty(fullName)) continue;

                        if (await _context.Staffs.AnyAsync(s => s.EmployeeCode == employeeCode && s.IsActive)) continue;

                        var staff = new Staff
                        {
                            EmployeeCode = string.IsNullOrEmpty(employeeCode) ? await GenerateNextEmployeeCode() : employeeCode,
                            FullName = fullName,
                            FullNameUnsigned = StringHelper.RemoveVietnameseAccents(fullName).ToUpper(),
                            JobTitle = r.Cell(3).Value.ToString() ?? "Bác sĩ",
                            BaseSalary = decimal.TryParse(r.Cell(4).Value.ToString(), out var salary) ? salary : 1000000,
                            IsActive = true,
                            CreatedDate = DateTime.Now
                        };
                        _context.Staffs.Add(staff);
                        await _context.SaveChangesAsync(); // Save staff first to get ID/Code

                        // Tự động tạo tài khoản
                        if (medicalStaffRole != null)
                        {
                            var username = staff.EmployeeCode.ToLower();
                            if (!await _context.Users.AnyAsync(u => u.Username == username))
                            {
                                var newUser = new AppUser
                                {
                                    Username = username,
                                    FullName = staff.FullName,
                                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password@123"),
                                    RoleId = medicalStaffRole.RoleId
                                };
                                _context.Users.Add(newUser);
                            }
                        }
                    }
                    await _context.SaveChangesAsync();
                }
            }
            return Ok(new { message = "Đã nhập dữ liệu nhân sự và tạo tài khoản thành công!" });
        }

        private async Task<string> GenerateNextEmployeeCode()
        {
            // Lấy mã NV lớn nhất (bắt đầu bằng NV và theo sau là số)
            var lastStaff = await _context.Staffs
                .Where(s => s.EmployeeCode.StartsWith("NV") && s.EmployeeCode.Length > 2)
                .OrderByDescending(s => s.EmployeeCode)
                .FirstOrDefaultAsync();

            if (lastStaff == null) return "NV001";

            // Thử lấy phần số từ NVxxx
            var currentCode = lastStaff.EmployeeCode.Substring(2);
            if (int.TryParse(currentCode, out int number))
            {
                return $"NV{(number + 1).ToString("D3")}";
            }

            // Nếu mã NV hiện tại không phải số (vd: NV_BACSI), đếm tổng số NV để fallback
            var count = await _context.Staffs.CountAsync();
            return $"NV{(count + 1).ToString("D3")}";
        }

        private bool StaffExists(int id)
        {
            return _context.Staffs.Any(e => e.StaffId == id);
        }
    }
}
