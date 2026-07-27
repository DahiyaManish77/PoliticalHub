/*
    Centralized Admin Navigation
    ----------------------------
    Keeps every existing destination and MenuId intact while grouping the
    sidebar into a compact campaign-operations information architecture.
    Safe to run repeatedly.
*/
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @CampaignId int;
DECLARE @ConstituencyId int;
DECLARE @PeopleId int;
DECLARE @CitizenConnectId int;
DECLARE @FieldOperationsId int;
DECLARE @CommunicationId int;
DECLARE @WebsiteCmsId int;
DECLARE @AdministrationId int;

SELECT @CampaignId = MenuId FROM MenuMaster WHERE MenuName = N'Election War Room' AND ShowInAdminSidebar = 1;
IF @CampaignId IS NULL
    SELECT @CampaignId = MenuId FROM MenuMaster WHERE MenuName = N'Campaign Command' AND ShowInAdminSidebar = 1;

UPDATE MenuMaster
SET MenuName = N'Campaign Command',
    MenuDescription = N'Campaign planning, intelligence and command-centre operations',
    IconClass = N'bi bi-speedometer2',
    DisplayOrder = 20,
    ModifiedDate = GETDATE()
WHERE MenuId = @CampaignId;

SELECT @ConstituencyId = MenuId FROM MenuMaster WHERE MenuName = N'Constituency Management' AND ShowInAdminSidebar = 1;
IF @ConstituencyId IS NULL
    SELECT @ConstituencyId = MenuId FROM MenuMaster WHERE MenuName = N'Constituency' AND ShowInAdminSidebar = 1;
UPDATE MenuMaster
SET MenuName = N'Constituency',
    IconClass = N'bi bi-map',
    DisplayOrder = 30,
    ModifiedDate = GETDATE()
WHERE MenuId = @ConstituencyId;

SELECT @PeopleId = MenuId FROM MenuMaster WHERE MenuName = N'People & Volunteers' AND ShowInAdminSidebar = 1;
UPDATE MenuMaster
SET MenuName = N'People & Organization',
    IconClass = N'bi bi-people',
    DisplayOrder = 40,
    ModifiedDate = GETDATE()
WHERE MenuId = @PeopleId;

SELECT @CitizenConnectId = MenuId FROM MenuMaster WHERE MenuName = N'Citizen Connect' AND ShowInAdminSidebar = 1;
UPDATE MenuMaster
SET DisplayOrder = 60,
    IconClass = N'bi bi-chat-square-heart',
    ModifiedDate = GETDATE()
WHERE MenuId = @CitizenConnectId;

UPDATE MenuMaster
SET ParentMenuId = @CitizenConnectId,
    MenuLevel = 1,
    ModifiedDate = GETDATE()
WHERE MenuName = N'Jan Sampark';

IF NOT EXISTS (SELECT 1 FROM MenuMaster WHERE MenuName = N'Field Operations' AND MenuType = N'AdminGroup')
BEGIN
    INSERT MenuMaster
    (
        MenuName, MenuDescription, MenuType, IconClass, DisplayOrder,
        IsActive, ShowOnHome, ShowInAdminSidebar, OpenInNewTab,
        IsClickable, HasMegaMenu, CreatedDate, MenuLevel,
        ShowInFooter, ShowInQuickLinks, IsSystemMenu
    )
    VALUES
    (
        N'Field Operations', N'Events, teams, logistics and ground execution',
        N'AdminGroup', N'bi bi-calendar2-check', 50,
        1, 0, 1, 0, 0, 0, GETDATE(), 0, 0, 0, 1
    );
END;
SELECT @FieldOperationsId = MenuId FROM MenuMaster WHERE MenuName = N'Field Operations' AND MenuType = N'AdminGroup';

IF NOT EXISTS (SELECT 1 FROM MenuMaster WHERE MenuName = N'Communication & Media' AND MenuType = N'AdminGroup')
BEGIN
    INSERT MenuMaster
    (
        MenuName, MenuDescription, MenuType, IconClass, DisplayOrder,
        IsActive, ShowOnHome, ShowInAdminSidebar, OpenInNewTab,
        IsClickable, HasMegaMenu, CreatedDate, MenuLevel,
        ShowInFooter, ShowInQuickLinks, IsSystemMenu
    )
    VALUES
    (
        N'Communication & Media', N'News, media, galleries and campaign communication',
        N'AdminGroup', N'bi bi-megaphone', 70,
        1, 0, 1, 0, 0, 0, GETDATE(), 0, 0, 0, 1
    );
END;
SELECT @CommunicationId = MenuId FROM MenuMaster WHERE MenuName = N'Communication & Media' AND MenuType = N'AdminGroup';

