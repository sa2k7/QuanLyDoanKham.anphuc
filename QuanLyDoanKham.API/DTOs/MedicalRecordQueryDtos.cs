using System;
using System.Collections.Generic;

namespace QuanLyDoanKham.API.DTOs
{
    public class MedicalRecordGroupItemDto
    {
        public int MedicalRecordId { get; set; }
        public string? FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? IDCardNumber { get; set; }
        public string? Department { get; set; }
        public string? Status { get; set; }
        public DateTime? CheckInAt { get; set; }
        public int? QueueNo { get; set; }
    }

    public class StationQueueItemDto
    {
        public int TaskId { get; set; }
        public int MedicalRecordId { get; set; }
        public string? FullName { get; set; }
        public string? Gender { get; set; }
        public int? QueueNo { get; set; }
        public string? Status { get; set; }
        public DateTime? WaitingSince { get; set; }
        public DateTime? StartedAt { get; set; }
    }

    public class StationQueueSummaryDto
    {
        public string? StationCode { get; set; }
        public int WaitingCount { get; set; }
        public int InProgressCount { get; set; }
        public int DoneToday { get; set; }
    }

    public class GroupQueueOverviewDto
    {
        public string? StationCode { get; set; }
        public string? StationName { get; set; }
        public int SortOrder { get; set; }
        public int WaitingCount { get; set; }
        public int InProgressCount { get; set; }
    }

    public class QcPendingRecordDto
    {
        public int MedicalRecordId { get; set; }
        public string? FullName { get; set; }
        public int? QueueNo { get; set; }
        public string? Status { get; set; }
        public DateTime? CheckInAt { get; set; }
        public List<QcStationTaskStatusDto> StationTasks { get; set; } = new();
    }

    public class QcStationTaskStatusDto
    {
        public string? StationCode { get; set; }
        public string? Status { get; set; }
    }
}
