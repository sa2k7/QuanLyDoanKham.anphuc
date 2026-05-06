using FsCheck;
using FsCheck.Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Models.Enums;
using QuanLyDoanKham.API.Services.MedicalBatch;
using QuanLyDoanKham.PropertyTests.Helpers;

namespace QuanLyDoanKham.PropertyTests.Properties;

/// <summary>
/// Property-based tests for staff assignment invariants.
/// Feature: health-check-domain-refactor
/// Properties P8–P11 from design.md.
/// </summary>
public class StaffAssignmentPropertyTests
{
    private static readonly string[] AllowedRoles =
    {
        "TiepNhan", "KhamNoi", "KhamNgoai", "LayMau", "KhamSan", "SieuAm"
    };

    private static readonly string[] DisallowedRoles =
    {
        "Admin", "Manager", "BacSi", "DieuDuong", "KyThuatVien",
        "HauCan", "TaiXe", "BaoVe", "", "   ", "UNKNOWN_ROLE"
    };

    private static StaffAssignmentValidationService MakeService(API.Data.ApplicationDbContext db) =>
        new(db, NullLogger<StaffAssignmentValidationService>.Instance);

    private static API.Data.ApplicationDbContext MakeDb() => DbContextFactory.Create();

    // ── P8: Staff double-booking prevention ───────────────────────────────────

    // Feature: health-check-domain-refactor, Property 8: Staff double-booking prevention
    [Property(MaxTest = 100)]
    public Property P8_StaffAssignment_PreventDoubleBooking()
    {
        var gen =
            from staffId in Gen.Choose(1, 500)
            from campaignA in Gen.Choose(1, 100)
            from campaignB in Gen.Choose(101, 200) // guaranteed different
            from year in Gen.Choose(2025, 2026)
            from month in Gen.Choose(1, 12)
            from day in Gen.Choose(1, 28)
            select (staffId, campaignA, campaignB, new DateTime(year, month, day));

        return Prop.ForAll(Arb.From(gen), data =>
        {
            var (staffId, campaignA, campaignB, examDate) = data;

            using var db = MakeDb();

            // Seed: staff already assigned to campaignA on examDate
            var group = new MedicalGroup
            {
                GroupId = campaignA,
                GroupName = $"Campaign {campaignA}",
                ExamDate = examDate,
                Status = "Open",
                HealthContractId = 1
            };
            db.MedicalGroups.Add(group);

            var staff = new Staff
            {
                StaffId = staffId,
                FullName = $"Staff {staffId}",
                IsActive = true,
                CreatedDate = DateTime.Now
            };
            db.Staffs.Add(staff);

            db.GroupStaffDetails.Add(new GroupStaffDetail
            {
                GroupId = campaignA,
                StaffId = staffId,
                WorkPosition = "TiepNhan",
                ExamDate = examDate,
                WorkStatus = "Pending"
            });
            db.SaveChanges();

            var svc = MakeService(db);

            // Attempt to assign same staff to campaignB on same date → must fail
            var (valid, error) = svc.ValidateAssignmentAsync(
                staffId, campaignB, examDate, "TiepNhan")
                .GetAwaiter().GetResult();

            return !valid && error != null && error.Contains($"Staff {staffId}");
        });
    }

    // ── P9: Same-campaign re-assignment is idempotent ─────────────────────────

    // Feature: health-check-domain-refactor, Property 9: Same-campaign re-assignment is idempotent
    [Property(MaxTest = 100)]
    public Property P9_StaffAssignment_SameCampaignIsIdempotent()
    {
        var gen =
            from staffId in Gen.Choose(1, 500)
            from campaignId in Gen.Choose(1, 100)
            from year in Gen.Choose(2025, 2026)
            from month in Gen.Choose(1, 12)
            from day in Gen.Choose(1, 28)
            select (staffId, campaignId, new DateTime(year, month, day));

        return Prop.ForAll(Arb.From(gen), data =>
        {
            var (staffId, campaignId, examDate) = data;

            using var db = MakeDb();

            var group = new MedicalGroup
            {
                GroupId = campaignId,
                GroupName = $"Campaign {campaignId}",
                ExamDate = examDate,
                Status = "Open",
                HealthContractId = 1
            };
            db.MedicalGroups.Add(group);

            var staff = new Staff
            {
                StaffId = staffId,
                FullName = $"Staff {staffId}",
                IsActive = true,
                CreatedDate = DateTime.Now
            };
            db.Staffs.Add(staff);

            // Staff already assigned to the SAME campaign
            db.GroupStaffDetails.Add(new GroupStaffDetail
            {
                GroupId = campaignId,
                StaffId = staffId,
                WorkPosition = "KhamNoi",
                ExamDate = examDate,
                WorkStatus = "Pending"
            });
            db.SaveChanges();

            var svc = MakeService(db);

            // Re-assigning to the same campaign → must succeed (idempotent)
            var (valid, error) = svc.ValidateAssignmentAsync(
                staffId, campaignId, examDate, "KhamNoi")
                .GetAwaiter().GetResult();

            return valid && error == null;
        });
    }

