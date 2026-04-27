using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.API.Models.Enums;

namespace QuanLyDoanKham.API.Services.Contracts
{
    /// <summary>Service tự động tạo đoàn khám từ hợp đồng đã duyệt</summary>
    public interface IAutoCreateMedicalGroupService
    {
        Task<List<MedicalGroup>> CreateMedicalGroupsFromContractAsync(int contractId, int? createdByUserId);
        Task<bool> CanCreateMedicalGroups(int contractId);
    }

    public class AutoCreateMedicalGroupService : IAutoCreateMedicalGroupService
    {
        private readonly ApplicationDbContext _context;

        public AutoCreateMedicalGroupService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CanCreateMedicalGroups(int contractId)
        {
            var contract = await _context.Contracts.FindAsync(contractId);
            if (contract == null) return false;

            // Chỉ tạo đoàn khám khi hợp đồng đã được duyệt
            return contract.Status == ContractStatus.Approved || contract.Status == ContractStatus.Active;
        }

        public async Task<List<MedicalGroup>> CreateMedicalGroupsFromContractAsync(int contractId, int? createdByUserId)
        {
            var contract = await _context.Contracts
                .Include(c => c.Company)
                .FirstOrDefaultAsync(c => c.HealthContractId == contractId);

            if (contract == null)
                throw new InvalidOperationException("Không tìm thấy hợp đồng");

            if (contract.Status != ContractStatus.Approved && contract.Status != ContractStatus.Active)
                throw new InvalidOperationException("Hợp đồng chưa được duyệt. Không thể tạo đoàn khám.");

            var createdGroups = new List<MedicalGroup>();
            var currentDate = contract.StartDate.Date;
            var endDate = contract.EndDate.Date;
            var dayCount = 1;

            // Tạo đoàn khám cho mỗi ngày trong khoảng thời gian hợp đồng
            while (currentDate <= endDate)
            {
                // Kiểm tra xem đã có đoàn khám nào cho ngày này chưa
                var existingGroup = await _context.MedicalGroups
                    .FirstOrDefaultAsync(mg =>
                        mg.HealthContractId == contractId &&
                        mg.ExamDate.Date == currentDate);

                if (existingGroup == null)
                {
                    var groupName = $"{contract.Company?.CompanyName ?? "Đoàn khám"} - Ngày {currentDate:dd/MM/yyyy}";
                    if (contract.StartDate.Date != contract.EndDate.Date)
                    {
                        groupName = $"{contract.Company?.CompanyName ?? "Đoàn khám"} - Ngày {dayCount} ({currentDate:dd/MM/yyyy})";
                    }

                    var medicalGroup = new MedicalGroup
                    {
                        GroupName = groupName,
                        ExamDate = currentDate,
                        HealthContractId = contractId,
                        Status = "Draft",
                        Slot = "FullDay",
                        TeamCode = $"DAY_{dayCount:D2}",
                        ExamContent = "TỔNG QUÁT",
                        CreatedAt = DateTime.Now,
                        CreatedBy = createdByUserId?.ToString() ?? "system"
                    };

                    _context.MedicalGroups.Add(medicalGroup);
                    createdGroups.Add(medicalGroup);
                }

                currentDate = currentDate.AddDays(1);
                dayCount++;
            }

            if (createdGroups.Count > 0)
            {
                await _context.SaveChangesAsync();

                // Tạo lịch cho từng đoàn khám (ScheduleCalendar)
                foreach (var group in createdGroups)
                {
                    var schedule = new ScheduleCalendar
                    {
                        GroupId = group.GroupId,
                        ExamDate = group.ExamDate,
                        Note = $"Tự động tạo từ hợp đồng {contract.ContractCode}"
                    };
                    _context.ScheduleCalendars.Add(schedule);
                }

                // Cập nhật trạng thái hợp đồng thành Active nếu đang là Approved
                if (contract.Status == ContractStatus.Approved)
                {
                    contract.Status = ContractStatus.Active;
                    _context.ContractStatusHistories.Add(new ContractStatusHistory
                    {
                        HealthContractId = contractId,
                        OldStatus = ContractStatus.Approved.ToString(),
                        NewStatus = ContractStatus.Active.ToString(),
                        ChangedBy = createdByUserId?.ToString() ?? "system",
                        ChangedAt = DateTime.Now,
                        Note = $"Tự động tạo {createdGroups.Count} đoàn khám từ hợp đồng"
                    });
                }

                await _context.SaveChangesAsync();
            }

            return createdGroups;
        }
    }
}
