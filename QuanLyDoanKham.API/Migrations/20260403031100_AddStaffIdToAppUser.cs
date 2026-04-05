using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyDoanKham.API.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffIdToAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalHistories_Contracts_HealthContractId",
                table: "ApprovalHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalHistories_Users_ApproverId",
                table: "ApprovalHistories");

            migrationBuilder.AddColumn<int>(
                name: "StaffId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "StaffId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Users_StaffId",
                table: "Users",
                column: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalHistories_Contracts_HealthContractId",
                table: "ApprovalHistories",
                column: "HealthContractId",
                principalTable: "Contracts",
                principalColumn: "HealthContractId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalHistories_Users_ApproverId",
                table: "ApprovalHistories",
                column: "ApproverId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Staffs_StaffId",
                table: "Users",
                column: "StaffId",
                principalTable: "Staffs",
                principalColumn: "StaffId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalHistories_Contracts_HealthContractId",
                table: "ApprovalHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalHistories_Users_ApproverId",
                table: "ApprovalHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Staffs_StaffId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_StaffId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StaffId",
                table: "Users");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalHistories_Contracts_HealthContractId",
                table: "ApprovalHistories",
                column: "HealthContractId",
                principalTable: "Contracts",
                principalColumn: "HealthContractId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalHistories_Users_ApproverId",
                table: "ApprovalHistories",
                column: "ApproverId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
