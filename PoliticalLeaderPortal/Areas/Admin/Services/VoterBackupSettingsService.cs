using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using PoliticalLeaderPortal.Models;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace PoliticalLeaderPortal.Areas.Admin.Services
{
    public class VoterBackupSettingsService
    {
        private readonly PoliticalLeaderPortalDbEntities1 db;

        public VoterBackupSettingsService()
        {
            db = new PoliticalLeaderPortalDbEntities1();
        }

        public VoterBackupSettingsVM Get()
        {
            EnsureTable();

            var model = db.Database.SqlQuery<VoterBackupSettingsVM>(
                @"SELECT TOP 1 BackupSettingId, AutoBackupEnabled, KeepLatestFiles, MirrorToDriveFolder,
                         DriveMirrorFolderPath, LastBackupFilePath, LastMirrorFilePath, LastBackupStatus
                  FROM dbo.VoterBackupSetting
                  ORDER BY BackupSettingId")
                .FirstOrDefault();

            if (model != null)
            {
                return model;
            }

            db.Database.ExecuteSqlCommand(
                @"INSERT INTO dbo.VoterBackupSetting
                  (AutoBackupEnabled, KeepLatestFiles, MirrorToDriveFolder, CreatedDate)
                  VALUES (1, 30, 0, GETDATE())");

            return Get();
        }

        public bool Save(VoterBackupSettingsVM model, out string message)
        {
            EnsureTable();

            if (model.KeepLatestFiles <= 0)
            {
                model.KeepLatestFiles = 30;
            }

            model.DriveMirrorFolderPath = Clean(model.DriveMirrorFolderPath);

            if (model.MirrorToDriveFolder)
            {
                if (String.IsNullOrWhiteSpace(model.DriveMirrorFolderPath))
                {
                    message = "Please enter Google Drive synced folder path.";
                    return false;
                }

                if (!Directory.Exists(model.DriveMirrorFolderPath))
                {
                    message = "Google Drive synced folder path was not found.";
                    return false;
                }
            }

            db.Database.ExecuteSqlCommand(
                @"UPDATE dbo.VoterBackupSetting
                  SET AutoBackupEnabled = @AutoBackupEnabled,
                      KeepLatestFiles = @KeepLatestFiles,
                      MirrorToDriveFolder = @MirrorToDriveFolder,
                      DriveMirrorFolderPath = @DriveMirrorFolderPath,
                      UpdatedDate = GETDATE()
                  WHERE BackupSettingId = @BackupSettingId",
                new SqlParameter("@BackupSettingId", model.BackupSettingId),
                new SqlParameter("@AutoBackupEnabled", model.AutoBackupEnabled),
                new SqlParameter("@KeepLatestFiles", model.KeepLatestFiles),
                new SqlParameter("@MirrorToDriveFolder", model.MirrorToDriveFolder),
                new SqlParameter("@DriveMirrorFolderPath", (object)model.DriveMirrorFolderPath ?? DBNull.Value));

            message = "Backup settings saved successfully.";
            return true;
        }

        public void MarkBackupResult(string backupFilePath, string mirrorFilePath, string status)
        {
            EnsureTable();

            var settings = Get();

            db.Database.ExecuteSqlCommand(
                @"UPDATE dbo.VoterBackupSetting
                  SET LastBackupFilePath = @LastBackupFilePath,
                      LastMirrorFilePath = @LastMirrorFilePath,
                      LastBackupStatus = @LastBackupStatus,
                      UpdatedDate = GETDATE()
                  WHERE BackupSettingId = @BackupSettingId",
                new SqlParameter("@BackupSettingId", settings.BackupSettingId),
                new SqlParameter("@LastBackupFilePath", (object)backupFilePath ?? DBNull.Value),
                new SqlParameter("@LastMirrorFilePath", (object)mirrorFilePath ?? DBNull.Value),
                new SqlParameter("@LastBackupStatus", (object)status ?? DBNull.Value));
        }

        public void EnsureTable()
        {
            db.Database.ExecuteSqlCommand(
                @"IF OBJECT_ID('dbo.VoterBackupSetting', 'U') IS NULL
                  BEGIN
                      CREATE TABLE dbo.VoterBackupSetting
                      (
                          BackupSettingId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_VoterBackupSetting PRIMARY KEY,
                          AutoBackupEnabled BIT NOT NULL CONSTRAINT DF_VoterBackupSetting_AutoBackup DEFAULT (1),
                          KeepLatestFiles INT NOT NULL CONSTRAINT DF_VoterBackupSetting_KeepLatest DEFAULT (30),
                          MirrorToDriveFolder BIT NOT NULL CONSTRAINT DF_VoterBackupSetting_Mirror DEFAULT (0),
                          DriveMirrorFolderPath NVARCHAR(500) NULL,
                          LastBackupFilePath NVARCHAR(500) NULL,
                          LastMirrorFilePath NVARCHAR(500) NULL,
                          LastBackupStatus NVARCHAR(500) NULL,
                          CreatedDate DATETIME NOT NULL CONSTRAINT DF_VoterBackupSetting_CreatedDate DEFAULT (GETDATE()),
                          UpdatedDate DATETIME NULL
                      );

                      INSERT INTO dbo.VoterBackupSetting
                      (AutoBackupEnabled, KeepLatestFiles, MirrorToDriveFolder, CreatedDate)
                      VALUES (1, 30, 0, GETDATE());
                  END");
        }

        private static string Clean(string value)
        {
            return String.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
