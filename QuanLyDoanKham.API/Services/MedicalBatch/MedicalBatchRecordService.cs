using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;

namespace QuanLyDoanKham.API.Services.MedicalBatch
{
    public class MedicalBatchRecordService : IMedicalBatchRecordService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MedicalBatchRecordService> _logger;

        public MedicalBatchRecordService(
            ApplicationDbContext context,
            ILogger<MedicalBatchRecordService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ─── CreateRecordAsync (lazy, one at a time) ──────────────────────────

        public async Task<(MedicalBatchRecordDto? Record, string? ErrorMessage)> CreateRecordAsync(Guid batchId)
        {
            // Use a serializable transaction to prevent duplicate codes under concurrent requests.
            // The unique index (BatchId, Code) on MedicalBatchRecords is the final safety net,
            // but the serializable isolation prevents the phantom-read race condition.
            using var tx = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable);
            try
            {
                var batch = await _context.MedicalBatches
                    .FirstOrDefaultAsync(b => b.Id == batchId);

                if (batch == null)
                    return (null, $"Không tìm thấy lô khám với Id = {batchId}");

                var currentCount = await _context.MedicalBatchRecords
                    .CountAsync(r => r.BatchId == batchId);

                if (currentCount >= batch.EstimatedCount)
                    return (null, "Số lượng bản ghi đã đạt giới hạn EstimatedCount");

                var nextSeq = currentCount + 1;
                var code = $"BN{nextSeq:D4}";

                var record = new Models.MedicalBatchRecord
                {
                    Id = Guid.NewGuid(),
                    BatchId = batchId,
                    Code = code,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                _context.MedicalBatchRecords.Add(record);
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                _logger.LogInformation(
                    "MedicalBatchRecord {Code} created in batch {BatchId}",
                    code, batchId);

                return (MapToDto(record), null);
            }
            catch (DbUpdateException ex) when (IsDuplicateKeyException(ex))
            {
                // Unique index violation — concurrent request won the race; caller should retry
                await tx.RollbackAsync();
                _logger.LogWarning(
                    "Duplicate code conflict in batch {BatchId} — concurrent insert detected",
                    batchId);
                return (null, "Xung đột mã bản ghi. Vui lòng thử lại.");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Failed to create MedicalBatchRecord in batch {BatchId}", batchId);
                return (null, "Tạo bản ghi thất bại. Vui lòng thử lại.");
            }
        }

        // ─── UpdateStatusToDoneAsync ──────────────────────────────────────────

        public async Task<(bool Success, string? ErrorMessage)> UpdateStatusToDoneAsync(Guid recordId)
        {
            var record = await _context.MedicalBatchRecords
                .FirstOrDefaultAsync(r => r.Id == recordId);

            if (record == null)
                return (false, $"Không tìm thấy bản ghi với Id = {recordId}");

            // Idempotency guard — HTTP 409 signal via error message
            if (record.Status == "Done")
                return (false, "Bản ghi đã ở trạng thái Done");

            record.Status = "Done";
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "MedicalBatchRecord {RecordId} ({Code}) marked Done",
                recordId, record.Code);

            return (true, null);
        }

        // ─── GetRecordsByBatchAsync ───────────────────────────────────────────

        public async Task<PagedResult<MedicalBatchRecordDto>> GetRecordsByBatchAsync(
            Guid batchId,
            int page,
            int pageSize)
        {
            // Clamp to safe values
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var query = _context.MedicalBatchRecords
                .AsNoTracking()
                .Where(r => r.BatchId == batchId)
                .OrderBy(r => r.Code);

            var totalCount = await query.CountAsync();

            var items = totalCount == 0
                ? new List<MedicalBatchRecordDto>()
                : await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new MedicalBatchRecordDto
                    {
                        Id = r.Id,
                        BatchId = r.BatchId,
                        Code = r.Code,
                        Status = r.Status,
                        CreatedAt = r.CreatedAt
                    })
                    .ToListAsync();

            return new PagedResult<MedicalBatchRecordDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // ─── Private helpers ──────────────────────────────────────────────────

        private static MedicalBatchRecordDto MapToDto(Models.MedicalBatchRecord r) =>
            new()
            {
                Id = r.Id,
                BatchId = r.BatchId,
                Code = r.Code,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            };

        /// <summary>
        /// Detects SQL Server unique constraint violation (error 2601 or 2627).
        /// Used to handle the rare concurrent-insert race that slips past the
        /// serializable transaction on some edge cases.
        /// </summary>
        private static bool IsDuplicateKeyException(DbUpdateException ex)
        {
            return ex.InnerException is SqlException sqlEx
                && (sqlEx.Number == 2601 || sqlEx.Number == 2627);
        }
    }
}
