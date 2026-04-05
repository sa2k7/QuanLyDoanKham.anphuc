# 🧪 BẢNG TEST CASE CHI TIẾT — QuanLyDoanKham (An Phúc)
> Phiên bản: 1.0 | Cập nhật: 2026-04-06 | **Tổng: 132 Test Cases / 12 Module**

---

## MODULE 01 — XÁC THỰC & ĐĂNG NHẬP (13 TC)

| TC# | Tên Test Case | Đầu vào | Bước thực hiện | Kết quả Mong đợi | Kết quả Thực tế | Pass/Fail |
|---|---|---|---|---|---|---|
| TC-001 | Đăng nhập thành công | user=admin, pass=admin123 | Mở /login → nhập → Đăng nhập | Redirect Dashboard, hiện tên user | | |
| TC-002 | Sai mật khẩu | pass=sai123 | Nhập sai → Đăng nhập | Lỗi "Sai mật khẩu", không chuyển trang | | |
| TC-003 | Username không tồn tại | user=khongton | Nhập → Đăng nhập | "Tài khoản không tồn tại" | | |
| TC-004 | Để trống Username | user="" | Để trống → Submit | Validate "Vui lòng nhập username" | | |
| TC-005 | Để trống Password | pass="" | Để trống → Submit | Validate "Vui lòng nhập mật khẩu" | | |
| TC-006 | Token JWT hết hạn | Token > 8h | Gọi API protected | 401 Unauthorized, redirect /login | | |
| TC-007 | Truy cập khi chưa login | Chưa có token | URL / trực tiếp | Redirect /login | | |
| TC-008 | Truy cập không có quyền | Role MedicalStaff vào /contracts | Đăng nhập → Truy cập | Redirect /forbidden | | |
| TC-009 | Đổi mật khẩu thành công | MK hiện tại đúng, MK mới hợp lệ | Profile → Đổi MK | "Thành công", bắt đăng xuất | | |
| TC-010 | Đổi MK sai MK hiện tại | MK hiện tại sai | Nhập sai → Submit | "Mật khẩu hiện tại không đúng" | | |
| TC-011 | MK mới trùng MK cũ | MK mới = MK cũ | Nhập trùng → Submit | Cảnh báo hoặc cho phép | | |
| TC-012 | Đăng xuất | Đang đăng nhập | Bấm Đăng xuất | Xóa token, redirect /login | | |
| TC-013 | TK bị vô hiệu hóa | IsActive=false | Nhập đúng credentials | "Tài khoản đã bị khóa" | | |

---

## MODULE 02 — HỢP ĐỒNG (21 TC)

| TC# | Tên Test Case | Đầu vào | Bước thực hiện | Kết quả Mong đợi | Kết quả Thực tế | Pass/Fail |
|---|---|---|---|---|---|---|
| TC-020 | Xem danh sách HĐ | Role: ContractManager | Vào menu Hợp đồng | Bảng DS HĐ, đủ cột: Mã, Công ty, Ngày, Trạng thái | | |
| TC-021 | Tạo HĐ mới hợp lệ | Đầy đủ thông tin | Thêm HĐ → form → Lưu | HĐ mới, Status=Draft | | |
| TC-022 | Tạo HĐ thiếu Công ty | CompanyId=null | Form thiếu → Lưu | "Vui lòng chọn công ty" | | |
| TC-023 | Tạo HĐ thiếu Ngày bắt đầu | StartDate=null | Form thiếu ngày → Lưu | Validate thất bại | | |
| TC-024 | EndDate < StartDate | End trước Start | Nhập sai thứ tự | "Ngày kết thúc phải sau ngày bắt đầu" | | |
| TC-025 | Số lượng âm | ExpectedQuantity=-1 | Nhập âm | Không cho phép | | |
| TC-026 | Đơn giá = 0 | UnitPrice=0 | Nhập 0 | Cho phép hoặc cảnh báo | | |
| TC-027 | Upload PDF đính kèm | File PDF < 10MB | Chọn PDF → Upload | Lưu thành công, hiển thị link | | |
| TC-028 | Upload sai định dạng | File .exe | Chọn .exe → Upload | "Chỉ nhận PDF, Word, Excel" | | |
| TC-029 | Gửi duyệt (Draft→Pending) | HĐ=Draft | Bấm "Gửi duyệt" | Status=PendingApproval | | |
| TC-030 | Duyệt Bước 1 | HopDong.Approve | Duyệt Bước 1 | Ghi History, chờ Bước 2 | | |
| TC-031 | Duyệt Bước 2 | Bước 1 đã xong | Duyệt Bước 2 | Status=Active | | |
| TC-032 | Từ chối HĐ | Có lý do | Từ chối → nhập lý do | Status=Rejected, ghi lý do | | |
| TC-033 | Từ chối không nhập lý do | Lý do trống | Từ chối → trống → Submit | Yêu cầu nhập lý do | | |
| TC-034 | Sửa HĐ Draft | Status=Draft | Sửa → Lưu | Dữ liệu cập nhật | | |
| TC-035 | Sửa HĐ Active | Status=Active | Thử sửa | "Không thể sửa HĐ đang hoạt động" | | |
| TC-036 | Xóa HĐ Draft | Status=Draft | Xóa → Xác nhận | HĐ bị xóa | | |
| TC-037 | Xóa HĐ có Đoàn Khám | Có MedicalGroup con | Xóa → Xác nhận | "Không thể xóa HĐ đã có đoàn khám" | | |
| TC-038 | Tìm HĐ theo mã | ContractCode | Gõ tìm kiếm | Đúng HĐ | | |
| TC-039 | Lọc theo trạng thái | Status=Active | Chọn filter | Chỉ Active | | |
| TC-040 | Xuất Excel DS HĐ | BaoCao.Export | Bấm Xuất | File .xlsx đủ cột | | |

