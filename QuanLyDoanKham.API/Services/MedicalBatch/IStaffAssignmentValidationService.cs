using System;
using System.Threading.Tasks;

namespace QuanLyDoanKham.API.Services.MedicalBatch
{
    /// <summary>
    /// Validates staff assignment business rules before a GroupStaffDetail is saved.
    /// Enforced at the service layer — not just the controller layer.
    ///
    /// Rules enforced:
    ///   1. staffRole must be one of: TiepNhan, KhamNoi, KhamNgoai, LayMau, KhamSan, SieuAm
    ///   2. A staff member may only be assigned to ONE campaign per examDate
    ///      (re-assigning to the SAME campaign on the same date is idempotent — no error)
    /// </summary>
    public interface IStaffAssignmentValidationService
    {
        /// <summary>
        /// Validate whether a staff member can be assigned to a campaign on a given date.
        ///
        /// Returns (true, null) when the assignment is valid.
        /// Returns (false, errorMessage) when a rule is violated:
        ///   - Invalid role → "Vai trò nhân sự không phù hợp với vị trí trong chiến dịch"
        ///   - Double-booking → "Nhân sự {name} đã được phân công cho chiến dịch {campaignName} vào ngày này"
        /// </summary>
        /// <param name="staffId">ID of the staff member being assigned.</param>
        /// <param name="campaignId">GroupId of the target campaign (MedicalGroup.GroupId).</param>
        /// <param name="examDate">Date of the campaign session.</param>
        /// <param name="staffRole">Role string to validate (must match a StaffRole enum value).</param>
        Task<(bool Valid, string? ErrorMessage)> ValidateAssignmentAsync(
            int staffId,
            int campaignId,
            DateTime examDate,
            string staffRole);
    }
}
