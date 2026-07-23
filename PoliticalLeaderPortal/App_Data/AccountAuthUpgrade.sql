/*
    Account authentication upgrade
    Target database: PoliticalLeaderPortalDb

    Ensures public registrations have a safe non-admin role.
*/

IF OBJECT_ID('dbo.ApplicationRole', 'U') IS NULL
BEGIN
    RAISERROR('dbo.ApplicationRole table was not found in the selected database.', 16, 1);
    RETURN;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.ApplicationRole WHERE RoleName = 'Citizen')
BEGIN
    INSERT INTO dbo.ApplicationRole
    (
        RoleName,
        Description,
        CreatedBy,
        CreatedDate,
        ModifiedBy,
        ModifiedDate,
        IsActive
    )
    VALUES
    (
        'Citizen',
        'Public website registered user',
        NULL,
        GETDATE(),
        NULL,
        NULL,
        1
    );
END;

SELECT RoleId, RoleName, IsActive
FROM dbo.ApplicationRole
ORDER BY RoleId;
