# Phân tích đầy đủ yêu cầu phân quyền, hợp đồng, đoàn khám và vận hành

Ngày cập nhật: 2026-04-02

## 1. Mục tiêu tài liệu

Tài liệu này chuẩn hóa toàn bộ note thô thành một bộ yêu cầu có cấu trúc để đội phát triển bám theo khi thiết kế chức năng.

Mục tiêu chính:

- Không bỏ sót bất kỳ ý nào trong note gốc.
- Tách rõ ai làm gì, thuộc phòng nào, được xem gì, được thao tác gì.
- Chuẩn hóa chu trình từ công ty -> hợp đồng -> đoàn khám -> nhân sự -> vật tư -> chi phí -> lương -> báo cáo.
- Làm rõ nguyên tắc `admin không được làm hết`.
- Biến note gốc thành backlog/todo có thể triển khai tiếp.

## 2. Nguyên tắc nghiệp vụ bắt buộc

- `Admin hệ thống` không được mặc định làm toàn bộ nghiệp vụ. Admin chỉ nên quản lý tài khoản, role, permission, danh mục, cấu hình, audit.
- Nếu một tài khoản admin cần làm nghiệp vụ cụ thể thì vẫn phải được gán thêm role nghiệp vụ tương ứng; không được dùng quyền admin để bỏ qua chu trình.
- Khối `Hành chính - Nhân sự` là đầu mối cho phần ký kết, tạo hợp đồng, quản lý nhân sự, tính lương; nếu cần kiêm thêm phần kho thì vẫn phải tách permission rõ ràng.
- Phải có cơ chế `phân quyền theo role` và `phân quyền theo chức năng`.
- Một `user` có thể có nhiều `role/quyền/chức năng`.
- Luồng chuẩn là `tạo user trước`, sau đó mới `gán role/quyền` cho user đó.
- Mọi bước quan trọng phải lưu `ai tạo`, `ai duyệt`, `ai phụ trách`, `thời gian thực hiện`.
- Đoàn khám chỉ được tạo sau khi hợp đồng đã được duyệt.
- Ngày của đoàn khám phải nằm trong khoảng ngày bắt đầu khám và ngày kết thúc khám của hợp đồng.
- Phải quản lý được cả `chu trình` và `chi tiêu` của hợp đồng sau khi hợp đồng đã ký và được đưa vào phần mềm.

## 3. Bài toán tổng thể

Quy trình nghiệp vụ được hiểu theo chuỗi sau:

1. Tạo và quản lý công ty.
2. Ký kết và quản lý hợp đồng với công ty.
3. Duyệt hợp đồng theo cấp độ phê duyệt.
4. Dựa trên ngày khám của hợp đồng để sinh ra các đoàn khám theo từng ngày.
5. Thiết lập vị trí trong từng đoàn khám.
6. Gán nhân sự phù hợp vào từng vị trí của từng đoàn khám.
7. Quản lý lịch khám để nhân viên xem được lịch của mình.
8. Quản lý vật tư theo kho và theo đoàn khám bằng phiếu nhập/phiếu xuất.
9. Quản lý chấm công, check-in/check-out, QR của trưởng đoàn.
10. Tính lương theo ngày công thực tế.
11. Tổng hợp chi phí nhân sự, vật tư, chi phí khác để ra doanh thu/lợi nhuận.
12. Phân tích báo cáo theo ngày, tuần, tháng, quý.

## 4. Danh mục module bắt buộc

### 4.1. Quản lý công ty

- Tạo/sửa/xóa công ty.
- Lưu thông tin cơ bản của công ty.
- Một công ty có thể có nhiều hợp đồng.

### 4.2. Quản lý hợp đồng

- Tạo hợp đồng từ phía `Hành chính - Nhân sự`.
- Xác định rõ `ai tạo hợp đồng`, `ai phê duyệt hợp đồng`, `ai ký kết`.
- Cho phép đính kèm file hợp đồng sau khi ký.
- Quản lý ngày bắt đầu khám, ngày kết thúc khám.
- Khóa ràng buộc: đoàn khám phải bám theo ngày của hợp đồng.
- Chỉ sau khi hợp đồng được duyệt mới được tạo đoàn khám.

