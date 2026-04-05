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

        // DEPARTMENTS
        public DbSet<Department> Departments { get; set; }

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

        // SCHEDULE
        public DbSet<ScheduleCalendar> ScheduleCalendars { get; set; }

        // SUPPLIES & WAREHOUSE
        public DbSet<Supply> Supplies { get; set; }
        public DbSet<SupplyInventoryVoucher> SupplyInventoryVouchers { get; set; }
        public DbSet<SupplyInventoryDetail> SupplyInventoryDetails { get; set; }

        // COST & REVENUE
        public DbSet<GroupCost> GroupCosts { get; set; }
        public DbSet<ContractRevenueSummary> ContractRevenueSummaries { get; set; }
        public DbSet<ContractFinancialSummary> ContractFinancialSummaries { get; set; }

        // PAYROLL
        public DbSet<PayrollRecord> PayrollRecords { get; set; }

        // SYSTEM
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<PasswordResetRequest> PasswordResetRequests { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        // B2C CLINICAL & B2B FINANCIAL MODULES
        public DbSet<Patient> Patients { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<ExamService> ExamServices { get; set; }
        public DbSet<MedicalRecordService> MedicalRecordServices { get; set; }
        public DbSet<ContractPackage> ContractPackages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---- UNIQUE INDEXES ----
            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Username).IsUnique();

            modelBuilder.Entity<Permission>()
                .HasIndex(p => p.PermissionKey).IsUnique();

            // ---- RELATIONSHIPS ----
            // Trành cascade kép: AppUser -> Department
            modelBuilder.Entity<AppUser>()
                .HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

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

            // SupplyInventoryDetail -> Voucher
            modelBuilder.Entity<SupplyInventoryDetail>()
                .HasOne(d => d.Voucher)
                .WithMany(v => v.Details)
                .HasForeignKey(d => d.VoucherId)
                .OnDelete(DeleteBehavior.Cascade);

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

            // Staff -> Department (set null)
            modelBuilder.Entity<Staff>()
                .HasOne(s => s.Department)
                .WithMany(d => d.Staffs)
                .HasForeignKey(s => s.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // B2C & B2B Financial Relationships
            modelBuilder.Entity<MedicalRecord>()
                .HasOne(m => m.Patient)
                .WithMany(p => p.MedicalRecords)
                .HasForeignKey(m => m.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MedicalRecordService>()
                .HasOne(ms => ms.MedicalRecord)
                .WithMany(m => m.Services)
                .HasForeignKey(ms => ms.RecordId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ContractPackage>()
                .HasOne(cp => cp.HealthContract)
                .WithMany(hc => hc.ContractPackages)
                .HasForeignKey(cp => cp.HealthContractId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---- SEED DATA ----

            // Seed Roles
            modelBuilder.Entity<AppRole>().HasData(
                new AppRole { RoleId = 1, RoleName = "Admin", Description = "Quản trị hệ thống" },
                new AppRole { RoleId = 2, RoleName = "PersonnelManager", Description = "Quản lý nhân sự" },
                new AppRole { RoleId = 3, RoleName = "ContractManager", Description = "Quản lý hợp đồng" },
                new AppRole { RoleId = 4, RoleName = "PayrollManager", Description = "Quản lý tính lương" },
                new AppRole { RoleId = 5, RoleName = "MedicalGroupManager", Description = "Quản lý đoàn khám" },
                new AppRole { RoleId = 6, RoleName = "WarehouseManager", Description = "Quản lý kho vật tư" },
                new AppRole { RoleId = 7, RoleName = "GroupLeader", Description = "Trưởng đoàn khám" },
                new AppRole { RoleId = 8, RoleName = "MedicalStaff", Description = "Nhân viên đi đoàn" },
                new AppRole { RoleId = 9, RoleName = "Accountant", Description = "Kế toán" },
                new AppRole { RoleId = 10, RoleName = "Customer", Description = "Đại diện doanh nghiệp đối tác" }
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
                // LichKham module
                new Permission { PermissionId = 20, PermissionKey = "LichKham.ViewOwn", PermissionName = "Xem lịch của mình", Module = "LichKham" },
                new Permission { PermissionId = 21, PermissionKey = "LichKham.ViewAll", PermissionName = "Xem toàn bộ lịch khám", Module = "LichKham" },
                // ChamCong module
                new Permission { PermissionId = 30, PermissionKey = "ChamCong.QR", PermissionName = "Mở QR chấm công", Module = "ChamCong" },
                new Permission { PermissionId = 31, PermissionKey = "ChamCong.CheckInOut", PermissionName = "Check-in/out nhân viên", Module = "ChamCong" },
                new Permission { PermissionId = 32, PermissionKey = "ChamCong.ViewAll", PermissionName = "Xem toàn bộ chấm công", Module = "ChamCong" },
                // Kho module
                new Permission { PermissionId = 40, PermissionKey = "Kho.View", PermissionName = "Xem kho vật tư", Module = "Kho" },
                new Permission { PermissionId = 41, PermissionKey = "Kho.Import", PermissionName = "Nhập kho", Module = "Kho" },
                new Permission { PermissionId = 42, PermissionKey = "Kho.Export", PermissionName = "Xuất kho", Module = "Kho" },
                // Luong module
                new Permission { PermissionId = 50, PermissionKey = "Luong.View", PermissionName = "Xem bảng lương", Module = "Luong" },
                new Permission { PermissionId = 51, PermissionKey = "Luong.Manage", PermissionName = "Tính và duyệt lương", Module = "Luong" },
                // NhanSu module
                new Permission { PermissionId = 60, PermissionKey = "NhanSu.View", PermissionName = "Xem nhân sự", Module = "NhanSu" },
                new Permission { PermissionId = 61, PermissionKey = "NhanSu.Manage", PermissionName = "Quản lý nhân sự", Module = "NhanSu" },
                // BaoCao module
                new Permission { PermissionId = 70, PermissionKey = "BaoCao.View", PermissionName = "Xem báo cáo", Module = "BaoCao" },
                new Permission { PermissionId = 71, PermissionKey = "BaoCao.Export", PermissionName = "Xuất báo cáo", Module = "BaoCao" },
                // HeThong module
                new Permission { PermissionId = 80, PermissionKey = "HeThong.UserManage", PermissionName = "Quản lý tài khoản", Module = "HeThong" },
                new Permission { PermissionId = 81, PermissionKey = "HeThong.RoleManage", PermissionName = "Quản lý phân quyền", Module = "HeThong" }
            );

            // Seed RolePermissions - Admin gets all
            // Admin: chÆ°a phÃ©p thao tÃ¡c nghiá»‡p vá»¥ trá»±c tiáº¿p (PoLP) â†’ chá»‰ quyá»n xem + cáº¥u hÃ¬nh há»‡ thá»‘ng
            var adminPermissions = new int[] { 1, 10, 21, 32, 40, 50, 60, 70, 80, 81 };
            var adminRolePerms = adminPermissions.Select((pid, idx) =>
                new RolePermission { Id = idx + 1, RoleId = 1, PermissionId = pid }).ToList();

            // Contract Manager
            var contractPermissions = new int[] { 1, 2, 3, 4, 5, 6 };
            var contractRolePerms = contractPermissions.Select((pid, idx) =>
                new RolePermission { Id = 100 + idx, RoleId = 3, PermissionId = pid }).ToList();

            // Medical Group Manager
            var mgPermissions = new int[] { 10, 11, 12, 13, 14, 15, 21 };
            var mgRolePerms = mgPermissions.Select((pid, idx) =>
                new RolePermission { Id = 200 + idx, RoleId = 5, PermissionId = pid }).ToList();

            // Personnel Manager
            var hrPermissions = new int[] { 60, 61, 14, 21 };
            var hrRolePerms = hrPermissions.Select((pid, idx) =>
                new RolePermission { Id = 300 + idx, RoleId = 2, PermissionId = pid }).ToList();

            // Payroll Manager
            var payrollPermissions = new int[] { 50, 51, 60, 21, 32 };
            var payrollRolePerms = payrollPermissions.Select((pid, idx) =>
                new RolePermission { Id = 400 + idx, RoleId = 4, PermissionId = pid }).ToList();

            // Warehouse Manager
            var warehousePermissions = new int[] { 40, 41, 42 };
            var warehouseRolePerms = warehousePermissions.Select((pid, idx) =>
                new RolePermission { Id = 500 + idx, RoleId = 6, PermissionId = pid }).ToList();

            // Group Leader
            var leaderPermissions = new int[] { 10, 30, 31, 21 };
            var leaderRolePerms = leaderPermissions.Select((pid, idx) =>
                new RolePermission { Id = 600 + idx, RoleId = 7, PermissionId = pid }).ToList();

            // Medical Staff
            var staffPermissions = new int[] { 20 };
            var staffRolePerms = staffPermissions.Select((pid, idx) =>
                new RolePermission { Id = 700 + idx, RoleId = 8, PermissionId = pid }).ToList();

            // Accountant
            var accountantPermissions = new int[] { 70, 71, 50, 1 };
            var accountantRolePerms = accountantPermissions.Select((pid, idx) =>
                new RolePermission { Id = 800 + idx, RoleId = 9, PermissionId = pid }).ToList();

            var allSeedPerms = new List<RolePermission>();
            allSeedPerms.AddRange(adminRolePerms);
            allSeedPerms.AddRange(contractRolePerms);
            allSeedPerms.AddRange(mgRolePerms);
            allSeedPerms.AddRange(hrRolePerms);
            allSeedPerms.AddRange(payrollRolePerms);
            allSeedPerms.AddRange(warehouseRolePerms);
            allSeedPerms.AddRange(leaderRolePerms);
            allSeedPerms.AddRange(staffRolePerms);
            allSeedPerms.AddRange(accountantRolePerms);

            modelBuilder.Entity<RolePermission>().HasData(allSeedPerms);

            // Seed ContractApprovalSteps (default 2-level workflow)
            modelBuilder.Entity<ContractApprovalStep>().HasData(
                new ContractApprovalStep
                {
                    StepId = 1,
                    StepOrder = 1,
                    StepName = "Trưởng phòng xem xét",
                    RequiredPermission = "HopDong.Approve",
                    IsActive = true
                },
                new ContractApprovalStep
                {
                    StepId = 2,
                    StepOrder = 2,
                    StepName = "Ban giám đốc phê duyệt",
                    RequiredPermission = "HopDong.Approve",
                    IsActive = true
                }
            );

            // Seed Default Departments
            modelBuilder.Entity<Department>().HasData(
                new Department { DepartmentId = 1, DepartmentName = "Hành chính - Nhân sự", DepartmentCode = "HCNS", CreatedAt = new DateTime(2024, 1, 1) },
                new Department { DepartmentId = 2, DepartmentName = "Điều hành đoàn khám", DepartmentCode = "DHDK", CreatedAt = new DateTime(2024, 1, 1) },
                new Department { DepartmentId = 3, DepartmentName = "Kho - Vật tư", DepartmentCode = "KVT", CreatedAt = new DateTime(2024, 1, 1) },
                new Department { DepartmentId = 4, DepartmentName = "Kế toán", DepartmentCode = "KT", CreatedAt = new DateTime(2024, 1, 1) },
                new Department { DepartmentId = 5, DepartmentName = "Thống kê - Báo cáo", DepartmentCode = "TKBC", CreatedAt = new DateTime(2024, 1, 1) },
                new Department { DepartmentId = 6, DepartmentName = "Nhân viên đi khám", DepartmentCode = "NVDK", CreatedAt = new DateTime(2024, 1, 1) }
            );
        }
    }
}
