using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace QuanLyDoanKham.API.Hubs
{
    /// <summary>
    /// Real-time hub broadcasting queue state to station views and the kiosk check-in board.
    /// 
    /// Events emitted by the server:
    ///   - QueueUpdated(type: string)             → global refresh trigger ("ALL" | "QC")
    ///   - StationQueueUpdated(stationCode)       → per-station refresh trigger
    ///   - NewPatientArrived(payload: object)     → rich patient-arrived payload for Kiosk display
    ///   - PatientStatusChanged(payload: object)  → granular status update for a specific patient
    ///
    /// Client-invokable methods:
    ///   - JoinStationGroup(stationCode)
    ///   - LeaveStationGroup(stationCode)
    /// </summary>
    [Authorize]
    public class QueueHub : Hub
    {
        public async Task JoinStationGroup(string stationCode)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, stationCode);
        }

        public async Task LeaveStationGroup(string stationCode)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, stationCode);
        }
    }

    /// <summary>Strongly-typed payload for the NewPatientArrived event.</summary>
    public record NewPatientArrivedPayload(
        int MedicalRecordId,
        string FullName,
        int QueueNo,
        string? Gender,
        string FirstStationCode,
        DateTime CheckInAt
    );

    /// <summary>Strongly-typed payload for per-patient status changes.</summary>
    public record PatientStatusChangedPayload(
        int MedicalRecordId,
        string NewStatus,
        string StationCode,
        int QueueNo
    );
}
