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
        var jsonResult = System.Text.Json.JsonDocument.Parse("{\"value\": \"Bình thường\"}").RootElement;
        var dto = new SaveExamResultDto
        {
            MedicalRecordId = 999,
            StationCode = "NOI_KHOA",
            ExamType = "Khám Tổng quát",
            ResultData = jsonResult,
            Diagnosis = "Không có bệnh lý"
        };

        var result = await _service.SaveExamResultAsync(dto, "user1");

        Assert.False(result.IsSuccess);
        Assert.Equal("Không tìm thấy hồ sơ bệnh nhân.", result.Message);
    }

    [Fact]
    public async Task SaveExamResult_WhenRecordFound_UpsertsResult()
    {
        var groupId = await SeedGroupAsync(10);
        var patientId = await SeedPatientAsync(2, groupId, "999999999");
        var record = new MedicalRecord 
        { 
            MedicalRecordId = 1,
            PatientId = patientId, 
            GroupId = groupId, 
            FullName = "Test Patient",
            Status = RecordStatus.InProgress,
            QrToken = "test-token"
        };
        _db.MedicalRecords.Add(record);
        await _db.SaveChangesAsync();

        var jsonStr = "{\"value\": \"Huyết áp 120/80\"}";
        var jsonResult = System.Text.Json.JsonDocument.Parse(jsonStr).RootElement;
        
        var dto = new SaveExamResultDto
        {
            MedicalRecordId = record.MedicalRecordId,
            StationCode = "NOI_KHOA",
            ExamType = "Khám Nội",
            ResultData = jsonResult,
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
        Assert.Equal(jsonStr, saved!.Result);
        Assert.Equal(dto.Diagnosis, saved.Diagnosis);
    }

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
