using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using PoliticalLeaderPortal.Models;
using PoliticalLeaderPortal.ViewModels;
using System;
using System.Data.SqlClient;
using System.Linq;

namespace PoliticalLeaderPortal.Areas.Admin.Services
{
    public class AppDownloadSettingService
    {
        private const string DefaultGooglePlayUrl = "https://play.google.com/store/search?q=Bharatiya%20Janata%20Party&c=apps";
        private const string DefaultAppleAppStoreUrl = "https://apps.apple.com/in/search?term=Bharatiya%20Janata%20Party";

        private readonly PoliticalLeaderPortalDbEntities1 _db;

        public AppDownloadSettingService()
        {
            _db = new PoliticalLeaderPortalDbEntities1();
        }

        public AppDownloadSettingVM GetSetting()
        {
            EnsureTable();

            return _db.Database.SqlQuery<AppDownloadSettingVM>(
                @"SELECT TOP 1 AppDownloadSettingId, KickerText, HeadingText, SubHeadingText,
                         GooglePlayUrl, AppleAppStoreUrl, IsActive
                  FROM dbo.AppDownloadSetting
                  ORDER BY AppDownloadSettingId")
                .FirstOrDefault() ?? GetDefaultAdminSetting();
        }

        public AppDownloadDisplayVM GetDisplaySetting()
        {
            EnsureTable();

            return _db.Database.SqlQuery<AppDownloadDisplayVM>(
                @"SELECT TOP 1 KickerText, HeadingText, SubHeadingText,
                         GooglePlayUrl, AppleAppStoreUrl, IsActive
                  FROM dbo.AppDownloadSetting
                  WHERE IsActive = 1
                  ORDER BY AppDownloadSettingId")
                .FirstOrDefault() ?? GetDefaultDisplaySetting();
        }

        public void Save(AppDownloadSettingVM model)
        {
            EnsureTable();

            _db.Database.ExecuteSqlCommand(
                @"UPDATE dbo.AppDownloadSetting
                  SET KickerText = @KickerText,
                      HeadingText = @HeadingText,
                      SubHeadingText = @SubHeadingText,
                      GooglePlayUrl = @GooglePlayUrl,
                      AppleAppStoreUrl = @AppleAppStoreUrl,
                      IsActive = @IsActive,
                      ModifiedDate = GETDATE()
                  WHERE AppDownloadSettingId = 1",
                new SqlParameter("@KickerText", model.KickerText ?? ""),
                new SqlParameter("@HeadingText", model.HeadingText ?? ""),
                new SqlParameter("@SubHeadingText", (object)model.SubHeadingText ?? DBNull.Value),
                new SqlParameter("@GooglePlayUrl", model.GooglePlayUrl ?? DefaultGooglePlayUrl),
                new SqlParameter("@AppleAppStoreUrl", model.AppleAppStoreUrl ?? DefaultAppleAppStoreUrl),
                new SqlParameter("@IsActive", model.IsActive));
        }

        private void EnsureTable()
        {
            _db.Database.ExecuteSqlCommand(
                @"IF OBJECT_ID('dbo.AppDownloadSetting', 'U') IS NULL
                  BEGIN
                      CREATE TABLE dbo.AppDownloadSetting
                      (
                          AppDownloadSettingId INT NOT NULL PRIMARY KEY,
                          KickerText NVARCHAR(120) NOT NULL,
                          HeadingText NVARCHAR(180) NOT NULL,
                          SubHeadingText NVARCHAR(220) NULL,
                          GooglePlayUrl NVARCHAR(600) NOT NULL,
                          AppleAppStoreUrl NVARCHAR(600) NOT NULL,
                          IsActive BIT NOT NULL CONSTRAINT DF_AppDownloadSetting_IsActive DEFAULT(1),
                          CreatedDate DATETIME NOT NULL CONSTRAINT DF_AppDownloadSetting_CreatedDate DEFAULT(GETDATE()),
                          ModifiedDate DATETIME NULL
                      );
                  END

                  IF NOT EXISTS (SELECT 1 FROM dbo.AppDownloadSetting WHERE AppDownloadSettingId = 1)
                  BEGIN
                      INSERT INTO dbo.AppDownloadSetting
                      (
                          AppDownloadSettingId, KickerText, HeadingText, SubHeadingText,
                          GooglePlayUrl, AppleAppStoreUrl, IsActive, CreatedDate
                      )
                      VALUES
                      (
                          1, 'Bharatiya Janata Party', 'Download The App Now',
                          'Stay connected with organisation updates, campaigns and public outreach.',
                          'https://play.google.com/store/search?q=Bharatiya%20Janata%20Party&c=apps',
                          'https://apps.apple.com/in/search?term=Bharatiya%20Janata%20Party',
                          1, GETDATE()
                      );
                  END");
        }

        private static AppDownloadSettingVM GetDefaultAdminSetting()
        {
            return new AppDownloadSettingVM
            {
                AppDownloadSettingId = 1,
                KickerText = "Bharatiya Janata Party",
                HeadingText = "Download The App Now",
                SubHeadingText = "Stay connected with organisation updates, campaigns and public outreach.",
                GooglePlayUrl = DefaultGooglePlayUrl,
                AppleAppStoreUrl = DefaultAppleAppStoreUrl,
                IsActive = true
            };
        }

        private static AppDownloadDisplayVM GetDefaultDisplaySetting()
        {
            return new AppDownloadDisplayVM
            {
                KickerText = "Bharatiya Janata Party",
                HeadingText = "Download The App Now",
                SubHeadingText = "Stay connected with organisation updates, campaigns and public outreach.",
                GooglePlayUrl = DefaultGooglePlayUrl,
                AppleAppStoreUrl = DefaultAppleAppStoreUrl,
                IsActive = true
            };
        }
    }
}
