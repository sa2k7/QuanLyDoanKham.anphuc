using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Models.Enums;

namespace QuanLyDoanKham.API.Services.MedicalBatch
{
    public class MedicalBatchService : IMedicalBatchService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MedicalBatchService> _logger;

        // Allowed contract statuses for batch creation (Approved or Active)
        private static readonly HashSet<ContractStatus> AllowedContractStatuses = new()
        {
            ContractStatus.Approved,
            ContractStatus.Active
        };

        public MedicalBatchService(
            ApplicationDbContext context,
            ILogger<MedicalBatchService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ─── CreateBatchAsync ─────────────────────────────────────────────────

        public async Task<(MedicalBatchDto? Batch, string? ErrorMessage)> CreateBatchAsync(
            CreateMedicalBatchDto dto,
            string createdBy)
        {
            // Validate EstimatedCount bounds (defence-in-depth — annotations already checked at controller)
            if (dto.EstimatedCount <= 0)
                return (null, "EstimatedCount phải lớn hơn 0");

            if (dto.EstimatedCount > 10_000)
                return (null, "EstimatedCount vượt quá giới hạn cho phép (10,000)");

            // Validate contract exists and has an allowed status
            var contract = await _context.Contracts
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.HealthContractId == dto.ContractId);

            if (contract == null)
                return (null, $"Không tìm thấy hợp đồng với Id = {dto.ContractId}");

            if (!AllowedContractStatuses.Contains(contract.Status))
                return (null, "Hợp đồng phải ở trạng thái Approved hoặc Active");

            // Build the batch entity
            var batch = new Models.MedicalBatch
            {
                Id = Guid.NewGuid(),
                ContractId = dto.ContractId,
                EstimatedCount = dto.EstimatedCount,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            if (!dto.AutoGenerate)
            {
                // Lazy mode — create batch only, no records
                _context.MedicalBatches.Add(batch);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "MedicalBatch {BatchId} created (lazy mode) for contract {ContractId} by {User}",
                    batch.Id, dto.ContractId, createdBy);

                return (MapToDto(batch, recordCount: 0, recordsGenerated: null), null);
            }

            // Bulk mode — create batch + N records atomically
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.MedicalBatches.Add(batch);
                await _context.SaveChangesAsync(); // flush batch first so FK is satisfied

                var records = BuildBulkRecords(batch.Id, dto.EstimatedCount);
                await _context.MedicalBatchRecords.AddRangeAsync(records);
                await _context.SaveChangesAsync();

                await tx.CommitAsync();

                _logger.LogInformation(
                    "MedicalBatch {BatchId} created (bulk mode, {Count} records) for contract {ContractId} by {User}",
                    batch.Id, dto.EstimatedCount, dto.ContractId, createdBy);

                return (MapToDto(batch, recordCount: dto.EstimatedCount, recordsGenerated: dto.EstimatedCount), null);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex,
                    "Bulk generation failed for MedicalBatch (ContractId={ContractId}, Count={Count})",
                    dto.ContractId, dto.EstimatedCount);
                return (null, "Tạo lô khám thất bại. Vui lòng thử lại.");
            }
        }

        // ─── GetBatchByIdAsync ────────────────────────────────────────────────

        public async Task<MedicalBatchDto?> GetBatchByIdAsync(Guid id)
        {
            var batch = await _context.MedicalBatches
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id);

            if (batch == null) return null;

            var recordCount = await _context.MedicalBatchRecords
                .CountAsync(r => r.BatchId == id);

            return MapToDto(batch, recordCount, recordsGenerated: null);
        }

        // ─── GetBatchesByContractAsync ────────────────────────────────────────

        public async Task<IEnumerable<MedicalBatchDto>> GetBatchesByContractAsync(int contractId)
        {
            var batches = await _context.MedicalBatches
                .AsNoTracking()
                .Where(b => b.ContractId == contractId)
                .OrderBy(b => b.CreatedAt)
                .ToListAsync();

            if (batches.Count == 0)
                return Enumerable.Empty<MedicalBatchDto>();

            // Fetch record counts in one query to avoid N+1
            var batchIds = batches.Select(b => b.Id).ToList();
            var counts = await _context.MedicalBatchRecords
                .Where(r => batchIds.Contains(r.BatchId))
                .GroupBy(r => r.BatchId)
                .Select(g => new { BatchId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.BatchId, x => x.Count);

            return batches.Select(b =>
                MapToDto(b, counts.GetValueOrDefault(b.Id, 0), recordsGenerated: null));
        }

        // ─── Private helpers ──────────────────────────────────────────────────

        /// <summary>
        /// Pre-compute all N records with sequential codes BN0001..BN{N:D4}.
        /// All codes are generated in memory before any DB insert — avoids N round-trips.
        /// </summary>
        private static List<Models.MedicalBatchRecord> BuildBulkRecords(Guid batchId, int count)
        {
            var records = new List<Models.MedicalBatchRecord>(count);
            for (int i = 1; i <= count; i++)
            {
                records.Add(new Models.MedicalBatchRecord
                {
                    Id = Guid.NewGuid(),
                    BatchId = batchId,
                    Code = $"BN{i:D4}",
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                });
            }
            return records;
        }

        private static MedicalBatchDto MapToDto(
            Models.MedicalBatch batch,
            int recordCount,
            int? recordsGenerated)
        {
            return new MedicalBatchDto
            {
                Id = batch.Id,
                ContractId = batch.ContractId,
                EstimatedCount = batch.EstimatedCount,
                RecordCount = recordCount,
                RecordsGenerated = recordsGenerated,
                CreatedAt = batch.CreatedAt,
                CreatedBy = batch.CreatedBy
            };
        }
    }
}
