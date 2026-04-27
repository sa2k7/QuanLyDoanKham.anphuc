CREATE TABLE [Companies] (
    [CompanyId] int NOT NULL IDENTITY,
    [ShortName] nvarchar(100) NULL,
    [CompanyName] nvarchar(200) NOT NULL,
    [TaxCode] nvarchar(50) NULL,
    [Address] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [Email] nvarchar(max) NULL,
    [ContactPerson] nvarchar(200) NULL,
    [ContactPhone] nvarchar(15) NULL,
    [CompanySize] int NULL,
    [Industry] nvarchar(100) NULL,
    [Notes] nvarchar(max) NULL,
    CONSTRAINT [PK_Companies] PRIMARY KEY ([CompanyId])
);
GO


CREATE TABLE [ContractApprovalSteps] (
    [StepId] int NOT NULL IDENTITY,
    [StepOrder] int NOT NULL,
    [StepName] nvarchar(100) NOT NULL,
    [RequiredPermission] nvarchar(100) NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_ContractApprovalSteps] PRIMARY KEY ([StepId])
);
GO


CREATE TABLE [PasswordResetRequests] (
    [Id] int NOT NULL IDENTITY,
    [Username] nvarchar(50) NOT NULL,
    [RequestedDate] datetime2 NOT NULL,
    [IsProcessed] bit NOT NULL,
    [NewPassword] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_PasswordResetRequests] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Permissions] (
    [PermissionId] int NOT NULL IDENTITY,
    [PermissionKey] nvarchar(100) NOT NULL,
    [PermissionName] nvarchar(200) NOT NULL,
    [Module] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Permissions] PRIMARY KEY ([PermissionId])
);
GO


CREATE TABLE [Roles] (
    [RoleId] int NOT NULL IDENTITY,
    [RoleName] nvarchar(50) NOT NULL,
    [Description] nvarchar(200) NOT NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY ([RoleId])
);
GO


CREATE TABLE [Staffs] (
    [StaffId] int NOT NULL IDENTITY,
    [EmployeeCode] nvarchar(20) NULL,
    [FullName] nvarchar(100) NOT NULL,
    [FullNameUnsigned] nvarchar(100) NULL,
    [BirthYear] int NULL,
    [Gender] nvarchar(10) NULL,
    [IDCardNumber] nvarchar(20) NULL,
    [TaxCode] nvarchar(20) NULL,
    [BankAccountNumber] nvarchar(50) NULL,
    [BankAccountName] nvarchar(100) NULL,
    [BankName] nvarchar(100) NULL,
    [PhoneNumber] nvarchar(15) NULL,
    [Email] nvarchar(100) NULL,
    [JobTitle] nvarchar(100) NULL,
    [StaffType] nvarchar(50) NULL,
    [Specialty] nvarchar(100) NULL,
    [DepartmentName] nvarchar(100) NULL,
    [CertificateNumber] nvarchar(50) NULL,
    [EmployeeType] nvarchar(20) NULL,
    [DailyRate] decimal(18,2) NOT NULL,
    [BaseSalary] decimal(18,2) NOT NULL,
    [StandardWorkDays] int NOT NULL,
    [SalaryType] nvarchar(20) NOT NULL,
    [IDCardFrontPath] nvarchar(max) NULL,
    [IDCardBackPath] nvarchar(max) NULL,
    [PracticeCertificatePath] nvarchar(max) NULL,
    [AvatarPath] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NULL,
    CONSTRAINT [PK_Staffs] PRIMARY KEY ([StaffId])
);
GO


CREATE TABLE [Stations] (
    [StationCode] nvarchar(50) NOT NULL,
    [StationName] nvarchar(100) NOT NULL,
    [ServiceType] nvarchar(50) NOT NULL,
    [DefaultMinutesPerPatient] decimal(18,2) NOT NULL,
    [WipLimit] int NOT NULL,
    [SortOrder] int NOT NULL,
    [RequiredGender] nvarchar(10) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Stations] PRIMARY KEY ([StationCode])
);
GO


CREATE TABLE [SupplyItems] (
    [SupplyId] int NOT NULL IDENTITY,
    [ItemName] nvarchar(200) NOT NULL,
    [Unit] nvarchar(50) NOT NULL,
    [Category] nvarchar(100) NOT NULL,
    [CurrentStock] int NOT NULL,
    [MinStockLevel] int NOT NULL,
    [TypicalUnitPrice] decimal(18,2) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_SupplyItems] PRIMARY KEY ([SupplyId])
);
GO


