using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Models.Enums;
using QuanLyDoanKham.API.Services.Contracts;
using Xunit;

namespace QuanLyDoanKham.Api.Tests.Integration;

/// <summary>
/// Integration tests for contract approval hardening.
/// Task 36 — Validates:
///   - Status gate: only PendingApproval contracts can be approved
///   - Concurrent approval: exactly one succeeds, the other gets a conflict
///   - Audit log is written on success
/// </summary>
public class ContractApprovalIntegrationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public ContractApprovalIntegrationTests()
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

    // ── Helpers ───────────────────────────────────────────────────────────────

    private ApplicationDbContext MakeDb() => new(_options);

    private HealthContractService MakeSvc(ApplicationDbContext db) =>
        new(db, NullLogger<HealthContractService>.Instance);

    private static ApprovalActionDto ApprovalDto(string note = "Approved") =>
        new() { Note = note };

    /// <summary>Seeds a minimal contract in PendingApproval status. Returns its ID.</summary>
    private async Task<int> SeedPendingContractAsync()
    {
        using var db = MakeDb();

        var company = new Company
        {
            CompanyName = "Approval Test Co",
            TaxCode = $"TAX{Guid.NewGuid().ToString("N")[..6]}",
            Address = "Test"
        };
        db.Companies.Add(company);
        await db.SaveChangesAsync();

        var contract = new HealthContract
        {
            CompanyId = company.CompanyId,
            ContractCode = $"HD-APPROVE-{Guid.NewGuid().ToString("N")[..6]}",
            ContractName = "Approval Test Contract",
            SigningDate = DateTime.Today,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddYears(1),
            UnitPrice = 100_000,
            ExpectedQuantity = 50,
            TotalAmount = 5_000_000,
            Status = ContractStatus.PendingApproval,
            CreatedAt = DateTime.UtcNow,
            CurrentApprovalStep = 0
        };
        db.Contracts.Add(contract);
        await db.SaveChangesAsync();
        return contract.HealthContractId;
    }

    /// <summary>Seeds an admin user (RoleId=1) and returns their UserId.</summary>
    private async Task<int> SeedAdminUserAsync()
    {
        using var db = MakeDb();

        // Ensure Admin role exists
        var role = await db.Roles.FirstOrDefaultAsync(r => r.RoleName == "Admin");
        if (role == null)
        {
            role = new AppRole { RoleName = "Admin", Description = "Administrator" };
            db.Roles.Add(role);
            await db.SaveChangesAsync();
        }

        var user = new AppUser
        {
            Username = $"admin_{Guid.NewGuid().ToString("N")[..6]}",
            FullName = "Test Admin",
            PasswordHash = "hash",
            RoleId = role.RoleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user.UserId;
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Status gate: approving a Draft contract returns a CONFLICT error.
    /// </summary>
    [Theory]
    [InlineData(ContractStatus.Draft)]
    [InlineData(ContractStatus.Approved)]
    [InlineData(ContractStatus.Rejected)]
    public async Task ApproveContract_NonPendingStatus_ReturnsConflict(ContractStatus status)
    {
        // Seed contract with the given non-pending status
        using var setupDb = MakeDb();
        var company = new Company { CompanyName = "Gate Test", TaxCode = $"T{Guid.NewGuid().ToString("N")[..6]}", Address = "A" };
        setupDb.Companies.Add(company);
        await setupDb.SaveChangesAsync();

        var contract = new HealthContract
        {
            CompanyId = company.CompanyId,
            ContractCode = $"HD-GATE-{Guid.NewGuid().ToString("N")[..6]}",
            ContractName = "Gate Test",
            SigningDate = DateTime.Today,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddYears(1),
            UnitPrice = 1,
            ExpectedQuantity = 1,
            TotalAmount = 1,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            CurrentApprovalStep = 0
        };
        setupDb.Contracts.Add(contract);
        await setupDb.SaveChangesAsync();

        var adminId = await SeedAdminUserAsync();

        using var db = MakeDb();
        var svc = MakeSvc(db);

        var (success, error, _) = await svc.ApproveContractAsync(
            contract.HealthContractId, ApprovalDto(), "admin", adminId);

        Assert.False(success);
        Assert.NotNull(error);
        Assert.StartsWith("CONFLICT:", error);
    }

    /// <summary>
    /// Happy path: approving a PendingApproval contract (with no approval steps configured)
    /// succeeds and the contract status becomes Approved.
    /// </summary>
    [Fact]
    public async Task ApproveContract_PendingApproval_NoSteps_Succeeds()
    {
        var contractId = await SeedPendingContractAsync();
        var adminId = await SeedAdminUserAsync();

        using var db = MakeDb();
        var svc = MakeSvc(db);

        var (success, error, newStatus) = await svc.ApproveContractAsync(
            contractId, ApprovalDto(), "admin", adminId);

        Assert.True(success, $"Expected success but got error: {error}");
        Assert.Null(error);
        Assert.Equal("Approved", newStatus);

        // Verify DB state
        using var verify = MakeDb();
        var contract = await verify.Contracts.FindAsync(contractId);
        Assert.Equal(ContractStatus.Approved, contract!.Status);
    }

    /// <summary>
    /// Audit log: a successful approval writes a ContractApprovalHistory entry.
    /// </summary>
    [Fact]
    public async Task ApproveContract_Success_WritesAuditLog()
    {
        var contractId = await SeedPendingContractAsync();
        var adminId = await SeedAdminUserAsync();

        using var db = MakeDb();
        var svc = MakeSvc(db);

        var (success, _, _) = await svc.ApproveContractAsync(
            contractId, ApprovalDto("Audit test note"), "admin_user", adminId);

        Assert.True(success);

        using var verify = MakeDb();
        var auditEntry = await verify.ContractApprovalHistories
            .FirstOrDefaultAsync(h => h.HealthContractId == contractId);

        Assert.NotNull(auditEntry);
        Assert.Equal("Approved", auditEntry.Action);
        Assert.Equal(adminId, auditEntry.ApprovedByUserId);
    }

    /// <summary>
    /// Concurrent approval: two sequential calls on the same contract.
    /// The first succeeds; the second returns a CONFLICT because the contract
    /// is no longer in PendingApproval status.
    ///
    /// NOTE: True concurrent optimistic concurrency (DbUpdateConcurrencyException)
    /// requires a real SQL Server with RowVersion. With SQLite in-memory, we test
    /// the status-gate path which is the primary guard against double-approval.
    /// The concurrent test uses two separate DbContext instances to simulate
    /// two independent requests.
    /// </summary>
    [Fact]
    public async Task ApproveContract_ConcurrentRequests_ExactlyOneSucceeds()
    {
        var contractId = await SeedPendingContractAsync();
        var adminId = await SeedAdminUserAsync();

        // First approval — uses its own DbContext
        using var db1 = MakeDb();
        var svc1 = MakeSvc(db1);
        var (success1, error1, status1) = await svc1.ApproveContractAsync(
            contractId, ApprovalDto("First"), "admin1", adminId);

        // Second approval — uses a separate DbContext (simulates a second request)
        using var db2 = MakeDb();
        var svc2 = MakeSvc(db2);
        var (success2, error2, status2) = await svc2.ApproveContractAsync(
            contractId, ApprovalDto("Second"), "admin2", adminId);

        // Exactly one must succeed
        var successCount = (success1 ? 1 : 0) + (success2 ? 1 : 0);
        Assert.Equal(1, successCount);

        // The failing one must return a CONFLICT
        var conflictError = success1 ? error2 : error1;
        Assert.NotNull(conflictError);
        Assert.StartsWith("CONFLICT:", conflictError);

        // Final contract status must be Approved
        using var verify = MakeDb();
        var contract = await verify.Contracts.FindAsync(contractId);
        Assert.Equal(ContractStatus.Approved, contract!.Status);
    }

    /// <summary>
    /// Idempotency: calling approve twice on an already-Approved contract
    /// returns a conflict on the second call.
    /// </summary>
    [Fact]
    public async Task ApproveContract_AlreadyApproved_SecondCallReturnsConflict()
    {
        var contractId = await SeedPendingContractAsync();
        var adminId = await SeedAdminUserAsync();

        // First call — should succeed
        using var db1 = MakeDb();
        var (ok, _, _) = await MakeSvc(db1).ApproveContractAsync(
            contractId, ApprovalDto(), "admin", adminId);
        Assert.True(ok);

        // Second call — should conflict
        using var db2 = MakeDb();
        var (ok2, err2, _) = await MakeSvc(db2).ApproveContractAsync(
            contractId, ApprovalDto(), "admin", adminId);
        Assert.False(ok2);
        Assert.StartsWith("CONFLICT:", err2);
    }
}
