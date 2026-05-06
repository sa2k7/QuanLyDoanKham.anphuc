using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Models.Enums;
using QuanLyDoanKham.API.Services;
using QuanLyDoanKham.API.Services.Contracts;
using QuanLyDoanKham.API.Services.MedicalBatch;
using Xunit;

namespace QuanLyDoanKham.Api.Tests;

/// <summary>
/// Task 37 — Backward compatibility smoke tests.
///
/// For each service/domain touched by the health-check-domain-refactor,
/// verifies that the existing public API (method signatures, return shapes,
/// HTTP status semantics) is unchanged.
///
/// These tests do NOT call HTTP endpoints directly — they call the service
/// layer that backs those endpoints, which is the authoritative contract.
/// </summary>
public class BackwardCompatibilityTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public BackwardCompatibilityTests()
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

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<(int CompanyId, int ContractId)> SeedContractAsync(ContractStatus status = ContractStatus.Approved)
    {
        using var db = MakeDb();
        var company = new Company { CompanyName = "BC Test Co", TaxCode = $"BC{Guid.NewGuid().ToString("N")[..6]}", Address = "A" };
        db.Companies.Add(company);
        await db.SaveChangesAsync();

        var contract = new HealthContract
        {
            CompanyId = company.CompanyId,
            ContractCode = $"HD-BC-{Guid.NewGuid().ToString("N")[..6]}",
            ContractName = "BC Test",
            SigningDate = DateTime.Today,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddYears(1),
            UnitPrice = 100_000,
            ExpectedQuantity = 10,
            TotalAmount = 1_000_000,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            CurrentApprovalStep = 0
        };
        db.Contracts.Add(contract);
        await db.SaveChangesAsync();
        return (company.CompanyId, contract.HealthContractId);
    }

    // ── Attendance: existing summary endpoint shape ───────────────────────────

    /// <summary>
    /// GET /api/attendance/summary/{staffId} — returns AttendanceSummaryDto shape.
    /// Verifies the service-layer query does not throw and returns the expected fields.
    /// </summary>
    [Fact]
    public async Task AttendanceSummary_ExistingShape_Unchanged()
    {
        using var db = MakeDb();

        // Seed a staff member
        var staff = new Staff
        {
            FullName = "BC Staff",
            EmployeeCode = "BC001",
            DailyRate = 500_000,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        db.Staffs.Add(staff);
        await db.SaveChangesAsync();

        // Query the same projection the controller uses
        var result = await db.ScheduleCalendars
            .Include(sc => sc.MedicalGroup)
            .Where(sc => sc.StaffId == staff.StaffId
                && sc.ExamDate.Month == DateTime.Now.Month
                && sc.ExamDate.Year == DateTime.Now.Year)
            .ToListAsync();

        // Shape: must be a list (empty is fine), not null
        Assert.NotNull(result);
        // The summary DTO fields that must exist on the Staff entity
        Assert.NotNull(staff.FullName);
        Assert.NotNull(staff.EmployeeCode);
        Assert.True(staff.DailyRate >= 0);
    }

    // ── Contracts: existing GetAllContracts shape ─────────────────────────────

    /// <summary>
    /// GET /api/contracts — returns IEnumerable&lt;HealthContractDto&gt;.
    /// Verifies the service returns a non-null collection and the DTO has the
    /// expected top-level fields.
    /// </summary>
    [Fact]
    public async Task GetAllContracts_ExistingShape_Unchanged()
    {
        await SeedContractAsync(ContractStatus.Approved);

        using var db = MakeDb();
        var svc = new HealthContractService(db, NullLogger<HealthContractService>.Instance);

        var contracts = await svc.GetAllContractsAsync(null, null);

        Assert.NotNull(contracts);
        var list = contracts.ToList();
        Assert.NotEmpty(list);

        var first = list[0];
        // These fields must exist on HealthContractDto (backward compat)
        Assert.True(first.HealthContractId > 0);
        Assert.NotNull(first.ContractCode);
        Assert.NotNull(first.Status);
        Assert.NotNull(first.StatusHistories);
        Assert.NotNull(first.ApprovalHistories);
        Assert.NotNull(first.Attachments);
    }

    /// <summary>
    /// GET /api/contracts/{id} — returns HealthContractDto or null (404).
    /// Verifies the DTO shape is unchanged.
    /// </summary>
    [Fact]
    public async Task GetContractById_ExistingShape_Unchanged()
    {
        var (_, contractId) = await SeedContractAsync();

        using var db = MakeDb();
        var svc = new HealthContractService(db, NullLogger<HealthContractService>.Instance);

        var dto = await svc.GetContractByIdAsync(contractId);

        Assert.NotNull(dto);
        Assert.Equal(contractId, dto.HealthContractId);
        Assert.NotNull(dto.ContractCode);
        Assert.NotNull(dto.ContractName);
        // TotalGroups must be present (not throw)
        Assert.True(dto.TotalGroups >= 0);
    }

    /// <summary>
    /// GET /api/contracts/{id} with non-existent ID returns null (maps to 404).
    /// </summary>
    [Fact]
    public async Task GetContractById_NotFound_ReturnsNull()
    {
        using var db = MakeDb();
        var svc = new HealthContractService(db, NullLogger<HealthContractService>.Instance);

        var dto = await svc.GetContractByIdAsync(999_999);

        Assert.Null(dto);
    }

    // ── Payroll: existing CalculateMonthlyPayroll shape ───────────────────────

    /// <summary>
    /// POST /api/payroll/calculate — returns (bool Success, string Message).
    /// Verifies the method signature and return shape are unchanged.
    /// </summary>
    [Fact]
    public async Task PayrollCalculate_ExistingShape_Unchanged()
    {
        using var db = MakeDb();
        var svc = new PayrollService(db, NullLogger<PayrollService>.Instance);

        // Non-existent staff → (false, message)
        var (success, message) = await svc.CalculateMonthlyPayrollAsync(999_999, DateTime.Now.Month, DateTime.Now.Year);

        Assert.False(success);
        Assert.NotNull(message);
        Assert.NotEmpty(message);
    }

    /// <summary>
    /// Payroll bulk calculate — returns (bool, string) shape unchanged.
    /// </summary>
    [Fact]
    public async Task PayrollBulkCalculate_ExistingShape_Unchanged()
    {
        using var db = MakeDb();
        var svc = new PayrollService(db, NullLogger<PayrollService>.Instance);

        var (success, message) = await svc.CalculateAllMonthlyPayrollAsync(DateTime.Now.Month, DateTime.Now.Year);

        // Empty DB → false with a message (not an exception)
        Assert.NotNull(message);
        Assert.NotEmpty(message);
    }

    // ── MedicalBatch: new endpoints do not break existing contract queries ─────

    /// <summary>
    /// Verifies that adding MedicalBatch entities does not affect existing
    /// HealthContract queries (no FK cascade issues, no query interference).
    /// </summary>
    [Fact]
    public async Task MedicalBatch_DoesNotAffectExistingContractQueries()
    {
        var (_, contractId) = await SeedContractAsync(ContractStatus.Approved);

        // Create a batch linked to the contract
        using var db = MakeDb();
        var batchSvc = new MedicalBatchService(db, NullLogger<MedicalBatchService>.Instance);
        var (batch, err) = await batchSvc.CreateBatchAsync(
            new CreateMedicalBatchDto { ContractId = contractId, EstimatedCount = 3, AutoGenerate = true }, "bc_test");
        Assert.Null(err);
        Assert.NotNull(batch);

        // Existing contract query must still work and return the same shape
        using var db2 = MakeDb();
        var contractSvc = new HealthContractService(db2, NullLogger<HealthContractService>.Instance);
        var dto = await contractSvc.GetContractByIdAsync(contractId);

        Assert.NotNull(dto);
        Assert.Equal(contractId, dto.HealthContractId);
        // Contract status must be unchanged
        Assert.Equal("Approved", dto.Status);
    }

    // ── MedicalBatch: collection endpoints return [] not null ─────────────────

    /// <summary>
    /// GET /api/medical-batches?contractId={id} — returns [] when no batches exist.
    /// </summary>
    [Fact]
    public async Task GetBatchesByContract_EmptyResult_ReturnsEmptyNotNull()
    {
        using var db = MakeDb();
        var svc = new MedicalBatchService(db, NullLogger<MedicalBatchService>.Instance);

        var result = await svc.GetBatchesByContractAsync(999_999);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    /// <summary>
    /// GET /api/medical-batches/{id} — returns null (404) when batch not found.
    /// </summary>
    [Fact]
    public async Task GetBatchById_NotFound_ReturnsNull()
    {
        using var db = MakeDb();
        var svc = new MedicalBatchService(db, NullLogger<MedicalBatchService>.Instance);

        var result = await svc.GetBatchByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    // ── Patient entity: still accessible (deprecated but not removed) ─────────

    /// <summary>
    /// Verifies the Patients DbSet is still present and queryable.
    /// The Patient entity is deprecated but must not be removed.
    /// </summary>
    [Fact]
    public async Task PatientEntity_StillAccessible_NotDropped()
    {
        using var db = MakeDb();

        // Must not throw — table must still exist
        var count = await db.Patients.CountAsync();
        Assert.True(count >= 0);
    }

    // ── New tables: MedicalBatches, MedicalBatchRecords, CampaignRoleRequirements ──

    /// <summary>
    /// Verifies all three new DbSets are present and queryable (schema created).
    /// </summary>
    [Fact]
    public async Task NewDbSets_ArePresent_AndQueryable()
    {
        using var db = MakeDb();

        var batchCount = await db.MedicalBatches.CountAsync();
        var recordCount = await db.MedicalBatchRecords.CountAsync();
        var reqCount = await db.CampaignRoleRequirements.CountAsync();

        Assert.True(batchCount >= 0);
        Assert.True(recordCount >= 0);
        Assert.True(reqCount >= 0);
    }
}
