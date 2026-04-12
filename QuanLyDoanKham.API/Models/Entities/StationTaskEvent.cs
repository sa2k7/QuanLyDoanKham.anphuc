using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDoanKham.API.Models
{
    public class StationTaskEvent
    {
        [Key]
        public long EventId { get; set; }

        public int TaskId { get; set; }
        [ForeignKey("TaskId")]
        public RecordStationTask Task { get; set; } = null!;

        /// <summary>
        /// QUEUED, CALLED, STARTED, COMPLETED, REROUTED, SKIPPED
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string EventType { get; set; } = null!;

        public int? ActorUserId { get; set; }
        [ForeignKey("ActorUserId")]
        public AppUser? ActorUser { get; set; }

        public string? Payload { get; set; } // JSON context

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
