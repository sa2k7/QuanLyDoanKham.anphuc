using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>File đính kèm hợp đồng</summary>
    public class ContractAttachment
    {
        [Key]
        public int Id { get; set; }

        public int HealthContractId { get; set; }
        [ForeignKey("HealthContractId")]
        public HealthContract HealthContract { get; set; }

        [MaxLength(200)]
        public string FileName { get; set; }

        public string FilePath { get; set; }

        [MaxLength(50)]
        public string FileType { get; set; } // PDF, DOCX, IMG

        public DateTime UploadedAt { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string UploadedBy { get; set; }
    }
}
