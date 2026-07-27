SET NOCOUNT ON;
IF OBJECT_ID('dbo.PortalVideoMeeting','U') IS NULL
BEGIN
    CREATE TABLE dbo.PortalVideoMeeting
    (
        VideoMeetingId INT IDENTITY PRIMARY KEY,
        Title NVARCHAR(180) NOT NULL,
        Description NVARCHAR(1000) NULL,
        MeetingType NVARCHAR(40) NOT NULL,
        ScheduledStart DATETIME NOT NULL,
        DurationMinutes INT NOT NULL,
        MaximumParticipants INT NOT NULL,
        AllowParticipantCamera BIT NOT NULL,
        AllowParticipantMicrophone BIT NOT NULL,
        AllowRecording BIT NOT NULL,
        AutoRecord BIT NOT NULL,
        RequireHostApproval BIT NOT NULL,
        Invitees NVARCHAR(2000) NULL,
        Status NVARCHAR(20) NOT NULL,
        SecureJoinToken NVARCHAR(64) NOT NULL,
        ProviderMeetingId NVARCHAR(200) NULL,
        IsDeleted BIT NOT NULL DEFAULT(0),
        CreatedBy INT NULL,
        CreatedDate DATETIME NOT NULL DEFAULT(GETDATE()),
        UpdatedBy INT NULL,
        UpdatedDate DATETIME NULL
    );
    CREATE UNIQUE INDEX UX_PortalVideoMeeting_Token ON dbo.PortalVideoMeeting(SecureJoinToken);
END;
