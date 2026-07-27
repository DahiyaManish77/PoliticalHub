SET NOCOUNT ON;

IF OBJECT_ID('dbo.PeopleSayVideo', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PeopleSayVideo
    (
        PeopleSayVideoId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PeopleSayVideo PRIMARY KEY,
        PersonName NVARCHAR(150) NOT NULL,
        MobileNumber NVARCHAR(20) NOT NULL,
        AreaName NVARCHAR(150) NULL,
        Title NVARCHAR(180) NOT NULL,
        Message NVARCHAR(600) NULL,
        VideoPath NVARCHAR(500) NOT NULL,
        Status NVARCHAR(20) NOT NULL CONSTRAINT DF_PeopleSayVideo_Status DEFAULT('Pending'),
        RejectionReason NVARCHAR(500) NULL,
        PublicationConsent BIT NOT NULL,
        ApprovedBy INT NULL,
        ApprovedOn DATETIME NULL,
        LeaderResponseVideoPath NVARCHAR(500) NULL,
        LeaderResponseMessage NVARCHAR(600) NULL,
        ViewCount INT NOT NULL CONSTRAINT DF_PeopleSayVideo_Views DEFAULT(0),
        LikeCount INT NOT NULL CONSTRAINT DF_PeopleSayVideo_Likes DEFAULT(0),
        CommentCount INT NOT NULL CONSTRAINT DF_PeopleSayVideo_Comments DEFAULT(0),
        ShareCount INT NOT NULL CONSTRAINT DF_PeopleSayVideo_Shares DEFAULT(0),
        DownloadCount INT NOT NULL CONSTRAINT DF_PeopleSayVideo_Downloads DEFAULT(0),
        IsDeleted BIT NOT NULL CONSTRAINT DF_PeopleSayVideo_Deleted DEFAULT(0),
        CreatedDate DATETIME NOT NULL CONSTRAINT DF_PeopleSayVideo_Created DEFAULT(GETDATE()),
        UpdatedDate DATETIME NULL
    );
    CREATE INDEX IX_PeopleSayVideo_StatusCreated ON dbo.PeopleSayVideo(Status, IsDeleted, CreatedDate DESC);
END;

IF OBJECT_ID('dbo.PeopleSayComment', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PeopleSayComment
    (
        PeopleSayCommentId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PeopleSayComment PRIMARY KEY,
        PeopleSayVideoId INT NOT NULL,
        PersonName NVARCHAR(100) NOT NULL,
        CommentText NVARCHAR(500) NOT NULL,
        Status NVARCHAR(20) NOT NULL CONSTRAINT DF_PeopleSayComment_Status DEFAULT('Pending'),
        ReviewedBy INT NULL,
        ReviewedOn DATETIME NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_PeopleSayComment_Deleted DEFAULT(0),
        CreatedDate DATETIME NOT NULL CONSTRAINT DF_PeopleSayComment_Created DEFAULT(GETDATE()),
        CONSTRAINT FK_PeopleSayComment_Video FOREIGN KEY(PeopleSayVideoId) REFERENCES dbo.PeopleSayVideo(PeopleSayVideoId)
    );
    CREATE INDEX IX_PeopleSayComment_Status ON dbo.PeopleSayComment(Status, IsDeleted, CreatedDate DESC);
END;

IF OBJECT_ID('dbo.PeopleSayEngagement', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PeopleSayEngagement
    (
        PeopleSayEngagementId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PeopleSayEngagement PRIMARY KEY,
        PeopleSayVideoId INT NOT NULL,
        EngagementType NVARCHAR(20) NOT NULL,
        VisitorKey NVARCHAR(100) NOT NULL,
        CreatedDate DATETIME NOT NULL CONSTRAINT DF_PeopleSayEngagement_Created DEFAULT(GETDATE()),
        CONSTRAINT FK_PeopleSayEngagement_Video FOREIGN KEY(PeopleSayVideoId) REFERENCES dbo.PeopleSayVideo(PeopleSayVideoId)
    );
    CREATE UNIQUE INDEX UX_PeopleSayEngagement_UniqueLike
        ON dbo.PeopleSayEngagement(PeopleSayVideoId, EngagementType, VisitorKey)
        WHERE EngagementType = 'Like';
END;
