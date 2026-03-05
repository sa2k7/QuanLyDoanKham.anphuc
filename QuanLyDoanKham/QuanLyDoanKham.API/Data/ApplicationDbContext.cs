using Microsoft.EntityFrameworkCore;
using QuanLyDoanKham.API.Models;

namespace QuanLyDoanKham.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<AppUser> Users { get; set; }
        public DbSet<AppRole> Roles { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<HealthContract> Contracts { get; set; }
        public DbSet<MedicalGroup> MedicalGroups { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<GroupStaffDetail> GroupStaffDetails { get; set; }
        public DbSet<Supply> Supplies { get; set; }
        public DbSet<GroupSupplyDetail> GroupSupplyDetails { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<ExamResult> ExamResults { get; set; }
        public DbSet<PayrollRecord> PayrollRecords { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<SupplyTransaction> SupplyTransactions { get; set; }
        public DbSet<PasswordResetRequest> PasswordResetRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed Roles
            modelBuilder.Entity<AppRole>().HasData(
                new AppRole { RoleId = 1, RoleName = "Admin" },
                new AppRole { RoleId = 2, RoleName = "PersonnelManager" },   // Quản lý NHÂN SỰ
                new AppRole { RoleId = 3, RoleName = "ContractManager" },    // Quản lý CÔNG TY, HỢP ĐỒNG
                new AppRole { RoleId = 4, RoleName = "PayrollManager" },     // Quản lý TÍNH LƯƠNG
                new AppRole { RoleId = 5, RoleName = "MedicalGroupManager" },// Quản lý ĐOÀN KHÁM
                new AppRole { RoleId = 6, RoleName = "WarehouseManager" },   // Quản lý VẬT TƯ
                new AppRole { RoleId = 7, RoleName = "MedicalStaff" },       // Nhân viên đi khám (Chuyên môn)
                new AppRole { RoleId = 8, RoleName = "Customer" }            // Khách hàng (Xem kết quả)
            );

            // Seed Admin User (Pass: admin123)
            modelBuilder.Entity<AppUser>().HasData(
                new AppUser 
                { 
                    UserId = 1, 
                    Username = "admin", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), 
                    FullName = "System Administrator", 
                    RoleId = 1 
                }
            );

            // Fluent API configurations
            modelBuilder.Entity<GroupStaffDetail>()
                .HasOne(g => g.MedicalGroup)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
