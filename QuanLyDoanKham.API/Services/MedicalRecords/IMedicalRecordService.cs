using System.Threading.Tasks;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Services.MedicalRecords
{
    public interface IMedicalRecordService
    {
        Task<ServiceResult<List<MedicalRecord>>> BatchIngestAsync(MedicalRecordBatchIngestRequestDto request, string createdBy);
        string GenerateQrToken(int medicalRecordId, int groupId);
        
        // Query methods
        Task<List<MedicalRecordGroupItemDto>> GetByGroupAsync(int groupId);
        Task<MedicalRecord?> GetByIdAsync(int id);
        Task<ServiceResult<List<MedicalRecord>>> BatchIngestFromExcelAsync(int groupId, string filePath, string createdBy);
        Task<ServiceResult<bool>> DeleteAsync(int id);
        Task<ServiceResult<string>> GenerateAiClinicalSummaryAsync(int recordId);
        Task<List<MedicalRecordGroupItemDto>> GetQcPendingRecordsAsync();
    }

    public class ServiceResult<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = "";
        public T? Data { get; set; }

        public static ServiceResult<T> Success(T data) => new ServiceResult<T> { IsSuccess = true, Data = data };
        public static ServiceResult<T> Failure(string message) => new ServiceResult<T> { IsSuccess = false, Message = message };
    }
}
