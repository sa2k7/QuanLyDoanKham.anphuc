using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDoanKham.API.Authorization;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Services.MedicalBatch;

namespace QuanLyDoanKham.API.Controllers
{
    /// <summary>
    /// Manages per-campaign role quotas (CampaignRoleRequirement).
    /// Used by the UI to warn when staff assignment is under or over the required count.
    /// NEW controller — does NOT modify any existing endpoint.
    /// </summary>
    [Route("api/campaigns")]
    [ApiController]
    [Authorize]
    public class CampaignRoleRequirementsController : ControllerBase
    {
        private readonly ICampaignRoleRequirementService _requirementService;

        public CampaignRoleRequirementsController(ICampaignRoleRequirementService requirementService)
        {
            _requirementService = requirementService;
        }

        // ── POST /api/campaigns/{campaignId}/role-requirements ────────────────
        /// <summary>
        /// Upsert a role requirement for a campaign.
        /// Creates a new row or updates RequiredCount if (CampaignId, Role) already exists.
        /// </summary>
        [HttpPost("{campaignId:int}/role-requirements")]
        [AuthorizePermission("DoanKham.Edit")]
        public async Task<IActionResult> SetRequirement(
            int campaignId,
            [FromBody] SetRoleRequirementDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (requirement, error) = await _requirementService.SetRequirementAsync(campaignId, dto);

            if (error != null)
                return BadRequest(new { message = error });

            return Ok(requirement);
        }

        // ── GET /api/campaigns/{campaignId}/role-requirements ─────────────────
        /// <summary>
        /// Get all role requirements for a campaign with live assigned counts.
        /// Returns [] when no requirements are defined (never 404).
        /// Response includes assignedCount, isUnderAssigned, isOverAssigned per role.
        /// </summary>
        [HttpGet("{campaignId:int}/role-requirements")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> GetRequirements(int campaignId)
        {
            var requirements = await _requirementService.GetRequirementsWithCountsAsync(campaignId);
            return Ok(requirements);
        }
    }
}