### 4.3. Cấp độ phê duyệt hợp đồng

- Hợp đồng phải có nhiều bước/phân cấp duyệt nếu doanh nghiệp yêu cầu.
- Tối thiểu phải có:
- Người tạo hợp đồng.
- Người duyệt hợp đồng.
- Lịch sử duyệt hợp đồng.
- Trạng thái hợp đồng: nháp, chờ duyệt, đã duyệt, từ chối, đang thực hiện, hoàn tất.
- Cần log đầy đủ ngày giờ và người thao tác.

### 4.4. Quản lý đoàn khám

- Tạo đoàn khám từ hợp đồng đã duyệt.
- Một hợp đồng có thể sinh ra nhiều đoàn khám theo từng ngày khám.
- Ví dụ: hợp đồng khám từ `01/04` đến `02/04` thì phải có thể sinh ra `Đoàn khám ngày 01/04` và `Đoàn khám ngày 02/04`.
- Lưu `ai tạo đoàn khám`.
- Lưu `user quản lý đoàn khám`.
- Có thể tồn tại trạng thái đoàn khám mới tạo nhưng `chưa có vị trí`.

### 4.5. Thiết lập vị trí trong đoàn khám

- Mỗi đoàn khám phải khai báo được có bao nhiêu vị trí.
- Mỗi vị trí phải khai báo được cần bao nhiêu người.
- Ví dụ vị trí:
- Tiếp nhận.
- Khám nội.
- Khám ngoại.
- Lấy máu.
- Khám sản phụ khoa.
- Siêu âm.
- Chỉ khi đã có vị trí thì mới phân công nhân sự vào đoàn khám được.

### 4.6. Phân công nhân sự vào đoàn khám

- Phải biết nhân viên thuộc phòng nào, chuyên môn nào, chức năng nào.
- Gán nhân sự đúng vị trí trong đoàn khám.
- Vai trò phụ trách phải có khả năng `đẩy nhân sự lên vị trí trong đoàn khám`.
- Cần biết rõ ai là người:
- Tạo đoàn khám.
- Thiết lập vị trí.
- Gán nhân sự.
- Quản lý đoàn khám.

### 4.7. Quản lý lịch khám

- Khi tạo đoàn khám phải có danh sách lịch khám tương ứng.
- Phải có module hoặc chức năng con tên `Lịch khám`.
- Chức năng `Lịch khám` phải xem được theo ngày, tuần, tháng.
- Người xem có thể biết trong một ngày/tuần/tháng có bao nhiêu đoàn khám.
- Nhân viên chỉ nhìn thấy lịch phù hợp với quyền của mình.
- Nhân viên đi khám chỉ được thấy lịch khám của chính họ.

### 4.8. Quản lý kho và vật tư

- Quản lý vật tư theo kho.
- Mỗi đoàn khám phải gắn được vật tư sử dụng theo đoàn.
- Có `phiếu nhập`.
- Có `phiếu xuất`.
- Biết vật tư nhập kho là gì.
- Biết vật tư xuất cho đoàn khám nào.
- Theo dõi số lượng đầu kỳ.
- Theo dõi số lượng nhập.
- Theo dõi số lượng xuất.
- Theo dõi số lượng tồn.
- Cảnh báo thiếu hoặc dư vật tư.

### 4.9. Quản lý chi phí và doanh thu

- Tính chi phí đoàn khám.
- Chi phí gồm:
- Chi phí nhân sự.
- Chi phí vật tư.
- Chi phí khác.
- Từ chi phí từng đoàn khám phải tổng hợp ra doanh thu/lợi nhuận cuối cùng của hợp đồng.
- Việc tính chi phí phụ thuộc vào dữ liệu nhân sự, tính lương, quản lý kho.

