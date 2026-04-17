using System.Collections.Generic;

namespace QuanLyDoanKham.API.DTOs
{
    // ──────────────────────────────────────────────────────────────────
    // Phase 5: Group Analytics Report DTOs
    // ──────────────────────────────────────────────────────────────────

    /// <summary>Tổng quan một đoàn khám</summary>
    public class GroupAnalyticsDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = "";
        public string CompanyName { get; set; } = "";
        public string ExamDate { get; set; } = "";

        // Thống kê bệnh nhân
        public int TotalPatients { get; set; }
        public int CheckedIn { get; set; }
        public int Completed { get; set; }
        public int QcPending { get; set; }
        public int InProgress { get; set; }
        public int NoShow { get; set; }
        public int Cancelled { get; set; }

        // Tỷ lệ
        public double CompletionRate { get; set; }    // % COMPLETED / TotalPatients
        public double NoShowRate { get; set; }        // % NO_SHOW / TotalPatients
        public double QcPassRate { get; set; }        // % QC không phải rework

        // Thống kê theo trạm
        public List<StationThroughputDto> StationStats { get; set; } = new();

        // Top 5 chẩn đoán phổ biến nhất
        public List<DiagnosisFrequencyDto> TopDiagnoses { get; set; } = new();

        // Thời gian trung bình (phút) từ check-in đến hoàn thành
        public double? AvgCompletionMinutes { get; set; }
    }

    public class StationThroughputDto
    {
        public string StationCode { get; set; } = "";
        public string StationName { get; set; } = "";
        public int TotalAssigned { get; set; }
        public int Completed { get; set; }
        public int Skipped { get; set; }
        public double? AvgDurationMinutes { get; set; }
    }

    public class DiagnosisFrequencyDto
    {
        public string Diagnosis { get; set; } = "";
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    /// <summary>Tóm tắt nhiều đoàn — dùng cho danh sách</summary>
    public class GroupSummaryReportDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = "";
        public string CompanyName { get; set; } = "";
        public string ExamDate { get; set; } = "";
        public int TotalPatients { get; set; }
        public int Completed { get; set; }
        public double CompletionRate { get; set; }
        public string Status { get; set; } = "";
    }
}
