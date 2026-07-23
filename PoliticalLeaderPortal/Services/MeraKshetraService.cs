using PoliticalLeaderPortal.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;

namespace PoliticalLeaderPortal.Services
{
    public class MeraKshetraService
    {
        private readonly string _connectionString;

        public MeraKshetraService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["PoliticalLeaderPortalDbEntities1"].ConnectionString;
            if (_connectionString.IndexOf("metadata=", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var builder = new System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder(_connectionString);
                _connectionString = builder.ProviderConnectionString;
            }
        }

        public List<MeraKshetraItemVM> GetAll()
        {
            EnsureTable();

            return Query("SELECT * FROM dbo.MeraKshetraItem ORDER BY DisplayOrder, Title");
        }

        public List<MeraKshetraItemVM> GetActive()
        {
            EnsureTable();
            EnsureSeedData();

            return Query("SELECT * FROM dbo.MeraKshetraItem WHERE IsActive = 1 ORDER BY DisplayOrder, Title");
        }

        public MeraKshetraItemVM GetById(int id)
        {
            EnsureTable();

            return Query("SELECT * FROM dbo.MeraKshetraItem WHERE MeraKshetraItemId = @Id",
                new SqlParameter("@Id", id)).FirstOrDefault();
        }

        public void Create(MeraKshetraItemVM model, HttpServerUtilityBase server)
        {
            EnsureTable();

            model.ImagePath = SaveImage(model.ImageFile, server, null);

            Execute(@"INSERT INTO dbo.MeraKshetraItem
                    (ModuleType, Title, ShortTitle, Description, ImagePath, IconClass, LocationName, DistanceText,
                     SourceUrl, SourceName, DisplayOrder, IsFeatured, IsActive, CreatedDate)
                    VALUES
                    (@ModuleType, @Title, @ShortTitle, @Description, @ImagePath, @IconClass, @LocationName, @DistanceText,
                     @SourceUrl, @SourceName, @DisplayOrder, @IsFeatured, @IsActive, GETDATE())",
                Parameters(model).ToArray());
        }

        public void Update(MeraKshetraItemVM model, HttpServerUtilityBase server)
        {
            EnsureTable();

            var existing = GetById(model.MeraKshetraItemId);
            if (existing == null)
            {
                throw new InvalidOperationException("Mera Kshetra record not found.");
            }

            model.ImagePath = SaveImage(model.ImageFile, server, existing.ImagePath);

            var parameters = Parameters(model).ToList();
            parameters.Add(new SqlParameter("@Id", model.MeraKshetraItemId));

            Execute(@"UPDATE dbo.MeraKshetraItem
                    SET ModuleType = @ModuleType,
                        Title = @Title,
                        ShortTitle = @ShortTitle,
                        Description = @Description,
                        ImagePath = @ImagePath,
                        IconClass = @IconClass,
                        LocationName = @LocationName,
                        DistanceText = @DistanceText,
                        SourceUrl = @SourceUrl,
                        SourceName = @SourceName,
                        DisplayOrder = @DisplayOrder,
                        IsFeatured = @IsFeatured,
                        IsActive = @IsActive,
                        ModifiedDate = GETDATE()
                    WHERE MeraKshetraItemId = @Id",
                parameters.ToArray());
        }

        public void Delete(int id)
        {
            EnsureTable();

            Execute("DELETE FROM dbo.MeraKshetraItem WHERE MeraKshetraItemId = @Id",
                new SqlParameter("@Id", id));
        }

