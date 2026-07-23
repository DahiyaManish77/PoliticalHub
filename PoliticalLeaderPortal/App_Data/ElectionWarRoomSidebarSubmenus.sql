IF OBJECT_ID('dbo.MenuMaster', 'U') IS NOT NULL
BEGIN
    DECLARE @ElectionWarRoomMenuId INT;

    SELECT @ElectionWarRoomMenuId = MenuId
    FROM dbo.MenuMaster
    WHERE AreaName = 'Admin'
      AND ControllerName = 'ElectionWarRoom'
      AND ActionName = 'Index';

    IF @ElectionWarRoomMenuId IS NULL
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
            NULL, 'Election War Room', 'Election campaign command dashboard.',
            'Admin', 'ElectionWarRoom', 'Index', NULL, 'Admin', 'fas fa-bullhorn',
            NULL, 125, 1, 0, 1, 0, 1, 0, 'Election War Room',
            'Election campaign command dashboard.', NULL, GETDATE(), 0, 0, 0, 1
        );

        SET @ElectionWarRoomMenuId = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE dbo.MenuMaster
        SET ParentMenuId = NULL,
            MenuName = 'Election War Room',
            IconClass = 'fas fa-bullhorn',
            DisplayOrder = 125,
            IsActive = 1,
            ShowOnHome = 0,
            ShowInAdminSidebar = 1,
            IsClickable = 1,
            MenuLevel = 0,
            ModifiedDate = GETDATE()
        WHERE MenuId = @ElectionWarRoomMenuId;
    END;

    DECLARE @WarRoomMenus TABLE
    (
        MenuName NVARCHAR(200),
        MenuDescription NVARCHAR(500),
        ControllerName NVARCHAR(100),
        ActionName NVARCHAR(100),
        IconClass NVARCHAR(100),
        DisplayOrder INT
    );

    INSERT INTO @WarRoomMenus
    VALUES
        ('Events', 'Campaign events and election calendar.', 'ElectionWarRoom', 'Events', 'fas fa-calendar-days', 127),
        ('Tasks', 'Assign, update and track campaign tasks.', 'ElectionWarRoom', 'Tasks', 'fas fa-list-check', 128),
        ('Election Booths', 'Booth coverage, voter strength and priority monitoring.', 'ElectionWarRoom', 'ElectionBooths', 'fas fa-location-dot', 129),
        ('Booth Visits', 'Field visits, house coverage and supporter tracking.', 'ElectionWarRoom', 'BoothVisits', 'fas fa-route', 130),
        ('Jan Sampark', 'Citizen contact, complaints and resolution tracking.', 'ElectionWarRoom', 'JanSampark', 'fas fa-comments', 131),
        ('Voter Management', 'Add, update and map voters by booth, EPIC, part and serial number.', 'Voter', 'Index', 'fas fa-id-card', 132),
        ('Teams', 'Campaign teams and volunteer assignment.', 'ElectionWarRoom', 'Teams', 'fas fa-users', 133),
        ('Vehicles', 'Vehicle allocation and field logistics.', 'ElectionWarRoom', 'Vehicles', 'fas fa-car', 134),
        ('Attendance', 'Worker, volunteer and VIP attendance monitoring.', 'ElectionWarRoom', 'Attendance', 'fas fa-user-check', 135),
        ('Finance & Donations', 'Donation register, campaign spending, payment proof and approvals.', 'ElectionWarRoom', 'FinanceAndDonations', 'fas fa-indian-rupee-sign', 136),
        ('Media', 'Campaign media uploads and approvals.', 'ElectionWarRoom', 'Media', 'fas fa-photo-film', 137),
        ('Candidate Management', 'Candidate profile and approval workflow.', 'ElectionWarRoom', 'CandidateManagement', 'fas fa-user-tie', 139),
        ('Leader Campaign Kit', 'Leader brand, speeches, slogans and media kit.', 'ElectionWarRoom', 'LeaderCampaignKit', 'fas fa-bullhorn', 144),
        ('Manifesto Tracker', 'Promises, manifesto points and progress notes.', 'ElectionWarRoom', 'ManifestoTracker', 'fas fa-clipboard-check', 145),
        ('Booth Committee Network', 'Booth committee roles and meeting follow-up.', 'ElectionWarRoom', 'BoothCommittee', 'fas fa-sitemap', 146),
        ('Page/Social Coordination', 'Official channel coordination and content approval.', 'ElectionWarRoom', 'PageSocialCoordination', 'fas fa-hashtag', 147),
        ('Rally Material Kit', 'Banners, pamphlets, kits and distribution control.', 'ElectionWarRoom', 'RallyMaterialKit', 'fas fa-boxes-stacked', 148),
        ('Campaign Training', 'Volunteer and coordinator training readiness.', 'ElectionWarRoom', 'CampaignTraining', 'fas fa-person-chalkboard', 149),
        ('Social Media War Room', 'Content calendar and publishing workflow.', 'ElectionWarRoom', 'SocialMediaWarRoom', 'fas fa-share-nodes', 140),
        ('Campaign Polls', 'Public feedback polls with social sharing.', 'ElectionWarRoom', 'CampaignPolls', 'fas fa-square-poll-vertical', 141),
        ('Event Polls', 'Event-specific poll publishing and responses.', 'ElectionWarRoom', 'Polls', 'fas fa-chart-simple', 142),
        ('Compliance Center', 'Audit trail and sensitive action monitoring.', 'ElectionWarRoom', 'ComplianceCenter', 'fas fa-shield-halved', 143),
        ('Campaign Alerts', 'Critical alerts and assigned action tracking.', 'ElectionWarRoom', 'CampaignAlerts', 'fas fa-triangle-exclamation', 138);

    MERGE dbo.MenuMaster AS Target
    USING @WarRoomMenus AS Source
       ON ISNULL(Target.AreaName, 'Admin') = 'Admin'
      AND Target.ControllerName = Source.ControllerName
      AND Target.ActionName = Source.ActionName
    WHEN MATCHED THEN
        UPDATE SET
            Target.ParentMenuId = @ElectionWarRoomMenuId,
            Target.MenuName = Source.MenuName,
            Target.MenuDescription = Source.MenuDescription,
            Target.MenuType = 'Admin',
            Target.IconClass = Source.IconClass,
            Target.DisplayOrder = Source.DisplayOrder,
            Target.IsActive = 1,
            Target.ShowOnHome = 0,
            Target.ShowInAdminSidebar = 1,
            Target.OpenInNewTab = 0,
            Target.IsClickable = 1,
            Target.HasMegaMenu = 0,
            Target.MenuLevel = 1,
            Target.ShowInFooter = 0,
            Target.ShowInQuickLinks = 0,
            Target.IsSystemMenu = 1,
            Target.ModifiedDate = GETDATE()
    WHEN NOT MATCHED BY TARGET THEN
        INSERT
        (
            ParentMenuId, MenuName, MenuDescription, AreaName, ControllerName, ActionName,
            CustomUrl, MenuType, IconClass, CssClass, DisplayOrder, IsActive, ShowOnHome,
            ShowInAdminSidebar, OpenInNewTab, IsClickable, HasMegaMenu, PageTitle,
            MetaDescription, CreatedBy, CreatedDate, MenuLevel, ShowInFooter,
            ShowInQuickLinks, IsSystemMenu
        )
        VALUES
        (
            @ElectionWarRoomMenuId, Source.MenuName, Source.MenuDescription,
            'Admin', Source.ControllerName, Source.ActionName, NULL, 'Admin', Source.IconClass,
            NULL, Source.DisplayOrder, 1, 0, 1, 0, 1, 0, Source.MenuName,
            Source.MenuDescription, NULL, GETDATE(), 1, 0, 0, 1
        );
END;


