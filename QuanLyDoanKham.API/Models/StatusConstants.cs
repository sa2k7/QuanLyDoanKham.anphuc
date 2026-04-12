namespace QuanLyDoanKham.API.Models;

/// <summary>
/// Nguồn dữ liệu duy nhất cho tên trạng thái trong toàn hệ thống.
/// Tránh dùng magic string rải rác trong code.
/// </summary>
public static class RecordStatus
{
    public const string Created        = "CREATED";
    public const string Ready          = "READY";
    public const string CheckedIn      = "CHECKED_IN";
    public const string InProgress     = "IN_PROGRESS";
    public const string QcPending      = "QC_PENDING";
    public const string QcPassed       = "QC_PASSED";
    public const string QcRework       = "QC_REWORK";
    public const string Completed      = "COMPLETED";
    public const string NoShow         = "NO_SHOW";
    public const string Cancelled      = "CANCELLED";
}

public static class StationTaskStatus
{
    public const string Waiting           = "WAITING";
    public const string StationInProgress = "STATION_IN_PROGRESS";
    public const string StationDone       = "STATION_DONE";
    public const string Skipped           = "SKIPPED"; // Phase 2: patient bypassed this station
}
