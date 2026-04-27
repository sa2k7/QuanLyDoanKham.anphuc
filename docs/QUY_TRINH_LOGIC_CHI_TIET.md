# Quy Trình Logic Nghiệp Vụ - Quản Lý Đoàn Khám An Phúc

> Tài liệu này mô tả chi tiết luồng nghiệp vụ, validation, và business rules để áp dụng trong thực tế.
> Phiên bản: 1.0 | Ngày cập nhật: 04/04/2026

---

## Mục Lục
1. [Tổng Quan Luồng Nghiệp Vụ](#1-tổng-quan-luồng-nghiệp-vụ)
2. [Chi Tiết Từng Module](#2-chi-tiết-từng-module)
   - 2.1 [Module Hợp Đồng](#21-module-hợp-đồng)
   - 2.2 [Module Đoàn Khám](#22-module-đoàn-khám)
   - 2.3 [Module Phân Công Nhân Sự](#23-module-phân-công-nhân-sự)
   - 2.4 [Module Chấm Công](#24-module-chấm-công)
   - 2.5 [Module Tính Lương](#25-module-tính-lương)
   - 2.6 [Module Vật Tư](#26-module-vật-tư)
3. [Ma Trận Phân Quyền](#3-ma-trận-phân-quyền)
4. [Xử Lý Lỗi & Edge Cases](#4-xử-lý-lỗi--edge-cases)

---

## 1. Tổng Quan Luồng Nghiệp Vụ

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         LUỒNG NGHIỆP VỤ TỔNG THỂ                          │
└─────────────────────────────────────────────────────────────────────────────┘

PHASE 1: KINH DOANH (Sales)
├── 1.1 Tạo hồ sơ Công ty mới
│   └── Validation: MST (TaxCode) không trùng, Email hợp lệ
├── 1.2 Tạo Hợp đồng khám
│   ├── Required: CompanyId, SigningDate, StartDate, EndDate
│   ├── Validation: StartDate < EndDate, EndDate >= Today
│   └── Auto-generate: ContractCode = "HD{Year}{Seq:0000}"
└── 1.3 Upload file hợp đồng (PDF/DOCX)
    └── Validation: File size <= 10MB

PHASE 2: PHÊ DUYỆT (Management)
├── 2.1 Submit hợp đồng
│   ├── Pre-condition: Status = "Draft"
│   ├── Action: Chuyển sang "PendingApproval", CurrentStep = 1
│   └── Notification: Gửi đến người có quyền duyệt bước 1
├── 2.2 Multi-level Approval
│   ├── Bước 1: Trưởng phòng duyệt
│   ├── Bước 2: Ban giám đốc duyệt (nếu giá trị > threshold)
│   └── Final: Chuyển sang "Approved"
├── 2.3 Hoặc Reject
│   ├── Action: Chuyển sang "Rejected", CurrentStep = 0
│   └── Note: Bắt buộc nhập lý do
└── 2.4 Activate (kích hoạt thực thi)
    └── Pre-condition: Đã duyệt + Đã upload file hợp đồng

PHASE 3: LẬP KẾ HOẠCH (Planning)
├── 3.1 Tự động sinh đoàn từ hợp đồng
│   ├── Input: HealthContractId
│   ├── Logic: Mỗi ngày trong [StartDate, EndDate] tạo 1 đoàn
│   ├── Auto-name: "{Company.ShortName} - {dd/MM/yyyy}"
│   └── Validation: Skip nếu ngày đó đã có đoàn cho hợp đồng này
├── 3.2 Thiết lập vị trí cần thiết
│   ├── Ví dụ: Tiếp nhận(2), Khám nội(1), Khám ngoại(1), Siêu âm(1)
│   └── Validation: RequiredCount >= 1
└── 3.3 AI Điều phối nhân sự
    ├── Input: Danh sách vị trí cần tuyển
    ├── Logic: Tỷ lệ 1:15 (1 nhân sự : 15 người khám)
    ├── Constraint: 
    │   - Kiểm tra lịch bận (trùng ngày + ca)
    │   - Kiểm tra chuyên môn (Specialty phù hợp vị trí)
    └── Output: Danh sách gợi ý với lý do

PHASE 4: VẬN HÀNH (Operations)
├── 4.1 Mở QR chấm công (Trưởng đoàn)
│   ├── Pre-condition: Đoàn ở trạng thái "Open" hoặc "InProgress"
│   ├── Token: Base64(GroupId:Expiry:Signature)
│   ├── Expiry: 12 giờ
│   └── Security: HMAC-SHA256 để chống giả mạo
├── 4.2 Nhân viên quét QR
│   ├── Scan bằng điện thoại
│   ├── Validate token (chưa hết hạn, đúng group)
│   └── Check-in: Ghi nhận thời gian, tạo ScheduleCalendar
├── 4.3 Check-out
│   ├── Tính thời gian làm việc
│   ├── ShiftType = (hours >= 4) ? 1.0 : 0.5
│   └── Update: GroupStaffDetail.ShiftType + CalculatedSalary
└── 4.4 Khóa sổ đoàn
    ├── Pre-condition: Tất cả nhân viên đã check-out
    ├── Action: Chuyển sang "Locked"
    └── Post-action: Không cho phép chỉnh sửa gì thêm

PHASE 5: NGHIỆM THU & LƯƠNG (Payroll)
├── 5.1 Tính lương theo tháng
│   ├── Pre-condition: Tất cả đoàn trong tháng đã Locked
│   ├── Formula (Monthly): (BaseSalary / 26) × TotalActualDays
│   ├── Formula (Daily): DailyRate × TotalActualDays
│   └── Validation: Chỉ tính nhân viên có WorkStatus = "Đã tham gia"
├── 5.2 Duyệt bảng lương
│   ├── Status: Draft → Confirmed
│   └── Lock: Không thể sửa sau khi Confirmed (chỉ Admin unlock)
└── 5.3 Export Excel
    └── Format: Mã NV, Họ tên, Công thực tế, Lương thực lĩnh
```

---

## 2. Chi Tiết Từng Module

### 2.1 Module Hợp Đồng

#### 2.1.1 State Machine

```
                    ┌─────────────┐
         ┌─────────►│   Draft     │◄────────┐
         │          │  (Nháp)     │         │
         │          └──────┬──────┘         │
         │                 │ submit         │ edit
         │                 ▼                │
         │          ┌─────────────┐         │
         │          │  Pending    │         │
    recall│          │  Approval   │         │
         │          └──────┬──────┘         │
         │                 │                │
         │        ┌────────┴────────┐       │
         │        │                 │       │
         │   reject▼                 ▼approve│
         │┌───────────┐         ┌───────────┐ │
         └┤ Rejected  │         │ Approved  │─┘
          └───────────┘         └─────┬─────┘
                                      │ activate
                                      ▼
                              ┌───────────┐
                              │  Active   │
                              └─────┬─────┘
                                    │ finish
                                    ▼
                              ┌───────────┐
                              │  Finished │
                              └─────┬─────┘
                                    │ lock
                                    ▼
                              ┌───────────┐
                              │  Locked   │
                              └───────────┘
```

#### 2.1.2 Business Rules

| # | Rule | Validation | Error Message |
|---|------|------------|---------------|
| 1 | Ngày ký ≤ Ngày bắt đầu | `SigningDate <= StartDate` | "Ngày ký không thể sau ngày bắt đầu" |
| 2 | Ngày bắt đầu < Ngày kết thúc | `StartDate < EndDate` | "Ngày kết thúc phải sau ngày bắt đầu" |
| 3 | Số người khám > 0 | `ExpectedQuantity > 0` | "Số người khám phải lớn hơn 0" |
| 4 | Đơn giá > 0 | `UnitPrice > 0` | "Đơn giá phải lớn hơn 0" |
| 5 | Tổng tiền tự động | `TotalAmount = UnitPrice × ExpectedQuantity` | Auto-calculate |
| 6 | Không trùng hợp đồng/ngày | Unique(CompanyId, SigningDate) | "Công ty đã có hợp đồng ngày này" |
| 7 | Không xóa khi đã khóa | `Status != "Locked"` | "Hợp đồng đã khóa, không thể xóa" |
| 8 | Chỉ sửa Draft/Rejected | `Status in ["Draft", "Rejected"]` | "Chỉ hợp đồng nháp mới được sửa" |

#### 2.1.3 Approval Workflow

```csharp
// Logic submit
if (contract.Status != "Draft") 
    throw new InvalidOperation("Chỉ hợp đồng nháp mới được gửi duyệt");

var firstStep = await GetFirstActiveApprovalStep();
contract.Status = "PendingApproval";
contract.CurrentApprovalStep = firstStep.StepOrder;
await SaveStatusHistory("Draft", "PendingApproval", userId, "Gửi phê duyệt");

// Logic approve
if (contract.Status != "PendingApproval")
    throw new InvalidOperation("Hợp đồng không ở trạng thái chờ duyệt");

if (contract.CreatedByUserId == currentUserId)
    throw new InvalidOperation("Người tạo không được tự duyệt");

var currentStep = await GetApprovalStep(contract.CurrentApprovalStep);
var nextStep = await GetNextApprovalStep(contract.CurrentApprovalStep);

await SaveApprovalHistory(contract.Id, currentStep, "Approved", userId, note);

if (nextStep != null) {
    contract.CurrentApprovalStep = nextStep.StepOrder;
    await SaveStatusHistory("PendingApproval", "PendingApproval", userId, 
        $"Đã duyệt bước {currentStep.StepOrder}, chờ {nextStep.StepName}");
} else {
    contract.Status = "Approved";
    contract.CurrentApprovalStep = 0;
    await SaveStatusHistory("PendingApproval", "Approved", userId, "Phê duyệt hoàn tất");
}

// Logic reject
await SaveApprovalHistory(contract.Id, currentStep, "Rejected", userId, note);
contract.Status = "Rejected";
contract.CurrentApprovalStep = 0;
await SaveStatusHistory("PendingApproval", "Rejected", userId, note);
```

---

### 2.2 Module Đoàn Khám

#### 2.2.1 State Machine

```
┌─────────┐   create from    ┌─────────┐   open QR    ┌───────────┐
│  Draft  │ ─────────────── │  Open   │ ───────────► │InProgress │
└─────────┘    contract      └─────────┘  screen     └───────────┘
       │                                                │
       │ manual                                         │ all checked
       │ update                                         │ out
       ▼                                                ▼
  ┌─────────┐                                      ┌───────────┐
  │ Cancel  │                                      │  Finished │
  └─────────┘                                      └─────┬─────┘
                                                         │
                                                    lock ▼
                                                    ┌───────────┐
                                                    │  Locked   │
                                                    └───────────┘
```

#### 2.2.2 Business Rules

| # | Rule | Validation | Error Message |
|---|------|------------|---------------|
| 1 | Hợp đồng phải Approved | `Contract.Status in ["Approved", "Active"]` | "Hợp đồng chưa được phê duyệt" |
| 2 | Không trùng ngày+hợp đồng | Unique(HealthContractId, ExamDate.Date) | "Ngày này đã có đoàn cho hợp đồng" |
| 3 | Ngày khám trong khoảng HĐ | `ExamDate between Contract.StartDate and Contract.EndDate` | "Ngày khám ngoài phạm vi hợp đồng" |
| 4 | Slot hợp lệ | `Slot in ["Morning", "Afternoon", "Evening", "FullDay"]` | "Ca khám không hợp lệ" |
| 5 | Không sửa khi Locked | `Status != "Locked"` | "Đoàn đã khóa" |
| 6 | Có trưởng đoàn | `GroupLeaderStaffId != null` | "Phải chỉ định trưởng đoàn" |

#### 2.2.3 Auto-Create Logic

```csharp
// Sinh đoàn tự động từ hợp đồng
async Task GenerateGroupsFromContract(int contractId) {
    var contract = await GetContract(contractId);
    
    if (contract.Status != "Approved" && contract.Status != "Active")
        throw new InvalidOperation("Hợp đồng chưa được duyệt");
    
    var start = contract.StartDate.Date;
    var end = contract.EndDate.Date;
    var createdCount = 0;
    
    for (var date = start; date <= end; date = date.AddDays(1)) {
        // Skip nếu đã có đoàn
        var exists = await _context.MedicalGroups.AnyAsync(g => 
            g.HealthContractId == contractId && 
            g.ExamDate.Date == date);
        
        if (exists) continue;
        
        // Tạo đoàn mới
        var group = new MedicalGroup {
            GroupName = $"{contract.Company.ShortName} - {date:dd/MM/yyyy}",
            ExamDate = date,
            HealthContractId = contractId,
            Status = "Open",
            Slot = "FullDay",
            TeamCode = "A", // Mặc định team A
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUser
        };
        
        _context.MedicalGroups.Add(group);
        createdCount++;
    }
    
    await _context.SaveChangesAsync();
    return createdCount;
}
```

#### 2.2.4 Finish Validation

```csharp
// Kiểm tra điều kiện chuyển sang Finished
async Task<bool> CanFinishGroup(int groupId) {
    var group = await GetGroup(groupId);
    
    // Lấy danh sách nhân sự trong đoàn
    var staffDetails = await _context.GroupStaffDetails
        .Where(g => g.GroupId == groupId)
        .ToListAsync();
    
    if (!staffDetails.Any()) 
        return true; // Đoàn không có ai thì cho finish
    
    // Kiểm tra tất cả đã check-out
    var allCheckedOut = staffDetails.All(s => s.CheckOutTime != null);
    if (!allCheckedOut) {
        var pendingStaffs = staffDetails
            .Where(s => s.CheckOutTime == null)
            .Select(s => s.Staff.FullName);
        throw new InvalidOperation($"Còn nhân viên chưa check-out: {string.Join(", ", pendingStaffs)}");
    }
    
    return true;
}
```

---

### 2.3 Module Phân Công Nhân Sự

#### 2.3.1 Business Rules

| # | Rule | Validation | Error Message |
|---|------|------------|---------------|
| 1 | Không trùng lịch | `!HasOverlap(StaffId, ExamDate, ShiftType)` | "Nhân viên đã có lịch trong ngày/ca này" |
| 2 | Đúng chuyên môn | `Position.SpecialtyRequired == null \|\| Staff.Specialty == Position.SpecialtyRequired` | "Chuyên môn không phù hợp vị trí" |
| 3 | Chưa đủ định biên | `Quota.Assigned < Quota.Required` | "Vị trí đã đủ người" |
| 4 | Đoàn đang mở | `Group.Status in ["Open", "InProgress"]` | "Đoàn đã đóng/khóa" |
| 5 | Nhân viên active | `Staff.IsActive == true` | "Nhân viên không còn hoạt động" |
| 6 | Tính lương ca | `CalculatedSalary = BaseSalary / 26 × ShiftType` | Auto-calculate |

#### 2.3.2 Check Schedule Conflict

```csharp
// Kiểm tra trùng lịch chi tiết
async Task<bool> HasScheduleConflict(int staffId, DateTime examDate, double shiftType, int? excludeGroupId = null) {
    var query = _context.GroupStaffDetails
        .Include(g => g.MedicalGroup)
        .Where(g => g.StaffId == staffId 
            && g.MedicalGroup.ExamDate.Date == examDate.Date
            && Math.Abs(g.ShiftType - shiftType) < 0.001);
    
    if (excludeGroupId.HasValue)
        query = query.Where(g => g.GroupId != excludeGroupId.Value);
    
    var conflicts = await query.ToListAsync();
    
    if (conflicts.Any()) {
        var conflictInfo = conflicts.Select(c => 
            $"{c.MedicalGroup.GroupName} (Ca: {c.ShiftType})");
        throw new InvalidOperation($"Trùng lịch với: {string.Join(", ", conflictInfo)}");
    }
    
    return false;
}
```

#### 2.3.3 Quota Management

```csharp
// Thêm nhân sự vào vị trí
async Task AddStaffToPosition(int groupId, int staffId, int? positionQuotaId, double shiftType) {
    var group = await GetGroup(groupId);
    if (group.Status == "Locked" || group.Status == "Finished")
        throw new InvalidOperation("Đoàn đã đóng");
    
    // Kiểm tra quota
    if (positionQuotaId.HasValue) {
        var quota = await _context.GroupPositionQuotas
            .Include(q => q.Position)
            .FirstOrDefaultAsync(q => q.Id == positionQuotaId.Value);
        
        if (quota != null) {
            if (quota.Assigned >= quota.Required)
                throw new InvalidOperation($"Vị trí {quota.Position.Name} đã đủ {quota.Required} người");
            
            // Kiểm tra chuyên môn
            if (!string.IsNullOrEmpty(quota.Position.SpecialtyRequired)) {
                var staff = await GetStaff(staffId);
                if (staff.Specialty != quota.Position.SpecialtyRequired)
                    throw new InvalidOperation($"Cần chuyên khoa {quota.Position.SpecialtyRequired}");
            }
        }
    }
    
    // Kiểm tra trùng lịch
    await HasScheduleConflict(staffId, group.ExamDate, shiftType);
    
    // Thêm vào đoàn
    var detail = new GroupStaffDetail {
        GroupId = groupId,
        StaffId = staffId,
        GroupPositionQuotaId = positionQuotaId,
        ShiftType = shiftType,
        WorkStatus = "Đang chờ",
        ExamDate = group.ExamDate
    };
    
    // Cập nhật quota
    if (positionQuotaId.HasValue) {
        var quota = await _context.GroupPositionQuotas.FindAsync(positionQuotaId.Value);
        quota.Assigned++;
    }
    
    _context.GroupStaffDetails.Add(detail);
    await _context.SaveChangesAsync();
}
```

---

### 2.4 Module Chấm Công

#### 2.4.1 QR Token Security

```csharp
// Tạo secure QR token với HMAC
string GenerateSecureQrToken(int groupId, int expiryHours = 12) {
    var expiry = DateTimeOffset.UtcNow.AddHours(expiryHours).ToUnixTimeSeconds();
    var payload = $"{groupId}:{expiry}";
    
    // Tạo signature với secret key
    var key = Encoding.UTF8.GetBytes(_configuration["QrSecretKey"]);
    using var hmac = new HMACSHA256(key);
    var signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
    
    // Format: payload.signature (Base64)
    var token = $"{Convert.ToBase64String(Encoding.UTF8.GetBytes(payload))}.{Convert.ToBase64String(signature)}";
    return token;
}

// Validate QR token
bool ValidateQrToken(string token, out int groupId, out string error) {
    groupId = 0;
    error = null;
    
    try {
        var parts = token.Split('.');
        if (parts.Length != 2) {
            error = "Token format không hợp lệ";
            return false;
        }
        
        var payload = Encoding.UTF8.GetString(Convert.FromBase64String(parts[0]));
        var providedSignature = Convert.FromBase64String(parts[1]);
        
        // Verify signature
        var key = Encoding.UTF8.GetBytes(_configuration["QrSecretKey"]);
        using var hmac = new HMACSHA256(key);
        var expectedSignature = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        
        if (!CryptographicOperations.FixedTimeEquals(providedSignature, expectedSignature)) {
            error = "Token signature không hợp lệ";
            return false;
        }
        
        // Parse payload
        var payloadParts = payload.Split(':');
        groupId = int.Parse(payloadParts[0]);
        var expiry = long.Parse(payloadParts[1]);
        
        // Check expiry
        if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expiry) {
            error = "Token đã hết hạn";
            return false;
        }
        
        return true;
    } catch (Exception ex) {
        error = $"Lỗi validate token: {ex.Message}";
        return false;
    }
}
```

#### 2.4.2 Check-in/out Logic

```csharp
// Check-in
async Task<CheckInResult> CheckIn(int groupId, int staffId, string qrToken) {
    // Validate token
    if (!ValidateQrToken(qrToken, out var tokenGroupId, out var error))
        throw new InvalidOperation(error);
    
    if (tokenGroupId != groupId)
        throw new InvalidOperation("Token không khớp với đoàn");
    
    // Kiểm tra nhân viên trong đoàn
    var groupDetail = await _context.GroupStaffDetails
        .FirstOrDefaultAsync(g => g.GroupId == groupId && g.StaffId == staffId);
    
    if (groupDetail == null)
        throw new InvalidOperation("Nhân viên không thuộc đoàn này");
    
    // Kiểm tra đã check-in chưa
    var today = DateTime.Today;
    var existing = await _context.ScheduleCalendars
        .FirstOrDefaultAsync(s => s.GroupId == groupId 
            && s.StaffId == staffId 
            && s.ExamDate.Date == today);
    
    if (existing != null && existing.CheckInTime.HasValue)
        throw new InvalidOperation($"Đã check-in lúc {existing.CheckInTime:HH:mm}");
    
    // Thực hiện check-in
    if (existing == null) {
        existing = new ScheduleCalendar {
            GroupId = groupId,
            StaffId = staffId,
            ExamDate = today,
            CheckInTime = DateTime.Now,
            IsConfirmed = false
        };
        _context.ScheduleCalendars.Add(existing);
    } else {
        existing.CheckInTime = DateTime.Now;
    }
    
    // Cập nhật work status
    groupDetail.WorkStatus = "Đã tham gia";
    
    await _context.SaveChangesAsync();
    
    return new CheckInResult {
        Success = true,
        CheckInTime = existing.CheckInTime.Value,
        Message = "Check-in thành công"
    };
}

// Check-out
async Task<CheckOutResult> CheckOut(int groupId, int staffId) {
    var today = DateTime.Today;
    
    // Tìm bản ghi check-in
    var schedule = await _context.ScheduleCalendars
        .FirstOrDefaultAsync(s => s.GroupId == groupId 
            && s.StaffId == staffId 
            && s.ExamDate.Date == today);
    
    if (schedule == null || !schedule.CheckInTime.HasValue)
        throw new InvalidOperation("Chưa check-in, không thể check-out");
    
    if (schedule.CheckOutTime.HasValue)
        throw new InvalidOperation($"Đã check-out lúc {schedule.CheckOutTime:HH:mm}");
    
    // Thực hiện check-out
    schedule.CheckOutTime = DateTime.Now;
    schedule.IsConfirmed = true;
    
    // Tính shift type và lương
    var hours = (schedule.CheckOutTime.Value - schedule.CheckInTime.Value).TotalHours;
    var shiftType = hours >= 4 ? 1.0 : 0.5;
    
    // Cập nhật GroupStaffDetail
    var groupDetail = await _context.GroupStaffDetails
        .Include(g => g.Staff)
        .FirstOrDefaultAsync(g => g.GroupId == groupId && g.StaffId == staffId);
    
    groupDetail.ShiftType = shiftType;
    groupDetail.CheckOutTime = schedule.CheckOutTime;
    
    // Tính lương ca
    var staff = groupDetail.Staff;
    if (staff.SalaryType == "Daily" && staff.DailyRate > 0) {
        groupDetail.CalculatedSalary = staff.DailyRate * (decimal)shiftType;
    } else {
        var stdDays = staff.StandardWorkDays > 0 ? staff.StandardWorkDays : 26;
        groupDetail.CalculatedSalary = (staff.BaseSalary / stdDays) * (decimal)shiftType;
    }
    
    await _context.SaveChangesAsync();
    
    return new CheckOutResult {
        Success = true,
        CheckOutTime = schedule.CheckOutTime.Value,
        TotalHours = hours,
        ShiftType = shiftType,
        CalculatedSalary = groupDetail.CalculatedSalary,
        Message = $"Check-out thành công! Công: {shiftType}"
    };
}
```

#### 2.4.3 Manual Check-in (Cho trưởng đoàn)

```csharp
// Chấm công thủ công (khi QR không hoạt động)
async Task ManualCheckIn(int groupId, int staffId, string reason) {
    // Chỉ trưởng đoàn hoặc admin được phép
    var currentUser = GetCurrentUser();
    var group = await GetGroup(groupId);
    
    if (!currentUser.HasPermission("ChamCong.Manual") && 
        group.GroupLeaderStaffId != currentUser.StaffId)
        throw new Unauthorized("Không có quyền chấm công thủ công");
    
    // Logic tương tự check-in bình thường nhưng ghi nhận là manual
    var result = await CheckIn(groupId, staffId, GenerateOneTimeToken());
    
    // Ghi log
    await SaveAuditLog("MANUAL_CHECKIN", "ScheduleCalendar", result.CalendarId, 
        null, $"Manual by {currentUser.FullName}. Lý do: {reason}");
    
    return result;
}
```

---

### 2.5 Module Tính Lương

#### 2.5.1 Business Rules

| # | Rule | Validation | Error Message |
|---|------|------------|---------------|
| 1 | Chỉ tính khi đã khóa | Tất cả đoàn trong tháng đã Locked | "Còn đoàn chưa khóa sổ" |
| 2 | Chỉ tính người tham gia | `WorkStatus == "Đã tham gia"` | Skip nếu vắng/nghỉ |
| 3 | Công chuẩn mặc định | `StandardWorkDays = 26` | Default value |
| 4 | Lương tháng | `(BaseSalary / 26) × ActualDays` | Formula |
| 5 | Lương ngày | `DailyRate × ActualDays` | Formula |
| 6 | Không generate trùng | `!Exists(PayrollRecord for month/year)` | "Đã có bảng lương tháng này" |
| 7 | Confirm lock | Draft → Confirmed không rollback | Chỉ Admin mới unlock |

#### 2.5.2 Payroll Calculation Flow

```csharp
// Tính lương tháng
async Task<List<PayrollRecord>> GenerateMonthlyPayroll(int month, int year) {
    // Kiểm tra đã có bảng lương chưa
    var existing = await _context.PayrollRecords
        .AnyAsync(p => p.Month == month && p.Year == year && p.Status != "Cancelled");
    
    if (existing)
        throw new InvalidOperation("Đã có bảng lương tháng này");
    
    // Kiểm tra tất cả đoàn đã locked
    var hasUnlocked = await _context.MedicalGroups
        .AnyAsync(g => g.ExamDate.Month == month 
            && g.ExamDate.Year == year 
            && g.Status != "Locked");
    
    if (hasUnlocked)
        throw new InvalidOperation("Còn đoàn chưa khóa sổ, không thể tính lương");
    
    var records = new List<PayrollRecord>();
    var staffs = await _context.Staffs.Where(s => s.IsActive).ToListAsync();
    
    foreach (var staff in staffs) {
        // Lấy tất cả đoàn nhân viên tham gia trong tháng
        var groupDetails = await _context.GroupStaffDetails
            .Include(g => g.MedicalGroup)
            .Where(g => g.StaffId == staff.StaffId
                && g.MedicalGroup.ExamDate.Month == month
                && g.MedicalGroup.ExamDate.Year == year
                && g.WorkStatus == "Đã tham gia")
            .ToListAsync();
        
        if (!groupDetails.Any()) continue;
        
        // Tính tổng công
        var totalDays = groupDetails.Sum(g => g.ShiftType);
        
        // Tính lương
        decimal totalAmount;
        decimal dailyRate;
        
        if (staff.SalaryType == "Daily" && staff.DailyRate > 0) {
            dailyRate = staff.DailyRate;
            totalAmount = dailyRate * (decimal)totalDays;
        } else {
            var stdDays = staff.StandardWorkDays > 0 ? staff.StandardWorkDays : 26;
            dailyRate = staff.BaseSalary / stdDays;
            totalAmount = dailyRate * (decimal)totalDays;
        }
        
        var record = new PayrollRecord {
            StaffId = staff.StaffId,
            Month = month,
            Year = year,
            BaseSalary = staff.BaseSalary,
            DailyRate = dailyRate,
            SalaryType = staff.SalaryType ?? "Monthly",
            StandardWorkDays = staff.StandardWorkDays > 0 ? staff.StandardWorkDays : 26,
            TotalActualDays = totalDays,
            TotalAmount = totalAmount,
            Status = "Draft",
            GeneratedBy = currentUser,
            GeneratedAt = DateTime.Now
        };
        
        records.Add(record);
        _context.PayrollRecords.Add(record);
    }
    
    await _context.SaveChangesAsync();
    return records;
}
```

#### 2.5.3 Confirm Payroll

```csharp
// Chốt bảng lương
async Task ConfirmPayroll(int month, int year) {
    var records = await _context.PayrollRecords
        .Where(p => p.Month == month && p.Year == year && p.Status == "Draft")
        .ToListAsync();
    
    if (!records.Any())
        throw new InvalidOperation("Không có bảng lương Draft để chốt");
    
    // Kiểm tra lại tất cả đoàn đã locked (an toàn kép)
    var hasUnlocked = await _context.MedicalGroups
        .AnyAsync(g => g.ExamDate.Month == month 
            && g.ExamDate.Year == year 
            && g.Status != "Locked");
    
    if (hasUnlocked)
        throw new InvalidOperation("Phát hiện đoàn chưa khóa sổ, không thể chốt");
    
    // Chốt tất cả records
    foreach (var record in records) {
        record.Status = "Confirmed";
    }
    
    // Ghi audit log
    await SaveAuditLog("CONFIRM_PAYROLL", "PayrollRecord", 0, null,
        $"Chốt lương T{month}/{year} - {records.Count} nhân viên");
    
    await _context.SaveChangesAsync();
}
```

---

### 2.6 Module Vật Tư

#### 2.6.1 Business Rules

| # | Rule | Validation | Error Message |
|---|------|------------|---------------|
| 1 | Tồn kho không âm | `TotalStock >= 0` | "Tồn kho không đủ" |
| 2 | Cảnh báo ngưỡng thấp | `TotalStock <= MinStockLevel` | Warning: "Sắp hết hàng" |
| 3 | Hạn sử dụng | `ExpirationDate > Today` | "Vật tư đã hết hạn" |
| 4 | Xuất kho có định danh | `GroupId != null` | "Phải chỉ định đoàn xuất" |
| 5 | Số lượng > 0 | `Quantity > 0` | "Số lượng phải lớn hơn 0" |

#### 2.6.2 Inventory Transaction

```csharp
// Nhập kho
async Task<SupplyInventoryVoucher> ImportSupplies(List<ImportItem> items) {
    var voucher = new SupplyInventoryVoucher {
        VoucherCode = GenerateVoucherCode("IMP"),
        Type = "IMPORT",
        CreateDate = DateTime.Now,
        CreatedByUserId = currentUserId
    };
    
    foreach (var item in items) {
        // Cập nhật tồn kho
        var supply = await _context.Supplies.FindAsync(item.SupplyId);
        supply.TotalStock += item.Quantity;
        
        // Thêm chi tiết
        voucher.Details.Add(new SupplyInventoryDetail {
            SupplyId = item.SupplyId,
            Quantity = item.Quantity,
            Price = item.Price
        });
    }
    
    _context.SupplyInventoryVouchers.Add(voucher);
    await _context.SaveChangesAsync();
    return voucher;
}

// Xuất kho cho đoàn
async Task<SupplyInventoryVoucher> ExportSupplies(int groupId, List<ExportItem> items) {
    // Kiểm tra đoàn tồn tại và chưa khóa
    var group = await GetGroup(groupId);
    if (group.Status == "Locked")
        throw new InvalidOperation("Đoàn đã khóa, không thể xuất thêm vật tư");
    
    var voucher = new SupplyInventoryVoucher {
        VoucherCode = GenerateVoucherCode("EXP"),
        Type = "EXPORT",
        GroupId = groupId,
        CreateDate = DateTime.Now,
        CreatedByUserId = currentUserId
    };
    
    foreach (var item in items) {
        var supply = await _context.Supplies.FindAsync(item.SupplyId);
        
        // Kiểm tra tồn kho
        if (supply.TotalStock < item.Quantity)
            throw new InvalidOperation($"{supply.SupplyName} không đủ tồn kho (còn {supply.TotalStock})");
        
        // Kiểm tra hạn sử dụng
        if (supply.ExpirationDate.HasValue && supply.ExpirationDate.Value < DateTime.Now)
            throw new InvalidOperation($"{supply.SupplyName} đã hết hạn sử dụng");
        
        // Trừ tồn kho
        supply.TotalStock -= item.Quantity;
        
        voucher.Details.Add(new SupplyInventoryDetail {
            SupplyId = item.SupplyId,
            Quantity = item.Quantity,
            Price = item.Price
        });
    }
    
    _context.SupplyInventoryVouchers.Add(voucher);
    await _context.SaveChangesAsync();
    return voucher;
}
```

---

### 2.7 Module Khám Bệnh (OMS - Operations Management System)

#### 2.7.1 Medical Record State Machine

Hồ sơ khám bệnh (`MedicalRecord`) vận hành theo máy trạng thái nghiêm ngặt để đảm bảo tính toàn vẹn dữ liệu lâm sàng:

```
┌───────────┐   Check-in   ┌─────────────┐   Khám xong   ┌───────────────┐
│   READY   │ ───────────► │ CHECKED_IN  │ ────────────► │ WAITING_FOR_QC│
└───────────┘ (tạo tasks)  └─────────────┘ (tất cả trạm) └───────┬───────┘
     │                            │                             │
     │ Cancel                     │ No-show                     │ Finalize
     ▼                            ▼                             ▼
┌───────────┐              ┌─────────────┐               ┌───────────────┐
│ CANCELLED │              │   NO_SHOW   │               │   COMPLETED   │
└───────────┘              └─────────────┘               └───────────────┘
```

#### 2.7.2 Business Rules & Defensive Coding

| # | Rule | Validation | Logic Xử Lý |
|---|------|------------|-------------|
| 1 | Check-in phải có trạm | `ActiveStations.Any()` | Trả về `ServiceResult.Failure` nếu trạm chưa mở. Tránh FK_TaskId = 0. |
| 2 | Chặn Bypass quy trình | `record.Status == "WAITING_FOR_QC"` | Chỉ cho phép gọi `Finalize` khi đã hoàn tất tất cả trạm khám. |
| 3 | Đồng bộ hóa EF Core | Option B: Double `SaveChanges` | Luôn gọi `SaveChangesAsync` sau khi đổi Status Task trước khi kiểm đếm `AnyAsync`. |
| 4 | Trả về chuẩn RESTful | `ServiceResult<T>` | Mọi lỗi nghiệp vụ trả về `HTTP 400` kèm JSON `{ message: "..." }`. |

#### 2.7.3 Quy Trình Hoàn Tất (Finalize Logic)

```csharp
// Logic chuẩn hóa architect
if (record.Status != "WAITING_FOR_QC")
    return ServiceResult.Failure("Chưa hoàn tất các trạm khám.");

record.Status = "COMPLETED";
var lastTask = await GetLastCompletedTask(recordId);
if (lastTask == null) return ServiceResult.Failure("Thiếu lịch sử khám.");

recordEvent.TaskId = lastTask.TaskId; // Luôn đảm bảo TaskId hợp lệ
await SaveChangesAsync();
```

---

## 3. Ma Trận Phân Quyền

### 3.1 Permission Definitions

| Permission Key | Module | Mô tả | Roles |
|----------------|--------|-------|-------|
| HopDong.View | Hợp đồng | Xem danh sách/chi tiết | Admin, Manager, Sale |
| HopDong.Create | Hợp đồng | Tạo hợp đồng mới | Admin, Manager, Sale |
| HopDong.Edit | Hợp đồng | Sửa/xóa hợp đồng | Admin, Manager |
| HopDong.Approve | Hợp đồng | Duyệt hợp đồng | Admin, Manager |
| HopDong.Reject | Hợp đồng | Từ chối hợp đồng | Admin, Manager |
| HopDong.Upload | Hợp đồng | Upload file đính kèm | Admin, Manager, Sale |
| DoanKham.View | Đoàn khám | Xem danh sách đoàn | Admin, Manager, MedicalStaff |
| DoanKham.Create | Đoàn khám | Tạo đoàn mới | Admin, Manager |
| DoanKham.Edit | Đoàn khám | Sửa thông tin đoàn | Admin, Manager, GroupLeader |
| DoanKham.AssignStaff | Đoàn khám | Phân công nhân sự | Admin, Manager, GroupLeader |
| ChamCong.QR | Chấm công | Mở mã QR | Admin, Manager, GroupLeader |
| ChamCong.CheckInOut | Chấm công | Check-in/out thủ công | Admin, Manager, GroupLeader |
| ChamCong.ViewAll | Chấm công | Xem tất cả chấm công | Admin, Manager |
| ChamCong.ViewOwn | Chấm công | Xem chấm công cá nhân | MedicalStaff |
| Luong.View | Lương | Xem bảng lương | Admin, Manager, MedicalStaff (own) |
| Luong.Manage | Lương | Tạo/chốt bảng lương | Admin, Manager |
| NhanSu.View | Nhân sự | Xem danh sách nhân sự | Admin, Manager, HR |
| NhanSu.Manage | Nhân sự | CRUD nhân sự | Admin, HR |
| Kho.View | Vật tư | Xem tồn kho | Admin, Manager |
| Kho.Manage | Vật tư | Nhập/xuất kho | Admin, Manager |

### 3.2 Role Hierarchy

```
┌─────────────┐
│    Admin    │ ← Full access, có thể unlock mọi thứ
└──────┬──────┘
       │
┌──────┴──────┐
│   Manager   │ ← Quản lý đoàn, duyệt hợp đồng, chốt lương
└──────┬──────┘
       │
┌──────┴──────┐
│ GroupLeader │ ← Trưởng đoàn: mở QR, phân công nhân sự đoàn mình
└──────┬──────┘
       │
┌──────┴──────┐
│MedicalStaff │ ← Chỉ xem lịch cá nhân, quét QR check-in
└─────────────┘
```

---

## 4. Xử Lý Lỗi & Edge Cases

### 4.1 Danh Sách Edge Cases

| Case | Mô tả | Xử lý |
|------|-------|-------|
| 1 | Nhân viên quét QR sau khi đoàn đã khóa | Check group.Status trước khi nhận token |
| 2 | Check-out khi chưa check-in | Validation: require CheckInTime != null |
| 3 | Tính lương khi còn đoàn chưa khóa | Kiểm tra tất cả đoàn Locked trước khi generate |
| 4 | Xóa đoàn khi đã có nhân sự được phân công | Warning + Cascade delete GroupStaffDetails |
| 5 | Trùng lịch nhân viên | Check (StaffId + ExamDate + ShiftType) trước khi add |
| 6 | Token QR hết hạn | Validate expiry, yêu cầu tạo token mới |
| 7 | Người tạo tự duyệt hợp đồng | Validation: CreatedByUserId != CurrentUserId |
| 8 | Xuất kho khi tồn kho không đủ | Validation: TotalStock >= RequestQuantity |
| 9 | Thêm nhân viên vào đoàn đã khóa | Validation: Status != "Locked" && Status != "Finished" |
| 10 | Chấm công thủ công khi đã có QR check | Ghi đè hoặc báo lỗi tùy policy |
| 11 | Check-in MedicalRecord nhưng không có cấu hình trạm | Trả về ServiceResult.Failure thay vì throw Exception |
| 12 | Gọi Finalize Record khi chưa qua bước QC | Chặn bằng logic status != "WAITING_FOR_QC" |
| 13 | Lỗi DB Foreign Key do TaskId = 0 | Validation chặn đứng trước khi SaveChangesAsync |
| 14 | Lỗi đếm Task hoàn tất bị chậm (Stale Read) | Gọi SaveChangesAsync ngay sau khi update task status |

### 4.2 Error Handling Pattern

```csharp
// Standard API Error Response
public class ApiErrorResponse {
    public string Code { get; set; }
    public string Message { get; set; }
    public string Details { get; set; }
    public Dictionary<string, string> FieldErrors { get; set; }
}

// Middleware xử lý lỗi toàn cục
public async Task ErrorHandlingMiddleware(HttpContext context, RequestDelegate next) {
    try {
        await next(context);
    } catch (ValidationException ex) {
        context.Response.StatusCode = 400;
        await WriteErrorResponse(context, "VALIDATION_ERROR", ex.Message, ex.Errors);
    } catch (UnauthorizedAccessException ex) {
        context.Response.StatusCode = 403;
        await WriteErrorResponse(context, "FORBIDDEN", ex.Message);
    } catch (InvalidOperationException ex) {
        context.Response.StatusCode = 400;
        await WriteErrorResponse(context, "BUSINESS_RULE_VIOLATION", ex.Message);
    } catch (Exception ex) {
        _logger.LogError(ex, "Unhandled exception");
        context.Response.StatusCode = 500;
        await WriteErrorResponse(context, "INTERNAL_ERROR", "Đã xảy ra lỗi hệ thống");
    }
}
```

### 4.3 Audit Logging

```csharp
// Ghi log mọi thay đổi quan trọng
async Task SaveAuditLog(string action, string entityType, int entityId, 
    string oldValue, string newValue) {
    var log = new AuditLog {
        UserId = currentUserId,
        Action = action,
        EntityType = entityType,
        EntityId = entityId,
        OldValue = oldValue,
        NewValue = newValue,
        Timestamp = DateTime.Now,
        IPAddress = GetClientIp()
    };
    
    _context.AuditLogs.Add(log);
    await _context.SaveChangesAsync();
}

// Các action cần log:
// - CHECK_IN, CHECK_OUT
// - CONFIRM_PAYROLL, GENERATE_PAYROLL
// - CONTRACT_APPROVE, CONTRACT_REJECT
// - GROUP_CREATE, GROUP_LOCK
// - STAFF_ASSIGN, STAFF_REMOVE
```

---

## 5. Quy Trình Khẩn Cấp (Emergency Procedures)

### 5.1 Unlock Payroll (khi đã chốt nhầm)

```
1. Admin gọi API: POST /api/payroll/emergency-unlock
2. Parameters: month, year, reason
3. Validation: reason.Length >= 20
4. Action:
   - Đổi tất cả records từ "Confirmed" → "Draft"
   - Ghi AuditLog với action = "EMERGENCY_UNLOCK"
   - Gửi notification đến tất cả Manager
5. Required: Permission "Luong.EmergencyUnlock"
```

### 5.2 Fix Check-in Time

```
1. Admin hoặc Manager có quyền ChamCong.Manual
2. API: PATCH /api/attendance/{calendarId}/time
3. Parameters: newCheckInTime, newCheckOutTime, reason
4. Validation:
   - newCheckOutTime > newCheckInTime
   - reason.Length >= 10
5. Action:
   - Ghi OldValue vào AuditLog
   - Cập nhật thời gian
   - Recalculate ShiftType và CalculatedSalary
```

### 5.3 Cancel Contract

```
1. Chỉ Admin có quyền
2. Pre-condition: Chưa có đoàn nào ở trạng thái InProgress/Finished/Locked
3. Action:
   - Xóa (soft delete) tất cả đoàn Draft/Open
   - Chuyển hợp đồng sang "Cancelled"
   - Ghi lý do hủy
```

---

## 6. Module Import & Export

### 6.1 Import Danh Sách Công Nhan (DSCN) tu Excel

#### 6.1.1 Template Excel

| Cột | Ten | Kieu du lieu | Bat buoc | Ghi chu |
|-----|-----|-------------|----------|---------|
| A | STT | int | Co | So thu tu |
| B | Ho ten | string | Co | Ho va ten day du |
| C | Ngay sinh | DateTime | Co | dd/MM/yyyy |
| D | Gioi tinh | string | Co | Nam / Nu |
| E | So dien thoai | string | Khong | 10-11 so |
| F | Ma doan kham | int | Co | MedicalGroupId |
| G | Ghi chu | string | Khong | Benh ly nen, dac diem |

#### 6.1.2 Business Rules

| # | Rule | Validation | Error Message |
|---|------|------------|---------------|
| 1 | File dinh dang | `.xlsx` hoac `.xls` | "Chi chap nhan file Excel" |
| 2 | Sheet dau tien | Sheet[0] | "Khong tim thay du lieu" |
| 3 | MedicalGroupId ton tai | `Any(g => g.Id == groupId)` | "Ma doan kham khong ton tai" |
| 4 | Doan chua khoa | `Group.Status != "Locked"` | "Doan da khoa, khong the import" |
| 5 | Ten khong rong | `!string.IsNullOrWhiteSpace(FullName)` | "Ho ten khong duoc de trong (dong X)" |
| 6 | Ngay sinh hop le | `DateTime.TryParse` | "Ngay sinh khong hop le (dong X)" |
| 7 | Gioi tinh hop le | `In ["Nam","Nu","Male","Female"]` | "Gioi tinh khong hop le" |
| 8 | Trung SDT trong doan | `!Exists(Phone, GroupId)` | Warning: "SDT da ton tai trong doan" |

#### 6.1.3 Import Logic (Pseudo)

```csharp
async Task<ImportResult> ImportPatientsFromExcel(IFormFile file, int medicalGroupId) {
    // Validate file + MedicalGroup ton tai + chua Locked
    // Dung ClosedXML doc lap qua tung dong (bo qua header)
    // Validate tung dong: FullName, DOB, Gender
    // Check trung SDT trong cung doan (warning nhung van import)
    // Insert Patient { FullName, DOB, Gender, Phone, MedicalGroupId, Source="ExcelImport" }
    // AuditLog: action="IMPORT_PATIENTS", entityType="Patient", entityId=groupId
    // Tra ve ImportResult { TotalRows, ImportedCount, SkippedCount, Errors[] }
}
```

### 6.2 Export Bao Cao

#### 6.2.1 Danh Sach Benh Nhan Theo Doan

```csharp
async Task<byte[]> ExportPatientsByGroup(int groupId) {
    // Lay group + company info
    // Query Patients.Where(p => p.MedicalGroupId == groupId).OrderBy(name)
    // Tao XLWorkbook: sheet "DSCN" (header + data), sheet "Thong tin" (ten doan, cong ty, ngay, tong so)
    // Tra ve MemoryStream.ToArray()
}
```

#### 6.2.2 Bao Cao P&L Doan Kham

```csharp
async Task<byte[]> ExportGroupPnl(int groupId) {
    // Goi _reportingService.CalculateGroupPnlAsync(groupId)
    // Tao Excel: LaborCost, MaterialCost, OverheadCost, TotalCost
    // Ghi chu: "So tien nay la chi phi THUC TE, khong lien quan den gia tri hop dong"
    // Tra ve byte[]
}
```

#### 6.2.3 Bang Cham Cong / Luong

```csharp
async Task<byte[]> ExportPayroll(int month, int year) {
    // Query PayrollRecords.Where(p => p.Month==month && p.Year==year)
    // Cot: Ma NV, Ho ten, Cong thuc te, Don gia/ngay, Luong thuc linh
    // Tra ve byte[]
}
```

---

## 7. Module AI Dieu Phoi Nhan Su

### 7.1 Muc tieu

Tu dong goi y nhan su phu hop cho tung vi tri trong doan kham, dua tren:
- Vai tro chuyen mon (`StaffRole.SpecialtyRequired`)
- Lich lam viec hien tai (khong trung ngay/ca)
- So luong da di kham trong thang (can bang workload)

### 7.2 Business Rules

| # | Rule | Validation |
|---|------|------------|
| 1 | Chuyen mon khop | `Staff.Specialty == Position.SpecialtyRequired` hoac Position khong yeu cau |
| 2 | Khong trung lich | `!HasScheduleConflict(staffId, examDate, shiftType)` - Bat buoc |
| 3 | Nhan vien hoat dong | `Staff.IsActive == true` - Bat buoc |
| 4 | Workload can bang | Uu tien staff co so ngay kham thang nay thap hon |
| 5 | Ty le co ban | 1 nhan su / 15 benh nhan |

### 7.3 AI Suggest Algorithm (Pseudo)

```csharp
async Task<List<SuggestedStaff>> AiSuggestStaffs(int groupId, int positionQuotaId) {
    // 1. Lay group va quota (Position + Required count)
    // 2. Query Staffs.Where(IsActive && (Specialty khop hoac khong yeu cau))
    // 3. Voi moi staff:
    //    - Check HasScheduleConflict (hard filter, loai neu trung)
    //    - Tinh Score:
    //      +10: Khop chuyen khoa chinh xac
    //      +Min(pastAssignments*2, 10): Co kinh nghiem vi tri nay
    //      -daysThisMonth*3: Giam diem neu da di nhieu ngay (can bang workload)
    // 4. Sort by Score desc
    // 5. Tra ve top `needed` ket qua
}

class SuggestedStaff {
    int StaffId; string FullName; string Specialty;
    int Score; int DaysThisMonth; int PastAssignments; string Reason;
}
```

### 7.4 API Endpoint

- `POST /api/ai/suggest-staffs`
  - Body: `{ groupId, positionQuotaId }`
  - Response: `[{ staffId, fullName, specialty, score, reason }]`
  - Auth: `DoanKham.AssignStaff` permission

---

## 8. Module Lich & Thong Bao

### 8.1 Lich Doan Kham (Calendar View)

#### 8.1.1 API

- `GET /api/calendar?month={m}&year={y}`
  - Response: `[{ date, groups: [{ id, name, companyName, status, assignedStaffs[] }] }]`
  - Auth: Admin/Manager xem tat ca; MedicalStaff chi xem co minh

- `GET /api/calendar/my-schedule?month={m}&year={y}`
  - Response: Lich ca nhan (doan nao, ngay nao, vi tri gi)

#### 8.1.2 Frontend

- FullCalendar hoac tu xay dung Grid view
- Ngay co doan: highlight + click xem chi tiet
- Filter: Theo trang thai, theo cong ty, theo vi tri can nhan su

### 8.2 Thong Bao (Notifications)

#### 8.2.1 Bang `Notifications`

| Cot | Kieu | Mo ta |
|-----|------|-------|
| Id | int | PK |
| UserId | int | FK Users (null = broadcast) |
| Type | string | ContractPending, GroupAssigned, ScheduleReminder, PayrollReady |
| Title | string | Tieu de |
| Message | string | Noi dung |
| IsRead | bool | false |
| CreatedAt | DateTime | Thoi diem tao |
| ActionUrl | string | Duong dan khi click |

#### 8.2.2 Cac Su Kien Tu Dong Gui Thong Bao

| Su kien | Nguoi nhan | Noi dung | Kenh |
|---------|-----------|----------|------|
| Hop dong tao xong, cho duyet | HCNS/Manager co quyen duyet | "Hop dong X can duyet" | SignalR + Badge |
| Hop dong duyet xong | Nguoi tao | "Hop dong X da duyet" | SignalR |
| Doan kham sap dien ra (T-1 ngay) | Nhan su trong doan | "Ngay mai ban co lich di kham..." | SignalR |
| Phan cong vao doan moi | Nhan su duoc them | "Ban duoc phan cong vao doan X" | SignalR |
| Bang luong da tao | Tung nhan su | "Bang luong T{month}/{year} da san sang" | SignalR |
| Vat tu sap het (duoi MinStock) | Admin/Kho | "Vat tu X chi con {qty}" | SignalR + Badge |

#### 8.2.3 SignalR Hub

```csharp
public class NotificationHub : Hub {
    // User ket noi -> join group theo UserId
    // Server gui: Clients.Group(userId).SendAsync("ReceiveNotification", notification)
    // Frontend Vue: lang nghe su kien, hien toast + cap nhat badge
}
```

#### 8.2.4 Reminder Job (Hangfire/Quartz)

```csharp
// Chay moi ngay luc 18:00
async Task DailyReminderJob() {
    var tomorrow = DateTime.Today.AddDays(1);
    var groups = await _context.MedicalGroups
        .Where(g => g.ExamDate.Date == tomorrow && g.Status != "Cancelled")
        .Include(g => g.Staffs)
        .ToListAsync();
    
    foreach (var group in groups) {
        foreach (var staff in group.Staffs) {
            await _notificationService.SendAsync(staff.UserId, "ScheduleReminder",
                "Nhac lich di kham",
                $"Ngay mai ({tomorrow:dd/MM}) ban co lich di kham: {group.GroupName}",
                $"/medical-groups/{group.Id}");
        }
    }
}
```

---

*End of Document*
