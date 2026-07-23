/*
    Election War Room - simple support script
    Target: PoliticalLeaderPortalDb

    Purpose:
    - Keeps the database-first model unchanged.
    - Adds only practical indexes for dashboard/listing performance.
    - Adds sidebar/menu safety for Election War Room.
*/

IF OBJECT_ID('dbo.EventMaster', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EventMaster_IsActive_EventDate_Status' AND object_id = OBJECT_ID('dbo.EventMaster'))
        CREATE INDEX IX_EventMaster_IsActive_EventDate_Status ON dbo.EventMaster(IsActive, EventDate, Status);
END;

IF OBJECT_ID('dbo.EventTask', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EventTask_IsActive_Status_DueDate' AND object_id = OBJECT_ID('dbo.EventTask'))
        CREATE INDEX IX_EventTask_IsActive_Status_DueDate ON dbo.EventTask(IsActive, Status, DueDate);
END;

IF OBJECT_ID('dbo.EventTeam', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EventTeam_IsActive_Status_Priority' AND object_id = OBJECT_ID('dbo.EventTeam'))
        CREATE INDEX IX_EventTeam_IsActive_Status_Priority ON dbo.EventTeam(IsActive, Status, Priority);
END;

IF OBJECT_ID('dbo.ElectionBooth', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ElectionBooth_IsActive_District_Priority' AND object_id = OBJECT_ID('dbo.ElectionBooth'))
        CREATE INDEX IX_ElectionBooth_IsActive_District_Priority ON dbo.ElectionBooth(IsActive, District, Priority);
END;

IF OBJECT_ID('dbo.ElectionBoothVisit', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ElectionBoothVisit_IsActive_VisitDate_Status' AND object_id = OBJECT_ID('dbo.ElectionBoothVisit'))
        CREATE INDEX IX_ElectionBoothVisit_IsActive_VisitDate_Status ON dbo.ElectionBoothVisit(IsActive, VisitDate, VisitStatus);
END;

IF OBJECT_ID('dbo.JanSampark', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_JanSampark_IsActive_Status_Priority' AND object_id = OBJECT_ID('dbo.JanSampark'))
        CREATE INDEX IX_JanSampark_IsActive_Status_Priority ON dbo.JanSampark(IsActive, Status, Priority);
END;

IF OBJECT_ID('dbo.EventExpense', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EventExpense_IsActive_ExpenseDate_Status' AND object_id = OBJECT_ID('dbo.EventExpense'))
        CREATE INDEX IX_EventExpense_IsActive_ExpenseDate_Status ON dbo.EventExpense(IsActive, ExpenseDate, ExpenseStatus);
END;

IF OBJECT_ID('dbo.CampaignAlert', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CampaignAlert_IsActive_Status_Severity' AND object_id = OBJECT_ID('dbo.CampaignAlert'))
        CREATE INDEX IX_CampaignAlert_IsActive_Status_Severity ON dbo.CampaignAlert(IsActive, AlertStatus, Severity);
END;

IF OBJECT_ID('dbo.MenuMaster', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.MenuMaster
        WHERE MenuName = 'Election War Room'
          AND AreaName = 'Admin'
          AND ControllerName = 'ElectionWarRoom'
          AND ActionName = 'Index'
    )
    BEGIN
        INSERT INTO dbo.MenuMaster
        (
            ParentMenuId, MenuName, MenuDescription, AreaName, ControllerName, ActionName,
            MenuType, IconClass, DisplayOrder, IsActive, ShowOnHome, ShowInAdminSidebar,
            OpenInNewTab, IsClickable, HasMegaMenu, CreatedDate, MenuLevel, ShowInFooter,
            ShowInQuickLinks, IsSystemMenu
        )
        VALUES
        (
            NULL, 'Election War Room', 'Election campaign command dashboard',
            'Admin', 'ElectionWarRoom', 'Index', 'Sidebar', 'fas fa-bullhorn',
            125, 1, 0, 1, 0, 1, 0, GETDATE(), 0, 0, 0, 0
        );
    END
    ELSE
    BEGIN
        UPDATE dbo.MenuMaster
        SET IsActive = 1,
            ShowInAdminSidebar = 1,
            ShowOnHome = 0,
            IconClass = 'fas fa-bullhorn',
            DisplayOrder = 125,
            ModifiedDate = GETDATE()
        WHERE MenuName = 'Election War Room'
          AND AreaName = 'Admin'
          AND ControllerName = 'ElectionWarRoom'
          AND ActionName = 'Index';
    END;
END;
