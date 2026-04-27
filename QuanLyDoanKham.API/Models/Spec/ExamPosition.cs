using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models.Spec
{
    /// <summary>Vị trí khám trong đoàn (Tiếp nhận, Khám nội, Siêu âm, ...)</summary>
    public class ExamPosition
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = null!;

        public bool IsDefault { get; set; } = false;

        public int TeamId { get; set; }
        [ForeignKey("TeamId")]
        public ExaminationTeam Team { get; set; } = null!;

        public ICollection<TeamStaff> TeamStaffs { get; set; } = new List<TeamStaff>();
        public ICollection<PatientExamRecord> PatientExamRecords { get; set; } = new List<PatientExamRecord>();
    }
}
