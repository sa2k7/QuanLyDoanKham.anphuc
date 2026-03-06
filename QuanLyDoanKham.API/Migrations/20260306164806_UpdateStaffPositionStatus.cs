using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyDoanKham.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStaffPositionStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WorkPosition",
                table: "GroupStaffDetails",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkStatus",
                table: "GroupStaffDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$uCQogxsA97UIcCvj5jrD6u51qfAoZYU711sZgYs0aCqEFb8Mr6m7K");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkPosition",
                table: "GroupStaffDetails");

            migrationBuilder.DropColumn(
                name: "WorkStatus",
                table: "GroupStaffDetails");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$Ht6qOTcBcL.WvYvJViJ5nO47sy22Gf0/Jyv7nkyCNwHECLdf6KQ96");
        }
    }
}
