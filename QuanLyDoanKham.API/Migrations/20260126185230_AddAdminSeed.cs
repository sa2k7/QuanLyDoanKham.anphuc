using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyDoanKham.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CompanyId", "FullName", "PasswordHash", "RoleId", "Username" },
                values: new object[] { 1, null, "System Administrator", "$2a$11$59PIszTcdafQ4MKzvP5KPO.B7P5u4Y9flt39rbqwKFH9APJn/VjZe", 1, "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1);
        }
    }
}
