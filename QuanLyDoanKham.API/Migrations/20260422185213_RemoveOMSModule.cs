using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuanLyDoanKham.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOMSModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StationTaskEvents");

            migrationBuilder.DropTable(
                name: "RecordStationTasks");

            migrationBuilder.DropColumn(
                name: "CurrentStation",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "QueueNo",
                table: "MedicalRecords");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentStation",
                table: "MedicalRecords",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QueueNo",
                table: "MedicalRecords",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RecordStationTasks",
                columns: table => new
                {
                    TaskId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssignedStaffId = table.Column<int>(type: "int", nullable: true),
                    MedicalRecordId = table.Column<int>(type: "int", nullable: false),
                    StationCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QueueNo = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    WaitingSince = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecordStationTasks", x => x.TaskId);
                    table.ForeignKey(
                        name: "FK_RecordStationTasks_MedicalRecords_MedicalRecordId",
                        column: x => x.MedicalRecordId,
                        principalTable: "MedicalRecords",
                        principalColumn: "MedicalRecordId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecordStationTasks_Staffs_AssignedStaffId",
                        column: x => x.AssignedStaffId,
                        principalTable: "Staffs",
                        principalColumn: "StaffId");
                    table.ForeignKey(
                        name: "FK_RecordStationTasks_Stations_StationCode",
                        column: x => x.StationCode,
                        principalTable: "Stations",
                        principalColumn: "StationCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StationTaskEvents",
                columns: table => new
                {
                    EventId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActorUserId = table.Column<int>(type: "int", nullable: true),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StationTaskEvents", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_StationTaskEvents_RecordStationTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "RecordStationTasks",
                        principalColumn: "TaskId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StationTaskEvents_Users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecordStationTasks_AssignedStaffId",
                table: "RecordStationTasks",
                column: "AssignedStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_RecordStationTasks_MedicalRecordId",
                table: "RecordStationTasks",
                column: "MedicalRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_RecordStationTasks_StationCode_Status",
                table: "RecordStationTasks",
                columns: new[] { "StationCode", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StationTaskEvents_ActorUserId",
                table: "StationTaskEvents",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StationTaskEvents_TaskId",
                table: "StationTaskEvents",
                column: "TaskId");
        }
    }
}
