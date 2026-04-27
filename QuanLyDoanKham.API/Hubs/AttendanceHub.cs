using Microsoft.AspNetCore.SignalR;

namespace QuanLyDoanKham.API.Hubs;

public class AttendanceHub : Hub
{
    /// <summary>Nhân sự join vào group của đoàn khám để nhận realtime updates</summary>
    public async Task JoinTeamGroup(int teamId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"team_{teamId}");
    }

    public async Task LeaveTeamGroup(int teamId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"team_{teamId}");
    }

    /// <summary>Broadcast check-in event tới tất cả client trong team group</summary>
    public async Task NotifyCheckIn(int teamId, object checkInData)
    {
        await Clients.Group($"team_{teamId}")
            .SendAsync("StaffCheckedIn", checkInData);
    }

    /// <summary>Broadcast check-out event tới tất cả client trong team group</summary>
    public async Task NotifyCheckOut(int teamId, object checkOutData)
    {
        await Clients.Group($"team_{teamId}")
            .SendAsync("StaffCheckedOut", checkOutData);
    }

    /// <summary>Cập nhật lịch realtime khi có phân công mới cho nhân sự</summary>
    public async Task NotifyScheduleUpdate(int staffId, object scheduleItem)
    {
        await Clients.User(staffId.ToString())
            .SendAsync("ScheduleUpdated", scheduleItem);
    }
}
