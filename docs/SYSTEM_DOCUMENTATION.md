# 📋 Tài liệu Tổng Hợp Hệ Thống — QuanLyDoanKham (An Phúc)
> Tổng hợp từ mã nguồn thực tế | Cập nhật: 2026-04-06

---

## 🏛️ 1. KIẾN TRÚC TỔNG QUAN

```
┌─────────────────────────────────────────────────┐
│               FRONTEND (Vue 3 + Vite)            │
│  Dashboard.vue (SPA Shell) → View Components    │
│  Vue Router + Pinia (Auth, Notification stores) │
└─────────────────────┬───────────────────────────┘
                      │ HTTP/REST (axios)
┌─────────────────────▼───────────────────────────┐
│              BACKEND (.NET 10 API)               │
│  JWT Auth + Permission-based Authorization       │
│  14 Controllers → EF Core → ApplicationDbContext │
└─────────────────────┬───────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────┐
│  SQL Server (LocalDB dev / SmarterASP cloud)     │
│  48+ Tables                                     │
└─────────────────────────────────────────────────┘
```

---

## 👤 2. PHÂN QUYỀN (RBAC)

### 2.1 Danh sách Roles

| RoleId | RoleName | Mô tả |
|---|---|---|
| 1 | `Admin` | Quản trị hệ thống |
| 2 | `PersonnelManager` | Quản lý nhân sự |
| 3 | `ContractManager` | Quản lý hợp đồng |
| 4 | `PayrollManager` | Quản lý tính lương |
| 5 | `MedicalGroupManager` | Quản lý đoàn khám |
| 6 | `WarehouseManager` | Quản lý kho vật tư |
| 7 | `GroupLeader` | Trưởng đoàn khám |
| 8 | `MedicalStaff` | Nhân viên đi đoàn |
| 9 | `Accountant` | Kế toán |
| 10 | `Customer` | Đại diện doanh nghiệp |

### 2.2 Ma trận Quyền

| Module | Permission Key | Admin | ContractMgr | MedGrpMgr | PersonnelMgr | PayrollMgr | WarehouseMgr | GroupLeader | MedStaff | Accountant |
|---|---|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| **Hợp đồng** | HopDong.View | ✅ | ✅ | | | | | | | ✅ |
| | HopDong.Create | | ✅ | | | | | | | |
| | HopDong.Edit | | ✅ | | | | | | | |
| | HopDong.Approve | | ✅ | | | | | | | |
| | HopDong.Reject | | ✅ | | | | | | | |
| | HopDong.Upload | | ✅ | | | | | | | |
| **Đoàn Khám** | DoanKham.View | ✅ | | ✅ | | | | ✅ | | |
| | DoanKham.Create | | | ✅ | | | | | | |
| | DoanKham.Edit | | | ✅ | | | | | | |
| | DoanKham.SetPosition | | | ✅ | | | | | | |
| | DoanKham.AssignStaff | | | ✅ | ✅ | | | | | |
| | DoanKham.ManageOwn | | | ✅ | | | | | | |
| **Lịch Khám** | LichKham.ViewOwn | | | | | | | | ✅ | |
| | LichKham.ViewAll | | | ✅ | ✅ | ✅ | | ✅ | | |
| **Chấm Công** | ChamCong.QR | | | | | | | ✅ | | |
| | ChamCong.CheckInOut | | | | | | | ✅ | | |
| | ChamCong.ViewAll | ✅ | | | | ✅ | | | | |
| **Kho** | Kho.View | ✅ | | | | | ✅ | | | |
| | Kho.Import | | | | | | ✅ | | | |
| | Kho.Export | | | | | | ✅ | | | |
| **Lương** | Luong.View | ✅ | | | | ✅ | | | | ✅ |
| | Luong.Manage | | | | | ✅ | | | | |
| **Nhân Sự** | NhanSu.View | ✅ | | | ✅ | ✅ | | | | |
| | NhanSu.Manage | | | | ✅ | | | | | |
| **Báo Cáo** | BaoCao.View | ✅ | | | | | | | | ✅ |
| | BaoCao.Export | | | | | | | | | ✅ |
| **Hệ Thống** | HeThong.UserManage | ✅ | | | | | | | | |
| | HeThong.RoleManage | ✅ | | | | | | | | |

> [!NOTE]
> **PoLP:** Admin KHÔNG có quyền tạo HĐ, tính lương hay phân công nhân sự trực tiếp. Chỉ xem + cấu hình hệ thống.

---

## 🔄 3. QUY TRÌNH NGHIỆP VỤ ĐẦU CUỐI (B2B Core Flow)

