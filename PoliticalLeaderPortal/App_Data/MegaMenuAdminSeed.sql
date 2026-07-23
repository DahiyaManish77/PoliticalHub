/*
    Mega menu and admin login seed
    Target database: PoliticalLeaderPortalDb
*/

IF OBJECT_ID('dbo.MenuMaster', 'U') IS NULL
BEGIN
    RAISERROR('dbo.MenuMaster table was not found.', 16, 1);
    RETURN;
END;

IF OBJECT_ID('dbo.ApplicationRole', 'U') IS NULL OR OBJECT_ID('dbo.ApplicationUser', 'U') IS NULL
BEGIN
    RAISERROR('ApplicationRole/ApplicationUser tables were not found.', 16, 1);
    RETURN;
END;

DECLARE @Now DATETIME = GETDATE();
DECLARE @SuperAdminRoleId INT;

IF EXISTS (SELECT 1 FROM dbo.MenuMaster WHERE MenuName = 'About Leader' AND ParentMenuId IS NULL)
BEGIN
    UPDATE dbo.MenuMaster
    SET MenuName = 'About Som',
        AreaName = '',
        ControllerName = 'AboutLeader',
        ActionName = 'Biography',
        MenuType = 'MegaMenu',
        IconClass = 'fas fa-user-tie',
        DisplayOrder = 10,
        ShowOnHome = 1,
        ShowInAdminSidebar = 0,
        HasMegaMenu = 1,
        IsClickable = 1,
        IsActive = 1,
        ModifiedDate = @Now
    WHERE MenuName = 'About Leader'
      AND ParentMenuId IS NULL
      AND NOT EXISTS (SELECT 1 FROM dbo.MenuMaster WHERE MenuName = 'About Som' AND ParentMenuId IS NULL);
END;

IF EXISTS (SELECT 1 FROM dbo.MenuMaster WHERE MenuName = 'About Leader' AND ParentMenuId IS NULL)
BEGIN
    UPDATE dbo.MenuMaster
    SET IsActive = 0,
        ShowOnHome = 0,
        ShowInAdminSidebar = 0,
        ModifiedDate = @Now
    WHERE MenuName = 'About Leader'
      AND ParentMenuId IS NULL;

    UPDATE child
    SET child.IsActive = 0,
        child.ShowOnHome = 0,
        child.ShowInAdminSidebar = 0,
        child.ModifiedDate = @Now
    FROM dbo.MenuMaster child
    INNER JOIN dbo.MenuMaster parent ON child.ParentMenuId = parent.MenuId
    WHERE parent.MenuName = 'About Leader'
      AND parent.ParentMenuId IS NULL;
END;

SELECT @SuperAdminRoleId = RoleId
FROM dbo.ApplicationRole
WHERE RoleName = 'SuperAdmin';

IF @SuperAdminRoleId IS NULL
BEGIN
    INSERT INTO dbo.ApplicationRole
    (
        RoleName,
        Description,
        CreatedDate,
        IsActive
    )
    VALUES
    (
        'SuperAdmin',
        'Full administrative access',
        @Now,
        1
    );

    SET @SuperAdminRoleId = SCOPE_IDENTITY();
END;

IF NOT EXISTS (SELECT 1 FROM dbo.ApplicationUser WHERE EmailAddress = 'admin@politicalportal.com')
BEGIN
    INSERT INTO dbo.ApplicationUser
    (
        RoleId,
        FullName,
        EmailAddress,
        MobileNumber,
        PasswordHash,
        ProfilePhotoPath,
        LastLoginDate,
        CreatedBy,
        CreatedDate,
        ModifiedBy,
        ModifiedDate,
        IsActive
    )
    VALUES
    (
        @SuperAdminRoleId,
        'Portal Administrator',
        'admin@politicalportal.com',
        NULL,
        'PBKDF2$120000$M1WOJvvYrV1r161exaIqAg==$hCMEk9/Mqa2qMfj+inLaw5yeklonpgTSN/37XCuncts=',
        NULL,
        NULL,
        NULL,
        @Now,
        NULL,
        NULL,
        1
    );
