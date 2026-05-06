using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>
    /// Bridge table: assigns a Patient (Bệnh Án) to a specific MedicalBatch (ngày khám).
    ///
    /// Architecture:
    ///   Patient (1) ──── (N) BatchMedicalRecord (N) ──── (1) MedicalBatch
    ///
    /// Rules:
    ///   - A Patient belongs to exactly ONE Contract (via Patient.HealthContractId)
    ///   - A Patient can appear in MULTIPLE batches (via this bridge)
    ///   - Duplicate assignment in the same batch is prevented by UQ_BatchMedicalRecord_BatchId_PatientId
    ///   - EstimatedCount on MedicalBatch is NOT a hard constraint
    /// </summary>
    public class BatchMedicalRecord
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // ── FK → MedicalBatch ─────────────────────────────────────────────────
        public Guid MedicalBatchId { get; set; }

        [ForeignKey("MedicalBatchId")]
        public MedicalBatch? MedicalBatch { get; set; }

        // ── FK → Patient (Bệnh Án) ────────────────────────────────────────────
        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }

        // ── Status ────────────────────────────────────────────────────────────
        /// <summary>Waiting | InProgress | Done</summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Waiting";

        // ── Audit ─────────────────────────────────────────────────────────────
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? AssignedByUserId { get; set; }

        [MaxLength(200)]
        public string? Note { get; set; }
    }
}
