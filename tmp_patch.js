const fs = require('fs');

const path = 'd:/QuanLyDoanKham/QuanLyDoanKham.Api/Controllers/MedicalGroupsController.cs';
let content = fs.readFileSync(path, 'utf8');

const exportGroupsOld = `        // GET: api/MedicalGroups/export
        [HttpGet("export")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> ExportGroupsExcel()
        {
            var groups = await _context.MedicalGroups
                .Include(g => g.HealthContract)
                .ThenInclude(c => c.Company)
                .ToListAsync();

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("DanhSachDoanKham");
                worksheet.Cell(1, 1).Value = "Tên Đoàn Khám";
                worksheet.Cell(1, 2).Value = "Khách Hàng";
                worksheet.Cell(1, 3).Value = "Ngày Khám";
                worksheet.Cell(1, 4).Value = "Trạng Thái";
                
                var headerRange = worksheet.Range(1, 1, 1, 4);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.AliceBlue;

                for (int i = 0; i < groups.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = groups[i].GroupName;
                    worksheet.Cell(i + 2, 2).Value = groups[i].HealthContract.Company.ShortName ?? groups[i].HealthContract.Company.CompanyName;
                    worksheet.Cell(i + 2, 3).Value = groups[i].ExamDate.ToString("dd/MM/yyyy");
                    worksheet.Cell(i + 2, 4).Value = groups[i].Status;
                }
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DanhSachDoanKham.xlsx");
                }
            }
        }`;

const exportGroupsNew = `        // GET: api/MedicalGroups/export
        [HttpGet("export")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> ExportGroupsExcel()
        {
            var groups = await _context.MedicalGroups
                .Include(g => g.HealthContract)
                .ThenInclude(c => c.Company)
                .Include(g => g.StaffDetails)
                .Include(g => g.SupplyInventoryVouchers)
                    .ThenInclude(v => v.SupplyInventoryDetails)
                .ToListAsync();

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("THEODOIHDKSK");

                var headers = new string[] {
                    "STT", "Tên cty", "Người phụ trách", "Số điện thoại", "Số HĐ", 
                    "Thời gian khám", "Thời gian hoàn thành", "Csở ký HĐ", "Người phụ trách", 
                    "HĐ tạm tính", "Thanh toán", "PK", "NGOÀI", "Tổng chi phí nhân lực", 
                    "Tổng chi phí trang thiết bị, vật tư thực tế", "Tổng chi phí dụng cụ, hoá chất cho mỗi lần chạy mẫu", 
                    "Tổng chi phí hành chính", "Tổng chi phí hoa hồng", "THUẾ TNDN", "TỔNG THU"
                };

                for (int i = 0; i < headers.Length; i++) worksheet.Cell(1, i + 1).Value = headers[i];

                var headerRange = worksheet.Range(1, 1, 1, headers.Length);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;
                headerRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                headerRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                for (int i = 0; i < groups.Count; i++)
                {
                    int row = i + 2;
                    var group = groups[i];
                    var company = group.HealthContract?.Company;
                    var contract = group.HealthContract;

                    decimal laborCost = group.StaffDetails.Sum(s => s.CalculatedSalary);
                    decimal equipCost = group.SupplyInventoryVouchers.SelectMany(v => v.SupplyInventoryDetails).Sum(d => d.UnitPrice * d.Quantity);
                    decimal totalExpected = contract?.TotalAmount ?? 0;
                    decimal thanhToan = 0; 
                    decimal totalThu = totalExpected - laborCost - equipCost;

                    worksheet.Cell(row, 1).Value = i + 1;
                    worksheet.Cell(row, 2).Value = company?.ShortName ?? company?.CompanyName;
                    worksheet.Cell(row, 3).Value = company?.ContactPerson;
                    worksheet.Cell(row, 4).Value = company?.PhoneNumber ?? company?.ContactPhone;
                    worksheet.Cell(row, 5).Value = contract?.HealthContractId.ToString();
                    worksheet.Cell(row, 6).Value = group.ExamDate.ToString("dd/MM/yyyy");
                    worksheet.Cell(row, 7).Value = group.Status == "Finished" ? "HOÀN THÀNH" : "";
                    worksheet.Cell(row, 8).Value = "AN PHUC";
                    worksheet.Cell(row, 9).Value = company?.ContactPerson;
                    worksheet.Cell(row, 10).Value = totalExpected;
                    worksheet.Cell(row, 11).Value = thanhToan;
                    worksheet.Cell(row, 12).Value = ""; 
                    worksheet.Cell(row, 13).Value = ""; 
                    worksheet.Cell(row, 14).Value = laborCost;
                    worksheet.Cell(row, 15).Value = equipCost;
                    worksheet.Cell(row, 16).Value = 0; 
                    worksheet.Cell(row, 17).Value = 0; 
                    worksheet.Cell(row, 18).Value = 0; 
                    worksheet.Cell(row, 19).Value = 0; 
                    worksheet.Cell(row, 20).Value = totalThu;

                    var dtRange = worksheet.Range(row, 1, row, headers.Length);
                    dtRange.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                    dtRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TheoDoiHDKhachHang.xlsx");
                }
            }
        }`;