---

## MODULE 03 — ĐOÀN KHÁM (22 TC)

| TC# | Tên Test Case | Đầu vào | Bước thực hiện | Kết quả Mong đợi | Kết quả Thực tế | Pass/Fail |
|---|---|---|---|---|---|---|
| TC-050 | Xem DS đoàn | Role: MedGrpManager | Vào Đoàn Khám | Bảng DS đoàn đủ cột | | |
| TC-051 | Tạo đoàn từ HĐ Active | HĐ=Active | Tạo → Chọn HĐ → Ngày → Lưu | Đoàn mới, Status=Planning | | |
| TC-052 | Tạo đoàn từ HĐ Draft | HĐ=Draft | Chọn HĐ Draft → Tạo | "Hợp đồng chưa được duyệt" | | |
| TC-053 | Tạo đoàn thiếu ngày khám | ExamDate=null | Để trống ngày | Validate | | |
| TC-054 | Ngày khám quá khứ | ExamDate < Today | Nhập ngày cũ | Cảnh báo "Ngày khám đã qua" | | |
| TC-055 | Thiết lập cơ cấu vị trí | Đoàn=Planning | Cơ cấu → Thêm → SL | GroupPositionQuota tạo | | |
| TC-056 | SL vị trí âm | Quota=-1 | Nhập âm | Không cho phép | | |
| TC-057 | Phân công NV thủ công | Có cơ cấu | Chọn NV → Vị trí → Thêm | GroupStaffDetail tạo | | |
| TC-058 | NV đã trong đoàn | NV đã phân công | Chọn lại → Thêm | "Nhân viên đã trong đoàn" | | |
| TC-059 | AI Suggest Staff thành công | Có NV trong DB | Bấm "Gợi ý AI" | DS gợi ý: tên + lý do | | |
| TC-060 | AI Suggest - Không có NV | DB trống NV | Bấm "Gợi ý AI" | "Không có nhân viên phù hợp" | | |
| TC-061 | Áp dụng đề xuất AI | Có DS gợi ý | Bấm "Áp dụng" | GroupStaffDetail theo AI | | |
| TC-062 | Xuất Excel DS nhân sự đoàn | Đoàn có NV | Bấm "Xuất Excel NV" | DSDOAN_*.xlsx | | |
| TC-063 | Xuất Excel tổng quan | Đoàn có dữ liệu | Bấm "Xuất Báo cáo" | THEODOIHDKSK.xlsx | | |
| TC-064 | Tạo QR Check-in | Role: GroupLeader | Bấm "Mở QR" | QR hiển thị, hết hạn sau 12h | | |
| TC-065 | QR Token hết hạn | Token > 12h | Quét QR cũ | "QR đã hết hạn" | | |
| TC-066 | Xóa NV khỏi đoàn | NV trong đoàn | Xóa → Xác nhận | GroupStaffDetail xóa | | |
| TC-067 | Đóng đoàn | Đoàn InProgress | Bấm "Kết thúc" | Status=Finished | | |
| TC-068 | Tìm đoàn theo tên | Keyword | Gõ tìm | Lọc đúng | | |
| TC-069 | Lọc đoàn theo ngày | From-To | Bộ lọc | DS trong khoảng ngày | | |
| TC-070 | Import BN từ Excel | File đúng format | Upload → Import | Patient records hàng loạt | | |
| TC-071 | Import file sai format | Thiếu cột | Upload sai | Lỗi cột thiếu | | |

