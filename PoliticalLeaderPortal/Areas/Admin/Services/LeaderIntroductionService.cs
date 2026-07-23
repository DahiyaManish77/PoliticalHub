using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using System;
using System.Configuration;
using System.Data;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;

namespace PoliticalLeaderPortal.Areas.Admin.Services
{
    public class LeaderIntroductionService
    {
        private readonly string _connectionString;

        public LeaderIntroductionService()
        {
            _connectionString = GetProviderConnectionString();
        }

        public LeaderIntroductionVM GetSetting()
        {
            EnsureTable();

            var model = QuerySingle(@"SELECT TOP 1 LeaderIntroductionId, LeaderName, RoleTagline, SectionLabel,
                    IntroText, SecondaryText, VisionTitle, VisionText, MissionTitle, MissionText,
                    PortraitImagePath, StatusTitle, StatusText, PrimaryButtonText, PrimaryButtonUrl,
                    SecondaryButtonText, SecondaryButtonUrl, IsActive
                FROM dbo.LeaderIntroduction
                ORDER BY LeaderIntroductionId;");

            return model ?? GetDefaultModel();
        }

        public LeaderIntroductionVM GetDisplaySetting()
        {
            EnsureTable();

            var model = QuerySingle(@"SELECT TOP 1 LeaderIntroductionId, LeaderName, RoleTagline, SectionLabel,
                    IntroText, SecondaryText, VisionTitle, VisionText, MissionTitle, MissionText,
                    PortraitImagePath, StatusTitle, StatusText, PrimaryButtonText, PrimaryButtonUrl,
                    SecondaryButtonText, SecondaryButtonUrl, IsActive
                FROM dbo.LeaderIntroduction
                WHERE IsActive = 1
                ORDER BY LeaderIntroductionId;");

            return model ?? GetDefaultModel();
        }

        public void Save(LeaderIntroductionVM model)
        {
            if (model == null)
            {
                return;
            }

            EnsureTable();

            ExecuteSql(@"UPDATE dbo.LeaderIntroduction
                SET LeaderName = @LeaderName,
                    RoleTagline = @RoleTagline,
                    SectionLabel = @SectionLabel,
                    IntroText = @IntroText,
                    SecondaryText = @SecondaryText,
                    VisionTitle = @VisionTitle,
                    VisionText = @VisionText,
                    MissionTitle = @MissionTitle,
                    MissionText = @MissionText,
                    PortraitImagePath = @PortraitImagePath,
                    StatusTitle = @StatusTitle,
                    StatusText = @StatusText,
                    PrimaryButtonText = @PrimaryButtonText,
                    PrimaryButtonUrl = @PrimaryButtonUrl,
                    SecondaryButtonText = @SecondaryButtonText,
                    SecondaryButtonUrl = @SecondaryButtonUrl,
                    IsActive = @IsActive,
                    UpdatedDate = GETDATE()
                WHERE LeaderIntroductionId = @LeaderIntroductionId;",
                Param("@LeaderName", model.LeaderName),
                Param("@RoleTagline", model.RoleTagline),
                Param("@SectionLabel", model.SectionLabel),
                Param("@IntroText", model.IntroText),
                Param("@SecondaryText", model.SecondaryText),
                Param("@VisionTitle", model.VisionTitle),
                Param("@VisionText", model.VisionText),
                Param("@MissionTitle", model.MissionTitle),
                Param("@MissionText", model.MissionText),
                Param("@PortraitImagePath", model.PortraitImagePath),
                Param("@StatusTitle", model.StatusTitle),
                Param("@StatusText", model.StatusText),
                Param("@PrimaryButtonText", model.PrimaryButtonText),
                Param("@PrimaryButtonUrl", model.PrimaryButtonUrl),
                Param("@SecondaryButtonText", model.SecondaryButtonText),
                Param("@SecondaryButtonUrl", model.SecondaryButtonUrl),
                new SqlParameter("@IsActive", model.IsActive),
                new SqlParameter("@LeaderIntroductionId", model.LeaderIntroductionId));
        }

        private void EnsureTable()
        {
            ExecuteSql(@"IF OBJECT_ID('dbo.LeaderIntroduction', 'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.LeaderIntroduction
                    (
                        LeaderIntroductionId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        LeaderName NVARCHAR(150) NOT NULL,
                        RoleTagline NVARCHAR(250) NULL,
                        SectionLabel NVARCHAR(80) NULL,
                        IntroText NVARCHAR(MAX) NULL,
                        SecondaryText NVARCHAR(MAX) NULL,
                        VisionTitle NVARCHAR(150) NULL,
                        VisionText NVARCHAR(500) NULL,
                        MissionTitle NVARCHAR(150) NULL,
                        MissionText NVARCHAR(500) NULL,
                        PortraitImagePath NVARCHAR(500) NULL,
                        StatusTitle NVARCHAR(150) NULL,
                        StatusText NVARCHAR(250) NULL,
                        PrimaryButtonText NVARCHAR(80) NULL,
                        PrimaryButtonUrl NVARCHAR(300) NULL,
                        SecondaryButtonText NVARCHAR(80) NULL,
                        SecondaryButtonUrl NVARCHAR(300) NULL,
                        IsActive BIT NOT NULL DEFAULT(1),
                        UpdatedDate DATETIME NULL
                    );
                END");

            ExecuteSql(@"IF NOT EXISTS (SELECT 1 FROM dbo.LeaderIntroduction)
                BEGIN
                    INSERT INTO dbo.LeaderIntroduction
                    (
                        LeaderName, RoleTagline, SectionLabel, IntroText, SecondaryText,
                        VisionTitle, VisionText, MissionTitle, MissionText, PortraitImagePath,
                        StatusTitle, StatusText, PrimaryButtonText, PrimaryButtonUrl,
                        SecondaryButtonText, SecondaryButtonUrl, IsActive, UpdatedDate
                    )
                    VALUES
                    (
                        N'Sangeet Som',
                        N'Public service, development and grassroots coordination',
                        N'Leader Introduction',
                        N'This official platform keeps citizens, supporters, volunteers and campaign teams connected through one clear public website and a structured campaign management system.',
                        N'The mission is to make public communication faster, event planning more organized, and constituency feedback easier to track from village, ward and booth level.',
                        N'Development With Accountability',
                        N'Clear priorities for infrastructure, youth, farmers, public welfare and responsive representation.',
                        N'Connect Every Citizen',
                        N'Booth-level outreach, volunteer coordination, feedback tracking and transparent public communication.',
                        N'~/Content/images/leader.png',
                        N'Public Service Desk',
                        N'Citizen connect, events and campaign updates',
                        N'Read Full Profile',
                        N'/AboutLeader/Biography',
                        N'Join As Volunteer',
                        N'/CitizenConnect/Volunteer',
                        1,
                        GETDATE()
                    );
                END");
        }

        private LeaderIntroductionVM QuerySingle(string sql)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    return new LeaderIntroductionVM
                    {
                        LeaderIntroductionId = Convert.ToInt32(reader["LeaderIntroductionId"]),
                        LeaderName = Convert.ToString(reader["LeaderName"]),
                        RoleTagline = Convert.ToString(reader["RoleTagline"]),
                        SectionLabel = Convert.ToString(reader["SectionLabel"]),
                        IntroText = Convert.ToString(reader["IntroText"]),
                        SecondaryText = Convert.ToString(reader["SecondaryText"]),
                        VisionTitle = Convert.ToString(reader["VisionTitle"]),
                        VisionText = Convert.ToString(reader["VisionText"]),
                        MissionTitle = Convert.ToString(reader["MissionTitle"]),
                        MissionText = Convert.ToString(reader["MissionText"]),
                        PortraitImagePath = Convert.ToString(reader["PortraitImagePath"]),
                        StatusTitle = Convert.ToString(reader["StatusTitle"]),
                        StatusText = Convert.ToString(reader["StatusText"]),
                        PrimaryButtonText = Convert.ToString(reader["PrimaryButtonText"]),
                        PrimaryButtonUrl = Convert.ToString(reader["PrimaryButtonUrl"]),
                        SecondaryButtonText = Convert.ToString(reader["SecondaryButtonText"]),
                        SecondaryButtonUrl = Convert.ToString(reader["SecondaryButtonUrl"]),
                        IsActive = Convert.ToBoolean(reader["IsActive"])
                    };
                }
            }
        }

        private void ExecuteSql(string sql, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static SqlParameter Param(string name, string value)
        {
            return new SqlParameter(name, (object)value ?? DBNull.Value);
        }

        private static string GetProviderConnectionString()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["PoliticalLeaderPortalDbEntities1"].ConnectionString;
            if (connectionString.IndexOf("metadata=", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var builder = new EntityConnectionStringBuilder(connectionString);
                return builder.ProviderConnectionString;
            }

            return connectionString;
        }

        private static LeaderIntroductionVM GetDefaultModel()
        {
            return new LeaderIntroductionVM
            {
                LeaderName = "Sangeet Som",
                RoleTagline = "Public service, development and grassroots coordination",
                SectionLabel = "Leader Introduction",
                IntroText = "This official platform keeps citizens, supporters, volunteers and campaign teams connected through one clear public website and a structured campaign management system.",
                SecondaryText = "The mission is to make public communication faster, event planning more organized, and constituency feedback easier to track from village, ward and booth level.",
                VisionTitle = "Development With Accountability",
                VisionText = "Clear priorities for infrastructure, youth, farmers, public welfare and responsive representation.",
                MissionTitle = "Connect Every Citizen",
                MissionText = "Booth-level outreach, volunteer coordination, feedback tracking and transparent public communication.",
                PortraitImagePath = "~/Content/images/leader.png",
                StatusTitle = "Public Service Desk",
                StatusText = "Citizen connect, events and campaign updates",
                PrimaryButtonText = "Read Full Profile",
                PrimaryButtonUrl = "/AboutLeader/Biography",
                SecondaryButtonText = "Join As Volunteer",
                SecondaryButtonUrl = "/CitizenConnect/Volunteer",
                IsActive = true
            };
        }
    }
}
