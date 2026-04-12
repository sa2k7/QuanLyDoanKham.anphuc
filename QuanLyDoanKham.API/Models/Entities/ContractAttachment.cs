using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    /// <summary>File Ä‘Ă­nh kĂ¨m há»£p Ä‘á»“ng</summary>
    public class ContractAttachment
    {
        [Key]
        public int Id { get; set; }

        public int HealthContractId { get; set; }
        [ForeignKey("HealthContractId")]
        public HealthContract HealthContract { get; set; } = null!;

        [MaxLength(200)]
        public string FileName { get; set; } = null!;

        public string FilePath { get; set; } = null!;

        [MaxLength(50)]
        public string FileType { get; set; } = null!; // PDF, DOCX, IMG

        public DateTime UploadedAt { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string UploadedBy { get; set; } = null!;
    }
}
