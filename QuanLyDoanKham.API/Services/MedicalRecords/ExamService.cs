using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Services.MedicalRecords
{
    public class ExamService : IExamService
    {
        private readonly ApplicationDbContext _context;

        public ExamService(ApplicationDbContext context)
        {
            _context = context;
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


                var stationTask = FindStationTask(record, dto.StationCode);
                if (stationTask == null)
                    return ServiceResult<bool>.Failure($"Hồ sơ không có chỉ định khám tại trạm '{dto.StationCode}'.");

                await UpsertExamResultAsync(record, dto);
                CompleteStationTask(stationTask, actorUserId);
                TryAdvanceRecordToQcPending(record);

                await _context.SaveChangesAsync();
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] SaveExamResultAsync: {ex.Message}");
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
            var existing = await _context.ExamResults.FirstOrDefaultAsync(er =>
                er.PatientId == record.PatientId &&
                er.GroupId   == record.GroupId   &&
                er.ExamType  == dto.ExamType);

            if (existing != null)
            {
                existing.Result       = dto.Result;
                existing.Diagnosis    = dto.Diagnosis;
                existing.DoctorStaffId = dto.DoctorStaffId;
                existing.ExamDate     = DateTime.Now;
            }
            else
            {
                _context.ExamResults.Add(new ExamResult
                {
                    PatientId    = record.PatientId ?? 0,
                    GroupId      = record.GroupId,
                    ExamType     = dto.ExamType,
                    Result       = dto.Result,
                    Diagnosis    = dto.Diagnosis,
                    DoctorStaffId = dto.DoctorStaffId,
                    ExamDate     = DateTime.Now,
                    Doctor       = null!,
                    MedicalGroup = null!,
                    Patient      = null!
                });
            }
        }

        private static void CompleteStationTask(RecordStationTask task, string actorUserId)
        {
            task.Status      = StationTaskStatus.StationDone;
            task.CompletedAt = DateTime.Now;
            task.Notes       = $"[{actorUserId}] Hoàn thành";
        }

        private static void TryAdvanceRecordToQcPending(MedicalRecord record)
        {
            bool allTasksDone = record.StationTasks.All(t => t.Status == StationTaskStatus.StationDone);
            bool isAlreadyQc  = record.Status is RecordStatus.QcPending or RecordStatus.QcPassed;

            if (allTasksDone && !isAlreadyQc)
            {
                record.Status    = RecordStatus.QcPending;
                record.UpdatedAt = DateTime.Now;
                Console.WriteLine($"[INFO] Record {record.MedicalRecordId} → QC_PENDING.");
            }
        }

        private Task<List<ExamResultResponseDto>> QueryExamResultsForRecord(MedicalRecord record)
            => _context.ExamResults
                .Include(er => er.Doctor)
                .Where(er => er.PatientId == record.PatientId && er.GroupId == record.GroupId)
                .OrderBy(er => er.ExamDate)
                .Select(er => new ExamResultResponseDto
                {
                    ExamResultId = er.ExamResultId,
                    ExamType     = er.ExamType,
                    Result       = er.Result,
                    Diagnosis    = er.Diagnosis,
                    ExamDate     = er.ExamDate,
                    DoctorName   = er.Doctor != null ? er.Doctor.FullName : "N/A"
                })
                .ToListAsync();
    }
}
