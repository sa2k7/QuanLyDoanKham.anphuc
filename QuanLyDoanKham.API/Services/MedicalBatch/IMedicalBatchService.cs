using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuanLyDoanKham.API.DTOs;

namespace QuanLyDoanKham.API.Services.MedicalBatch
{
    /// <summary>
    /// Service for creating and querying MedicalBatch entities.
    /// Supports both bulk-generation and lazy-creation modes.
    /// </summary>
    public interface IMedicalBatchService
    {
        /// <summary>
        /// Create a new MedicalBatch.
        /// If dto.AutoGenerate is true, atomically generates dto.EstimatedCount
        /// MedicalBatchRecords (BN0001..BN{N:D4}) in a single transaction.
        /// Returns (null, errorMessage) on validation failure or transaction error.
        /// </summary>
        Task<(MedicalBatchDto? Batch, string? ErrorMessage)> CreateBatchAsync(
            CreateMedicalBatchDto dto,
            string createdBy);

        /// <summary>
        /// Get a single batch by ID, including its current RecordCount.
        /// Returns null if not found.
        /// </summary>
        Task<MedicalBatchDto?> GetBatchByIdAsync(Guid id);

        /// <summary>
        /// List all batches for a given contract.
        /// Returns an empty list (never null) when no batches exist.
        /// </summary>
        Task<IEnumerable<MedicalBatchDto>> GetBatchesByContractAsync(int contractId);
    }
}