---

## MODULE 04 — NHÂN SỰ (11 TC)

| TC# | Tên Test Case | Đầu vào | Bước thực hiện | Kết quả Mong đợi | Kết quả Thực tế | Pass/Fail |
|---|---|---|---|---|---|---|
| TC-080 | Xem DS nhân viên | Role: PersonnelMgr | Vào Nhân sự | Bảng DS NV đầy đủ | | |
| TC-081 | Thêm NV mới | Đủ thông tin | Điền form → Lưu | NV mới trong DS | | |
| TC-082 | Trùng mã NV | EmployeeCode tồn tại | Nhập trùng → Lưu | "Mã NV đã tồn tại" | | |
| TC-083 | Trống Họ tên | FullName=null | Để trống → Lưu | Validate bắt buộc | | |
| TC-084 | SĐT sai format | Phone="abc123" | Nhập sai → Lưu | Validate format | | |
| TC-085 | Sửa thông tin NV | NV tồn tại | Sửa → Lưu | Cập nhật thành công | | |
| TC-086 | Xóa NV không có lịch | Chưa có dữ liệu | Xóa → Xác nhận | NV bị xóa | | |
| TC-087 | Xóa NV đang trong đoàn | Có GroupStaffDetail | Xóa | "Không thể xóa NV đang tham gia đoàn" | | |
| TC-088 | Tìm NV theo tên | Keyword | Gõ tìm | Lọc đúng | | |
| TC-089 | Lọc NV theo phòng ban | DepartmentId | Chọn phòng | Chỉ NV phòng đó | | |
| TC-090 | Xem lịch làm việc NV | NV có lịch | Bấm "Xem lịch" | Lịch theo tháng | | |

---

## MODULE 05 — CHẤM CÔNG QR (9 TC)

| TC# | Tên Test Case | Đầu vào | Bước thực hiện | Kết quả Mong đợi | Kết quả Thực tế | Pass/Fail |
|---|---|---|---|---|---|---|
| TC-100 | Check-in lần 1 thành công | Token hợp lệ | /checkin?token=xxx → Check-in | Ghi CheckInTime, "Thành công" | | |
| TC-101 | Check-out sau Check-in | Đã Check-in | Quét lần 2 | Ghi CheckOutTime, tính ShiftType | | |
| TC-102 | Check-in 2 lần liên tiếp | Đã có CheckInTime | Quét lần 2 không Check-out | Báo "Đã Check-in rồi" hoặc tự Check-out | | |
| TC-103 | QR sai đoàn | Token đoàn khác | Quét không đúng | "QR không hợp lệ" | | |
| TC-104 | Check-in ngoài ngày | Ngoài ExamDate | Quét sai ngày | "Chưa/đã qua ngày khám" | | |
| TC-105 | Token giả mạo | Chữ ký sai | Nhập token giả | 401 Unauthorized | | |
| TC-106 | Xem lịch cá nhân | MedicalStaff | Vào "Lịch cá nhân" | Lịch tháng hiện tại | | |
| TC-107 | Xem lịch tháng khác | Filter tháng | Chọn tháng | Dữ liệu tháng đó | | |
| TC-108 | Admin xem toàn bộ | ChamCong.ViewAll | Báo cáo chấm công | Tất cả bản ghi | | |

---

## MODULE 06 — TÍNH LƯƠNG (9 TC)

| TC# | Tên Test Case | Đầu vào | Bước thực hiện | Kết quả Mong đợi | Kết quả Thực tế | Pass/Fail |
|---|---|---|---|---|---|---|
| TC-110 | Xem bảng lương tháng | Có dữ liệu chấm công | Vào Tính lương | Họ tên, Số ca, Tổng lương | | |
| TC-111 | Tính lương tháng mới | Có ScheduleCalendar | Bấm "Tính lương" | PayrollRecord cho từng NV | | |
| TC-112 | Ca ngày + nửa ngày | ShiftType mix 1.0/0.5 | Tính lương | Tổng = (Ca ngày × Wage) + (Nửa ngày × Wage × 0.5) | | |
| TC-113 | NV không có ca | 0 ScheduleCalendar | Tính | PayrollRecord TotalAmount=0 | | |
| TC-114 | Tính lại lương đã tính | Record tồn tại | Bấm lần 2 | "Đã tính tháng này, muốn tính lại?" | | |
| TC-115 | Lọc theo NV | StaffId | Chọn NV | Chỉ lương NV đó | | |
| TC-116 | Lọc theo tháng | Month | Chọn tháng | Đúng dữ liệu | | |
| TC-117 | Xuất Excel lương | Có PayrollRecord | Xuất | xlsx đủ cột, số liệu đúng | | |
| TC-118 | NV xem lương người khác | MedicalStaff | /api/payroll staffId khác | 403 Forbidden | | |

