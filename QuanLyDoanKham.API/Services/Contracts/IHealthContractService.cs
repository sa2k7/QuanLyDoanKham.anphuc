using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Services.Contracts
{
    public interface IHealthContractService
    {
        Task<IEnumerable<HealthContractDto>> GetAllContractsAsync(string? status, int? companyId);
        Task<HealthContractDto?> GetContractByIdAsync(int id);
        Task<(HealthContract? Contract, string? ErrorMessage)> CreateContractAsync(HealthContractDto dto, string username, int? userId);
        Task<(bool Success, string? ErrorMessage)> UpdateContractAsync(int id, HealthContractDto dto, string username);
        Task<(bool Success, string? ErrorMessage, string? StatusName)> SubmitForApprovalAsync(int id, string username);
        Task<(bool Success, string? ErrorMessage, string? NewStatus)> ApproveContractAsync(int id, ApprovalActionDto dto, string username, int userId);
        Task<(bool Success, string? ErrorMessage)> RejectContractAsync(int id, ApprovalActionDto dto, string username, int userId);
        Task<(bool Success, string? ErrorMessage)> UpdateStatusAsync(int id, StatusUpdateDto dto, string username);
        Task<(bool Success, string? ErrorMessage)> DeleteContractAsync(int id);
        Task<(bool Success, string? ErrorMessage)> LockContractAsync(int id, string username);
        Task<(bool Success, string? ErrorMessage)> UnlockContractAsync(int id, string username);
    }
}
