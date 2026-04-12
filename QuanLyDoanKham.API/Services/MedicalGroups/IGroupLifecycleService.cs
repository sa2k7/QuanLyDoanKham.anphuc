using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Services.MedicalRecords;

namespace QuanLyDoanKham.API.Services.MedicalGroups
{
    /// <summary>
    /// Manages the financial lifecycle of a MedicalGroup: ReadyToLock validation and Lock finalization.
    /// </summary>
    public interface IGroupLifecycleService
    {
        /// <summary>
        /// Checks if all MedicalRecords in the group are in a terminal state (COMPLETED or NO_SHOW).
        /// If so, marks the group as ReadyToLock without actually locking it.
        /// </summary>
        Task<ServiceResult<GroupLockStatusDto>> CheckReadyToLockAsync(int groupId);

        /// <summary>
        /// Admin-triggered lock. Calculates CalculatedSalary for all Joined staff,
        /// saves a CostSnapshot(ACT), and sets Group.Status = Locked.
        /// Group must be in ReadyToLock state.
        /// </summary>
        Task<ServiceResult<GroupLockResultDto>> LockGroupAsync(int groupId, int actorUserId);

        /// <summary>
        /// Backfill: for groups already in "Finished" status, create a CostSnapshot (ACT)
        /// and mark them as Locked. Run once on migration.
        /// </summary>
        Task<ServiceResult<BackfillResultDto>> BackfillFinishedGroupsAsync(int actorUserId);
    }

    public class GroupLockStatusDto
    {
        public int GroupId { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsReadyToLock { get; set; }
        public int TotalRecords { get; set; }
        public int CompletedRecords { get; set; }
        public int NoShowRecords { get; set; }
        public int PendingRecords { get; set; }
        public List<string> BlockingReasons { get; set; } = new();
    }

    public class GroupLockResultDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int StaffSalariesCalculated { get; set; }
        public decimal TotalLaborCost { get; set; }
        public int CostSnapshotId { get; set; }
        public DateTime LockedAt { get; set; }
    }

    public class BackfillResultDto
    {
        public int GroupsProcessed { get; set; }
        public int GroupsSkipped { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
