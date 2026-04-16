# 📋 Hướng dẫn Tích hợp Module Phân quyền & Tài khoản Nâng cấp

Tài liệu này hướng dẫn cách tích hợp hệ thống Nhật ký thao tác (Audit Log) và chuẩn bị cho phân quyền động.

---

## 📁 Các File Mới Được Tạo

| File | Vị trí | Mô tả |
|------|--------|-------|
| `AuditLogService.cs` | `/QuanLyDoanKham.API/Services/Auth/` | Service ghi nhật ký thao tác người dùng |
| `AuditLogController.cs` | `/QuanLyDoanKham.API/Controllers/` | API endpoints cho Nhật ký thao tác |
| `AuditLogView.vue` | `/QuanLyDoanKham.Web/src/views/` | Giao diện hiển thị nhật ký cho Admin |

---

## 🔧 Các Bước Tích hợp

### Bước 1: Đăng ký Service trong Dependency Injection (Program.cs)

Mở file `QuanLyDoanKham.API/Program.cs` và thêm các dòng sau:

```csharp
services.AddHttpContextAccessor(); // Bắt buộc để lấy IP và Username
services.AddScoped<IAuditLogService, AuditLogService>();
```

---

### Bước 2: Cập nhật Router Frontend

Mở file `QuanLyDoanKham.Web/src/router/index.js` và thêm route cho trang Nhật ký:

```javascript
{
  path: '/audit-logs',
  name: 'AuditLogs',
  component: () => import('../views/AuditLogView.vue'),
  meta: {
    title: 'Nhật Ký Thao Tác',
    permission: 'HeThong.AuditLog'
  }
}
```

---

### Bước 3: Cấu hình Database cho Audit Log

Bạn cần tạo bảng `AuditLogs` trong SQL Server để lưu trữ dữ liệu. Chạy câu lệnh SQL sau:

```sql
CREATE TABLE AuditLogs (
    AuditLogId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL,
    Action NVARCHAR(50) NOT NULL,
    Module NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    OldValue NVARCHAR(MAX) NULL,
    NewValue NVARCHAR(MAX) NULL,
    Metadata NVARCHAR(MAX) NULL,
    IpAddress NVARCHAR(50) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
```

---

### Bước 4: Cách sử dụng Audit Log trong Code

Để ghi nhật ký khi thực hiện một hành động (ví dụ: Xóa hợp đồng), hãy inject `IAuditLogService` vào Controller và gọi:

```csharp
await _auditLogService.LogAsync(
    action: "DELETE",
    module: "Hợp đồng",
    description: $"Xóa hợp đồng ID: {contractId} - Tên: {contractName}",
    oldValue: JsonSerializer.Serialize(oldData),
    newValue: null
);
```

---

## 🛡️ Cải thiện về Phân quyền (Dynamic Roles)

Hệ thống hiện tại của bạn đã có các bảng `Roles`, `Permissions`, `RolePermissions`. Để chuyển sang phân quyền động hoàn toàn:

1.  **Sử dụng Giao diện Role Management**: Bạn có thể dùng API `PUT api/Auth/role-permissions/{roleId}` (đã có sẵn trong `AuthController.cs`) để cập nhật quyền cho từng Role ngay trên giao diện Web mà không cần sửa code.
2.  **Middleware Kiểm tra Quyền**: Attribute `[AuthorizePermission("HeThong.AuditLog")]` sẽ tự động kiểm tra xem Role của người dùng hiện tại có PermissionKey tương ứng trong bảng `RolePermissions` hay không.

---

## 🔍 Các Tính năng Mới của Audit Log

- **Minh bạch**: Biết chính xác AI đã sửa gì, khi nào và từ IP nào.
- **Truy vết**: Lưu trữ cả giá trị cũ (`OldValue`) và giá trị mới (`NewValue`) để có thể khôi phục dữ liệu nếu cần.
- **Bộ lọc thông minh**: Admin có thể lọc theo Người dùng, Module hoặc khoảng thời gian để tìm kiếm sự cố.

---

*Hướng dẫn được tạo ngày 15/04/2026 bởi Manus AI.*
