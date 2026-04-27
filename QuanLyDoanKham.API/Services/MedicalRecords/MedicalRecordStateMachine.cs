using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Models.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyDoanKham.API.Services.MedicalRecords
{
    public class MedicalRecordStateMachine : IMedicalRecordStateMachine
    {
        private readonly ApplicationDbContext _context;

        public MedicalRecordStateMachine(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult<MedicalRecord>> CheckInAsync(int medicalRecordId, string actorUserId)
        {
            var check = await EnsureContractNotSettledAsync(medicalRecordId);
            if (!check.IsSuccess) return ServiceResult<MedicalRecord>.Failure(check.Message);

            var record = await _context.MedicalRecords
                .FirstOrDefaultAsync(m => m.MedicalRecordId == medicalRecordId);

            if (record == null) return ServiceResult<MedicalRecord>.Failure("Không tìm thấy hồ sơ.");

            if (record.Status != RecordStatus.Created && record.Status != RecordStatus.Ready)
                return ServiceResult<MedicalRecord>.Failure($"Hồ sơ đang ở trạng thái '{record.Status}', không thể check-in.");

            record.Status = RecordStatus.InProgress;
            record.CheckInAt = DateTime.Now;
            record.UpdatedAt = DateTime.Now;
            
            if (int.TryParse(actorUserId, out var userId))
            {
                record.CheckInByUserId = userId;
            }

            await _context.SaveChangesAsync();

            return ServiceResult<MedicalRecord>.Success(record);
        }

        public async Task<ServiceResult<MedicalRecord>> FinalizeRecordAsync(int medicalRecordId, string actorUserId)
        {
            var check = await EnsureContractNotSettledAsync(medicalRecordId);
            if (!check.IsSuccess) return ServiceResult<MedicalRecord>.Failure(check.Message);

            var record = await _context.MedicalRecords.FindAsync(medicalRecordId);
            if (record == null) return ServiceResult<MedicalRecord>.Failure("Không tìm thấy hồ sơ.");
            
            record.Status = RecordStatus.Completed;
            record.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return ServiceResult<MedicalRecord>.Success(record);
        }

        public async Task<ServiceResult<MedicalRecord>> MarkNoShowAsync(int medicalRecordId, string actorUserId)
        {
            var check = await EnsureContractNotSettledAsync(medicalRecordId);
            if (!check.IsSuccess) return ServiceResult<MedicalRecord>.Failure(check.Message);

            var record = await _context.MedicalRecords.FindAsync(medicalRecordId);
            if (record == null) return ServiceResult<MedicalRecord>.Failure("Không tìm thấy hồ sơ.");

            record.Status = RecordStatus.NoShow;
            record.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return ServiceResult<MedicalRecord>.Success(record);
        }

        public async Task<ServiceResult<MedicalRecord>> QCPassAsync(int medicalRecordId, string actorUserId)
        {
            var check = await EnsureContractNotSettledAsync(medicalRecordId);
            if (!check.IsSuccess) return ServiceResult<MedicalRecord>.Failure(check.Message);

            var record = await _context.MedicalRecords.FindAsync(medicalRecordId);
            if (record == null) return ServiceResult<MedicalRecord>.Failure("Không tìm thấy hồ sơ.");

            record.Status = RecordStatus.Completed;
            record.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return ServiceResult<MedicalRecord>.Success(record);
        }

        public async Task<ServiceResult<MedicalRecord>> QCReworkAsync(int medicalRecordId, string actorUserId, string reason)
        {
            var check = await EnsureContractNotSettledAsync(medicalRecordId);
            if (!check.IsSuccess) return ServiceResult<MedicalRecord>.Failure(check.Message);

            var record = await _context.MedicalRecords.FindAsync(medicalRecordId);
            if (record == null) return ServiceResult<MedicalRecord>.Failure("Không tìm thấy hồ sơ.");

            record.Status = RecordStatus.InProgress; // Chuyển về InProgress để khám lại
            record.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return ServiceResult<MedicalRecord>.Success(record);
        }

        public async Task<ServiceResult<MedicalRecord>> CancelRecordAsync(int medicalRecordId, string actorUserId, string reason)
        {
            var check = await EnsureContractNotSettledAsync(medicalRecordId);
            if (!check.IsSuccess) return ServiceResult<MedicalRecord>.Failure(check.Message);

            var record = await _context.MedicalRecords.FindAsync(medicalRecordId);
            if (record == null) return ServiceResult<MedicalRecord>.Failure("Không tìm thấy hồ sơ.");

            record.Status = RecordStatus.Cancelled;
            record.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return ServiceResult<MedicalRecord>.Success(record);
        }

        private async Task<ServiceResult<bool>> EnsureContractNotSettledAsync(int medicalRecordId)
        {
            var contractStatus = await _context.MedicalRecords
                .Where(m => m.MedicalRecordId == medicalRecordId)
                .Select(m => m.MedicalGroup != null && m.MedicalGroup.HealthContract != null 
                             ? (ContractStatus?)m.MedicalGroup.HealthContract.Status 
                             : null)
                .FirstOrDefaultAsync();

            if (contractStatus == ContractStatus.Finished || contractStatus == ContractStatus.Locked)
            {
                return ServiceResult<bool>.Failure("Hợp đồng đã quyết toán, không thể thay đổi dữ liệu y khoa.");
            }

            return ServiceResult<bool>.Success(true);
        }
    }
}
