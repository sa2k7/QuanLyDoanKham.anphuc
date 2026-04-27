using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuanLyDoanKham.API.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidationSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── CONSOLIDATE ROLES: SAFE MOVE ───────────────────────────────────
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM Roles WHERE RoleId IN (2, 3, 11)) UPDATE Users SET RoleId = 5 WHERE RoleId IN (2, 3, 11);");
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM Roles WHERE RoleId = 4) UPDATE Users SET RoleId = 9 WHERE RoleId = 4;");
            // ───────────────────────────────────────────────────────────────────

            migrationBuilder.DropIndex(
                name: "IX_GroupStaffDetail_StaffId_ExamDate_NoConflict",
                table: "GroupStaffDetails");

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 100);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 105);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 300);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 301);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 302);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 400);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 401);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 402);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 403);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 3);


            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 200,
                column: "PermissionId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 201,
                column: "PermissionId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 202,
                column: "PermissionId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 203,
                column: "PermissionId",
                value: 4);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 204,
                column: "PermissionId",
                value: 5);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 205,
                column: "PermissionId",
                value: 6);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 206,
                column: "PermissionId",
                value: 10);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 207,
                column: "PermissionId",
                value: 11);

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 208, 12, 5 },
                    { 209, 13, 5 },
                    { 210, 14, 5 },
                    { 211, 15, 5 },
                    { 212, 16, 5 },
                    { 213, 21, 5 },
                    { 214, 32, 5 },
                    { 215, 40, 5 },
                    { 216, 41, 5 }
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 5,
                columns: new[] { "Description", "RoleName" },
                values: new object[] { "Quản lý (Hợp đồng, Nhân sự, Đoàn khám)", "Manager" });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 7,
                column: "Description",
                value: "Nhân viên y tế");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 8,
                column: "Description",
                value: "Khách hàng đối tác");

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "Description", "RoleName" },
                values: new object[,]
                {
                    { 9, "Kế toán (Lương, Quyết toán, Báo cáo)", "Accountant" },
                    { 10, "Trưởng đoàn hiện trường", "GroupLeader" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 700, 10, 10 },
                    { 701, 30, 10 },
                    { 702, 31, 10 },
                    { 703, 32, 10 },
                    { 704, 21, 10 }
                });

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalRecords_MedicalGroups_MedicalGroupGroupId",
                table: "MedicalRecords");

            migrationBuilder.DropIndex(
                name: "IX_MedicalRecords_MedicalGroupGroupId",
                table: "MedicalRecords");

            migrationBuilder.DropIndex(
                name: "IX_GroupStaffDetails_StaffId",
                table: "GroupStaffDetails");

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
                keyValue: 210);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 211);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 212);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 213);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 214);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 215);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 216);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 700);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 701);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 702);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 703);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 704);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 10);

            migrationBuilder.DropColumn(
                name: "MedicalGroupGroupId",
                table: "MedicalRecords");

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 200,
                column: "PermissionId",
                value: 10);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 201,
                column: "PermissionId",
                value: 11);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 202,
                column: "PermissionId",
                value: 12);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 203,
                column: "PermissionId",
                value: 13);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 204,
                column: "PermissionId",
                value: 14);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 205,
                column: "PermissionId",
                value: 15);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 206,
                column: "PermissionId",
                value: 16);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 207,
                column: "PermissionId",
                value: 21);

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 400, 10, 11 },
                    { 401, 21, 11 },
                    { 402, 40, 11 },
                    { 403, 41, 11 }
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 5,
                columns: new[] { "Description", "RoleName" },
                values: new object[] { "Quản lý đoàn khám", "MedicalGroupManager" });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 7,
                column: "Description",
                value: "Nhân viên đi đoàn");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 8,
                column: "Description",
                value: "Đại diện doanh nghiệp đối tác");

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "Description", "RoleName" },
                values: new object[,]
                {
                    { 2, "Quản lý nhân sự", "PersonnelManager" },
                    { 3, "Quản lý hợp đồng", "ContractManager" },
                    { 4, "Quản lý tính lương", "PayrollManager" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 100, 1, 3 },
                    { 101, 2, 3 },
                    { 102, 3, 3 },
                    { 103, 4, 3 },
                    { 104, 5, 3 },
                    { 105, 6, 3 },
                    { 300, 14, 2 },
                    { 301, 21, 2 },
                    { 302, 32, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupStaffDetail_StaffId_ExamDate_NoConflict",
                table: "GroupStaffDetails",
                columns: new[] { "StaffId", "ExamDate" },
                unique: true,
                filter: "[WorkStatus] != 'Absent'");
        }
    }
}
