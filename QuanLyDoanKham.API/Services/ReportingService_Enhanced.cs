using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Services
{
    /// <summary>
    /// Enhanced Reporting Service với logic tính toán chính xác hơn cho Quyết toán & Thống kê
    /// Cải thiện: Tính toán doanh thu phát sinh chính xác, chi phí vật tư theo định mức, lợi nhuận ròng thực tế
    /// </summary>
    public interface IReportingServiceEnhanced
    {
        Task<SettlementDetailDto> GetSettlementDetailAsync(int healthContractId);
        Task<MasterStatsDto> GetMasterStatsAsync(DateTime? startDate, DateTime? endDate);
        Task<List<ReconciliationItemDto>> GetReconciliationListAsync(DateTime? startDate, DateTime? endDate);
    }

    public class ReportingServiceEnhanced : IReportingServiceEnhanced
    {
        private readonly ApplicationDbContext _context;

        public ReportingServiceEnhanced(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy chi tiết quyết toán cho một hợp đồng cụ thể
        /// Bao gồm: Doanh thu gói + Phát sinh, Chi phí (Lương + Vật tư + Overhead), Lợi nhuận ròng
        /// </summary>
        public async Task<SettlementDetailDto> GetSettlementDetailAsync(int healthContractId)
        {
            var contract = await _context.Contracts
                .Include(c => c.MedicalGroups)
                .FirstOrDefaultAsync(c => c.HealthContractId == healthContractId);

            if (contract == null)
                throw new Exception("Hợp đồng không tồn tại");

            // 1. DOANH THU
            // 1.1 Doanh thu gói = Số ca COMPLETED × Đơn giá
            var packageRevenue = await _context.MedicalRecords
                .Where(r => r.Status == "COMPLETED" 
                         && r.MedicalGroup!.HealthContractId == healthContractId
                         && r.MedicalGroup != null)
                .SumAsync(r => (decimal?)r.MedicalGroup!.HealthContract!.UnitPrice) ?? 0;

            // 1.2 Doanh thu phát sinh = ExtraServiceRevenue từ Contract
            var extraRevenue = contract.ExtraServiceRevenue;

            var totalRevenue = packageRevenue + extraRevenue;

            // 2. CHI PHÍ
            // 2.1 Chi phí nhân sự = Tổng lương từ GroupStaffDetails liên quan đến hợp đồng này
            var staffCost = await _context.GroupStaffDetails
                .Where(g => g.MedicalGroup!.HealthContractId == healthContractId 
                         && g.WorkStatus == "Joined")
                .SumAsync(g => (decimal?)g.CalculatedSalary) ?? 0;

            // 2.2 Chi phí vật tư = Tính theo định mức từ từng ca khám
            // Giả định: Mỗi ca khám có định mức vật tư = 10% giá dịch vụ (có thể cấu hình)
            var completedCount = await _context.MedicalRecords
                .CountAsync(r => r.Status == "COMPLETED" 
                              && r.MedicalGroup!.HealthContractId == healthContractId);
            
            var materialCostExpected = packageRevenue * 0.10m; // 10% định mức
            
            // Tạm thời chỉ dùng định mức do chưa có tracking chi phí vật tư đích danh theo hợp đồng
            var materialCost = materialCostExpected;

            // 2.3 Chi phí chung (Overhead) - Chia tỷ lệ theo doanh thu
            var totalOverhead = await _context.Set<Overhead>()
                .SumAsync(o => (decimal?)o.Amount) ?? 0;
            
            var allContractRevenue = await _context.Contracts
                .Where(c => c.Status != "Rejected")
                .SumAsync(c => (decimal?)(c.TotalAmount + c.ExtraServiceRevenue)) ?? 0;
            
            var overheadCost = allContractRevenue > 0 
                ? (totalRevenue / allContractRevenue) * totalOverhead 
                : 0;

            var totalCost = staffCost + materialCost + overheadCost;

            // 3. LỢI NHUẬN
            var netProfit = totalRevenue - totalCost;
            var profitMargin = totalRevenue > 0 ? (netProfit / totalRevenue) * 100 : 0;

            // 4. THÔNG TIN BỔ SUNG
            var completedRecords = await _context.MedicalRecords
                .CountAsync(r => r.Status == "COMPLETED" 
                              && r.MedicalGroup!.HealthContractId == healthContractId);

            var expectedQuantity = contract.ExpectedQuantity;
            var quantityVariance = completedRecords - expectedQuantity;

            return new SettlementDetailDto
            {
                HealthContractId = healthContractId,
                ContractName = contract.ContractName,
                CompanyName = contract.Company?.CompanyName ?? "N/A",
                
                // Doanh thu
                PackageRevenue = packageRevenue,
                ExtraRevenue = extraRevenue,
                TotalRevenue = totalRevenue,
                
                // Chi phí
                StaffCost = staffCost,
                MaterialCost = materialCost,
                OverheadCost = overheadCost,
                TotalCost = totalCost,
                
                // Lợi nhuận
                NetProfit = netProfit,
                ProfitMargin = Math.Round(profitMargin, 2),
                
                // Thống kê
                CompletedRecords = completedRecords,
                ExpectedQuantity = expectedQuantity,
                QuantityVariance = quantityVariance,
                
                // Cảnh báo
                IsLowMargin = profitMargin < 20,
                IsQuantityVarianceHigh = Math.Abs(quantityVariance) > expectedQuantity * 0.1m
            };
        }

        /// <summary>
        /// Lấy thống kê tổng hợp (Master Stats) cho toàn bộ hệ thống trong kỳ
        /// </summary>
        public async Task<MasterStatsDto> GetMasterStatsAsync(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Now.AddMonths(-1);
            var end = endDate ?? DateTime.Now;

            // Tính toán tất cả các hợp đồng trong kỳ
            var contracts = await _context.Contracts
                .Where(c => c.EndDate >= start && c.EndDate <= end && c.Status != "Rejected")
                .ToListAsync();

            decimal totalPackageRevenue = 0;
            decimal totalExtraRevenue = 0;
            decimal totalStaffCost = 0;
            decimal totalMaterialCost = 0;
            decimal totalOverheadCost = 0;
            int totalCompletedRecords = 0;

            foreach (var contract in contracts)
            {
                var settlement = await GetSettlementDetailAsync(contract.HealthContractId);
                totalPackageRevenue += settlement.PackageRevenue;
                totalExtraRevenue += settlement.ExtraRevenue;
                totalStaffCost += settlement.StaffCost;
                totalMaterialCost += settlement.MaterialCost;
                totalOverheadCost += settlement.OverheadCost;
                totalCompletedRecords += settlement.CompletedRecords;
            }

            var totalRevenue = totalPackageRevenue + totalExtraRevenue;
            var totalCost = totalStaffCost + totalMaterialCost + totalOverheadCost;
            var netProfit = totalRevenue - totalCost;
            var profitMargin = totalRevenue > 0 ? (netProfit / totalRevenue) * 100 : 0;

            // Tính toán chi tiết chi phí
            var staffCostPercentage = totalRevenue > 0 ? (totalStaffCost / totalRevenue) * 100 : 0;
            var materialCostPercentage = totalRevenue > 0 ? (totalMaterialCost / totalRevenue) * 100 : 0;
            var overheadCostPercentage = totalRevenue > 0 ? (totalOverheadCost / totalRevenue) * 100 : 0;

            // Số hợp đồng
            var totalContracts = contracts.Count;
            var lowMarginContracts = contracts.Count(c => c.ExtraServiceRevenue > 0); // Giả định

            // Nhân sự
            var totalStaffDeployed = await _context.GroupStaffDetails
                .Where(g => g.ExamDate >= start && g.ExamDate <= end && g.WorkStatus == "Joined")
                .CountAsync();

            var uniqueStaff = await _context.GroupStaffDetails
                .Where(g => g.ExamDate >= start && g.ExamDate <= end && g.WorkStatus == "Joined")
                .Select(g => g.StaffId)
                .Distinct()
                .CountAsync();

            return new MasterStatsDto
            {
                // Doanh thu
                TotalPackageRevenue = totalPackageRevenue,
                TotalExtraRevenue = totalExtraRevenue,
                TotalRevenue = totalRevenue,
                
                // Chi phí chi tiết
                TotalStaffCost = totalStaffCost,
                TotalMaterialCost = totalMaterialCost,
                TotalOverheadCost = totalOverheadCost,
                TotalCost = totalCost,
                
                // Phần trăm chi phí
                StaffCostPercentage = Math.Round(staffCostPercentage, 2),
                MaterialCostPercentage = Math.Round(materialCostPercentage, 2),
                OverheadCostPercentage = Math.Round(overheadCostPercentage, 2),
                
                // Lợi nhuận
                NetProfit = netProfit,
                ProfitMargin = Math.Round(profitMargin, 2),
                
                // Thống kê
                TotalCompletedRecords = totalCompletedRecords,
                TotalContracts = totalContracts,
                LowMarginContracts = lowMarginContracts,
                TotalStaffDeployed = totalStaffDeployed,
                UniqueStaffCount = uniqueStaff,
                
                // Thời gian
                StartDate = start,
                EndDate = end
            };
        }

        /// <summary>
        /// Lấy danh sách các ca khám "lệch" (Outliers) để kế toán đối soát
        /// Bao gồm: Ca khám ngoài gói, ca khám thiếu, ca khám bị hủy
        /// </summary>
        public async Task<List<ReconciliationItemDto>> GetReconciliationListAsync(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Now.AddMonths(-1);
            var end = endDate ?? DateTime.Now;

            var reconciliationItems = new List<ReconciliationItemDto>();

            // 1. Ca khám COMPLETED nhưng ngoài gói (Quantity Variance)
            var contractsWithVariance = await _context.Contracts
                .Where(c => c.EndDate >= start && c.EndDate <= end && c.Status != "Rejected")
                .ToListAsync();

            foreach (var contract in contractsWithVariance)
            {
                var completedCount = await _context.MedicalRecords
                    .CountAsync(r => r.Status == "COMPLETED" 
                                  && r.MedicalGroup!.HealthContractId == contract.HealthContractId);

                var variance = completedCount - contract.ExpectedQuantity;

                if (variance > 0)
                {
                    reconciliationItems.Add(new ReconciliationItemDto
                    {
                        Type = "EXTRA_COMPLETED",
                        ContractId = contract.HealthContractId,
                        ContractName = contract.ContractName,
                        Description = $"Khám thêm {variance} ca ngoài gói",
                        Quantity = variance,
                        Amount = variance * contract.UnitPrice,
                        Status = "PENDING"
                    });
                }
                else if (variance < 0)
                {
                    reconciliationItems.Add(new ReconciliationItemDto
                    {
                        Type = "INCOMPLETE",
                        ContractId = contract.HealthContractId,
                        ContractName = contract.ContractName,
                        Description = $"Khám thiếu {Math.Abs(variance)} ca so với gói",
                        Quantity = Math.Abs(variance),
                        Amount = Math.Abs(variance) * contract.UnitPrice,
                        Status = "PENDING"
                    });
                }
            }

            // 2. Ca khám bị CANCELLED hoặc REJECTED (cần hoàn tiền)
            var cancelledRecords = await _context.MedicalRecords
                .Where(r => (r.Status == "CANCELLED" || r.Status == "REJECTED")
                         && r.CheckInAt >= start && r.CheckInAt <= end)
                .Include(r => r.MedicalGroup)
                .ToListAsync();

            foreach (var record in cancelledRecords)
            {
                reconciliationItems.Add(new ReconciliationItemDto
                {
                    Type = "CANCELLED",
                    ContractId = record.MedicalGroup?.HealthContractId ?? 0,
                    ContractName = record.MedicalGroup?.HealthContract?.ContractName ?? "N/A",
                    Description = $"Ca khám bị hủy - {record.FullName}",
                    Quantity = 1,
                    Amount = -(record.MedicalGroup?.HealthContract?.UnitPrice ?? 0),
                    Status = "PENDING"
                });
            }

            return reconciliationItems.OrderByDescending(x => Math.Abs(x.Amount)).ToList();
        }
    }

    // ===== DTOs =====

    public class SettlementDetailDto
    {
        public int HealthContractId { get; set; }
        public string ContractName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;

        // Doanh thu
        public decimal PackageRevenue { get; set; }
        public decimal ExtraRevenue { get; set; }
        public decimal TotalRevenue { get; set; }

        // Chi phí
        public decimal StaffCost { get; set; }
        public decimal MaterialCost { get; set; }
        public decimal OverheadCost { get; set; }
        public decimal TotalCost { get; set; }

        // Lợi nhuận
        public decimal NetProfit { get; set; }
        public decimal ProfitMargin { get; set; }

        // Thống kê
        public int CompletedRecords { get; set; }
        public int ExpectedQuantity { get; set; }
        public int QuantityVariance { get; set; }

        // Cảnh báo
        public bool IsLowMargin { get; set; }
        public bool IsQuantityVarianceHigh { get; set; }
    }

    public class MasterStatsDto
    {
        // Doanh thu
        public decimal TotalPackageRevenue { get; set; }
        public decimal TotalExtraRevenue { get; set; }
        public decimal TotalRevenue { get; set; }

        // Chi phí chi tiết
        public decimal TotalStaffCost { get; set; }
        public decimal TotalMaterialCost { get; set; }
        public decimal TotalOverheadCost { get; set; }
        public decimal TotalCost { get; set; }

        // Phần trăm chi phí
        public decimal StaffCostPercentage { get; set; }
        public decimal MaterialCostPercentage { get; set; }
        public decimal OverheadCostPercentage { get; set; }

        // Lợi nhuận
        public decimal NetProfit { get; set; }
        public decimal ProfitMargin { get; set; }

        // Thống kê
        public int TotalCompletedRecords { get; set; }
        public int TotalContracts { get; set; }
        public int LowMarginContracts { get; set; }
        public int TotalStaffDeployed { get; set; }
        public int UniqueStaffCount { get; set; }

        // Thời gian
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class ReconciliationItemDto
    {
        public string Type { get; set; } = string.Empty; // EXTRA_COMPLETED, INCOMPLETE, CANCELLED
        public int ContractId { get; set; }
        public string ContractName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "PENDING"; // PENDING, APPROVED, REJECTED
    }
}
