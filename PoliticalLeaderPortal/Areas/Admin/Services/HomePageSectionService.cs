using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;

namespace PoliticalLeaderPortal.Areas.Admin.Services
{
    public class HomePageSectionService
    {
        private readonly string _connectionString;

        public HomePageSectionService()
        {
            _connectionString = GetProviderConnectionString();
        }

        public HomePageSectionPageVM GetPage()
        {
            return new HomePageSectionPageVM
            {
                Sections = GetAllSections()
            };
        }

        public IList<HomePageSectionVM> GetAllSections()
        {
            EnsureHomePageSectionTable();

            return QuerySections(@"SELECT HomePageSectionId, SectionKey, SectionName, Description, RenderType,
                    ControllerName, ActionName, PartialViewPath, DisplayOrder, IsVisible, IsSystem,
                    StartDate, EndDate, UpdatedDate
                FROM dbo.HomePageSection
                ORDER BY DisplayOrder, HomePageSectionId;");
        }

        public IList<HomePageSectionVM> GetVisibleSectionsForHome()
        {
            EnsureHomePageSectionTable();

            return QuerySections(@"SELECT HomePageSectionId, SectionKey, SectionName, Description, RenderType,
                    ControllerName, ActionName, PartialViewPath, DisplayOrder, IsVisible, IsSystem,
                    StartDate, EndDate, UpdatedDate
                FROM dbo.HomePageSection
                WHERE IsVisible = 1
                  AND (StartDate IS NULL OR StartDate <= GETDATE())
                  AND (EndDate IS NULL OR EndDate >= GETDATE())
                ORDER BY DisplayOrder, HomePageSectionId;");
        }

        public void SaveSections(HomePageSectionPageVM model)
        {
            if (model == null || model.Sections == null)
            {
                return;
            }

            EnsureHomePageSectionTable();

            foreach (var section in model.Sections)
            {
                ExecuteSql(@"UPDATE dbo.HomePageSection
                    SET DisplayOrder = @DisplayOrder,
                        IsVisible = @IsVisible,
                        StartDate = @StartDate,
                        EndDate = @EndDate,
                        UpdatedDate = GETDATE()
                    WHERE HomePageSectionId = @HomePageSectionId;",
                    new SqlParameter("@DisplayOrder", section.DisplayOrder),
                    new SqlParameter("@IsVisible", section.IsVisible),
                    new SqlParameter("@StartDate", (object)section.StartDate ?? DBNull.Value),
                    new SqlParameter("@EndDate", (object)section.EndDate ?? DBNull.Value),
                    new SqlParameter("@HomePageSectionId", section.HomePageSectionId));
            }
        }

        private void EnsureHomePageSectionTable()
        {
            ExecuteSql(@"IF OBJECT_ID('dbo.HomePageSection', 'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.HomePageSection
                    (
                        HomePageSectionId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        SectionKey NVARCHAR(80) NOT NULL UNIQUE,
                        SectionName NVARCHAR(150) NOT NULL,
                        Description NVARCHAR(400) NULL,
                        RenderType NVARCHAR(30) NOT NULL,
                        ControllerName NVARCHAR(100) NULL,
                        ActionName NVARCHAR(100) NULL,
                        PartialViewPath NVARCHAR(300) NULL,
                        DisplayOrder INT NOT NULL DEFAULT(0),
                        IsVisible BIT NOT NULL DEFAULT(1),
                        IsSystem BIT NOT NULL DEFAULT(1),
                        StartDate DATETIME NULL,
                        EndDate DATETIME NULL,
                        UpdatedDate DATETIME NULL
                    );
                END");

            foreach (var section in GetDefaultSections())
            {
                ExecuteSql(@"IF NOT EXISTS (SELECT 1 FROM dbo.HomePageSection WHERE SectionKey = @SectionKey)
                    BEGIN
                        INSERT INTO dbo.HomePageSection
                        (
                            SectionKey, SectionName, Description, RenderType, ControllerName, ActionName,
                            PartialViewPath, DisplayOrder, IsVisible, IsSystem, UpdatedDate
                        )
                        VALUES
                        (
                            @SectionKey, @SectionName, @Description, @RenderType, @ControllerName, @ActionName,
                            @PartialViewPath, @DisplayOrder, @IsVisible, @IsSystem, GETDATE()
                        );
                    END",
                    new SqlParameter("@SectionKey", section.SectionKey),
                    new SqlParameter("@SectionName", section.SectionName),
                    new SqlParameter("@Description", (object)section.Description ?? DBNull.Value),
                    new SqlParameter("@RenderType", section.RenderType),
                    new SqlParameter("@ControllerName", (object)section.ControllerName ?? DBNull.Value),
                    new SqlParameter("@ActionName", (object)section.ActionName ?? DBNull.Value),
                    new SqlParameter("@PartialViewPath", (object)section.PartialViewPath ?? DBNull.Value),
                    new SqlParameter("@DisplayOrder", section.DisplayOrder),
                    new SqlParameter("@IsVisible", section.IsVisible),
                    new SqlParameter("@IsSystem", section.IsSystem));
            }
        }

        private IList<HomePageSectionVM> QuerySections(string sql)
        {
            var sections = new List<HomePageSectionVM>();

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (reader.Read())
                    {
                        sections.Add(new HomePageSectionVM
                        {
                            HomePageSectionId = Convert.ToInt32(reader["HomePageSectionId"]),
                            SectionKey = Convert.ToString(reader["SectionKey"]),
                            SectionName = Convert.ToString(reader["SectionName"]),
                            Description = Convert.ToString(reader["Description"]),
                            RenderType = Convert.ToString(reader["RenderType"]),
                            ControllerName = Convert.ToString(reader["ControllerName"]),
                            ActionName = Convert.ToString(reader["ActionName"]),
                            PartialViewPath = Convert.ToString(reader["PartialViewPath"]),
                            DisplayOrder = Convert.ToInt32(reader["DisplayOrder"]),
                            IsVisible = Convert.ToBoolean(reader["IsVisible"]),
                            IsSystem = Convert.ToBoolean(reader["IsSystem"]),
                            StartDate = reader["StartDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["StartDate"]),
                            EndDate = reader["EndDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EndDate"]),
                            UpdatedDate = reader["UpdatedDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["UpdatedDate"])
                        });
                    }
                }
            }

            return sections;
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

        private static IList<HomePageSectionVM> GetDefaultSections()
        {
            return new List<HomePageSectionVM>
            {
                BuildAction("HeroSlider", "Hero Slider", "Homepage banner slides.", "Layout", "HeroSlider", 10),
                BuildPartial("LeaderIntroduction", "Leader Introduction", "Profile, message and primary introduction.", "~/Views/Home/Partials/_LeaderIntroduction.cshtml", 20),
                BuildAction("HomeStatistics", "Achievements & Stats", "Homepage achievement counters.", "Layout", "HomeStatistics", 30),
                BuildAction("UpcomingEvents", "Upcoming Events", "Upcoming public and campaign events.", "Layout", "UpcomingEvents", 40),
                BuildAction("LatestNews", "Latest Campaign Activities", "News and campaign updates.", "Layout", "LatestNews", 50),
                BuildAction("PhotoGallery", "Image Gallery", "Public image gallery.", "Layout", "PhotoGallery", 60),
                BuildAction("VideoGallery", "Video Gallery", "Public video gallery.", "Layout", "VideoGallery", 70),
                BuildAction("HomeMembers", "Team Members", "Visible office bearers and public team members.", "Layout", "HomeMembers", 80),
                BuildAction("MediaCoverage", "Media Coverage", "Press and media highlights.", "Layout", "MediaCoverage", 90),
                BuildAction("CitizenConnect", "Citizen Connect", "Contact, suggestion and volunteer forms for the homepage.", "CitizenConnect", "HomeSection", 95),
                BuildAction("PollSurvey", "Poll / Survey", "Public feedback and survey callout.", "Layout", "PollSurvey", 100),
                BuildAction("Downloads", "Documents", "Public downloads and resources.", "Layout", "Downloads", 110),
                BuildAction("AppDownload", "App Download", "Mobile app callout.", "Layout", "AppDownload", 120),
                BuildPartial("JoinUsCTA", "Volunteer CTA", "Join the campaign call to action.", "~/Views/Home/Partials/_JoinUsCallToAction.cshtml", 130),
                BuildPartial("ContactCTA", "Contact CTA", "Public contact call to action.", "~/Views/Home/Partials/_ContactCallToAction.cshtml", 140)
            };
        }

        private static HomePageSectionVM BuildAction(string key, string name, string description, string controller, string action, int order)
        {
            return new HomePageSectionVM
            {
                SectionKey = key,
                SectionName = name,
                Description = description,
                RenderType = "Action",
                ControllerName = controller,
                ActionName = action,
                DisplayOrder = order,
                IsVisible = true,
                IsSystem = true
            };
        }

        private static HomePageSectionVM BuildPartial(string key, string name, string description, string partialViewPath, int order)
        {
            return new HomePageSectionVM
            {
                SectionKey = key,
                SectionName = name,
                Description = description,
                RenderType = "Partial",
                PartialViewPath = partialViewPath,
                DisplayOrder = order,
                IsVisible = true,
                IsSystem = true
            };
        }
    }
}
