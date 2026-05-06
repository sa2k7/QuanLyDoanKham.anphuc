using System.Collections.Generic;
using System.Threading.Tasks;
using QuanLyDoanKham.API.DTOs;

namespace QuanLyDoanKham.API.Services.MedicalBatch
{
    /// <summary>
    /// Service for managing per-campaign role quotas (CampaignRoleRequirement).
    /// Used by the UI to warn when staff assignment is under or over the required count.
    /// </summary>
    public interface ICampaignRoleRequirementService
    {
        /// <summary>
        /// Upsert a role requirement for a campaign.
        /// If (CampaignId, Role) already exists, updates RequiredCount.
        /// If not, inserts a new row.
        /// Returns (null, errorMessage) on validation failure (e.g. RequiredCount &lt; 1).
        /// </summary>
        Task<(RoleRequirementWithCountDto? Requirement, string? ErrorMessage)> SetRequirementAsync(
            int campaignId,
            SetRoleRequirementDto dto);

        /// <summary>
        /// Get all role requirements for a campaign, each enriched with the current
        /// AssignedCount (live count from GroupStaffDetails for that role + campaign).
        /// Returns an empty list (never null) when no requirements are defined.
        /// </summary>
        Task<IEnumerable<RoleRequirementWithCountDto>> GetRequirementsWithCountsAsync(int campaignId);
    }
}
