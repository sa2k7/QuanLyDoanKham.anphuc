using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Models.Spec;

namespace QuanLyDoanKham.API.Data
{
    /// <summary>
    /// DbContext cho các entities theo spec thiết kế mới.
    /// Tách biệt với ApplicationDbContext (legacy) để tránh xung đột.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // AUTH & RBAC
        public DbSet<Department> Departments { get; set; }
        public DbSet<User> SpecUsers { get; set; }
        public DbSet<Role> SpecRoles { get; set; }
        public DbSet<UserRole> SpecUserRoles { get; set; }
        public DbSet<SpecPermission> SpecPermissions { get; set; }
        public DbSet<RolePermission> SpecRolePermissions { get; set; }

        // STAFF
        public DbSet<SpecStaff> SpecStaffs { get; set; }

        // CONTRACTS & TEAMS
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ExaminationTeam> ExaminationTeams { get; set; }
        public DbSet<ExamPosition> ExamPositions { get; set; }
        public DbSet<TeamStaff> TeamStaffs { get; set; }

        // PATIENTS
        public DbSet<SpecPatient> SpecPatients { get; set; }
        public DbSet<PatientExamRecord> PatientExamRecords { get; set; }

        // ATTENDANCE & QR
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
        public DbSet<QRSession> QRSessions { get; set; }

        // INVENTORY
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<InventoryReceipt> InventoryReceipts { get; set; }
        public DbSet<InventoryIssue> InventoryIssues { get; set; }

        // FINANCE
        public DbSet<TeamFinance> TeamFinances { get; set; }
        public DbSet<OtherExpense> OtherExpenses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureAuth(modelBuilder);
            ConfigureStaff(modelBuilder);
            ConfigureContracts(modelBuilder);
            ConfigureTeams(modelBuilder);
            ConfigurePatients(modelBuilder);
            ConfigureAttendance(modelBuilder);
            ConfigureInventory(modelBuilder);
            ConfigureFinance(modelBuilder);
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Departments (Requirement 8.2)
            modelBuilder.Entity<Department>().HasData(
                new Department { Id = 1, Name = "Hành chính Nhân sự", Code = "HCNS" },
                new Department { Id = 2, Name = "Kho", Code = "KHO" },
                new Department { Id = 3, Name = "Kế toán", Code = "KETOAN" }
            );

