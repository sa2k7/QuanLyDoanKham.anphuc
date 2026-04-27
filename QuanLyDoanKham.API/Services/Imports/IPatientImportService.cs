using QuanLyDoanKham.API.DTOs;

namespace QuanLyDoanKham.API.Services.Imports
{
    public interface IPatientImportService
    {
        Task<ImportResultDto> ImportFromExcelAsync(IFormFile file, int medicalGroupId);
    }
}
