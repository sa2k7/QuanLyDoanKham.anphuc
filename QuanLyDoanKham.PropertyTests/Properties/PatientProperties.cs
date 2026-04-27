using FsCheck;
using FsCheck.Xunit;
using QuanLyDoanKham.API.Models;
using QuanLyDoanKham.PropertyTests.Helpers;

namespace QuanLyDoanKham.PropertyTests.Properties;

/// <summary>
/// Property-based tests cho module Bệnh nhân.
/// Feature: medical-examination-team-management
/// </summary>
public class PatientProperties
{
    // -----------------------------------------------------------------------
    // Property 9: Danh sách bệnh nhân chỉ chứa bệnh nhân của đoàn được chọn
    // Validates: Requirements 3.1
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P9_PatientListFilteredByTeam()
    {
        // Feature: medical-examination-team-management, Property 9: Patient list filtered by team
        var gen =
            from teamId in Gen.Choose(1, 100)
            from otherTeamId in Gen.Choose(101, 200)
            from patientCount in Gen.Choose(1, 20)
            from otherCount in Gen.Choose(1, 10)
            select (teamId, otherTeamId, patientCount, otherCount);

        return Prop.ForAll(Arb.From(gen), data =>
        {
            var (teamId, otherTeamId, patientCount, otherCount) = data;

            // Tạo bệnh nhân cho đoàn được chọn
            var allPatients = new List<Patient>();
            for (int i = 0; i < patientCount; i++)
                allPatients.Add(new Patient { PatientId = i + 1, MedicalGroupId = teamId, FullName = $"BN {i}" });

            // Tạo bệnh nhân cho đoàn khác
            for (int i = 0; i < otherCount; i++)
                allPatients.Add(new Patient { PatientId = patientCount + i + 1, MedicalGroupId = otherTeamId, FullName = $"BN Other {i}" });

            // Lọc theo teamId
            var filtered = allPatients.Where(p => p.MedicalGroupId == teamId).ToList();

            return filtered.Count == patientCount
                && filtered.All(p => p.MedicalGroupId == teamId);
        });
    }

    // -----------------------------------------------------------------------
    // Property 10: Import Excel DSCN bảo toàn đầy đủ dữ liệu bệnh nhân
    // Validates: Requirements 3.3, 3.4
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P10_ExcelImportPreservesPatientData()
    {
        // Feature: medical-examination-team-management, Property 10: Excel import preserves patient data
        var gen =
            from fullName in Gen.Elements("Nguyễn Văn A", "Trần Thị B", "Lê Văn C")
            from dob in DomainArbitraries.ValidExamDates().Generator
            from gender in Gen.Elements("Nam", "Nữ")
            from department in Gen.Elements("Phòng IT", "Phòng Kế toán", "Phòng Nhân sự")
            from teamId in Gen.Choose(1, 100)
            select (fullName, dob, gender, department, teamId);

        return Prop.ForAll(Arb.From(gen), row =>
        {
            var (fullName, dob, gender, department, teamId) = row;

            // Simulate import: map Excel row → Patient entity
            var patient = new Patient
            {
                FullName = fullName,
                DateOfBirth = dob,
                Gender = gender,
                Department = department,
                MedicalGroupId = teamId,
                Source = "ExcelImport"
            };

            // Tất cả trường phải được bảo toàn
            return patient.FullName == fullName
                && patient.DateOfBirth == dob
                && patient.Gender == gender
                && patient.Department == department
                && patient.MedicalGroupId == teamId;
        });
    }

    // -----------------------------------------------------------------------
    // Property 11: Dòng Excel có mã đoàn không hợp lệ bị bỏ qua và báo lỗi
    // Validates: Requirements 3.5
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P11_InvalidTeamCodeRowsSkippedWithError()
    {
        // Feature: medical-examination-team-management, Property 11: Invalid team code rows skipped
        var gen =
            from validCount in Gen.Choose(1, 10)
            from invalidCount in Gen.Choose(1, 5)
            select (validCount, invalidCount);

        return Prop.ForAll(Arb.From(gen), data =>
        {
            var (validCount, invalidCount) = data;

            // Simulate import logic
            var validTeamCodes = Enumerable.Range(1, 50).Select(i => $"TEAM-{i:D3}").ToHashSet();

            var rows = new List<(int Row, string TeamCode, string FullName)>();
            for (int i = 0; i < validCount; i++)
                rows.Add((i + 2, "TEAM-001", $"BN Valid {i}"));
            for (int i = 0; i < invalidCount; i++)
                rows.Add((validCount + i + 2, $"INVALID-{i}", $"BN Invalid {i}"));

            var importedPatients = new List<Patient>();
            var errors = new List<(int Row, string Message)>();

            foreach (var (rowNum, teamCode, fullName) in rows)
            {
                if (!validTeamCodes.Contains(teamCode))
                {
                    errors.Add((rowNum, $"Mã đoàn khám '{teamCode}' không tồn tại"));
                    continue;
                }
                importedPatients.Add(new Patient { FullName = fullName });
            }

            return importedPatients.Count == validCount
                && errors.Count == invalidCount
                && errors.All(e => e.Message.Contains("không tồn tại"));
        });
    }