```
[ContractManager]    [MedGrpManager]     [GroupLeader]    [MedicalStaff]
       │                   │                   │                │
       ▼                   │                   │                │
 1. Tạo HĐ (Draft)        │                   │                │
       │                   │                   │                │
       ▼                   │                   │                │
 2. Trình duyệt 2 bước    │                   │                │
    Trưởng phòng → BGĐ    │                   │                │
       │                   │                   │                │
       ▼                   │                   │                │
 3. HĐ → Active           │                   │                │
       │                   │                   │                │
       │                   ▼                   │                │
                     4. Tạo Đoàn Khám         │                │
                        (Từ HĐ Active)         │                │
                           │                   │                │
                           ▼                   │                │
                     5. Cơ cấu Vị trí         │                │
                        (Bác sĩ, Y tá, KTV..) │                │
                           │                   │                │
                           ▼                   │                │
                     6. Phân công NV          │                │
                        (Thủ công / AI)        │                │
                           │                   │                │
                           ▼                   │                │
                     7. Xuất kho VT           │                │
                           │                   ▼                │
                           │             8. Mở QR Code         │
                           │                (12h token)         │
                           │                   │                ▼
                           │                   │         9. Quét QR
                           │                   │            Check-in/out
                           │                   │            → ScheduleCalendar
                           │                   │                │
                           └───────────────────┘                │
                           │                                     │
                           ▼                                     │
                    10. Đóng Đoàn → Finished ←──────────────────┘
                           │
                           ▼
                    11. Tính Lương [PayrollManager]
                        ShiftType × WagePerShift
                           │
                           ▼
                    12. Báo cáo Excel [Accountant]
                        - DSDOAN (Nhân sự)
                        - THEODOIHDKSK (Thu/Chi)
```

---

## 📦 4. USE CASES CHI TIẾT

### Module Hợp Đồng (`/contracts`) — ContractManager

| UC# | Use Case | Permission | Kết quả DB |
|---|---|---|---|
| UC-01 | Tạo hợp đồng | HopDong.Create | HealthContract (Status=Draft) |
| UC-02 | Upload file HĐ | HopDong.Upload | ContractAttachment |
| UC-03 | Gửi duyệt | HopDong.Approve | Status=PendingApproval |
| UC-04 | Duyệt Bước 1 | HopDong.Approve | ContractApprovalHistory, step++ |
| UC-05 | Duyệt Bước 2 | HopDong.Approve | Status=Active |
| UC-06 | Từ chối | HopDong.Reject | Status=Rejected |

**Status Flow:** `Draft → PendingApproval → Approved → Active → Finished → Locked`

---

### Module Đoàn Khám (`/groups`) — MedicalGroupManager

| UC# | Use Case | Permission | Kết quả DB |
|---|---|---|---|
| UC-10 | Tạo đoàn từ HĐ | DoanKham.Create | MedicalGroup (Status=Planning) |
| UC-11 | Import DS bệnh nhân | DoanKham.Edit | Patient[] từ file Excel |
| UC-12 | Thiết lập cơ cấu | DoanKham.SetPosition | GroupPositionQuota |
| UC-13 | Phân công thủ công | DoanKham.AssignStaff | GroupStaffDetail |
| UC-14 | AI Suggest Staff | DoanKham.View | Preview gợi ý Gemini |
| UC-15 | Xuất kho vật tư | Kho.Export | SupplyInventoryVoucher (Xuất) |
| UC-16 | Tạo QR Check-in | ChamCong.QR | JWT token 12h |
| UC-17 | Xuất Excel NV | DoanKham.View | File DSDOAN_*.xlsx |
| UC-18 | Xuất Excel Tổng | DoanKham.View | File THEODOIHDKSK.xlsx |
| UC-19 | Đóng Đoàn | DoanKham.Edit | Status=Finished |

**Status Flow:** `Planning → Ready → InProgress → Finished → Archived`

---

### Module Chấm Công (`/checkin` — Public)

| UC# | Use Case | Actor | Kết quả |
|---|---|---|---|
| UC-20 | Mở QR | GroupLeader | URL + QR hiển thị |
| UC-21 | Quét QR lần 1 | MedicalStaff | ScheduleCalendar.CheckInTime |
| UC-22 | Quét QR lần 2 | MedicalStaff | ScheduleCalendar.CheckOutTime |
| UC-23 | Tính ShiftType | Auto | 1.0=Cả ngày / 0.5=Nửa ngày |

---

### Module Tính Lương (`/payroll`) — PayrollManager

| UC# | Use Case | Logic |
|---|---|---|
| UC-30 | Tính lương tháng | SUM(ShiftType × WagePerShift) per Staff |
| UC-31 | Xem lịch sử | PayrollRecord filter by month/staff |
| UC-32 | Xuất bảng lương | Excel toàn nhân sự |

---

### Module Kho Vật Tư (`/supplies`) — WarehouseManager

| UC# | Use Case | Kết quả |
|---|---|---|
| UC-40 | Nhập kho | Voucher=Nhập + SupplyInventoryDetail |
| UC-41 | Xuất kho theo đoàn | Voucher=Xuất, gắn MedicalGroupId |
| UC-42 | Xem tồn kho | TotalIn - TotalOut per Supply |

