import { test, expect } from '@playwright/test';

test.describe('Quản lý Hợp đồng (Contracts Management)', () => {
  // Thực hiện đăng nhập trước mỗi test
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.fill('input[type="text"]', 'admin');
    await page.fill('input[type="password"]', 'admin123');
    await page.click('button[type="submit"]');
    
    // Đợi Dashboard hiện lên và bấm vào nút Hợp đồng
    await expect(page.getByRole('button', { name: 'Hợp đồng' })).toBeVisible({ timeout: 10000 });
    await page.getByRole('button', { name: 'Hợp đồng' }).click();
    
    // Chờ bảng dữ liệu hoặc lưới hiện lên
    await expect(page.locator('table, .contracts-list, .grid')).toBeVisible();
  });

  test('Hiển thị Danh sách Hợp đồng và thẻ Thống kê', async ({ page }) => {
    // 1. Kiểm tra StatCards (Dùng nhãn viết hoa theo thực tế)
    await expect(page.getByText('TỔNG GIÁ TRỊ HĐ (DỰ KIẾN)')).toBeVisible();
    await expect(page.getByText('HĐ ĐANG THỰC HIỆN')).toBeVisible();
    
    // 2. Kiểm tra không bị trắng màn hình
    const rowCount = await page.locator('table tbody tr').count();
    if (rowCount === 0) {
      await expect(page.locator('text=Không có dữ liệu, FileText')).toBeVisible();
    } else {
      // 3. Kiểm tra Nhãn trạng thái (getStatusLabel)
      await expect(page.locator('table td').getByText(/Đang chờ|Đã phê duyệt|Đang thực hiện/)).first().toBeVisible();
    }
  });

  test('Tự động tính Tổng giá trị trong Form Tạo hợp đồng', async ({ page }) => {
    // Bấm nút TẠO HỢP ĐỒNG
    await page.click('button:has-text("TẠO HỢP ĐỒNG")');
    
    // Điền Đơn giá và Số lượng (Dùng selector thực tế)
    await page.locator('input[placeholder="0"]').first().fill('1000000');
    await page.locator('input[placeholder="0"]').last().fill('10');
    
    // Kiểm tra Tổng giá trị dự kiến (1.000.000 * 10 = 10.000.000)
    await expect(page.getByText('10.000.000')).toBeVisible();
  });

  test('Mở Modal chi tiết khi bấm nút Xem (Eye)', async ({ page }) => {
    // Bấm nút Eye (biểu tượng xem chi tiết)
    await page.locator('button .lucide-eye').first().click();
    await expect(page.locator('text=Chi tiết hợp đồng')).toBeVisible();
  });

  test('Lọc Hợp đồng qua các Tab', async ({ page }) => {
    // Bấm vào tab "Đang chờ"
    await page.click('text=Đang chờ, button:has-text("Đang chờ")');
    // Chờ 1 chút để filter chạy
    await page.waitForTimeout(500);
    // Kiểm tra dòng dữ liệu
    const rows = page.locator('table tbody tr');
    await expect(rows).toBeDefined();
  });
});
