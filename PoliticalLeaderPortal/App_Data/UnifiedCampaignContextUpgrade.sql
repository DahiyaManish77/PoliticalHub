/*
    Unified campaign context
    ------------------------
    CampaignMaster remains the planning/master record.
    ElectionCampaign remains the operational record used by campaign-linked
    War Room tables. This mapping provides one stable relationship without
    changing generated EDMX entities or destroying legacy data.
*/
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.CampaignContextMap', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CampaignContextMap
    (
        CampaignContextMapId INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_CampaignContextMap PRIMARY KEY,
        CampaignMasterId INT NOT NULL,
        OperationalCampaignId INT NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_CampaignContextMap_IsActive DEFAULT (1),
        CreatedDate DATETIME NOT NULL CONSTRAINT DF_CampaignContextMap_CreatedDate DEFAULT (GETDATE()),
        UpdatedDate DATETIME NULL,
        CONSTRAINT UQ_CampaignContextMap_Master UNIQUE (CampaignMasterId),
        CONSTRAINT UQ_CampaignContextMap_Operational UNIQUE (OperationalCampaignId),
        CONSTRAINT FK_CampaignContextMap_Master FOREIGN KEY (CampaignMasterId)
            REFERENCES dbo.CampaignMaster(CampaignMasterId),
        CONSTRAINT FK_CampaignContextMap_Operational FOREIGN KEY (OperationalCampaignId)
            REFERENCES dbo.ElectionCampaign(CampaignId)
    );
END;

/* Backfill planning campaigns that do not yet have an operational identity. */
DECLARE @MasterId INT;
DECLARE @CampaignName NVARCHAR(180);
DECLARE @ElectionType NVARCHAR(80);
DECLARE @StartDate DATETIME;
DECLARE @EndDate DATETIME;
DECLARE @Status NVARCHAR(40);
DECLARE @Description NVARCHAR(MAX);
DECLARE @IsActive BIT;
DECLARE @CreatedBy INT;
DECLARE @OperationalId INT;

DECLARE campaign_cursor CURSOR LOCAL FAST_FORWARD FOR
SELECT c.CampaignMasterId, c.CampaignName, e.ElectionType, c.StartDate,
       c.EndDate, c.Status, c.Description, c.IsActive, c.CreatedBy
FROM dbo.CampaignMaster c
INNER JOIN dbo.ElectionMaster e ON e.ElectionId = c.ElectionId
LEFT JOIN dbo.CampaignContextMap map ON map.CampaignMasterId = c.CampaignMasterId
WHERE c.IsDeleted = 0 AND map.CampaignContextMapId IS NULL;

OPEN campaign_cursor;
FETCH NEXT FROM campaign_cursor INTO
    @MasterId, @CampaignName, @ElectionType, @StartDate, @EndDate,
    @Status, @Description, @IsActive, @CreatedBy;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @OperationalId = NULL;

    SELECT TOP (1) @OperationalId = operational.CampaignId
    FROM dbo.ElectionCampaign operational
    LEFT JOIN dbo.CampaignContextMap existingMap
        ON existingMap.OperationalCampaignId = operational.CampaignId
    WHERE existingMap.CampaignContextMapId IS NULL
      AND operational.CampaignName = @CampaignName
    ORDER BY operational.CampaignId;

    IF @OperationalId IS NULL
    BEGIN
        INSERT dbo.ElectionCampaign
        (
            CampaignName, ElectionType, StateId, StartDate, EndDate,
            Status, Description, IsActive, CreatedDate, CreatedBy
        )
        VALUES
        (
            @CampaignName, @ElectionType, NULL, @StartDate, @EndDate,
            @Status, @Description, @IsActive, GETDATE(), @CreatedBy
        );

        SET @OperationalId = CONVERT(INT, SCOPE_IDENTITY());
    END;

    INSERT dbo.CampaignContextMap
    (
        CampaignMasterId, OperationalCampaignId, IsActive
    )
    VALUES
    (
        @MasterId, @OperationalId, @IsActive
    );

    FETCH NEXT FROM campaign_cursor INTO
        @MasterId, @CampaignName, @ElectionType, @StartDate, @EndDate,
        @Status, @Description, @IsActive, @CreatedBy;
END;

CLOSE campaign_cursor;
DEALLOCATE campaign_cursor;

EXEC(N'
CREATE OR ALTER VIEW dbo.vw_UnifiedCampaignContext
AS
    SELECT
        operational.CampaignId AS OperationalCampaignId,
        map.CampaignMasterId,
        COALESCE(master.CampaignName, operational.CampaignName) AS CampaignName,
        master.CampaignCode,
        master.CandidateName,
        master.ConstituencyName,
        master.ConstituencyNumber,
        operational.ElectionType,
        operational.StartDate,
        operational.EndDate,
        operational.Status,
        operational.IsActive,
        CAST(CASE WHEN map.CampaignContextMapId IS NULL THEN 0 ELSE 1 END AS BIT) AS IsUnified
    FROM dbo.ElectionCampaign operational
    LEFT JOIN dbo.CampaignContextMap map
        ON map.OperationalCampaignId = operational.CampaignId
       AND map.IsActive = 1
    LEFT JOIN dbo.CampaignMaster master
        ON master.CampaignMasterId = map.CampaignMasterId
       AND master.IsDeleted = 0;
');

COMMIT TRANSACTION;
