using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using PoliticalLeaderPortal.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.Services
{
    public class RoleMenuPermissionService
    {
        private readonly PoliticalLeaderPortalDbEntities1 db;

        public RoleMenuPermissionService()
        {
            db = new PoliticalLeaderPortalDbEntities1();
        }

        public RoleMenuPermissionPageVM BuildPage(int? selectedRoleId)
        {
            EnsurePermissionTable();

            var roles = db.ApplicationRoles
                .Where(x => x.IsActive)
                .OrderBy(x => x.RoleName)
                .ToList();

            int roleId = selectedRoleId.GetValueOrDefault();

            if (roleId <= 0 && roles.Any())
            {
                roleId = roles.First().RoleId;
            }

            return new RoleMenuPermissionPageVM
            {
                SelectedRoleId = roleId,
                Roles = roles.Select(x => new SelectListItem
                {
                    Value = x.RoleId.ToString(),
                    Text = x.RoleName,
                    Selected = x.RoleId == roleId
                }).ToList(),
                Menus = GetPermissionItems(roleId)
            };
        }

        public void Save(int roleId, IEnumerable<RoleMenuPermissionItemVM> permissions, int? userId)
        {
            EnsurePermissionTable();

            db.Database.ExecuteSqlCommand(
                "DELETE FROM dbo.RoleMenuPermission WHERE RoleId = @RoleId",
                new SqlParameter("@RoleId", roleId));

            foreach (var permission in permissions.Where(x => x.IsAllowed))
            {
                db.Database.ExecuteSqlCommand(
                    @"INSERT INTO dbo.RoleMenuPermission
                      (RoleId, MenuId, CanView, CanCreate, CanEdit, CanDelete, CreatedBy, CreatedDate)
                      VALUES
                      (@RoleId, @MenuId, 1, @CanCreate, @CanEdit, @CanDelete, @CreatedBy, GETDATE())",
                    new SqlParameter("@RoleId", roleId),
                    new SqlParameter("@MenuId", permission.MenuId),
                    new SqlParameter("@CanCreate", permission.CanCreate),
                    new SqlParameter("@CanEdit", permission.CanEdit),
                    new SqlParameter("@CanDelete", permission.CanDelete),
                    new SqlParameter("@CreatedBy", (object)userId ?? DBNull.Value));
            }
        }

        public bool HasAccess(int? roleId, string roleName, string area, string controller, string action)
        {
            if (!roleId.HasValue)
            {
                return false;
            }

            if (IsFullAccessRole(roleName))
            {
                return true;
            }

            EnsurePermissionTable();

            string normalizedArea = String.IsNullOrWhiteSpace(area) ? "Admin" : area.Trim();
            string normalizedController = (controller ?? String.Empty).Trim();
            string normalizedAction = (action ?? String.Empty).Trim();

            const string sql =
                @"SELECT COUNT(1)
                  FROM dbo.RoleMenuPermission p
                  INNER JOIN dbo.MenuMaster m ON p.MenuId = m.MenuId
                  WHERE p.RoleId = @RoleId
                    AND p.CanView = 1
                    AND m.IsActive = 1
                    AND ISNULL(NULLIF(m.AreaName, ''), 'Admin') = @AreaName
                    AND m.ControllerName = @ControllerName
                    AND
                    (
                        m.ActionName = @ActionName
                        OR
                        (
                            m.ControllerName = 'ElectionWarRoom'
                            AND @ControllerName = 'ElectionWarRoom'
                        )
                        OR
                        (
                            m.ControllerName = 'Voter'
                            AND @ControllerName = 'Voter'
                        )
                    )";

            int count = db.Database.SqlQuery<int>(
                sql,
                new SqlParameter("@RoleId", roleId.Value),
                new SqlParameter("@AreaName", normalizedArea),
                new SqlParameter("@ControllerName", normalizedController),
                new SqlParameter("@ActionName", normalizedAction))
                .FirstOrDefault();

            return count > 0;
        }

        public bool HasActionPermission(int? roleId, string roleName, string area, string controller, string action, string permission)
        {
            if (!roleId.HasValue)
            {
                return false;
            }

            if (IsFullAccessRole(roleName))
            {
                return true;
            }

            EnsurePermissionTable();

            string columnName = "CanView";

            if (String.Equals(permission, "Create", StringComparison.OrdinalIgnoreCase) ||
                String.Equals(permission, "CanCreate", StringComparison.OrdinalIgnoreCase))
            {
                columnName = "CanCreate";
            }
            else if (String.Equals(permission, "Edit", StringComparison.OrdinalIgnoreCase) ||
                     String.Equals(permission, "CanEdit", StringComparison.OrdinalIgnoreCase))
            {
                columnName = "CanEdit";
            }
            else if (String.Equals(permission, "Delete", StringComparison.OrdinalIgnoreCase) ||
                     String.Equals(permission, "CanDelete", StringComparison.OrdinalIgnoreCase))
            {
                columnName = "CanDelete";
            }

            string normalizedArea = String.IsNullOrWhiteSpace(area) ? "Admin" : area.Trim();
            string normalizedController = (controller ?? String.Empty).Trim();
            string normalizedAction = (action ?? String.Empty).Trim();

            string sql =
                @"SELECT COUNT(1)
                  FROM dbo.RoleMenuPermission p
                  INNER JOIN dbo.MenuMaster m ON p.MenuId = m.MenuId
                  WHERE p.RoleId = @RoleId
                    AND p." + columnName + @" = 1
                    AND m.IsActive = 1
                    AND ISNULL(NULLIF(m.AreaName, ''), 'Admin') = @AreaName
                    AND m.ControllerName = @ControllerName
                    AND (m.ActionName = @ActionName OR m.ControllerName = @ControllerName)";

            int count = db.Database.SqlQuery<int>(
                sql,
                new SqlParameter("@RoleId", roleId.Value),
                new SqlParameter("@AreaName", normalizedArea),
                new SqlParameter("@ControllerName", normalizedController),
                new SqlParameter("@ActionName", normalizedAction))
                .FirstOrDefault();

            return count > 0;
        }

        public List<int> GetAllowedMenuIds(int? roleId, string roleName)
        {
            EnsurePermissionTable();

            if (!roleId.HasValue || IsFullAccessRole(roleName))
            {
                return null;
            }

            return db.Database.SqlQuery<int>(
                "SELECT MenuId FROM dbo.RoleMenuPermission WHERE RoleId = @RoleId AND CanView = 1",
                new SqlParameter("@RoleId", roleId.Value))
                .ToList();
        }

        public static bool IsFullAccessRole(string roleName)
        {
            return String.Equals(roleName, "SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
                   String.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase);
        }

        private List<RoleMenuPermissionItemVM> GetPermissionItems(int roleId)
        {
            const string sql =
                @"SELECT
                    m.MenuId,
                    m.ParentMenuId,
                    m.MenuName,
                    m.ControllerName,
                    m.ActionName,
                    m.IconClass,
                    m.MenuLevel,
                    CAST(CASE WHEN p.PermissionId IS NULL THEN 0 ELSE 1 END AS bit) AS IsAllowed,
                    CAST(ISNULL(p.CanCreate, 0) AS bit) AS CanCreate,
                    CAST(ISNULL(p.CanEdit, 0) AS bit) AS CanEdit,
                    CAST(ISNULL(p.CanDelete, 0) AS bit) AS CanDelete
                  FROM dbo.MenuMaster m
                  LEFT JOIN dbo.RoleMenuPermission p
                    ON p.MenuId = m.MenuId
                   AND p.RoleId = @RoleId
                  WHERE m.IsActive = 1
                    AND m.ShowInAdminSidebar = 1
                  ORDER BY m.DisplayOrder, m.MenuName";

            return db.Database.SqlQuery<RoleMenuPermissionItemVM>(
                sql,
                new SqlParameter("@RoleId", roleId))
                .ToList();
        }

        private void EnsurePermissionTable()
        {
            db.Database.ExecuteSqlCommand(
                @"IF OBJECT_ID('dbo.RoleMenuPermission', 'U') IS NULL
                  BEGIN
                      CREATE TABLE dbo.RoleMenuPermission
                      (
                          PermissionId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RoleMenuPermission PRIMARY KEY,
                          RoleId INT NOT NULL,
                          MenuId INT NOT NULL,
                          CanView BIT NOT NULL CONSTRAINT DF_RoleMenuPermission_CanView DEFAULT (1),
                          CanCreate BIT NOT NULL CONSTRAINT DF_RoleMenuPermission_CanCreate DEFAULT (0),
                          CanEdit BIT NOT NULL CONSTRAINT DF_RoleMenuPermission_CanEdit DEFAULT (0),
                          CanDelete BIT NOT NULL CONSTRAINT DF_RoleMenuPermission_CanDelete DEFAULT (0),
                          CreatedBy INT NULL,
                          CreatedDate DATETIME NOT NULL CONSTRAINT DF_RoleMenuPermission_CreatedDate DEFAULT (GETDATE()),
                          ModifiedBy INT NULL,
                          ModifiedDate DATETIME NULL,
                          CONSTRAINT UQ_RoleMenuPermission_Role_Menu UNIQUE (RoleId, MenuId)
                      );
                  END

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
                          'Manage Google Play and Apple App Store app links.',
                          NULL, GETDATE(), 0, 0, 0, 1
                      );
                  END
                  ELSE
                  BEGIN
                      UPDATE dbo.MenuMaster
                      SET MenuName = 'App Download Settings',
                          MenuDescription = 'Manage home page app download banner links.',
                          IconClass = 'fas fa-mobile-screen-button',
                          DisplayOrder = 143,
                          IsActive = 1,
                          ShowOnHome = 0,
                          ShowInAdminSidebar = 1,
                          IsClickable = 1,
                          ModifiedDate = GETDATE()
                      WHERE AreaName = 'Admin'
                        AND ControllerName = 'AppDownloadSetting'
                        AND ActionName = 'Index';
                  END

                  IF NOT EXISTS
                  (
                      SELECT 1
                      FROM dbo.MenuMaster
                      WHERE AreaName = 'Admin'
                        AND ControllerName = 'HomeMember'
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
                          NULL, 'Home Members', 'Manage member showcase section on the home page.',
                          'Admin', 'HomeMember', 'Index', NULL, 'Admin', 'fas fa-user-tie',
                          NULL, 142, 1, 0, 1, 0, 1, 0, 'Home Members',
                          'Upload member photos, names, designations and tenure for home page display.',
                          NULL, GETDATE(), 0, 0, 0, 1
                      );
                  END
                  ELSE
                  BEGIN
                      UPDATE dbo.MenuMaster
                      SET MenuName = 'Home Members',
                          MenuDescription = 'Manage member showcase section on the home page.',
                          IconClass = 'fas fa-user-tie',
                          DisplayOrder = 142,
                          IsActive = 1,
                          ShowOnHome = 0,
                          ShowInAdminSidebar = 1,
                          IsClickable = 1,
                          ModifiedDate = GETDATE()
                      WHERE AreaName = 'Admin'
                        AND ControllerName = 'HomeMember'
                        AND ActionName = 'Index';
                  END

                  IF NOT EXISTS
                  (
                      SELECT 1
                      FROM dbo.MenuMaster
                      WHERE AreaName = 'Admin'
                        AND ControllerName = 'RoleMenuPermission'
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
                          NULL, 'Role Permissions', 'Assign admin menus and Election War Room modules to roles.',
                          'Admin', 'RoleMenuPermission', 'Index', NULL, 'Admin', 'fas fa-user-shield',
                          NULL, 999, 1, 0, 1, 0, 1, 0, 'Role Permissions',
                          'Role based menu and module permissions.', NULL, GETDATE(), 1, 0, 0, 1
                      );
                  END

                  IF NOT EXISTS
                  (
                      SELECT 1
                      FROM dbo.MenuMaster
                      WHERE AreaName = 'Admin'
                        AND ControllerName = 'Voter'
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
                          NULL, 'Voter Management', 'Add, update and map voters by booth, EPIC, part and serial number.',
                          'Admin', 'Voter', 'Index', NULL, 'Admin', 'fas fa-id-card',
                          NULL, 510, 1, 0, 1, 0, 1, 0, 'Voter Management',
                          'Election voter data management and duplicate prevention.', NULL, GETDATE(), 1, 0, 0, 1
                      );
                  END

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
                  END

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
                      ('Expenses', 'Campaign expense and approval tracking.', 'ElectionWarRoom', 'Expenses', 'fas fa-indian-rupee-sign', 136),
                      ('Media', 'Campaign media uploads and approvals.', 'ElectionWarRoom', 'Media', 'fas fa-photo-film', 137),
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

                  DECLARE @GalleryManagementMenuId INT;

                  SELECT @GalleryManagementMenuId = MenuId
                  FROM dbo.MenuMaster
                  WHERE AreaName = 'Admin'
                    AND MenuName = 'Gallery Management'
                    AND ParentMenuId IS NULL;

                  IF @GalleryManagementMenuId IS NULL
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
                          NULL, 'Gallery Management', 'Manage public image and video galleries.',
                          'Admin', NULL, NULL, NULL, 'Admin', 'fas fa-photo-film',
                          NULL, 150, 1, 0, 1, 0, 0, 0, 'Gallery Management',
                          'Manage image and video gallery content.', NULL, GETDATE(), 0, 0, 0, 1
                      );

                      SET @GalleryManagementMenuId = SCOPE_IDENTITY();
                  END
                  ELSE
                  BEGIN
                      UPDATE dbo.MenuMaster
                      SET MenuName = 'Gallery Management',
                          MenuDescription = 'Manage public image and video galleries.',
                          AreaName = 'Admin',
                          ControllerName = NULL,
                          ActionName = NULL,
                          CustomUrl = NULL,
                          MenuType = 'Admin',
                          IconClass = 'fas fa-photo-film',
                          DisplayOrder = 150,
                          IsActive = 1,
                          ShowOnHome = 0,
                          ShowInAdminSidebar = 1,
                          OpenInNewTab = 0,
                          IsClickable = 0,
                          HasMegaMenu = 0,
                          MenuLevel = 0,
                          ShowInFooter = 0,
                          ShowInQuickLinks = 0,
                          IsSystemMenu = 1,
                          ModifiedDate = GETDATE()
                      WHERE MenuId = @GalleryManagementMenuId;
                  END

                  DECLARE @GalleryMenus TABLE
                  (
                      MenuName NVARCHAR(200),
                      MenuDescription NVARCHAR(500),
                      ControllerName NVARCHAR(100),
                      ActionName NVARCHAR(100),
                      IconClass NVARCHAR(100),
                      DisplayOrder INT
                  );

                  INSERT INTO @GalleryMenus
                  VALUES
                      ('Images', 'Upload, organise and publish photo gallery images.', 'GalleryImage', 'Index', 'fas fa-image', 151),
                      ('Videos', 'Upload videos and sync YouTube channel videos.', 'VideoGallery', 'Index', 'fas fa-video', 152);

                  MERGE dbo.MenuMaster AS Target
                  USING @GalleryMenus AS Source
                     ON ISNULL(Target.AreaName, 'Admin') = 'Admin'
                    AND Target.ControllerName = Source.ControllerName
                    AND Target.ActionName = Source.ActionName
                  WHEN MATCHED THEN
                      UPDATE SET
                          Target.ParentMenuId = @GalleryManagementMenuId,
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
                          @GalleryManagementMenuId, Source.MenuName, Source.MenuDescription,
                          'Admin', Source.ControllerName, Source.ActionName, NULL, 'Admin', Source.IconClass,
                          NULL, Source.DisplayOrder, 1, 0, 1, 0, 1, 0, Source.MenuName,
                          Source.MenuDescription, NULL, GETDATE(), 1, 0, 0, 1
                      );

                  UPDATE dbo.MenuMaster
                  SET IsActive = 0,
                      ShowInAdminSidebar = 0,
                      ModifiedDate = GETDATE()
                  WHERE AreaName = 'Admin'
                    AND ControllerName IN ('GalleryCategory', 'VideoCategory');

                  IF NOT EXISTS
                  (
                      SELECT 1
                      FROM dbo.MenuMaster
                      WHERE AreaName = 'Admin'
                        AND ControllerName = 'CitizenConnect'
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
                          NULL, 'Citizen Connect', 'Contact, volunteer and suggestion submissions.',
                          'Admin', 'CitizenConnect', 'Index', NULL, 'Admin', 'fas fa-inbox',
                          NULL, 139, 1, 0, 1, 0, 1, 0, 'Citizen Connect',
                          'Manage public contact, volunteer and suggestion requests.', NULL, GETDATE(), 0, 0, 0, 1
                      );
                  END
                  ELSE
                  BEGIN
                      UPDATE dbo.MenuMaster
                      SET MenuName = 'Citizen Connect',
                          MenuDescription = 'Contact, volunteer and suggestion submissions.',
                          IconClass = 'fas fa-inbox',
                          DisplayOrder = 139,
                          IsActive = 1,
                          ShowOnHome = 0,
                          ShowInAdminSidebar = 1,
                          ModifiedDate = GETDATE()
                      WHERE AreaName = 'Admin'
                        AND ControllerName = 'CitizenConnect'
                        AND ActionName = 'Index';
                  END

                  IF NOT EXISTS
                  (
                      SELECT 1
                      FROM dbo.MenuMaster
                      WHERE AreaName = 'Admin'
                        AND ControllerName = 'TodaySchedule'
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
                          NULL, 'Today''s Schedule', 'Daily date, time, place and program coordination entries.',
                          'Admin', 'TodaySchedule', 'Index', NULL, 'Admin', 'fas fa-calendar-day',
                          NULL, 141, 1, 0, 1, 0, 1, 0, 'Today''s Schedule',
                          'Manage daily schedule entries.', NULL, GETDATE(), 0, 0, 0, 1
                      );
                  END
                  ELSE
                  BEGIN
                      UPDATE dbo.MenuMaster
                      SET MenuName = 'Today''s Schedule',
                          MenuDescription = 'Daily date, time, place and program coordination entries.',
                          IconClass = 'fas fa-calendar-day',
                          DisplayOrder = 141,
                          IsActive = 1,
                          ShowOnHome = 0,
                          ShowInAdminSidebar = 1,
                          IsClickable = 1,
                          ModifiedDate = GETDATE()
                      WHERE AreaName = 'Admin'
                        AND ControllerName = 'TodaySchedule'
                        AND ActionName = 'Index';
                  END

                  IF NOT EXISTS
                  (
                      SELECT 1
                      FROM dbo.MenuMaster
                      WHERE AreaName = 'Admin'
                        AND ControllerName = 'VoterBackupSettings'
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
                          NULL, 'Voter Backup Settings', 'Configure voter backup retention and Google Drive folder mirroring.',
                          'Admin', 'VoterBackupSettings', 'Index', NULL, 'Admin', 'fas fa-cloud-arrow-up-alt',
                          NULL, 140, 1, 0, 1, 0, 1, 0, 'Voter Backup Settings',
                          'Configure voter backup destination and retention.', NULL, GETDATE(), 0, 0, 0, 1
                      );
                  END
                  ELSE
                  BEGIN
                      UPDATE dbo.MenuMaster
                      SET MenuName = 'Voter Backup Settings',
                          MenuDescription = 'Configure voter backup retention and Google Drive folder mirroring.',
                          IconClass = 'fas fa-cloud-arrow-up-alt',
                          DisplayOrder = 140,
                          IsActive = 1,
                          ShowOnHome = 0,
                          ShowInAdminSidebar = 1,
                          ModifiedDate = GETDATE()
                      WHERE AreaName = 'Admin'
                        AND ControllerName = 'VoterBackupSettings'
                        AND ActionName = 'Index';
                  END

                  DECLARE @CommunicationMenuId INT =
                  (
                      SELECT TOP 1 MenuId
                      FROM dbo.MenuMaster
                      WHERE MenuName IN ('Communication', 'Communication & Media')
                        AND IsActive = 1
                      ORDER BY CASE WHEN MenuName = 'Communication & Media' THEN 0 ELSE 1 END, MenuId
                  );

                  DECLARE @VideoMeetingMenuId INT =
                  (
                      SELECT TOP 1 MenuId
                      FROM dbo.MenuMaster
                      WHERE MenuName = 'Video Meetings'
                         OR (AreaName = 'Admin' AND ControllerName = 'VideoMeeting' AND ActionName = 'Index')
                      ORDER BY
                          CASE WHEN ParentMenuId = @CommunicationMenuId THEN 0 ELSE 1 END,
                          CASE WHEN ControllerName = 'VideoMeeting' AND ActionName = 'Index' THEN 0 ELSE 1 END,
                          MenuId
                  );

                  IF @VideoMeetingMenuId IS NULL
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
                          @CommunicationMenuId, 'Video Meetings', 'Create secure video meetings and invite authorised participants.',
                          'Admin', 'VideoMeeting', 'Index', NULL, 'Admin', 'fas fa-video',
                          NULL, 166, 1, 0, 1, 0, 1, 0, 'Video Meetings',
                          'Schedule and manage secure video meetings.', NULL, GETDATE(),
                          CASE WHEN @CommunicationMenuId IS NULL THEN 0 ELSE 1 END, 0, 0, 1
                      );
                      SET @VideoMeetingMenuId = SCOPE_IDENTITY();
                  END
                  ELSE
                  BEGIN
                      UPDATE dbo.MenuMaster
                      SET ParentMenuId = @CommunicationMenuId,
                          MenuName = 'Video Meetings',
                          MenuDescription = 'Create secure video meetings and invite authorised participants.',
                          IconClass = 'fas fa-video',
                          DisplayOrder = 166,
                          IsActive = 1,
                          ShowOnHome = 0,
                          ShowInAdminSidebar = 1,
                          IsClickable = 1,
                          MenuLevel = CASE WHEN @CommunicationMenuId IS NULL THEN 0 ELSE 1 END,
                          ModifiedDate = GETDATE()
                      WHERE MenuId = @VideoMeetingMenuId;
                  END

                  UPDATE dbo.MenuMaster
                  SET IsActive = 0,
                      ShowInAdminSidebar = 0,
                      ModifiedDate = GETDATE()
                  WHERE MenuId <> @VideoMeetingMenuId
                    AND
                    (
                        MenuName = 'Video Meetings'
                        OR (AreaName = 'Admin' AND ControllerName = 'VideoMeeting' AND ActionName = 'Index')
                    );

                  DECLARE @VoiceAgentMenuId INT =
                  (
                      SELECT TOP 1 MenuId FROM dbo.MenuMaster
                      WHERE MenuName = 'Voice Agent'
                         OR (AreaName='Admin' AND ControllerName='VoiceAgent' AND ActionName='Index')
                      ORDER BY MenuId
                  );
                  IF @VoiceAgentMenuId IS NULL
                  BEGIN
                      INSERT dbo.MenuMaster
                      (
                          ParentMenuId,MenuName,MenuDescription,AreaName,ControllerName,ActionName,
                          CustomUrl,MenuType,IconClass,CssClass,DisplayOrder,IsActive,ShowOnHome,
                          ShowInAdminSidebar,OpenInNewTab,IsClickable,HasMegaMenu,PageTitle,
                          MetaDescription,CreatedBy,CreatedDate,MenuLevel,ShowInFooter,
                          ShowInQuickLinks,IsSystemMenu
                      )
                      VALUES
                      (
                          @CommunicationMenuId,'Voice Agent','Manage incoming calls, missed calls, recordings and voice API settings.',
                          'Admin','VoiceAgent','Index',NULL,'Admin','fas fa-headset',NULL,165,1,0,1,0,1,0,
                          'Voice Agent','AI voice call operations and analytics.',NULL,GETDATE(),
                          CASE WHEN @CommunicationMenuId IS NULL THEN 0 ELSE 1 END,0,0,1
                      );
                      SET @VoiceAgentMenuId=SCOPE_IDENTITY();
                  END
                  ELSE
                  BEGIN
                      UPDATE dbo.MenuMaster SET ParentMenuId=@CommunicationMenuId,MenuName='Voice Agent',
                          MenuDescription='Manage incoming calls, missed calls, recordings and voice API settings.',
                          AreaName='Admin',ControllerName='VoiceAgent',ActionName='Index',IconClass='fas fa-headset',
                          DisplayOrder=165,IsActive=1,ShowInAdminSidebar=1,IsClickable=1,
                          MenuLevel=CASE WHEN @CommunicationMenuId IS NULL THEN 0 ELSE 1 END,ModifiedDate=GETDATE()
                      WHERE MenuId=@VoiceAgentMenuId;
                  END

                  IF NOT EXISTS
                  (
                      SELECT 1 FROM dbo.MenuMaster
                      WHERE AreaName='Admin' AND ControllerName='PeopleSay' AND ActionName='Index'
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
                          @CommunicationMenuId, 'People Say', 'Moderate public testimonial videos, comments and leader responses.',
                          'Admin', 'PeopleSay', 'Index', NULL, 'Admin', 'fas fa-comment-dots',
                          NULL, 167, 1, 0, 1, 0, 1, 0, 'People Say About Som',
                          'Review and analyse public testimonial videos.', NULL, GETDATE(),
                          CASE WHEN @CommunicationMenuId IS NULL THEN 0 ELSE 1 END, 0, 0, 1
                      );
                  END
                  ELSE
                  BEGIN
                      UPDATE dbo.MenuMaster SET ParentMenuId=@CommunicationMenuId,MenuName='People Say',
                          MenuDescription='Moderate public testimonial videos, comments and leader responses.',
                          IconClass='fas fa-comment-dots',DisplayOrder=167,IsActive=1,ShowInAdminSidebar=1,
                          IsClickable=1,MenuLevel=CASE WHEN @CommunicationMenuId IS NULL THEN 0 ELSE 1 END,
                          ModifiedDate=GETDATE()
                      WHERE AreaName='Admin' AND ControllerName='PeopleSay' AND ActionName='Index';
                  END

                  MERGE dbo.RoleMenuPermission AS Target
                  USING
                  (
                      SELECT r.RoleId, m.MenuId
                      FROM dbo.ApplicationRole r
                      CROSS JOIN dbo.MenuMaster m
                      WHERE r.IsActive = 1
                        AND m.IsActive = 1
                        AND m.ShowInAdminSidebar = 1
                  ) AS Source
                     ON Target.RoleId = Source.RoleId
                    AND Target.MenuId = Source.MenuId
                  WHEN MATCHED THEN
                      UPDATE SET
                          Target.CanView = 1,
                          Target.ModifiedDate = GETDATE()
                  WHEN NOT MATCHED BY TARGET THEN
                      INSERT
                      (
                          RoleId, MenuId, CanView, CanCreate, CanEdit, CanDelete, CreatedBy, CreatedDate
                      )
                      VALUES
                      (
                          Source.RoleId, Source.MenuId, 1, 0, 0, 0, NULL, GETDATE()
                      );");
        }
    }
}
