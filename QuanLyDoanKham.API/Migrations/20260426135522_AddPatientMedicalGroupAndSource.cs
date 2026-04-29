using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyDoanKham.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientMedicalGroupAndSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Convert Status from nvarchar to int - first update existing string values to numeric equivalents
            migrationBuilder.Sql(@"
                UPDATE [Contracts] SET [Status] = 0 WHERE [Status] = 'Draft' OR [Status] = '0';
                UPDATE [Contracts] SET [Status] = 1 WHERE [Status] = 'PendingApproval' OR [Status] = '1';
                UPDATE [Contracts] SET [Status] = 2 WHERE [Status] = 'Approved' OR [Status] = '2';
                UPDATE [Contracts] SET [Status] = 3 WHERE [Status] = 'Rejected' OR [Status] = '3';
                UPDATE [Contracts] SET [Status] = 4 WHERE [Status] = 'Active' OR [Status] = '4';
                UPDATE [Contracts] SET [Status] = 5 WHERE [Status] = 'Finished' OR [Status] = '5';
                UPDATE [Contracts] SET [Status] = 6 WHERE [Status] = 'Cancelled' OR [Status] = '6';
                -- Set any remaining unknown values to 0 (Draft)
                UPDATE [Contracts] SET [Status] = 0 WHERE TRY_CAST([Status] AS int) IS NULL;
            ");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Contracts",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 26, 20, 55, 18, 296, DateTimeKind.Local).AddTicks(5498));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 26, 20, 55, 18, 296, DateTimeKind.Local).AddTicks(5515));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 26, 20, 55, 18, 296, DateTimeKind.Local).AddTicks(5519));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 26, 20, 55, 18, 296, DateTimeKind.Local).AddTicks(5522));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 26, 20, 55, 18, 296, DateTimeKind.Local).AddTicks(5525));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Contracts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 25, 17, 48, 24, 761, DateTimeKind.Local).AddTicks(3770));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 25, 17, 48, 24, 761, DateTimeKind.Local).AddTicks(3796));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 25, 17, 48, 24, 761, DateTimeKind.Local).AddTicks(3881));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 25, 17, 48, 24, 761, DateTimeKind.Local).AddTicks(3883));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 25, 17, 48, 24, 761, DateTimeKind.Local).AddTicks(3884));
        }
    }
}
