using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyDoanKham.API.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicalAndFinancialModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupplyInventoryVouchers_Users_CreatedByUserId",
                table: "SupplyInventoryVouchers");

            migrationBuilder.DropIndex(
                name: "IX_SupplyInventoryVouchers_CreatedByUserId",
                table: "SupplyInventoryVouchers");

            migrationBuilder.AddColumn<int>(
                name: "ApproverId",
                table: "SupplyInventoryVouchers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AdvancePayment",
                table: "Contracts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercent",
                table: "Contracts",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalSettlementValue",
                table: "Contracts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "ContractPackages",
                columns: table => new
                {
                    PackageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HealthContractId = table.Column<int>(type: "int", nullable: false),
                    PackageName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExpectedQuantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractPackages", x => x.PackageId);
                    table.ForeignKey(
                        name: "FK_ContractPackages_Contracts_HealthContractId",
                        column: x => x.HealthContractId,
                        principalTable: "Contracts",
                        principalColumn: "HealthContractId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamServices",
                columns: table => new
                {
                    ServiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamServices", x => x.ServiceId);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    PatientId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IdentityCard = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.PatientId);
                    table.ForeignKey(
                        name: "FK_Patients_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "CompanyId");
                });

            migrationBuilder.CreateTable(
                name: "MedicalRecords",
                columns: table => new
                {
                    RecordId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: true),
                    ExamDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ConcludingDoctorId = table.Column<int>(type: "int", nullable: true),
                    Conclusion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalRecords", x => x.RecordId);
                    table.ForeignKey(
                        name: "FK_MedicalRecords_MedicalGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "MedicalGroups",
                        principalColumn: "GroupId");
                    table.ForeignKey(
                        name: "FK_MedicalRecords_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicalRecords_Staffs_ConcludingDoctorId",
                        column: x => x.ConcludingDoctorId,
                        principalTable: "Staffs",
                        principalColumn: "StaffId");
                });

            migrationBuilder.CreateTable(
                name: "MedicalRecordServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecordId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    ExecutingStaffId = table.Column<int>(type: "int", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ResultData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalRecordServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalRecordServices_ExamServices_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "ExamServices",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicalRecordServices_MedicalRecords_RecordId",
                        column: x => x.RecordId,
                        principalTable: "MedicalRecords",
                        principalColumn: "RecordId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicalRecordServices_Staffs_ExecutingStaffId",
                        column: x => x.ExecutingStaffId,
                        principalTable: "Staffs",
                        principalColumn: "StaffId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupplyInventoryVouchers_ApproverId",
                table: "SupplyInventoryVouchers",
                column: "ApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractPackages_HealthContractId",
                table: "ContractPackages",
                column: "HealthContractId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_ConcludingDoctorId",
                table: "MedicalRecords",
                column: "ConcludingDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_GroupId",
                table: "MedicalRecords",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_PatientId",
                table: "MedicalRecords",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecordServices_ExecutingStaffId",
                table: "MedicalRecordServices",
                column: "ExecutingStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecordServices_RecordId",
                table: "MedicalRecordServices",
                column: "RecordId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecordServices_ServiceId",
                table: "MedicalRecordServices",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_CompanyId",
                table: "Patients",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupplyInventoryVouchers_Users_ApproverId",
                table: "SupplyInventoryVouchers",
                column: "ApproverId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupplyInventoryVouchers_Users_ApproverId",
                table: "SupplyInventoryVouchers");

            migrationBuilder.DropTable(
                name: "ContractPackages");

            migrationBuilder.DropTable(
                name: "MedicalRecordServices");

            migrationBuilder.DropTable(
                name: "ExamServices");

            migrationBuilder.DropTable(
                name: "MedicalRecords");

            migrationBuilder.DropTable(
                name: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_SupplyInventoryVouchers_ApproverId",
                table: "SupplyInventoryVouchers");

            migrationBuilder.DropColumn(
                name: "ApproverId",
                table: "SupplyInventoryVouchers");

            migrationBuilder.DropColumn(
                name: "AdvancePayment",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "FinalSettlementValue",
                table: "Contracts");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyInventoryVouchers_CreatedByUserId",
                table: "SupplyInventoryVouchers",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupplyInventoryVouchers_Users_CreatedByUserId",
                table: "SupplyInventoryVouchers",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
