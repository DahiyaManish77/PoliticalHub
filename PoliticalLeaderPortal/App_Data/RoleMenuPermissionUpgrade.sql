IF OBJECT_ID('dbo.RoleMenuPermission', 'U') IS NULL
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
        CONSTRAINT UQ_RoleMenuPermission_Role_Menu UNIQUE (RoleId, MenuId),
        CONSTRAINT FK_RoleMenuPermission_Role FOREIGN KEY (RoleId) REFERENCES dbo.ApplicationRole(RoleId),
        CONSTRAINT FK_RoleMenuPermission_Menu FOREIGN KEY (MenuId) REFERENCES dbo.MenuMaster(MenuId)
    );
END
GO

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
        ParentMenuId,
        MenuName,
        MenuDescription,
        AreaName,
        ControllerName,
        ActionName,
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
        MenuLevel,
        ShowInFooter,
        ShowInQuickLinks,
        IsSystemMenu
    )
    VALUES
    (
        NULL,
        'Role Permissions',
        'Assign admin menus and Election War Room modules to roles.',
        'Admin',
        'RoleMenuPermission',
        'Index',
        NULL,
        'Admin',
        'fas fa-user-shield',
        NULL,
        999,
        1,
        0,
        1,
        0,
        1,
        0,
        'Role Permissions',
        'Role based menu and module permissions.',
        NULL,
        GETDATE(),
        1,
        0,
        0,
        1
    );
END
GO
