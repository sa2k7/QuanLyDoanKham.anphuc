# QuanLyDoanKham.API

## Giới thiệu
Đây là Backend API cho hệ thống Quản lý Đoàn Khám, được xây dựng bằng **ASP.NET Core Web API (.NET 8)** và **Entity Framework Core**.

## Cấu trúc Dự án
- **Models/Entities.cs**: Chứa toàn bộ định nghĩa bảng (User, Company, Contract...).
- **Data/ApplicationDbContext.cs**: Cấu hình kết nối CSDL và quan hệ bảng.
- **Program.cs**: Cấu hình Dependency Injection và Swagger.

## Cách chạy dự án
1. Mở project bằng Visual Studio 2022 hoặc VS Code.
2. Kiểm tra chuỗi kết nối trong `appsettings.json` (đã cấu hình sẵn cho Local SQL Server).
3. Chạy lệnh sau để khởi tạo Database (nếu chưa có):
   ```bash
   dotnet ef database update
   ```
4. Chạy dự án:
   ```bash
   dotnet run
   ```
5. Truy cập Swagger UI tại: `http://localhost:5xxx/swagger` để test API.

## Thông tin CSDL
- Database: `QuanLyDoanKham` (được tạo tự động).
- Bảng chính: `AppUsers`, `AppRoles`, `Companies`, `Contracts`, `MedicalGroups`...