CREATE TABLE [RolePermissions] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] int NOT NULL,
    [PermissionId] int NOT NULL,
    CONSTRAINT [PK_RolePermissions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RolePermissions_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [Permissions] ([PermissionId]) ON DELETE CASCADE,
    CONSTRAINT [FK_RolePermissions_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([RoleId]) ON DELETE CASCADE
);
GO


CREATE TABLE [Users] (
    [UserId] int NOT NULL IDENTITY,
    [Username] nvarchar(50) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [FullName] nvarchar(100) NULL,
    [RoleId] int NOT NULL,
    [CompanyId] int NULL,
    [StaffId] int NULL,
    [RefreshToken] nvarchar(max) NULL,
    [RefreshTokenExpiry] datetime2 NULL,
    [Email] nvarchar(200) NULL,
    [AvatarPath] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([UserId]),
    CONSTRAINT [FK_Users_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]) ON DELETE SET NULL,
    CONSTRAINT [FK_Users_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([RoleId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Users_Staffs_StaffId] FOREIGN KEY ([StaffId]) REFERENCES [Staffs] ([StaffId])
);
GO


CREATE TABLE [Positions] (
    [PositionId] int NOT NULL IDENTITY,
    [Code] nvarchar(50) NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [SpecialtyRequired] nvarchar(100) NOT NULL,
    [Description] nvarchar(200) NOT NULL,
    CONSTRAINT [PK_Positions] PRIMARY KEY ([PositionId]),
    CONSTRAINT [FK_Positions_Stations_Code] FOREIGN KEY ([Code]) REFERENCES [Stations] ([StationCode]) ON DELETE CASCADE
);
GO


CREATE TABLE [AuditLogs] (
    [LogId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [Action] nvarchar(50) NOT NULL,
    [EntityType] nvarchar(100) NOT NULL,
    [EntityId] int NOT NULL,
    [OldValue] nvarchar(max) NOT NULL,
    [NewValue] nvarchar(max) NOT NULL,
    [Timestamp] datetime2 NOT NULL,
    [IPAddress] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([LogId]),
    CONSTRAINT [FK_AuditLogs_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);
GO


CREATE TABLE [Contracts] (
    [HealthContractId] int NOT NULL IDENTITY,
    [CompanyId] int NOT NULL,
    [ContractName] nvarchar(200) NOT NULL,
    [ContractCode] nvarchar(50) NULL,
    [SigningDate] datetime2 NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [ExpectedQuantity] int NOT NULL,
    [AdvancePayment] decimal(18,2) NOT NULL,
    [FinalSettlementValue] decimal(18,2) NOT NULL,
    [DiscountPercent] decimal(5,2) NOT NULL,
    [UnitName] nvarchar(50) NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [ExtraServiceRevenue] decimal(18,2) NOT NULL,
    [ExtraServiceDetails] nvarchar(max) NULL,
    [VATRate] decimal(5,2) NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [CurrentApprovalStep] int NOT NULL,
    [FilePath] nvarchar(max) NULL,
    [CreatedByUserId] int NULL,
    [UpdatedByUserId] int NULL,
    [SignerId] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(100) NULL,
    [Notes] nvarchar(max) NULL,
    CONSTRAINT [PK_Contracts] PRIMARY KEY ([HealthContractId]),
    CONSTRAINT [FK_Contracts_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Contracts_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([UserId]) ON DELETE SET NULL,
    CONSTRAINT [FK_Contracts_Users_SignerId] FOREIGN KEY ([SignerId]) REFERENCES [Users] ([UserId]),
    CONSTRAINT [FK_Contracts_Users_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [Users] ([UserId])
);
GO


CREATE TABLE [Notifications] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NULL,
    [Title] nvarchar(max) NOT NULL,
    [Message] nvarchar(max) NOT NULL,
    [Type] nvarchar(50) NOT NULL,
    [IsRead] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [Link] nvarchar(max) NULL,
    [ActionUrl] nvarchar(max) NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Notifications_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId])
);
GO


CREATE TABLE [UserRoles] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [RoleId] int NULL,
    [AssignedAt] datetime2 NOT NULL,
    [AssignedBy] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_UserRoles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([RoleId]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);
GO


CREATE TABLE [ApprovalSteps] (
    [Id] int NOT NULL IDENTITY,
    [HealthContractId] int NOT NULL,
    [Order] int NOT NULL,
    [RoleId] int NOT NULL,
    [RequiredPermission] nvarchar(100) NOT NULL,
    [IsFinal] bit NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_ApprovalSteps] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ApprovalSteps_Contracts_HealthContractId] FOREIGN KEY ([HealthContractId]) REFERENCES [Contracts] ([HealthContractId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ApprovalSteps_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([RoleId]) ON DELETE NO ACTION
);
GO


CREATE TABLE [ContractApprovalHistories] (
    [Id] int NOT NULL IDENTITY,
    [HealthContractId] int NOT NULL,
    [StepOrder] int NOT NULL,
    [StepName] nvarchar(100) NOT NULL,
    [Action] nvarchar(20) NOT NULL,
    [Note] nvarchar(max) NOT NULL,
    [ApprovedByUserId] int NOT NULL,
    [ActionDate] datetime2 NOT NULL,
    CONSTRAINT [PK_ContractApprovalHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ContractApprovalHistories_Contracts_HealthContractId] FOREIGN KEY ([HealthContractId]) REFERENCES [Contracts] ([HealthContractId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ContractApprovalHistories_Users_ApprovedByUserId] FOREIGN KEY ([ApprovedByUserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);
GO


CREATE TABLE [ContractAttachments] (
    [Id] int NOT NULL IDENTITY,
    [HealthContractId] int NOT NULL,
    [FileName] nvarchar(200) NOT NULL,
    [FilePath] nvarchar(max) NOT NULL,
    [FileType] nvarchar(50) NOT NULL,
    [UploadedAt] datetime2 NOT NULL,
    [UploadedBy] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_ContractAttachments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ContractAttachments_Contracts_HealthContractId] FOREIGN KEY ([HealthContractId]) REFERENCES [Contracts] ([HealthContractId]) ON DELETE CASCADE
);
GO


CREATE TABLE [ContractFinancialSummaries] (
    [Id] int NOT NULL IDENTITY,
    [HealthContractId] int NOT NULL,
    [StaffCost] decimal(18,2) NOT NULL,
    [SupplyCost] decimal(18,2) NOT NULL,
    [OtherCost] decimal(18,2) NOT NULL,
    [Revenue] decimal(18,2) NOT NULL,
    [SnapshotAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ContractFinancialSummaries] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ContractFinancialSummaries_Contracts_HealthContractId] FOREIGN KEY ([HealthContractId]) REFERENCES [Contracts] ([HealthContractId]) ON DELETE CASCADE
);
GO


CREATE TABLE [ContractRevenueSummaries] (
    [SummaryId] int NOT NULL IDENTITY,
    [HealthContractId] int NOT NULL,
    [TotalContractValue] decimal(18,2) NOT NULL,
    [TotalGroupCosts] decimal(18,2) NOT NULL,
    [Revenue] decimal(18,2) NOT NULL,
    [TotalGroups] int NOT NULL,
    [GeneratedAt] datetime2 NOT NULL,
    [GeneratedBy] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_ContractRevenueSummaries] PRIMARY KEY ([SummaryId]),
    CONSTRAINT [FK_ContractRevenueSummaries_Contracts_HealthContractId] FOREIGN KEY ([HealthContractId]) REFERENCES [Contracts] ([HealthContractId]) ON DELETE CASCADE
);
GO


CREATE TABLE [ContractStatusHistories] (
    [Id] int NOT NULL IDENTITY,
    [HealthContractId] int NOT NULL,
    [OldStatus] nvarchar(50) NOT NULL,
    [NewStatus] nvarchar(50) NOT NULL,
    [Note] nvarchar(max) NOT NULL,
    [ChangedAt] datetime2 NOT NULL,
    [ChangedBy] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_ContractStatusHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ContractStatusHistories_Contracts_HealthContractId] FOREIGN KEY ([HealthContractId]) REFERENCES [Contracts] ([HealthContractId]) ON DELETE CASCADE
);
GO


CREATE TABLE [CostSnapshots] (
    [SnapshotId] int NOT NULL IDENTITY,
    [HealthContractId] int NOT NULL,
    [SnapshotType] nvarchar(20) NOT NULL,
    [Revenue] decimal(18,2) NOT NULL,
    [LaborCost] decimal(18,2) NOT NULL,
    [SupplyCost] decimal(18,2) NOT NULL,
    [OverheadCost] decimal(18,2) NOT NULL,
    [TotalCost] decimal(18,2) NOT NULL,
    [GrossProfit] decimal(18,2) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedByUserId] int NULL,
    [Note] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_CostSnapshots] PRIMARY KEY ([SnapshotId]),
    CONSTRAINT [FK_CostSnapshots_Contracts_HealthContractId] FOREIGN KEY ([HealthContractId]) REFERENCES [Contracts] ([HealthContractId]) ON DELETE CASCADE,
    CONSTRAINT [FK_CostSnapshots_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([UserId]) ON DELETE SET NULL
);
GO


CREATE TABLE [MedicalGroups] (
    [GroupId] int NOT NULL IDENTITY,
    [GroupName] nvarchar(200) NOT NULL,
    [ExamDate] datetime2 NOT NULL,
    [Slot] nvarchar(20) NOT NULL,
    [TeamCode] nvarchar(50) NULL,
    [HealthContractId] int NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [StartTime] nvarchar(50) NULL,
    [DepartureTime] nvarchar(50) NULL,
    [ExamContent] nvarchar(500) NULL,
    [ImportFilePath] nvarchar(max) NULL,
    [ManagerUserId] int NULL,
    [GroupLeaderStaffId] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(100) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(100) NULL,
    CONSTRAINT [PK_MedicalGroups] PRIMARY KEY ([GroupId]),
    CONSTRAINT [FK_MedicalGroups_Contracts_HealthContractId] FOREIGN KEY ([HealthContractId]) REFERENCES [Contracts] ([HealthContractId]) ON DELETE CASCADE,
    CONSTRAINT [FK_MedicalGroups_Staffs_GroupLeaderStaffId] FOREIGN KEY ([GroupLeaderStaffId]) REFERENCES [Staffs] ([StaffId]) ON DELETE SET NULL,
    CONSTRAINT [FK_MedicalGroups_Users_ManagerUserId] FOREIGN KEY ([ManagerUserId]) REFERENCES [Users] ([UserId]) ON DELETE SET NULL
);
GO


CREATE TABLE [Overheads] (
    [OverheadId] int NOT NULL IDENTITY,
    [HealthContractId] int NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [IncurredAt] datetime2 NOT NULL,
    [RecordedByUserId] int NULL,
    CONSTRAINT [PK_Overheads] PRIMARY KEY ([OverheadId]),
    CONSTRAINT [FK_Overheads_Contracts_HealthContractId] FOREIGN KEY ([HealthContractId]) REFERENCES [Contracts] ([HealthContractId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Overheads_Users_RecordedByUserId] FOREIGN KEY ([RecordedByUserId]) REFERENCES [Users] ([UserId]) ON DELETE SET NULL
);
GO


CREATE TABLE [ApprovalHistories] (
    [Id] int NOT NULL IDENTITY,
    [ApprovalStepId] int NOT NULL,
    [HealthContractId] int NOT NULL,
    [ApproverId] int NOT NULL,
    [Decision] nvarchar(20) NOT NULL,
    [Note] nvarchar(max) NOT NULL,
    [ApprovedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ApprovalHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ApprovalHistories_ApprovalSteps_ApprovalStepId] FOREIGN KEY ([ApprovalStepId]) REFERENCES [ApprovalSteps] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ApprovalHistories_Contracts_HealthContractId] FOREIGN KEY ([HealthContractId]) REFERENCES [Contracts] ([HealthContractId]),
    CONSTRAINT [FK_ApprovalHistories_Users_ApproverId] FOREIGN KEY ([ApproverId]) REFERENCES [Users] ([UserId])
);
GO


CREATE TABLE [GroupCosts] (
    [CostId] int NOT NULL IDENTITY,
    [GroupId] int NOT NULL,
    [StaffCost] decimal(18,2) NOT NULL,
    [SupplyCost] decimal(18,2) NOT NULL,
    [OtherCost] decimal(18,2) NOT NULL,
    [TotalCost] decimal(18,2) NOT NULL,
    [Note] nvarchar(max) NOT NULL,
    [CalculatedAt] datetime2 NOT NULL,
    [CalculatedByUserId] int NULL,
    CONSTRAINT [PK_GroupCosts] PRIMARY KEY ([CostId]),
    CONSTRAINT [FK_GroupCosts_MedicalGroups_GroupId] FOREIGN KEY ([GroupId]) REFERENCES [MedicalGroups] ([GroupId]) ON DELETE CASCADE,
    CONSTRAINT [FK_GroupCosts_Users_CalculatedByUserId] FOREIGN KEY ([CalculatedByUserId]) REFERENCES [Users] ([UserId]) ON DELETE SET NULL
);
GO


CREATE TABLE [GroupPositionQuotas] (
    [Id] int NOT NULL IDENTITY,
    [MedicalGroupId] int NOT NULL,
    [PositionId] int NOT NULL,
    [Required] int NOT NULL,
    [Assigned] int NOT NULL,
    CONSTRAINT [PK_GroupPositionQuotas] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GroupPositionQuotas_MedicalGroups_MedicalGroupId] FOREIGN KEY ([MedicalGroupId]) REFERENCES [MedicalGroups] ([GroupId]) ON DELETE CASCADE,
    CONSTRAINT [FK_GroupPositionQuotas_Positions_PositionId] FOREIGN KEY ([PositionId]) REFERENCES [Positions] ([PositionId]) ON DELETE NO ACTION
);
GO