END
ELSE
BEGIN
    UPDATE dbo.ApplicationUser
    SET RoleId = @SuperAdminRoleId,
        FullName = 'Portal Administrator',
        PasswordHash = 'PBKDF2$120000$M1WOJvvYrV1r161exaIqAg==$hCMEk9/Mqa2qMfj+inLaw5yeklonpgTSN/37XCuncts=',
        ModifiedDate = @Now,
        IsActive = 1
    WHERE EmailAddress = 'admin@politicalportal.com';
END;

DECLARE @Seed TABLE
(
    MenuName NVARCHAR(200),
    ParentName NVARCHAR(200) NULL,
    AreaName NVARCHAR(100) NULL,
    ControllerName NVARCHAR(100) NULL,
    ActionName NVARCHAR(100) NULL,
    CustomUrl NVARCHAR(500) NULL,
    MenuType NVARCHAR(100),
    IconClass NVARCHAR(100),
    DisplayOrder INT,
    ShowInMegaMenu BIT,
    ShowInSidebar BIT,
    HasMegaMenu BIT,
    IsClickable BIT
);

INSERT INTO @Seed
(
    MenuName,
    ParentName,
    AreaName,
    ControllerName,
    ActionName,
    CustomUrl,
    MenuType,
    IconClass,
    DisplayOrder,
    ShowInMegaMenu,
    ShowInSidebar,
    HasMegaMenu,
    IsClickable
)
VALUES
('About Som', NULL, '', 'AboutLeader', 'Biography', NULL, 'MegaMenu', 'fas fa-user-tie', 10, 1, 0, 1, 1),
('Biography', 'About Som', '', 'AboutLeader', 'Biography', NULL, 'MegaMenu', 'fas fa-id-card', 11, 1, 0, 0, 1),
('Vision', 'About Som', '', 'AboutLeader', 'Vision', NULL, 'MegaMenu', 'fas fa-bullseye', 12, 1, 0, 0, 1),
('Political Timeline', 'About Som', '', 'AboutLeader', 'Timeline', NULL, 'MegaMenu', 'fas fa-clock', 13, 1, 0, 0, 1),
('Public Work', 'About Som', '', 'AboutLeader', 'Achievements', NULL, 'MegaMenu', 'fas fa-award', 14, 1, 0, 0, 1),
('FAQs', 'About Som', '', 'AboutLeader', 'Faq', NULL, 'MegaMenu', 'fas fa-circle-question', 15, 1, 0, 0, 1),

('News', NULL, '', 'News', 'Index', NULL, 'MegaMenu', 'fas fa-newspaper', 20, 1, 0, 1, 1),
('News Updates', 'News', '', 'News', 'Index', NULL, 'MegaMenu', 'fas fa-rss', 21, 1, 0, 0, 1),
('Media Coverage', 'News', '', 'PublicMediaCoverage', 'Index', NULL, 'MegaMenu', 'fas fa-tv', 22, 1, 0, 0, 1),

('Public Connect', NULL, '', 'Event', 'Index', NULL, 'MegaMenu', 'fas fa-people-arrows', 30, 1, 0, 1, 1),
('Events', 'Public Connect', '', 'Event', 'Index', NULL, 'MegaMenu', 'fas fa-calendar-days', 31, 1, 0, 0, 1),
('Downloads', 'Public Connect', '', 'Downloads', 'Index', NULL, 'MegaMenu', 'fas fa-download', 32, 1, 0, 0, 1),
('Contact Us', 'Public Connect', '', NULL, NULL, '#contact-section', 'MegaMenu', 'fas fa-phone', 33, 1, 0, 0, 1),

