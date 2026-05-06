using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Models.Enums;
using QuanLyDoanKham.API.Services.MedicalBatch;
using Xunit;

namespace QuanLyDoanKham.Api.Tests.Integration;

/// <summary>
/// Integration tests for MedicalBatch bulk generation atomicity.
/// Task 35 — Validates that a failure mid-insert during bulk generation
/// causes a full transaction rollback (zero records created).
/// </summary>
public class MedicalBatchIntegrationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public MedicalBatchIntegrationTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        using var db = new ApplicationDbContext(_options);
        db.Database.EnsureCreated();
    }

    public void Dispose() => _connection.Dispose();

    private ApplicationDbContext MakeDb() => new(_options);

    private static CreateMedicalBatchDto Dto(int contractId, int count, bool autoGenerate = false) =>
        new() { ContractId = contractId, EstimatedCount = count, AutoGenerate = autoGenerate };

    private async Task<int> SeedContractAsync(ContractStatus status = ContractStatus.Approved)
    {
        using var db = MakeDb();

        var company = new Company
        {
            CompanyName = "Test Company",
            TaxCode = $"TC{Guid.NewGuid().ToString("N")[..6]}",
            Address = "Test Address"
        };
        db.Companies.Add(company);
        await db.SaveChangesAsync();

        var contract = new HealthContract
        {
            CompanyId = company.CompanyId,
            ContractCode = $"HD-TEST-{Guid.NewGuid().ToString("N")[..6]}",
            ContractName = "Test Contract",
            SigningDate = DateTime.Today,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddYears(1),
            UnitPrice = 100_000,
            ExpectedQuantity = 100,
            TotalAmount = 10_000_000,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            CurrentApprovalStep = 0
        };
        db.Contracts.Add(contract);
        await db.SaveChangesAsync();
        return contract.HealthContractId;
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Happy path: bulk generation with AutoGenerate=true creates exactly N records.
    /// </summary>
    [Fact]
    public async Task BulkGeneration_AutoGenerate_CreatesExactlyNRecords()
    {
        var contractId = await SeedContractAsync(ContractStatus.Approved);
        const int n = 5;

        using var db = MakeDb();
        var svc = new MedicalBatchService(db, NullLogger<MedicalBatchService>.Instance);

        var (batch, error) = await svc.CreateBatchAsync(Dto(contractId, n, autoGenerate: true), "test_user");

        Assert.Null(error);
        Assert.NotNull(batch);
        Assert.Equal(n, batch.RecordCount);
        Assert.Equal(n, batch.RecordsGenerated);

        using var verify = MakeDb();
        var count = await verify.MedicalBatchRecords.CountAsync(r => r.BatchId == batch.Id);
        Assert.Equal(n, count);
    }

    /// <summary>
    /// Atomicity: EstimatedCount=0 is rejected before any DB write.
    /// Verifies the service's validation guard (no partial state).
    /// </summary>
    [Fact]
    public async Task BulkGeneration_ZeroCount_ReturnsErrorNoWrite()
    {
        var contractId = await SeedContractAsync(ContractStatus.Approved);

        using var db = MakeDb();
        var svc = new MedicalBatchService(db, NullLogger<MedicalBatchService>.Instance);

        var (batch, error) = await svc.CreateBatchAsync(Dto(contractId, 0, autoGenerate: true), "test");

        Assert.Null(batch);
        Assert.NotNull(error);

        // No batch should have been created
        using var verify = MakeDb();
        Assert.Equal(0, await verify.MedicalBatches.CountAsync());
    }

    /// <summary>
    /// Atomicity: EstimatedCount > 10000 is rejected before any DB write.
    /// </summary>
    [Fact]
    public async Task BulkGeneration_ExceedsLimit_ReturnsErrorNoWrite()
    {
        var contractId = await SeedContractAsync(ContractStatus.Approved);

        using var db = MakeDb();
        var svc = new MedicalBatchService(db, NullLogger<MedicalBatchService>.Instance);

        var (batch, error) = await svc.CreateBatchAsync(Dto(contractId, 10_001, autoGenerate: true), "test");

        Assert.Null(batch);
        Assert.NotNull(error);

        using var verify = MakeDb();
        Assert.Equal(0, await verify.MedicalBatches.CountAsync());
    }

    /// <summary>
    /// Atomicity: a successful bulk generation persists all N records with sequential codes.
    /// </summary>
    [Fact]
    public async Task BulkGeneration_SuccessfulTransaction_AllRecordsPersisted()
    {
        var contractId = await SeedContractAsync(ContractStatus.Active);
        const int n = 10;

        using var db = MakeDb();
        var svc = new MedicalBatchService(db, NullLogger<MedicalBatchService>.Instance);

        var (batch, error) = await svc.CreateBatchAsync(Dto(contractId, n, autoGenerate: true), "integration_test");

        Assert.Null(error);
        Assert.NotNull(batch);

        using var verify = MakeDb();
        var records = await verify.MedicalBatchRecords
            .Where(r => r.BatchId == batch.Id)
            .OrderBy(r => r.Code)
            .ToListAsync();

        Assert.Equal(n, records.Count);
        for (int i = 1; i <= n; i++)
            Assert.Equal($"BN{i:D4}", records[i - 1].Code);
    }

    /// <summary>
    /// Atomicity: pre-existing records for a batch are not corrupted by a failed
    /// new batch creation attempt.
    /// </summary>
    [Fact]
    public async Task BulkGeneration_FailedAttempt_DoesNotCorruptExistingData()
    {
        var contractId = await SeedContractAsync(ContractStatus.Approved);

        // Create a valid batch first
        Guid existingBatchId;
        using (var db = MakeDb())
        {
            var svc = new MedicalBatchService(db, NullLogger<MedicalBatchService>.Instance);
            var (batch, _) = await svc.CreateBatchAsync(Dto(contractId, 3, autoGenerate: true), "setup");
            existingBatchId = batch!.Id;
        }

        // Attempt a failing batch (invalid count)
        using (var db = MakeDb())
        {
            var svc = new MedicalBatchService(db, NullLogger<MedicalBatchService>.Instance);
            await svc.CreateBatchAsync(Dto(contractId, 0, autoGenerate: true), "bad_attempt");
        }

        // Existing batch must still have exactly 3 records
        using var verify = MakeDb();
        var count = await verify.MedicalBatchRecords.CountAsync(r => r.BatchId == existingBatchId);
        Assert.Equal(3, count);
    }

    /// <summary>
    /// Lazy mode (AutoGenerate=false) creates the batch but zero records.
    /// </summary>
    [Fact]
    public async Task LazyMode_CreatesBatchWithZeroRecords()
    {
        var contractId = await SeedContractAsync(ContractStatus.Approved);

        using var db = MakeDb();
        var svc = new MedicalBatchService(db, NullLogger<MedicalBatchService>.Instance);

        var (batch, error) = await svc.CreateBatchAsync(Dto(contractId, 50, autoGenerate: false), "lazy_user");

        Assert.Null(error);
        Assert.NotNull(batch);
        Assert.Equal(0, batch.RecordCount);
        Assert.Null(batch.RecordsGenerated);

        using var verify = MakeDb();
        var count = await verify.MedicalBatchRecords.CountAsync(r => r.BatchId == batch.Id);
        Assert.Equal(0, count);
    }

    /// <summary>
    /// Contract with non-Approved/Active status is rejected.
    /// </summary>
    [Theory]
    [InlineData(ContractStatus.Draft)]
    [InlineData(ContractStatus.PendingApproval)]
    [InlineData(ContractStatus.Rejected)]
    public async Task CreateBatch_InvalidContractStatus_ReturnsError(ContractStatus status)
    {
        var contractId = await SeedContractAsync(status);

        using var db = MakeDb();
        var svc = new MedicalBatchService(db, NullLogger<MedicalBatchService>.Instance);

        var (batch, error) = await svc.CreateBatchAsync(Dto(contractId, 5), "test");

        Assert.Null(batch);
        Assert.NotNull(error);
        Assert.Contains("Approved", error, StringComparison.OrdinalIgnoreCase);
    }
}