CREATE TABLE [MedicalGroupPositions] (
    [Id] int NOT NULL IDENTITY,
    [PositionId] int NULL,
    [GroupId] int NOT NULL,
    [PositionName] nvarchar(100) NOT NULL,
    [RequiredCount] int NOT NULL,
    [AssignedCount] int NOT NULL,
    [Description] nvarchar(200) NOT NULL,
    [SortOrder] int NOT NULL,
    CONSTRAINT [PK_MedicalGroupPositions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MedicalGroupPositions_MedicalGroups_GroupId] FOREIGN KEY ([GroupId]) REFERENCES [MedicalGroups] ([GroupId]) ON DELETE CASCADE,
    CONSTRAINT [FK_MedicalGroupPositions_Positions_PositionId] FOREIGN KEY ([PositionId]) REFERENCES [Positions] ([PositionId])
);
GO


CREATE TABLE [Patients] (
    [PatientId] int NOT NULL IDENTITY,
    [HealthContractId] int NOT NULL,
    [MedicalGroupId] int NULL,
    [FullName] nvarchar(100) NOT NULL,
    [DateOfBirth] datetime2 NOT NULL,
    [Gender] nvarchar(10) NULL,
    [IDCardNumber] nvarchar(20) NULL,
    [PhoneNumber] nvarchar(15) NULL,
    [Department] nvarchar(100) NULL,
    [Source] nvarchar(50) NULL,
    [Notes] nvarchar(max) NULL,
    [CreatedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_Patients] PRIMARY KEY ([PatientId]),
    CONSTRAINT [FK_Patients_Contracts_HealthContractId] FOREIGN KEY ([HealthContractId]) REFERENCES [Contracts] ([HealthContractId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Patients_MedicalGroups_MedicalGroupId] FOREIGN KEY ([MedicalGroupId]) REFERENCES [MedicalGroups] ([GroupId])
);
GO


CREATE TABLE [ScheduleCalendars] (
    [CalendarId] int NOT NULL IDENTITY,
    [GroupId] int NOT NULL,
    [ExamDate] datetime2 NOT NULL,
    [StaffId] int NULL,
    [CheckInTime] datetime2 NULL,
    [CheckOutTime] datetime2 NULL,
    [Note] nvarchar(200) NOT NULL,
    [IsConfirmed] bit NOT NULL,
    CONSTRAINT [PK_ScheduleCalendars] PRIMARY KEY ([CalendarId]),
    CONSTRAINT [FK_ScheduleCalendars_MedicalGroups_GroupId] FOREIGN KEY ([GroupId]) REFERENCES [MedicalGroups] ([GroupId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ScheduleCalendars_Staffs_StaffId] FOREIGN KEY ([StaffId]) REFERENCES [Staffs] ([StaffId]) ON DELETE SET NULL
);
GO


CREATE TABLE [StockMovements] (
    [MovementId] int NOT NULL IDENTITY,
    [MedicalGroupId] int NULL,
    [SupplyId] int NULL,
    [ItemName] nvarchar(100) NOT NULL,
    [Unit] nvarchar(50) NOT NULL,
    [Quantity] int NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [TotalValue] decimal(18,2) NOT NULL,
    [MovementType] nvarchar(20) NOT NULL,
    [MovementDate] datetime2 NOT NULL,
    [RecordedByUserId] int NULL,
    [Note] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_StockMovements] PRIMARY KEY ([MovementId]),
    CONSTRAINT [FK_StockMovements_MedicalGroups_MedicalGroupId] FOREIGN KEY ([MedicalGroupId]) REFERENCES [MedicalGroups] ([GroupId]) ON DELETE CASCADE,
    CONSTRAINT [FK_StockMovements_SupplyItems_SupplyId] FOREIGN KEY ([SupplyId]) REFERENCES [SupplyItems] ([SupplyId]),
    CONSTRAINT [FK_StockMovements_Users_RecordedByUserId] FOREIGN KEY ([RecordedByUserId]) REFERENCES [Users] ([UserId]) ON DELETE SET NULL
);
GO


CREATE TABLE [GroupStaffDetails] (
    [Id] int NOT NULL IDENTITY,
    [GroupId] int NOT NULL,
    [StaffId] int NOT NULL,
    [PositionId] int NULL,
    [GroupPositionQuotaId] int NULL,
    [WorkPosition] nvarchar(100) NOT NULL,
    [WorkStatus] nvarchar(50) NOT NULL,
    [ExamDate] datetime2 NOT NULL,
    [CheckInTime] datetime2 NULL,
    [CheckOutTime] datetime2 NULL,
    [ShiftType] float NOT NULL,
    [CalculatedSalary] decimal(18,2) NOT NULL,
    [AssignedByUserId] int NULL,
    [AssignedAt] datetime2 NULL,
    [PickupLocation] nvarchar(200) NULL,
    [Note] nvarchar(max) NULL,
    CONSTRAINT [PK_GroupStaffDetails] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GroupStaffDetails_GroupPositionQuotas_GroupPositionQuotaId] FOREIGN KEY ([GroupPositionQuotaId]) REFERENCES [GroupPositionQuotas] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_GroupStaffDetails_MedicalGroups_GroupId] FOREIGN KEY ([GroupId]) REFERENCES [MedicalGroups] ([GroupId]) ON DELETE CASCADE,
    CONSTRAINT [FK_GroupStaffDetails_Positions_PositionId] FOREIGN KEY ([PositionId]) REFERENCES [Positions] ([PositionId]),
    CONSTRAINT [FK_GroupStaffDetails_Staffs_StaffId] FOREIGN KEY ([StaffId]) REFERENCES [Staffs] ([StaffId]) ON DELETE NO ACTION
);
GO


