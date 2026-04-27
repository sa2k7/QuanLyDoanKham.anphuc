using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.Json;

namespace QuanLyDoanKham.API.Services.MedicalRecords
{
    public interface IMedicalReportPdfGenerator
    {
        Task<byte[]> GenerateReportAsync(int medicalRecordId);
    }

    public class MedicalReportPdfGenerator : IMedicalReportPdfGenerator
    {
        private readonly ApplicationDbContext _context;

        public MedicalReportPdfGenerator(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenerateReportAsync(int medicalRecordId)
        {
            var record = await _context.MedicalRecords
                .Include(r => r.Patient)
                .Include(r => r.MedicalGroup)
                    .ThenInclude(mg => mg!.HealthContract)
                        .ThenInclude(hc => hc!.Company)
                .FirstOrDefaultAsync(r => r.MedicalRecordId == medicalRecordId);

            if (record == null) throw new Exception("Không tìm thấy hồ sơ.");

            var results = await _context.ExamResults
                .Where(e => e.PatientId == record.PatientId && e.GroupId == record.GroupId)
                .ToListAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                    page.Header().Element(header => ComposeHeader(header, record));
                    page.Content().Element(content => ComposeContent(content, record, results));
                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
        }

        private void ComposeHeader(IContainer container, MedicalRecord record)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("BỆNH VIỆN / PHÒNG KHÁM ĐA KHOA AN PHÚC").FontSize(14).Bold();
                    column.Item().Text("Phòng khám đạt tiêu chuẩn quốc tế").FontSize(10).FontColor(Colors.Grey.Medium);
                });

                row.ConstantItem(150).Column(column =>
                {
                    column.Item().Text($"Số hồ sơ: {record.MedicalRecordId}").Bold();
                    column.Item().Text($"Ngày khám: {record.CheckInAt?.ToString("dd/MM/yyyy")}").FontSize(10);
                });
            });
        }

        private void ComposeContent(IContainer container, MedicalRecord record, List<ExamResult> results)
        {
            container.PaddingVertical(1, Unit.Centimetre).Column(column =>
            {
                column.Spacing(20);

                column.Item().AlignCenter().Text("KẾT QUẢ KHÁM SỨC KHỎE").FontSize(20).Bold().FontColor(Colors.Blue.Darken2);

                column.Item().Element(c => ComposePatientInfo(c, record));

                column.Item().Text("CHI TIẾT KẾT QUẢ KHÁM").FontSize(14).Bold().FontColor(Colors.Blue.Darken2);
                column.Item().Element(c => ComposeExamResults(c, results));

                column.Item().PaddingTop(20).Element(c => ComposeConclusion(c, record));
            });
        }

        private void ComposePatientInfo(IContainer container, MedicalRecord record)
        {
            container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(column =>
            {
                column.Spacing(5);

                column.Item().Row(row =>
                {
                    row.RelativeItem().Text($"Họ và tên: {record.Patient?.FullName}").Bold();
                    row.RelativeItem().Text($"Ngày sinh: {(record.Patient != null ? record.Patient.DateOfBirth.ToString("dd/MM/yyyy") : "N/A")}");
                    row.RelativeItem().Text($"Giới tính: {record.Patient?.Gender ?? "N/A"}");
                });

                column.Item().Row(row =>
                {
                    row.RelativeItem().Text($"CMND/CCCD: {record.Patient?.IDCardNumber ?? "N/A"}");
                    row.RelativeItem().Text($"Điện thoại: {record.Patient?.PhoneNumber ?? "N/A"}");
                });

                column.Item().Text($"Công ty: {record.MedicalGroup?.HealthContract?.Company?.CompanyName ?? "Khách lẻ"}").Bold().FontColor(Colors.Indigo.Medium);
            });
        }

        private void ComposeExamResults(IContainer container, List<ExamResult> results)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3); // Specialty / Type
                    columns.RelativeColumn(5); // Result JSON or text
                    columns.RelativeColumn(4); // Diagnosis
                });

                table.Header(header =>
                {
                    header.Cell().BorderBottom(1).PaddingBottom(5).Text("CHUYÊN KHOA").Bold();
                    header.Cell().BorderBottom(1).PaddingBottom(5).Text("KẾT QUẢ / CHỈ SỐ").Bold();
                    header.Cell().BorderBottom(1).PaddingBottom(5).Text("KẾT LUẬN").Bold();
                });

                foreach (var result in results)
                {
                    table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(result.ExamType).Bold();
                    
                    var formattedResult = ParseResultJson(result.Result);
                    table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(formattedResult).FontSize(10);
                    
                    table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(result.Diagnosis).FontSize(10).FontColor(Colors.Red.Darken2);
                }
            });
        }

        private string ParseResultJson(string rawJson)
        {
            if (string.IsNullOrEmpty(rawJson)) return "N/A";
            
            try
            {
                var options = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip };
                using var doc = JsonDocument.Parse(rawJson);
                var root = doc.RootElement;
                
                if (root.ValueKind == JsonValueKind.Object)
                {
                    var lines = new List<string>();
                    foreach (var property in root.EnumerateObject())
                    {
                        if (property.Name.Equals("Attachments", StringComparison.OrdinalIgnoreCase)) continue;
                        
                        var val = property.Value.ValueKind != JsonValueKind.Null ? property.Value.ToString() : "";
                        lines.Add($"{property.Name}: {val}");
                    }
                    return string.Join("\n", lines);
                }
                
                return rawJson; // fallback
            }
            catch
            {
                return rawJson;
            }
        }

        private void ComposeConclusion(IContainer container, MedicalRecord record)
        {
            container.Background(Colors.Grey.Lighten4).Padding(15).Column(column =>
            {
                column.Item().Text("KẾT LUẬN CỦA BÁC SĨ CHUYÊN KHOA").FontSize(12).Bold().Underline();
                column.Item().PaddingTop(5).Text("Vui lòng xem kết luận theo từng chuyên khoa ở trên.").Bold().FontColor(Colors.Red.Medium);
                
                column.Item().PaddingTop(30).Row(row =>
                {
                    row.RelativeItem(); // Spacer
                    row.ConstantItem(200).Column(c =>
                    {
                        c.Item().AlignCenter().Text($"Ngày {DateTime.Now.Day} tháng {DateTime.Now.Month} năm {DateTime.Now.Year}").FontSize(10).Italic();
                        c.Item().PaddingBottom(40).AlignCenter().Text("Bác sĩ Rà soát (QC)").Bold();
                        c.Item().AlignCenter().Text("(Ký và ghi rõ họ tên)");
                    });
                });
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(x =>
            {
                x.Span("Trang ");
                x.CurrentPageNumber();
                x.Span(" / ");
                x.TotalPages();
            });
        }
    }
}
