using System.Threading.Tasks;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Services.MedicalRecords
{
    public interface IMedicalRecordStateMachine
    {
        // Happy path
        Task<ServiceResult<MedicalRecord>> CheckInAsync(int medicalRecordId, string actorUserId);
        Task<ServiceResult<RecordStationTask>> StartStationAsync(int medicalRecordId, string stationCode, string actorUserId);
        Task<ServiceResult<RecordStationTask>> CompleteStationAsync(int medicalRecordId, string stationCode, string actorUserId, string? resultNotes = null);
        Task<ServiceResult<MedicalRecord>> FinalizeRecordAsync(int medicalRecordId, string actorUserId);
        
        // Dynamic additions (Phase 4 Add-on Services)
        Task<ServiceResult<RecordStationTask>> AddExtraStationAsync(int medicalRecordId, string stationCode, string actorUserId, string? notes = null);

        // Exception branches (Plan section 4.3)
        Task<ServiceResult<MedicalRecord>> MarkNoShowAsync(int medicalRecordId, string actorUserId);
        Task<ServiceResult<MedicalRecord>> QCPassAsync(int medicalRecordId, string actorUserId);
        Task<ServiceResult<MedicalRecord>> QCReworkAsync(int medicalRecordId, string actorUserId, string reason);
        Task<ServiceResult<MedicalRecord>> CancelRecordAsync(int medicalRecordId, string actorUserId, string reason);
    }
}