CREATE TABLE [ExamResults] (
    [ExamResultId] int NOT NULL IDENTITY,
    [PatientId] int NOT NULL,
    [GroupId] int NULL,
    [ExamType] nvarchar(100) NOT NULL,
    [Result] nvarchar(max) NOT NULL,
    [Diagnosis] nvarchar(500) NOT NULL,
    [DoctorStaffId] int NULL,
    [ExamDate] datetime2 NOT NULL,
    CONSTRAINT [PK_ExamResults] PRIMARY KEY ([ExamResultId]),
    CONSTRAINT [FK_ExamResults_MedicalGroups_GroupId] FOREIGN KEY ([GroupId]) REFERENCES [MedicalGroups] ([GroupId]),
    CONSTRAINT [FK_ExamResults_Patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [Patients] ([PatientId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ExamResults_Staffs_DoctorStaffId] FOREIGN KEY ([DoctorStaffId]) REFERENCES [Staffs] ([StaffId])
);
GO


CREATE TABLE [MedicalRecords] (
    [MedicalRecordId] int NOT NULL IDENTITY,
    [GroupId] int NOT NULL,
    [FullName] nvarchar(100) NOT NULL,
    [DateOfBirth] datetime2 NULL,
    [Gender] nvarchar(10) NULL,
    [IDCardNumber] nvarchar(20) NULL,
    [Department] nvarchar(100) NULL,
    [QrToken] nvarchar(512) NOT NULL,
    [Status] nvarchar(30) NOT NULL,
    [PatientId] int NULL,
    [CheckInAt] datetime2 NULL,
    [CheckInByUserId] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [MedicalGroupGroupId] int NULL,
    CONSTRAINT [PK_MedicalRecords] PRIMARY KEY ([MedicalRecordId]),
    CONSTRAINT [FK_MedicalRecords_MedicalGroups_GroupId] FOREIGN KEY ([GroupId]) REFERENCES [MedicalGroups] ([GroupId]) ON DELETE CASCADE,
    CONSTRAINT [FK_MedicalRecords_MedicalGroups_MedicalGroupGroupId] FOREIGN KEY ([MedicalGroupGroupId]) REFERENCES [MedicalGroups] ([GroupId]),
    CONSTRAINT [FK_MedicalRecords_Patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [Patients] ([PatientId])
);
GO


IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PermissionId', N'Module', N'PermissionKey', N'PermissionName') AND [object_id] = OBJECT_ID(N'[Permissions]'))
    SET IDENTITY_INSERT [Permissions] ON;
INSERT INTO [Permissions] ([PermissionId], [Module], [PermissionKey], [PermissionName])
VALUES (1, N'HopDong', N'HopDong.View', N'Xem hợp đồng'),
(2, N'HopDong', N'HopDong.Create', N'Tạo hợp đồng'),
(3, N'HopDong', N'HopDong.Edit', N'Sửa hợp đồng'),
(4, N'HopDong', N'HopDong.Approve', N'Phê duyệt hợp đồng'),
(5, N'HopDong', N'HopDong.Reject', N'Từ chối hợp đồng'),
(6, N'HopDong', N'HopDong.Upload', N'Đính kèm file hợp đồng'),
(10, N'DoanKham', N'DoanKham.View', N'Xem đoàn khám'),
(11, N'DoanKham', N'DoanKham.Create', N'Tạo đoàn khám'),
(12, N'DoanKham', N'DoanKham.Edit', N'Sửa đoàn khám'),
(13, N'DoanKham', N'DoanKham.SetPosition', N'Thiết lập vị trí đoàn'),
(14, N'DoanKham', N'DoanKham.AssignStaff', N'Phân công nhân sự đoàn'),
(15, N'DoanKham', N'DoanKham.ManageOwn', N'Quản lý đoàn mình phụ trách'),
(16, N'DoanKham', N'DoanKham.Lock', N'Khóa sổ đoàn khám'),
(20, N'LichKham', N'LichKham.ViewOwn', N'Xem lịch của mình'),
(21, N'LichKham', N'LichKham.ViewAll', N'Xem toàn bộ lịch khám'),
(30, N'ChamCong', N'ChamCong.QR', N'Mở QR chấm công'),
(31, N'ChamCong', N'ChamCong.CheckInOut', N'Check-in/out nhân viên'),
(32, N'ChamCong', N'ChamCong.ViewAll', N'Xem toàn bộ chấm công'),
(40, N'BaoCao', N'BaoCao.View', N'Xem báo cáo'),
(41, N'BaoCao', N'BaoCao.QC', N'Thực hiện QC hồ sơ'),
(50, N'Kho', N'Kho.View', N'Xem kho vật tư'),
(51, N'Kho', N'Kho.Edit', N'Nhập xuất kho'),
(60, N'TaiChinh', N'QuyetToan.Edit', N'Sửa phát sinh quyết toán'),
(62, N'TaiChinh', N'QuyetToan.Calculate', N'Tính toán quyết toán'),
(63, N'TaiChinh', N'QuyetToan.Finalize', N'Chốt xác nhận quyết toán'),
(64, N'BaoCao', N'BaoCao.ViewFinance', N'Xem báo cáo tài chính P&L'),
(65, N'BaoCao', N'BaoCao.Export', N'Xuất báo cáo'),
(100, N'HeThong', N'HeThong.UserManage', N'Quản lý người dùng'),
(101, N'HeThong', N'HeThong.RoleManage', N'Quản lý vai trò & quyền'),
(110, N'PhongBan', N'PhongBan.View', N'Xem phòng ban'),
(111, N'PhongBan', N'PhongBan.Edit', N'Sửa phòng ban'),
(120, N'WorkRule', N'WorkRule.View', N'Xem bảng rule chức năng'),
(121, N'WorkRule', N'WorkRule.Edit', N'Sửa bảng rule chức năng'),
(130, N'AI', N'AI.SuggestStaff', N'AI gợi ý phân công nhân sự'),
(140, N'Kho', N'Kho.Reports', N'Xem báo cáo tồn kho'),
(141, N'Kho', N'Kho.Import', N'Import phiếu nhập kho'),
(142, N'Kho', N'Kho.Export', N'Export phiếu xuất kho');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PermissionId', N'Module', N'PermissionKey', N'PermissionName') AND [object_id] = OBJECT_ID(N'[Permissions]'))
    SET IDENTITY_INSERT [Permissions] OFF;
GO


IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RoleId', N'Description', N'RoleName') AND [object_id] = OBJECT_ID(N'[Roles]'))
    SET IDENTITY_INSERT [Roles] ON;
INSERT INTO [Roles] ([RoleId], [Description], [RoleName])
VALUES (1, N'Quản trị hệ thống', N'Admin'),
(2, N'Quản lý nhân sự', N'PersonnelManager'),
(3, N'Quản lý hợp đồng', N'ContractManager'),
(4, N'Quản lý tính lương', N'PayrollManager'),
(5, N'Quản lý đoàn khám', N'MedicalGroupManager'),
(6, N'Quản lý kho vật tư', N'WarehouseManager'),
(7, N'Nhân viên đi đoàn', N'MedicalStaff'),
(8, N'Đại diện doanh nghiệp đối tác', N'Customer');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RoleId', N'Description', N'RoleName') AND [object_id] = OBJECT_ID(N'[Roles]'))
    SET IDENTITY_INSERT [Roles] OFF;
GO


IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'StationCode', N'DefaultMinutesPerPatient', N'IsActive', N'RequiredGender', N'ServiceType', N'SortOrder', N'StationName', N'WipLimit') AND [object_id] = OBJECT_ID(N'[Stations]'))
    SET IDENTITY_INSERT [Stations] ON;
INSERT INTO [Stations] ([StationCode], [DefaultMinutesPerPatient], [IsActive], [RequiredGender], [ServiceType], [SortOrder], [StationName], [WipLimit])
VALUES (N'CHECKIN', 5.0, CAST(1 AS bit), NULL, N'ADMIN', 1, N'Tiếp đón & Cấp số', 1),
(N'LAY_MAU', 5.0, CAST(1 AS bit), NULL, N'LAB', 3, N'Lấy mẫu xét nghiệm', 1),
(N'MAT_TAI_MUI_HONG', 5.0, CAST(1 AS bit), NULL, N'CLINICAL', 7, N'Mắt - Tai Mũi Họng', 1),
(N'NOI_KHOA', 5.0, CAST(1 AS bit), NULL, N'CLINICAL', 6, N'Khám Nội khoa', 1),
(N'QC', 5.0, CAST(1 AS bit), NULL, N'ADMIN', 99, N'Kiểm tra hồ sơ (QC)', 1),
(N'SIEU_AM', 5.0, CAST(1 AS bit), NULL, N'IMAGING', 5, N'Siêu âm', 1),
(N'SINH_HIEU', 5.0, CAST(1 AS bit), NULL, N'CLINICAL', 2, N'Đo sinh hiệu', 1),
(N'XQUANG', 5.0, CAST(1 AS bit), NULL, N'IMAGING', 4, N'Chụp X-Quang', 1);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'StationCode', N'DefaultMinutesPerPatient', N'IsActive', N'RequiredGender', N'ServiceType', N'SortOrder', N'StationName', N'WipLimit') AND [object_id] = OBJECT_ID(N'[Stations]'))
    SET IDENTITY_INSERT [Stations] OFF;
