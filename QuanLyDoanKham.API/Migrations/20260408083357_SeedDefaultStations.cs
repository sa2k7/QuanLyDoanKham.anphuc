using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuanLyDoanKham.API.Migrations
{
    /// <inheritdoc />
    public partial class SeedDefaultStations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Stations",
                columns: new[] { "StationCode", "DefaultMinutesPerPatient", "IsActive", "ServiceType", "SortOrder", "StationName", "WipLimit" },
                values: new object[,]
                {
                    { "CHECKIN", 5m, true, "ADMIN", 1, "Tiếp đón & Cấp số", 1 },
                    { "LAY_MAU", 5m, true, "LAB", 3, "Lấy mẫu xét nghiệm", 1 },
                    { "MAT_TAI_MUI_HONG", 5m, true, "CLINICAL", 7, "Mắt - Tai Mũi Họng", 1 },
                    { "NOI_KHOA", 5m, true, "CLINICAL", 6, "Khám Nội khoa", 1 },
                    { "QC", 5m, true, "ADMIN", 99, "Kiểm tra hồ sơ (QC)", 1 },
                    { "SIEU_AM", 5m, true, "IMAGING", 5, "Siêu âm", 1 },
                    { "SINH_HIEU", 5m, true, "CLINICAL", 2, "Đo sinh hiệu", 1 },
                    { "XQUANG", 5m, true, "IMAGING", 4, "Chụp X-Quang", 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Stations",
                keyColumn: "StationCode",
                keyValue: "CHECKIN");

            migrationBuilder.DeleteData(
                table: "Stations",
                keyColumn: "StationCode",
                keyValue: "LAY_MAU");

            migrationBuilder.DeleteData(
                table: "Stations",
                keyColumn: "StationCode",
                keyValue: "MAT_TAI_MUI_HONG");

            migrationBuilder.DeleteData(
                table: "Stations",
                keyColumn: "StationCode",
                keyValue: "NOI_KHOA");

            migrationBuilder.DeleteData(
                table: "Stations",
                keyColumn: "StationCode",
                keyValue: "QC");

            migrationBuilder.DeleteData(
                table: "Stations",
                keyColumn: "StationCode",
                keyValue: "SIEU_AM");

            migrationBuilder.DeleteData(
                table: "Stations",
                keyColumn: "StationCode",
                keyValue: "SINH_HIEU");

            migrationBuilder.DeleteData(
                table: "Stations",
                keyColumn: "StationCode",
                keyValue: "XQUANG");
        }
    }
}
