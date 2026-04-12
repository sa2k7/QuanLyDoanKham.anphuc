using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    public class RecordStationTask
    {
        [Key]
        public int TaskId { get; set; }

        public int MedicalRecordId { get; set; }
        [ForeignKey("MedicalRecordId")]
        public MedicalRecord MedicalRecord { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string StationCode { get; set; } = null!;
        [ForeignKey("StationCode")]
        public Station Station { get; set; } = null!;

        public int QueueNo { get; set; }

        /// <summary>
        /// PENDING, CALLED, IN_PROGRESS, DONE, SKIPPED
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "PENDING";

        public int? AssignedStaffId { get; set; }
        [ForeignKey("AssignedStaffId")]
        public Staff? AssignedStaff { get; set; }

        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public string? Notes { get; set; }

        public DateTime WaitingSince { get; set; } = DateTime.Now;

        public ICollection<StationTaskEvent> Events { get; set; } = new List<StationTaskEvent>();
    }
}
