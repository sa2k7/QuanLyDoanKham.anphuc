using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyDoanKham.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePayrollRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Details",
                table: "PayrollRecords");

            migrationBuilder.DropColumn(
                name: "TotalDays",
                table: "PayrollRecords");

            migrationBuilder.RenameColumn(
                name: "GeneratedDate",
                table: "PayrollRecords",
                newName: "GeneratedAt");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "PayrollRecords",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "PayrollRecords");

            migrationBuilder.RenameColumn(
                name: "GeneratedAt",
                table: "PayrollRecords",
                newName: "GeneratedDate");

            migrationBuilder.AddColumn<string>(
                name: "Details",
                table: "PayrollRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalDays",
                table: "PayrollRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
