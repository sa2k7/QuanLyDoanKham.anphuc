using System.Threading.Tasks;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Services.Settlement
{
    public interface ICostCalculationService
    {
        Task<CostSnapshot> CalculateAndSaveSnapshotAsync(int contractId, string snapshotType, string? note = null, int? userId = null);
        Task<CostSnapshot?> GetLatestSnapshotAsync(int contractId, string snapshotType);
    }
}