GO


IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'PermissionId', N'RoleId') AND [object_id] = OBJECT_ID(N'[RolePermissions]'))
    SET IDENTITY_INSERT [RolePermissions] ON;
INSERT INTO [RolePermissions] ([Id], [PermissionId], [RoleId])
VALUES (1, 1, 1),
(2, 2, 1),
(3, 3, 1),
(4, 4, 1),
(5, 5, 1),
(6, 6, 1),
(7, 10, 1),
(8, 11, 1),
(9, 12, 1),
(10, 13, 1),
(11, 14, 1),
(12, 15, 1),
(13, 16, 1),
(14, 20, 1),
(15, 21, 1),
(16, 30, 1),
(17, 31, 1),
(18, 32, 1),
(19, 40, 1),
(20, 41, 1),
(21, 50, 1),
(22, 51, 1),
(23, 60, 1),
(24, 62, 1),
(25, 63, 1),
(26, 64, 1),
(27, 65, 1),
(28, 100, 1),
(29, 101, 1),
(30, 110, 1),
(31, 111, 1),
(32, 120, 1),
(33, 121, 1),
(34, 130, 1),
(35, 140, 1),
(36, 141, 1),
(37, 142, 1),
(100, 1, 3),
(101, 2, 3),
(102, 3, 3),
(103, 4, 3),
(104, 5, 3);
INSERT INTO [RolePermissions] ([Id], [PermissionId], [RoleId])
VALUES (105, 6, 3),
(106, 110, 3),
(107, 120, 3),
(200, 10, 5),
(201, 11, 5),
(202, 12, 5),
(203, 13, 5),
(204, 14, 5),
(205, 15, 5),
(206, 16, 5),
(207, 21, 5),
(208, 30, 5),
(209, 130, 5),
(300, 14, 2),
(301, 21, 2),
(302, 32, 2),
(303, 100, 2),
(304, 110, 2),
(400, 10, 11),
(401, 21, 11),
(402, 40, 11),
(403, 41, 11),
(404, 110, 11),
(500, 50, 6),
(501, 51, 6),
(502, 140, 6),
(503, 141, 6),
(504, 142, 6),
(600, 1, 9),
(601, 40, 9),
(602, 60, 9),
(603, 62, 9),
(604, 63, 9),
(605, 64, 9),
(606, 65, 9),
(607, 110, 9);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'PermissionId', N'RoleId') AND [object_id] = OBJECT_ID(N'[RolePermissions]'))
    SET IDENTITY_INSERT [RolePermissions] OFF;
