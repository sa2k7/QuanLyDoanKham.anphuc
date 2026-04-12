using System.Threading.Tasks;

namespace QuanLyDoanKham.API.Services.MedicalRecords
{
    public interface ICheckInService
    {
        Task<ServiceResult<object>> ProcessCheckInTokenAsync(string qrToken, string actorUserId);
    }
}
