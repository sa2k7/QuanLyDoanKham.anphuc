using System;
using System.IO;
using System.Threading.Tasks;

namespace QuanLyDoanKham.API.Services.Reports
{
    public interface IReportExportService
    {
        Task<byte[]> GenerateDashboardPdfAsync(DateTime? startDate, DateTime? endDate);
        Task<byte[]> GenerateDashboardExcelAsync(DateTime? startDate, DateTime? endDate);
    }
}
