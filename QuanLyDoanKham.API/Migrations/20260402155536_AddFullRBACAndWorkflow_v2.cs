using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuanLyDoanKham.API.Migrations
{
    /// <inheritdoc />
    public partial class AddFullRBACAndWorkflow_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupStaffDetails_Staffs_StaffId",
                table: "GroupStaffDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Companies_CompanyId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "Department",
                table: "Staffs",
                newName: "DepartmentName");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Staffs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalaryType",
                table: "Staffs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StandardWorkDays",
                table: "Staffs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Roles",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BaseSalary",
                table: "PayrollRecords",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DailyRate",
                table: "PayrollRecords",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "GeneratedBy",
                table: "PayrollRecords",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalaryType",
                table: "PayrollRecords",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StandardWorkDays",
                table: "PayrollRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "PayrollRecords",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TotalActualDays",
                table: "PayrollRecords",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "MedicalGroups",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GroupLeaderStaffId",
                table: "MedicalGroups",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ManagerUserId",
                table: "MedicalGroups",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slot",
                table: "MedicalGroups",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeamCode",
                table: "MedicalGroups",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "MedicalGroups",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "MedicalGroups",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PositionId",
                table: "GroupStaffDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContractCode",
                table: "Contracts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Contracts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Contracts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentApprovalStep",
                table: "Contracts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Contracts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Contracts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ContractApprovalHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HealthContractId = table.Column<int>(type: "int", nullable: false),
                    StepOrder = table.Column<int>(type: "int", nullable: false),
                    StepName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Action = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedByUserId = table.Column<int>(type: "int", nullable: false),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractApprovalHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractApprovalHistories_Contracts_HealthContractId",
                        column: x => x.HealthContractId,
                        principalTable: "Contracts",
                        principalColumn: "HealthContractId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContractApprovalHistories_Users_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContractApprovalSteps",
                columns: table => new
                {
                    StepId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StepOrder = table.Column<int>(type: "int", nullable: false),
                    StepName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RequiredPermission = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractApprovalSteps", x => x.StepId);
                });

            migrationBuilder.CreateTable(
                name: "ContractAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HealthContractId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractAttachments_Contracts_HealthContractId",
                        column: x => x.HealthContractId,
                        principalTable: "Contracts",
                        principalColumn: "HealthContractId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractRevenueSummaries",
                columns: table => new
                {
                    SummaryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HealthContractId = table.Column<int>(type: "int", nullable: false),
                    TotalContractValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalGroupCosts = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Revenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalGroups = table.Column<int>(type: "int", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GeneratedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractRevenueSummaries", x => x.SummaryId);
                    table.ForeignKey(
                        name: "FK_ContractRevenueSummaries_Contracts_HealthContractId",
                        column: x => x.HealthContractId,
                        principalTable: "Contracts",
                        principalColumn: "HealthContractId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DepartmentCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.DepartmentId);
                });

            migrationBuilder.CreateTable(
                name: "GroupCosts",
                columns: table => new
                {
                    CostId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    StaffCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SupplyCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CalculatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CalculatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupCosts", x => x.CostId);
                    table.ForeignKey(
                        name: "FK_GroupCosts_MedicalGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "MedicalGroups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupCosts_Users_CalculatedByUserId",
                        column: x => x.CalculatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MedicalGroupPositions",
                columns: table => new
                {
                    PositionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    PositionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RequiredCount = table.Column<int>(type: "int", nullable: false),
                    AssignedCount = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalGroupPositions", x => x.PositionId);
                    table.ForeignKey(
                        name: "FK_MedicalGroupPositions_MedicalGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "MedicalGroups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PermissionKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PermissionName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Module = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.PermissionId);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleCalendars",
                columns: table => new
                {
                    CalendarId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    ExamDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StaffId = table.Column<int>(type: "int", nullable: true),
                    CheckInTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CheckOutTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsConfirmed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleCalendars", x => x.CalendarId);
                    table.ForeignKey(
                        name: "FK_ScheduleCalendars_MedicalGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "MedicalGroups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScheduleCalendars_Staffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staffs",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "PermissionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ContractApprovalSteps",
                columns: new[] { "StepId", "IsActive", "RequiredPermission", "StepName", "StepOrder" },
                values: new object[,]
                {
                    { 1, true, "HopDong.Approve", "Trưởng phòng xem xét", 1 },
                    { 2, true, "HopDong.Approve", "Ban giám đốc phê duyệt", 2 }
                });

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "DepartmentId", "CreatedAt", "DepartmentCode", "DepartmentName", "Description" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "HCNS", "Hành chính - Nhân sự", null },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "DHDK", "Điều hành đoàn khám", null },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "KVT", "Kho - Vật tư", null },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "KT", "Kế toán", null },
                    { 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "TKBC", "Thống kê - Báo cáo", null },
                    { 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "NVDK", "Nhân viên đi khám", null }
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "PermissionId", "Module", "PermissionKey", "PermissionName" },
                values: new object[,]
                {
                    { 1, "HopDong", "HopDong.View", "Xem hợp đồng" },
                    { 2, "HopDong", "HopDong.Create", "Tạo hợp đồng" },
                    { 3, "HopDong", "HopDong.Edit", "Sửa hợp đồng" },
                    { 4, "HopDong", "HopDong.Approve", "Phê duyệt hợp đồng" },
                    { 5, "HopDong", "HopDong.Reject", "Từ chối hợp đồng" },
                    { 6, "HopDong", "HopDong.Upload", "Đính kèm file hợp đồng" },
                    { 10, "DoanKham", "DoanKham.View", "Xem đoàn khám" },
                    { 11, "DoanKham", "DoanKham.Create", "Tạo đoàn khám" },
                    { 12, "DoanKham", "DoanKham.Edit", "Sửa đoàn khám" },
                    { 13, "DoanKham", "DoanKham.SetPosition", "Thiết lập vị trí đoàn" },
                    { 14, "DoanKham", "DoanKham.AssignStaff", "Phân công nhân sự đoàn" },
                    { 15, "DoanKham", "DoanKham.ManageOwn", "Quản lý đoàn mình phụ trách" },
                    { 20, "LichKham", "LichKham.ViewOwn", "Xem lịch của mình" },
                    { 21, "LichKham", "LichKham.ViewAll", "Xem toàn bộ lịch khám" },
                    { 30, "ChamCong", "ChamCong.QR", "Mở QR chấm công" },
                    { 31, "ChamCong", "ChamCong.CheckInOut", "Check-in/out nhân viên" },
                    { 32, "ChamCong", "ChamCong.ViewAll", "Xem toàn bộ chấm công" },
                    { 40, "Kho", "Kho.View", "Xem kho vật tư" },
                    { 41, "Kho", "Kho.Import", "Nhập kho" },
                    { 42, "Kho", "Kho.Export", "Xuất kho" },
                    { 50, "Luong", "Luong.View", "Xem bảng lương" },
                    { 51, "Luong", "Luong.Manage", "Tính và duyệt lương" },
                    { 60, "NhanSu", "NhanSu.View", "Xem nhân sự" },
                    { 61, "NhanSu", "NhanSu.Manage", "Quản lý nhân sự" },
                    { 70, "BaoCao", "BaoCao.View", "Xem báo cáo" },
                    { 71, "BaoCao", "BaoCao.Export", "Xuất báo cáo" },
                    { 80, "HeThong", "HeThong.UserManage", "Quản lý tài khoản" },
                    { 81, "HeThong", "HeThong.RoleManage", "Quản lý phân quyền" }
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 1,
                column: "Description",
                value: "Quản trị hệ thống");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 2,
                column: "Description",
                value: "Quản lý nhân sự");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 3,
                column: "Description",
                value: "Quản lý hợp đồng");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 4,
                column: "Description",
                value: "Quản lý tính lương");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 5,
                column: "Description",
                value: "Quản lý đoàn khám");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 6,
                column: "Description",
                value: "Quản lý kho vật tư");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 7,
                columns: new[] { "Description", "RoleName" },
                values: new object[] { "Trưởng đoàn khám", "GroupLeader" });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 8,
                columns: new[] { "Description", "RoleName" },
                values: new object[] { "Nhân viên đi đoàn", "MedicalStaff" });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "Description", "RoleName" },
                values: new object[,]
                {
                    { 9, "Kế toán", "Accountant" },
                    { 10, "Đại diện doanh nghiệp đối tác", "Customer" }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "DepartmentId", "IsActive" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 2, 1 },
                    { 3, 3, 1 },
                    { 4, 4, 1 },
                    { 5, 5, 1 },
                    { 6, 6, 1 },
                    { 7, 10, 1 },
                    { 8, 11, 1 },
                    { 9, 12, 1 },
                    { 10, 13, 1 },
                    { 11, 14, 1 },
                    { 12, 15, 1 },
                    { 13, 20, 1 },
                    { 14, 21, 1 },
                    { 15, 30, 1 },
                    { 16, 31, 1 },
                    { 17, 32, 1 },
                    { 18, 40, 1 },
                    { 19, 41, 1 },
                    { 20, 42, 1 },
                    { 21, 50, 1 },
                    { 22, 51, 1 },
                    { 23, 60, 1 },
                    { 24, 61, 1 },
                    { 25, 70, 1 },
                    { 26, 71, 1 },
                    { 27, 80, 1 },
                    { 28, 81, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_DepartmentId",
                table: "Users",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_DepartmentId",
                table: "Staffs",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalGroups_GroupLeaderStaffId",
                table: "MedicalGroups",
                column: "GroupLeaderStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalGroups_ManagerUserId",
                table: "MedicalGroups",
                column: "ManagerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupStaffDetails_PositionId",
                table: "GroupStaffDetails",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_CreatedByUserId",
                table: "Contracts",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractApprovalHistories_ApprovedByUserId",
                table: "ContractApprovalHistories",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractApprovalHistories_HealthContractId",
                table: "ContractApprovalHistories",
                column: "HealthContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractAttachments_HealthContractId",
                table: "ContractAttachments",
                column: "HealthContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractRevenueSummaries_HealthContractId",
                table: "ContractRevenueSummaries",
                column: "HealthContractId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupCosts_CalculatedByUserId",
                table: "GroupCosts",
                column: "CalculatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupCosts_GroupId",
                table: "GroupCosts",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalGroupPositions_GroupId",
                table: "MedicalGroupPositions",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_PermissionKey",
                table: "Permissions",
                column: "PermissionKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId",
                table: "RolePermissions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleCalendars_GroupId",
                table: "ScheduleCalendars",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleCalendars_StaffId",
                table: "ScheduleCalendars",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Users_CreatedByUserId",
                table: "Contracts",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupStaffDetails_MedicalGroupPositions_PositionId",
                table: "GroupStaffDetails",
                column: "PositionId",
                principalTable: "MedicalGroupPositions",
                principalColumn: "PositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupStaffDetails_Staffs_StaffId",
                table: "GroupStaffDetails",
                column: "StaffId",
                principalTable: "Staffs",
                principalColumn: "StaffId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalGroups_Staffs_GroupLeaderStaffId",
                table: "MedicalGroups",
                column: "GroupLeaderStaffId",
                principalTable: "Staffs",
                principalColumn: "StaffId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalGroups_Users_ManagerUserId",
                table: "MedicalGroups",
                column: "ManagerUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Staffs_Departments_DepartmentId",
                table: "Staffs",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Companies_CompanyId",
                table: "Users",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Departments_DepartmentId",
                table: "Users",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Users_CreatedByUserId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupStaffDetails_MedicalGroupPositions_PositionId",
                table: "GroupStaffDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupStaffDetails_Staffs_StaffId",
                table: "GroupStaffDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalGroups_Staffs_GroupLeaderStaffId",
                table: "MedicalGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalGroups_Users_ManagerUserId",
                table: "MedicalGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_Staffs_Departments_DepartmentId",
                table: "Staffs");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Companies_CompanyId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Departments_DepartmentId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "ContractApprovalHistories");

            migrationBuilder.DropTable(
                name: "ContractApprovalSteps");

            migrationBuilder.DropTable(
                name: "ContractAttachments");

            migrationBuilder.DropTable(
                name: "ContractRevenueSummaries");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "GroupCosts");

            migrationBuilder.DropTable(
                name: "MedicalGroupPositions");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "ScheduleCalendars");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_Users_DepartmentId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Staffs_DepartmentId",
                table: "Staffs");

            migrationBuilder.DropIndex(
                name: "IX_MedicalGroups_GroupLeaderStaffId",
                table: "MedicalGroups");

            migrationBuilder.DropIndex(
                name: "IX_MedicalGroups_ManagerUserId",
                table: "MedicalGroups");

            migrationBuilder.DropIndex(
                name: "IX_GroupStaffDetails_PositionId",
                table: "GroupStaffDetails");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_CreatedByUserId",
                table: "Contracts");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 10);

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "SalaryType",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "StandardWorkDays",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "BaseSalary",
                table: "PayrollRecords");

            migrationBuilder.DropColumn(
                name: "DailyRate",
                table: "PayrollRecords");

            migrationBuilder.DropColumn(
                name: "GeneratedBy",
                table: "PayrollRecords");

            migrationBuilder.DropColumn(
                name: "SalaryType",
                table: "PayrollRecords");

            migrationBuilder.DropColumn(
                name: "StandardWorkDays",
                table: "PayrollRecords");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PayrollRecords");

            migrationBuilder.DropColumn(
                name: "TotalActualDays",
                table: "PayrollRecords");

            migrationBuilder.DropColumn(
                name: "GroupLeaderStaffId",
                table: "MedicalGroups");

            migrationBuilder.DropColumn(
                name: "ManagerUserId",
                table: "MedicalGroups");

            migrationBuilder.DropColumn(
                name: "Slot",
                table: "MedicalGroups");

            migrationBuilder.DropColumn(
                name: "TeamCode",
                table: "MedicalGroups");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "MedicalGroups");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "MedicalGroups");

            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "GroupStaffDetails");

            migrationBuilder.DropColumn(
                name: "ContractCode",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "CurrentApprovalStep",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Contracts");

            migrationBuilder.RenameColumn(
                name: "DepartmentName",
                table: "Staffs",
                newName: "Department");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "MedicalGroups",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 7,
                column: "RoleName",
                value: "MedicalStaff");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 8,
                column: "RoleName",
                value: "Customer");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupStaffDetails_Staffs_StaffId",
                table: "GroupStaffDetails",
                column: "StaffId",
                principalTable: "Staffs",
                principalColumn: "StaffId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Companies_CompanyId",
                table: "Users",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
