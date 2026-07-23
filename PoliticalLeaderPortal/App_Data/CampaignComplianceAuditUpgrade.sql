IF OBJECT_ID('dbo.CampaignAuditLog', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CampaignAuditLog
    (
        CampaignAuditLogId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CampaignAuditLog PRIMARY KEY,
        ModuleName NVARCHAR(100) NOT NULL,
        RecordId NVARCHAR(80) NULL,
        ActionName NVARCHAR(80) NOT NULL,
        PerformedBy NVARCHAR(120) NULL,
        PerformedByUserId INT NULL,
        PerformedOn DATETIME NOT NULL CONSTRAINT DF_CampaignAuditLog_PerformedOn DEFAULT(GETDATE()),
        IpAddress NVARCHAR(80) NULL,
        Remarks NVARCHAR(500) NULL,
        IsSensitive BIT NOT NULL CONSTRAINT DF_CampaignAuditLog_Sensitive DEFAULT(0)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CampaignAuditLog_ModuleDate' AND object_id = OBJECT_ID('dbo.CampaignAuditLog'))
    CREATE INDEX IX_CampaignAuditLog_ModuleDate ON dbo.CampaignAuditLog(ModuleName, PerformedOn DESC);
GO
