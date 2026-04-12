using QuanLyDoanKham.API.DTOs;

namespace QuanLyDoanKham.API.Services.MedicalGroups
{
    public interface IMedicalGroupAutoAssignmentService
    {
        Task<ApiResult<AutoCreateGroupWithStaffResponseDto>> AutoCreateAndAssignAsync(AutoCreateGroupWithStaffRequestDto request, string userId);
    }

    // A generic wrapper for service results (compatible with project patterns)
    public class ApiResult<T>
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }

        public static ApiResult<T> Success(T data, string? message = null) 
            => new ApiResult<T> { IsSuccess = true, Data = data, Message = message };
            
        public static ApiResult<T> Failure(string message) 
            => new ApiResult<T> { IsSuccess = false, Message = message };
    }
}