### 4.10. Quản lý người dùng, role, permission

- Luôn luôn phải có quản lý người dùng.
- Luôn luôn phải có quản lý phân quyền người dùng.
- Phải có ít nhất một role/phân quyền cho từng nhóm chức năng.
- Quyền phải kiểm soát được việc sử dụng các module:
- Hợp đồng.
- Quản lý đoàn khám.
- Kế toán.
- Quản lý vật tư.
- Quản lý thống kê.
- Quản lý tiến độ.
- Tiến độ đoàn khám.
- Quản lý kho.

### 4.11. Quản lý chấm công, check-in/check-out, QR

- Trưởng đoàn khám phải có chức năng chấm công cho nhân viên trong đoàn.
- Nếu user có vai trò `Trưởng đoàn khám` thì phải hiện được mã QR để nhân viên khác quét.
- Sau khi quét QR phải tự động cập nhật vào cơ sở dữ liệu:
- Giờ vào.
- Giờ ra.
- Công nửa ngày hoặc một ngày.
- Phải hỗ trợ tính số ngày công nhân viên đi khám trong tháng.
- Có thể triển khai thêm hoặc song song giao diện `2 cột check-in/check-out`.
- QR dùng để ghi nhận giờ vào/giờ ra thực tế.

### 4.12. Quản lý tính lương

- Tính lương dựa trên ngày công thực tế.
- Ghi nhận số ngày đi khám thực tế trong tháng.
- Ví dụ mong muốn trong note gốc: lương cơ bản nhân với ngày công để ra lương thực tế.
- Dữ liệu nguồn phải lấy từ chấm công thực tế.

### 4.13. Quản lý báo cáo và phân tích

- Báo cáo 1 tháng có bao nhiêu đoàn khám.
- Báo cáo 1 quý có bao nhiêu đoàn khám.
- Báo cáo số tiền/doanh số tương ứng.
- Cần tổng hợp được chi phí và doanh thu theo đoàn khám, theo hợp đồng, theo tháng, theo quý.
- Chức năng lịch khám cũng phải đóng vai trò phân tích sản lượng đoàn khám theo ngày/tuần/tháng.

### 4.14. AI hỗ trợ tạo đoàn khám

- Hệ thống phải có phương án để AI tạo tất cả đoàn khám theo hợp đồng.
- AI phải dựa trên ngày bắt đầu khám và ngày kết thúc khám của hợp đồng để sinh ra danh sách đoàn khám.
- AI có thể tạo luôn chi tiết từng đoàn khám.
- AI phải đẩy kết quả vào danh sách đoàn khám.
- AI phải hỗ trợ xuất danh sách đoàn khám.
- Cần có bước xác nhận/kiểm duyệt kết quả AI trước khi đưa vào vận hành chính thức.

## 5. Ma trận vai trò và phòng ban

## 5.1. Tách `phòng ban` và `role`

- `Phòng ban` dùng để biết nhân viên thuộc bộ phận nào.
- `Role/permission` dùng để biết nhân viên được làm chức năng gì trong hệ thống.
- Một nhân viên có thể thuộc một phòng ban nhưng có nhiều role.

Ví dụ phòng ban:

- Hành chính - Nhân sự.
- Kho/Vật tư.
- Điều hành đoàn khám.
- Kế toán.
- Thống kê/Báo cáo.
- Nhân viên đi khám.

## 5.2. Vai trò đề xuất theo note gốc

