using System.Threading.Tasks;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Services.MedicalRecords
{
    public interface IMedicalRecordStateMachine
    {
        // Basic lifecycle
        Task<ServiceResult<MedicalRecord>> CheckInAsync(int medicalRecordId, string actorUserId);
        Task<ServiceResult<MedicalRecord>> FinalizeRecordAsync(int medicalRecordId, string actorUserId);
        
        // Exception branches
        Task<ServiceResult<MedicalRecord>> MarkNoShowAsync(int medicalRecordId, string actorUserId);
        Task<ServiceResult<MedicalRecord>> QCPassAsync(int medicalRecordId, string actorUserId);
        Task<ServiceResult<MedicalRecord>> QCReworkAsync(int medicalRecordId, string actorUserId, string reason);
        Task<ServiceResult<MedicalRecord>> CancelRecordAsync(int medicalRecordId, string actorUserId, string reason);
    }
}
