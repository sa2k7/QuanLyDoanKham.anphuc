# 🔐 Thông Tin Đăng Nhập Hệ Thống

## Tài Khoản Mặc Định

### 👨‍💼 Admin (Quản Trị Viên)
```
Username: admin
Password: admin123
Role: Admin
Quyền hạn: Toàn quyền (Quản lý tất cả)
```

**Có thể truy cập:**
- ✅ Quản lý Công ty (`/companies`)
- ✅ Quản lý Hợp đồng (`/contracts`)
- ✅ Quản lý Đoàn khám (`/groups`)
- ✅ Quản lý Nhân sự (`/staff`)
- ✅ Quản lý Vật tư (`/supplies`)
- ✅ Quản lý Bệnh nhân (`/patients`)
- ✅ Báo cáo (`/reports`)
- ✅ Quản lý User (`/users`)

---

### 🏢 VinGroup (Khách Hàng)
```
Username: vingroup
Password: vingroup123
Role: Customer
Quyền hạn: Chỉ xem hợp đồng của công ty mình
```

**Có thể truy cập:**
- ✅ Xem hợp đồng của VinGroup
- ✅ Xem báo cáo liên quan đến hợp đồng của mình
- ❌ Không thể tạo/sửa/xóa dữ liệu

---

### 🏢 FPT Software (Khách Hàng)
```
Username: fpt
Password: fpt123
Role: Customer
Quyền hạn: Chỉ xem hợp đồng của công ty mình
```

**Có thể truy cập:**
- ✅ Xem hợp đồng của FPT
- ✅ Xem báo cáo liên quan đến hợp đồng của mình
- ❌ Không thể tạo/sửa/xóa dữ liệu

---

## 🔄 Cách Tạo Tài Khoản Mới

### Qua Code (Recommended)
```csharp
// Trong AuthController.cs
[HttpPost("register")]
public async Task<ActionResult<AppUser>> Register(RegisterDto dto)
{
    var user = new AppUser
    {
        Username = dto.Username,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
        FullName = dto.FullName,
        RoleId = dto.RoleId,
        CompanyId = dto.CompanyId
    };
    
    _context.Users.Add(user);
    await _context.SaveChangesAsync();
    return Ok(user);
}
```

### Qua SQL (Nhanh)
```sql
INSERT INTO Users (Username, PasswordHash, FullName, RoleId, CompanyId)
VALUES 
    ('newuser', '$2a$11$...', N'Tên người dùng', 2, NULL);
-- RoleId: 1=Admin, 2=Staff, 3=Customer
```

**Lưu ý:** Mật khẩu phải được hash bằng BCrypt trước khi lưu vào database.

---

## 📊 Dữ Liệu Mẫu Có Sẵn

Sau khi chạy script `03_SEED_SAMPLE_DATA.sql`, hệ thống sẽ có:

- **3 Công ty**: VinGroup, FPT Software, Viettel Group
- **3 Hợp đồng**: Với giá trị từ 300 triệu đến 800 triệu VNĐ
- **3 Nhân viên Y tế**: Bác sĩ, Điều dưỡng, Kỹ thuật viên
- **5 Loại vật tư**: Găng tay, Khẩu trang, Máy đo huyết áp, v.v.
- **2 Đoàn khám**: Đã được tạo sẵn
- **3 Bệnh nhân**: Thuộc các hợp đồng khác nhau

---

## 🚀 Hướng Dẫn Sử Dụng

1. **Đăng nhập lần đầu:**
   - Truy cập: `http://localhost:5173/login`
   - Nhập: `admin` / `admin123`
   - Nhấn "Đăng nhập ngay"

2. **Kiểm tra quyền Admin:**
   - Sau khi đăng nhập, bạn sẽ thấy đầy đủ menu bên trái
   - Thử truy cập: `http://localhost:5173/admin` → Sẽ redirect về Dashboard

3. **Test phân quyền Customer:**
   - Đăng xuất
   - Đăng nhập bằng `vingroup` / `vingroup123`
   - Bạn sẽ chỉ thấy menu "Hợp đồng" và "Báo cáo"
   - Chỉ hiển thị hợp đồng của VinGroup

---

## 🔒 Bảo Mật

- ✅ Mật khẩu được hash bằng BCrypt (không lưu plain text)
- ✅ JWT Token có thời hạn 24 giờ
- ✅ Refresh Token có thời hạn 7 ngày
- ✅ Navigation Guards kiểm tra quyền truy cập mỗi route
- ✅ Backend có `[Authorize]` attribute bảo vệ API

---

## ❓ Troubleshooting

**Lỗi: "Đăng nhập thất bại"**
- Kiểm tra Backend đã chạy chưa (`http://localhost:5283`)
- Kiểm tra database đã có user `admin` chưa
- Xem Console log để biết lỗi cụ thể

**Lỗi: "Không thể tải danh sách hợp đồng"**
- Kiểm tra CORS đã được bật trong `Program.cs`
- Kiểm tra token còn hạn không (F12 → Application → Local Storage)
- Thử đăng xuất và đăng nhập lại

**Route `/admin` không hoạt động**
- Route này đã được cấu hình redirect về Dashboard (`/`)
- Chỉ Admin mới truy cập được
- Nếu chưa đăng nhập, sẽ redirect về `/login`