| Vai trò | Phòng/Bộ phận thường thuộc | Chức năng chính | Giới hạn bắt buộc |
| --- | --- | --- | --- |
| Admin hệ thống | IT/Hệ thống | Quản lý user, role, permission, danh mục, cấu hình, audit | Không mặc định thay thế toàn bộ nghiệp vụ |
| Nhân viên hành chính - nhân sự | Hành chính - Nhân sự | Tạo hợp đồng, quản lý hồ sơ nhân sự, tính lương, theo dõi ký kết, đính kèm file hợp đồng | Không tự duyệt hợp đồng của chính mình nếu quy trình cần tách người duyệt |
| Người duyệt hợp đồng | Hành chính - Nhân sự / quản lý | Duyệt hợp đồng, xác nhận ký kết, mở quyền tạo đoàn khám | Phải khác hoặc tách vai trò với người tạo hợp đồng theo quy trình |
| Quản lý đoàn khám / điều phối | Điều hành đoàn khám | Tạo đoàn khám từ hợp đồng đã duyệt, thiết lập vị trí, phân công nhân sự, theo dõi tiến độ đoàn | Không được tạo đoàn nếu hợp đồng chưa duyệt |
| Trưởng đoàn khám | Điều hành đoàn khám / nhân sự đi đoàn | Quản lý nhân sự trong đoàn, mở QR chấm công, xác nhận check-in/check-out, theo dõi ngày công thực tế | Chỉ quản lý đoàn được phân công |
| Nhân viên đi khám | Nhân sự đi đoàn | Xem lịch của mình, check-in/check-out, tham gia đoàn khám theo vị trí được phân công | Chỉ thấy lịch của mình, không thấy toàn bộ dữ liệu hệ thống |
| Nhân viên kho | Kho/Vật tư | Lập phiếu nhập, phiếu xuất, theo dõi tồn kho, cấp vật tư cho đoàn khám | Không tự ý thay đổi dữ liệu nhân sự/hợp đồng nếu không có quyền bổ sung |
| Kế toán | Kế toán | Tổng hợp chi phí, doanh thu, đối soát hợp đồng và đoàn khám | Không chỉnh phân công nhân sự nếu không có role phù hợp |
| Nhân viên thống kê / báo cáo | Thống kê/Báo cáo | Xem dashboard, báo cáo tháng/quý, tổng hợp số tiền, phân tích sản lượng đoàn khám | Chỉ xem dữ liệu được cấp quyền |

## 5.3. Các rule phân quyền cốt lõi

- Một user có thể có rất nhiều quyền/chức năng.
- Một chức năng có thể cần nhiều quyền nhỏ hơn, ví dụ:
- `Hợp đồng.View`
- `Hợp đồng.Create`
- `Hợp đồng.Approve`
- `ĐoànKhám.Create`
- `ĐoànKhám.AssignStaff`
- `Kho.Import`
- `Kho.Export`
- `LịchKhám.ViewOwn`
- `LịchKhám.ViewAll`
- `ChấmCông.CheckInOut`
- `BáoCáo.View`
- Phải có phân quyền theo `menu`, `API`, `dữ liệu`, `hành động`.

## 6. Chu trình nghiệp vụ chuẩn

## 6.1. Chu trình phân quyền tài khoản

1. Tạo user.
2. Gán phòng ban cho user.
3. Gán một hoặc nhiều role cho user.
4. Gán các permission chi tiết nếu cần.
5. Kiểm tra user được thấy menu nào, dữ liệu nào, thao tác nào.
6. Ví dụ:
7. Nhân viên đi khám chỉ thấy lịch khám của chính họ.
8. Trưởng đoàn chỉ thấy các đoàn mình phụ trách.
9. Quản lý đoàn khám thấy danh sách đoàn khám do mình phụ trách hoặc toàn bộ nếu được cấp.

## 6.2. Chu trình hợp đồng

1. Tạo công ty.
2. Nhân viên hành chính - nhân sự tạo hợp đồng.
3. Nhập ngày bắt đầu khám, ngày kết thúc khám, giá trị hợp đồng.
4. Đính kèm file hợp đồng đã ký hoặc bản scan liên quan.
5. Gửi hợp đồng đi duyệt.
6. Người duyệt hợp đồng phê duyệt theo cấp độ được cấu hình.
7. Sau khi duyệt xong, hợp đồng mới mở quyền sinh đoàn khám.
8. Hệ thống lưu ai tạo, ai duyệt, khi nào duyệt.