        private void EnsureTable()
        {
            Execute(@"IF OBJECT_ID('dbo.MeraKshetraItem', 'U') IS NULL
                    BEGIN
                        CREATE TABLE dbo.MeraKshetraItem
                        (
                            MeraKshetraItemId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            ModuleType NVARCHAR(80) NOT NULL,
                            Title NVARCHAR(160) NOT NULL,
                            ShortTitle NVARCHAR(220) NULL,
                            Description NVARCHAR(MAX) NULL,
                            ImagePath NVARCHAR(500) NULL,
                            IconClass NVARCHAR(80) NULL,
                            LocationName NVARCHAR(120) NULL,
                            DistanceText NVARCHAR(80) NULL,
                            SourceUrl NVARCHAR(500) NULL,
                            SourceName NVARCHAR(80) NULL,
                            DisplayOrder INT NOT NULL CONSTRAINT DF_MeraKshetraItem_DisplayOrder DEFAULT(1),
                            IsFeatured BIT NOT NULL CONSTRAINT DF_MeraKshetraItem_IsFeatured DEFAULT(0),
                            IsActive BIT NOT NULL CONSTRAINT DF_MeraKshetraItem_IsActive DEFAULT(1),
                            CreatedDate DATETIME NOT NULL CONSTRAINT DF_MeraKshetraItem_CreatedDate DEFAULT(GETDATE()),
                            ModifiedDate DATETIME NULL
                        );
                    END");
        }

        private void EnsureSeedData()
        {
            Execute(@"IF NOT EXISTS (SELECT 1 FROM dbo.MeraKshetraItem)
                    BEGIN
                        INSERT INTO dbo.MeraKshetraItem
                        (ModuleType, Title, ShortTitle, Description, IconClass, LocationName, DistanceText, SourceUrl, SourceName, DisplayOrder, IsFeatured, IsActive)
                        VALUES
                        (N'Heritage', N'Basilica of Our Lady of Graces', N'Sardhana Church', N'A landmark heritage place associated with Begum Samru and one of Sardhana''s most recognised public identities.', N'bi bi-building', N'Sardhana', N'Local landmark', N'https://en.wikipedia.org/wiki/Basilica_of_Our_Lady_of_Graces', N'Wikipedia', 1, 1, 1),
                        (N'Heritage', N'Begum Samru Heritage', N'Historic Sardhana legacy', N'Sardhana is strongly linked with Begum Samru, her principality, and the region''s distinctive historical identity.', N'bi bi-person-vcard', N'Sardhana', N'Local history', N'https://en.wikipedia.org/wiki/Begum_Samru', N'Wikipedia', 2, 1, 1),
                        (N'Nearby Place', N'Meerut City', N'Administration, markets and transport', N'A major nearby city for education, healthcare, administration, business, and transport connectivity.', N'bi bi-map', N'Meerut', N'About 20 km', N'https://en.wikipedia.org/wiki/Sardhana', N'Wikipedia', 3, 0, 1),
                        (N'Development', N'Major Dhyan Chand Sports University Area', N'Youth and sports corridor', N'Reported sports-university development in the Sardhana area can strengthen youth, training, and local opportunity.', N'bi bi-trophy', N'Sardhana area', N'Nearby', N'https://timesofindia.indiatimes.com/city/meerut/major-dhyan-chand-sports-university-to-open-first-phase-in-2026/articleshow/124251556.cms', N'Times of India', 4, 0, 1),
                        (N'Citizen Service', N'Villages and Wards', N'Local public-service network', N'Use this module for village and ward updates, public meetings, complaints, volunteer activity, and citizen support.', N'bi bi-people', N'Sardhana constituency', N'Constituency-wide', NULL, NULL, 5, 0, 1);
                    END");
        }

        private List<MeraKshetraItemVM> Query(string sql, params SqlParameter[] parameters)
        {
            var result = new List<MeraKshetraItemVM>();

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(Map(reader));
                    }
                }
            }

            return result;
        }

        private void Execute(string sql, params SqlParameter[] parameters)
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

        private static MeraKshetraItemVM Map(IDataRecord record)
        {
            return new MeraKshetraItemVM
            {
                MeraKshetraItemId = Convert.ToInt32(record["MeraKshetraItemId"]),
                ModuleType = Convert.ToString(record["ModuleType"]),
                Title = Convert.ToString(record["Title"]),
                ShortTitle = Convert.ToString(record["ShortTitle"]),
                Description = Convert.ToString(record["Description"]),
                ImagePath = Convert.ToString(record["ImagePath"]),
                IconClass = Convert.ToString(record["IconClass"]),
                LocationName = Convert.ToString(record["LocationName"]),
                DistanceText = Convert.ToString(record["DistanceText"]),
                SourceUrl = Convert.ToString(record["SourceUrl"]),
                SourceName = Convert.ToString(record["SourceName"]),
                DisplayOrder = Convert.ToInt32(record["DisplayOrder"]),
                IsFeatured = Convert.ToBoolean(record["IsFeatured"]),
                IsActive = Convert.ToBoolean(record["IsActive"]),
                CreatedDate = Convert.ToDateTime(record["CreatedDate"])
            };
        }

        private static IEnumerable<SqlParameter> Parameters(MeraKshetraItemVM model)
        {
            yield return new SqlParameter("@ModuleType", (object)model.ModuleType ?? DBNull.Value);
            yield return new SqlParameter("@Title", (object)model.Title ?? DBNull.Value);
            yield return new SqlParameter("@ShortTitle", (object)model.ShortTitle ?? DBNull.Value);
            yield return new SqlParameter("@Description", (object)model.Description ?? DBNull.Value);
            yield return new SqlParameter("@ImagePath", (object)model.ImagePath ?? DBNull.Value);
            yield return new SqlParameter("@IconClass", (object)model.IconClass ?? DBNull.Value);
            yield return new SqlParameter("@LocationName", (object)model.LocationName ?? DBNull.Value);
            yield return new SqlParameter("@DistanceText", (object)model.DistanceText ?? DBNull.Value);
            yield return new SqlParameter("@SourceUrl", (object)model.SourceUrl ?? DBNull.Value);
            yield return new SqlParameter("@SourceName", (object)model.SourceName ?? DBNull.Value);
            yield return new SqlParameter("@DisplayOrder", model.DisplayOrder);
            yield return new SqlParameter("@IsFeatured", model.IsFeatured);
            yield return new SqlParameter("@IsActive", model.IsActive);
        }

        private static string SaveImage(HttpPostedFileBase file, HttpServerUtilityBase server, string existingPath)
        {
            if (file == null || file.ContentLength <= 0)
            {
                return existingPath;
            }

            var extension = Path.GetExtension(file.FileName);
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

            if (string.IsNullOrWhiteSpace(extension) ||
                !allowed.Contains(extension.ToLowerInvariant()))
            {
                throw new InvalidOperationException("Only JPG, PNG, WEBP and GIF images are allowed.");
            }

            var folder = server.MapPath("~/Uploads/MeraKshetra");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var fileName = "kshetra_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + extension.ToLowerInvariant();
            var physicalPath = Path.Combine(folder, fileName);
            file.SaveAs(physicalPath);

            return "~/Uploads/MeraKshetra/" + fileName;
        }
    }
}
