using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>
    /// Lô khám sức khỏe — model nhẹ gắn với hợp đồng, chứa số lượng ước tính.
    /// Thay thế luồng Patient trong các chiến dịch khám doanh nghiệp mới.
    /// KHÔNG xóa bảng Patient — bảng này dùng cho luồng mới.
    /// </summary>
    public class MedicalBatch
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>FK → Contracts (HealthContract.HealthContractId)</summary>
        public int ContractId { get; set; }

        [ForeignKey("ContractId")]
        public HealthContract? Contract { get; set; }

        /// <summary>Số lượng bản ghi ước tính (1–10000)</summary>
        [Range(1, 10000, ErrorMessage = "EstimatedCount phải từ 1 đến 10,000")]
        public int EstimatedCount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        public ICollection<MedicalBatchRecord> Records { get; set; } = new List<MedicalBatchRecord>();
    }
}
