using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyDoanKham.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientGroupIdAndSource_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MedicalGroupId",
                table: "Patients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "Patients",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 26, 21, 7, 11, 838, DateTimeKind.Local).AddTicks(4197));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 26, 21, 7, 11, 838, DateTimeKind.Local).AddTicks(4210));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 26, 21, 7, 11, 838, DateTimeKind.Local).AddTicks(4212));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 26, 21, 7, 11, 838, DateTimeKind.Local).AddTicks(4214));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 26, 21, 7, 11, 838, DateTimeKind.Local).AddTicks(4216));

            migrationBuilder.CreateIndex(
                name: "IX_Patients_MedicalGroupId",
                table: "Patients",
                column: "MedicalGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_MedicalGroups_MedicalGroupId",
                table: "Patients",
                column: "MedicalGroupId",
                principalTable: "MedicalGroups",
                principalColumn: "GroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Patients_MedicalGroups_MedicalGroupId",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Patients_MedicalGroupId",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "MedicalGroupId",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "Patients");

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
    }
}
