using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuanLyDoanKham.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchemaV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamResults");

            migrationBuilder.DropTable(
                name: "GroupSupplyDetails");

            migrationBuilder.DropTable(
                name: "SupplyTransactions");

            migrationBuilder.DropTable(
                name: "Patients");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 8);

            migrationBuilder.DropColumn(
                name: "IsFinished",
                table: "MedicalGroups");

            migrationBuilder.DropColumn(
                name: "IsFinished",
                table: "Contracts");

            migrationBuilder.RenameColumn(
                name: "StockQuantity",
                table: "Supplies",
                newName: "TotalStock");

            migrationBuilder.RenameColumn(
                name: "TotalShifts",
                table: "PayrollRecords",
                newName: "TotalDays");

            migrationBuilder.RenameColumn(
                name: "PatientCount",
                table: "Contracts",
                newName: "ExpectedQuantity");

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "Supplies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImportFilePath",
                table: "MedicalGroups",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "MedicalGroups",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExamDate",
                table: "GroupStaffDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SigningDate",
                table: "Contracts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Contracts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnitName",
                table: "Contracts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "Contracts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "SupplyInventoryVouchers",
                columns: table => new
                {
                    VoucherId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VoucherCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GroupId = table.Column<int>(type: "int", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplyInventoryVouchers", x => x.VoucherId);
                    table.ForeignKey(
                        name: "FK_SupplyInventoryVouchers_MedicalGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "MedicalGroups",
                        principalColumn: "GroupId");
                    table.ForeignKey(
                        name: "FK_SupplyInventoryVouchers_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplyInventoryDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VoucherId = table.Column<int>(type: "int", nullable: false),
                    SupplyId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplyInventoryDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplyInventoryDetails_Supplies_SupplyId",
                        column: x => x.SupplyId,
                        principalTable: "Supplies",
                        principalColumn: "SupplyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupplyInventoryDetails_SupplyInventoryVouchers_VoucherId",
                        column: x => x.VoucherId,
                        principalTable: "SupplyInventoryVouchers",
                        principalColumn: "VoucherId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$Ht6qOTcBcL.WvYvJViJ5nO47sy22Gf0/Jyv7nkyCNwHECLdf6KQ96");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyInventoryDetails_SupplyId",
                table: "SupplyInventoryDetails",
                column: "SupplyId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyInventoryDetails_VoucherId",
                table: "SupplyInventoryDetails",
                column: "VoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyInventoryVouchers_CreatedByUserId",
                table: "SupplyInventoryVouchers",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyInventoryVouchers_GroupId",
                table: "SupplyInventoryVouchers",
                column: "GroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupplyInventoryDetails");

            migrationBuilder.DropTable(
                name: "SupplyInventoryVouchers");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "Supplies");

            migrationBuilder.DropColumn(
                name: "ImportFilePath",
                table: "MedicalGroups");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "MedicalGroups");

            migrationBuilder.DropColumn(
                name: "ExamDate",
                table: "GroupStaffDetails");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "SigningDate",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "UnitName",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "Contracts");

            migrationBuilder.RenameColumn(
                name: "TotalStock",
                table: "Supplies",
                newName: "StockQuantity");

            migrationBuilder.RenameColumn(
                name: "TotalDays",
                table: "PayrollRecords",
                newName: "TotalShifts");

            migrationBuilder.RenameColumn(
                name: "ExpectedQuantity",
                table: "Contracts",
                newName: "PatientCount");

            migrationBuilder.AddColumn<bool>(
                name: "IsFinished",
                table: "MedicalGroups",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFinished",
                table: "Contracts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "GroupSupplyDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    SupplyId = table.Column<int>(type: "int", nullable: false),
                    QuantityUsed = table.Column<int>(type: "int", nullable: false),
                    ReturnQuantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupSupplyDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupSupplyDetails_MedicalGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "MedicalGroups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupSupplyDetails_Supplies_SupplyId",
                        column: x => x.SupplyId,
                        principalTable: "Supplies",
                        principalColumn: "SupplyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    PatientId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HealthContractId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IDCardNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.PatientId);
                    table.ForeignKey(
                        name: "FK_Patients_Contracts_HealthContractId",
                        column: x => x.HealthContractId,
                        principalTable: "Contracts",
                        principalColumn: "HealthContractId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplyTransactions",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProcessedByUserId = table.Column<int>(type: "int", nullable: true),
                    SupplyId = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
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

            migrationBuilder.CreateTable(
                name: "ExamResults",
                columns: table => new
                {
                    ExamResultId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DoctorStaffId = table.Column<int>(type: "int", nullable: true),
                    GroupId = table.Column<int>(type: "int", nullable: true),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    Diagnosis = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExamDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExamType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamResults", x => x.ExamResultId);
                    table.ForeignKey(
                        name: "FK_ExamResults_MedicalGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "MedicalGroups",
                        principalColumn: "GroupId");
                    table.ForeignKey(
                        name: "FK_ExamResults_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamResults_Staffs_DoctorStaffId",
                        column: x => x.DoctorStaffId,
                        principalTable: "Staffs",
                        principalColumn: "StaffId");
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "RoleName" },
                values: new object[,]
                {
                    { 7, "MedicalStaff" },
                    { 8, "Customer" }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$SFddXxxBYTr8Xi.kKAvJle3qThr1SwBElZihGqeWj0jgOkuPX.sIm");

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_DoctorStaffId",
                table: "ExamResults",
                column: "DoctorStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_GroupId",
                table: "ExamResults",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_PatientId",
                table: "ExamResults",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupSupplyDetails_GroupId",
                table: "GroupSupplyDetails",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupSupplyDetails_SupplyId",
                table: "GroupSupplyDetails",
                column: "SupplyId");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_HealthContractId",
                table: "Patients",
                column: "HealthContractId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyTransactions_ProcessedByUserId",
                table: "SupplyTransactions",
                column: "ProcessedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyTransactions_SupplyId",
                table: "SupplyTransactions",
                column: "SupplyId");
        }
    }
}
