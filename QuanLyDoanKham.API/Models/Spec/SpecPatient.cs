using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models.Spec
{
    /// <summary>Bệnh nhân trong đoàn khám (spec-aligned)</summary>
    public class SpecPatient
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;

        public DateOnly DateOfBirth { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }

        [MaxLength(200)]
        public string? WorkUnit { get; set; }

        public int TeamId { get; set; }
        [ForeignKey("TeamId")]
        public ExaminationTeam Team { get; set; } = null!;

        /// <summary>Chưa khám | Đang khám | Đã khám xong</summary>
        [MaxLength(50)]
        public string Status { get; set; } = "Chưa khám";

        public ICollection<PatientExamRecord> ExamRecords { get; set; } = new List<PatientExamRecord>();
    }
}
