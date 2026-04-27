namespace QuanLyDoanKham.API.DTOs.Reports
{
    /// <summary>P&amp;L chi tiết theo từng đoàn khám</summary>
    public class GroupPnlDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = "";
        public string CompanyName { get; set; } = "";
        public string ExamDate { get; set; } = "";
        public int PatientCount { get; set; }

        // Doanh thu
        public decimal ContractValue { get; set; }

        // Chi phí
        public decimal LaborCost { get; set; }
        public decimal MaterialCost { get; set; }
        public decimal OtherCost { get; set; }
        public decimal TotalCost { get; set; }

        // Lợi nhuận
        public decimal Profit { get; set; }
        public double ProfitMargin { get; set; }   // %

        // Chi tiết
        public List<PnlStaffLineDto> StaffLines { get; set; } = new();
        public List<PnlMaterialLineDto> MaterialLines { get; set; } = new();
    }

    public class PnlStaffLineDto
    {
        public string StaffName { get; set; } = "";
        public double ShiftType { get; set; }      // 0.5 hoặc 1.0
        public decimal DailyRate { get; set; }
        public decimal Salary { get; set; }
    }

    public class PnlMaterialLineDto
    {
        public string ItemName { get; set; } = "";
        public string Unit { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalValue { get; set; }
    }

    /// <summary>Tổng hợp P&amp;L theo kỳ (from → to)</summary>
    public class PeriodSummaryDto
    {
        public string From { get; set; } = "";
        public string To { get; set; } = "";
        public int TotalGroups { get; set; }
        public int TotalPatients { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalLaborCost { get; set; }
        public decimal TotalMaterialCost { get; set; }
        public decimal TotalOtherCost { get; set; }
        public decimal TotalCost { get; set; }
        public decimal Profit { get; set; }
        public double ProfitMargin { get; set; }   // %
    }
}
