using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;
using System.Text;

namespace QuanLyDoanKham.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MedicalGroupsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly Services.IGeminiService _geminiService;

        public MedicalGroupsController(ApplicationDbContext context, Services.IGeminiService geminiService)
        {
            _context = context;
            _geminiService = geminiService;
        }

        // GET: api/MedicalGroups
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MedicalGroupDto>>> GetMedicalGroups()
        {
            var today = DateTime.Today;
            return await _context.MedicalGroups
                .Include(g => g.HealthContract)
                .ThenInclude(c => c.Company)
                .OrderByDescending(g => g.GroupId)
                .Select(g => new MedicalGroupDto
                {
                    GroupId = g.GroupId,
                    GroupName = g.GroupName,
                    ExamDate = g.ExamDate,
                    HealthContractId = g.HealthContractId,
                    ShortName = g.HealthContract.Company.ShortName,
                    CompanyName = g.HealthContract.Company.CompanyName,
                    Status = g.Status,
                    ImportFilePath = g.ImportFilePath
                })
                .ToListAsync();
        }

        // POST: api/MedicalGroups
        [HttpPost]
        [Authorize(Roles = "Admin,MedicalGroupManager")]
        public async Task<ActionResult<MedicalGroup>> PostMedicalGroup(MedicalGroupDto dto)
        {
            var contract = await _context.Contracts.FindAsync(dto.HealthContractId);
            if (contract == null) return NotFound("Không tìm thấy hợp đồng.");
            
            if (contract.Status != "Active" && contract.Status != "Approved") 
                return BadRequest("Hợp đồng chưa được phê duyệt hoặc đã kết thúc. Chỉ hợp đồng 'Approved' hoặc 'Active' mới được tạo đoàn khám.");

            var entity = new MedicalGroup
            {
                GroupName = dto.GroupName,
                ExamDate = dto.ExamDate,
                HealthContractId = dto.HealthContractId,
                Status = "Open",
                ImportFilePath = dto.ImportFilePath
            };

            _context.MedicalGroups.Add(entity);
            await _context.SaveChangesAsync();
            return Ok(entity);
        }

        // PUT: api/MedicalGroups/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,MedicalGroupManager")]
        public async Task<IActionResult> PutMedicalGroup(int id, MedicalGroupDto dto)
        {
            var group = await _context.MedicalGroups.FindAsync(id);
            if (group == null) return NotFound();

            if (group.Status == "Locked") return BadRequest("Đoàn khám đã bị khóa.");

            group.GroupName = dto.GroupName;
            group.ExamDate = dto.ExamDate;
            group.Status = dto.Status;
            group.ImportFilePath = dto.ImportFilePath;

            await _context.SaveChangesAsync();
            return Ok(group);
        }

        // PUT: api/MedicalGroups/{id}/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,MedicalGroupManager")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] StatusUpdateDto dto)
        {
            var group = await _context.MedicalGroups.FindAsync(id);
            if (group == null) return NotFound();

            // Nếu đã khóa thì không cho đổi nữa trừ khi là Admin (tùy chính sách, hiện tại giữ đơn giản)
            if (group.Status == "Locked" && !User.IsInRole("Admin")) 
                return BadRequest("Đoàn khám đã bị khóa, không thể thay đổi trạng thái.");

            group.Status = dto.Status;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật trạng thái thành công", status = group.Status });
        }

        // GET: api/MedicalGroups/{id}/staffs
        [HttpGet("{id}/staffs")]
        public async Task<ActionResult<IEnumerable<GroupStaffItemDto>>> GetGroupStaffs(int id)
        {
            return await _context.GroupStaffDetails
                .Include(gsd => gsd.Staff)
                .Where(gsd => gsd.GroupId == id)
                .Select(gsd => new GroupStaffItemDto
                {
                    Id = gsd.Id,
                    StaffId = gsd.StaffId,
                    FullName = gsd.Staff.FullName,
                    JobTitle = gsd.Staff.JobTitle,
                    ShiftType = gsd.ShiftType,
                    CalculatedSalary = gsd.CalculatedSalary,
                    WorkPosition = gsd.WorkPosition,
                    WorkStatus = gsd.WorkStatus
                })
                .ToListAsync();
        }

        // POST: api/MedicalGroups/{id}/staffs
        [HttpPost("{id}/staffs")]
        [Authorize(Roles = "Admin,MedicalGroupManager")]
        public async Task<IActionResult> AddStaffToGroup(int id, [FromBody] AddStaffToGroupDto dto)
        {
            var group = await _context.MedicalGroups.FindAsync(id);
            if (group == null) return NotFound("Đoàn khám không tồn tại.");
            if (group.Status == "Locked" || group.Status == "Finished") 
                return BadRequest("Đoàn khám đã đóng hoặc khóa.");

            var staff = await _context.Staffs.FindAsync(dto.StaffId);
            if (staff == null) return NotFound("Nhân viên không tồn tại.");

            // RULE: Bác sĩ không được phân vào vị trí đơn giản
            var forbiddenPositionsForDoctor = new[] { "Tiếp nhận", "Cân đo huyết áp", "Lấy máu", "Hậu cần", "Khác" };
            if (staff.StaffType == "BacSi" && forbiddenPositionsForDoctor.Contains(dto.WorkPosition))
                return BadRequest($"Bác sĩ không được phân công vào vị trí '{dto.WorkPosition}'. Chỉ được phân vị trí khám bệnh (Khám nội, Khám ngoại, Siêu âm, Sản phụ khoa).");

            // KIỂM TRA TRÙNG LỊCH: Nhân viên không được tham gia đoàn khác vào cùng ngày ExamDate
            var examDate = group.ExamDate.Date;
            var isOverlapping = await _context.GroupStaffDetails
                .Include(gsd => gsd.MedicalGroup)
                .AnyAsync(gsd => gsd.StaffId == dto.StaffId && gsd.MedicalGroup.ExamDate.Date == examDate);

            if (isOverlapping)
                return BadRequest($"Nhân viên {staff.FullName} đã được phân công vào một đoàn khám khác trong ngày {examDate:dd/MM/yyyy}.");

            var detail = new GroupStaffDetail
            {
                GroupId = id,
                StaffId = dto.StaffId,
                ShiftType = dto.ShiftType,
                CalculatedSalary = staff.BaseSalary * (decimal)dto.ShiftType,
                ExamDate = examDate,
                WorkPosition = dto.WorkPosition,
                WorkStatus = dto.WorkStatus ?? "Đang chờ"
            };

            _context.GroupStaffDetails.Add(detail);
            await _context.SaveChangesAsync();

            // TẠO THÔNG BÁO NỘI BỘ
            try
            {
                var userAccount = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == staff.EmployeeCode.ToLower());
                if (userAccount != null)
                {
                    var notification = new Notification
                    {
                        UserId = userAccount.UserId,
                        Message = $"Bạn đã được điều động tham gia đoàn khám '{group.GroupName}' vào ngày {group.ExamDate:dd/MM/yyyy}. Vị trí: {dto.WorkPosition}.",
                        Link = $"/groups?id={group.GroupId}",
                        CreatedAt = DateTime.Now,
                        IsRead = false
                    };
                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                // Swallow error to not block the main transaction if notification fails
            }

            return Ok(detail);
        }

        // POST: api/MedicalGroups/auto-create/{contractId}
        [HttpPost("auto-create/{contractId}")]
        [Authorize(Roles = "Admin,MedicalGroupManager")]
        public async Task<ActionResult<MedicalGroup>> AutoCreateFromContract(int contractId)
        {
            var contract = await _context.Contracts.Include(c => c.Company).FirstOrDefaultAsync(c => c.HealthContractId == contractId);
            if (contract == null) return NotFound("Hợp đồng không khả dụng.");
            if (contract.Status != "Active" && contract.Status != "Approved") return BadRequest("Hợp đồng chưa được phê duyệt.");

            var existingGroupsCount = await _context.MedicalGroups.CountAsync(g => g.HealthContractId == contractId);
            
            var newGroup = new MedicalGroup
            {
                GroupName = $"Đoàn khám {contract.Company.ShortName ?? contract.Company.CompanyName} - Lần {existingGroupsCount + 1}",
                ExamDate = DateTime.Now.AddDays(7), // Mặc định 7 ngày sau
                HealthContractId = contractId,
                Status = "Open"
            };

            _context.MedicalGroups.Add(newGroup);
            await _context.SaveChangesAsync();
            return Ok(newGroup);
        }

        // DELETE: api/MedicalGroups/staffs/{detailId}
        [HttpDelete("staffs/{detailId}")]
        [Authorize(Roles = "Admin,MedicalGroupManager")]
        public async Task<IActionResult> RemoveStaffFromGroup(int detailId)
        {
            var detail = await _context.GroupStaffDetails.FindAsync(detailId);
            if (detail == null) return NotFound();

            var group = await _context.MedicalGroups.FindAsync(detail.GroupId);
            if (group != null && (group.Status == "Locked" || group.Status == "Finished"))
                return BadRequest("Đoàn khám đã đóng hoặc khóa.");

            _context.GroupStaffDetails.Remove(detail);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // PATCH: api/MedicalGroups/staffs/{detailId}/status
        // Cap nhat trang thai diem danh (Dang cho / Da tham gia / Vang mat / Xin nghi)
        [HttpPatch("staffs/{detailId}/status")]
        [Authorize(Roles = "Admin,MedicalGroupManager")]
        public async Task<IActionResult> UpdateWorkStatus(int detailId, [FromBody] UpdateWorkStatusDto dto)
        {
            var detail = await _context.GroupStaffDetails.FindAsync(detailId);
            if (detail == null) return NotFound("Không tìm thấy bản ghi phân công.");

            var validStatuses = new[] { "Đang chờ", "Đã tham gia", "Vắng mặt", "Xin nghỉ" };
            if (!validStatuses.Contains(dto.WorkStatus))
                return BadRequest($"Trạng thái '{dto.WorkStatus}' không hợp lệ.");

            detail.WorkStatus = dto.WorkStatus;
            if (!string.IsNullOrEmpty(dto.Note)) detail.Note = dto.Note;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã cập nhật trạng thái.", workStatus = detail.WorkStatus });
        }

        // POST: api/MedicalGroups/staffs/{detailId}/checkin
        // Ghi nhan gio check-in thuc te
        [HttpPost("staffs/{detailId}/checkin")]
        [Authorize(Roles = "Admin,MedicalGroupManager")]
        public async Task<IActionResult> CheckIn(int detailId)
        {
            var detail = await _context.GroupStaffDetails.FindAsync(detailId);
            if (detail == null) return NotFound("Không tìm thấy bản ghi phân công.");

            if (detail.CheckInTime.HasValue)
                return BadRequest($"Nhân viên này đã check-in lúc {detail.CheckInTime:HH:mm dd/MM/yyyy}.");

            detail.CheckInTime = DateTime.Now;
            detail.WorkStatus = "Đã tham gia"; // Tu dong chuyen sang Da tham gia khi check-in
            await _context.SaveChangesAsync();

            return Ok(new { message = "Check-in thành công!", checkInTime = detail.CheckInTime });
        }

        // POST: api/MedicalGroups/staffs/{detailId}/checkout
        // Ghi nhan gio check-out khi ket thuc buoi kham
        [HttpPost("staffs/{detailId}/checkout")]
        [Authorize(Roles = "Admin,MedicalGroupManager")]
        public async Task<IActionResult> CheckOut(int detailId)
        {
            var detail = await _context.GroupStaffDetails.FindAsync(detailId);
            if (detail == null) return NotFound("Không tìm thấy bản ghi phân công.");

            if (!detail.CheckInTime.HasValue)
                return BadRequest("Chưa có check-in. Không thể check-out.");

            if (detail.CheckOutTime.HasValue)
                return BadRequest($"Nhân viên này đã check-out lúc {detail.CheckOutTime:HH:mm dd/MM/yyyy}.");

            detail.CheckOutTime = DateTime.Now;
            await _context.SaveChangesAsync();

            var duration = detail.CheckOutTime.Value - detail.CheckInTime.Value;
            return Ok(new { 
                message = "Check-out thành công!", 
                checkOutTime = detail.CheckOutTime,
                totalHours = Math.Round(duration.TotalHours, 1)
            });
        }


        // POST: api/MedicalGroups/upload-data
        [HttpPost("upload-data")]
        [Authorize(Roles = "Admin,MedicalGroupManager")]
        public async Task<IActionResult> UploadData([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "groups");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { path = $"uploads/groups/{fileName}" });
        }
        // GET: api/MedicalGroups/export
        [HttpGet("export")]
        [Authorize(Roles = "Admin,MedicalGroupManager")]
        public async Task<IActionResult> ExportGroupsExcel()
        {
            var groups = await _context.MedicalGroups
                .Include(g => g.HealthContract)
                .ThenInclude(c => c.Company)
                .ToListAsync();

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("DanhSachDoanKham");
                worksheet.Cell(1, 1).Value = "Tên Đoàn Khám";
                worksheet.Cell(1, 2).Value = "Khách Hàng";
                worksheet.Cell(1, 3).Value = "Ngày Khám";
                worksheet.Cell(1, 4).Value = "Trạng Thái";
                
                var headerRange = worksheet.Range(1, 1, 1, 4);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.AliceBlue;

                for (int i = 0; i < groups.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = groups[i].GroupName;
                    worksheet.Cell(i + 2, 2).Value = groups[i].HealthContract.Company.ShortName ?? groups[i].HealthContract.Company.CompanyName;
                    worksheet.Cell(i + 2, 3).Value = groups[i].ExamDate.ToString("dd/MM/yyyy");
                    worksheet.Cell(i + 2, 4).Value = groups[i].Status;
                }
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DanhSachDoanKham.xlsx");
                }
            }
        }

        // GET: api/MedicalGroups/{id}/export-staff
        [HttpGet("{id}/export-staff")]
        [Authorize(Roles = "Admin,MedicalGroupManager")]
        public async Task<IActionResult> ExportGroupStaffExcel(int id)
        {
            var group = await _context.MedicalGroups.FindAsync(id);
            if (group == null) return NotFound();

            var staff = await _context.GroupStaffDetails
                .Include(gsd => gsd.Staff)
                .Where(gsd => gsd.GroupId == id)
                .ToListAsync();

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("DoiNguDiKham");
                worksheet.Cell(1, 1).Value = $"DANH SÁCH NHÂN SỰ: {group.GroupName}";
                worksheet.Range(1, 1, 1, 5).Merge().Style.Font.Bold = true;
                
                worksheet.Cell(3, 1).Value = "Mã NV";
                worksheet.Cell(3, 2).Value = "Họ và Tên";
                worksheet.Cell(3, 3).Value = "Vị trí tại đoàn";
                worksheet.Cell(3, 4).Value = "Cấp bậc";
                worksheet.Cell(3, 5).Value = "Ca làm";

                worksheet.Range(3, 1, 3, 5).Style.Font.Bold = true;

                for (int i = 0; i < staff.Count; i++)
                {
                    worksheet.Cell(i + 4, 1).Value = staff[i].Staff.EmployeeCode;
                    worksheet.Cell(i + 4, 2).Value = staff[i].Staff.FullName;
                    worksheet.Cell(i + 4, 3).Value = staff[i].WorkPosition;
                    worksheet.Cell(i + 4, 4).Value = staff[i].Staff.JobTitle;
                    worksheet.Cell(i + 4, 5).Value = staff[i].ShiftType == 1 ? "Cả ngày" : "Nửa ngày";
                }
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"NhanSu_{group.GroupName}.xlsx");
                }
            }
        }

        // POST: api/MedicalGroups/{id}/ai-suggest-staff
        [HttpPost("{id}/ai-suggest-staff")]
        [Authorize(Roles = "Admin,MedicalGroupManager")]
        public async Task<IActionResult> AiSuggestStaff(int id)
        {
            var group = await _context.MedicalGroups
                .Include(g => g.HealthContract)
                .ThenInclude(c => c.Company)
                .FirstOrDefaultAsync(g => g.GroupId == id);

            if (group == null) return NotFound("Đoàn khám không tồn tại.");

            // 1. Thu thập dữ liệu ngữ cảnh
            var contract = group.HealthContract;
            var expectedPeople = contract.ExpectedQuantity;
            
            // Lấy danh sách nhân sự rảnh trong ngày hôm đó (hoặc tất cả nhân sự để AI chọn)
            var allStaff = await _context.Staffs.ToListAsync();
            var busyStaffIds = await _context.GroupStaffDetails
                .Include(gsd => gsd.MedicalGroup)
                .Where(gsd => gsd.MedicalGroup.ExamDate.Date == group.ExamDate.Date && gsd.GroupId != id)
                .Select(gsd => gsd.StaffId)
                .ToListAsync();

            var availableStaff = allStaff
                .Select(s => new {
                    s.StaffId,
                    s.FullName,
                    s.StaffType, // BacSi, DieuDuong, KyThuatVien, Khac
                    s.JobTitle,
                    IsBusy = busyStaffIds.Contains(s.StaffId)
                })
                .ToList();

            // 2. Xây dựng Prompt cho Gemini
            var prompt = new StringBuilder();
            prompt.AppendLine("Bạn là một chuyên gia điều phối nhân sự y tế. Hãy phân bổ nhân sự cho đoàn khám sau:");
            prompt.AppendLine($"- Tên đoàn: {group.GroupName}");
            prompt.AppendLine($"- Quy mô: {expectedPeople} người khám");
            prompt.AppendLine($"- Ngày khám: {group.ExamDate:dd/MM/yyyy}");
            prompt.AppendLine("\nDanh sách nhân sự khả dụng (StaffType: BacSi, DieuDuong, KyThuatVien, Khac):");
            foreach (var s in availableStaff) {
                prompt.AppendLine($"- ID: {s.StaffId}, Tên: {s.FullName}, Loại: {s.StaffType}, Chức vụ: {s.JobTitle} {(s.IsBusy ? "[ĐÃ CÓ LỊCH]" : "")}");
            }

            prompt.AppendLine("\nYêu cầu phân bổ:");
            prompt.AppendLine("1. MUST: Tổng số nhân sự khoảng 1:15 hoặc 1:20 so với quy mô người khám.");
            prompt.AppendLine("2. MUST: Phải có ít nhất 2 BacSi (vị trí: Khám nội, Khám ngoại, Siêu âm, Sản phụ khoa). Bác sĩ CẤM làm Tiếp nhận/Hậu cần.");
            prompt.AppendLine("3. Vị trí Tiếp nhận & Phân loại: Thường là DieuDuong hoặc Khac.");
            prompt.AppendLine("4. Vị trí Cân đo & Huyết áp: DieuDuong.");
            prompt.AppendLine("5. Vị trí Lấy máu: DieuDuong hoặc KyThuatVien.");
            prompt.AppendLine("6. Vị trí Hậu cần: DieuDuong hoặc Khac.");
            prompt.AppendLine("\nKết quả trả về DUY NHẤT một mảng JSON format: [{\"staffId\": int, \"workPosition\": string, \"shiftType\": float, \"reason\": string}]");
            prompt.AppendLine("Vị trí (workPosition) phải là một trong: Tiếp nhận, Cân đo huyết áp, Khám nội, Khám ngoại, Lấy máu, Siêu âm, Khám sản phụ khoa, Hậu cần, Khác.");

            try {
                var aiResponse = await _geminiService.GetStaffSuggestionAsync(prompt.ToString());
                // Làm sạch response (Gemini đôi khi bọc trong ```json ... ```)
                var jsonStart = aiResponse.IndexOf('[');
                var jsonEnd = aiResponse.LastIndexOf(']');
                if (jsonStart >= 0 && jsonEnd > jsonStart) {
                    aiResponse = aiResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);
                }
                
                return Ok(aiResponse);
            } catch (Exception ex) {
                return BadRequest("Lỗi khi gọi AI: " + ex.Message);
            }
        }

        // TEST: api/MedicalGroups/{id}/ai-suggest-staff (GET)
        [HttpGet("{id}/ai-suggest-staff")]
        [AllowAnonymous] // Cho phép test trực tiếp bằng browser
        public IActionResult TestAiRoute(int id)
        {
            return Ok(new { 
                message = "Kết nối Backend OK! Route AI đã sẵn sàng.", 
                groupId = id,
                timestamp = DateTime.Now 
            });
        }
    }
}
