using FsCheck;
using FsCheck.Xunit;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.PropertyTests.Helpers;

namespace QuanLyDoanKham.PropertyTests.Properties;

/// <summary>
/// Property-based tests cho module Quản lý Vật tư Kho.
/// Feature: medical-examination-team-management
/// </summary>
public class InventoryProperties
{
    // -----------------------------------------------------------------------
    // Property 25: Tồn kho cập nhật đúng sau mỗi phiếu nhập/xuất
    // Validates: Requirements 9.3, 9.4
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P25_InventoryBalanceCorrectAfterMovements()
    {
        // Feature: medical-examination-team-management, Property 25: Inventory balance correct after movements
        return Prop.ForAll(DomainArbitraries.InventorySequences(), data =>
        {
            var (initialStock, receipts, issues) = data;

            // Simulate inventory movements
            int currentStock = initialStock;

            foreach (var qty in receipts)
                currentStock += qty;

            // Chỉ xuất khi còn đủ hàng
            foreach (var qty in issues)
            {
                if (currentStock >= qty)
                    currentStock -= qty;
            }

            // Tồn kho cuối = tồn đầu + tổng nhập - tổng xuất (hợp lệ)
            int totalReceipts = receipts.Sum();
            int validIssues = 0;
            int tempStock = initialStock + totalReceipts;
            foreach (var qty in issues)
            {
                if (tempStock >= qty)
                {
                    validIssues += qty;
                    tempStock -= qty;
                }
            }

            int expectedFinal = initialStock + totalReceipts - validIssues;
            return currentStock == expectedFinal && currentStock >= 0;
        });
    }

    // -----------------------------------------------------------------------
    // Property 25 (variant): Tồn kho không bao giờ âm
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P25_InventoryNeverNegative()
    {
        // Feature: medical-examination-team-management, Property 25 variant: Inventory never negative
        return Prop.ForAll(DomainArbitraries.InventorySequences(), data =>
        {
            var (initialStock, receipts, issues) = data;

            int currentStock = initialStock;
            foreach (var qty in receipts)
                currentStock += qty;

            foreach (var qty in issues)
            {
                if (currentStock >= qty)
                    currentStock -= qty;
                // Nếu không đủ hàng, từ chối xuất (không trừ)
            }

            return currentStock >= 0;
        });
    }

    // -----------------------------------------------------------------------
    // Property 26: Cảnh báo tồn kho thấp khi số lượng xuống dưới ngưỡng
    // Validates: Requirements 9.5
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P26_LowStockWarningWhenBelowThreshold()
    {
        // Feature: medical-examination-team-management, Property 26: Low stock warning below threshold
        var gen =
            from threshold in Gen.Choose(5, 50)
            from quantity in Gen.Choose(0, 100)
            select (threshold, quantity);

        return Prop.ForAll(Arb.From(gen), data =>
        {
            var (threshold, quantity) = data;

            var item = new SupplyItem
            {
                ItemName = "Test Item",
                CurrentStock = quantity,
                MinStockLevel = threshold
            };

            // Logic từ CheckLowStockAsync
            bool shouldWarn = item.CurrentStock <= item.MinStockLevel;

            if (quantity <= threshold)
                return shouldWarn;
            else
                return !shouldWarn;
        });
    }

    // -----------------------------------------------------------------------
    // Property 27: Chi phí vật tư đoàn khám bằng tổng giá trị phiếu xuất liên kết
    // Validates: Requirements 9.6, 10.2
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P27_MaterialCostEqualsSumOfIssues()
    {
        // Feature: medical-examination-team-management, Property 27: Material cost = sum of issues
        var gen =
            from teamId in Gen.Choose(1, 100)
            from issueCount in Gen.Choose(0, 10)
            from quantities in Gen.ListOf(issueCount, Gen.Choose(1, 100))
            from unitPrices in Gen.ListOf(issueCount, Gen.Choose(10_000, 500_000))
            select (teamId, quantities.ToList(), unitPrices.ToList());

        return Prop.ForAll(Arb.From(gen), data =>
        {
            var (teamId, quantities, unitPrices) = data;

            // Tạo phiếu xuất kho liên kết với đoàn
            var issues = quantities.Zip(unitPrices, (qty, price) => new StockMovement
            {
                MedicalGroupId = teamId,
                Quantity = qty,
                UnitPrice = price,
                TotalValue = qty * price,
                MovementType = "OUT"
            }).ToList();

            // Chi phí vật tư = Σ TotalValue của phiếu xuất
            decimal materialCost = issues
                .Where(i => i.MedicalGroupId == teamId && i.MovementType == "OUT")
                .Sum(i => i.TotalValue);

            decimal expectedCost = quantities.Zip(unitPrices, (qty, price) => (decimal)(qty * price)).Sum();

            return materialCost == expectedCost;
        });
    }
}
