using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuanLyDoanKham.API.Migrations
{
    /// <inheritdoc />
    public partial class _20260403_permission_polp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupStaffDetails_MedicalGroupPositions_PositionId",
                table: "GroupStaffDetails");

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 19);

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
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.AlterColumn<int>(
                name: "RoleId",
                table: "UserRoles",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedAt",
                table: "GroupStaffDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssignedByUserId",
                table: "GroupStaffDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GroupPositionQuotaId",
                table: "GroupStaffDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SignerId",
                table: "Contracts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "Contracts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ApprovalSteps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HealthContractId = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    RequiredPermission = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsFinal = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalSteps_Contracts_HealthContractId",
                        column: x => x.HealthContractId,
                        principalTable: "Contracts",
                        principalColumn: "HealthContractId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_ApprovalSteps_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContractFinancialSummaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HealthContractId = table.Column<int>(type: "int", nullable: false),
                    StaffCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SupplyCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Revenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SnapshotAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractFinancialSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractFinancialSummaries_Contracts_HealthContractId",
                        column: x => x.HealthContractId,
                        principalTable: "Contracts",
                        principalColumn: "HealthContractId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    PositionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SpecialtyRequired = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.PositionId);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApprovalStepId = table.Column<int>(type: "int", nullable: false),
                    HealthContractId = table.Column<int>(type: "int", nullable: false),
                    ApproverId = table.Column<int>(type: "int", nullable: false),
                    Decision = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalHistories_ApprovalSteps_ApprovalStepId",
                        column: x => x.ApprovalStepId,
                        principalTable: "ApprovalSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_ApprovalHistories_Contracts_HealthContractId",
                        column: x => x.HealthContractId,
                        principalTable: "Contracts",
                        principalColumn: "HealthContractId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_ApprovalHistories_Users_ApproverId",
                        column: x => x.ApproverId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroupPositionQuotas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicalGroupId = table.Column<int>(type: "int", nullable: false),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    Required = table.Column<int>(type: "int", nullable: false),
                    Assigned = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupPositionQuotas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupPositionQuotas_MedicalGroups_MedicalGroupId",
                        column: x => x.MedicalGroupId,
                        principalTable: "MedicalGroups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_GroupPositionQuotas_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "PositionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 2,
                column: "PermissionId",
                value: 10);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 3,
                column: "PermissionId",
                value: 21);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 4,
                column: "PermissionId",
                value: 32);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 5,
                column: "PermissionId",
                value: 40);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 6,
                column: "PermissionId",
                value: 50);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 7,
                column: "PermissionId",
                value: 60);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 8,
                column: "PermissionId",
                value: 70);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 9,
                column: "PermissionId",
                value: 80);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 10,
                column: "PermissionId",
                value: 81);

            migrationBuilder.CreateIndex(
                name: "IX_GroupStaffDetails_GroupPositionQuotaId",
                table: "GroupStaffDetails",
                column: "GroupPositionQuotaId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_SignerId",
                table: "Contracts",
                column: "SignerId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_UpdatedByUserId",
                table: "Contracts",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalHistories_ApprovalStepId",
                table: "ApprovalHistories",
                column: "ApprovalStepId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalHistories_ApproverId",
                table: "ApprovalHistories",
                column: "ApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalHistories_HealthContractId",
                table: "ApprovalHistories",
                column: "HealthContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalSteps_HealthContractId",
                table: "ApprovalSteps",
                column: "HealthContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalSteps_RoleId",
                table: "ApprovalSteps",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractFinancialSummaries_HealthContractId",
                table: "ContractFinancialSummaries",
                column: "HealthContractId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupPositionQuotas_MedicalGroupId",
                table: "GroupPositionQuotas",
                column: "MedicalGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupPositionQuotas_PositionId",
                table: "GroupPositionQuotas",
                column: "PositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Users_SignerId",
                table: "Contracts",
                column: "SignerId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Users_UpdatedByUserId",
                table: "Contracts",
                column: "UpdatedByUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupStaffDetails_GroupPositionQuotas_GroupPositionQuotaId",
                table: "GroupStaffDetails",
                column: "GroupPositionQuotaId",
                principalTable: "GroupPositionQuotas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupStaffDetails_Positions_PositionId",
                table: "GroupStaffDetails",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "PositionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Users_SignerId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Users_UpdatedByUserId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupStaffDetails_GroupPositionQuotas_GroupPositionQuotaId",
                table: "GroupStaffDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupStaffDetails_Positions_PositionId",
                table: "GroupStaffDetails");

            migrationBuilder.DropTable(
                name: "ApprovalHistories");

            migrationBuilder.DropTable(
                name: "ContractFinancialSummaries");

            migrationBuilder.DropTable(
                name: "GroupPositionQuotas");

            migrationBuilder.DropTable(
                name: "ApprovalSteps");

            migrationBuilder.DropTable(
                name: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_GroupStaffDetails_GroupPositionQuotaId",
                table: "GroupStaffDetails");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_SignerId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_UpdatedByUserId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "AssignedAt",
                table: "GroupStaffDetails");

            migrationBuilder.DropColumn(
                name: "AssignedByUserId",
                table: "GroupStaffDetails");

            migrationBuilder.DropColumn(
                name: "GroupPositionQuotaId",
                table: "GroupStaffDetails");

            migrationBuilder.DropColumn(
                name: "SignerId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Contracts");

            migrationBuilder.AlterColumn<int>(
                name: "RoleId",
                table: "UserRoles",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 2,
                column: "PermissionId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 3,
                column: "PermissionId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 4,
                column: "PermissionId",
                value: 4);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 5,
                column: "PermissionId",
                value: 5);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 6,
                column: "PermissionId",
                value: 6);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 7,
                column: "PermissionId",
                value: 10);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 8,
                column: "PermissionId",
                value: 11);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 9,
                column: "PermissionId",
                value: 12);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 10,
                column: "PermissionId",
                value: 13);

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 11, 14, 1 },
                    { 12, 15, 1 },
                    { 13, 20, 1 },
                    { 14, 21, 1 },
                    { 15, 30, 1 },
                    { 16, 31, 1 },
                    { 17, 32, 1 },
                    { 18, 40, 1 },
                    { 19, 41, 1 },
                    { 20, 42, 1 },
                    { 21, 50, 1 },
                    { 22, 51, 1 },
                    { 23, 60, 1 },
                    { 24, 61, 1 },
                    { 25, 70, 1 },
                    { 26, 71, 1 },
                    { 27, 80, 1 },
                    { 28, 81, 1 }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_GroupStaffDetails_MedicalGroupPositions_PositionId",
                table: "GroupStaffDetails",
                column: "PositionId",
                principalTable: "MedicalGroupPositions",
                principalColumn: "PositionId");
        }
    }
}
