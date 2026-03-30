import { test, expect } from '@playwright/test';

test.describe('Xác thực (Authentication)', () => {
  test('Đăng nhập thành công với quyền Admin', async ({ page }) => {
    await page.goto('/login');
    
    // Sử dụng getByPlaceholder chuẩn Playwright
    await page.getByPlaceholder('Email hoặc Tên đăng nhập').fill('admin');
    await page.getByPlaceholder('Mật khẩu bảo mật').fill('admin123');
    
    // Bấm Đăng nhập hệ thống (Nút to nhất)
    await page.getByRole('button', { name: /Đăng nhập hệ thống/i }).click();
    
    // Đợi chuyển hướng về trang chủ và tìm nút "Hợp đồng" trên Sidebar
    await page.waitForURL('**/');
    await expect(page.getByRole('button', { name: 'Hợp đồng' })).toBeVisible({ timeout: 15000 });
  });

  test('Hiển thị lỗi khi sai mật khẩu', async ({ page }) => {
    await page.goto('/login');
    await page.getByPlaceholder('Email hoặc Tên đăng nhập').fill('admin');
    await page.getByPlaceholder('Mật khẩu bảo mật').fill('wrongpassword');
    await page.getByRole('button', { name: /Đăng nhập hệ thống/i }).click();
    
    // Chờ thông báo lỗi cụ thể
    await expect(page.getByText('Mật khẩu không chính xác.')).toBeVisible();
  });
});
