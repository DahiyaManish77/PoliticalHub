/*
    PoliticalLeaderPortal enterprise public-home seed
    Target DB: PoliticalLeaderPortalDb

    Purpose:
    - Keeps mega menu and hero slider database-driven.
    - Seeds enterprise-style public menu groups and one active hero slide.
    - Does not change schema or EDMX.

    Run this script in SQL Server Management Studio against your project database.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DECLARE @Now DATETIME = GETDATE();
DECLARE @SystemUser INT = 1;

UPDATE dbo.WebsiteSetting
SET
    WebsiteName = 'Sangeet Som',
    WebsiteTagline = 'Public Service Portal',
    DefaultLanguage = 'en',
    ModifiedBy = @SystemUser,
    ModifiedDate = @Now,
    IsActive = 1
WHERE WebsiteSettingId = 1;

IF NOT EXISTS (SELECT 1 FROM dbo.WebsiteSetting WHERE WebsiteSettingId = 1)
BEGIN
    SET IDENTITY_INSERT dbo.WebsiteSetting ON;

    INSERT INTO dbo.WebsiteSetting
    (
        WebsiteSettingId, WebsiteName, WebsiteTagline, WebsiteLogoPath,
        FaviconPath, DefaultMetaTitle, DefaultMetaDescription, DefaultMetaKeywords,
        GoogleAnalyticsCode, GoogleSearchConsoleCode, DefaultLanguage,
        CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, IsActive
    )
    VALUES
    (
        1, 'Sangeet Som', 'Public Service Portal', '~/Content/images/logo.png',
        NULL, 'Sangeet Som | Public Service Portal',
        'Official public service, development work, news, events and citizen connect portal.',
        'Sangeet Som, public service, constituency, development, news, events',
        NULL, NULL, 'en',
        @SystemUser, @Now, NULL, NULL, 1
    );

    SET IDENTITY_INSERT dbo.WebsiteSetting OFF;
END;

DECLARE @Menu TABLE
(
    MenuName NVARCHAR(200),
    MenuDescription NVARCHAR(MAX),
    ControllerName NVARCHAR(100),
    ActionName NVARCHAR(100),
    CustomUrl NVARCHAR(500),
    IconClass NVARCHAR(100),
    DisplayOrder INT,
    ParentName NVARCHAR(200) NULL,
    HasMegaMenu BIT,
    IsClickable BIT
);

INSERT INTO @Menu
(
    MenuName, MenuDescription, ControllerName, ActionName, CustomUrl,
    IconClass, DisplayOrder, ParentName, HasMegaMenu, IsClickable
)
VALUES
('Home', 'Homepage, live updates and featured public communication.', 'Home', 'Index', NULL, 'bi bi-house-door', 10, NULL, 0, 1),
('About Leader', 'Biography, political journey, vision, mission and achievements.', NULL, NULL, NULL, 'bi bi-person-badge', 20, NULL, 1, 0),
('Biography', 'Leader profile and public life.', 'AboutLeader', 'Biography', NULL, 'bi bi-person-lines-fill', 21, 'About Leader', 0, 1),
('Vision & Mission', 'Development vision, priorities and commitments.', 'AboutLeader', 'Vision', NULL, 'bi bi-bullseye', 22, 'About Leader', 0, 1),
('Achievements', 'Work completed and recognitions.', 'AboutLeader', 'Achievements', NULL, 'bi bi-trophy', 23, 'About Leader', 0, 1),
('Public Updates', 'News, press releases and media coverage.', NULL, NULL, NULL, 'bi bi-broadcast', 30, NULL, 1, 0),
('News', 'Latest public updates and announcements.', 'News', 'Index', NULL, 'bi bi-newspaper', 31, 'Public Updates', 0, 1),
('Media Coverage', 'Coverage from public events and media reports.', 'PublicMediaCoverage', 'Index', NULL, 'bi bi-camera-reels', 32, 'Public Updates', 0, 1),
('Events', 'Public meetings, tours and upcoming programs.', 'Event', 'Index', NULL, 'bi bi-calendar-event', 40, NULL, 1, 1),
('Gallery', 'Photo moments, events and public life.', 'Gallery', 'Index', NULL, 'bi bi-images', 50, NULL, 1, 1),
('Videos', 'Speeches, interviews and video updates.', 'Video', 'Index', NULL, 'bi bi-play-btn', 60, NULL, 1, 1),
('Documents', 'Manifesto, downloads and public documents.', 'Downloads', 'Index', NULL, 'bi bi-file-earmark-arrow-down', 70, NULL, 1, 1),
('Citizen Connect', 'Contact office, join campaign and public assistance.', NULL, NULL, '#contact-section', 'bi bi-people', 80, NULL, 1, 1);

MERGE dbo.MenuMaster AS Target
USING
(
    SELECT
        M.MenuName,
        M.MenuDescription,
        M.ControllerName,
        M.ActionName,
        M.CustomUrl,
        M.IconClass,
        M.DisplayOrder,
        Parent.MenuId AS ParentMenuId,
        M.HasMegaMenu,
        M.IsClickable,
        CASE WHEN Parent.MenuId IS NULL THEN 0 ELSE 1 END AS MenuLevel
    FROM @Menu M
    LEFT JOIN dbo.MenuMaster Parent
        ON Parent.MenuName = M.ParentName
) AS Source
ON Target.MenuName = Source.MenuName
WHEN MATCHED THEN
    UPDATE SET
        Target.ParentMenuId = Source.ParentMenuId,
        Target.MenuDescription = Source.MenuDescription,
        Target.AreaName = NULL,
        Target.ControllerName = Source.ControllerName,
        Target.ActionName = Source.ActionName,
        Target.RouteValues = NULL,
        Target.CustomUrl = Source.CustomUrl,
        Target.MenuType = 'Navigation',
        Target.IconClass = Source.IconClass,
        Target.CssClass = NULL,
        Target.DisplayOrder = Source.DisplayOrder,
        Target.IsActive = 1,
        Target.ShowOnHome = 1,
        Target.ShowInAdminSidebar = 0,
        Target.OpenInNewTab = 0,
        Target.IsClickable = Source.IsClickable,
        Target.HasMegaMenu = Source.HasMegaMenu,
        Target.PageTitle = Source.MenuName,
        Target.MetaDescription = Source.MenuDescription,
        Target.ModifiedBy = @SystemUser,
        Target.ModifiedDate = @Now,
        Target.MenuLevel = Source.MenuLevel,
        Target.ShowInFooter = 1,
        Target.ShowInQuickLinks = 1,
        Target.IsSystemMenu = 0
WHEN NOT MATCHED BY TARGET THEN
    INSERT
    (
        ParentMenuId, MenuName, MenuDescription, AreaName, ControllerName,
        ActionName, RouteValues, CustomUrl, MenuType, IconClass, CssClass,
        DisplayOrder, IsActive, ShowOnHome, ShowInAdminSidebar, OpenInNewTab,
        IsClickable, HasMegaMenu, PageTitle, MetaDescription, CreatedBy,
        CreatedDate, ModifiedBy, ModifiedDate, MenuLevel, ShowInFooter,
        ShowInQuickLinks, IsSystemMenu
    )
    VALUES
    (
        Source.ParentMenuId, Source.MenuName, Source.MenuDescription, NULL, Source.ControllerName,
        Source.ActionName, NULL, Source.CustomUrl, 'Navigation', Source.IconClass, NULL,
        Source.DisplayOrder, 1, 1, 0, 0,
        Source.IsClickable, Source.HasMegaMenu, Source.MenuName, Source.MenuDescription, @SystemUser,
        @Now, NULL, NULL, Source.MenuLevel, 1,
        1, 0
    );

IF EXISTS (SELECT 1 FROM dbo.HeroSlider WHERE DisplayOrder = 1)
BEGIN
    UPDATE dbo.HeroSlider
    SET
        Title = 'Development, public service and constituency updates',
        SubTitle = 'Public Leadership Portal',
        Description = 'Latest news, events, gallery updates and citizen connect sections are ready to publish dynamically from admin.',
        ButtonText = 'Contact Office',
        ButtonUrl = '#contact-section',
        ButtonText2 = 'Latest News',
        ButtonUrl2 = '/News',
        TemplateType = 'template-enterprise',
        OverlayType = 'gradient',
        SliderTransition = 'fade',
        TitleAnimation = 'animate__fadeInUp',
        SubTitleAnimation = 'animate__fadeInDown',
        DescriptionAnimation = 'animate__fadeInUp',
        ButtonAnimation = 'animate__fadeInUp',
        TextAlignment = 'right',
        OverlayOpacity = 0.45,
        BackgroundColor = '#164c85',
        ShowLeaderImage = 0,
        ShowOverlay = 1,
        ShowButtons = 1,
        IsActive = 1,
        ModifiedBy = @SystemUser,
        ModifiedDate = @Now
    WHERE DisplayOrder = 1;
END
ELSE
BEGIN
    INSERT INTO dbo.HeroSlider
    (
        Title, SubTitle, Description, ImagePath, ButtonText, ButtonUrl,
        DisplayOrder, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, IsActive,
        ButtonText2, ButtonUrl2, MobileImagePath, TemplateType, OverlayType,
        SliderTransition, TitleAnimation, SubTitleAnimation, DescriptionAnimation,
        ButtonAnimation, TextAlignment, OverlayOpacity, BackgroundImagePath,
        LeaderImagePath, BackgroundColor, LeaderImagePosition, ShowLeaderImage,
        ShowOverlay, ShowButtons
    )
    VALUES
    (
        'Development, public service and constituency updates',
        'Public Leadership Portal',
        'Latest news, events, gallery updates and citizen connect sections are ready to publish dynamically from admin.',
        '~/Uploads/HeroSlider/hero_639169675952576831.png',
        'Contact Office',
        '#contact-section',
        1, @SystemUser, @Now, NULL, NULL, 1,
        'Latest News', '/News', NULL, 'template-enterprise', 'gradient',
        'fade', 'animate__fadeInUp', 'animate__fadeInDown', 'animate__fadeInUp',
        'animate__fadeInUp', 'right', 0.45, NULL,
        NULL, '#164c85', 'right', 0,
        1, 1
    );
END;

COMMIT TRANSACTION;

SET NOCOUNT OFF;
