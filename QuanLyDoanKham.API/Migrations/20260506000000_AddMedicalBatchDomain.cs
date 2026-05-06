using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyDoanKham.API.Migrations
{
    /// <summary>
    /// Migration: AddMedicalBatchDomain
    ///
    /// SAFE, ADDITIVE-ONLY migration for the health-check-domain-refactor.
    /// Creates 3 new tables and adds RowVersion to Contracts.
    /// DOES NOT drop, rename, or alter any existing table or column.
    /// Idempotent: all statements use IF NOT EXISTS guards.
    ///
    /// New tables:
    ///   - MedicalBatches
    ///   - MedicalBatchRecords  (NOTE: NOT MedicalRecords — avoids collision with existing clinical table)
    ///   - CampaignRoleRequirements
    ///
    /// Additive column:
    ///   - Contracts.RowVersion (nullable rowversion for optimistic concurrency)
    /// </summary>
    public partial class AddMedicalBatchDomain : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ================================================================
            // TABLE: MedicalBatches
            // FK → Contracts(HealthContractId), NO CASCADE (contract must not
            // silently delete batches)
            // ================================================================
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MedicalBatches')
BEGIN
    CREATE TABLE [MedicalBatches] (
        [Id]             UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        [ContractId]     INT              NOT NULL,
        [EstimatedCount] INT              NOT NULL
                             CONSTRAINT CK_MedicalBatches_EstimatedCount
                             CHECK ([EstimatedCount] >= 1 AND [EstimatedCount] <= 10000),
        [CreatedAt]      DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy]      NVARCHAR(100)    NOT NULL DEFAULT '',
        CONSTRAINT PK_MedicalBatches PRIMARY KEY ([Id]),
        CONSTRAINT FK_MedicalBatches_Contracts
            FOREIGN KEY ([ContractId])
            REFERENCES [Contracts]([HealthContractId])
            ON DELETE NO ACTION
    );
END
");

            // ================================================================
            // TABLE: MedicalBatchRecords
            // FK → MedicalBatches(Id), CASCADE DELETE
            // Unique index on (BatchId, Code) — prevents duplicate codes per batch
            // ================================================================
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MedicalBatchRecords')
BEGIN
    CREATE TABLE [MedicalBatchRecords] (
        [Id]        UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        [BatchId]   UNIQUEIDENTIFIER NOT NULL,
        [Code]      NVARCHAR(10)     NOT NULL,
        [Status]    NVARCHAR(20)     NOT NULL DEFAULT 'Pending',
        [CreatedAt] DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT PK_MedicalBatchRecords PRIMARY KEY ([Id]),
        CONSTRAINT FK_MedicalBatchRecords_MedicalBatches
            FOREIGN KEY ([BatchId])
            REFERENCES [MedicalBatches]([Id])
            ON DELETE CASCADE,
        CONSTRAINT UQ_MedicalBatchRecords_BatchId_Code
            UNIQUE ([BatchId], [Code])
    );
END
");

            // Performance index on BatchId (covered by UQ above, but explicit for clarity)
            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_MedicalBatchRecords_BatchId'
      AND object_id = OBJECT_ID('MedicalBatchRecords')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_MedicalBatchRecords_BatchId]
        ON [MedicalBatchRecords] ([BatchId]);
END
");

            // Performance index on Status for progress queries
            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_MedicalBatchRecords_Status'
      AND object_id = OBJECT_ID('MedicalBatchRecords')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_MedicalBatchRecords_Status]
        ON [MedicalBatchRecords] ([Status]);
END
");

            // ================================================================
            // TABLE: CampaignRoleRequirements
            // FK → MedicalGroups(GroupId), CASCADE DELETE
            // Unique constraint on (CampaignId, Role) — one row per role per campaign
            // ================================================================
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CampaignRoleRequirements')
BEGIN
    CREATE TABLE [CampaignRoleRequirements] (
        [Id]            UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        [CampaignId]    INT              NOT NULL,
        [Role]          NVARCHAR(30)     NOT NULL,
        [RequiredCount] INT              NOT NULL
                            CONSTRAINT CK_CampaignRoleRequirements_RequiredCount
                            CHECK ([RequiredCount] >= 1),
        CONSTRAINT PK_CampaignRoleRequirements PRIMARY KEY ([Id]),
        CONSTRAINT FK_CampaignRoleRequirements_MedicalGroups
            FOREIGN KEY ([CampaignId])
            REFERENCES [MedicalGroups]([GroupId])
            ON DELETE CASCADE,
        CONSTRAINT UQ_CampaignRoleRequirements_CampaignId_Role
            UNIQUE ([CampaignId], [Role])
    );
END
");

            // Performance index on CampaignId for role requirement lookups
            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_CampaignRoleRequirements_CampaignId'
      AND object_id = OBJECT_ID('CampaignRoleRequirements')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_CampaignRoleRequirements_CampaignId]
        ON [CampaignRoleRequirements] ([CampaignId]);
END
");

            // ================================================================
            // ADDITIVE COLUMN: Contracts.RowVersion
            // Nullable rowversion for optimistic concurrency on contract approval.
            // Existing rows get NULL — no data loss.
            // ================================================================
            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE name = 'RowVersion'
      AND object_id = OBJECT_ID('Contracts')
)
BEGIN
    ALTER TABLE [Contracts]
        ADD [RowVersion] ROWVERSION NULL;
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove RowVersion column from Contracts (safe — nullable, no data dependency)
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE name = 'RowVersion'
      AND object_id = OBJECT_ID('Contracts')
)
BEGIN
    ALTER TABLE [Contracts] DROP COLUMN [RowVersion];
END
");

            // Drop new tables in reverse FK dependency order
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CampaignRoleRequirements')
    DROP TABLE [CampaignRoleRequirements];
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MedicalBatchRecords')
    DROP TABLE [MedicalBatchRecords];
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MedicalBatches')
    DROP TABLE [MedicalBatches];
");
        }
    }
}
