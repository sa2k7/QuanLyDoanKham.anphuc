using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuanLyDoanKham.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSuppliesInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RecordStationTasks_StationCode",
                table: "RecordStationTasks");

            migrationBuilder.AddColumn<int>(
                name: "SupplyId",
                table: "StockMovements",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SupplyItems",
                columns: table => new
                {
                    SupplyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CurrentStock = table.Column<int>(type: "int", nullable: false),
                    MinStockLevel = table.Column<int>(type: "int", nullable: false),
                    TypicalUnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplyItems", x => x.SupplyId);
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "PermissionId", "Module", "PermissionKey", "PermissionName" },
                values: new object[,]
                {
                    { 50, "Kho", "Kho.View", "Xem kho vật tư" },
                    { 51, "Kho", "Kho.Edit", "Nhập xuất kho" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 20, 50, 1 },
                    { 21, 51, 1 },
                    { 500, 50, 6 },
                    { 501, 51, 6 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_SupplyId",
                table: "StockMovements",
                column: "SupplyId");

            migrationBuilder.CreateIndex(
                name: "IX_RecordStationTasks_StationCode_Status",
                table: "RecordStationTasks",
                columns: new[] { "StationCode", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_SupplyItems_SupplyId",
                table: "StockMovements",
                column: "SupplyId",
                principalTable: "SupplyItems",
                principalColumn: "SupplyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_SupplyItems_SupplyId",
                table: "StockMovements");

            migrationBuilder.DropTable(
                name: "SupplyItems");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_SupplyId",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_RecordStationTasks_StationCode_Status",
                table: "RecordStationTasks");

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 500);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 501);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 51);

            migrationBuilder.DropColumn(
                name: "SupplyId",
                table: "StockMovements");

            migrationBuilder.CreateIndex(
                name: "IX_RecordStationTasks_StationCode",
                table: "RecordStationTasks",
                column: "StationCode");
        }
    }
}