GO


IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'UserId', N'AvatarPath', N'CompanyId', N'CreatedAt', N'Email', N'FullName', N'IsActive', N'PasswordHash', N'RefreshToken', N'RefreshTokenExpiry', N'RoleId', N'StaffId', N'Username') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] ON;
INSERT INTO [Users] ([UserId], [AvatarPath], [CompanyId], [CreatedAt], [Email], [FullName], [IsActive], [PasswordHash], [RefreshToken], [RefreshTokenExpiry], [RoleId], [StaffId], [Username])
VALUES (1, NULL, NULL, '2024-01-01T00:00:00.0000000', NULL, N'System Administrator', CAST(1 AS bit), N'$2a$11$azxY7U9nnOEV4xF4Cto.gOLsbvUKtALtNoqMjMFRhVfP/45V1BrJ.', NULL, NULL, 1, NULL, N'admin');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'UserId', N'AvatarPath', N'CompanyId', N'CreatedAt', N'Email', N'FullName', N'IsActive', N'PasswordHash', N'RefreshToken', N'RefreshTokenExpiry', N'RoleId', N'StaffId', N'Username') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] OFF;
GO


CREATE INDEX [IX_ApprovalHistories_ApprovalStepId] ON [ApprovalHistories] ([ApprovalStepId]);
GO


