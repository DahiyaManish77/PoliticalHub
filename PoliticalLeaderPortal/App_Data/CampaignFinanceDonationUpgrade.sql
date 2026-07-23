IF OBJECT_ID('dbo.CampaignFinanceEntry', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CampaignFinanceEntry
    (
        CampaignFinanceEntryId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CampaignFinanceEntry PRIMARY KEY,
        EntryType NVARCHAR(40) NOT NULL,
        Title NVARCHAR(160) NOT NULL,
        ReferenceNo NVARCHAR(80) NULL,
        EntryDate DATETIME NOT NULL CONSTRAINT DF_CampaignFinanceEntry_Date DEFAULT(GETDATE()),
        PersonOrVendorName NVARCHAR(150) NULL,
        MobileNo NVARCHAR(30) NULL,
        Category NVARCHAR(120) NULL,
        PaymentMode NVARCHAR(80) NULL,
        Amount DECIMAL(18,2) NOT NULL CONSTRAINT DF_CampaignFinanceEntry_Amount DEFAULT(0),
        ProofUrl NVARCHAR(300) NULL,
        ApprovalStatus NVARCHAR(40) NOT NULL CONSTRAINT DF_CampaignFinanceEntry_Approval DEFAULT('Pending'),
        ApprovedBy NVARCHAR(120) NULL,
        Remarks NVARCHAR(500) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_CampaignFinanceEntry_Active DEFAULT(1),
        CreatedBy INT NULL,
        CreatedDate DATETIME NOT NULL CONSTRAINT DF_CampaignFinanceEntry_Created DEFAULT(GETDATE()),
        UpdatedBy INT NULL,
        UpdatedDate DATETIME NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CampaignFinanceEntry_TypeStatus' AND object_id = OBJECT_ID('dbo.CampaignFinanceEntry'))
    CREATE INDEX IX_CampaignFinanceEntry_TypeStatus ON dbo.CampaignFinanceEntry(EntryType, ApprovalStatus, EntryDate);
GO
