using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Services.Exports
{
    public class ExportService : IExportService
    {
        private readonly ApplicationDbContext _context;

        public ExportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> ExportPatientsByGroupAsync(int groupId)
        {
            var group = await _context.MedicalGroups
                .Include(g => g.HealthContract)
                .ThenInclude(c => c!.Company)
                .FirstOrDefaultAsync(g => g.GroupId == groupId);

            if (group == null)
                throw new Exception("Không tìm thấy đoàn khám.");

            // Nguồn dữ liệu từ MedicalRecords
            var patientRows = await _context.MedicalRecords
                .Include(mr => mr.Patient)
                .Where(mr => mr.GroupId == groupId)
                .OrderBy(mr => mr.FullName)
                .Select(mr => new
                {
                    mr.GroupId,
                    mr.PatientId,
                    mr.FullName,
                    mr.DateOfBirth,
                    mr.Gender,
                    mr.IDCardNumber,
                    PhoneNumber = mr.Patient != null ? mr.Patient.PhoneNumber : "",
                    mr.Department,
                    ExamFunction = "", // Dữ liệu khám mặc định trống
                    Notes = ""
                })
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("DSCN");

            // ── Title ──────────────────────────────────────────────────
            ws.Cell(1, 1).Value = "DANH SÁCH NGƯỜI KHÁM SỨC KHỎE";
            ws.Range(1, 1, 1, 9).Merge().Style
                .Font.SetBold(true).Font.SetFontSize(14)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#1e40af"))
                .Font.SetFontColor(XLColor.White);
            ws.Row(1).Height = 32;

            // ── Info đoàn ─────────────────────────────────────────────
            ws.Cell(3, 1).Value = "Đoàn khám:";
            ws.Cell(3, 2).Value = group.GroupName;
            ws.Range(3, 2, 3, 4).Merge().Style.Font.SetBold(true);

            ws.Cell(3, 6).Value = "Mã đoàn (GroupId):";
            ws.Cell(3, 7).Value = groupId;
            ws.Cell(3, 7).Style.Font.SetBold(true).Font.SetFontColor(XLColor.Red);

            ws.Cell(4, 1).Value = "Công ty:";
            ws.Cell(4, 2).Value = group.HealthContract?.Company?.CompanyName ?? "---";
            ws.Range(4, 2, 4, 4).Merge();

            ws.Cell(4, 6).Value = "Ngày khám:";
            ws.Cell(4, 7).Value = group.ExamDate.ToString("dd/MM/yyyy");
            ws.Cell(4, 7).Style.Font.SetBold(true);

            ws.Cell(5, 1).Value = "Tổng số người:";
            ws.Cell(5, 2).Value = patientRows.Count;
            ws.Cell(5, 2).Style.Font.SetBold(true);

            // ── Header bảng ───────────────────────────────────────────
            string[] headers = { "STT", "HỌ VÀ TÊN", "NGÀY SINH", "GIỚI TÍNH", "SỐ CCCD", "SỐ ĐIỆN THOẠI", "PHÒNG BAN", "CHỨC NĂNG KHÁM", "GHI CHÚ" };
            for (int col = 0; col < headers.Length; col++)
            {
                var cell = ws.Cell(7, col + 1);
                cell.Value = headers[col];
                cell.Style
                    .Font.SetBold(true)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#f1f5f9"))
                    .Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            }
            ws.Row(7).Height = 22;

            // ── Data rows ─────────────────────────────────────────────
            for (int i = 0; i < patientRows.Count; i++)
            {
                var p = patientRows[i];
                int row = i + 8;
                ws.Cell(row, 1).Value = i + 1;
                ws.Cell(row, 2).Value = p.FullName;
                ws.Cell(row, 3).Value = p.DateOfBirth?.ToString("dd/MM/yyyy") ?? "";
                ws.Cell(row, 4).Value = p.Gender;
                ws.Cell(row, 5).Value = p.IDCardNumber;
                ws.Cell(row, 6).Value = p.PhoneNumber;
                ws.Cell(row, 7).Value = p.Department;
                ws.Cell(row, 8).Value = p.ExamFunction;
                ws.Cell(row, 9).Value = p.Notes;

                var rowRange = ws.Range(row, 1, row, 9);
                rowRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                ws.Cell(row, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell(row, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            }

            ws.Columns().AdjustToContents();
            ws.Column(1).Width = 5;   // STT
            ws.Column(2).Width = 28;  // Họ tên
            ws.Column(5).Width = 14;  // CCCD

            // ── Sheet hướng dẫn import ────────────────────────────────
            var guideWs = workbook.Worksheets.Add("Huong dan import");
            guideWs.Cell(1, 1).Value = "HƯỚNG DẪN IMPORT LẠI VÀO HỆ THỐNG";
            guideWs.Cell(1, 1).Style.Font.SetBold(true).Font.SetFontSize(12);
            guideWs.Cell(3, 1).Value = "Để import danh sách này vào một đoàn khám:";
            guideWs.Cell(4, 1).Value = "1. Ghi nhớ Mã đoàn (GroupId) ở cột G dòng 3 của sheet DSCN";
            guideWs.Cell(5, 1).Value = "2. Vào mục Đoàn khám → Chi tiết đoàn → Tab Bệnh nhân → Nút Import";
            guideWs.Cell(6, 1).Value = "3. Upload file này, hệ thống sẽ tự nhận GroupId từ file";
            guideWs.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }

        public async Task<byte[]> ExportGroupPnlAsync(int groupId)
        {
            var group = await _context.MedicalGroups
                .Include(g => g.HealthContract)
                .ThenInclude(c => c!.Company)
                .FirstOrDefaultAsync(g => g.GroupId == groupId);

            if (group == null)
                throw new Exception("Đoàn khám không tồn tại.");

            // ── Tính chi phí ────────────────────────────────────────────
            var staffLines = await _context.GroupStaffDetails
                .Include(d => d.Staff)
                .Where(d => d.GroupId == groupId && d.WorkStatus == "Joined")
                .OrderBy(d => d.Staff!.FullName)
                .ToListAsync();

            var materialLines = await _context.StockMovements
                .Where(m => m.MedicalGroupId == groupId && m.MovementType == "OUT")
                .OrderBy(m => m.ItemName)
                .ToListAsync();

            var groupCost = await _context.GroupCosts
                .FirstOrDefaultAsync(c => c.GroupId == groupId);

            var patientCount = await _context.MedicalRecords
                .CountAsync(mr => mr.GroupId == groupId);

            var laborCost    = staffLines.Sum(d => d.CalculatedSalary);
            var materialCost = materialLines.Sum(m => m.TotalValue);
            var otherCost    = groupCost?.OtherCost ?? 0;
            var totalCost    = laborCost + materialCost + otherCost;

            var contractValue = group.HealthContract?.TotalAmount ?? 0;
            var profit        = contractValue - totalCost;
            var profitMargin  = contractValue > 0 ? profit / contractValue * 100 : 0;

            // ── Workbook ─────────────────────────────────────────────────
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("PNL");

            // ── Màu sắc chuẩn ─────────────────────────────────────────
            var headerBlue  = XLColor.FromHtml("#1e40af");
            var headerGreen = XLColor.FromHtml("#15803d");
            var headerRed   = XLColor.FromHtml("#991b1b");
            var lightBlue   = XLColor.FromHtml("#eff6ff");
            var lightGreen  = XLColor.FromHtml("#f0fdf4");
            var lightRed    = XLColor.FromHtml("#fef2f2");
            var altRow      = XLColor.FromHtml("#f8fafc");

            // ── Tiêu đề chính ─────────────────────────────────────────
            ws.Cell(1, 1).Value = "BÁO CÁO CHI PHÍ & LỢI NHUẬN ĐOÀN KHÁM";
            ws.Range(1, 1, 1, 8).Merge().Style
                .Font.SetBold(true).Font.SetFontSize(15)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Fill.SetBackgroundColor(headerBlue)
                .Font.SetFontColor(XLColor.White);
            ws.Row(1).Height = 36;

            // ── Thông tin đoàn ────────────────────────────────────────
            void SetInfoRow(int row, string label, string value)
            {
                ws.Cell(row, 1).Value = label;
                ws.Cell(row, 1).Style.Font.SetBold(true).Font.SetFontColor(XLColor.FromHtml("#64748b"));
                ws.Cell(row, 2).Value = value;
                ws.Range(row, 2, row, 4).Merge();
            }

            SetInfoRow(3, "Đoàn khám:",  group.GroupName);
            SetInfoRow(4, "Công ty:",    group.HealthContract?.Company?.CompanyName ?? "—");
            SetInfoRow(5, "Ngày khám:",  group.ExamDate.ToString("dd/MM/yyyy"));
            SetInfoRow(6, "Số BN:",      $"{patientCount} người");

            ws.Cell(3, 5).Value = "Mã đoàn:";  ws.Cell(3, 5).Style.Font.SetBold(true);
            ws.Cell(3, 6).Value = groupId;
            ws.Cell(4, 5).Value = "Ngày xuất:"; ws.Cell(4, 5).Style.Font.SetBold(true);
            ws.Cell(4, 6).Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            // ── Section 1: Tóm tắt P&L ───────────────────────────────
            int r = 8;
            ws.Cell(r, 1).Value = "TỔNG HỢP P&L";
            ws.Range(r, 1, r, 8).Merge().Style
                .Font.SetBold(true).Fill.SetBackgroundColor(headerBlue)
                .Font.SetFontColor(XLColor.White)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Row(r).Height = 22;
            r++;

            void AddSummaryRow(int row, string label, decimal value, XLColor bgColor, bool isBold = false)
            {
                ws.Cell(row, 1).Value = label;
                ws.Cell(row, 4).Value = value;
                ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0\" đ\"";
                var rng = ws.Range(row, 1, row, 8);
                rng.Style.Fill.SetBackgroundColor(bgColor);
                if (isBold) rng.Style.Font.SetBold(true);
                ws.Range(row, 1, row, 3).Merge();
                ws.Cell(row, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                ws.Row(row).Height = 20;
            }

            AddSummaryRow(r++, "💰  Doanh thu hợp đồng",  contractValue, lightBlue, true);
            AddSummaryRow(r++, "👥  Chi phí nhân sự",      laborCost,     lightRed);
            AddSummaryRow(r++, "📦  Chi phí vật tư",       materialCost,  lightRed);
            AddSummaryRow(r++, "🔧  Chi phí khác",         otherCost,     lightRed);
            AddSummaryRow(r++, "📊  Tổng chi phí",         totalCost,     XLColor.FromHtml("#fecaca"), true);

            // Lợi nhuận
            var profitBg = profit >= 0 ? lightGreen : lightRed;
            ws.Cell(r, 1).Value = profit >= 0 ? "✅  Lợi nhuận ròng" : "❌  Lỗ ròng";
            ws.Range(r, 1, r, 3).Merge();
            ws.Cell(r, 4).Value = profit;
            ws.Cell(r, 4).Style.NumberFormat.Format = "#,##0\" đ\"";
            ws.Cell(r, 5).Value = $"{profitMargin:0.0}%";
            ws.Cell(r, 5).Style.Font.SetBold(true);
            ws.Range(r, 1, r, 8).Style.Fill.SetBackgroundColor(profitBg).Font.SetBold(true).Font.SetFontSize(11);
            ws.Cell(r, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
            ws.Row(r).Height = 24;
            r += 2;

            // ── Section 2: Chi tiết nhân sự ──────────────────────────
            ws.Cell(r, 1).Value = "CHI TIẾT NHÂN SỰ";
            ws.Range(r, 1, r, 8).Merge().Style
                .Font.SetBold(true).Fill.SetBackgroundColor(XLColor.FromHtml("#374151"))
                .Font.SetFontColor(XLColor.White)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Row(r).Height = 20; r++;

            string[] staffHeaders = { "STT", "Họ và Tên", "Ca làm (hệ số)", "Đơn giá/ngày", "Thành tiền", "", "", "" };
            for (int c = 0; c < 5; c++)
            {
                ws.Cell(r, c + 1).Value = staffHeaders[c];
                ws.Cell(r, c + 1).Style
                    .Font.SetBold(true)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#f1f5f9"))
                    .Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            }
            ws.Row(r).Height = 20; r++;

            int stt = 1;
            foreach (var detail in staffLines)
            {
                bool isAlt = stt % 2 == 0;
                ws.Cell(r, 1).Value = stt++;
                ws.Cell(r, 2).Value = detail.Staff?.FullName ?? "—";
                ws.Cell(r, 3).Value = detail.ShiftType == 1.0 ? "Cả ngày (1.0)" : "Nửa ngày (0.5)";
                ws.Cell(r, 4).Value = detail.Staff?.DailyRate ?? 0;
                ws.Cell(r, 4).Style.NumberFormat.Format = "#,##0\" đ\"";
                ws.Cell(r, 5).Value = detail.CalculatedSalary;
                ws.Cell(r, 5).Style.NumberFormat.Format = "#,##0\" đ\"";
                for (int c = 1; c <= 5; c++)
                {
                    ws.Cell(r, c).Style.Border.SetOutsideBorder(XLBorderStyleValues.Hair);
                    if (isAlt) ws.Cell(r, c).Style.Fill.SetBackgroundColor(altRow);
                }
                ws.Cell(r, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell(r, 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                r++;
            }

            if (!staffLines.Any())
            {
                ws.Cell(r, 1).Value = "(Chưa có dữ liệu nhân sự)";
                ws.Range(r, 1, r, 5).Merge();
                ws.Cell(r, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Font.SetItalic(true).Font.SetFontColor(XLColor.Gray);
                r++;
            }

            // Tổng nhân sự
            ws.Cell(r, 1).Value = "TỔNG CHI PHÍ NHÂN SỰ";
            ws.Range(r, 1, r, 4).Merge();
            ws.Cell(r, 5).Value = laborCost;
            ws.Cell(r, 5).Style.NumberFormat.Format = "#,##0\" đ\"";
            ws.Range(r, 1, r, 5).Style.Font.SetBold(true).Fill.SetBackgroundColor(lightRed);
            r += 2;

            // ── Section 3: Chi tiết vật tư ───────────────────────────
            ws.Cell(r, 1).Value = "CHI TIẾT VẬT TƯ Y TẾ";
            ws.Range(r, 1, r, 8).Merge().Style
                .Font.SetBold(true).Fill.SetBackgroundColor(XLColor.FromHtml("#374151"))
                .Font.SetFontColor(XLColor.White)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Row(r).Height = 20; r++;

            string[] matHeaders = { "STT", "Tên vật tư", "Đơn vị", "Số lượng", "Đơn giá", "Thành tiền", "", "" };
            for (int c = 0; c < 6; c++)
            {
                ws.Cell(r, c + 1).Value = matHeaders[c];
                ws.Cell(r, c + 1).Style
                    .Font.SetBold(true)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#f1f5f9"))
                    .Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            }
            ws.Row(r).Height = 20; r++;

            stt = 1;
            foreach (var mat in materialLines)
            {
                bool isAlt = stt % 2 == 0;
                ws.Cell(r, 1).Value = stt++;
                ws.Cell(r, 2).Value = mat.ItemName;
                ws.Cell(r, 3).Value = mat.Unit;
                ws.Cell(r, 4).Value = mat.Quantity;
                ws.Cell(r, 5).Value = mat.UnitPrice;
                ws.Cell(r, 5).Style.NumberFormat.Format = "#,##0\" đ\"";
                ws.Cell(r, 6).Value = mat.TotalValue;
                ws.Cell(r, 6).Style.NumberFormat.Format = "#,##0\" đ\"";
                for (int c = 1; c <= 6; c++)
                {
                    ws.Cell(r, c).Style.Border.SetOutsideBorder(XLBorderStyleValues.Hair);
                    if (isAlt) ws.Cell(r, c).Style.Fill.SetBackgroundColor(altRow);
                }
                ws.Cell(r, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell(r, 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell(r, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                r++;
            }

            if (!materialLines.Any())
            {
                ws.Cell(r, 1).Value = "(Chưa có dữ liệu vật tư)";
                ws.Range(r, 1, r, 6).Merge();
                ws.Cell(r, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Font.SetItalic(true).Font.SetFontColor(XLColor.Gray);
                r++;
            }

            // Tổng vật tư
            ws.Cell(r, 1).Value = "TỔNG CHI PHÍ VẬT TƯ";
            ws.Range(r, 1, r, 5).Merge();
            ws.Cell(r, 6).Value = materialCost;
            ws.Cell(r, 6).Style.NumberFormat.Format = "#,##0\" đ\"";
            ws.Range(r, 1, r, 6).Style.Font.SetBold(true).Fill.SetBackgroundColor(lightRed);
            r += 2;

            // ── Footer ────────────────────────────────────────────────
            ws.Cell(r, 1).Value = $"Báo cáo xuất lúc: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
            ws.Range(r, 1, r, 8).Merge();
            ws.Cell(r, 1).Style.Font.SetItalic(true).Font.SetFontColor(XLColor.Gray)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

            // ── Cột rộng ─────────────────────────────────────────────
            ws.Column(1).Width = 5;
            ws.Column(2).Width = 30;
            ws.Column(3).Width = 16;
            ws.Column(4).Width = 18;
            ws.Column(5).Width = 18;
            ws.Column(6).Width = 18;

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }

        public async Task<byte[]> ExportPayrollAsync(int month, int year)
        {
            // Lay tat ca GroupStaffDetails trong thang, group by Staff
            var details = await _context.GroupStaffDetails
                .Include(g => g.Staff)
                .Include(g => g.MedicalGroup)
                .Where(g => g.MedicalGroup.ExamDate.Month == month
                         && g.MedicalGroup.ExamDate.Year == year
                         && g.WorkStatus == "Joined")
                .ToListAsync();

            var staffGroups = details
                .GroupBy(g => g.StaffId)
                .Select(g => new
                {
                    Staff = g.First().Staff,
                    TotalDays = g.Sum(x => x.ShiftType),
                    TotalSalary = g.Sum(x => x.CalculatedSalary)
                })
                .ToList();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("BangLuong");

            ws.Cell(1, 1).Value = $"BANG LUONG THANG {month}/{year}";
            ws.Range(1, 1, 1, 5).Merge().Style.Font.Bold = true;

            ws.Cell(3, 1).Value = "Ma NV";
            ws.Cell(3, 2).Value = "Ho ten";
            ws.Cell(3, 3).Value = "Cong thuc te";
            ws.Cell(3, 4).Value = "Don gia/ngay";
            ws.Cell(3, 5).Value = "Luong thuc linh";
            ws.Row(3).Style.Font.Bold = true;

            for (int i = 0; i < staffGroups.Count; i++)
            {
                var item = staffGroups[i];
                ws.Cell(i + 4, 1).Value = item.Staff?.EmployeeCode ?? "";
                ws.Cell(i + 4, 2).Value = item.Staff?.FullName ?? "";
                ws.Cell(i + 4, 3).Value = item.TotalDays;
                ws.Cell(i + 4, 4).Value = item.Staff?.DailyRate ?? 0;
                ws.Cell(i + 4, 4).Style.NumberFormat.Format = "#,##0";
                ws.Cell(i + 4, 5).Value = item.TotalSalary;
                ws.Cell(i + 4, 5).Style.NumberFormat.Format = "#,##0";
            }

            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }
    }
}