## 6.3. Chu trình tạo đoàn khám từ hợp đồng

1. Chỉ chọn hợp đồng đã duyệt.
2. Dựa trên ngày bắt đầu khám và ngày kết thúc khám để tạo danh sách đoàn khám.
3. Có thể tạo bằng tay hoặc tạo tự động bằng AI.
4. Mỗi ngày khám tương ứng với một đoàn khám hoặc một bản ghi lịch đoàn khám.
5. Đoàn khám mới tạo có thể ở trạng thái `chưa thiết lập vị trí`.
6. Phải lưu:
7. Hợp đồng nguồn.
8. Ngày khám.
9. User tạo đoàn khám.
10. User quản lý đoàn khám.

## 6.4. Chu trình thiết lập vị trí và gán nhân sự

1. Xác định đoàn khám có những vị trí nào.
2. Nhập số lượng người cần cho từng vị trí.
3. Chỉ sau bước này mới được gán nhân sự.
4. Chọn nhân viên phù hợp theo phòng ban, chuyên môn, chức năng.
5. Đẩy nhân viên vào đúng vị trí trong đoàn khám.
6. Theo dõi trạng thái thiếu người, đủ người, dư người.

## 6.5. Chu trình lịch khám

1. Khi tạo đoàn khám phải tự động sinh dữ liệu lịch khám.
2. Có màn hình `Lịch khám` riêng.
3. Có bộ lọc theo ngày, tuần, tháng.
4. Có thống kê số lượng đoàn khám theo từng mốc thời gian.
5. Nhân viên vào lịch khám để biết mình có lịch đi hay không.
6. Nhân viên chỉ xem lịch phù hợp với quyền của mình.

## 6.6. Chu trình vật tư theo đoàn khám

1. Ghi nhận vật tư nhập kho.
2. Ghi nhận vật tư xuất cho từng đoàn khám.
3. Theo dõi tồn đầu kỳ, nhập, xuất, tồn cuối kỳ.
4. Biết đoàn khám nào đang dùng vật tư nào.
5. Từ dữ liệu vật tư phải tính được chi phí vật tư của từng đoàn khám.

## 6.7. Chu trình chấm công và tính lương

1. Trưởng đoàn mở QR hoặc công cụ check-in/check-out.
2. Nhân viên quét QR hoặc cập nhật check-in/check-out.
3. Hệ thống lưu giờ vào, giờ ra cho từng nhân viên trong từng đoàn khám.
4. Hệ thống quy đổi sang nửa ngày hoặc một ngày công theo rule nghiệp vụ.
5. Cuối tháng tổng hợp ngày công thực tế.
6. Tính lương thực tế từ dữ liệu ngày công.

## 6.8. Chu trình chi phí, doanh số, báo cáo

1. Tập hợp chi phí nhân sự từ dữ liệu lương/chấm công.
2. Tập hợp chi phí vật tư từ phiếu xuất kho và vật tư dùng cho đoàn khám.
3. Cộng thêm chi phí khác nếu có.
4. Tính tổng chi phí từng đoàn khám.
5. Tổng hợp chi phí các đoàn khám thuộc cùng hợp đồng.
6. So sánh với giá trị hợp đồng để ra doanh thu/lợi nhuận cuối cùng.
7. Xuất báo cáo theo ngày, tháng, quý.

## 7. Ràng buộc dữ liệu và rule kiểm tra

- Không cho tạo đoàn khám từ hợp đồng chưa duyệt.
- Không cho tạo đoàn khám ngoài khoảng ngày của hợp đồng.
- Ngày đoàn khám và ngày hợp đồng phải khớp logic với nhau.
- Không cho gán nhân sự vào đoàn khám nếu đoàn khám chưa có vị trí.
- Không cho user không có quyền nhìn thấy dữ liệu không thuộc phạm vi của họ.
- Nhân viên đi khám chỉ được thấy lịch của chính họ.
- Người tạo hợp đồng và người duyệt hợp đồng nên được tách riêng trong quy trình chuẩn.
- Mọi thay đổi quan trọng cần có audit log.
- Mọi đối tượng chính cần có `CreatedBy`, `CreatedAt`, `UpdatedBy`, `UpdatedAt`.

