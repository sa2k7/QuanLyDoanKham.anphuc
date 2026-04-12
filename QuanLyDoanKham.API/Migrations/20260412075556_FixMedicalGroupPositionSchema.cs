using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuanLyDoanKham.API.Migrations
{
    /// <inheritdoc />
    public partial class FixMedicalGroupPositionSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MedicalGroupPositions",
                table: "MedicalGroupPositions");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 11);

            migrationBuilder.AlterColumn<int>(
                name: "PositionId",
                table: "MedicalGroupPositions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "MedicalGroupPositions",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MedicalGroupPositions",
                table: "MedicalGroupPositions",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 7,
                columns: new[] { "Description", "RoleName" },
                values: new object[] { "Nhân viên đi đoàn", "MedicalStaff" });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 8,
                columns: new[] { "Description", "RoleName" },
                values: new object[] { "Đại diện doanh nghiệp đối tác", "Customer" });

            migrationBuilder.CreateIndex(
                name: "IX_Positions_Code",
                table: "Positions",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalGroupPositions_PositionId",
                table: "MedicalGroupPositions",
                column: "PositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalGroupPositions_Positions_PositionId",
                table: "MedicalGroupPositions",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "PositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_Stations_Code",
                table: "Positions",
                column: "Code",
                principalTable: "Stations",
                principalColumn: "StationCode",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalGroupPositions_Positions_PositionId",
                table: "MedicalGroupPositions");

            migrationBuilder.DropForeignKey(
                name: "FK_Positions_Stations_Code",
                table: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_Positions_Code",
                table: "Positions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MedicalGroupPositions",
                table: "MedicalGroupPositions");

            migrationBuilder.DropIndex(
                name: "IX_MedicalGroupPositions_PositionId",
                table: "MedicalGroupPositions");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "MedicalGroupPositions");

            migrationBuilder.AlterColumn<int>(
                name: "PositionId",
                table: "MedicalGroupPositions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MedicalGroupPositions",
                table: "MedicalGroupPositions",
                column: "PositionId");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 7,
                columns: new[] { "Description", "RoleName" },
                values: new object[] { "Trưởng đoàn khám", "GroupLeader" });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 8,
                columns: new[] { "Description", "RoleName" },
                values: new object[] { "Nhân viên đi đoàn", "MedicalStaff" });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "Description", "RoleName" },
                values: new object[,]
                {
                    { 9, "Kế toán", "Accountant" },
                    { 10, "Đại diện doanh nghiệp đối tác", "Customer" },
                    { 11, "Quản lý chất lượng (QC/QA)", "QA" }
                });
        }
    }
}