IF NOT EXISTS (SELECT 1 FROM MenuMaster WHERE MenuName = N'Website CMS' AND MenuType = N'AdminGroup')
BEGIN
    INSERT MenuMaster
    (
        MenuName, MenuDescription, MenuType, IconClass, DisplayOrder,
        IsActive, ShowOnHome, ShowInAdminSidebar, OpenInNewTab,
        IsClickable, HasMegaMenu, CreatedDate, MenuLevel,
        ShowInFooter, ShowInQuickLinks, IsSystemMenu
    )
    VALUES
    (
        N'Website CMS', N'Public website content and presentation',
        N'AdminGroup', N'bi bi-window-stack', 80,
        1, 0, 1, 0, 0, 0, GETDATE(), 0, 0, 0, 1
    );
END;
SELECT @WebsiteCmsId = MenuId FROM MenuMaster WHERE MenuName = N'Website CMS' AND MenuType = N'AdminGroup';

IF NOT EXISTS (SELECT 1 FROM MenuMaster WHERE MenuName = N'Administration' AND MenuType = N'AdminGroup')
BEGIN
    INSERT MenuMaster
    (
        MenuName, MenuDescription, MenuType, IconClass, DisplayOrder,
        IsActive, ShowOnHome, ShowInAdminSidebar, OpenInNewTab,
        IsClickable, HasMegaMenu, CreatedDate, MenuLevel,
        ShowInFooter, ShowInQuickLinks, IsSystemMenu
    )
    VALUES
    (
        N'Administration', N'Access control, configuration and system operations',
        N'AdminGroup', N'bi bi-gear', 90,
        1, 0, 1, 0, 0, 0, GETDATE(), 0, 0, 0, 1
    );
END;
SELECT @AdministrationId = MenuId FROM MenuMaster WHERE MenuName = N'Administration' AND MenuType = N'AdminGroup';

/* Campaign planning and intelligence */
UPDATE MenuMaster
SET ParentMenuId = @CampaignId, MenuLevel = 1, ModifiedDate = GETDATE()
WHERE MenuName IN
(
    N'Election & Campaign Masters', N'Polls & Surveys',
    N'Campaign Alerts', N'Today''s Schedule'
);

/* Constituency, voter and booth structure */
UPDATE MenuMaster
SET ParentMenuId = @ConstituencyId, MenuLevel = 1, ModifiedDate = GETDATE()
WHERE MenuName IN
(
    N'Mera Kshetra Content', N'Voter Management',
    N'Election Booths', N'Booth Visits'
);

/* Ground execution */
UPDATE MenuMaster
SET ParentMenuId = @FieldOperationsId, MenuLevel = 1, ModifiedDate = GETDATE()
WHERE MenuName IN
(
    N'Events', N'Tasks', N'Teams', N'Vehicles', N'Attendance',
    N'Expenses', N'Village Turnout', N'Food Management',
    N'Borrowed Assets', N'Appreciation'
);

/* Communication */
UPDATE MenuMaster
SET ParentMenuId = @CommunicationId, MenuLevel = 1, ModifiedDate = GETDATE()
WHERE MenuName IN
(
    N'News Management', N'Gallery Management', N'Media',
    N'Video Meetings', N'Documents'
);

/* Public website configuration */
UPDATE MenuMaster
SET ParentMenuId = @WebsiteCmsId, MenuLevel = 1, ModifiedDate = GETDATE()
WHERE MenuName IN
(
    N'Hero Slider', N'Website Settings', N'Home Members',
    N'App Download Settings'
);

/* System administration */
UPDATE MenuMaster
SET ParentMenuId = @AdministrationId, MenuLevel = 1, ModifiedDate = GETDATE()
WHERE MenuName IN
(
    N'Menu Management', N'Role Permissions', N'Voter Backup Settings'
);

/* Stable ordering inside each group */
;WITH Ordered AS
(
    SELECT MenuId,
           ROW_NUMBER() OVER
           (
               PARTITION BY ParentMenuId
               ORDER BY DisplayOrder, MenuName
           ) * 10 AS NewDisplayOrder
    FROM MenuMaster
    WHERE ShowInAdminSidebar = 1
      AND ParentMenuId IS NOT NULL
)
UPDATE menu
SET DisplayOrder = ordered.NewDisplayOrder
FROM MenuMaster menu
INNER JOIN Ordered ordered ON ordered.MenuId = menu.MenuId;

UPDATE MenuMaster
SET ParentMenuId = NULL, MenuLevel = 0, DisplayOrder = 10
WHERE MenuName = N'Dashboard' AND ShowInAdminSidebar = 1;

COMMIT TRANSACTION;