## 8. Thực thể dữ liệu nên có

- Company
- HealthContract
- ContractApprovalStep
- ContractApprovalHistory
- ContractAttachment
- MedicalGroup
- MedicalGroupPosition
- MedicalGroupStaffAssignment
- Department
- Staff
- AppUser
- Role
- Permission
- UserRole
- RolePermission
- ScheduleCalendar
- Supply
- WarehouseVoucher
- WarehouseVoucherDetail
- Attendance
- Payroll
- GroupCost
- GroupRevenueSummary
- ContractRevenueSummary

## 9. Backlog/TODO triển khai đề xuất

## 9.1. Nền tảng phân quyền

- [ ] Tạo mô hình `Role`, `Permission`, `UserRole`, `RolePermission`.
- [ ] Tách quyền theo chức năng thay vì chỉ một cột role đơn.
- [ ] Cho phép một user có nhiều role.
- [ ] Tạo màn hình tạo user trước, gán quyền sau.
- [ ] Tạo rule `Admin không mặc định làm toàn bộ nghiệp vụ`.

## 9.2. Hợp đồng và phê duyệt

- [ ] Bổ sung cấp độ phê duyệt hợp đồng.
- [ ] Lưu người tạo hợp đồng.
- [ ] Lưu người duyệt hợp đồng.
- [ ] Lưu lịch sử trạng thái hợp đồng.
- [ ] Cho phép upload file hợp đồng đính kèm.
- [ ] Chặn tạo đoàn khám nếu hợp đồng chưa duyệt.

## 9.3. Đoàn khám và vị trí

- [ ] Tạo cơ chế sinh nhiều đoàn khám từ 1 hợp đồng theo từng ngày khám.
- [ ] Cho phép tạo đoàn khám ở trạng thái chưa có vị trí.
- [ ] Tạo danh mục vị trí đoàn khám.
- [ ] Khai báo số lượng người cho từng vị trí.
- [ ] Chỉ cho phép phân công nhân sự sau khi đã thiết lập vị trí.
- [ ] Ghi nhận người tạo và người quản lý đoàn khám.

## 9.4. Nhân sự và phân công

- [ ] Gắn nhân viên với phòng ban.
- [ ] Gắn nhân viên với chuyên môn/chức năng.
- [ ] Xây dựng màn hình phân công nhân sự theo vị trí đoàn khám.
- [ ] Kiểm tra trùng lịch hoặc quá tải nhân sự.
- [ ] Tạo quyền riêng cho quản lý đoàn khám và trưởng đoàn khám.

## 9.5. Lịch khám

- [ ] Tự động sinh lịch khám khi tạo đoàn khám.
- [ ] Tạo màn hình lịch theo ngày/tuần/tháng.
- [ ] Có thống kê số đoàn khám theo ngày/tuần/tháng.
- [ ] Tạo màn hình `lịch của tôi` cho nhân viên đi khám.

## 9.6. Kho và vật tư

- [ ] Tạo phiếu nhập kho.
- [ ] Tạo phiếu xuất kho theo đoàn khám.
- [ ] Theo dõi đầu kỳ, nhập, xuất, tồn.
- [ ] Tính chi phí vật tư theo từng đoàn khám.
- [ ] Cảnh báo vật tư thiếu.

## 9.7. Chấm công và lương

- [ ] Tạo check-in/check-out cho nhân viên đi đoàn.
- [ ] Tạo QR cho trưởng đoàn khám.
- [ ] Lưu giờ vào/giờ ra thực tế.
- [ ] Quy đổi công nửa ngày/một ngày.
- [ ] Tính lương theo ngày công thực tế.

