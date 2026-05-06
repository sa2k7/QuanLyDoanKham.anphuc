using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDoanKham.API.Authorization;
using QuanLyDoanKham.API.Services.MedicalBatch;

namespace QuanLyDoanKham.API.Controllers
{
    /// <summary>
    /// Provides a warehouse-scoped view of campaigns.
    /// Fixes the 403 issue for WarehouseManager users who have Kho.View
    /// but NOT DoanKham.View — they cannot call GET /api/MedicalGroups.
    ///
    /// NEW controller — the existing MedicalGroupsController is NOT modified.
    /// </summary>
    [Route("api/warehouse")]
    [ApiController]
    [Authorize]
    public class WarehouseCampaignsController : ControllerBase
    {
        private readonly IWarehouseCampaignService _warehouseService;

        public WarehouseCampaignsController(IWarehouseCampaignService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        // ── GET /api/warehouse/campaigns ──────────────────────────────────────
        /// <summary>
        /// Returns campaigns relevant to warehouse operations (Active, Completed, Finished, Locked).
        /// Requires Kho.View permission — accessible to WarehouseManager without DoanKham.View.
        /// Returns [] when no matching campaigns exist (never 404).
        /// </summary>
        [HttpGet("campaigns")]
        [AuthorizePermission("Kho.View")]
        public async Task<IActionResult> GetWarehouseCampaigns()
        {
            var campaigns = await _warehouseService.GetWarehouseCampaignsAsync();
            return Ok(campaigns);
        }
    }
}