('Gallery', NULL, '', 'Gallery', 'Index', NULL, 'MegaMenu', 'fas fa-images', 40, 1, 0, 1, 1),
('Photo Gallery', 'Gallery', '', 'Gallery', 'Index', NULL, 'MegaMenu', 'fas fa-camera', 41, 1, 0, 0, 1),
('Video Gallery', 'Gallery', '', 'Video', 'Index', NULL, 'MegaMenu', 'fas fa-video', 42, 1, 0, 0, 1),

('Dashboard', NULL, 'Admin', 'Dashboard', 'Index', NULL, 'Sidebar', 'fas fa-gauge-high', 100, 0, 1, 0, 1),
('Hero Slider', NULL, 'Admin', 'HeroSlider', 'Index', NULL, 'Sidebar', 'fas fa-images', 110, 0, 1, 0, 1),
('Menu Management', NULL, 'Admin', 'MenuCMS', 'Index', NULL, 'Sidebar', 'fas fa-bars-staggered', 120, 0, 1, 0, 1),
('Election War Room', NULL, 'Admin', 'ElectionWarRoom', 'Index', NULL, 'Sidebar', 'fas fa-bullhorn', 125, 0, 1, 0, 1),
('News Management', NULL, 'Admin', 'LatestNews', 'Index', NULL, 'Sidebar', 'fas fa-newspaper', 130, 0, 1, 0, 1),
('Events Management', NULL, 'Admin', 'UpcomingEvent', 'Index', NULL, 'Sidebar', 'fas fa-calendar-check', 140, 0, 0, 0, 1),
('Today''s Schedule', NULL, 'Admin', 'TodaySchedule', 'Index', NULL, 'Sidebar', 'fas fa-calendar-day', 141, 0, 1, 0, 1),
('Gallery Management', NULL, 'Admin', NULL, NULL, NULL, 'Sidebar', 'fas fa-photo-film', 150, 0, 1, 0, 0),
('Images', 'Gallery Management', 'Admin', 'GalleryImage', 'Index', NULL, 'Sidebar', 'fas fa-image', 151, 0, 1, 0, 1),
('Videos', 'Gallery Management', 'Admin', 'VideoGallery', 'Index', NULL, 'Sidebar', 'fas fa-video', 152, 0, 1, 0, 1),
('Website Settings', NULL, 'Admin', 'WebsiteSetting', 'Index', NULL, 'Sidebar', 'fas fa-gear', 160, 0, 1, 0, 1);

DECLARE @MenuName NVARCHAR(200),
        @ParentName NVARCHAR(200),
        @AreaName NVARCHAR(100),
        @ControllerName NVARCHAR(100),
        @ActionName NVARCHAR(100),
        @CustomUrl NVARCHAR(500),
        @MenuType NVARCHAR(100),
        @IconClass NVARCHAR(100),
        @DisplayOrder INT,
        @ShowInMegaMenu BIT,
        @ShowInSidebar BIT,
        @HasMegaMenu BIT,
        @IsClickable BIT,
        @ParentMenuId INT,
        @MenuLevel INT;

DECLARE menu_cursor CURSOR LOCAL FAST_FORWARD FOR
SELECT MenuName, ParentName, AreaName, ControllerName, ActionName, CustomUrl, MenuType, IconClass, DisplayOrder, ShowInMegaMenu, ShowInSidebar, HasMegaMenu, IsClickable
FROM @Seed
ORDER BY CASE WHEN ParentName IS NULL THEN 0 ELSE 1 END, DisplayOrder;

OPEN menu_cursor;