## 9.8. Chi phí, doanh thu, báo cáo

- [ ] Tính chi phí nhân sự theo từng đoàn khám.
- [ ] Tính chi phí vật tư theo từng đoàn khám.
- [ ] Tính chi phí khác theo từng đoàn khám.
- [ ] Tổng hợp doanh số/doanh thu theo hợp đồng.
- [ ] Báo cáo tháng số đoàn khám và số tiền.
- [ ] Báo cáo quý số đoàn khám và số tiền.

## 9.9. AI hỗ trợ

- [ ] Cho AI sinh danh sách đoàn khám theo hợp đồng.
- [ ] Cho AI sinh chi tiết đoàn khám theo ngày.
- [ ] Cho AI xuất danh sách đoàn khám.
- [ ] Bổ sung bước xác nhận kết quả AI trước khi lưu chính thức.

## 10. Các điểm cần chốt thêm khi đi vào thiết kế chi tiết

- Cấp độ phê duyệt hợp đồng là 2 cấp hay nhiều hơn.
- Công thức tính lương chính xác là:
- Lương cơ bản nhân ngày công.
- Hay lương cơ bản chia công chuẩn rồi nhân công thực tế.
- Khối Nhân sự có trực tiếp quản lý kho hay chỉ phối hợp với bộ phận kho.
- AI được phép tự lưu đoàn khám ngay hay chỉ đề xuất để người dùng duyệt.
- Một ngày có thể có nhiều đoàn khám cho cùng một hợp đồng hay mặc định 1 ngày = 1 đoàn.

## 11. Checklist đối chiếu note gốc để bảo đảm không sót ý

