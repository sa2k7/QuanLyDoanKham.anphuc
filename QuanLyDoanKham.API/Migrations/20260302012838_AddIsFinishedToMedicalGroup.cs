using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuanLyDoanKham.API.Migrations
{
    /// <inheritdoc />
    public partial class AddIsFinishedToMedicalGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "CompanyId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "CompanyId",
                keyValue: 2);

            migrationBuilder.AddColumn<bool>(
                name: "IsFinished",
                table: "MedicalGroups",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$NnTns2sgNNcajrqlsiF9AuaCsRYfL2h70aVN9B8HvynGEQ6HvLeMq");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFinished",
                table: "MedicalGroups");

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "CompanyId", "Address", "CompanyName", "PhoneNumber", "TaxCode" },
                values: new object[,]
                {
                    { 1, "Hà Nội", "Tập đoàn VinGroup", "0243123456", "0101234567" },
                    { 2, "Hồ Chí Minh", "FPT Software", "0283987654", "0307654321" }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$ktWIv/uRrywvbajJXZQWP.vFGGOsHHk/pm2q0vzbv10WHc1o1pxDu");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "AvatarPath", "CompanyId", "FullName", "PasswordHash", "RefreshToken", "RefreshTokenExpiry", "RoleId", "Username" },
                values: new object[,]
                {
                    { 2, null, 1, "Quản lý VinGroup", "$2a$11$byWZG9oNTAJ/kMku9TGOCuBFTShikswfbzYz7yubMi/XtnuUaMCxO", null, null, 3, "vingroup" },
                    { 3, null, 2, "Quản lý FPT", "$2a$11$uf3W.csRz35L/U8DC9K4TuVZRi5nuoAAGTBC72aZxTK39yr8R7lr2", null, null, 3, "fpt" }
                });
        }
    }
}
