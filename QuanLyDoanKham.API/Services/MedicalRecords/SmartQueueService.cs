using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Services.MedicalRecords
{
    public interface ISmartQueueService
    {
        Task<string?> SuggestNextStationAsync(int medicalRecordId);
        Task<List<StationLoadDto>> GetStationLoadsAsync(int groupId);
    }

    public class SmartQueueService : ISmartQueueService
    {
        private readonly ApplicationDbContext _context;

        public SmartQueueService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gợi ý trạm tiếp theo cho bệnh nhân dựa trên:
        /// 1. Danh sách trạm chưa khám (WAITING)
        /// 2. Độ ưu tiên của trạm (SortOrder)
        /// 3. Tải hiện tại của trạm (Số người đang chờ)
        /// </summary>
        public async Task<string?> SuggestNextStationAsync(int medicalRecordId)
        {
            var record = await _context.MedicalRecords
                .Include(m => m.StationTasks)
                .FirstOrDefaultAsync(m => m.MedicalRecordId == medicalRecordId);

            if (record == null) return null;

            // Lấy danh sách các trạm đang ở trạng thái WAITING
            var pendingTasks = record.StationTasks
                .Where(t => t.Status == StationTaskStatus.Waiting)
                .ToList();

            if (!pendingTasks.Any()) return null;

            // Lấy thông tin tải của tất cả các trạm trong đoàn khám này
            var stationLoads = await GetStationLoadsAsync(record.GroupId);

            // Thuật toán gợi ý:
            // Ưu tiên trạm có (Số người chờ * 0.7 + Thứ tự ưu tiên * 0.3) thấp nhất
            var suggestedTask = pendingTasks
                .Select(t => {
                    var load = stationLoads.FirstOrDefault(s => s.StationCode == t.StationCode);
                    var waitingCount = load?.WaitingCount ?? 0;
                    var sortOrder = load?.SortOrder ?? 99;
                    
                    // Trạm "Huyết áp/Cân đo" (thường là trạm đầu) nên có trọng số ưu tiên cao hơn nếu vắng
                    double score = (waitingCount * 0.8) + (sortOrder * 0.2);
                    return new { Task = t, Score = score };
                })
                .OrderBy(x => x.Score)
                .FirstOrDefault();

            return suggestedTask?.Task.StationCode;
        }

        public async Task<List<StationLoadDto>> GetStationLoadsAsync(int groupId)
        {
            return await _context.RecordStationTasks
                .Include(t => t.Station)
                .Where(t => t.MedicalRecord!.GroupId == groupId 
                         && (t.Status == StationTaskStatus.Waiting || t.Status == StationTaskStatus.StationInProgress))
                .GroupBy(t => new { t.StationCode, t.Station!.StationName, t.Station!.SortOrder })
                .Select(g => new StationLoadDto
                {
                    StationCode = g.Key.StationCode,
                    StationName = g.Key.StationName,
                    SortOrder = g.Key.SortOrder,
                    WaitingCount = g.Count(t => t.Status == StationTaskStatus.Waiting),
                    InProgressCount = g.Count(t => t.Status == StationTaskStatus.StationInProgress)
                })
                .OrderBy(s => s.SortOrder)
                .ToListAsync();
        }
    }

    public class StationLoadDto
    {
        public string StationCode { get; set; } = string.Empty;
        public string StationName { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public int WaitingCount { get; set; }
        public int InProgressCount { get; set; }
        public int TotalLoad => WaitingCount + InProgressCount;
    }
}
