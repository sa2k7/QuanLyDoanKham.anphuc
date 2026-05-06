using System;
using System.Threading.Tasks;
using QuanLyDoanKham.API.DTOs;

namespace QuanLyDoanKham.API.Services.MedicalBatch
{
    /// <summary>
    /// Service for lazy creation and status management of MedicalBatchRecord entities.
    /// Code generation (BN0001..BNXXXX) is handled here using a serializable transaction
    /// to prevent duplicate codes under concurrent requests.
    /// </summary>
    public interface IMedicalBatchRecordService
    {
        /// <summary>
        /// Lazily create a single MedicalBatchRecord for the given batch.
        /// Auto-generates the next sequential Code within the batch scope.
        /// Returns (null, errorMessage) if the batch is at capacity (count >= EstimatedCount).
        /// </summary>
        Task<(MedicalBatchRecordDto? Record, string? ErrorMessage)> CreateRecordAsync(Guid batchId);

        /// <summary>
        /// Update a record's status to "Done".
        /// Returns (false, errorMessage) with HTTP 409 signal if already Done (idempotency guard).
        /// </summary>
        Task<(bool Success, string? ErrorMessage)> UpdateStatusToDoneAsync(Guid recordId);

        /// <summary>
        /// Paginated list of records for a batch.
        /// Returns an empty PagedResult (never null) when no records exist.
        /// </summary>
        Task<PagedResult<MedicalBatchRecordDto>> GetRecordsByBatchAsync(
            Guid batchId,
            int page,
            int pageSize);
    }
}
