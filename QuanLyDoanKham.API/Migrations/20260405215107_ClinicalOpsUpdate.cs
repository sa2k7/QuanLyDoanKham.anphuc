using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyDoanKham.API.Migrations
{
    /// <inheritdoc />
    public partial class ClinicalOpsUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "IdentityCard",
                table: "Patients",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Patients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeCode",
                table: "Patients",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PackageId",
                table: "MedicalRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContractName",
                table: "Contracts",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_PackageId",
                table: "MedicalRecords",
                column: "PackageId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalRecords_ContractPackages_PackageId",
                table: "MedicalRecords",
                column: "PackageId",
                principalTable: "ContractPackages",
                principalColumn: "PackageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalRecords_ContractPackages_PackageId",
                table: "MedicalRecords");

            migrationBuilder.DropIndex(
                name: "IX_MedicalRecords_PackageId",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "EmployeeCode",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "PackageId",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "ContractName",
                table: "Contracts");

            migrationBuilder.AlterColumn<string>(
                name: "IdentityCard",
                table: "Patients",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);
        }
    }
}
