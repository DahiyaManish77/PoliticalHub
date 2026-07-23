IF OBJECT_ID('dbo.HomeMember', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.HomeMember
    (
        HomeMemberId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        MemberName NVARCHAR(160) NOT NULL,
        Designation NVARCHAR(160) NULL,
        Tenure NVARCHAR(120) NULL,
        PhotoPath NVARCHAR(500) NULL,
        DisplayOrder INT NOT NULL CONSTRAINT DF_HomeMember_DisplayOrder DEFAULT(0),
        IsActive BIT NOT NULL CONSTRAINT DF_HomeMember_IsActive DEFAULT(1),
        ShowOnHome BIT NOT NULL CONSTRAINT DF_HomeMember_ShowOnHome DEFAULT(1),
        CreatedDate DATETIME NOT NULL CONSTRAINT DF_HomeMember_CreatedDate DEFAULT(GETDATE()),
        ModifiedDate DATETIME NULL
    );
END
GO

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
GO

MERGE dbo.RoleMenuPermission AS Target
USING
(
    SELECT r.RoleId, m.MenuId
    FROM dbo.ApplicationRole r
    CROSS JOIN dbo.MenuMaster m
    WHERE r.IsActive = 1
      AND m.IsActive = 1
      AND m.ShowInAdminSidebar = 1
      AND m.ControllerName = 'HomeMember'
      AND m.ActionName = 'Index'
) AS Source
   ON Target.RoleId = Source.RoleId
  AND Target.MenuId = Source.MenuId
WHEN MATCHED THEN
    UPDATE SET CanView = 1, CanCreate = 1, CanEdit = 1, CanDelete = 1, ModifiedDate = GETDATE()
WHEN NOT MATCHED BY TARGET THEN
    INSERT (RoleId, MenuId, CanView, CanCreate, CanEdit, CanDelete, CreatedBy, CreatedDate)
    VALUES (Source.RoleId, Source.MenuId, 1, 1, 1, 1, NULL, GETDATE());
GO
