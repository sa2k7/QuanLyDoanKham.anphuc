using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyDoanKham.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStaffFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "JobTitle",
                table: "Staffs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountName",
                table: "Staffs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                table: "Staffs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "Staffs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BirthYear",
                table: "Staffs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Staffs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Staffs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeCode",
                table: "Staffs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeType",
                table: "Staffs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullNameUnsigned",
                table: "Staffs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Staffs",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IDCardBackPath",
                table: "Staffs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IDCardFrontPath",
                table: "Staffs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IDCardNumber",
                table: "Staffs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Staffs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "Staffs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Staffs",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PracticeCertificatePath",
                table: "Staffs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxCode",
                table: "Staffs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$meK7l0U8WiI5O9u2Go5JE.OkljsKYx6Zg6JNJtcDgLBrUrS6tYdju");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankAccountName",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "BirthYear",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "EmployeeCode",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "EmployeeType",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "FullNameUnsigned",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "IDCardBackPath",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "IDCardFrontPath",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "IDCardNumber",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "PracticeCertificatePath",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "TaxCode",
                table: "Staffs");

            migrationBuilder.AlterColumn<string>(
                name: "JobTitle",
                table: "Staffs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$59PIszTcdafQ4MKzvP5KPO.B7P5u4Y9flt39rbqwKFH9APJn/VjZe");
        }
    }
}
