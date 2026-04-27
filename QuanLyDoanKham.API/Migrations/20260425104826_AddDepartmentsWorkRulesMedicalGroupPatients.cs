using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuanLyDoanKham.API.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentsWorkRulesMedicalGroupPatients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DepartmentCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.DepartmentId);
                });

            migrationBuilder.CreateTable(
                name: "MedicalGroupPatients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    ImportBatchId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExamFunction = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AddedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalGroupPatients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalGroupPatients_MedicalGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "MedicalGroups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicalGroupPatients_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffDepartments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    PositionInDepartment = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffDepartments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffDepartments_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StaffDepartments_Staffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staffs",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkRules",
                columns: table => new
                {
                    RuleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    FunctionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FunctionCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RequiredPermission = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkRules", x => x.RuleId);
                    table.ForeignKey(
                        name: "FK_WorkRules_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "DepartmentId", "CreatedAt", "DepartmentCode", "DepartmentName", "Description", "IsActive" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 4, 25, 17, 48, 24, 761, DateTimeKind.Local).AddTicks(3770), "HCNS", "Hành chính nhân sự", "Quản lý hợp đồng, nhân sự, tính lương", true },
                    { 2, new DateTime(2026, 4, 25, 17, 48, 24, 761, DateTimeKind.Local).AddTicks(3796), "KHO", "Kho vật tư", "Quản lý nhập xuất kho, tồn kho", true },
                    { 3, new DateTime(2026, 4, 25, 17, 48, 24, 761, DateTimeKind.Local).AddTicks(3881), "YTE", "Y tế", "Quản lý đoàn khám, khám chữa bệnh", true },
                    { 4, new DateTime(2026, 4, 25, 17, 48, 24, 761, DateTimeKind.Local).AddTicks(3883), "KT", "Kế toán", "Quản lý tài chính, báo cáo", true },
                    { 5, new DateTime(2026, 4, 25, 17, 48, 24, 761, DateTimeKind.Local).AddTicks(3884), "QLCL", "Quản lý chất lượng", "QC hồ sơ, kiểm tra chất lượng", true }
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "PermissionId", "Module", "PermissionKey", "PermissionName" },
                values: new object[,]
                {
                    { 110, "PhongBan", "PhongBan.View", "Xem phòng ban" },
                    { 111, "PhongBan", "PhongBan.Edit", "Sửa phòng ban" },
                    { 120, "WorkRule", "WorkRule.View", "Xem bảng rule chức năng" },
                    { 121, "WorkRule", "WorkRule.Edit", "Sửa bảng rule chức năng" },
                    { 130, "AI", "AI.SuggestStaff", "AI gợi ý phân công nhân sự" },
                    { 140, "Kho", "Kho.Reports", "Xem báo cáo tồn kho" },
                    { 141, "Kho", "Kho.Import", "Import phiếu nhập kho" },
                    { 142, "Kho", "Kho.Export", "Export phiếu xuất kho" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 208, 30, 5 },
                    { 303, 100, 2 },
                    { 30, 110, 1 },
                    { 31, 111, 1 },
                    { 32, 120, 1 },
                    { 33, 121, 1 },
                    { 34, 130, 1 },
                    { 35, 140, 1 },
                    { 36, 141, 1 },
                    { 37, 142, 1 },
                    { 106, 110, 3 },
                    { 107, 120, 3 },
                    { 209, 130, 5 },
                    { 304, 110, 2 },
                    { 404, 110, 11 },
                    { 502, 140, 6 },
                    { 503, 141, 6 },
                    { 504, 142, 6 },
                    { 607, 110, 9 }
                });

            migrationBuilder.InsertData(
                table: "WorkRules",
                columns: new[] { "RuleId", "DepartmentId", "Description", "DisplayOrder", "FunctionCode", "FunctionName", "IsActive", "IsRequired", "RequiredPermission" },
                values: new object[,]
                {
                    { 1, 1, null, 1, "HopDong.Create", "Tạo hợp đồng", true, true, "HopDong.Create" },
                    { 2, 1, null, 2, "HopDong.Approve", "Phê duyệt hợp đồng", true, true, "HopDong.Approve" },
                    { 3, 1, null, 3, "NhanSu.QLTatCa", "Quản lý tất cả nhân sự", true, true, "HeThong.UserManage" },
                    { 4, 1, null, 4, "NhanSu.QLHopDong", "Quản lý hợp đồng lao động", true, true, "HeThong.UserManage" },
                    { 5, 1, null, 5, "NhanSu.QLLuong", "Quản lý tính lương", true, true, "BaoCao.ViewFinance" },
                    { 10, 2, null, 1, "Kho.Nhap", "Nhập kho", true, true, "Kho.Edit" },
                    { 11, 2, null, 2, "Kho.Xuat", "Xuất kho", true, true, "Kho.Edit" },
                    { 12, 2, null, 3, "Kho.QLTon", "Quản lý tồn kho", true, true, "Kho.View" },
                    { 20, 3, null, 1, "DoanKham.Create", "Tạo đoàn khám", true, true, "DoanKham.Create" },
                    { 21, 3, null, 2, "DoanKham.Manage", "Quản lý đoàn khám", true, true, "DoanKham.ManageOwn" },
                    { 22, 3, null, 3, "DoanKham.QRCheckIn", "QR Check-in đoàn khám", true, true, "ChamCong.QR" },
                    { 23, 3, null, 4, "DoanKham.PhanCong", "Phân công nhân sự đoàn", true, true, "DoanKham.AssignStaff" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DepartmentCode",
                table: "Departments",
                column: "DepartmentCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalGroupPatients_GroupId_PatientId",
                table: "MedicalGroupPatients",
                columns: new[] { "GroupId", "PatientId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalGroupPatients_PatientId",
                table: "MedicalGroupPatients",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffDepartments_DepartmentId",
                table: "StaffDepartments",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffDepartments_StaffId_IsPrimary",
                table: "StaffDepartments",
                columns: new[] { "StaffId", "IsPrimary" },
                unique: true,
                filter: "IsPrimary = 1");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRules_DepartmentId_FunctionCode",
                table: "WorkRules",
                columns: new[] { "DepartmentId", "FunctionCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicalGroupPatients");

            migrationBuilder.DropTable(
                name: "StaffDepartments");

            migrationBuilder.DropTable(
                name: "WorkRules");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 106);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 107);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 208);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 209);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 303);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 304);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 404);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 502);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 503);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 504);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 607);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 110);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 111);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 120);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 121);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 130);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 140);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 141);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 142);
        }
    }
}
