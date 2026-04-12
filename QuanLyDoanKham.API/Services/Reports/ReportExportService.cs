using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Services;

namespace QuanLyDoanKham.API.Services.Reports
{
    public class ReportExportService : IReportExportService
    {
        private readonly IReportingService _reportingService;

        public ReportExportService(IReportingService reportingService)
        {
            _reportingService = reportingService;
            // QuestPDF Community License for non-commercial or small entities
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GenerateDashboardPdfAsync(DateTime? startDate, DateTime? endDate)
        {
            var kpis = await _reportingService.GetDashboardKpisAsync(startDate, endDate);
            var financial = await _reportingService.GetFinancialReportAsync(startDate, endDate);
            var staff = await _reportingService.GetStaffEfficiencyAsync(startDate, endDate);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));

                    // HEADER
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("HỆ THỐNG QUẢN LÝ ĐOÀN KHÁM")
                                .FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                            col.Item().Text("BÁO CÁO PHÂN TÍCH HIỆU SUẤT VẬN HÀNH")
                                .FontSize(14).Medium().FontColor(Colors.Grey.Medium);
                            col.Item().PaddingTop(5).Text($"Thời gian: {(startDate?.ToString("dd/MM/yyyy") ?? "...")} - {(endDate?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy"))}")
                                .FontSize(9).Italic();
                        });

                        row.ConstantItem(80).Height(80).Placeholder(); // Logo Placeholder
                    });

                    // CONTENT
                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        // 1. KPI Grid (Simulated visually)
                        col.Item().PaddingBottom(15).Row(row =>
                        {
                            row.RelativeItem().Component(new KpiBox("Doanh thu dự kiến", kpis.TotalRevenue.ToString("N0") + "đ", Colors.Indigo.Lighten5, Colors.Indigo.Medium));
                            row.ConstantItem(10);
                            row.RelativeItem().Component(new KpiBox("Lợi nhuận ròng", kpis.NetProfit.ToString("N0") + "đ", Colors.Green.Lighten5, Colors.Green.Medium));
                            row.ConstantItem(10);
                            row.RelativeItem().Component(new KpiBox("Tiến độ", kpis.CompletionRate + "%", Colors.Blue.Lighten5, Colors.Blue.Medium));
                        });

                        col.Item().PaddingBottom(10).Text("1. PHÂN TÍCH TÀI CHÍNH").FontSize(12).SemiBold().Underline();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(150);
                                columns.RelativeColumn();
                                columns.ConstantColumn(120);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Hạng mục");
                                header.Cell().Element(CellStyle).Text("Giá trị");
                                header.Cell().Element(CellStyle).Text("Tỷ lệ");

                                static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                            });

                            table.Cell().Element(DataStyle).Text("Doanh thu (HĐ)");
                            table.Cell().Element(DataStyle).Text(financial.Revenue.ToString("N0") + " VNĐ");
                            table.Cell().Element(DataStyle).Text("100%");
 
                            table.Cell().Element(DataStyle).Text("Chi phí nhân sự");
                            table.Cell().Element(DataStyle).Text(financial.StaffCost.ToString("N0") + " VNĐ");
                            table.Cell().Element(DataStyle).Text(((financial.StaffCost / (financial.Revenue != 0 ? financial.Revenue : 1)) * 100).ToString("N1") + "%");
 
                            table.Cell().Element(DataStyle).Text("Lợi nhuận gộp");
                            table.Cell().Element(DataStyle).Text((financial.Revenue - financial.StaffCost).ToString("N0") + " VNĐ").FontColor(Colors.Green.Medium).SemiBold();
                            table.Cell().Element(DataStyle).Text(financial.Margin.ToString("N1") + "%").SemiBold().FontColor(Colors.Green.Medium);

                            static IContainer DataStyle(IContainer container) => container.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten3);
                        });

                        col.Item().PaddingTop(20).PaddingBottom(10).Text("2. HIỆU SUẤT NHÂN SỰ TOP ĐẦU").FontSize(12).SemiBold().Underline();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.ConstantColumn(100);
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(120);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderStyle).Text("Nhân viên");
                                header.Cell().Element(HeaderStyle).Text("Vị trí");
                                header.Cell().Element(HeaderStyle).Text("Số đoàn");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Tổng thu nhập");

                                static IContainer HeaderStyle(IContainer container) => container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                            });

                            foreach (var s in staff.Take(5))
                            {
                                table.Cell().Element(ItemStyle).Text(s.StaffName);
                                table.Cell().Element(ItemStyle).Text(s.Role);
                                table.Cell().Element(ItemStyle).AlignCenter().Text(s.TotalGroups.ToString());
                                table.Cell().Element(ItemStyle).AlignRight().Text(s.TotalSalary.ToString("N0") + " VNĐ");
                            }

                            static IContainer ItemStyle(IContainer container) => container.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten3);
                        });
                    });

                    page.Footer().AlignCenter().Row(row => {
                        row.RelativeItem().Text(x => {
                            x.Span("Báo cáo được trích xuất tự động tại ");
                            x.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).SemiBold();
                        });
                        row.ConstantItem(50).AlignRight().Text(x => {
                            x.Span("Trang ");
                            x.CurrentPageNumber();
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> GenerateDashboardExcelAsync(DateTime? startDate, DateTime? endDate)
        {
            var kpis = await _reportingService.GetDashboardKpisAsync(startDate, endDate);
            var financial = await _reportingService.GetFinancialReportAsync(startDate, endDate);
            var staff = await _reportingService.GetStaffEfficiencyAsync(startDate, endDate);

            using var workbook = new XLWorkbook();
            
            // Sheet 1: Tổng quan
            var wsKpi = workbook.Worksheets.Add("Tổng quan");
            wsKpi.Cell("A1").Value = "BÁO CÁO TỔNG QUAN HỆ THỐNG";
            wsKpi.Range("A1:C1").Merge().Style.Font.Bold = true;
            wsKpi.Range("A1:C1").Style.Font.FontSize = 16;

            wsKpi.Cell("A3").Value = "Thời gian:";
            wsKpi.Cell("B3").Value = $"{(startDate?.ToShortDateString() ?? "...")} - {(endDate?.ToShortDateString() ?? DateTime.Now.ToShortDateString())}";

            var kpiTable = wsKpi.Range("A5:B9");
            wsKpi.Cell("A5").Value = "Chỉ số";
            wsKpi.Cell("B5").Value = "Giá trị";
            wsKpi.Cell("A6").Value = "Doanh thu dự kiến";
            wsKpi.Cell("B6").Value = kpis.TotalRevenue;
            wsKpi.Cell("A7").Value = "Lợi nhuận ròng";
            wsKpi.Cell("B7").Value = kpis.NetProfit;
            wsKpi.Cell("A8").Value = "Tỷ lệ hoàn thành";
            wsKpi.Cell("B8").Value = kpis.CompletionRate / 100;
            wsKpi.Cell("B8").Style.NumberFormat.Format = "0.0%";

            wsKpi.Columns().AdjustToContents();

            // Sheet 2: Nhân sự
            var wsStaff = workbook.Worksheets.Add("Hiệu suất Nhân sự");
            wsStaff.Cell(1, 1).Value = "Tên nhân sự";
            wsStaff.Cell(1, 2).Value = "Vị trí";
            wsStaff.Cell(1, 3).Value = "Số đoàn tham gia";
            wsStaff.Cell(1, 4).Value = "Ngày công";
            wsStaff.Cell(1, 5).Value = "Tổng thu nhập";
            wsStaff.Range("A1:E1").Style.Font.Bold = true;
            wsStaff.Range("A1:E1").Style.Fill.BackgroundColor = XLColor.LightBlue;

            int row = 2;
            foreach (var s in staff)
            {
                wsStaff.Cell(row, 1).Value = s.StaffName;
                wsStaff.Cell(row, 2).Value = s.Role;
                wsStaff.Cell(row, 3).Value = s.TotalGroups;
                wsStaff.Cell(row, 4).Value = s.DaysWorked;
                wsStaff.Cell(row, 5).Value = s.TotalSalary;
                row++;
            }
            wsStaff.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }

    // Helper components for QuestPDF
    public class KpiBox : IComponent
    {
        public string Title { get; }
        public string Value { get; }
        public string BgColor { get; }
        public string TextColor { get; }

        public KpiBox(string title, string value, string bgColor, string textColor)
        {
            Title = title;
            Value = value;
            BgColor = bgColor;
            TextColor = textColor;
        }

        public void Compose(IContainer container)
        {
            container
                .Background(BgColor)
                .Padding(10)
                .Column(column =>
                {
                    column.Item().Text(Title.ToUpper()).FontSize(8).SemiBold().FontColor(TextColor);
                    column.Item().PaddingTop(2).Text(Value).FontSize(14).Black().FontColor(TextColor);
                });
        }
    }
}
