using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models.Spec
{
    /// <summary>Hồ sơ khám của bệnh nhân tại từng vị trí khám</summary>
    public class PatientExamRecord
    {
        [Key]
        public int Id { get; set; }

        public int PatientId { get; set; }
        [ForeignKey("PatientId")]
        public SpecPatient Patient { get; set; } = null!;

        public int ExamPositionId { get; set; }
        [ForeignKey("ExamPositionId")]
        public ExamPosition ExamPosition { get; set; } = null!;

        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedAt { get; set; }
    }
}
