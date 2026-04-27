using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyDoanKham.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminFieldsToMedicalGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DepartureTime",
                table: "MedicalGroups",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExamContent",
                table: "MedicalGroups",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartTime",
                table: "MedicalGroups",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickupLocation",
                table: "GroupStaffDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartureTime",
                table: "MedicalGroups");

            migrationBuilder.DropColumn(
                name: "ExamContent",
                table: "MedicalGroups");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "MedicalGroups");

            migrationBuilder.DropColumn(
                name: "PickupLocation",
                table: "GroupStaffDetails");
        }
    }
}