CREATE INDEX [IX_ApprovalHistories_ApproverId] ON [ApprovalHistories] ([ApproverId]);
GO


CREATE INDEX [IX_ApprovalHistories_HealthContractId] ON [ApprovalHistories] ([HealthContractId]);
GO


CREATE INDEX [IX_ApprovalSteps_HealthContractId] ON [ApprovalSteps] ([HealthContractId]);
GO


CREATE INDEX [IX_ApprovalSteps_RoleId] ON [ApprovalSteps] ([RoleId]);
GO


CREATE INDEX [IX_AuditLogs_UserId] ON [AuditLogs] ([UserId]);
GO


CREATE INDEX [IX_ContractApprovalHistories_ApprovedByUserId] ON [ContractApprovalHistories] ([ApprovedByUserId]);
GO


CREATE INDEX [IX_ContractApprovalHistories_HealthContractId] ON [ContractApprovalHistories] ([HealthContractId]);
GO


CREATE INDEX [IX_ContractAttachments_HealthContractId] ON [ContractAttachments] ([HealthContractId]);
GO


CREATE INDEX [IX_ContractFinancialSummaries_HealthContractId] ON [ContractFinancialSummaries] ([HealthContractId]);
GO


CREATE INDEX [IX_ContractRevenueSummaries_HealthContractId] ON [ContractRevenueSummaries] ([HealthContractId]);
GO


CREATE INDEX [IX_Contracts_CompanyId] ON [Contracts] ([CompanyId]);
GO


CREATE INDEX [IX_Contracts_CreatedByUserId] ON [Contracts] ([CreatedByUserId]);
GO


