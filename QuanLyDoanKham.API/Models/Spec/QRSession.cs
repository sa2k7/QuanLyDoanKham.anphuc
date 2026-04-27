using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models.Spec
{
    /// <summary>Phiên QR check-in/check-out của đoàn khám</summary>
    public class QRSession
    {
        [Key]
        public int Id { get; set; }

        public int TeamId { get; set; }
        [ForeignKey("TeamId")]
        public ExaminationTeam Team { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string QRCode { get; set; } = null!;

        /// <summary>CheckIn | CheckOut</summary>
        [Required]
        [MaxLength(20)]
        public string Type { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
