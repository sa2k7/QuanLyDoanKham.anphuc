using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuanLyDoanKham.API.Migrations
{
    /// <inheritdoc />
    public partial class Phase1_ScheduleConflictIndex_FinancialPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GroupStaffDetails_StaffId",
                table: "GroupStaffDetails");

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "PermissionId", "Module", "PermissionKey", "PermissionName" },
                values: new object[,]
                {
                    { 60, "TaiChinh", "QuyetToan.Edit", "Sửa phát sinh quyết toán" },
                    { 61, "DieuPhoi", "DieuPhoi.Edit", "Hủy ca điều phối" },
                    { 62, "TaiChinh", "QuyetToan.Calculate", "Tính toán quyết toán" },
                    { 63, "TaiChinh", "QuyetToan.Finalize", "Chốt xác nhận quyết toán" },
                    { 64, "BaoCao", "BaoCao.ViewFinance", "Xem báo cáo tài chính P&L" },
                    { 65, "BaoCao", "BaoCao.Export", "Xuất báo cáo" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 600, 1, 9 },
                    { 601, 40, 9 },
                    { 23, 60, 1 },
                    { 24, 61, 1 },
                    { 25, 62, 1 },
                    { 26, 63, 1 },
                    { 27, 64, 1 },
                    { 28, 65, 1 },
                    { 602, 60, 9 },
                    { 603, 62, 9 },
                    { 604, 63, 9 },
                    { 605, 64, 9 },
                    { 606, 65, 9 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupStaffDetail_StaffId_ExamDate_NoConflict",
                table: "GroupStaffDetails",
                columns: new[] { "StaffId", "ExamDate" },
                unique: true,
                filter: "[WorkStatus] != 'Absent'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GroupStaffDetail_StaffId_ExamDate_NoConflict",
                table: "GroupStaffDetails");

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 600);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 601);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 602);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 603);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 604);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 605);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 606);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 62);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 63);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 64);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 65);

            migrationBuilder.CreateIndex(
                name: "IX_GroupStaffDetails_StaffId",
                table: "GroupStaffDetails",
                column: "StaffId");
        }
    }
}
