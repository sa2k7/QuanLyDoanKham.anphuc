using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Services.MedicalRecords;
using QuanLyDoanKham.API.Services;
using System.Data.Common;

namespace QuanLyDoanKham.Api.Tests.Services;

public class MedicalRecordServiceTests : IDisposable
{
    private readonly DbConnection _connection;
    private readonly ApplicationDbContext _db;
    private readonly MedicalRecordService _service;

    public MedicalRecordServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new ApplicationDbContext(options);
        _db.Database.EnsureCreated();

        var mockConfig = new Mock<IConfiguration>();
        var tokenSection = new Mock<IConfigurationSection>();
        tokenSection.Setup(s => s.Value).Returns("super-secret-key-that-is-at-least-64-characters-long-for-hmac-sha512-test-only");
        mockConfig.Setup(c => c.GetSection("AppSettings:Token")).Returns(tokenSection.Object);
        var issuerSection = new Mock<IConfigurationSection>();
        issuerSection.Setup(s => s.Value).Returns("QLDK-Test-Issuer");
        mockConfig.Setup(c => c.GetSection("AppSettings:Issuer")).Returns(issuerSection.Object);
        var audienceSection = new Mock<IConfigurationSection>();
        audienceSection.Setup(s => s.Value).Returns("QLDK-Test-Audience");
        mockConfig.Setup(c => c.GetSection("AppSettings:Audience")).Returns(audienceSection.Object);

        var mockGemini = new Mock<IGeminiService>();

        _service = new MedicalRecordService(_db, mockConfig.Object, mockGemini.Object);
    }

    [Fact]
    public async Task BatchIngestAsync_CreatesRecordsAndLinksPatients()
    {
        var group = await SeedGroupAsync(1, "Group 1");

        _db.Patients.Add(new Patient
        {
            PatientId = 10,
            HealthContractId = group.HealthContractId,
            FullName = "Existing Patient",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Nam",
            IDCardNumber = "123456789",
            PhoneNumber = "0900000000"
        });
        await _db.SaveChangesAsync();

        var request = new MedicalRecordBatchIngestRequestDto
        {
            GroupId = group.GroupId,
            Records = new List<MedicalRecordIngestDto>
            {
                new() { FullName = "New Patient", IDCardNumber = "999999999", Department = "IT" },
                new() { FullName = "Existing Patient Ref", IDCardNumber = "123456789", Department = "HR" }
            }
        };

        var result = await _service.BatchIngestAsync(request, "tester");

        Assert.True(result.IsSuccess, result.Message);
        Assert.Equal(2, result.Data!.Count);

        var records = await _db.MedicalRecords.OrderBy(r => r.MedicalRecordId).ToListAsync();
        Assert.Equal(2, records.Count);
        Assert.All(records, r => Assert.NotEqual("PENDING_TOKEN", r.QrToken));

        var linkedRecord = records.FirstOrDefault(r => r.IDCardNumber == "123456789");
        Assert.NotNull(linkedRecord);
        Assert.Equal(10, linkedRecord!.PatientId);
    }

    [Fact]
    public async Task GetByGroupAsync_ReturnsMappedData()
    {
        var g1 = await SeedGroupAsync(1, "Group A");
        var g2 = await SeedGroupAsync(2, "Group B");

        _db.MedicalRecords.AddRange(
            NewRecord(1, g1.GroupId, "B", RecordStatus.Ready),
            NewRecord(2, g1.GroupId, "A", RecordStatus.Ready),
            NewRecord(3, g2.GroupId, "C", RecordStatus.Ready)
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetByGroupAsync(g1.GroupId);

        Assert.Equal(2, result.Count);
        Assert.Equal("A", result[0].FullName);
    }

    [Fact]
    public async Task GetQcPendingRecordsAsync_ReturnsOnlyQcPending()
    {
        var group = await SeedGroupAsync(1, "QC Group");

        _db.MedicalRecords.AddRange(
            NewRecord(1, group.GroupId, "P1", RecordStatus.QcPending),
            NewRecord(2, group.GroupId, "P2", RecordStatus.CheckedIn)
        );
        await _db.SaveChangesAsync();

        var result = await _service.GetQcPendingRecordsAsync();

        Assert.Single(result);
        Assert.Equal("P1", result[0].FullName);
    }

    private async Task<MedicalGroup> SeedGroupAsync(int id, string groupName)
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
            GroupName = groupName,
            ExamDate = DateTime.Today,
            HealthContractId = contract.HealthContractId,
            Status = "Open"
        };

        _db.Companies.Add(company);
        _db.Contracts.Add(contract);
        _db.MedicalGroups.Add(group);
        await _db.SaveChangesAsync();

        return group;
    }

    private static MedicalRecord NewRecord(int id, int groupId, string fullName, string status)
        => new()
        {
            MedicalRecordId = id,
            GroupId = groupId,
            FullName = fullName,
            Status = status,
            QrToken = $"qr-{id}-{Guid.NewGuid():N}",
            CreatedAt = DateTime.Now
        };

    public void Dispose()
    {
        _db.Dispose();
        _connection.Close();
        _connection.Dispose();
    }
}
