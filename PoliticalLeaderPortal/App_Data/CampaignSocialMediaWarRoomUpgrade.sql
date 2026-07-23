IF OBJECT_ID('dbo.SocialMediaPost', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SocialMediaPost
    (
        SocialMediaPostId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SocialMediaPost PRIMARY KEY,
        Platform NVARCHAR(120) NOT NULL,
        ContentTitle NVARCHAR(180) NOT NULL,
        ContentType NVARCHAR(80) NULL,
        Caption NVARCHAR(600) NULL,
        MediaUrl NVARCHAR(300) NULL,
        PublicUrl NVARCHAR(300) NULL,
        ScheduledOn DATETIME NULL,
        AssignedTo NVARCHAR(80) NULL,
        ApprovalStatus NVARCHAR(40) NOT NULL CONSTRAINT DF_SocialMediaPost_Approval DEFAULT('Draft'),
        PublishStatus NVARCHAR(40) NOT NULL CONSTRAINT DF_SocialMediaPost_Publish DEFAULT('Planned'),
        ReachCount INT NOT NULL CONSTRAINT DF_SocialMediaPost_Reach DEFAULT(0),
        EngagementCount INT NOT NULL CONSTRAINT DF_SocialMediaPost_Engagement DEFAULT(0),
        ShareCount INT NOT NULL CONSTRAINT DF_SocialMediaPost_Share DEFAULT(0),
        CommentCount INT NOT NULL CONSTRAINT DF_SocialMediaPost_Comment DEFAULT(0),
        ReviewRemarks NVARCHAR(300) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_SocialMediaPost_Active DEFAULT(1),
        CreatedBy INT NULL,
        CreatedDate DATETIME NOT NULL CONSTRAINT DF_SocialMediaPost_Created DEFAULT(GETDATE()),
        UpdatedBy INT NULL,
        UpdatedDate DATETIME NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SocialMediaPost_Status' AND object_id = OBJECT_ID('dbo.SocialMediaPost'))
    CREATE INDEX IX_SocialMediaPost_Status ON dbo.SocialMediaPost(Platform, ApprovalStatus, PublishStatus, ScheduledOn);
GO
