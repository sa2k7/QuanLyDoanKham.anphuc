# 🛡️ CẨM NANG BẢO VỆ & ỔN ĐỊNH DỰ ÁN (DEFENSE & STABILIZATION MANUAL)

Tài liệu này tổng hợp toàn bộ cấu hình hệ thống Phân quyền (RBAC) và các tài khoản kiểm thử để phục vụ cho việc Demo và Bảo vệ dự án.

---

## 👥 1. DANH SÁCH TÀI KHOẢN KIỂM THỬ (TEST ACCOUNTS)
**Mật khẩu chung cho tất cả:** `Test@123456`

| Username | Vai trò (Role) | Phòng ban (Department) | Mục đích chính |
| :--- | :--- | :--- | :--- |
| **admin_master** | Admin | Ban giám đốc | Toàn quyền, quản trị hệ thống. |
| **contract_creator** | ContractCreator | Hành chính nhân sự | Tạo và soạn thảo hợp đồng. |
| **contract_approver** | ContractApprover | Phòng nhân sự | Phê duyệt hợp đồng. |
| **medical_group_manager** | MedicalGroupManager | Quản lý đoàn khám | Tạo đoàn, điều phối nhân sự (AI Suggest). |
| **group_leader** | GroupLeader | Đoàn khám | Quản lý tại chỗ, QR, Check-in/out. |
| **warehouse_staff** | WarehouseManager | Kho | Quản lý vật tư tiêu hao. |
| **payroll_accountant** | Accountant | Kế toán | Quyết toán & Tính lương nhân sự. |
| **staff_user** | MedicalStaff | Nhân viên đoàn khám | Xem lịch cá nhân, tự điểm danh. |

---

## 🗺️ 2. MA TRẬN HIỂN THỊ MENU (ROLE-MENU MATRIX)

Hệ thống tự động ẩn/hiện Menu dựa trên vai trò để tối ưu giao diện người dùng (UI/UX).

| Vai trò | Menu hiển thị |
| :--- | :--- |
| **Admin** | Toàn bộ hệ thống. |
| **ContractCreator** | Dashboard, Công ty, Hợp đồng. |
| **ContractApprover** | Dashboard, Hợp đồng. |
| **MedicalGroupManager**| Dashboard, Đoàn khám, Bệnh nhân, Thống kê. |
| **GroupLeader** | Dashboard, Đoàn khám, Bệnh nhân, Lịch của tôi, Chấm công. |
| **WarehouseManager** | Dashboard, Vật tư. |
| **Accountant** | Dashboard, Quyết toán, Thống kê, Tính lương. |
| **MedicalStaff** | Hôm nay, Bệnh nhân, Lịch của tôi. |

---

## 🔑 3. MA TRẬN QUYỀN HẠN CHI TIẾT (ROLE-PERMISSION MATRIX)

| Vai trò | Quyền hạn nghiệp vụ chính |
| :--- | :--- |
| **ContractCreator** | Xem/Tạo/Sửa/Upload hợp đồng, Gửi duyệt. Không có quyền Approve/Reject. |
| **ContractApprover** | Xem/Duyệt/Từ chối hợp đồng. Không có quyền Tạo/Sửa. |
| **MedicalGroupManager**| Quản lý Đoàn khám (Tạo/Sửa/Setup vị trí/Phân nhân sự/AI Suggest/QR/Attendance). |
| **GroupLeader** | Xem đoàn & bệnh nhân, Điểm danh QR, Check-in/out, Xem lịch & chấm công. |
| **WarehouseManager** | Quản lý Kho (Xem/Sửa/Import/Export/Báo cáo). |
| **Accountant** | Xem hợp đồng, Quản lý Lương (Payroll), Quyết toán tài chính (Settlement). |
| **MedicalStaff** | Xem lịch cá nhân, Tự điểm danh, Xem danh sách bệnh nhân đoàn tham gia. |
| **Admin** | Mọi quyền hạn trong hệ thống. |

---

## 🚀 4. KỊCH BẢN DEMO ĐỀ XUẤT (DEMO FLOW)

1.  **Bước 1 (Hợp đồng):** Dùng `contract_creator` tạo hợp đồng -> Dùng `contract_approver` để duyệt.
2.  **Bước 2 (Đoàn khám):** Dùng `medical_group_manager` tạo đoàn từ hợp đồng đã duyệt -> Phân nhân sự.
3.  **Bước 3 (Vận hành):** Dùng `group_leader` hoặc `staff_user` để thực hiện Điểm danh (Attendance) qua QR.
4.  **Bước 4 (Tài chính):** Dùng `payroll_accountant` để tính lương dựa trên dữ liệu điểm danh ở Bước 3.

---

## 🛠️ 5. GHI CHÚ KỸ THUẬT (TECHNICAL NOTES)

*   **API Base URL:** `http://localhost:5283`
*   **Database Seeding:** Để nạp lại bộ dữ liệu này, chạy lệnh `dotnet run --seed` (nếu đã tích hợp).
*   **Security:** Toàn bộ API đều được bảo vệ bởi lớp Authorization Middleware. Việc cố tình truy cập sai quyền sẽ trả về mã lỗi **403 Forbidden**.
