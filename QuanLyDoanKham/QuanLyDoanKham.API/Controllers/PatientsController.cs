using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyDoanKham.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PatientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PatientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Patients/by-contract/5 — Admin, MedicalStaff và Customer (nếu đúng công ty) được xem
        [HttpGet("by-contract/{contractId}")]
        [Authorize(Roles = "Admin,MedicalStaff,Customer")]
        public async Task<ActionResult<IEnumerable<PatientDto>>> GetPatientsByContract(int contractId)
        {
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;

            // Validate access if Customer
            if (role == "Customer" && !string.IsNullOrEmpty(companyIdClaim))
            {
                int companyId = int.Parse(companyIdClaim);
                var contract = await _context.Contracts.FindAsync(contractId);
                if (contract == null || contract.CompanyId != companyId)
                    return StatusCode(403, "Bạn không có quyền xem bệnh nhân của hợp đồng này.");
            }

            var patients = await _context.Patients
                .Where(p => p.HealthContractId == contractId)
                .Select(p => new PatientDto
                {
                    PatientId = p.PatientId,
                    HealthContractId = p.HealthContractId,
                    FullName = p.FullName,
                    DateOfBirth = p.DateOfBirth,
                    Gender = p.Gender,
                    IDCardNumber = p.IDCardNumber,
                    PhoneNumber = p.PhoneNumber,
                    Department = p.Department,
                    CreatedDate = p.CreatedDate
                })
                .ToListAsync();

            return Ok(patients);
        }

        // GET: api/Patients/5 — Admin, MedicalStaff và Customer được xem
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,MedicalStaff,Customer")]
        public async Task<ActionResult<PatientDto>> GetPatient(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.HealthContract)
                .FirstOrDefaultAsync(p => p.PatientId == id);

            if (patient == null)
                return NotFound();

            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;

            if (role == "Customer" && !string.IsNullOrEmpty(companyIdClaim))
            {
                int companyId = int.Parse(companyIdClaim);
                if (patient.HealthContract.CompanyId != companyId)
                    return StatusCode(403, "Bạn không có quyền xem thông tin bệnh nhân này.");
            }

            return Ok(new PatientDto
            {
                PatientId = patient.PatientId,
                HealthContractId = patient.HealthContractId,
                FullName = patient.FullName,
                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender,
                IDCardNumber = patient.IDCardNumber,
                PhoneNumber = patient.PhoneNumber,
                Department = patient.Department,
                CreatedDate = patient.CreatedDate
            });
        }

        // POST: api/Patients — Chỉ Admin được tạo
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Patient>> CreatePatient(PatientDto dto)
        {
            var patient = new Patient
            {
                HealthContractId = dto.HealthContractId,
                FullName = dto.FullName,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                IDCardNumber = dto.IDCardNumber,
                PhoneNumber = dto.PhoneNumber,
                Department = dto.Department,
                CreatedDate = DateTime.Now
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            dto.PatientId = patient.PatientId;
            return CreatedAtAction(nameof(GetPatient), new { id = patient.PatientId }, dto);
        }

        // POST: api/Patients/5/exam-result
        [HttpPost("{id}/exam-result")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ExamResult>> AddExamResult(int id, ExamResultDto dto)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
                return NotFound("Patient not found");

            var examResult = new ExamResult
            {
                PatientId = id,
                GroupId = dto.GroupId,
                ExamType = dto.ExamType,
                Result = dto.Result,
                Diagnosis = dto.Diagnosis,
                DoctorStaffId = dto.DoctorStaffId,
                ExamDate = DateTime.Now
            };

            _context.ExamResults.Add(examResult);
            await _context.SaveChangesAsync();

            dto.ExamResultId = examResult.ExamResultId;
            return Ok(dto);
        }

        // GET: api/Patients/5/exam-results — Admin, MedicalStaff và Customer được xem
        [HttpGet("{id}/exam-results")]
        [Authorize(Roles = "Admin,MedicalStaff,Customer")]
        public async Task<ActionResult<IEnumerable<ExamResultDto>>> GetExamResults(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.HealthContract)
                .FirstOrDefaultAsync(p => p.PatientId == id);
            
            if (patient == null) return NotFound();

            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;

            if (role == "Customer" && !string.IsNullOrEmpty(companyIdClaim))
            {
                int companyId = int.Parse(companyIdClaim);
                if (patient.HealthContract.CompanyId != companyId)
                    return StatusCode(403, "Bạn không có quyền xem kết quả khám của bệnh nhân này.");
            }

            var results = await _context.ExamResults
                .Where(e => e.PatientId == id)
                .Include(e => e.DoctorStaff)
                .Select(e => new ExamResultDto
                {
                    ExamResultId = e.ExamResultId,
                    PatientId = e.PatientId,
                    GroupId = e.GroupId,
                    ExamType = e.ExamType,
                    Result = e.Result,
                    Diagnosis = e.Diagnosis,
                    DoctorStaffId = e.DoctorStaffId,
                    DoctorName = e.DoctorStaff != null ? e.DoctorStaff.FullName : null,
                    ExamDate = e.ExamDate
                })
                .ToListAsync();

            return Ok(results);
        }

        // DELETE: api/Patients/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
                return NotFound();

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Patients/export/{contractId}
        [HttpGet("export/{contractId}")]
        [Authorize(Roles = "Admin,MedicalStaff")]
        public async Task<IActionResult> ExportPatients(int contractId)
        {
            var patients = await _context.Patients
                .Where(p => p.HealthContractId == contractId)
                .ToListAsync();

            var contract = await _context.Contracts
                .Include(c => c.Company)
                .FirstOrDefaultAsync(c => c.HealthContractId == contractId);

            if (contract == null) return NotFound("Contract not found");

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Danh sách Bệnh nhân");
                var currentRow = 1;

                // Header info
                var title = worksheet.Cell(currentRow, 1);
                title.Value = "DANH SÁCH BỆNH NHÂN KHÁM SỨC KHỎE";
                title.Style.Font.Bold = true;
                title.Style.Font.FontSize = 16;
                worksheet.Range(currentRow, 1, currentRow, 6).Merge();
                currentRow++;

                worksheet.Cell(currentRow, 1).Value = $"Công ty: {contract.Company.CompanyName}";
                worksheet.Range(currentRow, 1, currentRow, 6).Merge();
                currentRow++;

                worksheet.Cell(currentRow, 1).Value = $"Hợp đồng số: #HD-{contractId}";
                worksheet.Range(currentRow, 1, currentRow, 6).Merge();
                currentRow += 2;

                // Table Header
                worksheet.Cell(currentRow, 1).Value = "STT";
                worksheet.Cell(currentRow, 2).Value = "Họ và tên";
                worksheet.Cell(currentRow, 3).Value = "Ngày sinh";
                worksheet.Cell(currentRow, 4).Value = "Giới tính";
                worksheet.Cell(currentRow, 5).Value = "CMND/CCCD";
                worksheet.Cell(currentRow, 6).Value = "Phòng ban";

                var headerRange = worksheet.Range(currentRow, 1, currentRow, 6);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightCyan;
                headerRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;

                // Data
                int stt = 1;
                foreach (var p in patients)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = stt++;
                    worksheet.Cell(currentRow, 2).Value = p.FullName;
                    worksheet.Cell(currentRow, 3).Value = p.DateOfBirth.ToString("dd/MM/yyyy");
                    worksheet.Cell(currentRow, 4).Value = p.Gender;
                    worksheet.Cell(currentRow, 5).Value = p.IDCardNumber;
                    worksheet.Cell(currentRow, 6).Value = p.Department;
                    
                    worksheet.Range(currentRow, 1, currentRow, 6).Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new System.IO.MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"DanhSachBenhNhan_{contract.Company.CompanyName.Replace(" ", "_")}.xlsx"
                    );
                }
            }
        }
    }
}
