using System.Threading.Tasks;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Services.Settlement
{
    public interface ISettlementService
    {
        /// <summary>
        /// Tính toán quyết toán thực tế cho một hợp đồng
        /// </summary>
        Task<SettlementResultDto?> CalculateSettlementAsync(int contractId);
    }

    public class SettlementResultDto
    {
        public int ContractId { get; set; }
        public string ContractName { get; set; } = string.Empty;
        public decimal ContractValue { get; set; } // Giá trị dự kiến ban đầu
        public decimal ActualValue { get; set; }   // Giá trị thực tế đã khám
        public int ExpectedQuantity { get; set; }
        public int ActualQuantity { get; set; }
        public decimal ExtraServiceValue { get; set; } // Giá trị dịch vụ ngoài gói
        public string? ExtraServiceDetails { get; set; } // Chi tiết dịch vụ phát sinh ngoài gói JSON
        public decimal VATRate { get; set; } // Tỉ lệ % VAT

        public decimal TotalSettlement { get; set; }   // Tổng quyết toán (Actual + Extra) + VAT? or VAT covers some part? Let's assume VAT is separate or just output it.
        
        // Chi tiết breakdown chi phí
        public decimal LaborCost { get; set; }      // Lương nhân viên y tế
        public decimal SupplyCost { get; set; }     // Vật tư tiêu hao
        public decimal OverheadCost { get; set; }   // Chi phí chung khác
        public decimal TotalCost { get; set; }      // Tổng chi phí (Labor + Supply + Overhead)

        // Phân tích tài chính
        public decimal Revenue { get; set; }        // Doanh thu = TotalSettlement
        public decimal GrossProfit { get; set; }    // Lợi nhuận gộp = Revenue - TotalCost
        public decimal GrossMarginPercent { get; set; } // Biên lợi nhuận (%)
        public bool IsMarginWarning { get; set; }   // Cảnh báo nếu tỷ suất lợi nhuận < 20%
    }
}
