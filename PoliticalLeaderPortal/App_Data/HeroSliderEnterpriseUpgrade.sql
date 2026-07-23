/*
    Hero Slider enterprise upgrade
    Target database: PoliticalLeaderPortalDb
    Target table   : dbo.HeroSlider

    This script is idempotent. Run it after restoring the database or before
    using admin-managed hero videos and per-slide height control.
*/

IF OBJECT_ID('dbo.HeroSlider', 'U') IS NULL
BEGIN
    RAISERROR('dbo.HeroSlider table was not found in the selected database.', 16, 1);
    RETURN;
END;

IF COL_LENGTH('dbo.HeroSlider', 'IsVideoSlide') IS NULL
BEGIN
    ALTER TABLE dbo.HeroSlider
    ADD IsVideoSlide BIT NOT NULL
        CONSTRAINT DF_HeroSlider_IsVideoSlide DEFAULT (0);
END;

IF COL_LENGTH('dbo.HeroSlider', 'VideoSourceType') IS NULL
BEGIN
    ALTER TABLE dbo.HeroSlider
    ADD VideoSourceType NVARCHAR(30) NULL;
END;

IF COL_LENGTH('dbo.HeroSlider', 'VideoUrl') IS NULL
BEGIN
    ALTER TABLE dbo.HeroSlider
    ADD VideoUrl NVARCHAR(1000) NULL;
END;

IF COL_LENGTH('dbo.HeroSlider', 'VideoPath') IS NULL
BEGIN
    ALTER TABLE dbo.HeroSlider
    ADD VideoPath NVARCHAR(500) NULL;
END;

IF COL_LENGTH('dbo.HeroSlider', 'VideoAutoplay') IS NULL
BEGIN
    ALTER TABLE dbo.HeroSlider
    ADD VideoAutoplay BIT NOT NULL
        CONSTRAINT DF_HeroSlider_VideoAutoplay DEFAULT (1);
END;

IF COL_LENGTH('dbo.HeroSlider', 'VideoMuted') IS NULL
BEGIN
    ALTER TABLE dbo.HeroSlider
    ADD VideoMuted BIT NOT NULL
        CONSTRAINT DF_HeroSlider_VideoMuted DEFAULT (1);
END;

IF COL_LENGTH('dbo.HeroSlider', 'VideoLoop') IS NULL
BEGIN
    ALTER TABLE dbo.HeroSlider
    ADD VideoLoop BIT NOT NULL
        CONSTRAINT DF_HeroSlider_VideoLoop DEFAULT (1);
END;

IF COL_LENGTH('dbo.HeroSlider', 'HeroHeightCss') IS NULL
BEGIN
    ALTER TABLE dbo.HeroSlider
    ADD HeroHeightCss NVARCHAR(40) NULL;
END;

EXEC(N'
UPDATE dbo.HeroSlider
SET VideoSourceType = ISNULL(NULLIF(VideoSourceType, ''''), ''Image''),
    HeroHeightCss = CASE
        WHEN HeroHeightCss IS NULL OR HeroHeightCss = '''' OR HeroHeightCss = ''520px''
            THEN ''440px''
        ELSE HeroHeightCss
    END;
');

EXEC(N'
SELECT
    HeroSliderId,
    Title,
    IsVideoSlide,
    VideoSourceType,
    VideoUrl,
    VideoPath,
    VideoAutoplay,
    VideoMuted,
    VideoLoop,
    HeroHeightCss
FROM dbo.HeroSlider
ORDER BY DisplayOrder, HeroSliderId;
');
