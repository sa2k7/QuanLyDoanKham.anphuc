using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // AUTH & PERMISSIONS
        public DbSet<AppUser> Users { get; set; }
        public DbSet<AppRole> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }


        // COMPANIES & CONTRACTS
        public DbSet<Company> Companies { get; set; }
        public DbSet<HealthContract> Contracts { get; set; }
        public DbSet<ContractApprovalStep> ContractApprovalSteps { get; set; }
        public DbSet<ContractApprovalHistory> ContractApprovalHistories { get; set; }
        public DbSet<ApprovalStep> ApprovalSteps { get; set; }
        public DbSet<ApprovalHistory> ApprovalHistories { get; set; }
        public DbSet<ContractAttachment> ContractAttachments { get; set; }
        public DbSet<ContractStatusHistory> ContractStatusHistories { get; set; }

        // MEDICAL GROUPS
        public DbSet<MedicalGroup> MedicalGroups { get; set; }
        public DbSet<MedicalGroupPosition> MedicalGroupPositions { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<GroupPositionQuota> GroupPositionQuotas { get; set; }
        public DbSet<GroupStaffDetail> GroupStaffDetails { get; set; }

        // STAFF
        public DbSet<Staff> Staffs { get; set; }

        // DEPARTMENTS & WORK RULES (Obsolete)

        // SCHEDULE
        public DbSet<ScheduleCalendar> ScheduleCalendars { get; set; }


        // EXAMINATIONS
        public DbSet<Patient> Patients { get; set; }
        public DbSet<ExamResult> ExamResults { get; set; }


        // COST & REVENUE
        public DbSet<GroupCost> GroupCosts { get; set; }
        public DbSet<CostSnapshot> CostSnapshots { get; set; }
        public DbSet<Overhead> Overheads { get; set; }
        public DbSet<SupplyItem> SupplyItems { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<ContractRevenueSummary> ContractRevenueSummaries { get; set; }
        public DbSet<ContractFinancialSummary> ContractFinancialSummaries { get; set; }


        // SYSTEM
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<PasswordResetRequest> PasswordResetRequests { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Station> Stations { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---- UNIQUE INDEXES ----
            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Username).IsUnique();

            modelBuilder.Entity<Permission>()
                .HasIndex(p => p.PermissionKey).IsUnique();

            // AppUser -> Company (no cascade to avoid multi-path)
            modelBuilder.Entity<AppUser>()
                .HasOne(u => u.Company)
                .WithMany()
                .HasForeignKey(u => u.CompanyId)
                .OnDelete(DeleteBehavior.SetNull);

            // AppUser -> AppRole (primary role)
            modelBuilder.Entity<AppUser>()
                .HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // UserRole many-to-many
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // RolePermission many-to-many
            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // HealthContract -> CreatedByUser (no cascade)
            modelBuilder.Entity<HealthContract>()
                .HasOne(c => c.CreatedByUser)
                .WithMany()
                .HasForeignKey(c => c.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Map Status Enum to string in DB
            modelBuilder.Entity<HealthContract>()
                .Property(c => c.Status)
                .HasConversion<string>();

            // ContractApprovalHistory -> ApprovedByUser (no cascade)
            modelBuilder.Entity<ContractApprovalHistory>()
                .HasOne(h => h.ApprovedByUser)
                .WithMany()
                .HasForeignKey(h => h.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ContractApprovalHistory -> HealthContract
            modelBuilder.Entity<ContractApprovalHistory>()
                .HasOne(h => h.HealthContract)
                .WithMany(c => c.ApprovalHistories)
                .HasForeignKey(h => h.HealthContractId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApprovalStep>()
                .HasOne(s => s.HealthContract)
                .WithMany()
                .HasForeignKey(s => s.HealthContractId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApprovalStep>()
                .HasOne(s => s.Role)
                .WithMany()
                .HasForeignKey(s => s.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApprovalHistory>()
                .HasOne(h => h.ApprovalStep)
                .WithMany()
                .HasForeignKey(h => h.ApprovalStepId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApprovalHistory>()
                .HasOne(h => h.Approver)
                .WithMany()
                .HasForeignKey(h => h.ApproverId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ApprovalHistory>()
                .HasOne(h => h.HealthContract)
                .WithMany()
                .HasForeignKey(h => h.HealthContractId)
                .OnDelete(DeleteBehavior.NoAction);

            // MedicalGroup -> ManagerUser (no cascade)
            modelBuilder.Entity<MedicalGroup>()
                .HasOne(g => g.ManagerUser)
                .WithMany()
                .HasForeignKey(g => g.ManagerUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // MedicalGroup -> GroupLeaderStaff (no cascade)
            modelBuilder.Entity<MedicalGroup>()
                .HasOne(g => g.GroupLeaderStaff)
                .WithMany()
                .HasForeignKey(g => g.GroupLeaderStaffId)
                .OnDelete(DeleteBehavior.SetNull);

            // GroupStaffDetail -> MedicalGroup (cascade delete)
            modelBuilder.Entity<GroupStaffDetail>()
                .HasOne(g => g.MedicalGroup)
                .WithMany(mg => mg.StaffDetails)
                .HasForeignKey(g => g.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // GroupStaffDetail -> Staff (no cascade)
            modelBuilder.Entity<GroupStaffDetail>()
                .HasOne(g => g.Staff)
                .WithMany(s => s.GroupStaffDetails)
                .HasForeignKey(g => g.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            // GroupStaffDetail -> Position (NO ACTION - tránh cascade cycle qua MedicalGroup)
            modelBuilder.Entity<GroupStaffDetail>()
                .HasOne(g => g.Position)
                .WithMany()
                .HasForeignKey(g => g.PositionId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<GroupStaffDetail>()
                .HasOne(g => g.GroupPositionQuota)
                .WithMany()
                .HasForeignKey(g => g.GroupPositionQuotaId)
                .OnDelete(DeleteBehavior.SetNull);

            // PHASE 1 HARDENING: Prevent double-booking a staff on the same exam date.
            // The partial filter excludes records where the staff was marked Absent,
            // so a legitimate absence does not block re-assignment on the same day.
            /*
            modelBuilder.Entity<GroupStaffDetail>()
                .HasIndex(g => new { g.StaffId, g.ExamDate })
                .IsUnique()
                .HasFilter("WorkStatus != 'Absent'")
                .HasDatabaseName("IX_GroupStaffDetail_StaffId_ExamDate_NoConflict");
            */

            modelBuilder.Entity<GroupPositionQuota>()
                .HasOne(q => q.MedicalGroup)
                .WithMany()
                .HasForeignKey(q => q.MedicalGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupPositionQuota>()
                .HasOne(q => q.Position)
                .WithMany()
                .HasForeignKey(q => q.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            // MedicalGroupPosition -> MedicalGroup (cascade)
            modelBuilder.Entity<MedicalGroupPosition>()
                .HasOne(p => p.MedicalGroup)
                .WithMany(g => g.Positions)
                .HasForeignKey(p => p.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // ScheduleCalendar -> MedicalGroup
            modelBuilder.Entity<ScheduleCalendar>()
                .HasOne(sc => sc.MedicalGroup)
                .WithMany()
                .HasForeignKey(sc => sc.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // ScheduleCalendar -> Staff (set null)
            modelBuilder.Entity<ScheduleCalendar>()
                .HasOne(sc => sc.Staff)
                .WithMany()
                .HasForeignKey(sc => sc.StaffId)
                .OnDelete(DeleteBehavior.SetNull);

            // GroupCost -> HealthContract (via MedicalGroup)
            modelBuilder.Entity<GroupCost>()
                .HasOne(gc => gc.MedicalGroup)
                .WithMany()
                .HasForeignKey(gc => gc.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupCost>()
                .HasOne(gc => gc.CalculatedByUser)
                .WithMany()
                .HasForeignKey(gc => gc.CalculatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // CostSnapshots
            modelBuilder.Entity<CostSnapshot>()
                .HasOne(cs => cs.HealthContract)
                .WithMany()
                .HasForeignKey(cs => cs.HealthContractId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CostSnapshot>()
                .HasOne(cs => cs.CreatedByUser)
                .WithMany()
                .HasForeignKey(cs => cs.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Overheads
            modelBuilder.Entity<Overhead>()
                .HasOne(o => o.HealthContract)
                .WithMany()
                .HasForeignKey(o => o.HealthContractId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Overhead>()
                .HasOne(o => o.RecordedByUser)
                .WithMany()
                .HasForeignKey(o => o.RecordedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // StockMovements
            modelBuilder.Entity<StockMovement>()
                .HasOne(sm => sm.MedicalGroup)
                .WithMany()
                .HasForeignKey(sm => sm.MedicalGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StockMovement>()
                .HasOne(sm => sm.RecordedByUser)
                .WithMany()
                .HasForeignKey(sm => sm.RecordedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // MedicalRecord configurations
            modelBuilder.Entity<MedicalRecord>()
                .HasIndex(m => new { m.GroupId, m.Status });
            modelBuilder.Entity<MedicalRecord>()
                .HasIndex(m => m.QrToken).IsUnique();
            /*
            modelBuilder.Entity<MedicalRecord>()
                .HasIndex(m => new { m.IDCardNumber, m.GroupId })
                .HasFilter("IDCardNumber IS NOT NULL") // Index uniqueness only for non-null IDCards
                .IsUnique();
            */

            modelBuilder.Entity<MedicalRecord>()
                .HasOne(m => m.MedicalGroup)
                .WithMany()
                .HasForeignKey(m => m.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MedicalRecord>()
                .HasOne(m => m.Patient)
                .WithMany()
                .HasForeignKey(m => m.PatientId)
                .OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<Station>()
                .Property(s => s.DefaultMinutesPerPatient)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Station>().HasData(
                new Station { StationCode = "CHECKIN", StationName = "Tiếp đón & Cấp số", ServiceType = "ADMIN", SortOrder = 1 },
                new Station { StationCode = "SINH_HIEU", StationName = "Đo sinh hiệu", ServiceType = "CLINICAL", SortOrder = 2 },
                new Station { StationCode = "LAY_MAU", StationName = "Lấy mẫu xét nghiệm", ServiceType = "LAB", SortOrder = 3 },
                new Station { StationCode = "XQUANG", StationName = "Chụp X-Quang", ServiceType = "IMAGING", SortOrder = 4 },
                new Station { StationCode = "SIEU_AM", StationName = "Siêu âm", ServiceType = "IMAGING", SortOrder = 5 },
                new Station { StationCode = "NOI_KHOA", StationName = "Khám Nội khoa", ServiceType = "CLINICAL", SortOrder = 6 },
                new Station { StationCode = "MAT_TAI_MUI_HONG", StationName = "Mắt - Tai Mũi Họng", ServiceType = "CLINICAL", SortOrder = 7 },
                new Station { StationCode = "QC", StationName = "Kiểm tra hồ sơ (QC)", ServiceType = "ADMIN", SortOrder = 99 }
            );

            // B2C & B2B Financial Relationships
            // ---- SEED DATA ----

            // ---- SEED DATA ----

            // Seed Roles
            modelBuilder.Entity<AppRole>().HasData(
                new AppRole { RoleId = 1, RoleName = "Admin", Description = "Quản trị hệ thống" },
                new AppRole { RoleId = 2, RoleName = "PersonnelManager", Description = "Quản lý nhân sự" },
                new AppRole { RoleId = 3, RoleName = "ContractManager", Description = "Quản lý hợp đồng" },
                new AppRole { RoleId = 4, RoleName = "PayrollManager", Description = "Quản lý tính lương" },
                new AppRole { RoleId = 5, RoleName = "MedicalGroupManager", Description = "Quản lý đoàn khám" },
                new AppRole { RoleId = 6, RoleName = "WarehouseManager", Description = "Quản lý kho vật tư" },

                new AppRole { RoleId = 7, RoleName = "MedicalStaff", Description = "Nhân viên đi đoàn" },

                new AppRole { RoleId = 8, RoleName = "Customer", Description = "Đại diện doanh nghiệp đối tác" }

            );

            // Seed Default Admin User (Pass: admin123)
            modelBuilder.Entity<AppUser>().HasData(
                new AppUser
                {
                    UserId = 1,
                    Username = "admin",
                    PasswordHash = "$2a$11$azxY7U9nnOEV4xF4Cto.gOLsbvUKtALtNoqMjMFRhVfP/45V1BrJ.",
                    FullName = "System Administrator",
                    RoleId = 1,
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                }
            );

            // Seed Permissions
            modelBuilder.Entity<Permission>().HasData(
                // HopDong module
                new Permission { PermissionId = 1, PermissionKey = "HopDong.View", PermissionName = "Xem hợp đồng", Module = "HopDong" },
                new Permission { PermissionId = 2, PermissionKey = "HopDong.Create", PermissionName = "Tạo hợp đồng", Module = "HopDong" },
                new Permission { PermissionId = 3, PermissionKey = "HopDong.Edit", PermissionName = "Sửa hợp đồng", Module = "HopDong" },
                new Permission { PermissionId = 4, PermissionKey = "HopDong.Approve", PermissionName = "Phê duyệt hợp đồng", Module = "HopDong" },
                new Permission { PermissionId = 5, PermissionKey = "HopDong.Reject", PermissionName = "Từ chối hợp đồng", Module = "HopDong" },
                new Permission { PermissionId = 6, PermissionKey = "HopDong.Upload", PermissionName = "Đính kèm file hợp đồng", Module = "HopDong" },
                // DoanKham module
                new Permission { PermissionId = 10, PermissionKey = "DoanKham.View", PermissionName = "Xem đoàn khám", Module = "DoanKham" },
                new Permission { PermissionId = 11, PermissionKey = "DoanKham.Create", PermissionName = "Tạo đoàn khám", Module = "DoanKham" },
                new Permission { PermissionId = 12, PermissionKey = "DoanKham.Edit", PermissionName = "Sửa đoàn khám", Module = "DoanKham" },
                new Permission { PermissionId = 13, PermissionKey = "DoanKham.SetPosition", PermissionName = "Thiết lập vị trí đoàn", Module = "DoanKham" },
                new Permission { PermissionId = 14, PermissionKey = "DoanKham.AssignStaff", PermissionName = "Phân công nhân sự đoàn", Module = "DoanKham" },
                new Permission { PermissionId = 15, PermissionKey = "DoanKham.ManageOwn", PermissionName = "Quản lý đoàn mình phụ trách", Module = "DoanKham" },
                new Permission { PermissionId = 16, PermissionKey = "DoanKham.Lock", PermissionName = "Khóa sổ đoàn khám", Module = "DoanKham" },
                // LichKham module
                new Permission { PermissionId = 20, PermissionKey = "LichKham.ViewOwn", PermissionName = "Xem lịch của mình", Module = "LichKham" },
                new Permission { PermissionId = 21, PermissionKey = "LichKham.ViewAll", PermissionName = "Xem toàn bộ lịch khám", Module = "LichKham" },
                // ChamCong module
                new Permission { PermissionId = 30, PermissionKey = "ChamCong.QR", PermissionName = "Mở QR chấm công", Module = "ChamCong" },
                new Permission { PermissionId = 31, PermissionKey = "ChamCong.CheckInOut", PermissionName = "Check-in/out nhân viên", Module = "ChamCong" },
                new Permission { PermissionId = 32, PermissionKey = "ChamCong.ViewAll", PermissionName = "Xem toàn bộ chấm công", Module = "ChamCong" },
                // BaoCao module
                new Permission { PermissionId = 40, PermissionKey = "BaoCao.View", PermissionName = "Xem báo cáo", Module = "BaoCao" },
                new Permission { PermissionId = 41, PermissionKey = "BaoCao.QC", PermissionName = "Thực hiện QC hồ sơ", Module = "BaoCao" },
                // Kho module
                new Permission { PermissionId = 50, PermissionKey = "Kho.View", PermissionName = "Xem kho vật tư", Module = "Kho" },
                new Permission { PermissionId = 51, PermissionKey = "Kho.Edit", PermissionName = "Nhập xuất kho", Module = "Kho" },
                // Operational module
                new Permission { PermissionId = 60, PermissionKey = "QuyetToan.Edit", PermissionName = "Sửa phát sinh quyết toán", Module = "TaiChinh" },
                // PHASE 1: New fine-grained financial permissions
                new Permission { PermissionId = 62, PermissionKey = "QuyetToan.Calculate", PermissionName = "Tính toán quyết toán", Module = "TaiChinh" },
                new Permission { PermissionId = 63, PermissionKey = "QuyetToan.Finalize", PermissionName = "Chốt xác nhận quyết toán", Module = "TaiChinh" },
                new Permission { PermissionId = 64, PermissionKey = "BaoCao.ViewFinance", PermissionName = "Xem báo cáo tài chính P&L", Module = "BaoCao" },
                new Permission { PermissionId = 65, PermissionKey = "BaoCao.Export", PermissionName = "Xuất báo cáo", Module = "BaoCao" },
                // HeThong module
                new Permission { PermissionId = 100, PermissionKey = "HeThong.UserManage", PermissionName = "Quản lý người dùng", Module = "HeThong" },
                new Permission { PermissionId = 101, PermissionKey = "HeThong.RoleManage", PermissionName = "Quản lý vai trò & quyền", Module = "HeThong" },

                // PhongBan module (new)
                new Permission { PermissionId = 110, PermissionKey = "PhongBan.View", PermissionName = "Xem phòng ban", Module = "PhongBan" },
                new Permission { PermissionId = 111, PermissionKey = "PhongBan.Edit", PermissionName = "Sửa phòng ban", Module = "PhongBan" },

                // WorkRule module (new)
                new Permission { PermissionId = 120, PermissionKey = "WorkRule.View", PermissionName = "Xem bảng rule chức năng", Module = "WorkRule" },
                new Permission { PermissionId = 121, PermissionKey = "WorkRule.Edit", PermissionName = "Sửa bảng rule chức năng", Module = "WorkRule" },

                // AI Suggestion module (new)
                new Permission { PermissionId = 130, PermissionKey = "AI.SuggestStaff", PermissionName = "AI gợi ý phân công nhân sự", Module = "AI" },

                // Inventory Reports module (new)
                new Permission { PermissionId = 140, PermissionKey = "Kho.Reports", PermissionName = "Xem báo cáo tồn kho", Module = "Kho" },
                new Permission { PermissionId = 141, PermissionKey = "Kho.Import", PermissionName = "Import phiếu nhập kho", Module = "Kho" },
                new Permission { PermissionId = 142, PermissionKey = "Kho.Export", PermissionName = "Export phiếu xuất kho", Module = "Kho" }

            );

            // Seed RolePermissions - Admin gets all (including Phase 1 new permissions)
            var adminPermissions = new int[] { 1, 2, 3, 4, 5, 6, 10, 11, 12, 13, 14, 15, 16, 20, 21, 30, 31, 32, 40, 41, 50, 51, 60, 62, 63, 64, 65, 100, 101, 110, 111, 120, 121, 130, 140, 141, 142 };
            var adminRolePerms = adminPermissions.Select((pid, idx) =>
                new RolePermission { Id = idx + 1, RoleId = 1, PermissionId = pid }).ToList();

            // Contract Manager (HCNS)
            var contractPermissions = new int[] { 1, 2, 3, 4, 5, 6, 110, 120 };
            var contractRolePerms = contractPermissions.Select((pid, idx) =>
                new RolePermission { Id = 100 + idx, RoleId = 3, PermissionId = pid }).ToList();

            // Medical Group Manager
            var mgPermissions = new int[] { 10, 11, 12, 13, 14, 15, 16, 21, 30, 130 };
            var mgRolePerms = mgPermissions.Select((pid, idx) =>
                new RolePermission { Id = 200 + idx, RoleId = 5, PermissionId = pid }).ToList();

            // Personnel Manager
            var hrPermissions = new int[] { 14, 21, 32, 100, 110 };
            var hrRolePerms = hrPermissions.Select((pid, idx) =>
                new RolePermission { Id = 300 + idx, RoleId = 2, PermissionId = pid }).ToList();

            // Accountant Role - can calculate and view financial reports
            var accountantPermissions = new int[] { 1, 40, 60, 62, 63, 64, 65, 110 };
            var accountantRolePerms = accountantPermissions.Select((pid, idx) =>
                new RolePermission { Id = 600 + idx, RoleId = 9, PermissionId = pid }).ToList();

            // QA Role
            var qaPermissions = new int[] { 10, 21, 40, 41, 110 };
            var qaRolePerms = qaPermissions.Select((pid, idx) =>
                new RolePermission { Id = 400 + idx, RoleId = 11, PermissionId = pid }).ToList();

            // Warehouse Manager
            var whPermissions = new int[] { 50, 51, 140, 141, 142 };
            var whRolePerms = whPermissions.Select((pid, idx) =>
                new RolePermission { Id = 500 + idx, RoleId = 6, PermissionId = pid }).ToList();

            var allSeedPerms = new List<RolePermission>();
            allSeedPerms.AddRange(adminRolePerms);
            allSeedPerms.AddRange(contractRolePerms);
            allSeedPerms.AddRange(accountantRolePerms);
            allSeedPerms.AddRange(mgRolePerms);
            allSeedPerms.AddRange(hrRolePerms);
            allSeedPerms.AddRange(qaRolePerms);
            allSeedPerms.AddRange(whRolePerms);

            modelBuilder.Entity<RolePermission>().HasData(allSeedPerms);
        }
    }
}
