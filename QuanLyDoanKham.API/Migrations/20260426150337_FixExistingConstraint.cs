using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyDoanKham.API.Migrations
{
    /// <inheritdoc />
    public partial class FixExistingConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop FK constraints that cause cascade cycle (Patient -> Contract cycle)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Patients_Contracts_HealthContractId')
                BEGIN
                    ALTER TABLE Patients DROP CONSTRAINT FK_Patients_Contracts_HealthContractId;
                END
            ");

            // Recreate Patients -> Contracts with NO ACTION
            migrationBuilder.Sql(@"
                ALTER TABLE Patients 
                ADD CONSTRAINT FK_Patients_Contracts_HealthContractId 
                FOREIGN KEY (HealthContractId) REFERENCES Contracts(HealthContractId) ON DELETE NO ACTION;
            ");

            // Drop both FK constraints on MedicalGroupPatients
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_MedicalGroupPatients_Patients_PatientId')
                BEGIN
                    ALTER TABLE MedicalGroupPatients DROP CONSTRAINT FK_MedicalGroupPatients_Patients_PatientId;
                END

                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_MedicalGroupPatients_MedicalGroups_GroupId')
                BEGIN
                    ALTER TABLE MedicalGroupPatients DROP CONSTRAINT FK_MedicalGroupPatients_MedicalGroups_GroupId;
                END
            ");

            // Recreate with NO ACTION
            migrationBuilder.Sql(@"
                ALTER TABLE MedicalGroupPatients 
                ADD CONSTRAINT FK_MedicalGroupPatients_MedicalGroups_GroupId 
                FOREIGN KEY (GroupId) REFERENCES MedicalGroups(GroupId) ON DELETE NO ACTION;

                ALTER TABLE MedicalGroupPatients 
                ADD CONSTRAINT FK_MedicalGroupPatients_Patients_PatientId 
                FOREIGN KEY (PatientId) REFERENCES Patients(PatientId) ON DELETE NO ACTION;
            ");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 26, 22, 3, 37, 257, DateTimeKind.Local).AddTicks(9194));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 26, 22, 3, 37, 257, DateTimeKind.Local).AddTicks(9209));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 26, 22, 3, 37, 257, DateTimeKind.Local).AddTicks(9211));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 26, 22, 3, 37, 257, DateTimeKind.Local).AddTicks(9213));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 26, 22, 3, 37, 257, DateTimeKind.Local).AddTicks(9215));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 26, 22, 2, 51, 685, DateTimeKind.Local).AddTicks(3019));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 26, 22, 2, 51, 685, DateTimeKind.Local).AddTicks(3031));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 26, 22, 2, 51, 685, DateTimeKind.Local).AddTicks(3033));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 26, 22, 2, 51, 685, DateTimeKind.Local).AddTicks(3034));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 26, 22, 2, 51, 685, DateTimeKind.Local).AddTicks(3035));
        }
    }
}
