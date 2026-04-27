using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;
using System.Globalization;

namespace QuanLyDoanKham.API.Services.Imports
{
    public class PatientImportService : IPatientImportService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PatientImportService> _logger;

        public PatientImportService(ApplicationDbContext context, ILogger<PatientImportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ImportResultDto> ImportFromExcelAsync(IFormFile file, int medicalGroupId)
        {
            var result = new ImportResultDto();

            // Validate file
            if (file == null || file.Length == 0)
                throw new ValidationException("File khong duoc de trong");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".xlsx" && ext != ".xls")
                throw new ValidationException("Chi chap nhan file .xlsx hoac .xls");

            // Validate MedicalGroup
            var group = await _context.MedicalGroups.FindAsync(medicalGroupId);
            if (group == null)
                throw new ValidationException("Ma doan kham khong ton tai");
            if (group.Status == "Locked")
                throw new InvalidOperationException("Doan da khoa, khong the import");

            // Get existing phones in this group for duplicate check
            var existingPhones = await _context.Patients
                .Where(p => p.MedicalGroupId == medicalGroupId && !string.IsNullOrEmpty(p.PhoneNumber))
                .Select(p => p.PhoneNumber)
                .ToListAsync();

            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);

            // Skip header row (row 1), start from row 2
            var rows = worksheet.RowsUsed().Skip(1);

            foreach (var row in rows)
            {
                result.TotalRows++;
                var rowNum = row.RowNumber();

                try
                {
                    // Column mapping from ExportService:
                    // 1: STT, 2: HỌ TÊN, 3: NGÀY SINH, 4: GIỚI TÍNH, 5: CCCD, 6: SĐT, 7: PHÒNG BAN, 8: CHỨC NĂNG, 9: GHI CHÚ
                    var fullName = row.Cell(2).GetValue<string>()?.Trim();
                    if (string.IsNullOrWhiteSpace(fullName))
                    {
                        result.SkippedCount++;
                        continue;
                    }

                    var dobStr = row.Cell(3).GetValue<string>()?.Trim();
                    DateTime? dob = null;
                    if (!string.IsNullOrWhiteSpace(dobStr))
                    {
                        if (row.Cell(3).DataType == XLDataType.DateTime)
                        {
                            dob = row.Cell(3).GetDateTime();
                        }
                        else if (!DateTime.TryParseExact(dobStr, new[] { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "MM/dd/yyyy" },
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDob))
                        {
                            result.Errors.Add($"Dòng {rowNum}: Ngày sinh không hợp lệ '{dobStr}'");
                            result.ErrorCount++;
                            continue;
                        }
                        else
                        {
                            dob = parsedDob;
                        }
                    }

                    var genderRaw = row.Cell(4).GetValue<string>()?.Trim() ?? "";
                    var gender = NormalizeGender(genderRaw);

                    var idCard = row.Cell(5).GetValue<string>()?.Trim();
                    var phone = row.Cell(6).GetValue<string>()?.Trim();
                    var department = row.Cell(7).GetValue<string>()?.Trim();
                    var examFunction = row.Cell(8).GetValue<string>()?.Trim();
                    var notes = row.Cell(9).GetValue<string>()?.Trim();

                    // Check duplicate phone in same group (warning, still import)
                    if (!string.IsNullOrEmpty(phone) && existingPhones.Contains(phone))
                    {
                        result.Errors.Add($"Dòng {rowNum}: Warning - SĐT {phone} đã tồn tại trong đoàn");
                    }

                    var patient = new Patient
                    {
                        FullName = fullName,
                        DateOfBirth = dob ?? DateTime.MinValue,
                        Gender = gender,
                        PhoneNumber = string.IsNullOrWhiteSpace(phone) ? "N/A" : phone,
                        IDCardNumber = idCard,
                        Department = department,
                        HealthContractId = group.HealthContractId,
                        Source = "ExcelImport",
                        CreatedDate = DateTime.Now
                    };

                    _context.Patients.Add(patient);
                    
                    var record = new MedicalRecord
                    {
                        GroupId = group.GroupId,
                        PatientId = patient.PatientId,
                        FullName = patient.FullName,
                        DateOfBirth = patient.DateOfBirth,
                        Gender = patient.Gender,
                        IDCardNumber = patient.IDCardNumber,
                        Department = patient.Department,
                        QrToken = Guid.NewGuid().ToString("N"),
                        Status = "CREATED",
                        CreatedAt = DateTime.Now
                    };
                    
                    _context.MedicalRecords.Add(record);
                    
                    result.ImportedCount++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Dong {rowNum}: {ex.Message}");
                    result.ErrorCount++;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Imported {Imported} patients into group {GroupId}. Skipped: {Skipped}, Errors: {Errors}",
                result.ImportedCount, medicalGroupId, result.SkippedCount, result.ErrorCount);

            return result;
        }

        private static string NormalizeGender(string input)
        {
            var normalized = input.ToLowerInvariant().Trim();
            return normalized switch
            {
                "nam" or "male" or "m" => "Nam",
                "nu" or "nữ" or "female" or "f" => "Nu",
                _ => "Khac"
            };
        }
    }
}