CREATE INDEX [IX_Contracts_SignerId] ON [Contracts] ([SignerId]);
GO


CREATE INDEX [IX_Contracts_UpdatedByUserId] ON [Contracts] ([UpdatedByUserId]);
GO


CREATE INDEX [IX_ContractStatusHistories_HealthContractId] ON [ContractStatusHistories] ([HealthContractId]);
GO


CREATE INDEX [IX_CostSnapshots_CreatedByUserId] ON [CostSnapshots] ([CreatedByUserId]);
GO


CREATE INDEX [IX_CostSnapshots_HealthContractId] ON [CostSnapshots] ([HealthContractId]);
GO


CREATE INDEX [IX_ExamResults_DoctorStaffId] ON [ExamResults] ([DoctorStaffId]);
GO


CREATE INDEX [IX_ExamResults_GroupId] ON [ExamResults] ([GroupId]);
GO


CREATE INDEX [IX_ExamResults_PatientId] ON [ExamResults] ([PatientId]);
GO


CREATE INDEX [IX_GroupCosts_CalculatedByUserId] ON [GroupCosts] ([CalculatedByUserId]);
GO


CREATE INDEX [IX_GroupCosts_GroupId] ON [GroupCosts] ([GroupId]);
GO


CREATE INDEX [IX_GroupPositionQuotas_MedicalGroupId] ON [GroupPositionQuotas] ([MedicalGroupId]);
GO


CREATE INDEX [IX_GroupPositionQuotas_PositionId] ON [GroupPositionQuotas] ([PositionId]);
GO


CREATE INDEX [IX_GroupStaffDetails_GroupId] ON [GroupStaffDetails] ([GroupId]);
GO


CREATE INDEX [IX_GroupStaffDetails_GroupPositionQuotaId] ON [GroupStaffDetails] ([GroupPositionQuotaId]);
GO


CREATE INDEX [IX_GroupStaffDetails_PositionId] ON [GroupStaffDetails] ([PositionId]);
GO


CREATE INDEX [IX_GroupStaffDetails_StaffId] ON [GroupStaffDetails] ([StaffId]);
GO


CREATE INDEX [IX_MedicalGroupPositions_GroupId] ON [MedicalGroupPositions] ([GroupId]);
GO


CREATE INDEX [IX_MedicalGroupPositions_PositionId] ON [MedicalGroupPositions] ([PositionId]);
GO


CREATE INDEX [IX_MedicalGroups_GroupLeaderStaffId] ON [MedicalGroups] ([GroupLeaderStaffId]);
GO


CREATE INDEX [IX_MedicalGroups_HealthContractId] ON [MedicalGroups] ([HealthContractId]);
GO


CREATE INDEX [IX_MedicalGroups_ManagerUserId] ON [MedicalGroups] ([ManagerUserId]);
GO


CREATE INDEX [IX_MedicalRecords_GroupId_Status] ON [MedicalRecords] ([GroupId], [Status]);
GO


CREATE INDEX [IX_MedicalRecords_MedicalGroupGroupId] ON [MedicalRecords] ([MedicalGroupGroupId]);
GO


CREATE INDEX [IX_MedicalRecords_PatientId] ON [MedicalRecords] ([PatientId]);
GO


CREATE UNIQUE INDEX [IX_MedicalRecords_QrToken] ON [MedicalRecords] ([QrToken]);
GO


CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
GO


CREATE INDEX [IX_Overheads_HealthContractId] ON [Overheads] ([HealthContractId]);
GO


CREATE INDEX [IX_Overheads_RecordedByUserId] ON [Overheads] ([RecordedByUserId]);
GO


CREATE INDEX [IX_Patients_HealthContractId] ON [Patients] ([HealthContractId]);
GO


CREATE INDEX [IX_Patients_MedicalGroupId] ON [Patients] ([MedicalGroupId]);
GO


CREATE UNIQUE INDEX [IX_Permissions_PermissionKey] ON [Permissions] ([PermissionKey]);
GO


CREATE INDEX [IX_Positions_Code] ON [Positions] ([Code]);
GO


CREATE INDEX [IX_RolePermissions_PermissionId] ON [RolePermissions] ([PermissionId]);
GO


CREATE INDEX [IX_RolePermissions_RoleId] ON [RolePermissions] ([RoleId]);
GO


CREATE INDEX [IX_ScheduleCalendars_GroupId] ON [ScheduleCalendars] ([GroupId]);
GO


CREATE INDEX [IX_ScheduleCalendars_StaffId] ON [ScheduleCalendars] ([StaffId]);
GO


CREATE INDEX [IX_StockMovements_MedicalGroupId] ON [StockMovements] ([MedicalGroupId]);
GO


CREATE INDEX [IX_StockMovements_RecordedByUserId] ON [StockMovements] ([RecordedByUserId]);
GO


CREATE INDEX [IX_StockMovements_SupplyId] ON [StockMovements] ([SupplyId]);
GO


CREATE INDEX [IX_UserRoles_RoleId] ON [UserRoles] ([RoleId]);
GO


CREATE INDEX [IX_UserRoles_UserId] ON [UserRoles] ([UserId]);
GO


CREATE INDEX [IX_Users_CompanyId] ON [Users] ([CompanyId]);
GO


CREATE INDEX [IX_Users_RoleId] ON [Users] ([RoleId]);
GO


CREATE INDEX [IX_Users_StaffId] ON [Users] ([StaffId]);
GO


CREATE UNIQUE INDEX [IX_Users_Username] ON [Users] ([Username]);
GO


