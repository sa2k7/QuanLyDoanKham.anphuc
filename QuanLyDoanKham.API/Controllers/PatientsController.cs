using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Authorization;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Models;
using ClosedXML.Excel;

namespace QuanLyDoanKham.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PatientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly Services.MedicalRecords.IMedicalRecordStateMachine _stateMachine;

        public PatientsController(ApplicationDbContext context, Services.MedicalRecords.IMedicalRecordStateMachine stateMachine)
        {
            _context = context;
            _stateMachine = stateMachine;
        }

        // ================================================================
        // POST: api/Patients/self-checkin — Bệnh nhân tự check-in qua mã bản thân
        // ================================================================
        [HttpPost("self-checkin")]
        [AllowAnonymous]
        public async Task<IActionResult> SelfCheckIn([FromBody] PatientSelfCheckInDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RecordId))
                return BadRequest(new { message = "Vui lòng nhập mã hồ sơ (CCCD hoặc Số điện thoại)." });

            var today = DateTime.Today;
            var input = dto.RecordId.Trim();

            // Tìm hồ sơ trong các đoàn đang mở hôm nay
            var record = await _context.MedicalRecords
                .Include(m => m.MedicalGroup)
                .Where(m => m.MedicalGroup != null && m.MedicalGroup.ExamDate.Date == today && m.MedicalGroup.Status == "Open")
                .Where(m => m.IDCardNumber == input || m.MedicalRecordId.ToString() == input) // Hỗ trợ CCCD hoặc ID nội bộ
                .FirstOrDefaultAsync();

            if (record == null)
            {
                // Thử tìm theo Patient (nếu hồ sơ chưa link PatientId nhưng CCCD khớp)
                // Hoặc tìm Patient trước rồi tìm Record của Patient đó
                return NotFound(new { message = "Không tìm thấy hồ sơ của bạn trong danh sách khám hôm nay. Vui lòng liên hệ quầy tiếp đón." });
            }

            if (record.CheckInAt != null)
            {
                return BadRequest(new { message = "Bạn đã báo danh thành công trước đó." });
            }

            var checkInResult = await _stateMachine.CheckInAsync(record.MedicalRecordId, "SYSTEM");

            if (!checkInResult.IsSuccess)
            {
                return BadRequest(new { message = "Lỗi hệ thống khi phân luồng hàng chờ: " + checkInResult.Message });
            }

            // Reload to get QueueNo and next station
            var updatedRecord = await _context.MedicalRecords
                .Include(m => m.StationTasks)
                .FirstOrDefaultAsync(m => m.MedicalRecordId == record.MedicalRecordId);

            var firstWaitingStationCode = updatedRecord?.StationTasks
                .Where(t => t.Status == "WAITING")
                .OrderBy(t => t.TaskId) // Assuming they were created in order
                .Select(t => t.StationCode)
                .FirstOrDefault();

            var stationName = "Khoa/Phòng khám";
            if (!string.IsNullOrEmpty(firstWaitingStationCode))
            {
                var station = await _context.Stations.FirstOrDefaultAsync(s => s.StationCode == firstWaitingStationCode);
                if (station != null) stationName = station.StationName;
            }

            return Ok(new
            {
                message = "Báo danh thành công!",
                fullName = updatedRecord?.FullName ?? record.FullName,
                queueNo = updatedRecord?.QueueNo,
                nextStation = stationName
            });
        }

        public class PatientSelfCheckInDto { public string RecordId { get; set; } = null!; }


        // ================================================================
        // GET: api/Patients — Danh sách tất cả bệnh nhân
        // ================================================================
        [HttpGet]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? contractId,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var query = _context.Patients
                .Include(p => p.HealthContract)
                    .ThenInclude(c => c!.Company)
                .AsQueryable();

            if (contractId.HasValue)
                query = query.Where(p => p.HealthContractId == contractId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(p =>
                    p.FullName.ToLower().Contains(s) ||
                    (p.IDCardNumber != null && p.IDCardNumber.Contains(s)) ||
                    (p.PhoneNumber != null && p.PhoneNumber.Contains(s)));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(p => p.CreatedDate)
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
                    p.HealthContractId,
                    ContractName = p.HealthContract != null ? p.HealthContract.ContractName : null,
                    CompanyName = p.HealthContract != null && p.HealthContract.Company != null
                        ? p.HealthContract.Company.CompanyName : null,
                    p.CreatedDate
                })
                .ToListAsync();

            return Ok(new { total, page, pageSize, items });
        }

        // ================================================================
        // GET: api/Patients/{id} — Chi tiết 1 bệnh nhân + Hồ sơ khám liên quan
        // ================================================================
        [HttpGet("{id}")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> GetById(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.HealthContract)
                    .ThenInclude(c => c!.Company)
                .FirstOrDefaultAsync(p => p.PatientId == id);

            if (patient == null) return NotFound(new { message = "Bệnh nhân không tồn tại." });

            // Lấy lịch sử hồ sơ khám của bệnh nhân (qua MedicalRecord liên kết PatientId)
            var medicalHistory = await _context.MedicalRecords
                .Include(m => m.MedicalGroup)
                .Include(m => m.StationTasks)
                .Where(m => m.PatientId == id)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new
                {
                    m.MedicalRecordId,
                    m.Status,
                    m.CheckInAt,
                    m.QueueNo,
                    m.CurrentStation,
                    GroupName = m.MedicalGroup != null ? m.MedicalGroup.GroupName : null,
                    ExamDate = m.MedicalGroup != null ? m.MedicalGroup.ExamDate : (DateTime?)null,
                    TasksDone = m.StationTasks.Count(t => t.Status == "STATION_DONE"),
                    TasksTotal = m.StationTasks.Count
                })
                .ToListAsync();

            return Ok(new
            {
                patient.PatientId,
                patient.FullName,
                patient.DateOfBirth,
                patient.Gender,
                patient.IDCardNumber,
                patient.PhoneNumber,
                patient.Department,
                patient.HealthContractId,
                ContractName = patient.HealthContract?.ContractName,
                CompanyName = patient.HealthContract?.Company?.CompanyName,
                patient.CreatedDate,
                MedicalHistory = medicalHistory
            });
        }

        // ================================================================
        // GET: api/Patients/by-contract/{contractId} — Theo hợp đồng
        // ================================================================
        [HttpGet("by-contract/{contractId}")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> GetByContract(int contractId)
        {
            var patients = await _context.Patients
                .Where(p => p.HealthContractId == contractId)
                .OrderBy(p => p.FullName)
                .Select(p => new
                {
                    p.PatientId,
                    p.FullName,
                    p.DateOfBirth,
                    p.Gender,
                    p.IDCardNumber,
                    p.PhoneNumber,
                    p.Department
                })
                .ToListAsync();

            return Ok(patients);
        }

        // ================================================================
        // GET: api/Patients/by-group/{groupId} — Theo đoàn khám (qua MedicalRecord)
        // ================================================================
        [HttpGet("by-group/{groupId}")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> GetByGroup(int groupId)
        {
            // Lấy bệnh nhân qua bảng MedicalRecord liên kết vào nhóm
            var records = await _context.MedicalRecords
                .Include(m => m.Patient)
                .Where(m => m.GroupId == groupId && m.PatientId.HasValue)
                .Select(m => new
                {
                    m.MedicalRecordId,
                    m.PatientId,
                    m.FullName,
                    m.Gender,
                    m.IDCardNumber,
                    m.Status,
                    m.CheckInAt,
                    m.QueueNo,
                    Department = m.Department,
                })
                .OrderBy(m => m.FullName)
                .ToListAsync();

            return Ok(records);
        }

        // ================================================================
        // POST: api/Patients — Thêm mới bệnh nhân
        // ================================================================
        [HttpPost]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> Create([FromBody] PatientUpsertDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Kiểm tra hợp đồng tồn tại
            var contractExists = await _context.Contracts.AnyAsync(c => c.HealthContractId == dto.HealthContractId);
            if (!contractExists)
                return BadRequest(new { message = "Hợp đồng không tồn tại." });

            // Không cho trùng CCCD trong cùng 1 hợp đồng
            if (!string.IsNullOrWhiteSpace(dto.IDCardNumber))
            {
                var dup = await _context.Patients.AnyAsync(p =>
                    p.HealthContractId == dto.HealthContractId &&
                    p.IDCardNumber == dto.IDCardNumber.Trim());
                if (dup)
                    return BadRequest(new { message = "Số CCCD/CMND đã được đăng ký trong hợp đồng này." });
            }

            var patient = new Patient
            {
                HealthContractId = dto.HealthContractId,
                FullName = dto.FullName.Trim(),
                DateOfBirth = dto.DateOfBirth ?? DateTime.MinValue,
                Gender = dto.Gender,
                IDCardNumber = dto.IDCardNumber?.Trim(),
                PhoneNumber = dto.PhoneNumber?.Trim(),
                Department = dto.Department?.Trim(),
                CreatedDate = DateTime.Now
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Thêm bệnh nhân thành công.", patientId = patient.PatientId });
        }

        // ================================================================
        // PUT: api/Patients/{id} — Sửa thông tin bệnh nhân
        // ================================================================
        [HttpPut("{id}")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> Update(int id, [FromBody] PatientUpsertDto dto)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound(new { message = "Bệnh nhân không tồn tại." });

            // Kiểm tra trùng CCCD (loại trừ chính nó)
            if (!string.IsNullOrWhiteSpace(dto.IDCardNumber))
            {
                var dup = await _context.Patients.AnyAsync(p =>
                    p.HealthContractId == dto.HealthContractId &&
                    p.IDCardNumber == dto.IDCardNumber.Trim() &&
                    p.PatientId != id);
                if (dup)
                    return BadRequest(new { message = "Số CCCD/CMND đã được đăng ký cho bệnh nhân khác trong hợp đồng." });
            }

            patient.FullName = dto.FullName.Trim();
            patient.DateOfBirth = dto.DateOfBirth ?? patient.DateOfBirth;
            patient.Gender = dto.Gender;
            patient.IDCardNumber = dto.IDCardNumber?.Trim();
            patient.PhoneNumber = dto.PhoneNumber?.Trim();
            patient.Department = dto.Department?.Trim();

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thông tin bệnh nhân thành công." });
        }

        // ================================================================
        // DELETE: api/Patients/{id} — Xóa bệnh nhân
        // ================================================================
        [HttpDelete("{id}")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> Delete(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.HealthContract)
                .FirstOrDefaultAsync(p => p.PatientId == id);

            if (patient == null) return NotFound(new { message = "Bệnh nhân không tồn tại." });

            // Kiểm tra còn hồ sơ khám đang mở không
            var hasOpenRecord = await _context.MedicalRecords
                .AnyAsync(m => m.PatientId == id &&
                    m.Status != "CLOSED" && m.Status != "REPORTED");

            if (hasOpenRecord)
                return BadRequest(new { message = "Bệnh nhân đang có hồ sơ khám chưa đóng. Không thể xóa." });

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa bệnh nhân thành công." });
        }

        // ================================================================
        // POST: api/Patients/import — Nhập hàng loạt từ Excel
        // ================================================================
        [HttpPost("import")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> Import(IFormFile file, [FromQuery] int contractId)
        {
            if (file == null || file.Length == 0) return BadRequest(new { message = "Vui lòng chọn file Excel." });

            var contract = await _context.Contracts.FindAsync(contractId);
            if (contract == null) return BadRequest(new { message = "Hợp đồng không tồn tại." });

            try
            {
                using var stream = file.OpenReadStream();
                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RowsUsed().Skip(1); // Bỏ qua header

                var addedCount = 0;
                var errorCount = 0;
                var patientsToSave = new List<Patient>();

                foreach (var row in rows)
                {
                    try 
                    {
                        var fullName = row.Cell(1).GetValue<string>().Trim();
                        if (string.IsNullOrEmpty(fullName)) continue;

                        var gender = row.Cell(2).GetValue<string>().Trim();
                        var dobValue = row.Cell(3).Value;
                        DateTime dob = DateTime.MinValue;
                        if (dobValue.IsDateTime) dob = dobValue.GetDateTime();
                        else if (DateTime.TryParse(dobValue.ToString(), out var dt)) dob = dt;

                        var idCard = row.Cell(4).GetValue<string>().Trim();
                        var phone = row.Cell(5).GetValue<string>().Trim();
                        var dept = row.Cell(6).GetValue<string>().Trim();

                        // Kiểm tra trùng CCCD trong List chuẩn bị add hoặc trong DB
                        if (!string.IsNullOrEmpty(idCard))
                        {
                            var existsInDb = await _context.Patients.AnyAsync(p => p.HealthContractId == contractId && p.IDCardNumber == idCard);
                            var existsInBatch = patientsToSave.Any(p => p.IDCardNumber == idCard);
                            if (existsInDb || existsInBatch)
                            {
                                errorCount++;
                                continue;
                            }
                        }

                        patientsToSave.Add(new Patient
                        {
                            HealthContractId = contractId,
                            FullName = fullName,
                            Gender = gender,
                            DateOfBirth = dob,
                            IDCardNumber = idCard,
                            PhoneNumber = phone,
                            Department = dept,
                            CreatedDate = DateTime.Now
                        });
                        addedCount++;
                    }
                    catch { errorCount++; }
                }

                if (patientsToSave.Any())
                {
                    await _context.Patients.AddRangeAsync(patientsToSave);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { 
                    message = $"Import hoàn tất. Thành công: {addedCount}, Thất bại: {errorCount}.",
                    addedCount,
                    errorCount
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi xử lý file Excel: " + ex.Message });
            }
        }

        // ================================================================
        // GET: api/Patients/export — Xuất danh sách ra Excel
        // ================================================================
        [HttpGet("export")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> Export([FromQuery] int? contractId, [FromQuery] string? search)
        {
            var query = _context.Patients
                .Include(p => p.HealthContract)
                .AsQueryable();

            if (contractId.HasValue)
                query = query.Where(p => p.HealthContractId == contractId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(p =>
                    p.FullName.ToLower().Contains(s) ||
                    (p.IDCardNumber != null && p.IDCardNumber.Contains(s)));
            }

            var data = await query.OrderBy(p => p.FullName).ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("DanhSachBenhNhan");

            // Header
            ws.Cell(1, 1).Value = "Họ và Tên";
            ws.Cell(1, 2).Value = "Giới tính";
            ws.Cell(1, 3).Value = "Ngày sinh";
            ws.Cell(1, 4).Value = "CCCD/CMND";
            ws.Cell(1, 5).Value = "Điện thoại";
            ws.Cell(1, 6).Value = "Phòng ban";
            ws.Cell(1, 7).Value = "Hợp đồng";

            var range = ws.Range(1, 1, 1, 7);
            range.Style.Font.Bold = true;
            range.Style.Fill.BackgroundColor = XLColor.LightGray;

            // Data
            for (int i = 0; i < data.Count; i++)
            {
                var p = data[i];
                ws.Cell(i + 2, 1).Value = p.FullName;
                ws.Cell(i + 2, 2).Value = p.Gender;
                ws.Cell(i + 2, 3).Value = p.DateOfBirth == DateTime.MinValue ? "" : p.DateOfBirth.ToString("dd/MM/yyyy");
                ws.Cell(i + 2, 4).Value = p.IDCardNumber;
                ws.Cell(i + 2, 5).Value = p.PhoneNumber;
                ws.Cell(i + 2, 6).Value = p.Department;
                ws.Cell(i + 2, 7).Value = p.HealthContract?.ContractName;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"DanhSachBenhNhan_{DateTime.Now:yyyyMMdd}.xlsx");
        }
    }

    // DTO dùng cho Create và Update
    public class PatientUpsertDto
    {
        public int HealthContractId { get; set; }
        public string FullName { get; set; } = null!;
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? IDCardNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Department { get; set; }
    }
}
