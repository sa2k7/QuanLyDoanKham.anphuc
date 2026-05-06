using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyDoanKham.API.Migrations
{
    /// <summary>
    /// Migration: AddBatchMedicalRecord
    ///
    /// SAFE, ADDITIVE-ONLY migration.
    /// Creates the BatchMedicalRecords bridge table that links
    /// Patient (Bệnh Án) ↔ MedicalBatch.
    ///
    /// Architecture:
    ///   Patient (1) ──── (N) BatchMedicalRecord (N) ──── (1) MedicalBatch
    ///
    /// Rules enforced at DB level:
    ///   - UQ_BatchMedicalRecord_BatchId_PatientId: prevents duplicate assignment
    ///   - FK → Patients(PatientId) CASCADE DELETE
    ///   - FK → MedicalBatches(Id) CASCADE DELETE
    ///
    /// DOES NOT drop, rename, or alter any existing table or column.
    /// </summary>
    public partial class AddBatchMedicalRecord : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ================================================================
            // TABLE: BatchMedicalRecords
            // Bridge: Patient (Bệnh Án) ↔ MedicalBatch
            // ================================================================
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'BatchMedicalRecords')
BEGIN
    CREATE TABLE [BatchMedicalRecords] (
        [Id]                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        [MedicalBatchId]    UNIQUEIDENTIFIER NOT NULL,
        [PatientId]         INT              NOT NULL,
        [Status]            NVARCHAR(20)     NOT NULL DEFAULT 'Waiting'
                                CONSTRAINT CK_BatchMedicalRecords_Status
                                CHECK ([Status] IN ('Waiting', 'InProgress', 'Done')),
        [CreatedAt]         DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        [AssignedByUserId]  INT              NULL,
        [Note]              NVARCHAR(200)    NULL,
        CONSTRAINT PK_BatchMedicalRecords PRIMARY KEY ([Id]),
        CONSTRAINT FK_BatchMedicalRecords_MedicalBatches
            FOREIGN KEY ([MedicalBatchId])
            REFERENCES [MedicalBatches]([Id])
            ON DELETE CASCADE,
        CONSTRAINT FK_BatchMedicalRecords_Patients
            FOREIGN KEY ([PatientId])
            REFERENCES [Patients]([PatientId])
            ON DELETE CASCADE,
        -- Prevent duplicate assignment of same patient to same batch
        CONSTRAINT UQ_BatchMedicalRecord_BatchId_PatientId
            UNIQUE ([MedicalBatchId], [PatientId])
    );
END
");

            // Index: PatientId — for "get all batches for a patient"
            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_BatchMedicalRecord_PatientId'
      AND object_id = OBJECT_ID('BatchMedicalRecords')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_BatchMedicalRecord_PatientId]
        ON [BatchMedicalRecords] ([PatientId]);
END
");

            // Index: Status — for progress queries (count Waiting/InProgress/Done per batch)
            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_BatchMedicalRecord_Status'
      AND object_id = OBJECT_ID('BatchMedicalRecords')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_BatchMedicalRecord_Status]
        ON [BatchMedicalRecords] ([Status])
        INCLUDE ([MedicalBatchId], [PatientId]);
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'BatchMedicalRecords')
    DROP TABLE [BatchMedicalRecords];
");
        }
    }
}