FETCH NEXT FROM menu_cursor INTO @MenuName, @ParentName, @AreaName, @ControllerName, @ActionName, @CustomUrl, @MenuType, @IconClass, @DisplayOrder, @ShowInMegaMenu, @ShowInSidebar, @HasMegaMenu, @IsClickable;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @ParentMenuId = NULL;
    SET @MenuLevel = 0;

    IF @ParentName IS NOT NULL
    BEGIN
        SELECT @ParentMenuId = MenuId, @MenuLevel = MenuLevel + 1
        FROM dbo.MenuMaster
        WHERE MenuName = @ParentName
          AND ParentMenuId IS NULL;
    END;

    IF EXISTS (SELECT 1 FROM dbo.MenuMaster WHERE MenuName = @MenuName AND ((@ParentMenuId IS NULL AND ParentMenuId IS NULL) OR ParentMenuId = @ParentMenuId))
    BEGIN
        UPDATE dbo.MenuMaster
        SET AreaName = @AreaName,
            ControllerName = @ControllerName,
            ActionName = @ActionName,
            CustomUrl = @CustomUrl,
            MenuType = @MenuType,
            IconClass = @IconClass,
            DisplayOrder = @DisplayOrder,
            IsActive = 1,
            ShowOnHome = @ShowInMegaMenu,
            ShowInAdminSidebar = @ShowInSidebar,
            HasMegaMenu = @HasMegaMenu,
            IsClickable = @IsClickable,
            OpenInNewTab = 0,
            MenuLevel = @MenuLevel,
            ModifiedDate = @Now
        WHERE MenuName = @MenuName
          AND ((@ParentMenuId IS NULL AND ParentMenuId IS NULL) OR ParentMenuId = @ParentMenuId);
    END
    ELSE
    BEGIN
        INSERT INTO dbo.MenuMaster
        (
            ParentMenuId,
            MenuName,
            MenuDescription,
            AreaName,
            ControllerName,
            ActionName,
            RouteValues,
            CustomUrl,
            MenuType,
            IconClass,
            CssClass,
            DisplayOrder,
            IsActive,
            ShowOnHome,
            ShowInAdminSidebar,
            OpenInNewTab,
            IsClickable,
            HasMegaMenu,
            PageTitle,
            MetaDescription,
            CreatedBy,
            CreatedDate,
            ModifiedBy,
            ModifiedDate,
            MenuLevel,
            ShowInFooter,
            ShowInQuickLinks,
            IsSystemMenu
        )
        VALUES
        (
            @ParentMenuId,
            @MenuName,
            NULL,
            @AreaName,
            @ControllerName,
            @ActionName,
            NULL,
            @CustomUrl,
            @MenuType,
            @IconClass,
            NULL,
            @DisplayOrder,
            1,
            @ShowInMegaMenu,
            @ShowInSidebar,
            0,
            @IsClickable,
            @HasMegaMenu,
            NULL,
            NULL,
            NULL,
            @Now,
            NULL,
            NULL,
            @MenuLevel,
            0,
            0,
            0
        );
    END;

    FETCH NEXT FROM menu_cursor INTO @MenuName, @ParentName, @AreaName, @ControllerName, @ActionName, @CustomUrl, @MenuType, @IconClass, @DisplayOrder, @ShowInMegaMenu, @ShowInSidebar, @HasMegaMenu, @IsClickable;
END;

CLOSE menu_cursor;
DEALLOCATE menu_cursor;

UPDATE dbo.MenuMaster
SET ShowInAdminSidebar = 0,
    IconClass = 'fas fa-home',
    AreaName = '',
    ControllerName = 'Home',
    ActionName = 'Index',
    ModifiedDate = @Now
WHERE MenuName = 'Home'
  AND ParentMenuId IS NULL;

UPDATE dbo.MenuMaster
SET ShowInAdminSidebar = 0,
    ShowOnHome = 1,
    AreaName = '',
    ControllerName = 'Video',
    ActionName = 'Index',
    IconClass = 'fas fa-video',
    ModifiedDate = @Now
WHERE MenuName = 'Videos'
  AND ParentMenuId IS NULL;

SELECT 'Admin Login' AS Item, 'admin@politicalportal.com' AS EmailAddress, 'Admin@123' AS Password;

SELECT MenuId, ParentMenuId, MenuName, AreaName, ControllerName, ActionName, DisplayOrder, IsActive, ShowOnHome AS ShowInMegaMenu, ShowInAdminSidebar AS ShowInSidebar
FROM dbo.MenuMaster
ORDER BY DisplayOrder, MenuId;
