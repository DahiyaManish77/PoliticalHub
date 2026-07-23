IF OBJECT_ID('dbo.RoleMenuPermission', 'U') IS NOT NULL
   AND OBJECT_ID('dbo.ApplicationRole', 'U') IS NOT NULL
   AND OBJECT_ID('dbo.MenuMaster', 'U') IS NOT NULL
BEGIN
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
            RoleId,
            MenuId,
            CanView,
            CanCreate,
            CanEdit,
            CanDelete,
            CreatedBy,
            CreatedDate
        )
        VALUES
        (
            Source.RoleId,
            Source.MenuId,
            1,
            0,
            0,
            0,
            NULL,
            GETDATE()
        );
END;
