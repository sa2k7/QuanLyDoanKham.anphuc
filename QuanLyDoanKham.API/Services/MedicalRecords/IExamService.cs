using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.DTOs;

namespace QuanLyDoanKham.API.Services.MedicalRecords
{
    public interface IExamService
    {
        Task<ServiceResult<bool>> SaveExamResultAsync(SaveExamResultDto dto, string actorUserId);
        Task<ServiceResult<List<ExamResultResponseDto>>> GetResultsByRecordIdAsync(int recordId);
    }
}
