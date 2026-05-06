using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;

namespace QuanLyDoanKham.API.Services.MedicalBatch
{
    public class WarehouseCampaignService : IWarehouseCampaignService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WarehouseCampaignService> _logger;

        // Only these statuses are relevant for warehouse export operations
        private static readonly HashSet<string> WarehouseStatuses = new(System.StringComparer.OrdinalIgnoreCase)
        {
            "Active",
            "Completed",
            "Finished",   // legacy status name used in existing data
            "Locked"      // locked groups still need supply export
        };

        public WarehouseCampaignService(
            ApplicationDbContext context,
            ILogger<WarehouseCampaignService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<WarehouseCampaignDto>> GetWarehouseCampaignsAsync()
        {
            var campaigns = await _context.MedicalGroups
                .AsNoTracking()
                .Include(g => g.HealthContract)
                    .ThenInclude(c => c!.Company)
                .Where(g => WarehouseStatuses.Contains(g.Status))
                .OrderByDescending(g => g.ExamDate)
                .Select(g => new WarehouseCampaignDto
                {
                    GroupId = g.GroupId,
                    GroupName = g.GroupName,
                    ExamDate = g.ExamDate,
                    Status = g.Status,
                    ContractCode = g.HealthContract != null ? g.HealthContract.ContractCode : null,
                    CompanyName = g.HealthContract != null && g.HealthContract.Company != null
                        ? g.HealthContract.Company.CompanyName
                        : null
                })
                .ToListAsync();

            // Return empty list (never null) — satisfies Property 23
            return campaigns ?? Enumerable.Empty<WarehouseCampaignDto>();
        }
    }
}