const exportStaffOld = `        // GET: api/MedicalGroups/{id}/export-staff
        [HttpGet("{id}/export-staff")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> ExportGroupStaffExcel(int id)
        {
            var group = await _context.MedicalGroups.FindAsync(id);
            if (group == null) return NotFound();

            var staff = await _context.GroupStaffDetails
                .Include(gsd => gsd.Staff)
                .Where(gsd => gsd.GroupId == id)
                .ToListAsync();

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("DoiNguDiKham");
                worksheet.Cell(1, 1).Value = $"DANH SÁCH NHÂN SỰ: {group.GroupName}";
                worksheet.Range(1, 1, 1, 5).Merge().Style.Font.Bold = true;
                
                worksheet.Cell(3, 1).Value = "Mã NV";
                worksheet.Cell(3, 2).Value = "Họ và Tên";
                worksheet.Cell(3, 3).Value = "Vị trí tại đoàn";
                worksheet.Cell(3, 4).Value = "Cấp bậc";
                worksheet.Cell(3, 5).Value = "Ca làm";

                worksheet.Range(3, 1, 3, 5).Style.Font.Bold = true;

                for (int i = 0; i < staff.Count; i++)
                {
                    worksheet.Cell(i + 4, 1).Value = staff[i].Staff.EmployeeCode;
                    worksheet.Cell(i + 4, 2).Value = staff[i].Staff.FullName;
                    worksheet.Cell(i + 4, 3).Value = staff[i].WorkPosition;
                    worksheet.Cell(i + 4, 4).Value = staff[i].Staff.JobTitle;
                    worksheet.Cell(i + 4, 5).Value = staff[i].ShiftType == 1 ? "Cả ngày" : "Nửa ngày";
                }
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"NhanSu_{group.GroupName}.xlsx");
                }
            }
        }`;

