using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyDoanKham.API.Migrations
{
    /// <inheritdoc />
    public partial class AddStation_RequiredGender : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequiredGender",
                table: "Stations",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Stations",
                keyColumn: "StationCode",
                keyValue: "CHECKIN",
                column: "RequiredGender",
                value: null);

            migrationBuilder.UpdateData(
                table: "Stations",
                keyColumn: "StationCode",
                keyValue: "LAY_MAU",
                column: "RequiredGender",
                value: null);

            migrationBuilder.UpdateData(
                table: "Stations",
                keyColumn: "StationCode",
                keyValue: "MAT_TAI_MUI_HONG",
                column: "RequiredGender",
                value: null);

            migrationBuilder.UpdateData(
                table: "Stations",
                keyColumn: "StationCode",
                keyValue: "NOI_KHOA",
                column: "RequiredGender",
                value: null);

            migrationBuilder.UpdateData(
                table: "Stations",
                keyColumn: "StationCode",
                keyValue: "QC",
                column: "RequiredGender",
                value: null);

            migrationBuilder.UpdateData(
                table: "Stations",
                keyColumn: "StationCode",
                keyValue: "SIEU_AM",
                column: "RequiredGender",
                value: null);

            migrationBuilder.UpdateData(
                table: "Stations",
                keyColumn: "StationCode",
                keyValue: "SINH_HIEU",
                column: "RequiredGender",
                value: null);

            migrationBuilder.UpdateData(
                table: "Stations",
                keyColumn: "StationCode",
                keyValue: "XQUANG",
                column: "RequiredGender",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiredGender",
                table: "Stations");
        }
    }
}
