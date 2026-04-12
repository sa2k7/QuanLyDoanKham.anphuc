using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Services.MedicalRecords;
using QuanLyDoanKham.API.Services.Settlement;

namespace QuanLyDoanKham.API.Services.MedicalGroups
{
    /// <summary>
    /// Implements the financial lifecycle of a MedicalGroup.
    /// The locking sequence:
    ///   1. CheckReadyToLockAsync() — validates all records are terminal, marks ReadyToLock
    ///   2. LockGroupAsync()        — admin confirms, salaries computed, ACT snapshot saved, group Locked
    /// </summary>
    public class GroupLifecycleService : IGroupLifecycleService
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeSheetService _timeSheetService;
        private readonly ICostCalculationService _costCalculationService;

        // Terminal states for MedicalRecord — no more action needed
        private static readonly HashSet<string> TerminalStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "COMPLETED", "NO_SHOW", "CANCELLED"
        };

        public GroupLifecycleService(
            ApplicationDbContext context,
            TimeSheetService timeSheetService,
            ICostCalculationService costCalculationService)
        {
            _context = context;
            _timeSheetService = timeSheetService;
            _costCalculationService = costCalculationService;
        }

        // ─────────────────────────────────────────────────────────
        // 1. Check ReadyToLock
        // ─────────────────────────────────────────────────────────
        public async Task<ServiceResult<GroupLockStatusDto>> CheckReadyToLockAsync(int groupId)
        {
            var group = await _context.MedicalGroups.FindAsync(groupId);
            if (group == null)
                return ServiceResult<GroupLockStatusDto>.Failure("Không tìm thấy đoàn khám.");

            if (group.Status == "Locked")
                return ServiceResult<GroupLockStatusDto>.Failure("Đoàn khám đã được khóa sổ rồi.");

            var records = await _context.MedicalRecords
                .Where(r => r.GroupId == groupId)
                .Select(r => r.Status)
                .ToListAsync();

            int total = records.Count;
            int completed = records.Count(s => string.Equals(s, "COMPLETED", StringComparison.OrdinalIgnoreCase));
            int noShow = records.Count(s => string.Equals(s, "NO_SHOW", StringComparison.OrdinalIgnoreCase));
            int cancelled = records.Count(s => string.Equals(s, "CANCELLED", StringComparison.OrdinalIgnoreCase));
            int pending = total - completed - noShow - cancelled;

            var blocking = new List<string>();
            if (pending > 0)
                blocking.Add($"Còn {pending} hồ sơ chưa hoàn tất (trạng thái chưa phải COMPLETED / NO_SHOW / CANCELLED).");

            bool isReady = blocking.Count == 0;

            // Auto-transition to ReadyToLock if eligible and not yet there
            if (isReady && group.Status != "ReadyToLock")
            {
                group.Status = "ReadyToLock";
                group.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return ServiceResult<GroupLockStatusDto>.Success(new GroupLockStatusDto
            {
                GroupId = groupId,
                Status = group.Status,
                IsReadyToLock = isReady,
                TotalRecords = total,
                CompletedRecords = completed,
                NoShowRecords = noShow,
                PendingRecords = pending,
                BlockingReasons = blocking
            });
        }

        // ─────────────────────────────────────────────────────────
        // 2. Lock Group (Admin confirmed)
        // ─────────────────────────────────────────────────────────
        public async Task<ServiceResult<GroupLockResultDto>> LockGroupAsync(int groupId, int actorUserId)
        {
            var group = await _context.MedicalGroups
                .Include(g => g.HealthContract)
                .FirstOrDefaultAsync(g => g.GroupId == groupId);

            if (group == null)
                return ServiceResult<GroupLockResultDto>.Failure("Không tìm thấy đoàn khám.");

            if (group.Status == "Locked")
                return ServiceResult<GroupLockResultDto>.Failure("Đoàn khám đã được khóa sổ rồi.");

            if (group.Status != "ReadyToLock" && group.Status != "Finished")
            {
                // Re-validate before rejecting — let admin force-check first
                var checkResult = await CheckReadyToLockAsync(groupId);
                if (!checkResult.IsSuccess || !checkResult.Data!.IsReadyToLock)
                    return ServiceResult<GroupLockResultDto>.Failure(
                        $"Chưa thể khóa sổ: {string.Join(" ", checkResult.Data?.BlockingReasons ?? new List<string> { "Vui lòng kiểm tra lại trạng thái đoàn." })}");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Step 1: Compute CalculatedSalary for all "Joined" staff using TimeSheetService
                var staffDetails = await _context.GroupStaffDetails
                    .Include(gsd => gsd.Staff)
                    .Where(gsd => gsd.GroupId == groupId && gsd.WorkStatus == "Đã tham gia")
                    .ToListAsync();

                decimal totalLaborCost = 0;
                foreach (var detail in staffDetails)
                {
                    double finalShiftType = detail.ShiftType;

                    // If actual check-in/check-out times are recorded, compute shift type from them
                    if (detail.CheckInTime.HasValue && detail.CheckOutTime.HasValue)
                    {
                        var computedUnits = _timeSheetService.ComputeWorkUnits(
                            detail.CheckInTime.Value,
                            detail.CheckOutTime.Value);
                        finalShiftType = (double)computedUnits;
                        detail.ShiftType = finalShiftType;
                    }

                    // BaseSalary × ShiftType
                    decimal baseSalary = detail.Staff?.BaseSalary ?? 0;
                    detail.CalculatedSalary = baseSalary * (decimal)finalShiftType;
                    totalLaborCost += detail.CalculatedSalary;
                }

                await _context.SaveChangesAsync();

                // Step 2: Create ACT CostSnapshot for the contract
                int contractId = group.HealthContractId;
                var snapshot = await _costCalculationService.CalculateAndSaveSnapshotAsync(
                    contractId,
                    "ACT",
                    $"Khóa sổ Đoàn #{groupId} — {group.GroupName}",
                    actorUserId);

                // Step 3: Mark group as Locked
                group.Status = "Locked";
                group.UpdatedAt = DateTime.Now;
                group.UpdatedBy = actorUserId.ToString();
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return ServiceResult<GroupLockResultDto>.Success(new GroupLockResultDto
                {
                    GroupId = groupId,
                    GroupName = group.GroupName,
                    StaffSalariesCalculated = staffDetails.Count,
                    TotalLaborCost = totalLaborCost,
                    CostSnapshotId = snapshot.SnapshotId,
                    LockedAt = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ServiceResult<GroupLockResultDto>.Failure($"Lỗi khi khóa sổ: {ex.Message}");
            }
        }

        // ─────────────────────────────────────────────────────────
        // 3. Backfill: mark old "Finished" groups as Locked
        // ─────────────────────────────────────────────────────────
        public async Task<ServiceResult<BackfillResultDto>> BackfillFinishedGroupsAsync(int actorUserId)
        {
            var finishedGroups = await _context.MedicalGroups
                .Where(g => g.Status == "Finished")
                .ToListAsync();

            int processed = 0;
            int skipped = 0;
            var errors = new List<string>();

            foreach (var group in finishedGroups)
            {
                try
                {
                    // Check if a CostSnapshot ACT already exists for this contract + group timeframe
                    var existingSnapshot = await _costCalculationService
                        .GetLatestSnapshotAsync(group.HealthContractId, "ACT");

                    if (existingSnapshot == null)
                    {
                        // Create snapshot from whatever data is available (best-effort)
                        await _costCalculationService.CalculateAndSaveSnapshotAsync(
                            group.HealthContractId,
                            "ACT",
                            $"[BACKFILL] Đoàn #{group.GroupId} — {group.GroupName}",
                            actorUserId);
                    }

                    group.Status = "Locked";
                    group.UpdatedAt = DateTime.Now;
                    group.UpdatedBy = $"backfill:{actorUserId}";
                    processed++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Group #{group.GroupId} ({group.GroupName}): {ex.Message}");
                    skipped++;
                }
            }

            await _context.SaveChangesAsync();

            return ServiceResult<BackfillResultDto>.Success(new BackfillResultDto
            {
                GroupsProcessed = processed,
                GroupsSkipped = skipped,
                Errors = errors
            });
        }
    }
}
