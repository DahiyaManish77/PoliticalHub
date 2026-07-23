IF OBJECT_ID('dbo.CampaignOperationItem', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CampaignOperationItem
    (
        CampaignOperationItemId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ModuleKey NVARCHAR(80) NOT NULL,
        Title NVARCHAR(220) NOT NULL,
        Category NVARCHAR(100) NULL,
        OwnerName NVARCHAR(150) NULL,
        OwnerMobile NVARCHAR(30) NULL,
        AreaName NVARCHAR(150) NULL,
        Priority NVARCHAR(40) NULL,
        Status NVARCHAR(50) NULL,
        StartDate DATE NULL,
        DueDate DATE NULL,
        Quantity INT NULL,
        BudgetAmount DECIMAL(18,2) NULL,
        ReferenceUrl NVARCHAR(500) NULL,
        Description NVARCHAR(1200) NULL,
        ComplianceNote NVARCHAR(800) NULL,
        IsApproved BIT NOT NULL DEFAULT(0),
        IsActive BIT NOT NULL DEFAULT(1),
        CreatedBy INT NULL,
        CreatedOn DATETIME NOT NULL DEFAULT(GETDATE()),
        UpdatedBy INT NULL,
        UpdatedOn DATETIME NULL
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CampaignOperationItem_Module' AND object_id = OBJECT_ID('dbo.CampaignOperationItem'))
    CREATE INDEX IX_CampaignOperationItem_Module ON dbo.CampaignOperationItem(ModuleKey, IsActive, Status);
