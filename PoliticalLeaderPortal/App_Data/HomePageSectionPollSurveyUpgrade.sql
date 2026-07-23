/*
    Adds the Poll / Survey homepage section to the CMS section manager.
    Run against the PoliticalLeaderPortal database if you seed production manually.
*/

SET NOCOUNT ON;

IF OBJECT_ID('dbo.HomePageSection', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.HomePageSection WHERE SectionKey = 'PollSurvey')
    BEGIN
        INSERT INTO dbo.HomePageSection
        (
            SectionKey,
            SectionName,
            Description,
            RenderType,
            ControllerName,
            ActionName,
            PartialViewPath,
            DisplayOrder,
            IsVisible,
            IsSystem,
            UpdatedDate
        )
        VALUES
        (
            'PollSurvey',
            'Poll / Survey',
            'Public feedback and survey callout.',
            'Action',
            'Layout',
            'PollSurvey',
            NULL,
            100,
            1,
            1,
            GETDATE()
        );
    END;

    UPDATE dbo.HomePageSection
    SET DisplayOrder = CASE SectionKey
        WHEN 'Downloads' THEN 110
        WHEN 'AppDownload' THEN 120
        WHEN 'JoinUsCTA' THEN 130
        WHEN 'ContactCTA' THEN 140
        ELSE DisplayOrder
    END,
    UpdatedDate = GETDATE()
    WHERE SectionKey IN ('Downloads', 'AppDownload', 'JoinUsCTA', 'ContactCTA');
END;

SET NOCOUNT OFF;
