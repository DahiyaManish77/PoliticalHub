/*
    Least-privilege role permissions.
    Safe to run repeatedly after CentralizedAdminNavigationUpgrade.sql.
*/
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @SuperAdminId int = (SELECT TOP 1 RoleId FROM ApplicationRole WHERE RoleName = N'SuperAdmin');
DECLARE @AdminId int = (SELECT TOP 1 RoleId FROM ApplicationRole WHERE RoleName = N'Admin');
DECLARE @EditorId int = (SELECT TOP 1 RoleId FROM ApplicationRole WHERE RoleName = N'Editor');
DECLARE @CitizenId int = (SELECT TOP 1 RoleId FROM ApplicationRole WHERE RoleName = N'Citizen');

/* Public citizens must never inherit back-office access. */
IF @CitizenId IS NOT NULL
    DELETE FROM RoleMenuPermission WHERE RoleId = @CitizenId;

/* Administrators receive explicit full access to every active Admin item. */
;WITH FullAccessRoles AS
(
    SELECT @SuperAdminId RoleId WHERE @SuperAdminId IS NOT NULL
    UNION ALL
    SELECT @AdminId WHERE @AdminId IS NOT NULL
)
MERGE RoleMenuPermission AS target
USING
(
    SELECT roles.RoleId, menu.MenuId
    FROM FullAccessRoles roles
    CROSS JOIN MenuMaster menu
    WHERE menu.IsActive = 1 AND menu.ShowInAdminSidebar = 1
) AS source
ON target.RoleId = source.RoleId AND target.MenuId = source.MenuId
WHEN MATCHED THEN
    UPDATE SET CanView = 1, CanCreate = 1, CanEdit = 1, CanDelete = 1, ModifiedDate = GETDATE()
WHEN NOT MATCHED THEN
    INSERT (RoleId, MenuId, CanView, CanCreate, CanEdit, CanDelete, CreatedDate)
    VALUES (source.RoleId, source.MenuId, 1, 1, 1, 1, GETDATE());

/* Editor: dashboard plus communication and public website content only. */
IF @EditorId IS NOT NULL
BEGIN
    DELETE FROM RoleMenuPermission WHERE RoleId = @EditorId;

    ;WITH AllowedEditorMenus AS
    (
        SELECT MenuId
        FROM MenuMaster
        WHERE IsActive = 1
          AND ShowInAdminSidebar = 1
          AND MenuName IN (N'Dashboard', N'Communication & Media', N'Website CMS')

        UNION ALL

        SELECT child.MenuId
        FROM MenuMaster child
        INNER JOIN MenuMaster parent ON parent.MenuId = child.ParentMenuId
        WHERE child.IsActive = 1
          AND child.ShowInAdminSidebar = 1
          AND parent.MenuName IN (N'Communication & Media', N'Website CMS')

        UNION ALL

        SELECT grandchild.MenuId
        FROM MenuMaster grandchild
        INNER JOIN MenuMaster child ON child.MenuId = grandchild.ParentMenuId
        INNER JOIN MenuMaster parent ON parent.MenuId = child.ParentMenuId
        WHERE grandchild.IsActive = 1
          AND grandchild.ShowInAdminSidebar = 1
          AND parent.MenuName IN (N'Communication & Media', N'Website CMS')
    )
    INSERT RoleMenuPermission
    (
        RoleId, MenuId, CanView, CanCreate, CanEdit, CanDelete, CreatedDate
    )
    SELECT DISTINCT
        @EditorId,
        allowed.MenuId,
        1,
        CASE WHEN menu.ControllerName IS NULL THEN 0 ELSE 1 END,
        CASE WHEN menu.ControllerName IS NULL THEN 0 ELSE 1 END,
        0,
        GETDATE()
    FROM AllowedEditorMenus allowed
    INNER JOIN MenuMaster menu ON menu.MenuId = allowed.MenuId;
END;

COMMIT TRANSACTION;