    // ── P10: Role compatibility enforcement ───────────────────────────────────

    // Feature: health-check-domain-refactor, Property 10: Role compatibility enforcement
    [Property(MaxTest = 100)]
    public Property P10_RoleCompatibility_RejectInvalidRoles()
    {
        var gen =
            from role in Gen.Elements(DisallowedRoles)
            from staffId in Gen.Choose(1, 500)
            from campaignId in Gen.Choose(1, 100)
            select (role, staffId, campaignId);

        return Prop.ForAll(Arb.From(gen), data =>
        {
            var (role, staffId, campaignId) = data;

            using var db = MakeDb();
            var svc = MakeService(db);

            var (valid, error) = svc.ValidateAssignmentAsync(
                staffId, campaignId, DateTime.Today, role)
                .GetAwaiter().GetResult();

            return !valid && error != null && error.Contains("Vai trò");
        });
    }

    // ── P10b: Allowed roles pass role check ───────────────────────────────────

    // Feature: health-check-domain-refactor, Property 10b: Allowed roles pass role compatibility check
    [Property(MaxTest = 100)]
    public Property P10b_RoleCompatibility_AcceptAllowedRoles()
    {
        var gen =
            from role in Gen.Elements(AllowedRoles)
            from staffId in Gen.Choose(1, 500)
            from campaignId in Gen.Choose(1, 100)
            select (role, staffId, campaignId);

        return Prop.ForAll(Arb.From(gen), data =>
        {
            var (role, staffId, campaignId) = data;

            using var db = MakeDb();
            // No existing assignments → only role check matters
            var svc = MakeService(db);

            var (valid, error) = svc.ValidateAssignmentAsync(
                staffId, campaignId, DateTime.Today, role)
                .GetAwaiter().GetResult();

            // Role is valid — no double-booking either → must pass
            return valid && error == null;
        });
    }

    // ── P11: Role requirement assigned count accuracy ─────────────────────────

    // Feature: health-check-domain-refactor, Property 11: Role requirement assigned count accuracy
    [Property(MaxTest = 100)]
    public Property P11_RoleRequirement_AssignedCountAccuracy()
    {
        var gen =
            from campaignId in Gen.Choose(1, 100)
            from n in Gen.Choose(0, 10) // number of staff assigned to TiepNhan
            select (campaignId, n);

        return Prop.ForAll(Arb.From(gen), data =>
        {
            var (campaignId, n) = data;

            using var db = MakeDb();

            var group = new MedicalGroup
            {
                GroupId = campaignId,
                GroupName = $"Campaign {campaignId}",
                ExamDate = DateTime.Today,
                Status = "Open",
                HealthContractId = 1
            };
            db.MedicalGroups.Add(group);

            // Seed N staff assigned to "TiepNhan" role in this campaign
            for (int i = 1; i <= n; i++)
            {
                db.Staffs.Add(new Staff
                {
                    StaffId = i,
                    FullName = $"Staff {i}",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                });
                db.GroupStaffDetails.Add(new GroupStaffDetail
                {
                    GroupId = campaignId,
                    StaffId = i,
                    WorkPosition = "TiepNhan",
                    ExamDate = DateTime.Today,
                    WorkStatus = "Pending"
                });
            }
            db.SaveChanges();

            var svc = new CampaignRoleRequirementService(
                db, NullLogger<CampaignRoleRequirementService>.Instance);

            // Set a requirement for TiepNhan
            var (req, _) = svc.SetRequirementAsync(campaignId,
                new API.DTOs.SetRoleRequirementDto { Role = "TiepNhan", RequiredCount = 5 })
                .GetAwaiter().GetResult();

            if (req == null) return false;

            // AssignedCount must equal the number of staff seeded
            return req.AssignedCount == n;
        });
    }
}
