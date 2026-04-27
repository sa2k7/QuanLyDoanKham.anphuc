using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuanLyDoanKham.API.Migrations.Spec
{
    /// <inheritdoc />
    public partial class InitialSpecSchemaWithSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Spec_Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spec_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Spec_InventoryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    MinThreshold = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spec_InventoryItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Spec_Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Feature = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spec_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Spec_Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spec_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Spec_Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spec_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Spec_Users_Spec_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Spec_Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Spec_InventoryReceipts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReceiptDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Supplier = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spec_InventoryReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Spec_InventoryReceipts_Spec_InventoryItems_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Spec_InventoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Spec_RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spec_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_Spec_RolePermissions_Spec_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Spec_Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Spec_RolePermissions_Spec_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Spec_Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Spec_Contracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    ApprovedByUserId = table.Column<int>(type: "int", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AttachmentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spec_Contracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Spec_Contracts_Spec_Users_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "Spec_Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Spec_Contracts_Spec_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Spec_Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Spec_Staffs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ProfessionalRole = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DailyRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spec_Staffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Spec_Staffs_Spec_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Spec_Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Spec_Staffs_Spec_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Spec_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Spec_UserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spec_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_Spec_UserRoles_Spec_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Spec_Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Spec_UserRoles_Spec_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Spec_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Spec_ExaminationTeams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TeamName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    ExamDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    LeaderStaffId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spec_ExaminationTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Spec_ExaminationTeams_Spec_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Spec_Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Spec_ExaminationTeams_Spec_Staffs_LeaderStaffId",
                        column: x => x.LeaderStaffId,
                        principalTable: "Spec_Staffs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Spec_ExaminationTeams_Spec_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Spec_Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Spec_AttendanceRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    CheckInTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CheckOutTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WorkUnits = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spec_AttendanceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Spec_AttendanceRecords_Spec_ExaminationTeams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Spec_ExaminationTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Spec_AttendanceRecords_Spec_Staffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Spec_Staffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Spec_ExamPositions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spec_ExamPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Spec_ExamPositions_Spec_ExaminationTeams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Spec_ExaminationTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Spec_InventoryIssues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IssueDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spec_InventoryIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Spec_InventoryIssues_Spec_ExaminationTeams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Spec_ExaminationTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Spec_InventoryIssues_Spec_InventoryItems_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Spec_InventoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Spec_OtherExpenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spec_OtherExpenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Spec_OtherExpenses_Spec_ExaminationTeams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Spec_ExaminationTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Spec_Patients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    WorkUnit = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spec_Patients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Spec_Patients_Spec_ExaminationTeams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Spec_ExaminationTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Spec_QRSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    QRCode = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spec_QRSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Spec_QRSessions_Spec_ExaminationTeams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Spec_ExaminationTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Spec_TeamFinances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    Revenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StaffCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaterialCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Profit = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spec_TeamFinances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Spec_TeamFinances_Spec_ExaminationTeams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Spec_ExaminationTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Spec_TeamStaffs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    ExamPositionId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spec_TeamStaffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Spec_TeamStaffs_Spec_ExamPositions_ExamPositionId",
                        column: x => x.ExamPositionId,
                        principalTable: "Spec_ExamPositions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Spec_TeamStaffs_Spec_ExaminationTeams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Spec_ExaminationTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Spec_TeamStaffs_Spec_Staffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Spec_Staffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Spec_PatientExamRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    ExamPositionId = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spec_PatientExamRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Spec_PatientExamRecords_Spec_ExamPositions_ExamPositionId",
                        column: x => x.ExamPositionId,
                        principalTable: "Spec_ExamPositions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Spec_PatientExamRecords_Spec_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Spec_Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Spec_Departments",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { 1, "HCNS", "Hành chính Nhân sự" },
                    { 2, "KHO", "Kho" },
                    { 3, "KETOAN", "Kế toán" }
                });

            migrationBuilder.InsertData(
                table: "Spec_Permissions",
                columns: new[] { "Id", "Action", "Feature" },
                values: new object[,]
                {
                    { 1, "View", "Contract" },
                    { 2, "Create", "Contract" },
                    { 3, "Edit", "Contract" },
                    { 4, "Approve", "Contract" },
                    { 5, "View", "Team" },
                    { 6, "Create", "Team" },
                    { 7, "Edit", "Team" },
                    { 8, "AssignStaff", "Team" },
                    { 9, "Generate", "QR" },
                    { 10, "View", "Attendance" },
                    { 11, "View", "Inventory" },
                    { 12, "Create", "Inventory" },
                    { 13, "View", "Finance" },
                    { 14, "View", "Report" },
                    { 15, "Manage", "Staff" },
                    { 16, "Manage", "Permission" }
                });

            migrationBuilder.InsertData(
                table: "Spec_Roles",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { 1, "HCNS", "HCNS" },
                    { 2, "KHO", "Kho" },
                    { 3, "MANAGER", "Manager" },
                    { 4, "TRUONGDOAN", "Trưởng Đoàn" },
                    { 5, "KETOAN", "Kế toán" },
                    { 6, "ADMIN", "Admin" }
                });

            migrationBuilder.InsertData(
                table: "Spec_RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 3, 1 },
                    { 4, 1 },
                    { 15, 1 },
                    { 11, 2 },
                    { 12, 2 },
                    { 5, 3 },
                    { 6, 3 },
                    { 7, 3 },
                    { 8, 3 },
                    { 10, 3 },
                    { 9, 4 },
                    { 10, 4 },
                    { 10, 5 },
                    { 13, 5 },
                    { 14, 5 },
                    { 1, 6 },
                    { 2, 6 },
                    { 3, 6 },
                    { 4, 6 },
                    { 5, 6 },
                    { 6, 6 },
                    { 7, 6 },
                    { 8, 6 },
                    { 9, 6 },
                    { 10, 6 },
                    { 11, 6 },
                    { 12, 6 },
                    { 13, 6 },
                    { 14, 6 },
                    { 15, 6 },
                    { 16, 6 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Spec_AttendanceRecords_StaffId_TeamId",
                table: "Spec_AttendanceRecords",
                columns: new[] { "StaffId", "TeamId" });

            migrationBuilder.CreateIndex(
                name: "IX_Spec_AttendanceRecords_TeamId",
                table: "Spec_AttendanceRecords",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Spec_Contracts_ApprovedByUserId",
                table: "Spec_Contracts",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Spec_Contracts_ContractCode",
                table: "Spec_Contracts",
                column: "ContractCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Spec_Contracts_CreatedByUserId",
                table: "Spec_Contracts",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Spec_Departments_Code",
                table: "Spec_Departments",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Spec_ExaminationTeams_ContractId",
                table: "Spec_ExaminationTeams",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Spec_ExaminationTeams_CreatedByUserId",
                table: "Spec_ExaminationTeams",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Spec_ExaminationTeams_ExamDate",
                table: "Spec_ExaminationTeams",
                column: "ExamDate");

            migrationBuilder.CreateIndex(
                name: "IX_Spec_ExaminationTeams_LeaderStaffId",
                table: "Spec_ExaminationTeams",
                column: "LeaderStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Spec_ExaminationTeams_TeamCode",
                table: "Spec_ExaminationTeams",
                column: "TeamCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Spec_ExamPositions_TeamId_Code",
                table: "Spec_ExamPositions",
                columns: new[] { "TeamId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Spec_InventoryIssues_ItemId_TeamId",
                table: "Spec_InventoryIssues",
                columns: new[] { "ItemId", "TeamId" });

            migrationBuilder.CreateIndex(
                name: "IX_Spec_InventoryIssues_TeamId",
                table: "Spec_InventoryIssues",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Spec_InventoryItems_Name",
                table: "Spec_InventoryItems",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Spec_InventoryReceipts_ItemId",
                table: "Spec_InventoryReceipts",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Spec_OtherExpenses_TeamId",
                table: "Spec_OtherExpenses",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Spec_PatientExamRecords_ExamPositionId",
                table: "Spec_PatientExamRecords",
                column: "ExamPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Spec_PatientExamRecords_PatientId_ExamPositionId",
                table: "Spec_PatientExamRecords",
                columns: new[] { "PatientId", "ExamPositionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Spec_Patients_Status",
                table: "Spec_Patients",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Spec_Patients_TeamId",
                table: "Spec_Patients",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Spec_Permissions_Feature_Action",
                table: "Spec_Permissions",
                columns: new[] { "Feature", "Action" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Spec_QRSessions_QRCode",
                table: "Spec_QRSessions",
                column: "QRCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Spec_QRSessions_TeamId_IsActive",
                table: "Spec_QRSessions",
                columns: new[] { "TeamId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Spec_RolePermissions_PermissionId",
                table: "Spec_RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Spec_Roles_Code",
                table: "Spec_Roles",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Spec_Staffs_DepartmentId",
                table: "Spec_Staffs",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Spec_Staffs_UserId",
                table: "Spec_Staffs",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Spec_TeamFinances_TeamId",
                table: "Spec_TeamFinances",
                column: "TeamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Spec_TeamStaffs_ExamPositionId",
                table: "Spec_TeamStaffs",
                column: "ExamPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Spec_TeamStaffs_StaffId",
                table: "Spec_TeamStaffs",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Spec_TeamStaffs_TeamId_StaffId_ExamPositionId",
                table: "Spec_TeamStaffs",
                columns: new[] { "TeamId", "StaffId", "ExamPositionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Spec_UserRoles_RoleId",
                table: "Spec_UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Spec_Users_DepartmentId",
                table: "Spec_Users",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Spec_Users_Username",
                table: "Spec_Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Spec_AttendanceRecords");

            migrationBuilder.DropTable(
                name: "Spec_InventoryIssues");

            migrationBuilder.DropTable(
                name: "Spec_InventoryReceipts");

            migrationBuilder.DropTable(
                name: "Spec_OtherExpenses");

            migrationBuilder.DropTable(
                name: "Spec_PatientExamRecords");

            migrationBuilder.DropTable(
                name: "Spec_QRSessions");

            migrationBuilder.DropTable(
                name: "Spec_RolePermissions");

            migrationBuilder.DropTable(
                name: "Spec_TeamFinances");

            migrationBuilder.DropTable(
                name: "Spec_TeamStaffs");

            migrationBuilder.DropTable(
                name: "Spec_UserRoles");

            migrationBuilder.DropTable(
                name: "Spec_InventoryItems");

            migrationBuilder.DropTable(
                name: "Spec_Patients");

            migrationBuilder.DropTable(
                name: "Spec_Permissions");

            migrationBuilder.DropTable(
                name: "Spec_ExamPositions");

            migrationBuilder.DropTable(
                name: "Spec_Roles");

            migrationBuilder.DropTable(
                name: "Spec_ExaminationTeams");

            migrationBuilder.DropTable(
                name: "Spec_Contracts");

            migrationBuilder.DropTable(
                name: "Spec_Staffs");

            migrationBuilder.DropTable(
                name: "Spec_Users");

            migrationBuilder.DropTable(
                name: "Spec_Departments");
        }
    }
}
