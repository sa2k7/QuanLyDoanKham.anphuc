using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>
    /// Bản ghi khám nhẹ — dùng để đếm và theo dõi tiến độ.
    /// KHÔNG chứa thông tin cá nhân (không có tên, SĐT, địa chỉ).
    /// Tên class là MedicalBatchRecord (KHÔNG phải MedicalRecord) để tránh
    /// xung đột với entity lâm sàng MedicalRecord đã tồn tại.
    /// </summary>
    public class MedicalBatchRecord
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>FK → MedicalBatches</summary>
        public Guid BatchId { get; set; }

        [ForeignKey("BatchId")]
        public MedicalBatch? Batch { get; set; }

        /// <summary>Mã tự động: BN0001 → BNXXXX (duy nhất trong phạm vi Batch)</summary>
        [Required]
        [MaxLength(10)]
        public string Code { get; set; } = string.Empty;

        /// <summary>Trạng thái: Pending (chưa khám) | Done (đã khám)</summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
