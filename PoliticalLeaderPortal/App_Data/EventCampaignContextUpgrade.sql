SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.EventCampaignContext', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EventCampaignContext
    (
        EventCampaignContextId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EventCampaignContext PRIMARY KEY,
        EventId INT NOT NULL,
        OperationalCampaignId INT NOT NULL,
        CreatedBy INT NULL,
        CreatedDate DATETIME NOT NULL CONSTRAINT DF_EventCampaignContext_CreatedDate DEFAULT (GETDATE()),
        UpdatedBy INT NULL,
        UpdatedDate DATETIME NULL,
        CONSTRAINT UQ_EventCampaignContext_Event UNIQUE (EventId),
        CONSTRAINT FK_EventCampaignContext_Event FOREIGN KEY (EventId) REFERENCES dbo.EventMaster(EventId),
        CONSTRAINT FK_EventCampaignContext_Campaign FOREIGN KEY (OperationalCampaignId) REFERENCES dbo.ElectionCampaign(CampaignId)
    );
    CREATE INDEX IX_EventCampaignContext_Campaign ON dbo.EventCampaignContext(OperationalCampaignId, EventId);
END;

EXEC(N'
CREATE OR ALTER VIEW dbo.vw_CampaignFieldOperation
AS
SELECT context.OperationalCampaignId,campaign.CampaignName,event.EventId,event.EventCode,
       event.EventTitle,event.EventDate,event.EventType,event.Status,event.Priority,
       event.Budget,event.ActualExpense,event.IsActive
FROM dbo.EventCampaignContext context
INNER JOIN dbo.EventMaster event ON event.EventId=context.EventId
INNER JOIN dbo.ElectionCampaign campaign ON campaign.CampaignId=context.OperationalCampaignId;
');

COMMIT TRANSACTION;
