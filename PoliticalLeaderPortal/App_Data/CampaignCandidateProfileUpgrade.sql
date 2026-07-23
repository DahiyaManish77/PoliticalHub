IF OBJECT_ID('dbo.CandidateProfile', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CandidateProfile
    (
        CandidateProfileId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CandidateProfile PRIMARY KEY,
        FullName NVARCHAR(150) NOT NULL,
        PartyName NVARCHAR(100) NULL,
        ElectionType NVARCHAR(120) NULL,
        ConstituencyName NVARCHAR(150) NULL,
        District NVARCHAR(120) NULL,
        State NVARCHAR(120) NULL,
        Education NVARCHAR(200) NULL,
        Profession NVARCHAR(150) NULL,
        PublicBio NVARCHAR(500) NULL,
        ManifestoUrl NVARCHAR(300) NULL,
        AffidavitUrl NVARCHAR(300) NULL,
        PhotoUrl NVARCHAR(300) NULL,
        FacebookUrl NVARCHAR(300) NULL,
        TwitterUrl NVARCHAR(300) NULL,
        InstagramUrl NVARCHAR(300) NULL,
        YouTubeUrl NVARCHAR(300) NULL,
        DeclaredAssets DECIMAL(18,2) NOT NULL CONSTRAINT DF_CandidateProfile_Assets DEFAULT(0),
        DeclaredLiabilities DECIMAL(18,2) NOT NULL CONSTRAINT DF_CandidateProfile_Liabilities DEFAULT(0),
        CriminalCaseSummary NVARCHAR(500) NULL,
        ApprovalStatus NVARCHAR(40) NOT NULL CONSTRAINT DF_CandidateProfile_Approval DEFAULT('Draft'),
        IsPublished BIT NOT NULL CONSTRAINT DF_CandidateProfile_Published DEFAULT(0),
        IsActive BIT NOT NULL CONSTRAINT DF_CandidateProfile_Active DEFAULT(1),
        CreatedBy INT NULL,
        CreatedDate DATETIME NOT NULL CONSTRAINT DF_CandidateProfile_Created DEFAULT(GETDATE()),
        UpdatedBy INT NULL,
        UpdatedDate DATETIME NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CandidateProfile_Status' AND object_id = OBJECT_ID('dbo.CandidateProfile'))
    CREATE INDEX IX_CandidateProfile_Status ON dbo.CandidateProfile(ApprovalStatus, IsPublished, IsActive);
GO
