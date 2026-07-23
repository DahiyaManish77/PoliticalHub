using PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom;
using PoliticalLeaderPortal.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace PoliticalLeaderPortal.Areas.Admin.Services
{
    public class VoterService
    {
        private readonly PoliticalLeaderPortalDbEntities1 db;

        public VoterService()
        {
            db = new PoliticalLeaderPortalDbEntities1();
        }

        public List<VoterVM> GetVoters(string keyword = null, string village = null, string assembly = null, string block = null)
        {
            EnsureVoterTable();

            string search = "%" + (keyword ?? string.Empty).Trim() + "%";
            bool hasKeyword = !string.IsNullOrWhiteSpace(keyword);

            string sql =
                @"SELECT TOP 500 *
                  FROM dbo.ElectionVoter
                  WHERE IsDeleted = 0
                    AND
                    (
                        @HasKeyword = 0
                        OR EpicNumber LIKE @Keyword
                        OR VoterName LIKE @Keyword
                        OR FatherOrHusbandName LIKE @Keyword
                        OR MobileNumber LIKE @Keyword
                        OR BoothNumber LIKE @Keyword
                        OR BoothName LIKE @Keyword
                        OR Village LIKE @Keyword
                        OR Ward LIKE @Keyword
                        OR PoliticalStatus LIKE @Keyword
                    )
                    AND (@Village IS NULL OR Village = @Village)
                    AND (@Assembly IS NULL OR AssemblyConstituency = @Assembly)
                    AND (@Block IS NULL OR Block = @Block)
                  ORDER BY CreatedDate DESC, VoterId DESC";

            return db.Database.SqlQuery<VoterVM>(
                sql,
                new SqlParameter("@HasKeyword", hasKeyword),
                new SqlParameter("@Keyword", search),
                new SqlParameter("@Village", (object)Clean(village) ?? DBNull.Value),
                new SqlParameter("@Assembly", (object)Clean(assembly) ?? DBNull.Value),
                new SqlParameter("@Block", (object)Clean(block) ?? DBNull.Value))
                .ToList();
        }

        public VoterVM GetById(int id)
        {
            EnsureVoterTable();

            return db.Database.SqlQuery<VoterVM>(
                "SELECT TOP 1 * FROM dbo.ElectionVoter WHERE VoterId = @VoterId AND IsDeleted = 0",
                new SqlParameter("@VoterId", id))
                .FirstOrDefault();
        }

        public List<VoterRollPdfVM> GetVoterRollPdfs(string keyword = null, string village = null, string partNumber = null)
        {
            EnsureVoterTable();

            string search = "%" + (keyword ?? string.Empty).Trim() + "%";
            bool hasKeyword = !String.IsNullOrWhiteSpace(keyword);

            return db.Database.SqlQuery<VoterRollPdfVM>(
                @"SELECT TOP 500 *
                  FROM dbo.VoterRollPdf
                  WHERE IsActive = 1
                    AND
                    (
                        @HasKeyword = 0
                        OR State LIKE @Keyword
                        OR District LIKE @Keyword
                        OR AssemblyConstituency LIKE @Keyword
                        OR ParliamentConstituency LIKE @Keyword
                        OR PartNumber LIKE @Keyword
                        OR BoothName LIKE @Keyword
                        OR Village LIKE @Keyword
                        OR Ward LIKE @Keyword
                        OR PollingStation LIKE @Keyword
                    )
                    AND (@Village IS NULL OR Village = @Village)
                    AND (@PartNumber IS NULL OR PartNumber = @PartNumber)
                  ORDER BY TRY_CONVERT(INT, PartNumber), PartNumber, Village, BoothName",
                new SqlParameter("@HasKeyword", hasKeyword),
                new SqlParameter("@Keyword", search),
                new SqlParameter("@Village", (object)Clean(village) ?? DBNull.Value),
                new SqlParameter("@PartNumber", (object)Clean(partNumber) ?? DBNull.Value))
                .ToList();
        }

        public VoterRollPdfVM GetVoterRollPdfById(int id)
        {
            EnsureVoterTable();

            return db.Database.SqlQuery<VoterRollPdfVM>(
                "SELECT TOP 1 * FROM dbo.VoterRollPdf WHERE VoterRollPdfId = @Id AND IsActive = 1",
                new SqlParameter("@Id", id))
                .FirstOrDefault();
        }

        public bool SaveVoterRollPdf(VoterRollPdfVM model, int? userId, out string message)
        {
            EnsureVoterTable();
            NormalizeRoll(model);

            if (IsDuplicateRollPdf(model))
            {
                message = "This booth/part roll PDF is already saved for the same assembly and year.";
                return false;
            }

            db.Database.ExecuteSqlCommand(
                @"INSERT INTO dbo.VoterRollPdf
                  (State,District,AssemblyConstituency,ParliamentConstituency,PartNumber,BoothName,Village,Ward,PollingStation,SourceUrl,PdfFilePath,RollYear,RevisionType,PublishedDate,DownloadDate,Notes,IsActive,CreatedBy,CreatedDate)
                  VALUES
                  (@State,@District,@AssemblyConstituency,@ParliamentConstituency,@PartNumber,@BoothName,@Village,@Ward,@PollingStation,@SourceUrl,@PdfFilePath,@RollYear,@RevisionType,@PublishedDate,@DownloadDate,@Notes,1,@CreatedBy,GETDATE())",
                RollParameters(model, userId, false).ToArray());

            message = "Official voter roll PDF saved successfully.";
            return true;
        }

        public bool UpdateVoterRollPdf(VoterRollPdfVM model, int? userId, out string message)
        {
            EnsureVoterTable();
            NormalizeRoll(model);

            if (IsDuplicateRollPdf(model))
            {
                message = "Another PDF already exists for this booth/part, assembly and year.";
                return false;
            }

            int affected = db.Database.ExecuteSqlCommand(
                @"UPDATE dbo.VoterRollPdf
                  SET State=@State,District=@District,AssemblyConstituency=@AssemblyConstituency,ParliamentConstituency=@ParliamentConstituency,
                      PartNumber=@PartNumber,BoothName=@BoothName,Village=@Village,Ward=@Ward,PollingStation=@PollingStation,
                      SourceUrl=@SourceUrl,PdfFilePath=@PdfFilePath,RollYear=@RollYear,RevisionType=@RevisionType,
                      PublishedDate=@PublishedDate,DownloadDate=@DownloadDate,Notes=@Notes,UpdatedBy=@UpdatedBy,UpdatedDate=GETDATE()
                  WHERE VoterRollPdfId=@VoterRollPdfId AND IsActive=1",
                RollParameters(model, userId, true).ToArray());

            message = affected > 0 ? "Official voter roll PDF updated successfully." : "Voter roll PDF not found.";
            return affected > 0;
        }

        public bool DeleteVoterRollPdf(int id, int? userId)
        {
            EnsureVoterTable();

            return db.Database.ExecuteSqlCommand(
                @"UPDATE dbo.VoterRollPdf
                  SET IsActive = 0, UpdatedBy = @UpdatedBy, UpdatedDate = GETDATE()
                  WHERE VoterRollPdfId = @Id AND IsActive = 1",
                new SqlParameter("@Id", id),
                new SqlParameter("@UpdatedBy", (object)userId ?? DBNull.Value)) > 0;
        }

        public bool Save(VoterVM model, int? userId, out string message)
        {
            EnsureVoterTable();
            Normalize(model);

            if (IsDuplicate(model))
            {
                message = "This voter already exists. Duplicate record was not saved.";
                return false;
            }

            db.Database.ExecuteSqlCommand(
                InsertSql,
                BuildParameters(model, userId, false).ToArray());

            var backupSettings = new VoterBackupSettingsService().Get();

            if (backupSettings.AutoBackupEnabled)
            {
                GenerateBackup();
            }

            message = "Voter saved successfully.";
            return true;
        }

        public bool Update(VoterVM model, int? userId, out string message)
        {
            EnsureVoterTable();
            Normalize(model);

            if (IsDuplicate(model))
            {
                message = "Another voter with same identity already exists.";
                return false;
            }

            int affected = db.Database.ExecuteSqlCommand(
                UpdateSql,
                BuildParameters(model, userId, true).ToArray());

            message = affected > 0
                ? "Voter updated successfully."
                : "Voter record not found.";

            return affected > 0;
        }

        public bool Delete(int id, int? userId)
        {
            EnsureVoterTable();

            return db.Database.ExecuteSqlCommand(
                @"UPDATE dbo.ElectionVoter
                  SET IsDeleted = 1, UpdatedBy = @UpdatedBy, UpdatedDate = GETDATE()
                  WHERE VoterId = @VoterId AND IsDeleted = 0",
                new SqlParameter("@VoterId", id),
                new SqlParameter("@UpdatedBy", (object)userId ?? DBNull.Value)) > 0;
        }

        public List<string> GetLocationOptions(string field, string state = null, string district = null, string block = null, string assembly = null, string parliament = null, string village = null)
        {
            EnsureVoterTable();

            var allowedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "State",
                "District",
                "Block",
                "AssemblyConstituency",
                "ParliamentConstituency",
                "Village",
                "Ward"
            };

            if (!allowedFields.Contains(field ?? ""))
            {
                return new List<string>();
            }

            string sql =
                @"SELECT DISTINCT " + field + @"
                  FROM dbo.ElectionLocationMaster
                  WHERE IsActive = 1
                    AND (@State IS NULL OR State = @State)
                    AND (@District IS NULL OR District = @District)
                    AND (@Block IS NULL OR Block = @Block)
                    AND (@Assembly IS NULL OR AssemblyConstituency = @Assembly)
                    AND (@Parliament IS NULL OR ParliamentConstituency = @Parliament)
                    AND (@Village IS NULL OR Village = @Village)
                    AND " + field + @" IS NOT NULL
                    AND LTRIM(RTRIM(" + field + @")) <> ''
                  ORDER BY " + field;

            return db.Database.SqlQuery<string>(
                sql,
                new SqlParameter("@State", (object)Clean(state) ?? DBNull.Value),
                new SqlParameter("@District", (object)Clean(district) ?? DBNull.Value),
                new SqlParameter("@Block", (object)Clean(block) ?? DBNull.Value),
                new SqlParameter("@Assembly", (object)Clean(assembly) ?? DBNull.Value),
                new SqlParameter("@Parliament", (object)Clean(parliament) ?? DBNull.Value),
                new SqlParameter("@Village", (object)Clean(village) ?? DBNull.Value))
                .ToList();
        }

        public string GenerateBackup()
        {
            EnsureVoterTable();

            var settingsService = new VoterBackupSettingsService();
            var settings = settingsService.Get();

            string backupFolder = GetBackupFolder();

            if (!Directory.Exists(backupFolder))
            {
                Directory.CreateDirectory(backupFolder);
            }

            string fileName = "VoterBackup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
            string filePath = Path.Combine(backupFolder, fileName);
            var voters = GetVoters();
            var csv = new StringBuilder();

            csv.AppendLine("VoterId,EPIC,VoterName,FatherOrHusbandName,Gender,Age,MobileNumber,AadhaarNumber,State,District,AssemblyConstituency,ParliamentConstituency,Block,Village,Ward,PartNumber,SectionNumber,SerialNumber,HouseNumber,BoothNumber,BoothName,PollingStation,VoterType,PoliticalStatus,SupportLevel,IsPriorityVoter,IsFirstTimeVoter,VoterPhotoPath,AadhaarPhotoPath,CreatedDate");

            foreach (var voter in voters)
            {
                csv.AppendLine(String.Join(",", new[]
                {
                    Csv(voter.VoterId),
                    Csv(voter.EpicNumber),
                    Csv(voter.VoterName),
                    Csv(voter.FatherOrHusbandName),
                    Csv(voter.Gender),
                    Csv(voter.Age),
                    Csv(voter.MobileNumber),
                    Csv(voter.AadhaarNumber),
                    Csv(voter.State),
                    Csv(voter.District),
                    Csv(voter.AssemblyConstituency),
                    Csv(voter.ParliamentConstituency),
                    Csv(voter.Block),
                    Csv(voter.Village),
                    Csv(voter.Ward),
                    Csv(voter.PartNumber),
                    Csv(voter.SectionNumber),
                    Csv(voter.SerialNumber),
                    Csv(voter.HouseNumber),
                    Csv(voter.BoothNumber),
                    Csv(voter.BoothName),
                    Csv(voter.PollingStation),
                    Csv(voter.VoterType),
                    Csv(voter.PoliticalStatus),
                    Csv(voter.SupportLevel),
                    Csv(voter.IsPriorityVoter),
                    Csv(voter.IsFirstTimeVoter),
                    Csv(voter.VoterPhotoPath),
                    Csv(voter.AadhaarPhotoPath),
                    Csv(voter.CreatedDate)
                }));
            }

            File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
            CleanupOldBackups(backupFolder, settings.KeepLatestFiles);

            string mirrorFilePath = null;
            string status = "Local backup generated successfully.";

            if (settings.MirrorToDriveFolder && !String.IsNullOrWhiteSpace(settings.DriveMirrorFolderPath))
            {
                try
                {
                    if (!Directory.Exists(settings.DriveMirrorFolderPath))
                    {
                        Directory.CreateDirectory(settings.DriveMirrorFolderPath);
                    }

                    mirrorFilePath = Path.Combine(settings.DriveMirrorFolderPath, fileName);
                    File.Copy(filePath, mirrorFilePath, true);
                    status = "Local backup generated and mirrored to Google Drive synced folder.";
                }
                catch (Exception ex)
                {
                    status = "Local backup generated. Mirror failed: " + ex.Message;
                }
            }

            settingsService.MarkBackupResult(filePath, mirrorFilePath, status);

            return filePath;
        }

        public string GetLatestBackupPath()
        {
            string backupFolder = GetBackupFolder();

            if (!Directory.Exists(backupFolder))
            {
                GenerateBackup();
            }

            return Directory.GetFiles(backupFolder, "VoterBackup_*.csv")
                .OrderByDescending(File.GetCreationTime)
                .FirstOrDefault();
        }

        private string GetBackupFolder()
        {
            string root = HttpContext.Current != null
                ? HttpContext.Current.Server.MapPath("~/App_Data/VoterBackups")
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "VoterBackups");

            return root;
        }

        private void CleanupOldBackups(string backupFolder, int keepLatest)
        {
            var oldFiles = Directory.GetFiles(backupFolder, "VoterBackup_*.csv")
                .OrderByDescending(File.GetCreationTime)
                .Skip(keepLatest)
                .ToList();

            foreach (var file in oldFiles)
            {
                File.Delete(file);
            }
        }

        private static string Csv(object value)
        {
            string text = Convert.ToString(value) ?? "";
            return "\"" + text.Replace("\"", "\"\"") + "\"";
        }

        public bool IsDuplicate(VoterVM model)
        {
            if (!string.IsNullOrWhiteSpace(model.EpicNumber))
            {
                int epicCount = db.Database.SqlQuery<int>(
                    @"SELECT COUNT(1)
                      FROM dbo.ElectionVoter
                      WHERE IsDeleted = 0
                        AND VoterId <> @VoterId
                        AND EpicNumber = @EpicNumber",
                    new SqlParameter("@VoterId", model.VoterId),
                    new SqlParameter("@EpicNumber", model.EpicNumber))
                    .FirstOrDefault();

                if (epicCount > 0)
                {
                    return true;
                }
            }

            int identityCount = db.Database.SqlQuery<int>(
                @"SELECT COUNT(1)
                  FROM dbo.ElectionVoter
                  WHERE IsDeleted = 0
                    AND VoterId <> @VoterId
                    AND VoterName = @VoterName
                    AND ISNULL(FatherOrHusbandName, '') = ISNULL(@FatherOrHusbandName, '')
                    AND ISNULL(BoothNumber, '') = ISNULL(@BoothNumber, '')
                    AND
                    (
                        ISNULL(MobileNumber, '') = ISNULL(@MobileNumber, '')
                        OR ISNULL(SerialNumber, '') = ISNULL(@SerialNumber, '')
                    )",
                new SqlParameter("@VoterId", model.VoterId),
                new SqlParameter("@VoterName", model.VoterName),
                new SqlParameter("@FatherOrHusbandName", (object)model.FatherOrHusbandName ?? DBNull.Value),
                new SqlParameter("@BoothNumber", (object)model.BoothNumber ?? DBNull.Value),
                new SqlParameter("@MobileNumber", (object)model.MobileNumber ?? DBNull.Value),
                new SqlParameter("@SerialNumber", (object)model.SerialNumber ?? DBNull.Value))
                .FirstOrDefault();

            return identityCount > 0;
        }

        public void EnsureVoterTable()
        {
            db.Database.ExecuteSqlCommand(
                @"IF OBJECT_ID('dbo.ElectionVoter', 'U') IS NULL
                  BEGIN
                      CREATE TABLE dbo.ElectionVoter
                      (
                          VoterId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ElectionVoter PRIMARY KEY,
                          EpicNumber NVARCHAR(30) NULL,
                          VoterName NVARCHAR(200) NOT NULL,
                          FatherOrHusbandName NVARCHAR(200) NULL,
                          Gender NVARCHAR(20) NULL,
                          Age INT NULL,
                          MobileNumber NVARCHAR(20) NULL,
                          AadhaarNumber NVARCHAR(12) NULL,
                          VoterPhotoPath NVARCHAR(500) NULL,
                          AadhaarPhotoPath NVARCHAR(500) NULL,
                          Caste NVARCHAR(100) NULL,
                          Religion NVARCHAR(100) NULL,
                          Category NVARCHAR(100) NULL,
                          State NVARCHAR(200) NULL,
                          District NVARCHAR(200) NULL,
                          AssemblyConstituency NVARCHAR(200) NULL,
                          ParliamentConstituency NVARCHAR(200) NULL,
                          Block NVARCHAR(200) NULL,
                          Village NVARCHAR(200) NULL,
                          Ward NVARCHAR(200) NULL,
                          PartNumber NVARCHAR(50) NULL,
                          SectionNumber NVARCHAR(50) NULL,
                          SerialNumber NVARCHAR(50) NULL,
                          HouseNumber NVARCHAR(50) NULL,
                          BoothNumber NVARCHAR(200) NULL,
                          BoothName NVARCHAR(250) NULL,
                          PollingStation NVARCHAR(300) NULL,
                          Address NVARCHAR(MAX) NULL,
                          VoterType NVARCHAR(50) NULL,
                          PoliticalStatus NVARCHAR(50) NULL,
                          SupportLevel NVARCHAR(50) NULL,
                          InfluencerName NVARCHAR(200) NULL,
                          PannaPramukhName NVARCHAR(200) NULL,
                          IsPriorityVoter BIT NOT NULL CONSTRAINT DF_ElectionVoter_IsPriority DEFAULT (0),
                          IsFirstTimeVoter BIT NOT NULL CONSTRAINT DF_ElectionVoter_IsFirstTime DEFAULT (0),
                          Remarks NVARCHAR(MAX) NULL,
                          IsDeleted BIT NOT NULL CONSTRAINT DF_ElectionVoter_IsDeleted DEFAULT (0),
                          CreatedBy INT NULL,
                          CreatedDate DATETIME NOT NULL CONSTRAINT DF_ElectionVoter_CreatedDate DEFAULT (GETDATE()),
                          UpdatedBy INT NULL,
                          UpdatedDate DATETIME NULL
                      );

                      CREATE UNIQUE INDEX UX_ElectionVoter_EpicNumber
                      ON dbo.ElectionVoter(EpicNumber)
                      WHERE EpicNumber IS NOT NULL AND IsDeleted = 0;

                      CREATE INDEX IX_ElectionVoter_Search
                      ON dbo.ElectionVoter(BoothNumber, VoterName, MobileNumber, SerialNumber);
                  END

                  IF COL_LENGTH('dbo.ElectionVoter', 'AadhaarNumber') IS NULL
                      ALTER TABLE dbo.ElectionVoter ADD AadhaarNumber NVARCHAR(12) NULL;

                  IF COL_LENGTH('dbo.ElectionVoter', 'VoterPhotoPath') IS NULL
                      ALTER TABLE dbo.ElectionVoter ADD VoterPhotoPath NVARCHAR(500) NULL;

                  IF COL_LENGTH('dbo.ElectionVoter', 'AadhaarPhotoPath') IS NULL
                      ALTER TABLE dbo.ElectionVoter ADD AadhaarPhotoPath NVARCHAR(500) NULL;

                  IF OBJECT_ID('dbo.VoterRollPdf', 'U') IS NULL
                  BEGIN
                      CREATE TABLE dbo.VoterRollPdf
                      (
                          VoterRollPdfId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_VoterRollPdf PRIMARY KEY,
                          State NVARCHAR(100) NOT NULL,
                          District NVARCHAR(100) NOT NULL,
                          AssemblyConstituency NVARCHAR(150) NOT NULL,
                          ParliamentConstituency NVARCHAR(150) NULL,
                          PartNumber NVARCHAR(20) NOT NULL,
                          BoothName NVARCHAR(250) NULL,
                          Village NVARCHAR(150) NULL,
                          Ward NVARCHAR(150) NULL,
                          PollingStation NVARCHAR(300) NULL,
                          SourceUrl NVARCHAR(500) NULL,
                          PdfFilePath NVARCHAR(500) NULL,
                          RollYear INT NOT NULL,
                          RevisionType NVARCHAR(100) NULL,
                          PublishedDate DATETIME NULL,
                          DownloadDate DATETIME NOT NULL CONSTRAINT DF_VoterRollPdf_DownloadDate DEFAULT (GETDATE()),
                          Notes NVARCHAR(MAX) NULL,
                          IsActive BIT NOT NULL CONSTRAINT DF_VoterRollPdf_IsActive DEFAULT (1),
                          CreatedBy INT NULL,
                          CreatedDate DATETIME NOT NULL CONSTRAINT DF_VoterRollPdf_CreatedDate DEFAULT (GETDATE()),
                          UpdatedBy INT NULL,
                          UpdatedDate DATETIME NULL
                      );

                      CREATE INDEX IX_VoterRollPdf_Search
                      ON dbo.VoterRollPdf(AssemblyConstituency, PartNumber, Village, BoothName);
                  END

                  IF OBJECT_ID('dbo.ElectionLocationMaster', 'U') IS NULL
                  BEGIN
                      CREATE TABLE dbo.ElectionLocationMaster
                      (
                          LocationId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ElectionLocationMaster PRIMARY KEY,
                          State NVARCHAR(200) NOT NULL,
                          District NVARCHAR(200) NOT NULL,
                          Block NVARCHAR(200) NULL,
                          AssemblyConstituency NVARCHAR(200) NULL,
                          ParliamentConstituency NVARCHAR(200) NULL,
                          Village NVARCHAR(200) NULL,
                          Ward NVARCHAR(200) NULL,
                          IsActive BIT NOT NULL CONSTRAINT DF_ElectionLocationMaster_IsActive DEFAULT (1),
                          CreatedDate DATETIME NOT NULL CONSTRAINT DF_ElectionLocationMaster_CreatedDate DEFAULT (GETDATE())
                      );

                      CREATE INDEX IX_ElectionLocationMaster_Cascade
                      ON dbo.ElectionLocationMaster(State, District, Block, AssemblyConstituency, ParliamentConstituency, Village);
                  END

                  IF NOT EXISTS (SELECT 1 FROM dbo.ElectionLocationMaster)
                  BEGIN
                      INSERT INTO dbo.ElectionLocationMaster
                      (State, District, Block, AssemblyConstituency, ParliamentConstituency, Village, Ward)
                      VALUES
                      ('Uttar Pradesh', 'Meerut', 'Sardhana', 'Sardhana', 'Muzaffarnagar', 'Sardhana', 'Ward 1'),
                      ('Uttar Pradesh', 'Meerut', 'Sardhana', 'Sardhana', 'Muzaffarnagar', 'Daurala', 'Ward 2'),
                      ('Uttar Pradesh', 'Meerut', 'Sardhana', 'Sardhana', 'Muzaffarnagar', 'Lawar', 'Ward 3'),
                      ('Uttar Pradesh', 'Meerut', 'Meerut', 'Meerut Cantt', 'Meerut', 'Meerut', 'Ward 10'),
                      ('Uttar Pradesh', 'Muzaffarnagar', 'Khatauli', 'Khatauli', 'Muzaffarnagar', 'Khatauli', 'Ward 5');
                  END

                  IF NOT EXISTS
                  (
                      SELECT 1
                      FROM dbo.MenuMaster
                      WHERE AreaName = 'Admin'
                        AND ControllerName = 'Voter'
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
                          NULL, 'Voter Management', 'Add, update and map voters by booth, EPIC, part and serial number.',
                          'Admin', 'Voter', 'Index', NULL, 'Admin', 'fas fa-id-card',
                          NULL, 510, 1, 0, 1, 0, 1, 0, 'Voter Management',
                          'Election voter data management and duplicate prevention.', NULL, GETDATE(), 1, 0, 0, 1
                      );
                  END

                  IF NOT EXISTS
                  (
                      SELECT 1
                      FROM dbo.MenuMaster
                      WHERE AreaName = 'Admin'
                        AND ControllerName = 'Voter'
                        AND ActionName = 'Rolls'
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
                          NULL, 'Official Voter Rolls', 'Upload, search and download booth-wise official electoral roll PDFs.',
                          'Admin', 'Voter', 'Rolls', NULL, 'Admin', 'fas fa-file-pdf',
                          NULL, 133, 1, 0, 1, 0, 1, 0, 'Official Voter Rolls',
                          'Official booth-wise electoral roll PDF register for Sardhana.', NULL, GETDATE(), 1, 0, 0, 1
                      );
                  END

                  DECLARE @ElectionWarRoomMenuId INT;

                  SELECT @ElectionWarRoomMenuId = MenuId
                  FROM dbo.MenuMaster
                  WHERE AreaName = 'Admin'
                    AND ControllerName = 'ElectionWarRoom'
                    AND ActionName = 'Index';

                  IF @ElectionWarRoomMenuId IS NOT NULL
                  BEGIN
                      UPDATE dbo.MenuMaster
                      SET ParentMenuId = @ElectionWarRoomMenuId,
                          DisplayOrder = 132,
                          ShowOnHome = 0,
                          ShowInAdminSidebar = 1,
                          MenuLevel = 1,
                          ModifiedDate = GETDATE()
                      WHERE AreaName = 'Admin'
                        AND ControllerName = 'Voter'
                        AND ActionName = 'Index';

                      UPDATE dbo.MenuMaster
                      SET ParentMenuId = @ElectionWarRoomMenuId,
                          DisplayOrder = 133,
                          ShowOnHome = 0,
                          ShowInAdminSidebar = 1,
                          MenuLevel = 1,
                          ModifiedDate = GETDATE()
                      WHERE AreaName = 'Admin'
                        AND ControllerName = 'Voter'
                        AND ActionName = 'Rolls';
                  END");

            db.Database.ExecuteSqlCommand(
                @"IF OBJECT_ID('dbo.RoleMenuPermission', 'U') IS NOT NULL
                  BEGIN
                      INSERT INTO dbo.RoleMenuPermission
                      (RoleId, MenuId, CanView, CanCreate, CanEdit, CanDelete, CreatedBy, CreatedDate)
                      SELECT r.RoleId, m.MenuId, 1, 1, 1, 1, NULL, GETDATE()
                      FROM dbo.ApplicationRole r
                      CROSS JOIN dbo.MenuMaster m
                      WHERE r.IsActive = 1
                        AND m.AreaName = 'Admin'
                        AND m.ControllerName = 'Voter'
                        AND m.ActionName IN ('Index', 'Rolls')
                        AND NOT EXISTS
                        (
                            SELECT 1
                            FROM dbo.RoleMenuPermission p
                            WHERE p.RoleId = r.RoleId
                              AND p.MenuId = m.MenuId
                        );
                  END");
        }

        private void Normalize(VoterVM model)
        {
            model.EpicNumber = Clean(model.EpicNumber).ToUpperInvariant();
            model.VoterName = Clean(model.VoterName);
            model.FatherOrHusbandName = Clean(model.FatherOrHusbandName);
            model.MobileNumber = Clean(model.MobileNumber);
            model.AadhaarNumber = Clean(model.AadhaarNumber);
            model.BoothNumber = Clean(model.BoothNumber);
            model.SerialNumber = Clean(model.SerialNumber);
            model.State = Clean(model.State);
            model.District = Clean(model.District);
            model.AssemblyConstituency = Clean(model.AssemblyConstituency);
            model.ParliamentConstituency = Clean(model.ParliamentConstituency);
            model.Block = Clean(model.Block);
            model.Village = Clean(model.Village);
            model.Ward = Clean(model.Ward);
        }

        private void NormalizeRoll(VoterRollPdfVM model)
        {
            model.State = Clean(model.State) ?? "Uttar Pradesh";
            model.District = Clean(model.District) ?? "Meerut";
            model.AssemblyConstituency = Clean(model.AssemblyConstituency) ?? "44 - Sardhana";
            model.ParliamentConstituency = Clean(model.ParliamentConstituency);
            model.PartNumber = Clean(model.PartNumber);
            model.BoothName = Clean(model.BoothName);
            model.Village = Clean(model.Village);
            model.Ward = Clean(model.Ward);
            model.PollingStation = Clean(model.PollingStation);
            model.SourceUrl = Clean(model.SourceUrl);
            model.PdfFilePath = Clean(model.PdfFilePath);
            model.RevisionType = Clean(model.RevisionType) ?? "Final Electoral Roll";
            model.Notes = Clean(model.Notes);

            if (model.RollYear <= 0)
            {
                model.RollYear = DateTime.Now.Year;
            }

            if (model.DownloadDate == DateTime.MinValue)
            {
                model.DownloadDate = DateTime.Now;
            }
        }

        private bool IsDuplicateRollPdf(VoterRollPdfVM model)
        {
            return db.Database.SqlQuery<int>(
                @"SELECT COUNT(1)
                  FROM dbo.VoterRollPdf
                  WHERE IsActive = 1
                    AND VoterRollPdfId <> @VoterRollPdfId
                    AND State = @State
                    AND District = @District
                    AND AssemblyConstituency = @AssemblyConstituency
                    AND PartNumber = @PartNumber
                    AND RollYear = @RollYear",
                new SqlParameter("@VoterRollPdfId", model.VoterRollPdfId),
                new SqlParameter("@State", model.State),
                new SqlParameter("@District", model.District),
                new SqlParameter("@AssemblyConstituency", model.AssemblyConstituency),
                new SqlParameter("@PartNumber", model.PartNumber),
                new SqlParameter("@RollYear", model.RollYear))
                .FirstOrDefault() > 0;
        }

        private List<SqlParameter> RollParameters(VoterRollPdfVM model, int? userId, bool includeId)
        {
            var parameters = new List<SqlParameter>();

            if (includeId)
            {
                parameters.Add(new SqlParameter("@VoterRollPdfId", model.VoterRollPdfId));
            }

            parameters.AddRange(new[]
            {
                P("@State", model.State),
                P("@District", model.District),
                P("@AssemblyConstituency", model.AssemblyConstituency),
                P("@ParliamentConstituency", model.ParliamentConstituency),
                P("@PartNumber", model.PartNumber),
                P("@BoothName", model.BoothName),
                P("@Village", model.Village),
                P("@Ward", model.Ward),
                P("@PollingStation", model.PollingStation),
                P("@SourceUrl", model.SourceUrl),
                P("@PdfFilePath", model.PdfFilePath),
                P("@RollYear", model.RollYear),
                P("@RevisionType", model.RevisionType),
                P("@PublishedDate", model.PublishedDate),
                P("@DownloadDate", model.DownloadDate),
                P("@Notes", model.Notes),
                P(includeId ? "@UpdatedBy" : "@CreatedBy", userId)
            });

            return parameters;
        }

        private static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private List<SqlParameter> BuildParameters(VoterVM model, int? userId, bool includeId)
        {
            var parameters = new List<SqlParameter>();

            if (includeId)
            {
                parameters.Add(new SqlParameter("@VoterId", model.VoterId));
            }

            parameters.AddRange(new[]
            {
                P("@EpicNumber", model.EpicNumber),
                P("@VoterName", model.VoterName),
                P("@FatherOrHusbandName", model.FatherOrHusbandName),
                P("@Gender", model.Gender),
                P("@Age", model.Age),
                P("@MobileNumber", model.MobileNumber),
                P("@AadhaarNumber", model.AadhaarNumber),
                P("@VoterPhotoPath", model.VoterPhotoPath),
                P("@AadhaarPhotoPath", model.AadhaarPhotoPath),
                P("@Caste", model.Caste),
                P("@Religion", model.Religion),
                P("@Category", model.Category),
                P("@State", model.State),
                P("@District", model.District),
                P("@AssemblyConstituency", model.AssemblyConstituency),
                P("@ParliamentConstituency", model.ParliamentConstituency),
                P("@Block", model.Block),
                P("@Village", model.Village),
                P("@Ward", model.Ward),
                P("@PartNumber", model.PartNumber),
                P("@SectionNumber", model.SectionNumber),
                P("@SerialNumber", model.SerialNumber),
                P("@HouseNumber", model.HouseNumber),
                P("@BoothNumber", model.BoothNumber),
                P("@BoothName", model.BoothName),
                P("@PollingStation", model.PollingStation),
                P("@Address", model.Address),
                P("@VoterType", model.VoterType),
                P("@PoliticalStatus", model.PoliticalStatus),
                P("@SupportLevel", model.SupportLevel),
                P("@InfluencerName", model.InfluencerName),
                P("@PannaPramukhName", model.PannaPramukhName),
                P("@IsPriorityVoter", model.IsPriorityVoter),
                P("@IsFirstTimeVoter", model.IsFirstTimeVoter),
                P("@Remarks", model.Remarks),
                P(includeId ? "@UpdatedBy" : "@CreatedBy", userId)
            });

            return parameters;
        }

        private SqlParameter P(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        private const string InsertSql =
            @"INSERT INTO dbo.ElectionVoter
              (EpicNumber,VoterName,FatherOrHusbandName,Gender,Age,MobileNumber,AadhaarNumber,VoterPhotoPath,AadhaarPhotoPath,Caste,Religion,Category,State,District,AssemblyConstituency,ParliamentConstituency,Block,Village,Ward,PartNumber,SectionNumber,SerialNumber,HouseNumber,BoothNumber,BoothName,PollingStation,Address,VoterType,PoliticalStatus,SupportLevel,InfluencerName,PannaPramukhName,IsPriorityVoter,IsFirstTimeVoter,Remarks,CreatedBy,CreatedDate)
              VALUES
              (@EpicNumber,@VoterName,@FatherOrHusbandName,@Gender,@Age,@MobileNumber,@AadhaarNumber,@VoterPhotoPath,@AadhaarPhotoPath,@Caste,@Religion,@Category,@State,@District,@AssemblyConstituency,@ParliamentConstituency,@Block,@Village,@Ward,@PartNumber,@SectionNumber,@SerialNumber,@HouseNumber,@BoothNumber,@BoothName,@PollingStation,@Address,@VoterType,@PoliticalStatus,@SupportLevel,@InfluencerName,@PannaPramukhName,@IsPriorityVoter,@IsFirstTimeVoter,@Remarks,@CreatedBy,GETDATE())";

        private const string UpdateSql =
            @"UPDATE dbo.ElectionVoter
              SET EpicNumber=@EpicNumber,VoterName=@VoterName,FatherOrHusbandName=@FatherOrHusbandName,Gender=@Gender,Age=@Age,MobileNumber=@MobileNumber,AadhaarNumber=@AadhaarNumber,VoterPhotoPath=@VoterPhotoPath,AadhaarPhotoPath=@AadhaarPhotoPath,Caste=@Caste,Religion=@Religion,Category=@Category,State=@State,District=@District,AssemblyConstituency=@AssemblyConstituency,ParliamentConstituency=@ParliamentConstituency,Block=@Block,Village=@Village,Ward=@Ward,PartNumber=@PartNumber,SectionNumber=@SectionNumber,SerialNumber=@SerialNumber,HouseNumber=@HouseNumber,BoothNumber=@BoothNumber,BoothName=@BoothName,PollingStation=@PollingStation,Address=@Address,VoterType=@VoterType,PoliticalStatus=@PoliticalStatus,SupportLevel=@SupportLevel,InfluencerName=@InfluencerName,PannaPramukhName=@PannaPramukhName,IsPriorityVoter=@IsPriorityVoter,IsFirstTimeVoter=@IsFirstTimeVoter,Remarks=@Remarks,UpdatedBy=@UpdatedBy,UpdatedDate=GETDATE()
              WHERE VoterId=@VoterId AND IsDeleted=0";
    }
}
