# PLAN: Medical Workflow Logic Overhaul

## 🎯 1. Mục Tiêu (Clear Goals)
Hoàn thiện 4 lỗ hổng quy trình nghiệp vụ khám chữa bệnh cốt lõi để đạt chuẩn Production:
1.  **Chuyển đổi dữ liệu Text sang Form có Cấu trúc (JSON):** Tối ưu nhập liệu riêng biệt cho các trạm đặc thù.
2.  **Mở rộng tính năng Cận lâm sàng:** Cho phép Bác sĩ chỉ định thêm dịch vụ ngoài hợp đồng, tự đẩy vào Queue mới và tính phí chênh lệch.
3.  **Tích hợp Hồ sơ Chứng từ Hình ảnh:** Hỗ trợ Upload File (X-quang, Siêu âm).
4.  **In Báo cáo Sức khỏe Y Tế (PDF):** Trả kết quả đầu ra trực quan khi hoàn thành QC.

---

## ⛓️ 2. Chuỗi Phụ Thuộc (Dependency Chains)
*   Giai đoạn 1 (Form cấu trúc) phải xong trước, vì nó định hình cách lưu dữ liệu của tất cả các Giai đoạn sau.
*   Giai đoạn 3 (File Hình Ảnh) phụ thuộc vào Entity `ExamResult` được mở rộng ở Giai đoạn 1.
*   Giai đoạn 4 (Báo cáo PDF) phải nằm cuối cùng, vì nó cần chờ Dữ liệu JSON (Giai đoạn 1) và Hình ảnh (Giai đoạn 3) để xuất ra file hoàn chỉnh.

---

## 🗺️ 3. Lộ Trình Triển Khai (Phase-by-Phase Breakdown)

### 🟢 Giai đoạn 1: Base Data Structure & UI Forms (Ưu tiên Cao nhất)
*   **DB/Models:** 
    *   Chuyển đổi `ExamResult.Result` sang JSON schema.
*   **API:**
    *   Cập nhật `SaveExamResultAsync` để validate JSON động.
*   **Frontend (`frontend-specialist`):**
    *   Tái cấu trúc `ExamForm.vue`. Thiết kế các loại Component mới: `VitalsForm`, `BloodTestForm`, `GeneralExamForm`.
    *   Map động Form dựa vào `stationCode`.

### 🟡 Giai đoạn 2: Báo Cáo Sức Khỏe PDF (Ưu tiên Cao)
*   **API:**
    *   Tạo `MedicalReportPdfGenerator.cs` áp dụng thư viện QuestPDF.
    *   Gom toàn bộ `ExamResult`, móc nối logic sinh layout PDF chuyên nghiệp chứa kết luận QC.
*   **Frontend:** 
    *   Thêm nút "In Hồ Sơ" trong `QcReview.vue` gọi về endpoint stream file PDF.

### 🟠 Giai đoạn 3: File Đính kèm Hình Ảnh Y Khoa (Ưu tiên Trung bình)
*   **DB/API:** 
    *   Thêm bảng `ExamResultAttachments` hoặc cột JSON FileUrl.
    *   Mở endpoint POST `/api/ExamResults/upload`.
*   **Frontend:** 
    *   Thêm Component Drag & Drop Upload trong `ExamForm.vue` dành cho trạm XQUANG / SIEU_AM.

### 🔴 Giai đoạn 4: Chỉ định thêm Dịch Vụ Cận Lâm Sàng (Ưu tiên Đóng gói)
*   **API:** 
    *   Bổ sung hàm `AddExtraStationAsync` vào `MedicalRecordStateMachine` để chèn Task mới và Broadcast SignalR cho phòng chờ.
    *   Điều chỉnh `SettlementCalculator` để cộng dồn khoản vượt mức hợp đồng.
*   **Frontend:**
    *   Thiết kế Modal "Chỉ định thêm" tại bàn khám Nội khoa.

> [!WARNING]
> Cần cực kỳ cẩn thận lúc can thiệp vào `MedicalRecordStateMachine` (Giai đoạn 4) vì nó chịu trách nhiệm điều phối Real-time Queue. Sai lệch có thể gây treo Hàng Chờ.

---

## 🔬 4. Kế hoạch Nghiệm thu (Verification Plan)
1.  **Dữ liệu (Automated):** Run Entity Framework tests đảm bảo `SaveExamResult` ghi đúng cấu trúc JSON.
2.  **UI/UX (Manual):** Bác sĩ mở màn hình Khám, giao diện form tự động biến đổi tùy theo mã Phòng.
3.  **PDF Output (Manual):** Sau khi hoàn tất khám, tải được File PDF rõ nét, có cả ảnh X-quang.
4.  **Tài chính (Manual):** Thử chỉ định thêm dịch vụ, kiểm tra báo cáo Quyết Toán xem Doanh Thu Phát Sinh có tăng tương ứng hay không.
