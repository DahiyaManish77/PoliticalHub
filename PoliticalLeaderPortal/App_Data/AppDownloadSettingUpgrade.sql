IF OBJECT_ID('dbo.AppDownloadSetting', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AppDownloadSetting
    (
        AppDownloadSettingId INT NOT NULL PRIMARY KEY,
        KickerText NVARCHAR(120) NOT NULL,
        HeadingText NVARCHAR(180) NOT NULL,
        SubHeadingText NVARCHAR(220) NULL,
        GooglePlayUrl NVARCHAR(600) NOT NULL,
        AppleAppStoreUrl NVARCHAR(600) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_AppDownloadSetting_IsActive DEFAULT(1),
        CreatedDate DATETIME NOT NULL CONSTRAINT DF_AppDownloadSetting_CreatedDate DEFAULT(GETDATE()),
        ModifiedDate DATETIME NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.AppDownloadSetting WHERE AppDownloadSettingId = 1)
BEGIN
    INSERT INTO dbo.AppDownloadSetting
    (
        AppDownloadSettingId, KickerText, HeadingText, SubHeadingText,
        GooglePlayUrl, AppleAppStoreUrl, IsActive, CreatedDate
    )
    VALUES
    (
        1, 'Bharatiya Janata Party', 'Download The App Now',
        'Stay connected with organisation updates, campaigns and public outreach.',
        'https://play.google.com/store/search?q=Bharatiya%20Janata%20Party&c=apps',
        'https://apps.apple.com/in/search?term=Bharatiya%20Janata%20Party',
        1, GETDATE()
    );
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.MenuMaster
    WHERE AreaName = 'Admin'
      AND ControllerName = 'AppDownloadSetting'
      AND ActionName = 'Index'
)
BEGIN
    INSERT INTO dbo.MenuMaster
    (
        ParentMenuId, MenuName, MenuDescription, AreaName, ControllerName, ActionName,
        CustomUrl, MenuType, IconClass, CssClass, DisplayOrder, IsActive, ShowOnHome,
        ShowInAdminSidebar, OpenInNewTab, IsClickable, HasMegaMenu, PageTitle,
        MetaDescription, CreatedBy, CreatedDate, MenuLevel, ShowInFooter,
        ShowInQuickLinks, IsSystemMenu
    )
    VALUES
    (
        NULL, 'App Download Settings', 'Manage home page app download banner links.',
        'Admin', 'AppDownloadSetting', 'Index', NULL, 'Admin', 'fas fa-mobile-screen-button',
        NULL, 143, 1, 0, 1, 0, 1, 0, 'App Download Settings',
        'Manage Google Play and Apple App Store app links.', NULL, GETDATE(), 0, 0, 0, 1
    );
END
GO
