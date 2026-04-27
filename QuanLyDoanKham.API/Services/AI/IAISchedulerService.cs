namespace QuanLyDoanKham.API.Services.AI;

public interface IAISchedulerService
{
    Task<ScheduleProposal> ProposeAsync(int teamId);
}

public class ScheduleProposal
{
    public int TeamId { get; set; }
    public List<Assignment> Assignments { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public class Assignment
{
    public int PositionId { get; set; }
    public string PositionName { get; set; } = null!;
    public int StaffId { get; set; }
    public string StaffName { get; set; } = null!;
    public bool IsFallback { get; set; }
}
