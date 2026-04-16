using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Data;
using QuanLyDoanKham.API.Hubs;
using QuanLyDoanKham.API.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyDoanKham.API.Services.MedicalRecords
{
    public class MedicalRecordStateMachine : IMedicalRecordStateMachine
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<QueueHub> _hubContext;

        public MedicalRecordStateMachine(ApplicationDbContext context, IHubContext<QueueHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<ServiceResult<MedicalRecord>> CheckInAsync(int medicalRecordId, string actorUserId)
        {
            var check = await EnsureContractNotSettledAsync(medicalRecordId);
            if (!check.IsSuccess) return ServiceResult<MedicalRecord>.Failure(check.Message);

            var record = await _context.MedicalRecords
                .Include(m => m.StationTasks)
                .FirstOrDefaultAsync(m => m.MedicalRecordId == medicalRecordId);

            if (record == null) return ServiceResult<MedicalRecord>.Failure("Không tìm thấy hồ sơ.");

            // Chấp nhận cả CREATED (mặc định khi tạo) và READY (khi Group khai mạc)
            bool canCheckIn = record.Status == RecordStatus.Created || record.Status == RecordStatus.Ready;
            if (!canCheckIn)
                return ServiceResult<MedicalRecord>.Failure($"Hồ sơ đang ở trạng thái '{record.Status}', không thể check-in.");

            // Áp dụng Lock ở mức Serializable để chống Race Condition khi nhiều Kiosk cùng check-in
            using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            try
            {
                // Generate QueueNo
                await AssignNextQueueNoAsync(record);

                // Lọc trạm và tạo Task
                var activeStations = await InitializeStationTasksAsync(record);

                await _context.SaveChangesAsync();

                // Ghi nhận sự kiện Check-in
                var checkResult = await LogCheckInEventAsync(record);
                if (!checkResult.IsSuccess) return checkResult;

                await transaction.CommitAsync();

                // Broadcast thông tin trạm
                await BroadcastCheckInEventsAsync(record, activeStations);

                return ServiceResult<MedicalRecord>.Success(record);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ServiceResult<MedicalRecord>.Failure("Lỗi Check-in: " + ex.Message);
            }
        }

        public async Task<ServiceResult<RecordStationTask>> StartStationAsync(int medicalRecordId, string stationCode, string actorUserId)
        {
            var check = await EnsureContractNotSettledAsync(medicalRecordId);
            if (!check.IsSuccess) return ServiceResult<RecordStationTask>.Failure(check.Message);

            var task = await _context.RecordStationTasks
                .FirstOrDefaultAsync(t => t.MedicalRecordId == medicalRecordId && t.StationCode == stationCode);

            if (task == null) return ServiceResult<RecordStationTask>.Failure("Không tìm thấy công việc tại trạm này.");
            if (task.Status != StationTaskStatus.Waiting) return ServiceResult<RecordStationTask>.Failure("Chỉ có thể bắt đầu khi ở trạng thái WAITING.");

            task.Status = StationTaskStatus.StationInProgress;
            task.StartedAt = DateTime.Now;

            var startEvent = new StationTaskEvent
            {
                TaskId = task.TaskId,
                EventType = "STATION_START",
                ActorUserId = int.TryParse(actorUserId, out var id) ? id : null,
                CreatedAt = DateTime.Now
            };
            _context.StationTaskEvents.Add(startEvent);

            await _context.SaveChangesAsync();

            // Broadcast that a patient started at this station
            await _hubContext.Clients.Group(stationCode).SendAsync("StationQueueUpdated", stationCode);

            return ServiceResult<RecordStationTask>.Success(task);
        }

        public async Task<ServiceResult<RecordStationTask>> CompleteStationAsync(int medicalRecordId, string stationCode, string actorUserId, string? resultNotes = null)
        {
            var check = await EnsureContractNotSettledAsync(medicalRecordId);
            if (!check.IsSuccess) return ServiceResult<RecordStationTask>.Failure(check.Message);

            var task = await _context.RecordStationTasks
                .Include(t => t.MedicalRecord)
                .FirstOrDefaultAsync(t => t.MedicalRecordId == medicalRecordId && t.StationCode == stationCode);

            if (task == null) return ServiceResult<RecordStationTask>.Failure("Không tìm thấy công việc.");
            if (task.Status != StationTaskStatus.StationInProgress) return ServiceResult<RecordStationTask>.Failure("Công việc chưa được bắt đầu.");

            task.Status = StationTaskStatus.StationDone;
            task.CompletedAt = DateTime.Now;
            task.Notes = resultNotes;

            var doneEvent = new StationTaskEvent
            {
                TaskId = task.TaskId,
                EventType = "STATION_COMPLETE",
                Payload = resultNotes,
                ActorUserId = int.TryParse(actorUserId, out var id) ? id : null,
                CreatedAt = DateTime.Now
            };
            _context.StationTaskEvents.Add(doneEvent);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Lưu sự kiện và trạng thái "Hoàn Tất" của trạm này xuống Database trước
                await _context.SaveChangesAsync();

                // Lúc này AnyAsync() sẽ phản ánh chính xác kết quả thực tế từ File Database
                var allDone = !await _context.RecordStationTasks
                    .AnyAsync(t => t.MedicalRecordId == medicalRecordId && t.Status != StationTaskStatus.StationDone);

                if (allDone)
                {
                    task.MedicalRecord.Status = RecordStatus.QcPending;
                    // Lưu lại trạng thái của MedicalRecord
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ServiceResult<RecordStationTask>.Failure("Lỗi lưu DB tại trạm: " + ex.Message);
            }

            // PHASE 2: Broadcast with rich payload so station views update without extra API call
            var statusPayload = new PatientStatusChangedPayload(
                medicalRecordId,
                task.Status,
                stationCode,
                task.MedicalRecord?.QueueNo ?? 0
            );

            await _hubContext.Clients.Group(stationCode).SendAsync("StationQueueUpdated", stationCode);
            await _hubContext.Clients.Group(stationCode).SendAsync("PatientStatusChanged", statusPayload);

            if (task.MedicalRecord?.Status == RecordStatus.QcPending)
            {
                await _hubContext.Clients.All.SendAsync("QueueUpdated", "QC");
            }

            return ServiceResult<RecordStationTask>.Success(task);
        }

        public async Task<ServiceResult<MedicalRecord>> FinalizeRecordAsync(int medicalRecordId, string actorUserId)
        {
            var check = await EnsureContractNotSettledAsync(medicalRecordId);
            if (!check.IsSuccess) return ServiceResult<MedicalRecord>.Failure(check.Message);

            var record = await _context.MedicalRecords
                .FirstOrDefaultAsync(m => m.MedicalRecordId == medicalRecordId);

            if (record == null) return ServiceResult<MedicalRecord>.Failure("Không tìm thấy hồ sơ.");
            
            if (record.Status != RecordStatus.QcPending)
            {
                return ServiceResult<MedicalRecord>.Failure($"Không thể hoàn tất hồ sơ. Trạng thái hiện tại: '{record.Status}'. Yêu cầu: QC_PENDING.");
            }

            record.Status = RecordStatus.Completed;
            record.UpdatedAt = DateTime.Now;

            // Log final event
            var finalEvent = new StationTaskEvent
            {
                EventType = "FINAL_COMPLETE",
                ActorUserId = int.TryParse(actorUserId, out var id) ? id : null,
                CreatedAt = DateTime.Now
            };
            // Try to link to a task for DB integrity if needed
            var lastTask = await _context.RecordStationTasks
                .OrderByDescending(t => t.CompletedAt)
                .FirstOrDefaultAsync(t => t.MedicalRecordId == medicalRecordId);
            
            if (lastTask == null)
            {
                return ServiceResult<MedicalRecord>.Failure("Không thể hoàn tất hồ sơ: Không có lịch sử khám bệnh.");
            }
            
            finalEvent.TaskId = lastTask.TaskId;
            _context.StationTaskEvents.Add(finalEvent);

            await _context.SaveChangesAsync();

            // Broadcast to update QC Dashboard
            await _hubContext.Clients.All.SendAsync("QueueUpdated", "QC");
            await _hubContext.Clients.All.SendAsync("QueueUpdated", "ALL");

            return ServiceResult<MedicalRecord>.Success(record);
        }

        public async Task<ServiceResult<RecordStationTask>> AddExtraStationAsync(int medicalRecordId, string stationCode, string actorUserId, string? notes = null)
        {
            var check = await EnsureContractNotSettledAsync(medicalRecordId);
            if (!check.IsSuccess) return ServiceResult<RecordStationTask>.Failure(check.Message);

            var record = await _context.MedicalRecords
                .Include(r => r.StationTasks)
                .FirstOrDefaultAsync(r => r.MedicalRecordId == medicalRecordId);

            if (record == null) return ServiceResult<RecordStationTask>.Failure("Không tìm thấy hồ sơ.");

            // Kiểm tra xem trạm này đã có chưa
            var existingTask = record.StationTasks.FirstOrDefault(t => t.StationCode == stationCode);
            if (existingTask != null)
            {
                if (existingTask.Status != StationTaskStatus.Waiting)
                {
                    return ServiceResult<RecordStationTask>.Failure($"Trạm này đã tồn tại và đang ở trạng thái {existingTask.Status}.");
                }
                // Nếu đang chờ thì có thể update note
                existingTask.Notes = notes;
                await _context.SaveChangesAsync();
                return ServiceResult<RecordStationTask>.Success(existingTask);
            }

            // Tạo Task mới
            var newTask = new RecordStationTask
            {
                MedicalRecordId = medicalRecordId,
                StationCode = stationCode,
                Status = StationTaskStatus.Waiting,
                WaitingSince = DateTime.Now,
                Notes = notes
            };

            record.StationTasks.Add(newTask);

            // Nếu hồ sơ đang chờ duyệt QC, thì trả về trạng thái Khám bệnh
            if (record.Status == RecordStatus.QcPending || record.Status == RecordStatus.Completed)
            {
                record.Status = RecordStatus.InProgress;
            }
            
            // Xóa lỗi Check-in nếu trạm này là trạm mới nhưng hồ sơ lại bị kẹt ở Created/Ready
            if (record.Status == RecordStatus.Created || record.Status == RecordStatus.Ready) {
                return ServiceResult<RecordStationTask>.Failure("Bệnh nhân chưa Check-in nên không thể chỉ định thêm trạm. Hãy Check-in trước.");
            }

            // Save changes
            await _context.SaveChangesAsync();

            // Broadcast real-time message to the target station so the doctor knows a new patient was assigned
            var assignEvent = new StationTaskEvent
            {
                TaskId = newTask.TaskId,
                EventType = "STATION_ASSIGNED_EXTRA",
                ActorUserId = int.TryParse(actorUserId, out var id) ? id : null,
                CreatedAt = DateTime.Now,
                Payload = notes
            };
            _context.StationTaskEvents.Add(assignEvent);
            await _context.SaveChangesAsync();

            // Hub alerts
            await _hubContext.Clients.Group(stationCode).SendAsync("QueueUpdated", stationCode);
            // Alert QC if it was demoted from QC_PENDING
            await _hubContext.Clients.All.SendAsync("QueueUpdated", "QC");

            return ServiceResult<RecordStationTask>.Success(newTask);
        }

        public async Task<ServiceResult<MedicalRecord>> MarkNoShowAsync(int medicalRecordId, string actorUserId)
        {
            var check = await EnsureContractNotSettledAsync(medicalRecordId);
            if (!check.IsSuccess) return ServiceResult<MedicalRecord>.Failure(check.Message);

            var record = await _context.MedicalRecords.FindAsync(medicalRecordId);
            if (record == null) return ServiceResult<MedicalRecord>.Failure("Không tìm thấy hồ sơ.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                record.Status = RecordStatus.NoShow;
                record.UpdatedAt = DateTime.Now;

                // Đồng bộ cập nhật các trạm Đang chờ -> Bỏ qua
                var pendingTasks = await _context.RecordStationTasks
                    .Where(t => t.MedicalRecordId == medicalRecordId && t.Status == StationTaskStatus.Waiting)
                    .ToListAsync();

                foreach (var task in pendingTasks)
                {
                    task.Status = StationTaskStatus.Skipped;
                    task.Notes = "Bệnh nhân vắng mặt (No-Show)";
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ServiceResult<MedicalRecord>.Failure("Lỗi lưu DB khi đánh dấu vắng mặt: " + ex.Message);
            }
            
            // Broadcast to update all queues
            await _hubContext.Clients.All.SendAsync("QueueUpdated", "ALL");

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

            // Broadcast to update QC Dashboard
            await _hubContext.Clients.All.SendAsync("QueueUpdated", "QC");
            await _hubContext.Clients.All.SendAsync("QueueUpdated", "ALL");

            return ServiceResult<MedicalRecord>.Success(record);
        }

        public async Task<ServiceResult<MedicalRecord>> QCReworkAsync(int medicalRecordId, string actorUserId, string reason)
        {
            var check = await EnsureContractNotSettledAsync(medicalRecordId);
            if (!check.IsSuccess) return ServiceResult<MedicalRecord>.Failure(check.Message);

            var record = await _context.MedicalRecords.FindAsync(medicalRecordId);
            if (record == null) return ServiceResult<MedicalRecord>.Failure("Không tìm thấy hồ sơ.");

            record.Status = RecordStatus.QcRework;
            record.UpdatedAt = DateTime.Now;
            // Todo logic to save reason...

            await _context.SaveChangesAsync();

            // Broadcast rework status
            await _hubContext.Clients.All.SendAsync("QueueUpdated", "QC");
            await _hubContext.Clients.All.SendAsync("QueueUpdated", "ALL");

            return ServiceResult<MedicalRecord>.Success(record);
        }

        public async Task<ServiceResult<MedicalRecord>> CancelRecordAsync(int medicalRecordId, string actorUserId, string reason)
        {
            var check = await EnsureContractNotSettledAsync(medicalRecordId);
            if (!check.IsSuccess) return ServiceResult<MedicalRecord>.Failure(check.Message);

            var record = await _context.MedicalRecords.FindAsync(medicalRecordId);
            if (record == null) return ServiceResult<MedicalRecord>.Failure("Không tìm thấy hồ sơ.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                record.Status = RecordStatus.Cancelled;
                record.UpdatedAt = DateTime.Now;

                // Đồng bộ cập nhật các trạm Đang chờ -> Bỏ qua
                var pendingTasks = await _context.RecordStationTasks
                    .Where(t => t.MedicalRecordId == medicalRecordId && t.Status == StationTaskStatus.Waiting)
                    .ToListAsync();

                foreach (var task in pendingTasks)
                {
                    task.Status = StationTaskStatus.Skipped;
                    task.Notes = $"Bị hủy: {reason}";
                }

                // Ghi nhận sự kiện hủy vào audit log
                _context.StationTaskEvents.Add(new StationTaskEvent
                {
                    EventType = "RECORD_CANCELLED",
                    Payload = reason,
                    ActorUserId = int.TryParse(actorUserId, out var id) ? id : null,
                    CreatedAt = DateTime.Now
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ServiceResult<MedicalRecord>.Failure("Lỗi lưu DB khi hủy hồ sơ: " + ex.Message);
            }

            // Broadcast cancellation
            await _hubContext.Clients.All.SendAsync("QueueUpdated", "ALL");

            return ServiceResult<MedicalRecord>.Success(record);
        }

        // ─── Private helpers ──────────────────────────────────────────────────

        private async Task AssignNextQueueNoAsync(MedicalRecord record)
        {
            var lastQueueNo = await _context.MedicalRecords
                .Where(m => m.GroupId == record.GroupId && m.CheckInAt.HasValue && m.CheckInAt.Value.Date == DateTime.Today)
                .MaxAsync(m => (int?)m.QueueNo) ?? 0;

            record.QueueNo = lastQueueNo + 1;
            record.Status = RecordStatus.CheckedIn;
            record.CheckInAt = DateTime.Now;
        }

        private async Task<List<Station>> InitializeStationTasksAsync(MedicalRecord record)
        {
            var activeStations = await _context.MedicalGroupPositions
                .Where(mgp => mgp.GroupId == record.GroupId)
                .Select(mgp => mgp.Position.Station)
                .Where(s => s.IsActive && IsStationApplicable(s, record.Gender))
                .OrderBy(s => s.SortOrder)
                .ToListAsync();

            foreach (var station in activeStations)
            {
                _context.RecordStationTasks.Add(new RecordStationTask
                {
                    MedicalRecordId = record.MedicalRecordId,
                    StationCode     = station.StationCode,
                    Status          = StationTaskStatus.Waiting,
                    WaitingSince    = DateTime.Now
                });
            }

            return activeStations;
        }

        private async Task<ServiceResult<MedicalRecord>> LogCheckInEventAsync(MedicalRecord record)
        {
            var checkInEvent = new StationTaskEvent
            {
                EventType = "CHECK_IN",
                Payload = $"BN Check-in. Số thứ tự: {record.QueueNo}",
                // ActorUserId lookup note: This helper assumes actor processing is handled or passed.
                // For simplicity in this extraction, we use null or pass it if required.
                CreatedAt = DateTime.Now
            };

            var firstTask = await _context.RecordStationTasks
                .FirstOrDefaultAsync(t => t.MedicalRecordId == record.MedicalRecordId);

            if (firstTask == null)
            {
                return ServiceResult<MedicalRecord>.Failure("Không thể check-in: Chưa có cấu hình phòng/bàn khám nào đang hoạt động.");
            }

            checkInEvent.TaskId = firstTask.TaskId;
            _context.StationTaskEvents.Add(checkInEvent);
            await _context.SaveChangesAsync();

            return ServiceResult<MedicalRecord>.Success(record);
        }

        private async Task BroadcastCheckInEventsAsync(MedicalRecord record, List<Station> activeStations)
        {
            var firstStation = activeStations.FirstOrDefault();
            var arrivedPayload = new NewPatientArrivedPayload(
                record.MedicalRecordId,
                record.FullName ?? string.Empty,
                record.QueueNo ?? 0,
                record.Gender,
                firstStation?.StationCode ?? string.Empty,
                record.CheckInAt ?? DateTime.Now
            );

            await _hubContext.Clients.All.SendAsync("NewPatientArrived", arrivedPayload);
            await _hubContext.Clients.All.SendAsync("QueueUpdated", "ALL");

            if (firstStation != null)
            {
                await _hubContext.Clients.Group(firstStation.StationCode)
                    .SendAsync("StationQueueUpdated", firstStation.StationCode);
            }
        }

        private async Task<ServiceResult<bool>> EnsureContractNotSettledAsync(int medicalRecordId)
        {
            var contractStatus = await _context.MedicalRecords
                .Where(m => m.MedicalRecordId == medicalRecordId)
                .Select(m => m.MedicalGroup != null && m.MedicalGroup.HealthContract != null 
                             ? m.MedicalGroup.HealthContract.Status 
                             : null)
                .FirstOrDefaultAsync();

            if (contractStatus == "Settled" || contractStatus == "Finalized")
            {
                return ServiceResult<bool>.Failure("Hợp đồng đã quyết toán, không thể thay đổi dữ liệu y khoa.");
            }

            return ServiceResult<bool>.Success(true);
        }

        /// <summary>
        /// Kiểm tra trạm có áp dụng cho bệnh nhân không dựa trên giới tính.
        /// - RequiredGender = null  → áp dụng cho tất cả
        /// - RequiredGender = "Nữ" → chỉ bệnh nhân nữ
        /// - RequiredGender = "Nam" → chỉ bệnh nhân nam
        /// </summary>
        private static bool IsStationApplicable(Station station, string? patientGender)
        {
            if (string.IsNullOrEmpty(station.RequiredGender)) return true;
            if (string.IsNullOrEmpty(patientGender)) return true; // Không rõ giới tính → không bỏ qua

            return string.Equals(station.RequiredGender, patientGender, StringComparison.OrdinalIgnoreCase);
        }
    }
}
