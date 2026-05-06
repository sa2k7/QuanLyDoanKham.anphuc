using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Services.MedicalBatch
{
    public class CampaignRoleRequirementService : ICampaignRoleRequirementService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CampaignRoleRequirementService> _logger;

        public CampaignRoleRequirementService(
            ApplicationDbContext context,
            ILogger<CampaignRoleRequirementService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ─── SetRequirementAsync (upsert) ─────────────────────────────────────

        public async Task<(RoleRequirementWithCountDto? Requirement, string? ErrorMessage)> SetRequirementAsync(
            int campaignId,
            SetRoleRequirementDto dto)
        {
            if (dto.RequiredCount < 1)
                return (null, "RequiredCount phải lớn hơn hoặc bằng 1");

            // Verify campaign exists
            var campaignExists = await _context.MedicalGroups
                .AnyAsync(g => g.GroupId == campaignId);

            if (!campaignExists)
                return (null, $"Không tìm thấy chiến dịch với Id = {campaignId}");

            // Upsert: find existing (CampaignId, Role) row or create new
            var existing = await _context.CampaignRoleRequirements
                .FirstOrDefaultAsync(r =>
                    r.CampaignId == campaignId &&
                    r.Role == dto.Role);

            if (existing != null)
            {
                existing.RequiredCount = dto.RequiredCount;
                _logger.LogInformation(
                    "CampaignRoleRequirement updated: campaign {CampaignId}, role {Role}, count {Count}",
                    campaignId, dto.Role, dto.RequiredCount);
            }
            else
            {
                existing = new CampaignRoleRequirement
                {
                    Id = System.Guid.NewGuid(),
                    CampaignId = campaignId,
                    Role = dto.Role,
                    RequiredCount = dto.RequiredCount
                };
                _context.CampaignRoleRequirements.Add(existing);
                _logger.LogInformation(
                    "CampaignRoleRequirement created: campaign {CampaignId}, role {Role}, count {Count}",
                    campaignId, dto.Role, dto.RequiredCount);
            }

            await _context.SaveChangesAsync();

            // Return with live assigned count
            var assignedCount = await GetAssignedCountAsync(campaignId, dto.Role);

            return (new RoleRequirementWithCountDto
            {
                Id = existing.Id,
                CampaignId = existing.CampaignId,
                Role = existing.Role,
                RequiredCount = existing.RequiredCount,
                AssignedCount = assignedCount
            }, null);
        }

        // ─── GetRequirementsWithCountsAsync ───────────────────────────────────

        public async Task<IEnumerable<RoleRequirementWithCountDto>> GetRequirementsWithCountsAsync(
            int campaignId)
        {
            var requirements = await _context.CampaignRoleRequirements
                .AsNoTracking()
                .Where(r => r.CampaignId == campaignId)
                .OrderBy(r => r.Role)
                .ToListAsync();

            if (requirements.Count == 0)
                return Enumerable.Empty<RoleRequirementWithCountDto>();

            // Compute assigned counts for all roles in one query.
            // GroupStaffDetails.WorkPosition stores the role string (denormalized).
            // We match on WorkPosition to count staff per role per campaign.
            var roles = requirements.Select(r => r.Role).ToList();

            var assignedCounts = await _context.GroupStaffDetails
                .AsNoTracking()
                .Where(g =>
                    g.GroupId == campaignId &&
                    roles.Contains(g.WorkPosition) &&
                    g.WorkStatus != "Absent")          // exclude absent staff from count
                .GroupBy(g => g.WorkPosition)
                .Select(grp => new { Role = grp.Key, Count = grp.Count() })
                .ToDictionaryAsync(x => x.Role, x => x.Count);

            return requirements.Select(r => new RoleRequirementWithCountDto
            {
                Id = r.Id,
                CampaignId = r.CampaignId,
                Role = r.Role,
                RequiredCount = r.RequiredCount,
                AssignedCount = assignedCounts.GetValueOrDefault(r.Role, 0)
            });
        }

        // ─── Private helpers ──────────────────────────────────────────────────

        private async Task<int> GetAssignedCountAsync(int campaignId, string role)
        {
            return await _context.GroupStaffDetails
                .AsNoTracking()
                .CountAsync(g =>
                    g.GroupId == campaignId &&
                    g.WorkPosition == role &&
                    g.WorkStatus != "Absent");
        }
    }
}
