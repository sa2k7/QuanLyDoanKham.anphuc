using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyDoanKham.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarPathToUserAndStaff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarPath",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarPath",
                table: "Staffs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SupplyTransactions",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplyId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplyTransactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_SupplyTransactions_Supplies_SupplyId",
                        column: x => x.SupplyId,
                        principalTable: "Supplies",
                        principalColumn: "SupplyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupplyTransactions_Users_ProcessedByUserId",
                        column: x => x.ProcessedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "AvatarPath", "PasswordHash" },
                values: new object[] { null, "$2a$11$RJlYff2jsFp7MjA0IpoUk.pGdv3JgzIjKlyt5MXRQ7HFFIKWjEE1a" });

            migrationBuilder.CreateIndex(
                name: "IX_SupplyTransactions_ProcessedByUserId",
                table: "SupplyTransactions",
                column: "ProcessedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyTransactions_SupplyId",
                table: "SupplyTransactions",
                column: "SupplyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupplyTransactions");

            migrationBuilder.DropColumn(
                name: "AvatarPath",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AvatarPath",
                table: "Staffs");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$4.HBcgWMaQla58Z5.6eayOtzVWm9Y9tNAp40/c6/lg/7wZIFs/kDq");
        }
    }
}