const exportStaffNew = `        // GET: api/MedicalGroups/{id}/export-staff
        [HttpGet("{id}/export-staff")]
        [AuthorizePermission("DoanKham.View")]
        public async Task<IActionResult> ExportGroupStaffExcel(int id)
        {
            var group = await _context.MedicalGroups
                .Include(g => g.HealthContract)
                .ThenInclude(c => c.Company)
                .FirstOrDefaultAsync(g => g.GroupId == id);
            
            if (group == null) return NotFound();

            var staff = await _context.GroupStaffDetails
                .Include(gsd => gsd.Staff)
                .Where(gsd => gsd.GroupId == id)
                .ToListAsync();

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("DoiNguDiKham");
                
                // Set column widths
                worksheet.Column(1).Width = 5;   
                worksheet.Column(2).Width = 20;  
                worksheet.Column(3).Width = 25;  
                worksheet.Column(4).Width = 10;  
                worksheet.Column(5).Width = 15;  
                worksheet.Column(6).Width = 15;  
                worksheet.Column(7).Width = 15;  
                worksheet.Column(8).Width = 15;  
                worksheet.Column(9).Width = 10;  
                worksheet.Column(10).Width = 10; 

                var company = group.HealthContract?.Company;

                worksheet.Cell("A1").Value = "HỆ THỐNG Y KHOA AN PHÚC";
                worksheet.Cell("A1").Style.Font.Bold = true;

                worksheet.Cell("A3").Value = "CÔNG TY TNHH PKĐK AN PHÚC SÀI GÒN";
                worksheet.Cell("A3").Style.Font.Bold = true;

                worksheet.Cell("A4").Value = "Địa chỉ: 2368 Ấp Thanh Hóa, xã Hố Nai 3, Trảng Bom, Đồng Nai";
                worksheet.Cell("A5").Value = "Mã số thuế: 3603393620 - Điện thoại liên hệ: 02513. 99 22 99";

                worksheet.Cell("A6").Value = "DANH SÁCH BÁC SỸ, NHÂN VIÊN Y TẾ KHÁM SỨC KHỎE";
                worksheet.Range("A6:J6").Merge();
                worksheet.Cell("A6").Style.Font.Bold = true;
                worksheet.Cell("A6").Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                worksheet.Cell("A6").Style.Font.FontSize = 14;

                worksheet.Cell("A7").Value = company?.CompanyName?.ToUpper();
                worksheet.Range("A7:J7").Merge();
                worksheet.Cell("A7").Style.Font.Bold = true;
                worksheet.Cell("A7").Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                worksheet.Cell("A8").Value = \`Địa chỉ: \${company?.Address}\`;
                worksheet.Range("A8:J8").Merge();
                worksheet.Cell("A8").Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                worksheet.Cell("A9").Value = \`Ngày khám: \${group.ExamDate:dd/MM/yyyy}\`;
                worksheet.Cell("C9").Value = \`THỨ \${(int)group.ExamDate.DayOfWeek + 1}\`;
                worksheet.Cell("D9").Value = "SỐ LƯỢNG: " + (group.HealthContract?.ExpectedQuantity ?? 0) + "NG";
                
                worksheet.Cell("A10").Value = \`Thời gian khám: CẢ NGÀY\`;
                worksheet.Cell("A11").Value = \`Thời xuất phát: \`;
                worksheet.Cell("A12").Value = \`Nội dung khám: TỔNG QUÁT\`;

                int row = 14;
                var h = new string[] { "STT", "CÔNG VIỆC (VỊ TRÍ)", "HỌ VÀ TÊN", "NĂM SINH", "CHỨC DANH", "ĐIỆN THOẠI", "ĐƠN VỊ", "GHI CHÚ", "ĐÓN TẠI", "KÝ TÊN" };
                for (int i=0; i<h.Length; i++) worksheet.Cell(row, i+1).Value = h[i];

                var headerRange = worksheet.Range(row, 1, row, 10);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                headerRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                row++;
                for (int i = 0; i < staff.Count; i++)
                {
                    worksheet.Cell(row, 1).Value = i + 1;
                    worksheet.Cell(row, 2).Value = staff[i].WorkPosition;
                    worksheet.Cell(row, 3).Value = staff[i].Staff.FullName;
                    worksheet.Cell(row, 4).Value = staff[i].Staff.BirthYear?.ToString();
                    worksheet.Cell(row, 5).Value = staff[i].Staff.JobTitle?.ToUpper();
                    worksheet.Cell(row, 6).Value = staff[i].Staff.PhoneNumber;
                    string dv = staff[i].Staff.EmployeeType == "NoiBo" ? "NỘI BỘ" : "THUÊ NGOÀI";
                    worksheet.Cell(row, 7).Value = dv;
                    worksheet.Cell(row, 8).Value = staff[i].Note;
                    
                    var dtRange = worksheet.Range(row, 1, row, 10);
                    dtRange.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                    dtRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", \`DSDOAN_\${group.ExamDate:ddMMyyyy}_\${group.GroupId}.xlsx\`);
                }
            }
        }`;

const aiOld = `            prompt.AppendLine("\\nKết quả trả về DUY NHẤT một mảng JSON format: [{\\"staffId\\": int, \\"workPosition\\": string, \\"shiftType\\": float, \\"reason\\": string}]");`;
const aiNew = `            prompt.AppendLine("\\nKết quả trả về DUY NHẤT một mảng JSON format: [{\\"staffId\\": int, \\"staffName\\": string, \\"workPosition\\": string, \\"shiftType\\": float, \\"reason\\": string}]");`;

content = content.replace(exportGroupsOld, exportGroupsNew);
content = content.replace(exportStaffOld, exportStaffNew);
content = content.replace(aiOld, aiNew);

fs.writeFileSync(path, content, 'utf8');
console.log('Patch success!');