            // Seed Roles (Requirement 8.2)
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "HCNS", Code = "HCNS" },
                new Role { Id = 2, Name = "Kho", Code = "KHO" },
                new Role { Id = 3, Name = "Manager", Code = "MANAGER" },
                new Role { Id = 4, Name = "Trưởng Đoàn", Code = "TRUONGDOAN" },
                new Role { Id = 5, Name = "Kế toán", Code = "KETOAN" },
                new Role { Id = 6, Name = "Admin", Code = "ADMIN" }
            );

            // Seed Permissions (Requirement 8.2)
            modelBuilder.Entity<SpecPermission>().HasData(
                // Contract permissions
                new SpecPermission { Id = 1, Feature = "Contract", Action = "View" },
                new SpecPermission { Id = 2, Feature = "Contract", Action = "Create" },
                new SpecPermission { Id = 3, Feature = "Contract", Action = "Edit" },
                new SpecPermission { Id = 4, Feature = "Contract", Action = "Approve" },
                
                // Team permissions
                new SpecPermission { Id = 5, Feature = "Team", Action = "View" },
                new SpecPermission { Id = 6, Feature = "Team", Action = "Create" },
                new SpecPermission { Id = 7, Feature = "Team", Action = "Edit" },
                new SpecPermission { Id = 8, Feature = "Team", Action = "AssignStaff" },
                
                // QR and Attendance permissions
                new SpecPermission { Id = 9, Feature = "QR", Action = "Generate" },
                new SpecPermission { Id = 10, Feature = "Attendance", Action = "View" },
                
                // Inventory permissions
                new SpecPermission { Id = 11, Feature = "Inventory", Action = "View" },
                new SpecPermission { Id = 12, Feature = "Inventory", Action = "Create" },
                
                // Finance and Report permissions
                new SpecPermission { Id = 13, Feature = "Finance", Action = "View" },
                new SpecPermission { Id = 14, Feature = "Report", Action = "View" },
                
                // Staff and Permission management
                new SpecPermission { Id = 15, Feature = "Staff", Action = "Manage" },
                new SpecPermission { Id = 16, Feature = "Permission", Action = "Manage" }
            );

            // Seed Role-Permission mappings based on the permission matrix (Requirement 8.2)
            modelBuilder.Entity<RolePermission>().HasData(
                // HCNS (RoleId = 1)
                new RolePermission { RoleId = 1, PermissionId = 1 },  // Contract.View
                new RolePermission { RoleId = 1, PermissionId = 2 },  // Contract.Create
                new RolePermission { RoleId = 1, PermissionId = 3 },  // Contract.Edit
                new RolePermission { RoleId = 1, PermissionId = 4 },  // Contract.Approve
                new RolePermission { RoleId = 1, PermissionId = 15 }, // Staff.Manage
                
                // Kho (RoleId = 2)
                new RolePermission { RoleId = 2, PermissionId = 11 }, // Inventory.View
                new RolePermission { RoleId = 2, PermissionId = 12 }, // Inventory.Create
                
                // Manager (RoleId = 3)
                new RolePermission { RoleId = 3, PermissionId = 5 },  // Team.View
                new RolePermission { RoleId = 3, PermissionId = 6 },  // Team.Create
                new RolePermission { RoleId = 3, PermissionId = 7 },  // Team.Edit
                new RolePermission { RoleId = 3, PermissionId = 8 },  // Team.AssignStaff
                new RolePermission { RoleId = 3, PermissionId = 10 }, // Attendance.View
                
                // Trưởng Đoàn (RoleId = 4)
                new RolePermission { RoleId = 4, PermissionId = 9 },  // QR.Generate
                new RolePermission { RoleId = 4, PermissionId = 10 }, // Attendance.View
                
                // Kế toán (RoleId = 5)
                new RolePermission { RoleId = 5, PermissionId = 10 }, // Attendance.View
                new RolePermission { RoleId = 5, PermissionId = 13 }, // Finance.View
                new RolePermission { RoleId = 5, PermissionId = 14 }, // Report.View
                
                // Admin (RoleId = 6) - all permissions
                new RolePermission { RoleId = 6, PermissionId = 1 },
                new RolePermission { RoleId = 6, PermissionId = 2 },
                new RolePermission { RoleId = 6, PermissionId = 3 },
                new RolePermission { RoleId = 6, PermissionId = 4 },
                new RolePermission { RoleId = 6, PermissionId = 5 },
                new RolePermission { RoleId = 6, PermissionId = 6 },
                new RolePermission { RoleId = 6, PermissionId = 7 },
                new RolePermission { RoleId = 6, PermissionId = 8 },
                new RolePermission { RoleId = 6, PermissionId = 9 },
                new RolePermission { RoleId = 6, PermissionId = 10 },
                new RolePermission { RoleId = 6, PermissionId = 11 },
                new RolePermission { RoleId = 6, PermissionId = 12 },
                new RolePermission { RoleId = 6, PermissionId = 13 },
                new RolePermission { RoleId = 6, PermissionId = 14 },
                new RolePermission { RoleId = 6, PermissionId = 15 },
                new RolePermission { RoleId = 6, PermissionId = 16 }
            );
        }

        private static void ConfigureAuth(ModelBuilder modelBuilder)
        {
            // Use Spec_ prefix for all tables to avoid conflicts with ApplicationDbContext
            modelBuilder.Entity<Department>().ToTable("Spec_Departments");
            modelBuilder.Entity<User>().ToTable("Spec_Users");
            modelBuilder.Entity<Role>().ToTable("Spec_Roles");
            modelBuilder.Entity<UserRole>().ToTable("Spec_UserRoles");
            modelBuilder.Entity<SpecPermission>().ToTable("Spec_Permissions");
            modelBuilder.Entity<RolePermission>().ToTable("Spec_RolePermissions");

            // Department
            modelBuilder.Entity<Department>()
                .HasIndex(d => d.Code).IsUnique();

            // User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username).IsUnique();

            modelBuilder.Entity<User>()
                .HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Role
            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Code).IsUnique();

            // UserRole — composite PK
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

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

            // SpecPermission
            modelBuilder.Entity<SpecPermission>()
                .HasIndex(p => new { p.Feature, p.Action }).IsUnique();

            // RolePermission — composite PK
            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

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
        }

        private static void ConfigureStaff(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SpecStaff>().ToTable("Spec_Staffs");

            // SpecStaff — 1:1 with User
            modelBuilder.Entity<SpecStaff>()
                .HasIndex(s => s.UserId).IsUnique();

            modelBuilder.Entity<SpecStaff>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SpecStaff>()
                .HasOne(s => s.Department)
                .WithMany()
                .HasForeignKey(s => s.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
        }

        private static void ConfigureContracts(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Contract>().ToTable("Spec_Contracts");

            modelBuilder.Entity<Contract>()
                .HasIndex(c => c.ContractCode).IsUnique();

            modelBuilder.Entity<Contract>()
                .HasOne(c => c.CreatedByUser)
                .WithMany()
                .HasForeignKey(c => c.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Contract>()
                .HasOne(c => c.ApprovedByUser)
                .WithMany()
                .HasForeignKey(c => c.ApprovedByUserId)
                .OnDelete(DeleteBehavior.NoAction);
        }

        private static void ConfigureTeams(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExaminationTeam>().ToTable("Spec_ExaminationTeams");
            modelBuilder.Entity<ExamPosition>().ToTable("Spec_ExamPositions");
            modelBuilder.Entity<TeamStaff>().ToTable("Spec_TeamStaffs");

            // ExaminationTeam
            modelBuilder.Entity<ExaminationTeam>()
                .HasIndex(t => t.TeamCode).IsUnique();

            modelBuilder.Entity<ExaminationTeam>()
                .HasIndex(t => t.ContractId);

            modelBuilder.Entity<ExaminationTeam>()
                .HasIndex(t => t.ExamDate);

            modelBuilder.Entity<ExaminationTeam>()
                .HasOne(t => t.Contract)
                .WithMany(c => c.ExaminationTeams)
                .HasForeignKey(t => t.ContractId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExaminationTeam>()
                .HasOne(t => t.CreatedByUser)
                .WithMany()
                .HasForeignKey(t => t.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ExaminationTeam>()
                .HasOne(t => t.LeaderStaff)
                .WithMany()
                .HasForeignKey(t => t.LeaderStaffId)
                .OnDelete(DeleteBehavior.NoAction);

            // ExamPosition
            modelBuilder.Entity<ExamPosition>()
                .HasIndex(p => new { p.TeamId, p.Code }).IsUnique();

            modelBuilder.Entity<ExamPosition>()
                .HasOne(p => p.Team)
                .WithMany(t => t.ExamPositions)
                .HasForeignKey(p => p.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            // TeamStaff
            modelBuilder.Entity<TeamStaff>()
                .HasIndex(ts => new { ts.TeamId, ts.StaffId, ts.ExamPositionId });

            modelBuilder.Entity<TeamStaff>()
                .HasOne(ts => ts.Team)
                .WithMany(t => t.TeamStaffs)
                .HasForeignKey(ts => ts.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TeamStaff>()
                .HasOne(ts => ts.Staff)
                .WithMany(s => s.TeamStaffs)
                .HasForeignKey(ts => ts.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TeamStaff>()
                .HasOne(ts => ts.ExamPosition)
                .WithMany(p => p.TeamStaffs)
                .HasForeignKey(ts => ts.ExamPositionId)
                .OnDelete(DeleteBehavior.NoAction);
        }

        private static void ConfigurePatients(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SpecPatient>().ToTable("Spec_Patients");
            modelBuilder.Entity<PatientExamRecord>().ToTable("Spec_PatientExamRecords");

            // SpecPatient
            modelBuilder.Entity<SpecPatient>()
                .HasIndex(p => p.TeamId);

            modelBuilder.Entity<SpecPatient>()
                .HasIndex(p => p.Status);

            modelBuilder.Entity<SpecPatient>()
                .HasOne(p => p.Team)
                .WithMany(t => t.Patients)
                .HasForeignKey(p => p.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            // PatientExamRecord
            modelBuilder.Entity<PatientExamRecord>()
                .HasIndex(r => new { r.PatientId, r.ExamPositionId }).IsUnique();

            modelBuilder.Entity<PatientExamRecord>()
                .HasOne(r => r.Patient)
                .WithMany(p => p.ExamRecords)
                .HasForeignKey(r => r.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PatientExamRecord>()
                .HasOne(r => r.ExamPosition)
                .WithMany(p => p.PatientExamRecords)
                .HasForeignKey(r => r.ExamPositionId)
                .OnDelete(DeleteBehavior.NoAction);
        }

        private static void ConfigureAttendance(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AttendanceRecord>().ToTable("Spec_AttendanceRecords");
            modelBuilder.Entity<QRSession>().ToTable("Spec_QRSessions");

            // AttendanceRecord
            modelBuilder.Entity<AttendanceRecord>()
                .HasIndex(a => new { a.StaffId, a.TeamId });

            modelBuilder.Entity<AttendanceRecord>()
                .HasOne(a => a.Staff)
                .WithMany(s => s.AttendanceRecords)
                .HasForeignKey(a => a.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AttendanceRecord>()
                .HasOne(a => a.Team)
                .WithMany(t => t.AttendanceRecords)
                .HasForeignKey(a => a.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            // QRSession
            modelBuilder.Entity<QRSession>()
                .HasIndex(q => q.QRCode).IsUnique();

            modelBuilder.Entity<QRSession>()
                .HasIndex(q => new { q.TeamId, q.IsActive });

            modelBuilder.Entity<QRSession>()
                .HasOne(q => q.Team)
                .WithMany(t => t.QRSessions)
                .HasForeignKey(q => q.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private static void ConfigureInventory(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InventoryItem>().ToTable("Spec_InventoryItems");
            modelBuilder.Entity<InventoryReceipt>().ToTable("Spec_InventoryReceipts");
            modelBuilder.Entity<InventoryIssue>().ToTable("Spec_InventoryIssues");

            // InventoryItem
            modelBuilder.Entity<InventoryItem>()
                .HasIndex(i => i.Name);

            // InventoryReceipt
            modelBuilder.Entity<InventoryReceipt>()
                .HasIndex(r => r.ItemId);

            modelBuilder.Entity<InventoryReceipt>()
                .HasOne(r => r.Item)
                .WithMany(i => i.Receipts)
                .HasForeignKey(r => r.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // InventoryIssue
            modelBuilder.Entity<InventoryIssue>()
                .HasIndex(i => new { i.ItemId, i.TeamId });

            modelBuilder.Entity<InventoryIssue>()
                .HasOne(i => i.Item)
                .WithMany(item => item.Issues)
                .HasForeignKey(i => i.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InventoryIssue>()
                .HasOne(i => i.Team)
                .WithMany(t => t.InventoryIssues)
                .HasForeignKey(i => i.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private static void ConfigureFinance(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TeamFinance>().ToTable("Spec_TeamFinances");
            modelBuilder.Entity<OtherExpense>().ToTable("Spec_OtherExpenses");

            // TeamFinance — 1:1 with ExaminationTeam
            modelBuilder.Entity<TeamFinance>()
                .HasIndex(f => f.TeamId).IsUnique();

            modelBuilder.Entity<TeamFinance>()
                .HasOne(f => f.Team)
                .WithOne(t => t.TeamFinance)
                .HasForeignKey<TeamFinance>(f => f.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            // OtherExpense
            modelBuilder.Entity<OtherExpense>()
                .HasIndex(e => e.TeamId);

            modelBuilder.Entity<OtherExpense>()
                .HasOne(e => e.Team)
                .WithMany(t => t.OtherExpenses)
                .HasForeignKey(e => e.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
