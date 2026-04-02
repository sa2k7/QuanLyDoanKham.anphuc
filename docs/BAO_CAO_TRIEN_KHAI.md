# BÁO CÁO TRIỂN KHAI VÀ SỬ DỤNG HỆ THỐNG QUẢN LÝ ĐOÀN KHÁM

## PHẦN 7 – TRIỂN KHAI VÀ SỬ DỤNG (ASP.NET Core + Vue.js + SmarterASP.NET + Vercel)

---

### 1. Đóng gói sản phẩm (Packaging)

#### Frontend (Vue.js)
- **Thư mục:** `QuanLyDoanKham.Web/`
- **Lệnh build:**
  ```bash
  cd QuanLyDoanKham.Web && npm install && npm run build
  ```
- **Output:** Thư mục `dist/` (Chứa các tệp tĩnh HTML/JS/CSS).
- **Tối ưu:** Sử dụng Vite để bundle, nén Gzip/Brotli, Lazy loading components.

#### Backend (ASP.NET Core API)
- **Thư mục:** `QuanLyDoanKham.API/`
- **Lệnh publish:**
  ```bash
  dotnet publish -c Release -o ./publish_backend
  ```
- **Output:** Các tệp `.dll` và cấu hình chạy trên IIS (.NET 8.0).
- **Cấu hình (`appsettings.json`):**
  ```json
  "ConnectionStrings": {
    "DefaultConnection": "Server=sql5111.site4now.net;Database=db_quanlydoankham;User Id=...;Password=..."
  },
  "JWT": {
    "Secret": "your_super_secret_key_at_least_32_chars",
    "ValidAudience": "https://quanlydoankham.vercel.app",
    "ValidIssuer": "https://api-quanlydoankham.com"
  }
  ```

---

### 2. Triển khai Backend & SQL Server (SmarterASP.NET)

#### Triển khai Cơ sở dữ liệu (MSSQL)
- **Tạo Database:** Trên control panel SmarterASP, khởi tạo DB mới (ví dụ: `db_quanlydoankham`).
- **Migration:** Sử dụng Entity Framework Core để cập nhật schema hoặc import script SQL thủ công.
  ```bash
  dotnet ef database update --project QuanLyDoanKham.API
  ```

#### Triển khai API (.NET Core)
- **Upload:** Sử dụng FTP hoặc File Manager để tải nội dung thư mục `publish_backend` lên thư mục gốc `/site1` (hoặc `/api`).
- **Phiên bản Runtime:** Đảm bảo Server chạy **.NET Core 8.0 Runtime**.
- **Quyền hạn:** Cấp quyền đọc/ghi cho thư mục `wwwroot` để lưu trữ tài liệu/ảnh đoàn khám.

#### Kiểm tra API
- Truy cập: `https://your-api-domain.com/swagger/index.html`
- Kiểm tra Endpoint Health: `https://your-api-domain.com/api/health`

---

### 3. Triển khai Frontend (Vercel)

- **Kết nối:** Link repository GitHub vào Vercel.
- **Cấu hình Build:**
    - **Framework Preset:** `Vite`
    - **Root Directory:** `QuanLyDoanKham.Web`
    - **Build Command:** `npm run build`
    - **Output Directory:** `dist`
- **Environment Variables:**
    - `VITE_API_BASE_URL`: `https://your-api-domain.com/api`

> [!IMPORTANT]
> **Xử lý SEO & Routing (SPA):** Cần thêm file `vercel.json` ở thư mục gốc của repo để tránh lỗi 404 khi người dùng refresh trình duyệt:
> ```json
> {
>   "rewrites": [
>     { "source": "/(.*)", "destination": "/index.html" }
>   ]
> }
> ```

---

### 4. Cấu hình Kết nối & Bảo mật (CORS)

#### API (Program.cs)
Cấu hình để API chấp nhận yêu cầu từ domain Vercel:
```csharp
builder.Services.AddCors(options => {
    options.AddPolicy("AllowVercel", policy => {
        policy.WithOrigins("https://quanlydoankham.vercel.app")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
app.useCors("AllowVercel");
```

#### Web (apiClient.js)
```javascript
const apiClient = axios.create({
    baseURL: import.meta.env.VITE_API_BASE_URL,
    withCredentials: true
});
```

---

### 5. Hướng dẫn sử dụng hệ thống

#### Quản trị viên (Admin)
- Thiết lập hệ thống, danh mục dịch vụ y tế.
- Quản lý tài khoản và phân quyền cán bộ y tế.

#### Quản lý Đoàn khám (Medical Group Manager)
- **Quản lý Hợp đồng:** Ký kết hợp đồng khám sức khỏe định kỳ cho công ty.
- **Quản lý Nhân sự Đoàn:** Import danh sách nhân viên từ Excel, phân loại gói khám.
- **Theo dõi tiến độ:** Xem báo cáo thời gian thực về tiến độ khám của từng đoàn.

#### Bác sĩ / Kỹ thuật viên
- Nhập kết quả cận lâm sàng (Xét nghiệm, X-quang) và lâm sàng.
- Kết luận và phân loại sức khỏe nhân viên.

---

### 6. Danh mục kiểm tra (Checklist)
- [ ] **Backend:** API phản hồi < 300ms, Swagger hoạt động.
- [ ] **Frontend:** Tải trang mượt, không lỗi console (404/500).
- [ ] **Kết nối:** CORS đã được cấu hình cho domain Production.
- [ ] **Database:** Đồng bộ dữ liệu local và production thành công.
- [ ] **SSL:** Đã kích hoạt HTTPS cho cả Web và API.

---

### 7. Bảo trì & Giám sát

- **Logs:** Kiểm tra lỗi phát sinh qua hệ thống Serilog/NLog trong Backend.
- **Backup:** Sao lưu Database hàng ngày (24h/lần) qua control panel SmarterASP.
- **Deployment:** CI/CD thông qua Git-Vercel giúp cập nhật frontend nhanh chóng.

---

### Thông tin liên kết (URLs)

*   **Trang chủ (Web):** `https://quanlydoankham.vercel.app`
*   **Cổng API (Swagger):** `https://api-quanlydoankham.com/swagger`
*   **Công nghệ:** ASP.NET Core 8 + Vue.js 3 + MSSQL + Tailwind CSS (Brutalist Style).

---
*Báo cáo được khởi tạo tự động bởi hệ thống AntiGravity (Agent: komi)*
