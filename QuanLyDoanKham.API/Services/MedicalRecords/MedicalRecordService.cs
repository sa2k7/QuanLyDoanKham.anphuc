using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace QuanLyDoanKham.API.Services.MedicalRecords
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public MedicalRecordService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<ServiceResult<List<MedicalRecord>>> BatchIngestAsync(MedicalRecordBatchIngestRequestDto request, string createdBy)
        {
            var group = await _context.MedicalGroups.FindAsync(request.GroupId);
            if (group == null) return ServiceResult<List<MedicalRecord>>.Failure("Không tìm thấy đoàn khám.");

            var recordsToAdd = new List<MedicalRecord>();
            var now = DateTime.Now;

            foreach (var recordDto in request.Records)
            {
                Patient? existingPatient = null;
                if (!string.IsNullOrEmpty(recordDto.IDCardNumber))
                {
                    existingPatient = await _context.Patients
                        .FirstOrDefaultAsync(p => p.IDCardNumber == recordDto.IDCardNumber);
                }
                
                var newRecord = new MedicalRecord
                {
                    GroupId = request.GroupId,
                    FullName = recordDto.FullName,
                    DateOfBirth = recordDto.DateOfBirth,
                    Gender = recordDto.Gender,
                    IDCardNumber = recordDto.IDCardNumber,
                    Department = recordDto.Department,
                    Status = "READY",
                    CreatedAt = now,
                    // Must be unique because MedicalRecord.QrToken has unique index.
                    QrToken = $"PENDING_{Guid.NewGuid():N}"
                };

                if (existingPatient != null)
                {
                    newRecord.PatientId = existingPatient.PatientId;
                }
                else
                {
                    // Tự động đẻ Patient mới qua navigation property để đảm bảo DB luôn có Patient map với Record
                    newRecord.Patient = new Patient
                    {
                        HealthContractId = group.HealthContractId,
                        FullName = recordDto.FullName,
                        IDCardNumber = recordDto.IDCardNumber ?? $"AUTO_{Guid.NewGuid().ToString("N").Substring(0, 8)}",
                        DateOfBirth = recordDto.DateOfBirth ?? DateTime.MinValue,
                        Gender = recordDto.Gender ?? "Khác",
                        PhoneNumber = "N/A"
                    };
                }

                recordsToAdd.Add(newRecord);
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.MedicalRecords.AddRange(recordsToAdd);
                    await _context.SaveChangesAsync();

                    // Now we have IDs, generate tokens
                    foreach (var record in recordsToAdd)
                    {
                        record.QrToken = GenerateQrToken(record.MedicalRecordId, record.GroupId);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return ServiceResult<List<MedicalRecord>>.Success(recordsToAdd);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return ServiceResult<List<MedicalRecord>>.Failure("Lỗi khi nhập dữ liệu hàng loạt: " + ex.Message);
                }
            }
        }

        public string GenerateQrToken(int medicalRecordId, int groupId)
        {
            var claims = new List<Claim>
            {
                new Claim("recordId", medicalRecordId.ToString()),
                new Claim("groupId", groupId.ToString()),
                new Claim("type", "MEDICAL_RECORD_CHECKIN")
            };

            var configKey = _configuration.GetSection("AppSettings:Token").Value
                ?? throw new InvalidOperationException("CRITICAL: AppSettings:Token is missing!");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // Token expires at the end of the day to prevent long-term misuse
            var expires = DateTime.Today.AddDays(1).AddSeconds(-1);

            var token = new JwtSecurityToken(
                issuer: _configuration.GetSection("AppSettings:Issuer").Value ?? "QuanLyDoanKham",
                audience: _configuration.GetSection("AppSettings:Audience").Value ?? "QuanLyDoanKham",
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ─── Query Implementation ─────────────────────────────────────────────

        public async Task<List<MedicalRecordGroupItemDto>> GetByGroupAsync(int groupId)
        {
            return await _context.MedicalRecords
                .Where(m => m.GroupId == groupId)
                .OrderBy(m => m.FullName)
                .Select(m => new MedicalRecordGroupItemDto {
                    MedicalRecordId = m.MedicalRecordId,
                    FullName = m.FullName,
                    DateOfBirth = m.DateOfBirth,
                    Gender = m.Gender,
                    IDCardNumber = m.IDCardNumber,
                    Department = m.Department,
                    Status = m.Status,
                    CheckInAt = m.CheckInAt,
                    QueueNo = m.QueueNo
                })
                .ToListAsync();
        }

        public async Task<MedicalRecord?> GetByIdAsync(int id)
        {
            return await _context.MedicalRecords
                .Include(m => m.StationTasks)
                .FirstOrDefaultAsync(m => m.MedicalRecordId == id);
        }

        public async Task<List<StationQueueItemDto>> GetQueueByStationAsync(string stationCode)
        {
            return await _context.RecordStationTasks
                .Include(t => t.MedicalRecord)
                .Where(t => t.StationCode == stationCode
                         && t.Status != StationTaskStatus.StationDone
                         && t.Status != StationTaskStatus.Skipped)
                .OrderBy(t => t.MedicalRecord!.QueueNo)
                .Select(t => new StationQueueItemDto {
                    TaskId = t.TaskId,
                    MedicalRecordId = t.MedicalRecordId,
                    FullName = t.MedicalRecord!.FullName,
                    Gender = t.MedicalRecord!.Gender,
                    QueueNo = t.MedicalRecord!.QueueNo,
                    Status = t.Status,
                    WaitingSince = t.WaitingSince,
                    StartedAt = t.StartedAt
                })
                .ToListAsync();
        }

        public async Task<StationQueueSummaryDto> GetStationQueueSummaryAsync(string stationCode)
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

            return new StationQueueSummaryDto { 
                StationCode = stationCode, 
                WaitingCount = waitingCount, 
                InProgressCount = inProgressCount, 
                DoneToday = doneToday 
            };
        }

        public async Task<List<GroupQueueOverviewDto>> GetGroupQueueOverviewAsync(int groupId)
        {
            return await _context.RecordStationTasks
                .Include(t => t.MedicalRecord)
                .Include(t => t.Station)
                .Where(t => t.MedicalRecord!.GroupId == groupId
                         && t.Status != StationTaskStatus.StationDone
                         && t.Status != StationTaskStatus.Skipped)
                .GroupBy(t => new { t.StationCode, t.Station!.StationName, t.Station!.SortOrder })
                .Select(g => new GroupQueueOverviewDto {
                    StationCode  = g.Key.StationCode,
                    StationName  = g.Key.StationName,
                    SortOrder    = g.Key.SortOrder,
                    WaitingCount    = g.Count(t => t.Status == StationTaskStatus.Waiting),
                    InProgressCount = g.Count(t => t.Status == StationTaskStatus.StationInProgress),
                })
                .OrderBy(s => s.SortOrder)
                .ToListAsync();
        }

        public async Task<List<QcPendingRecordDto>> GetQcPendingRecordsAsync()
        {
            return await _context.MedicalRecords
                .Include(r => r.StationTasks)
                .Where(r => r.Status == RecordStatus.QcPending)
                .OrderBy(r => r.CheckInAt)
                .Select(r => new QcPendingRecordDto
                {
                    MedicalRecordId = r.MedicalRecordId,
                    FullName = r.FullName,
                    QueueNo = r.QueueNo,
                    Status = r.Status,
                    CheckInAt = r.CheckInAt,
                    StationTasks = r.StationTasks.Select(t => new QcStationTaskStatusDto
                    {
                        StationCode = t.StationCode,
                        Status = t.Status
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<ServiceResult<List<MedicalRecord>>> BatchIngestFromExcelAsync(int groupId, string filePath, string createdBy)
        {
            try
            {
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath);
                if (!File.Exists(fullPath)) return ServiceResult<List<MedicalRecord>>.Failure("Không tìm thấy file trên server.");

                var records = new List<MedicalRecordIngestDto>();
                using (var workbook = new ClosedXML.Excel.XLWorkbook(fullPath))
                {
                    var worksheet = workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null) return ServiceResult<List<MedicalRecord>>.Failure("File không có worksheet.");

                    var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Skip header
                    foreach (var row in rows)
                    {
                        var fullName = row.Cell(1).GetValue<string>()?.Trim();
                        if (string.IsNullOrEmpty(fullName)) continue;

                        DateTime? dob = null;
                        try {
                            if (row.Cell(3).DataType == ClosedXML.Excel.XLDataType.DateTime)
                                dob = row.Cell(3).GetDateTime();
                            else if (DateTime.TryParse(row.Cell(3).GetValue<string>(), out var d))
                                dob = d;
                        } catch { /* Ignore date error, keep null */ }

                        records.Add(new MedicalRecordIngestDto
                        {
                            FullName = fullName,
                            Gender = row.Cell(2).GetValue<string>()?.Trim(),
                            DateOfBirth = dob,
                            IDCardNumber = row.Cell(4).GetValue<string>()?.Trim(),
                            Department = row.Cell(5).GetValue<string>()?.Trim()
                        });
                    }
                }

                if (!records.Any()) return ServiceResult<List<MedicalRecord>>.Failure("Không tìm thấy dữ liệu hợp lệ trong file.");

                return await BatchIngestAsync(new MedicalRecordBatchIngestRequestDto
                {
                    GroupId = groupId,
                    Records = records
                }, createdBy);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<MedicalRecord>>.Failure("Lỗi xử lý file Excel: " + ex.Message);
            }
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            var record = await _context.MedicalRecords.FindAsync(id);
            if (record == null) return ServiceResult<bool>.Failure("Hồ sơ không tồn tại.");

            // Chặn xóa nếu đã khám xong hoặc đã báo cáo
            if (record.Status == RecordStatus.Completed || record.Status == RecordStatus.QcPassed)
                return ServiceResult<bool>.Failure("Không thể xóa hồ sơ đã hoàn tất/đã báo cáo.");

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Xóa các dữ liệu liên quan
                    var tasks = await _context.RecordStationTasks.Where(t => t.MedicalRecordId == id).ToListAsync();
                    _context.RecordStationTasks.RemoveRange(tasks);

                    if (record.PatientId.HasValue)
                    {
                        var results = await _context.ExamResults
                            .Where(r => r.PatientId == record.PatientId.Value && r.GroupId == record.GroupId)
                            .ToListAsync();
                        _context.ExamResults.RemoveRange(results);
                    }

                    _context.MedicalRecords.Remove(record);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return ServiceResult<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return ServiceResult<bool>.Failure("Lỗi khi xóa hồ sơ: " + ex.Message);
                }
            }
        }
    }
}

