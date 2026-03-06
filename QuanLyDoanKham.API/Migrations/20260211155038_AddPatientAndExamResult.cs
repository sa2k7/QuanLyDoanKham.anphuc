using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyDoanKham.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientAndExamResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    PatientId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HealthContractId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IDCardNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                name: "ExamResults",
                columns: table => new
                {
                    ExamResultId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: true),
                    ExamType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Diagnosis = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DoctorStaffId = table.Column<int>(type: "int", nullable: true),
                    ExamDate = table.Column<DateTime>(type: "datetime2", nullable: false)
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

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$TWJ3KsRpcT0mtb8ycfHnPOd9w4um6CvjPA5F0AgJTyJvtQln3Y8hC");

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
                name: "IX_Patients_HealthContractId",
                table: "Patients",
                column: "HealthContractId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamResults");

            migrationBuilder.DropTable(
                name: "Patients");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$OCr5wOZCiamlbHE5KfMXceBBUO7Kte7T1Ua4cWddZNSCxUenwuYm2");
        }
    }
}
