using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using PoliticalLeaderPortal.Models;
using PoliticalLeaderPortal.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;

namespace PoliticalLeaderPortal.Areas.Admin.Services
{
    public class HomeMemberService
    {
        private const int MaxPhotoBytes = 5 * 1024 * 1024;

        private readonly PoliticalLeaderPortalDbEntities1 _db;

        public HomeMemberService()
        {
            _db = new PoliticalLeaderPortalDbEntities1();
        }

        public List<HomeMemberVM> GetAll()
        {
            EnsureTable();

            return _db.Database.SqlQuery<HomeMemberVM>(
                @"SELECT HomeMemberId, MemberName, Designation, Tenure, PhotoPath,
                         DisplayOrder, IsActive, ShowOnHome, CreatedDate, ModifiedDate
                  FROM dbo.HomeMember
                  ORDER BY DisplayOrder, HomeMemberId")
                .ToList();
        }

        public HomeMemberVM GetById(int id)
        {
            EnsureTable();

            return _db.Database.SqlQuery<HomeMemberVM>(
                @"SELECT HomeMemberId, MemberName, Designation, Tenure, PhotoPath,
                         DisplayOrder, IsActive, ShowOnHome, CreatedDate, ModifiedDate
                  FROM dbo.HomeMember
                  WHERE HomeMemberId = @HomeMemberId",
                new SqlParameter("@HomeMemberId", id))
                .FirstOrDefault();
        }

        public List<HomeMemberDisplayVM> GetHomeMembers(int count)
        {
            EnsureTable();

            return _db.Database.SqlQuery<HomeMemberDisplayVM>(
                @"SELECT TOP (@Count)
                         HomeMemberId, MemberName, Designation, Tenure, PhotoPath, DisplayOrder
                  FROM dbo.HomeMember
                  WHERE IsActive = 1 AND ShowOnHome = 1
                  ORDER BY DisplayOrder, HomeMemberId",
                new SqlParameter("@Count", count))
                .ToList();
        }

        public void Create(HomeMemberVM model, HttpServerUtilityBase server)
        {
            EnsureTable();

            string photoPath = SavePhoto(model.PhotoFile, server, null);

            _db.Database.ExecuteSqlCommand(
                @"INSERT INTO dbo.HomeMember
                  (MemberName, Designation, Tenure, PhotoPath, DisplayOrder, IsActive, ShowOnHome, CreatedDate)
                  VALUES
                  (@MemberName, @Designation, @Tenure, @PhotoPath, @DisplayOrder, @IsActive, @ShowOnHome, GETDATE())",
                ToSql("@MemberName", model.MemberName),
                ToSql("@Designation", model.Designation),
                ToSql("@Tenure", model.Tenure),
                ToSql("@PhotoPath", photoPath),
                new SqlParameter("@DisplayOrder", model.DisplayOrder),
                new SqlParameter("@IsActive", model.IsActive),
                new SqlParameter("@ShowOnHome", model.ShowOnHome));
        }

        public void Update(HomeMemberVM model, HttpServerUtilityBase server)
        {
            EnsureTable();

            var existing = GetById(model.HomeMemberId);

            if (existing == null)
            {
                return;
            }

            string photoPath = SavePhoto(model.PhotoFile, server, existing.PhotoPath);

            _db.Database.ExecuteSqlCommand(
                @"UPDATE dbo.HomeMember
                  SET MemberName = @MemberName,
                      Designation = @Designation,
                      Tenure = @Tenure,
                      PhotoPath = @PhotoPath,
                      DisplayOrder = @DisplayOrder,
                      IsActive = @IsActive,
                      ShowOnHome = @ShowOnHome,
                      ModifiedDate = GETDATE()
                  WHERE HomeMemberId = @HomeMemberId",
                ToSql("@MemberName", model.MemberName),
                ToSql("@Designation", model.Designation),
                ToSql("@Tenure", model.Tenure),
                ToSql("@PhotoPath", photoPath),
                new SqlParameter("@DisplayOrder", model.DisplayOrder),
                new SqlParameter("@IsActive", model.IsActive),
                new SqlParameter("@ShowOnHome", model.ShowOnHome),
                new SqlParameter("@HomeMemberId", model.HomeMemberId));
        }

        public void Delete(int id)
        {
            EnsureTable();

            _db.Database.ExecuteSqlCommand(
                "DELETE FROM dbo.HomeMember WHERE HomeMemberId = @HomeMemberId",
                new SqlParameter("@HomeMemberId", id));
        }

        private void EnsureTable()
        {
            _db.Database.ExecuteSqlCommand(
                @"IF OBJECT_ID('dbo.HomeMember', 'U') IS NULL
                  BEGIN
                      CREATE TABLE dbo.HomeMember
                      (
                          HomeMemberId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                          MemberName NVARCHAR(160) NOT NULL,
                          Designation NVARCHAR(160) NULL,
                          Tenure NVARCHAR(120) NULL,
                          PhotoPath NVARCHAR(500) NULL,
                          DisplayOrder INT NOT NULL CONSTRAINT DF_HomeMember_DisplayOrder DEFAULT(0),
                          IsActive BIT NOT NULL CONSTRAINT DF_HomeMember_IsActive DEFAULT(1),
                          ShowOnHome BIT NOT NULL CONSTRAINT DF_HomeMember_ShowOnHome DEFAULT(1),
                          CreatedDate DATETIME NOT NULL CONSTRAINT DF_HomeMember_CreatedDate DEFAULT(GETDATE()),
                          ModifiedDate DATETIME NULL
                      );
                  END");
        }

        private static SqlParameter ToSql(string name, string value)
        {
            return new SqlParameter(name, (object)value ?? DBNull.Value);
        }

        private static string SavePhoto(
            HttpPostedFileBase file,
            HttpServerUtilityBase server,
            string existingPath)
        {
            if (file == null || file.ContentLength <= 0)
            {
                return existingPath;
            }

            if (file.ContentLength > MaxPhotoBytes)
            {
                throw new InvalidOperationException("Member photo must be 5 MB or smaller.");
            }

            if (file.ContentType == null ||
                !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Only image files can be uploaded for members.");
            }

            string extension = Path.GetExtension(file.FileName);
            string fileName = Guid.NewGuid().ToString("N") + extension;
            string folder = server.MapPath("~/Uploads/HomeMembers/");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string fullPath = Path.Combine(folder, fileName);
            file.SaveAs(fullPath);

            return "/Uploads/HomeMembers/" + fileName;
        }
    }
}