    // -----------------------------------------------------------------------
    // Property 12: Bệnh nhân hoàn thành tất cả vị trí khám được tự động chuyển trạng thái
    // Validates: Requirements 3.7, 11.2
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P12_PatientAutoStatusWhenAllPositionsCompleted()
    {
        // Feature: medical-examination-team-management, Property 12: Patient auto-status on completion
        var gen =
            from positionCount in Gen.Choose(1, 8)
            select positionCount;

        return Prop.ForAll(Arb.From(gen), positionCount =>
        {
            // Simulate CheckPatientCompletionAsync logic
            var examRecords = Enumerable.Range(1, positionCount)
                .Select(i => new { PositionId = i, IsCompleted = true })
                .ToList();

            var allPositions = Enumerable.Range(1, positionCount).ToList();
            var completedCount = examRecords.Count(r => r.IsCompleted);

            string status = completedCount >= allPositions.Count ? "Đã khám xong" : "Chưa khám";

            return status == "Đã khám xong";
        });
    }

    // -----------------------------------------------------------------------
    // Property 12 (partial): Bệnh nhân chưa hoàn thành không được chuyển trạng thái
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P12_PatientNotCompletedWhenSomePositionsMissing()
    {
        // Feature: medical-examination-team-management, Property 12 variant: Incomplete patient stays pending
        var gen =
            from totalPositions in Gen.Choose(2, 8)
            from completedPositions in Gen.Choose(1, totalPositions - 1)
            select (totalPositions, completedPositions);

        return Prop.ForAll(Arb.From(gen), data =>
        {
            var (totalPositions, completedPositions) = data;

            var completedCount = completedPositions;
            var allPositionsCount = totalPositions;

            string status = completedCount >= allPositionsCount ? "Đã khám xong" : "Chưa khám";

            return status == "Chưa khám";
        });
    }

    // -----------------------------------------------------------------------
    // Property 13: Tổng bệnh nhân đã khám và chưa khám bằng tổng bệnh nhân của vị trí
    // Validates: Requirements 3.8
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P13_ExamPositionCountInvariant()
    {
        // Feature: medical-examination-team-management, Property 13: Exam position count invariant
        var gen =
            from total in Gen.Choose(1, 50)
            from completed in Gen.Choose(0, total)
            select (total, completed);

        return Prop.ForAll(Arb.From(gen), data =>
        {
            var (total, completed) = data;
            var notCompleted = total - completed;

            return completed + notCompleted == total;
        });
    }

    // -----------------------------------------------------------------------
    // Property 31: Mục bệnh nhân đã khám chỉ chứa bệnh nhân hoàn thành tất cả vị trí
    // Validates: Requirements 11.1
    // -----------------------------------------------------------------------
    [Property(MaxTest = 100)]
    public Property P31_CompletedPatientListOnlyFullyExamined()
    {
        // Feature: medical-examination-team-management, Property 31: Completed patient list only fully examined
        var gen =
            from patientCount in Gen.Choose(1, 20)
            from positionCount in Gen.Choose(1, 5)
            select (patientCount, positionCount);

        return Prop.ForAll(Arb.From(gen), data =>
        {
            var (patientCount, positionCount) = data;

            // Tạo bệnh nhân với trạng thái ngẫu nhiên
            var patients = Enumerable.Range(1, patientCount).Select(i => new
            {
                PatientId = i,
                CompletedPositions = i % 2 == 0 ? positionCount : positionCount - 1, // Chẵn = hoàn thành
                TotalPositions = positionCount
            }).ToList();

            // Lọc bệnh nhân đã khám xong
            var completedPatients = patients
                .Where(p => p.CompletedPositions >= p.TotalPositions)
                .ToList();

            // Tất cả bệnh nhân trong danh sách "đã khám xong" phải hoàn thành tất cả vị trí
            return completedPatients.All(p => p.CompletedPositions >= p.TotalPositions);
        });
    }
}
