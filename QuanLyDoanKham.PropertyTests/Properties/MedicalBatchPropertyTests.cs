using FsCheck;
using FsCheck.Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Models.Enums;
using QuanLyDoanKham.API.Services.MedicalBatch;
using QuanLyDoanKham.PropertyTests.Helpers;

namespace QuanLyDoanKham.PropertyTests.Properties;

/// <summary>
/// Property-based tests for MedicalBatch and MedicalBatchRecord invariants.
/// Feature: health-check-domain-refactor
/// Properties P1–P7 from design.md.
/// </summary>
public class MedicalBatchPropertyTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static API.Data.ApplicationDbContext MakeDb() => DbContextFactory.Create();

    private static HealthContract SeedApprovedContract(API.Data.ApplicationDbContext db)
    {
        var contract = new HealthContract
        {
            ContractName = "Test Contract",
            SigningDate = DateTime.Today.AddDays(-1),
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(30),
            TotalAmount = 10_000_000,
            Status = ContractStatus.Approved,
            CreatedAt = DateTime.Now
        };
        db.Contracts.Add(contract);
        db.SaveChanges();
        return contract;
    }

    private static MedicalBatchService MakeService(API.Data.ApplicationDbContext db) =>
        new(db, NullLogger<MedicalBatchService>.Instance);

    private static MedicalBatchRecordService MakeRecordService(API.Data.ApplicationDbContext db) =>
        new(db, NullLogger<MedicalBatchRecordService>.Instance);

    // ── P1: Contract status gate ──────────────────────────────────────────────

    // Feature: health-check-domain-refactor, Property 1: Contract status gate for MedicalBatch creation
    [Property(MaxTest = 100)]
    public Property P1_ContractStatusGate_RejectNonApprovedContracts()
    {
        var disallowedStatuses = new[]
        {
            ContractStatus.Draft,
            ContractStatus.PendingApproval,
            ContractStatus.Rejected,
            ContractStatus.Cancelled,
            ContractStatus.Finished,
            ContractStatus.Locked
        };

        var gen = Gen.Elements(disallowedStatuses);

        return Prop.ForAll(Arb.From(gen), status =>
        {
            using var db = MakeDb();
            var contract = new HealthContract
            {
                ContractName = "Test",
                SigningDate = DateTime.Today,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1),
                TotalAmount = 1_000_000,
                Status = status,
                CreatedAt = DateTime.Now
            };
            db.Contracts.Add(contract);
            db.SaveChanges();

            var svc = MakeService(db);
            var dto = new CreateMedicalBatchDto
            {
                ContractId = contract.HealthContractId,
                EstimatedCount = 10,
                AutoGenerate = false
            };

            var (batch, error) = svc.CreateBatchAsync(dto, "test").GetAwaiter().GetResult();

            return batch == null && error != null;
        });
    }

    // ── P2: EstimatedCount must be positive ───────────────────────────────────

    // Feature: health-check-domain-refactor, Property 2: EstimatedCount must be positive
    [Property(MaxTest = 100)]
    public Property P2_EstimatedCount_RejectNonPositive()
    {
        var gen = Gen.Choose(int.MinValue, 0);

        return Prop.ForAll(Arb.From(gen), count =>
        {
            using var db = MakeDb();
            var contract = SeedApprovedContract(db);
            var svc = MakeService(db);

            var dto = new CreateMedicalBatchDto
            {
                ContractId = contract.HealthContractId,
                EstimatedCount = count,
                AutoGenerate = false
            };

            var (batch, error) = svc.CreateBatchAsync(dto, "test").GetAwaiter().GetResult();

            return batch == null && error != null;
        });
    }

    // ── P3: EstimatedCount upper bound ────────────────────────────────────────

    // Feature: health-check-domain-refactor, Property 3: EstimatedCount upper bound
    [Property(MaxTest = 100)]
    public Property P3_EstimatedCount_RejectAbove10000()
    {
        var gen = Gen.Choose(10_001, 100_000);

        return Prop.ForAll(Arb.From(gen), count =>
        {
            using var db = MakeDb();
            var contract = SeedApprovedContract(db);
            var svc = MakeService(db);

            var dto = new CreateMedicalBatchDto
            {
                ContractId = contract.HealthContractId,
                EstimatedCount = count,
                AutoGenerate = true
            };

            var (batch, error) = svc.CreateBatchAsync(dto, "test").GetAwaiter().GetResult();

            return batch == null && error != null;
        });
    }

    // ── P4: Bulk generation produces exactly EstimatedCount records ───────────

    // Feature: health-check-domain-refactor, Property 4: Bulk generation produces exactly EstimatedCount records
    [Property(MaxTest = 100)]
    public Property P4_BulkGeneration_ProducesExactCount()
    {
        var gen = Gen.Choose(1, 100);

        return Prop.ForAll(Arb.From(gen), n =>
        {
            using var db = MakeDb();
            var contract = SeedApprovedContract(db);
            var svc = MakeService(db);

            var dto = new CreateMedicalBatchDto
            {
                ContractId = contract.HealthContractId,
                EstimatedCount = n,
                AutoGenerate = true
            };

            var (batch, error) = svc.CreateBatchAsync(dto, "test").GetAwaiter().GetResult();

            if (batch == null) return false;

            var actualCount = db.MedicalBatchRecords
                .Count(r => r.BatchId == batch.Id);

            return error == null
                && batch.RecordsGenerated == n
                && actualCount == n;
        });
    }

    // ── P5: Sequential code generation within batch scope ────────────────────

    // Feature: health-check-domain-refactor, Property 5: Sequential code generation within batch scope
    [Property(MaxTest = 100)]
    public Property P5_CodeGeneration_IsSequentialWithinBatch()
    {
        var gen = Gen.Choose(1, 50);

        return Prop.ForAll(Arb.From(gen), n =>
        {
            using var db = MakeDb();
            var contract = SeedApprovedContract(db);
            var svc = MakeService(db);

            var dto = new CreateMedicalBatchDto
            {
                ContractId = contract.HealthContractId,
                EstimatedCount = n,
                AutoGenerate = true
            };

            var (batch, _) = svc.CreateBatchAsync(dto, "test").GetAwaiter().GetResult();
            if (batch == null) return false;

            var codes = db.MedicalBatchRecords
                .Where(r => r.BatchId == batch.Id)
                .OrderBy(r => r.Code)
                .Select(r => r.Code)
                .ToList();

            // Must have exactly N codes, no duplicates, sequential BN0001..BN{N:D4}
            if (codes.Count != n) return false;
            if (codes.Distinct().Count() != n) return false; // no duplicates

            for (int i = 1; i <= n; i++)
            {
                var expected = $"BN{i:D4}";
                if (codes[i - 1] != expected) return false;
            }

            return true;
        });
    }

    // ── P6: Lazy creation respects EstimatedCount cap ─────────────────────────

    // Feature: health-check-domain-refactor, Property 6: Lazy creation respects EstimatedCount cap
    [Property(MaxTest = 100)]
    public Property P6_LazyCreation_RejectsWhenAtCapacity()
    {
        var gen = Gen.Choose(1, 20);

        return Prop.ForAll(Arb.From(gen), capacity =>
        {
            using var db = MakeDb();
            var contract = SeedApprovedContract(db);

            // Create batch at full capacity (bulk mode)
            var batchSvc = MakeService(db);
            var dto = new CreateMedicalBatchDto
            {
                ContractId = contract.HealthContractId,
                EstimatedCount = capacity,
                AutoGenerate = true
            };
            var (batch, _) = batchSvc.CreateBatchAsync(dto, "test").GetAwaiter().GetResult();
            if (batch == null) return false;

            // Attempt one more lazy record — must be rejected
            var recordSvc = MakeRecordService(db);
            var (record, error) = recordSvc.CreateRecordAsync(batch.Id).GetAwaiter().GetResult();

            return record == null && error != null && error.Contains("EstimatedCount");
        });
    }

    // ── P7: Done status is idempotency-guarded ────────────────────────────────

    // Feature: health-check-domain-refactor, Property 7: Done status is idempotency-guarded
    [Property(MaxTest = 100)]
    public Property P7_DoneStatus_IdempotencyGuard()
    {
        var gen = Gen.Choose(1, 10); // batch size

        return Prop.ForAll(Arb.From(gen), n =>
        {
            using var db = MakeDb();
            var contract = SeedApprovedContract(db);
            var batchSvc = MakeService(db);

            var dto = new CreateMedicalBatchDto
            {
                ContractId = contract.HealthContractId,
                EstimatedCount = n,
                AutoGenerate = true
            };
            var (batch, _) = batchSvc.CreateBatchAsync(dto, "test").GetAwaiter().GetResult();
            if (batch == null) return false;

            var recordId = db.MedicalBatchRecords
                .Where(r => r.BatchId == batch.Id)
                .Select(r => r.Id)
                .First();

            var recordSvc = MakeRecordService(db);

            // First update → success
            var (ok1, err1) = recordSvc.UpdateStatusToDoneAsync(recordId).GetAwaiter().GetResult();

            // Second update → conflict (idempotency guard)
            var (ok2, err2) = recordSvc.UpdateStatusToDoneAsync(recordId).GetAwaiter().GetResult();

            return ok1 && err1 == null
                && !ok2 && err2 != null && err2.Contains("Done");
        });
    }
}