---

## MODULE 07 — KHO VẬT TƯ (8 TC)

| TC# | Tên Test Case | Đầu vào | Bước thực hiện | Kết quả Mong đợi | Kết quả Thực tế | Pass/Fail |
|---|---|---|---|---|---|---|
| TC-120 | Xem DS vật tư | WarehouseManager | Vào Vật tư kho | Bảng: tên, đơn vị, tồn kho | | |
| TC-121 | Thêm vật tư mới | SupplyName, Unit | Điền form → Lưu | Vật tư mới trong DS | | |
| TC-122 | Nhập kho thành công | Vật tư tồn tại, SL > 0 | Phiếu Nhập → Lưu | Tồn kho tăng | | |
| TC-123 | SL nhập âm | Quantity=-5 | Nhập âm | Validate | | |
| TC-124 | Xuất kho thành công | Tồn đủ | Phiếu Xuất → Gắn đoàn → Lưu | Tồn kho giảm | | |
| TC-125 | Xuất vượt tồn | Xuất > TonKho | Nhập vượt | "Không đủ hàng" | | |
| TC-126 | Xem tồn kho | Có Nhập/Xuất | Xem cột Tồn | = TổngNhập - TổngXuất | | |
| TC-127 | Tìm kiếm vật tư | Keyword | Gõ tìm | Đúng kết quả | | |

---

## MODULE 08 — BỆNH NHÂN B2C (7 TC)

| TC# | Tên Test Case | Đầu vào | Bước thực hiện | Kết quả Mong đợi | Kết quả Thực tế | Pass/Fail |
|---|---|---|---|---|---|---|
| TC-130 | Xem DS bệnh nhân | Có BN trong DB | Vào Bệnh nhân | Bảng: Họ tên, Giới tính, SĐT | | |
| TC-131 | Thêm BN hợp lệ | FullName bắt buộc | Điền form → Lưu | BN mới trong DS | | |
| TC-132 | BN thiếu Họ tên | FullName=null | Để trống → Lưu | Validate bắt buộc | | |
| TC-133 | Xóa BN không có hồ sơ | Không có Record | Xóa → Xác nhận | BN bị xóa | | |
| TC-134 | Xóa BN có hồ sơ | Có MedicalRecord | Xóa | Cảnh báo hoặc cascade | | |
| TC-135 | Tạo hồ sơ khám | BN + Đoàn tồn tại | Tạo hồ sơ → Lưu | MedicalRecord Status=CheckIn | | |
| TC-136 | Xem lịch sử khám | BN có nhiều Record | "Hồ sơ khám" | Danh sách theo thời gian | | |

---

## MODULE 09 — BÁO CÁO (6 TC)

| TC# | Tên Test Case | Đầu vào | Bước thực hiện | Kết quả Mong đợi | Kết quả Thực tế | Pass/Fail |
|---|---|---|---|---|---|---|
| TC-140 | Dashboard thống kê | Có dữ liệu | Mở trang chủ | Stat Cards: HĐ, Đoàn, NV, Doanh thu | | |
| TC-141 | Báo cáo tổng quan | Role: Accountant | Vào Báo cáo | Biểu đồ donut + line chart | | |
| TC-142 | Lọc theo tháng | Month filter | Chọn tháng | Dữ liệu tháng đó | | |
| TC-143 | Lọc theo năm | Year filter | Chọn năm | Dữ liệu năm đó | | |
| TC-144 | Xuất Excel DS Đoàn | BaoCao.Export | Xuất Excel | xlsx header: Mã HĐ, Ngày, Cty, SL | | |
| TC-145 | Không có quyền | MedicalStaff | Vào /analytics | 403 hoặc redirect | | |

---

## MODULE 10 — PHÂN QUYỀN (8 TC)

