using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;

using Microsoft.Extensions.Logging;

namespace QuanLyDoanKham.API.Services.MedicalRecords
{
    public class ExamService : IExamService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMedicalRecordStateMachine _stateMachine;
        private readonly ILogger<ExamService> _logger;

        public ExamService(ApplicationDbContext context, IMedicalRecordStateMachine stateMachine, ILogger<ExamService> logger)
        {
            _context = context;
            _stateMachine = stateMachine;
            _logger = logger;
        }

        // ─── Public Interface ────────────────────────────────────────────────

        public async Task<ServiceResult<bool>> SaveExamResultAsync(SaveExamResultDto dto, string actorUserId)
        {
            try
            {
                var record = await LoadRecordWithTasksAsync(dto.MedicalRecordId);
                if (record == null)
                    return ServiceResult<bool>.Failure("Không tìm thấy hồ sơ bệnh nhân.");

                if (record.MedicalGroup?.HealthContract?.Status is "Settled" or "Finalized")
                {
                    return ServiceResult<bool>.Failure("Hợp đồng đã quyết toán, không thể thay đổi kết quả khám bệnh.");
                }

                // Guard state: Chặn nhập kết quả nếu bệnh nhân chưa có mặt hoặc đã hủy
                if (record.Status == RecordStatus.Created || record.Status == RecordStatus.Ready)
                {
                    return ServiceResult<bool>.Failure("Bệnh nhân chưa tiếp đón (Check-in), không thể nhập kết quả khám.");
                }
                if (record.Status == RecordStatus.NoShow || record.Status == RecordStatus.Cancelled)
                {
                    return ServiceResult<bool>.Failure("Bệnh nhân vắng mặt hoặc bị hủy hồ sơ, không thể nhập kết quả khám.");
                }

                if (record.PatientId == null)
                {
                    return ServiceResult<bool>.Failure("Lỗi cấu trúc dữ liệu: Hồ sơ chưa được định danh gốc (Thiếu PatientId).");
                }

                var stationTask = FindStationTask(record, dto.StationCode);
                if (stationTask == null)
                    return ServiceResult<bool>.Failure($"Hồ sơ không có chỉ định khám tại trạm '{dto.StationCode}'.");

                // Lưu kết quả y khoa 
                await UpsertExamResultAsync(record, dto);
                await _context.SaveChangesAsync();

                // Giao phó logic Hoàn thành trạm, Real-time và Auto QC cho State Machine
                if (stationTask.Status != StationTaskStatus.StationDone)
                {
                    var completeRes = await _stateMachine.CompleteStationAsync(dto.MedicalRecordId, dto.StationCode, actorUserId, $"Kết luận: {dto.Diagnosis}");
                    if (!completeRes.IsSuccess)
                    {
                        throw new Exception("Lỗi StateMachine: " + completeRes.Message);
                    }
                }

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi lưu kết quả khám. RecordID: {RecordId}, Station: {Station}", dto.MedicalRecordId, dto.StationCode);
                return ServiceResult<bool>.Failure("Lỗi hệ thống khi lưu kết quả khám.");
            }
        }

        public async Task<ServiceResult<List<ExamResultResponseDto>>> GetResultsByRecordIdAsync(int recordId)
        {
            var record = await _context.MedicalRecords.FindAsync(recordId);
            if (record == null)
                return ServiceResult<List<ExamResultResponseDto>>.Failure("Không tìm thấy hồ sơ.");

            var results = await QueryExamResultsForRecord(record);
            return ServiceResult<List<ExamResultResponseDto>>.Success(results);
        }

        // ─── Private Helpers ─────────────────────────────────────────────────

        private Task<MedicalRecord?> LoadRecordWithTasksAsync(int recordId)
            => _context.MedicalRecords
                .Include(r => r.StationTasks)
                .Include(r => r.MedicalGroup)
                    .ThenInclude(mg => mg!.HealthContract)
                .FirstOrDefaultAsync(r => r.MedicalRecordId == recordId);


        private static RecordStationTask? FindStationTask(MedicalRecord record, string stationCode)
            => record.StationTasks.FirstOrDefault(t => t.StationCode == stationCode);

        private async Task UpsertExamResultAsync(MedicalRecord record, SaveExamResultDto dto)
        {
            var jsonString = dto.ResultData.ValueKind == System.Text.Json.JsonValueKind.Undefined ? "{}" : dto.ResultData.GetRawText();

            var existing = await _context.ExamResults.FirstOrDefaultAsync(er =>
                er.PatientId == record.PatientId &&
                er.GroupId   == record.GroupId   &&
                er.ExamType  == dto.ExamType);

            if (existing != null)
            {
                existing.Result       = jsonString;
                existing.Diagnosis    = dto.Diagnosis;
                existing.DoctorStaffId = dto.DoctorStaffId;
                existing.ExamDate     = DateTime.Now;
            }
            else
            {
                _context.ExamResults.Add(new ExamResult
                {
                    PatientId    = record.PatientId.Value, 
                    GroupId      = record.GroupId,
                    ExamType     = dto.ExamType,
                    Result       = jsonString,
                    Diagnosis    = dto.Diagnosis,
                    DoctorStaffId = dto.DoctorStaffId,
                    ExamDate     = DateTime.Now,
                    Doctor       = null!,
                    MedicalGroup = null!,
                    Patient      = null!
                });
            }
        }



        private async Task<List<ExamResultResponseDto>> QueryExamResultsForRecord(MedicalRecord record)
        {
            var rawResults = await _context.ExamResults
                .Include(er => er.Doctor)
                .Where(er => er.PatientId == record.PatientId && er.GroupId == record.GroupId)
                .OrderBy(er => er.ExamDate)
                .ToListAsync();

            return rawResults.Select(er => new ExamResultResponseDto
                {
                    ExamResultId = er.ExamResultId,
                    ExamType     = er.ExamType,
                    ResultData   = string.IsNullOrEmpty(er.Result) ? null : System.Text.Json.JsonSerializer.Deserialize<object>(er.Result, new System.Text.Json.JsonSerializerOptions()),
                    Diagnosis    = er.Diagnosis,
                    ExamDate     = er.ExamDate,
                    DoctorName   = er.Doctor != null ? er.Doctor.FullName : "N/A"
                }).ToList();
        }
    }
}
