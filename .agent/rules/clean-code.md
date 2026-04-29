---
trigger: always_on
---

# CLEAN-CODE.MD — Quy tắc viết code tối thiểu

> **Áp dụng cho**: TOÀN BỘ code Backend (.NET) và Frontend (Vue) trong dự án QuanLyDoanKham.  
> **Mục tiêu**: Code dễ đọc, dễ sửa, dễ test — không over-engineer.

---

## ✅ CHECKLIST BẮT BUỘC (Đọc trước khi code)

Trước khi viết hoặc sửa bất kỳ file nào, Agent PHẢI tự hỏi:

- [ ] **Tên có nói lên ý nghĩa?** → Nếu cần comment để giải thích tên, hãy đổi tên.
- [ ] **Logic này đã tồn tại chỗ khác chưa?** → Nếu có, tách hàm dùng chung.
- [ ] **Luồng chính đã có test chưa?** → Mỗi business logic quan trọng cần ít nhất 1 test.

---

## 📏 NGUYÊN TẮC 1 — ĐẶT TÊN RÕ RÀNG

### ✅ Đúng / ❌ Sai

```csharp
// ❌ Mơ hồ
var t = record.StationTasks.FirstOrDefault(x => x.StationCode == code);
var flag = tasks.All(t => t.Status == "COMPLETED");

// ✅ Rõ ràng — đọc xong biết ngay đang làm gì
var stationTask = record.StationTasks.FirstOrDefault(t => t.StationCode == stationCode);
var allTasksCompleted = tasks.All(t => t.Status == "COMPLETED");
```

```javascript
// ❌ Mơ hồ
const d = res.data
const fn = () => api.post('/save', payload)

// ✅ Rõ ràng
const examResult = res.data
const submitExamResult = () => api.post('/api/ExamResults/save', payload)
```

### Quy tắc đặt tên cụ thể

| Loại | Quy tắc | Ví dụ |
|---|---|---|
| **Hàm** | Động từ + Danh từ | `LoadRecordWithTasks`, `fetchActiveQueue` |
| **Biến bool** | `is/has/can` tiền tố | `isCompleted`, `hasPermission`, `canSubmit` |
| **Hàm private** | Mô tả đúng 1 việc | `CompleteStationTask()`, `UpsertExamResult()` |
| **Hàm async** | Hậu tố `Async` | `SaveExamResultAsync()`, `fetchQueueAsync()` |

---

## 🔧 NGUYÊN TẮC 2 — TÁCH LOGIC LẶP LẠI

Logic lặp ≥ 2 nơi PHẢI được tách ra.

### Backend — Private helpers trong Service

```csharp
// ❌ Viết lại cùng một query EF ở nhiều chỗ
var record = await _context.MedicalRecords
    .Include(r => r.StationTasks)
    .FirstOrDefaultAsync(r => r.MedicalRecordId == id);

// ✅ Tách ra hàm dùng chung
private Task<MedicalRecord?> LoadRecordWithTasksAsync(int recordId)
    => _context.MedicalRecords
        .Include(r => r.StationTasks)
        .FirstOrDefaultAsync(r => r.MedicalRecordId == recordId);
```

### Frontend — Composables hoặc utils

```javascript
// ❌ Lặp logic format ngày ở nhiều component
const date = new Date(dateStr).toLocaleDateString('vi-VN')

// ✅ Dùng hàm dùng chung
// utils/dateFormatter.js
export const formatViDate = (dateStr) =>
  new Date(dateStr).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
```

---

## 🧪 NGUYÊN TẮC 3 — TEST LUỒNG CHÍNH

**Không cần 100% coverage.** Chỉ cần test các Business Logic quan trọng:

### Ưu tiên test:
1. **Happy Path** — Luồng thành công bình thường.
2. **Guard Clause** — Trường hợp dữ liệu không hợp lệ / không tìm thấy.
3. **Side Effect** — Tính toán/thay đổi trạng thái tự động (state machine).

### Mẫu test chuẩn (xUnit)

```csharp
[Fact]
public async Task <Hàm>_When<Điều kiện>_<Kết quả mong đợi>()
{
    // ARRANGE — Chuẩn bị dữ liệu
    // ACT     — Thực thi logic
    // ASSERT  — Kiểm tra kết quả
}
```

---

## 🚫 CÁC ĐIỀU KHÔNG ĐƯỢC LÀM

1. **Không viết comment giải thích `what`** (code tự nói), chỉ dùng comment cho `why` (lý do nghiệp vụ).
2. **Không để lại `// TODO` không có deadline** — Fix ngay hoặc ghi vào `ERRORS.md`.
3. **Không đặt tên `data`, `result`, `item`, `obj`, `temp`** — Luôn đặt tên cụ thể.
4. **Không viết hàm quá 30 dòng** — Nếu vượt, tách thành private helpers.
5. **Không dùng magic string lặp lại** — Trích xuất thành constant.

```csharp
// ❌ Magic string
if (record.Status == "QC_PENDING") { ... }
if (record.Status == "QC_PENDING") { ... } // Chỗ khác lặp lại

// ✅ Constant dùng chung
public static class RecordStatus
{
    public const string QcPending = "QC_PENDING";
    public const string Completed = "COMPLETED";
}
```

---

## 🏥 NGỮ CẢNH DỰ ÁN (QuanLyDoanKham)

- **Domains chính**: MedicalRecord, StationTask, ExamResult, MedicalGroup
- **State Machine**: `CREATED → CHECKED_IN → IN_PROGRESS → QC_PENDING → QC_PASSED → REPORTED → CLOSED`
- **Test project**: `d:\QuanLyDoanKham\QuanLyDoanKham.Api.Tests`
- **Test pattern**: Dùng EF Core InMemory v8.0.0 + xUnit

---

*Rule này được đọc tự động mỗi lần Agent thực hiện tác vụ code trong dự án này.*
