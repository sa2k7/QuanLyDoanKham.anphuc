using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.DTOs;
using QuanLyDoanKham.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace QuanLyDoanKham.API.Services.MedicalRecords
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public MedicalRecordService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<ServiceResult<List<MedicalRecord>>> BatchIngestAsync(MedicalRecordBatchIngestRequestDto request, string createdBy)
        {
            var group = await _context.MedicalGroups.FindAsync(request.GroupId);
            if (group == null) return ServiceResult<List<MedicalRecord>>.Failure("Không tìm thấy đoàn khám.");

            var recordsToAdd = new List<MedicalRecord>();
            var now = DateTime.Now;

            foreach (var recordDto in request.Records)
            {
                int? existingPatientId = null;
                if (!string.IsNullOrEmpty(recordDto.IDCardNumber))
                {
                    var existingPatient = await _context.Patients
                        .FirstOrDefaultAsync(p => p.IDCardNumber == recordDto.IDCardNumber);
                    existingPatientId = existingPatient?.PatientId;
                }
                
                var newRecord = new MedicalRecord
                {
                    GroupId = request.GroupId,
                    FullName = recordDto.FullName,
                    DateOfBirth = recordDto.DateOfBirth,
                    Gender = recordDto.Gender,
                    IDCardNumber = recordDto.IDCardNumber,
                    Department = recordDto.Department,
                    Status = "READY",
                    CreatedAt = now,
                    PatientId = existingPatientId, // Map to existing patient if found
                    QrToken = "PENDING_TOKEN" 
                };

                recordsToAdd.Add(newRecord);
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.MedicalRecords.AddRange(recordsToAdd);
                    await _context.SaveChangesAsync();

                    // Now we have IDs, generate tokens
                    foreach (var record in recordsToAdd)
                    {
                        record.QrToken = GenerateQrToken(record.MedicalRecordId, record.GroupId);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return ServiceResult<List<MedicalRecord>>.Success(recordsToAdd);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return ServiceResult<List<MedicalRecord>>.Failure("Lỗi khi nhập dữ liệu hàng loạt: " + ex.Message);
                }
            }
        }

        public string GenerateQrToken(int medicalRecordId, int groupId)
        {
            var claims = new List<Claim>
            {
                new Claim("recordId", medicalRecordId.ToString()),
                new Claim("groupId", groupId.ToString()),
                new Claim("type", "MEDICAL_RECORD_CHECKIN")
            };

            var configKey = _configuration.GetSection("AppSettings:Token").Value
                ?? throw new InvalidOperationException("CRITICAL: AppSettings:Token is missing!");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // Token expires at the end of the day to prevent long-term misuse
            var expires = DateTime.Today.AddDays(1).AddSeconds(-1);

            var token = new JwtSecurityToken(
                issuer: _configuration.GetSection("AppSettings:Issuer").Value ?? "QuanLyDoanKham",
                audience: _configuration.GetSection("AppSettings:Audience").Value ?? "QuanLyDoanKham",
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