- [x] Admin không được làm hết.
- [x] Chu trình phân quyền phải rõ ràng.
- [x] Ký kết là phải bên nhân sự.
- [x] Nhân sự làm hợp đồng và tính lương.
- [x] Nhân sự quản lý tất cả nhân sự, hợp đồng, tính lương, quản lý kho hoặc phối hợp kho theo quyền.
- [x] Phải xác định nhân viên thuộc phòng nào và làm chức năng nào.
- [x] Phải phân biệt chức năng của phòng nhân sự, kho, quản lý đoàn khám, thống kê báo cáo.
- [x] Phải có phân tích báo cáo theo tháng và quý, gồm số đoàn khám và số tiền.
- [x] Một đoàn khám có nhiều vị trí và mỗi vị trí có số lượng người riêng.
- [x] Ví dụ vị trí phải bao gồm tiếp nhận, khám nội, khám ngoại, lấy máu, khám sản phụ khoa, siêu âm.
- [x] Hợp đồng phải có cấp độ phê duyệt, biết ai tạo và ai phê duyệt.
- [x] Ngày của đoàn khám và hợp đồng phải khớp với nhau.
- [x] Cần làm rõ ai tự động tạo đoàn khám khi đoàn khám chưa có vị trí.
- [x] Tách hai công việc: tạo đoàn khám và tạo nhân sự trong đoàn khám.
- [x] Trước khi đưa nhân sự vào đoàn khám phải thiết lập vị trí.
- [x] Vai trò phụ trách phải có chức năng đẩy nhân sự vào vị trí trong đoàn khám.
- [x] Chu trình đoàn khám bắt đầu từ công ty.
- [x] Sau công ty là ký hợp đồng với công ty đó.
- [x] Phải rõ ai tạo hợp đồng, ai duyệt hợp đồng, ai ký hợp đồng.
- [x] Nhân viên hành chính nhân sự là đầu mối cho nghiệp vụ hợp đồng.
- [x] Chỉ sau khi hợp đồng được duyệt mới tạo ra đoàn khám.
- [x] Phải xác định ai quản lý đoàn khám và ai tạo đoàn khám.
- [x] Trong chi tiết đoàn khám, chỉ thêm được nhân sự sau khi đã có vị trí.
- [x] Đoàn khám phải có vật tư theo đoàn khám.
- [x] Vật tư phải có phiếu nhập và phiếu xuất.
- [x] Phải biết vật tư nhập kho là gì và xuất cho đoàn khám nào.
- [x] Phải theo dõi số lượng đầu kỳ, xuất, tồn để biết còn hay thiếu.
- [x] Phải có chi phí đoàn khám.
- [x] Phải quản lý role của từng nhân viên và quản lý phân quyền hệ thống.
- [x] Phải có ít nhất một role/phân quyền rõ ràng cho hệ thống.
- [x] Phải quản lý lịch đi của đoàn khám để nhân viên xem được lịch.
- [x] Phải tính chi phí của 1 đoàn khám gồm nhân sự, vật tư và chi phí khác để ra doanh thu cuối cùng.
- [x] Luôn luôn phải có quản lý người dùng và phân quyền người dùng.
- [x] Quyền phải bao trùm hợp đồng, đoàn khám, kế toán, vật tư, thống kê, tiến độ, kho.
- [x] Ký hợp đồng xong phải đính kèm file vào hệ thống.
- [x] Hệ thống chỉ quản lý chu trình và chi tiêu của hợp đồng đã ký trong phần mềm.
- [x] Dựa trên ngày bắt đầu và ngày kết thúc khám để sinh đoàn khám theo từng ngày.
- [x] Từ từng đoàn khám phải tính được chi phí nhân sự, chi phí vật tư và doanh số của hợp đồng.
- [x] Việc tính chi phí phụ thuộc quản lý nhân sự, tính lương và quản lý kho.
- [x] Phân quyền tài khoản phải cho phép nhân viên đi khám chỉ thấy lịch của mình.
- [x] Phải có quản lý lịch khám.
- [x] Khi tạo đoàn khám sẽ có danh sách lịch khám.
- [x] Phải có chức năng con `Lịch khám` để xem theo ngày, tuần, tháng có bao nhiêu đoàn khám.
- [x] Dựa trên lịch khám để cho nhân viên thấy lịch đi của họ.
- [x] Phải có phương án cho AI tạo tất cả đoàn khám theo hợp đồng.
- [x] AI có thể tạo chi tiết từng đoàn khám và đẩy vào danh sách đoàn khám.
- [x] AI phải hỗ trợ xuất danh sách đoàn khám.
- [x] Một user có thể có rất nhiều quyền/chức năng.
- [x] Phải tạo user trước rồi mới phân quyền cho user đó.
- [x] Tính lương dựa trên ngày công thực tế.
- [x] Trưởng đoàn khám phải có chức năng tính công/chấm công cho người trong đoàn.
- [x] Trưởng đoàn phải hiện được mã QR cho nhân viên quét.
- [x] Sau khi quét QR phải cập nhật giờ vào, giờ ra vào cơ sở dữ liệu.
- [x] Từ giờ vào/giờ ra phải quy đổi được nửa ngày hoặc một ngày công.
- [x] Từ ngày công phải tính được lương nhân viên đi khám trong tháng.
- [x] Có thể triển khai thêm 2 cột check-in và check-out cho nhân viên.

## 12. Kết luận ngắn

Đây là bài toán `quản trị vận hành đoàn khám theo hợp đồng`, trong đó phần khó nhất không phải chỉ là CRUD dữ liệu mà là `chu trình phân quyền`, `chu trình sinh đoàn khám từ hợp đồng`, `phân công nhân sự theo vị trí`, `tính chi phí theo đoàn`, `chấm công thực tế` và `quy đổi ra doanh thu/lương/báo cáo`.

Nếu bám đúng tài liệu này thì có thể chia việc tiếp theo thành 4 mảng lớn:

- Nền tảng phân quyền và tài khoản.
- Hợp đồng -> đoàn khám -> lịch khám.
- Nhân sự -> vị trí -> chấm công -> lương.
- Kho/vật tư -> chi phí -> báo cáo -> AI hỗ trợ.
