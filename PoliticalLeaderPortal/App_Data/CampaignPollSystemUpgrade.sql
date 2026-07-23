IF OBJECT_ID('dbo.CampaignPollResponse', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CampaignPollResponse
    (
        CampaignPollResponseId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CampaignPollId INT NOT NULL,
        CampaignPollOptionId INT NOT NULL,
        RespondentName NVARCHAR(150) NULL,
        MobileNo NVARCHAR(30) NULL,
        AreaName NVARCHAR(150) NULL,
        Source NVARCHAR(50) NULL,
        IpAddress NVARCHAR(64) NULL,
        UserAgent NVARCHAR(300) NULL,
        ConsentGiven BIT NOT NULL DEFAULT(0),
        Remarks NVARCHAR(500) NULL,
        SubmittedOn DATETIME NOT NULL DEFAULT(GETDATE())
    );
END;

IF OBJECT_ID('dbo.CampaignPollOption', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CampaignPollOption
    (
        CampaignPollOptionId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CampaignPollId INT NOT NULL,
        OptionText NVARCHAR(250) NOT NULL,
        DisplayOrder INT NOT NULL DEFAULT(0),
        IsActive BIT NOT NULL DEFAULT(1)
    );
END;

IF OBJECT_ID('dbo.CampaignPoll', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CampaignPoll
    (
        CampaignPollId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Title NVARCHAR(200) NOT NULL,
        Question NVARCHAR(500) NOT NULL,
        Description NVARCHAR(1000) NULL,
        TargetArea NVARCHAR(150) NULL,
        PollType NVARCHAR(80) NULL,
        PublicSlug NVARCHAR(160) NOT NULL,
        StartDate DATE NULL,
        EndDate DATE NULL,
        ShowPublicResults BIT NOT NULL DEFAULT(0),
        RequireConsent BIT NOT NULL DEFAULT(1),
        IsActive BIT NOT NULL DEFAULT(1),
        CreatedBy INT NULL,
        CreatedOn DATETIME NOT NULL DEFAULT(GETDATE()),
        UpdatedBy INT NULL,
        UpdatedOn DATETIME NULL
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CampaignPoll_PublicSlug' AND object_id = OBJECT_ID('dbo.CampaignPoll'))
    CREATE UNIQUE INDEX IX_CampaignPoll_PublicSlug ON dbo.CampaignPoll(PublicSlug);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CampaignPollResponse_Poll' AND object_id = OBJECT_ID('dbo.CampaignPollResponse'))
    CREATE INDEX IX_CampaignPollResponse_Poll ON dbo.CampaignPollResponse(CampaignPollId, CampaignPollOptionId);
