using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Services.MedicalRecords;

namespace QuanLyDoanKham.Api.Tests.Services;

public class ExamServiceTests : IDisposable
{
    private readonly ApplicationDbContext _db;
    private readonly Mock<IMedicalRecordStateMachine> _stateMachineMock;
    private readonly ExamService _service;

    public ExamServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new ApplicationDbContext(options);
        _stateMachineMock = new Mock<IMedicalRecordStateMachine>(MockBehavior.Strict);
        _service = new ExamService(_db, _stateMachineMock.Object, NullLogger<ExamService>.Instance);
    }

    [Fact]
    public async Task SaveExamResult_WhenRecordNotFound_ReturnsFailure()
    {
        var dto = new SaveExamResultDto
        {
            MedicalRecordId = 999,
            StationCode = "NOI_KHOA",
            ExamType = "Khám Tổng quát",
            Result = "Bình thường",
            Diagnosis = "Không có bệnh lý"
        };

        var result = await _service.SaveExamResultAsync(dto, "user1");

        Assert.False(result.IsSuccess);
        Assert.Equal("Không tìm thấy hồ sơ bệnh nhân.", result.Message);
    }

    [Fact]
    public async Task SaveExamResult_WhenStationNotAssigned_ReturnsFailure()
    {
        var groupId = await SeedGroupAsync(1);
        var patientId = await SeedPatientAsync(1, groupId, "123456789");
        var record = CreateRecordWithTask(patientId: patientId, groupId: groupId, stationCode: "XQUANG", taskStatus: StationTaskStatus.Waiting);
        _db.MedicalRecords.Add(record);
        await _db.SaveChangesAsync();

        var dto = new SaveExamResultDto
        {
            MedicalRecordId = record.MedicalRecordId,
            StationCode = "NOI_KHOA",
            ExamType = "Khám Nội",
            Result = "OK",
            Diagnosis = "Bình thường"
        };

        var result = await _service.SaveExamResultAsync(dto, "user1");

        Assert.False(result.IsSuccess);
        Assert.Contains("NOI_KHOA", result.Message);
    }

    [Fact]
    public async Task SaveExamResult_WhenTaskWaiting_UpsertsResultAndCallsCompleteStation()
    {
        var groupId = await SeedGroupAsync(10);
        var patientId = await SeedPatientAsync(2, groupId, "999999999");
        var record = CreateRecordWithTask(patientId: patientId, groupId: groupId, stationCode: "NOI_KHOA", taskStatus: StationTaskStatus.Waiting);
        _db.MedicalRecords.Add(record);
        await _db.SaveChangesAsync();

        _stateMachineMock
            .Setup(s => s.CompleteStationAsync(record.MedicalRecordId, "NOI_KHOA", "bsNguyen", It.IsAny<string?>()))
            .ReturnsAsync(ServiceResult<RecordStationTask>.Success(record.StationTasks.First()));

        var dto = new SaveExamResultDto
        {
            MedicalRecordId = record.MedicalRecordId,
            StationCode = "NOI_KHOA",
            ExamType = "Khám Nội",
            Result = "Huyết áp 120/80",
            Diagnosis = "Ổn định",
            DoctorStaffId = null
        };

        var result = await _service.SaveExamResultAsync(dto, "bsNguyen");

        Assert.True(result.IsSuccess);

        var saved = await _db.ExamResults.SingleOrDefaultAsync(r =>
            r.PatientId == record.PatientId &&
            r.GroupId == record.GroupId &&
            r.ExamType == dto.ExamType);

        Assert.NotNull(saved);
        Assert.Equal(dto.Result, saved!.Result);
        Assert.Equal(dto.Diagnosis, saved.Diagnosis);

        _stateMachineMock.Verify(
            s => s.CompleteStationAsync(record.MedicalRecordId, "NOI_KHOA", "bsNguyen", It.IsAny<string?>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveExamResult_WhenTaskAlreadyDone_DoesNotCallCompleteStation()
    {
        var groupId = await SeedGroupAsync(11);
        var patientId = await SeedPatientAsync(3, groupId, "888888888");
        var record = CreateRecordWithTask(patientId: patientId, groupId: groupId, stationCode: "NOI_KHOA", taskStatus: StationTaskStatus.StationDone);
        _db.MedicalRecords.Add(record);
        await _db.SaveChangesAsync();

        var dto = new SaveExamResultDto
        {
            MedicalRecordId = record.MedicalRecordId,
            StationCode = "NOI_KHOA",
            ExamType = "Khám Nội",
            Result = "Đã có kết quả",
            Diagnosis = "Theo dõi định kỳ"
        };

        var result = await _service.SaveExamResultAsync(dto, "bsNguyen");

        Assert.True(result.IsSuccess);
        _stateMachineMock.Verify(
            s => s.CompleteStationAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()),
            Times.Never);
    }

    private static MedicalRecord CreateRecordWithTask(int patientId, int groupId, string stationCode, string taskStatus)
        => new()
        {
            FullName = "Nguyen Van A",
            GroupId = groupId,
            PatientId = patientId,
            QrToken = $"token-{Guid.NewGuid():N}",
            Status = RecordStatus.CheckedIn,
            StationTasks = new List<RecordStationTask>
            {
                new() { StationCode = stationCode, Status = taskStatus, QueueNo = 1 }
            }
        };

    private async Task<int> SeedGroupAsync(int id)
    {
        var company = new Company
        {
            CompanyId = id,
            CompanyName = $"Company {id}",
            ShortName = $"C{id}"
        };

        var contract = new HealthContract
        {
            HealthContractId = id,
            CompanyId = company.CompanyId,
            ContractName = $"Contract {id}",
            SigningDate = DateTime.Today.AddDays(-1),
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(1),
            TotalAmount = 1000,
            Status = "Active"
        };

        var group = new MedicalGroup
        {
            GroupId = id,
            GroupName = $"Group {id}",
            ExamDate = DateTime.Today,
            HealthContractId = contract.HealthContractId,
            Status = "Open"
        };

        _db.Companies.Add(company);
        _db.Contracts.Add(contract);
        _db.MedicalGroups.Add(group);
        await _db.SaveChangesAsync();

        return group.GroupId;
    }

    private async Task<int> SeedPatientAsync(int id, int healthContractId, string idCardNumber)
    {
        _db.Patients.Add(new Patient
        {
            PatientId = id,
            HealthContractId = healthContractId,
            FullName = $"Patient {id}",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Nam",
            IDCardNumber = idCardNumber,
            PhoneNumber = "0900000000"
        });
        await _db.SaveChangesAsync();
        return id;
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
