SET NOCOUNT ON;

IF OBJECT_ID(N'dbo.VerifiedDocument', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.VerifiedDocument
    (
        VerifiedDocumentId INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_VerifiedDocument PRIMARY KEY,
        DocumentNumber NVARCHAR(40) NOT NULL,
        VerificationCode NVARCHAR(64) NOT NULL,
        DocumentType NVARCHAR(30) NOT NULL,
        RecipientName NVARCHAR(150) NOT NULL,
        RecipientReference NVARCHAR(80) NULL,
        RecipientRole NVARCHAR(120) NULL,
        RecipientPhotoPath NVARCHAR(500) NULL,
        CampaignId INT NULL,
        Subject NVARCHAR(250) NULL,
        BodyText NVARCHAR(MAX) NULL,
        IssueDate DATE NOT NULL,
        ExpiryDate DATE NULL,
        Status NVARCHAR(20) NOT NULL
            CONSTRAINT DF_VerifiedDocument_Status DEFAULT(N'Active'),
        IssuedByName NVARCHAR(150) NULL,
        IssuedByDesignation NVARCHAR(120) NULL,
        CreatedBy NVARCHAR(128) NULL,
        CreatedOn DATETIME2(0) NOT NULL
            CONSTRAINT DF_VerifiedDocument_CreatedOn DEFAULT(SYSDATETIME()),
        RevokedBy NVARCHAR(128) NULL,
        RevokedOn DATETIME2(0) NULL,
        RevocationReason NVARCHAR(300) NULL,
        CONSTRAINT UQ_VerifiedDocument_DocumentNumber UNIQUE(DocumentNumber),
        CONSTRAINT UQ_VerifiedDocument_VerificationCode UNIQUE(VerificationCode),
        CONSTRAINT CK_VerifiedDocument_Type CHECK
            (DocumentType IN (N'DigitalCard', N'AppointmentLetter', N'AuthorizationLetter', N'VolunteerLetter')),
        CONSTRAINT CK_VerifiedDocument_Status CHECK
            (Status IN (N'Active', N'Revoked')),
        CONSTRAINT CK_VerifiedDocument_Dates CHECK
            (ExpiryDate IS NULL OR ExpiryDate >= IssueDate)
    );

    CREATE INDEX IX_VerifiedDocument_Recipient
        ON dbo.VerifiedDocument(RecipientName, IssueDate DESC);
    CREATE INDEX IX_VerifiedDocument_Campaign
        ON dbo.VerifiedDocument(CampaignId, IssueDate DESC);
END;

IF COL_LENGTH(N'dbo.VerifiedDocument', N'RecipientPhotoPath') IS NULL
    ALTER TABLE dbo.VerifiedDocument ADD RecipientPhotoPath NVARCHAR(500) NULL;

IF NOT EXISTS
(
    SELECT 1 FROM dbo.MenuMaster
    WHERE ControllerName = N'VerifiedDocument' AND AreaName = N'Admin'
)
BEGIN
    DECLARE @CommunicationParentId INT =
    (
        SELECT TOP (1) MenuId
        FROM dbo.MenuMaster
        WHERE MenuName = N'Communication & Media'
        ORDER BY MenuId
    );

    IF @CommunicationParentId IS NOT NULL
    BEGIN
        INSERT dbo.MenuMaster
        (
            MenuName, MenuDescription, AreaName, ControllerName, ActionName,
            MenuType, IconClass, DisplayOrder, IsActive, ShowOnHome,
            ShowInAdminSidebar, OpenInNewTab, IsClickable, HasMegaMenu,
            CreatedDate, ParentMenuId, MenuLevel, ShowInFooter,
            ShowInQuickLinks, IsSystemMenu
        )
        VALUES
        (
            N'Verified Cards & Letters',
            N'Issue QR-verifiable cards and official campaign letters',
            N'Admin', N'VerifiedDocument', N'Index', N'AdminPage',
            N'bi bi-patch-check', 90, 1, 0, 1, 0, 1, 0,
            GETDATE(), @CommunicationParentId, 1, 0, 0, 1
        );
    END;
END;
