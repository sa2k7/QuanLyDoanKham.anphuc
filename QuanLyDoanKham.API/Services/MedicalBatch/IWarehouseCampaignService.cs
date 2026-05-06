using System.Collections.Generic;
using System.Threading.Tasks;
using QuanLyDoanKham.API.DTOs;

namespace QuanLyDoanKham.API.Services.MedicalBatch
{
    /// <summary>
    /// Provides a warehouse-scoped view of campaigns.
    /// Fixes the 403 issue for WarehouseManager users who have Kho.View
    /// but not DoanKham.View — they cannot call GET /api/MedicalGroups.
    ///
    /// This service returns only campaigns relevant to warehouse operations
    /// (Status == "Active" || Status == "Completed").
    /// The existing MedicalGroupsController and its endpoint are NOT modified.
    /// </summary>
    public interface IWarehouseCampaignService
    {
        /// <summary>
        /// Get campaigns scoped for warehouse export operations.
        /// Filters to Status "Active" or "Completed" only.
        /// Returns an empty list (never null) when no matching campaigns exist.
        /// </summary>
        Task<IEnumerable<WarehouseCampaignDto>> GetWarehouseCampaignsAsync();
    }
}