| TC# | Tên Test Case | Đầu vào | Bước thực hiện | Kết quả Mong đợi | Kết quả Thực tế | Pass/Fail |
|---|---|---|---|---|---|---|
| TC-150 | Xem DS người dùng | Admin | Vào Tài khoản | DS user + role + trạng thái | | |
| TC-151 | Tạo tài khoản mới | Đủ thông tin | Điền → Lưu | TK mới, MK BCrypt | | |
| TC-152 | Trùng username | Username tồn tại | Nhập trùng | "Username đã tồn tại" | | |
| TC-153 | Vô hiệu hóa TK | IsActive=true | Toggle → false | Không đăng nhập được | | |
| TC-154 | Gán thêm quyền | Admin | Phân quyền → Thêm Permission | Quyền có ngay | | |
| TC-155 | Thu hồi quyền | Role có Permission | Bỏ chọn → Lưu | 403 với quyền đó | | |
| TC-156 | Manager không có quyền Admin | ContractManager | HeThong.UserManage | 403 Forbidden | | |
| TC-157 | Reset mật khẩu | Admin | Đặt lại MK cho user | User nhận MK mới | | |

---

## MODULE 11 — BẢO MẬT & EDGE CASES (8 TC)

| TC# | Tên Test Case | Đầu vào | Bước thực hiện | Kết quả Mong đợi | Kết quả Thực tế | Pass/Fail |
|---|---|---|---|---|---|---|
| TC-160 | SQL Injection | `' OR 1=1 --` | Nhập vào ô search | Xử lý an toàn, không lộ dữ liệu | | |
| TC-161 | XSS | `<script>alert(1)</script>` | Nhập vào ô tên | Script không chạy, hiển thị text | | |
| TC-162 | Gọi API không có token | Token trống | GET /api/medicalgroups | 401 Unauthorized | | |
| TC-163 | Token giả mạo | Token random | Header token sai | 401 Unauthorized | | |
| TC-164 | CORS domain khác | Origin lạ | Gọi từ evil.com | CORS blocked | | |
| TC-165 | File upload quá lớn | File 50MB | Upload | "File quá lớn" | | |
| TC-166 | 2 user đồng thời sửa | 2 session song song | Cùng sửa 1 HĐ | Concurrency xử lý đúng | | |
| TC-167 | Unicode / Emoji | FullName có emoji | Nhập | Lưu và hiển thị đúng | | |

---

## MODULE 12 — HIỆU NĂNG & UI/UX (10 TC)

| TC# | Tên Test Case | Đầu vào | Bước thực hiện | Kết quả Mong đợi | Kết quả Thực tế | Pass/Fail |
|---|---|---|---|---|---|---|
| TC-170 | Tải Dashboard < 3 giây | Production | Mở Dashboard | < 3s | | |
| TC-171 | Sidebar Collapse/Expand | Desktop | Bấm thu gọn | Thu gọn + icon vẫn hiện | | |
| TC-172 | Chuyển ngôn ngữ EN/VI | i18n | Toggle EN/VI | Toàn bộ text chuyển | | |
| TC-173 | Responsive Mobile | Viewport 375px | Thu nhỏ browser | Sidebar = hamburger, fit màn hình | | |
| TC-174 | Toast Notification | Sau thao tác | Tạo/Sửa/Xóa | Toast xanh tự hiện và biến | | |
| TC-175 | Confirm Dialog khi Xóa | Bấm xóa | Click xóa | Popup "Bạn có chắc?" | | |
| TC-176 | Empty State | Module trống | Vào module rỗng | Header bảng + "Không có dữ liệu" | | |
| TC-177 | Loading Spinner | API chậm | Gọi API lag | Spinner hiển thị khi chờ | | |
| TC-178 | Phân trang | > 20 items | Bảng nhiều bản ghi | Phân trang đúng | | |
| TC-179 | Sắp xếp cột | Cột có sort | Bấm tên cột | Tăng/giảm hoán đổi | | |

---

## 📊 TỔNG KẾT

| # | Module | Số TC |
|---|---|---|
| 01 | Xác thực & Đăng nhập | 13 |
| 02 | Hợp Đồng | 21 |
| 03 | Đoàn Khám | 22 |
| 04 | Nhân Sự | 11 |
| 05 | Chấm Công QR | 9 |
| 06 | Tính Lương | 9 |
| 07 | Kho Vật Tư | 8 |
| 08 | Bệnh Nhân B2C | 7 |
| 09 | Báo Cáo | 6 |
| 10 | Phân Quyền | 8 |
| 11 | Bảo Mật & Edge | 8 |
| 12 | UI/UX & Hiệu Năng | 10 |
| | **TỔNG CỘNG** | **132** |