---

### Module Bệnh Nhân (`/patients`) — Module Mới B2C

| UC# | Use Case | Kết quả |
|---|---|---|
| UC-50 | Tạo lý lịch BN | Patient (Họ tên, CMND, Công ty) |
| UC-51 | Tạo hồ sơ khám | MedicalRecord (liên kết Group) |
| UC-52 | Ghi dịch vụ phát sinh | MedicalRecordService (ngoài HĐ) |
| UC-53 | Xem lịch sử khám BN | Tất cả MedicalRecord của Patient |

---

## 🗄️ 5. SƠ ĐỒ QUAN HỆ DATABASE

```
Company ─────────────────────────────────────┐
  │ 1:N                                      │ 1:N
  ▼                                          ▼
HealthContract ─── ContractPackage         Patient
  │                                          │ 1:N
  │ 1:N                                      ▼
  ▼                                        MedicalRecord
MedicalGroup ────────────────────────────────┘
  │
  ├──── GroupStaffDetail ──── Staff ──── ScheduleCalendar ──── PayrollRecord
  ├──── GroupPositionQuota ── Position
  └──── SupplyInventoryVoucher ── SupplyInventoryDetail ── Supply
```

---

## 🔐 6. BẢO MẬT

| Cơ chế | Chi tiết |
|---|---|
| JWT Bearer Token | Tất cả API (trừ `/login`, `/checkin`) |
| `[Authorize]` | Decorator toàn bộ Controller |
| `[AuthorizePermission("Key")]` | Guard từng action theo PermissionKey |
| BCrypt hash | Mật khẩu `$2a$11$...` |
| QR JWT | Token riêng, hạn 12h, chỉ dùng checkin |
| CORS | Chỉ allow `FrontendUrl` trong appsettings |
| PoLP | Admin không có quyền CRUD nghiệp vụ |

---

## 🌐 7. API ENDPOINTS TÓM TẮT

| Endpoint | Method | Permission | Mô tả |
|---|---|---|---|
| `/api/auth/login` | POST | Public | Đăng nhập |
| `/api/auth/me` | GET | Auth | Info user |
| `/api/companies` | CRUD | HopDong.View | Quản lý công ty |
| `/api/healthcontracts` | CRUD | HopDong.View | HĐ |
| `/api/healthcontracts/{id}/approve` | POST | HopDong.Approve | Duyệt |
| `/api/medicalgroups` | CRUD | DoanKham.View | Đoàn khám |
| `/api/medicalgroups/{id}/ai-suggest-staff` | POST | DoanKham.View | AI Suggest |
| `/api/medicalgroups/{id}/qr-token` | GET | ChamCong.QR | Tạo QR |
| `/api/medicalgroups/{id}/export-staff` | GET | DoanKham.View | Excel NV |
| `/api/attendance/checkin` | POST | Public+Token | QR Check-in |
| `/api/staffs` | CRUD | NhanSu.View | Nhân viên |
| `/api/payroll` | GET/POST | Luong.View | Lương |
| `/api/supplies` | CRUD | Kho.View | Vật tư |
| `/api/supplies/vouchers` | CRUD | Kho.View | Phiếu XNK |
| `/api/patients` | CRUD | Auth | Bệnh nhân |
| `/api/medicalrecords` | GET/POST | Auth | Hồ sơ khám |
| `/api/examservices` | GET/POST | Auth | Dịch vụ y tế |
| `/api/reports/*` | GET | BaoCao.View | Báo cáo |

---

## ⚡ 8. LOGIC ĐẶC BIỆT

### AI Suggest Staff — Gemini API
```
POST /api/medicalgroups/{id}/ai-suggest-staff
  → Lấy danh sách Staff hiện có (tên, vị trí, ngày làm)
  → Build prompt yêu cầu format JSON thuần:
     [{staffId, staffName, workPosition, shiftType, reason}]
  → Gửi Gemini API → nhận JSON → Parse
  → Trả frontend preview → User confirm → Ghi GroupStaffDetail
```

### 2-Level Contract Approval
```
ContractApprovalStep: [Step1: Trưởng phòng] → [Step2: BGĐ]
Mỗi lần duyệt:
  → Ghi ContractApprovalHistory (ApprovedBy, Note, Timestamp)
  → currentApprovalStep++
  → Khi đủ steps → Status = Approved → chuyển Active
```

### QR Check-in Flow
```
GroupLeader bấm "Mở QR"
  → API tạo JWT payload: {groupId, examDate, expiresAt: +12h}
  → Frontend render QR Code từ URL /checkin?token=<jwt>
  → Nhân viên mở điện thoại, scan
  → Trang /checkin decode token → hiển thị tên đoàn
  → Bấm CHECK-IN → ghi ScheduleCalendar.CheckInTime
  → Bấm CHECK-OUT → ghi CheckOutTime + tính ShiftType
```
