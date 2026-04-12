using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Services.MedicalRecords;
using Xunit;

namespace QuanLyDoanKham.Api.Tests.Services
{
    /// <summary>
    /// Tests cho luồng chính: Lưu kết quả khám → Cập nhật Task → Chuyển trạng thái hồ sơ
    /// </summary>
    public class ExamServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _db;
        private readonly ExamService _service;

        public ExamServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);
            _service = new ExamService(_db);
        }

        // ─── [TEST 1] Lưu kết quả hồ sơ không tồn tại ──────────────────────

        [Fact]
        public async Task SaveExamResult_WhenRecordNotFound_ReturnsFailure()
        {
            var dto = new SaveExamResultDto
            {
                MedicalRecordId = 999, // ID không tồn tại
                StationCode = "NOI_KHOA",
                ExamType = "Khám Tổng quát",
                Result = "Bình thường",
                Diagnosis = "Không có bệnh lý"
            };

            var result = await _service.SaveExamResultAsync(dto, "user1");

            Assert.False(result.IsSuccess);
            // Service trả về message cụ thể khi record không tồn tại
            Assert.Equal("Không tìm thấy hồ sơ bệnh nhân.", result.Message);
        }

        // ─── [TEST 2] Lưu kết quả trạm không có trong chỉ định ─────────────

        [Fact]
        public async Task SaveExamResult_WhenStationNotAssigned_ReturnsFailure()
        {
            var record = CreateRecordWithTask(patientId: 1, groupId: 1, stationCode: "XET_NGHIEM");
            _db.MedicalRecords.Add(record);
            await _db.SaveChangesAsync();

            var dto = new SaveExamResultDto
            {
                MedicalRecordId = record.MedicalRecordId,
                StationCode = "NOI_KHOA", // Trạm khác không có trong chỉ định
                ExamType = "Khám Nội",
                Result = "...",
                Diagnosis = "..."
            };

            var result = await _service.SaveExamResultAsync(dto, "user1");

            Assert.False(result.IsSuccess);
            Assert.Contains("NOI_KHOA", result.Message);
        }

        // ─── [TEST 3] Luồng chính: 1 trạm → Hoàn thành → QC_PENDING ────────

        [Fact]
        public async Task SaveExamResult_WhenLastTaskCompleted_AdvancesRecordToQcPending()
        {
            var record = CreateRecordWithTask(patientId: 2, groupId: 1, stationCode: "NOI_KHOA");
            _db.MedicalRecords.Add(record);
            await _db.SaveChangesAsync();

            var dto = new SaveExamResultDto
            {
                MedicalRecordId = record.MedicalRecordId,
                StationCode = "NOI_KHOA",
                ExamType = "Khám Tổng quát",
                Result = "Huyết áp 120/80",
                Diagnosis = "Không có bệnh lý"
            };

            var result = await _service.SaveExamResultAsync(dto, "bsNguyen");

            Assert.True(result.IsSuccess);

            var updatedRecord = await _db.MedicalRecords.FindAsync(record.MedicalRecordId);
            Assert.Equal("QC_PENDING", updatedRecord!.Status);
        }

        // ─── [TEST 4] Nhiều trạm: Chưa xong hết → Không chuyển QC_PENDING ──

        [Fact]
        public async Task SaveExamResult_WhenOtherTasksStillPending_RecordStaysInProgress()
        {
            var record = new MedicalRecord
            {
                FullName = "Trần Thị Bình",
                GroupId = 1,
                PatientId = 3,
                QrToken = "token-test",
                Status = "PROCESSING",
                StationTasks = new List<RecordStationTask>
                {
                    new() { StationCode = "NOI_KHOA", Status = "IN_PROGRESS", QueueNo = 1 },
                    new() { StationCode = "XET_NGHIEM", Status = "PENDING", QueueNo = 2 },
                }
            };
            _db.MedicalRecords.Add(record);
            await _db.SaveChangesAsync();

            var dto = new SaveExamResultDto
            {
                MedicalRecordId = record.MedicalRecordId,
                StationCode = "NOI_KHOA", // Chỉ hoàn thành 1 trong 2 trạm
                ExamType = "Khám Nội",
                Result = "OK",
                Diagnosis = "Bình thường"
            };

            await _service.SaveExamResultAsync(dto, "user1");

            var updatedRecord = await _db.MedicalRecords.FindAsync(record.MedicalRecordId);
            Assert.NotEqual("QC_PENDING", updatedRecord!.Status); // Vẫn chưa được chuyển
        }

        // ─── Helpers ─────────────────────────────────────────────────────────

        private static MedicalRecord CreateRecordWithTask(int patientId, int groupId, string stationCode)
            => new()
            {
                FullName  = "Nguyễn Văn A",
                GroupId   = groupId,
                PatientId = patientId,
                QrToken   = "token-test",
                Status    = "PROCESSING",
                StationTasks = new List<RecordStationTask>
                {
                    new() { StationCode = stationCode, Status = "WAITING", QueueNo = 1 }
                }
            };

        public void Dispose() => _db.Dispose();
    }
}
