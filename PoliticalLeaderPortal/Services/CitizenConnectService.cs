using PoliticalLeaderPortal.Models;
using PoliticalLeaderPortal.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace PoliticalLeaderPortal.Services
{
    public class CitizenConnectService
    {
        private readonly PoliticalLeaderPortalDbEntities1 db;

        public CitizenConnectService()
        {
            db = new PoliticalLeaderPortalDbEntities1();
        }

        public void EnsureTable()
        {
            db.Database.ExecuteSqlCommand(
                @"IF OBJECT_ID('dbo.CitizenConnectRequest', 'U') IS NULL
                  BEGIN
                      CREATE TABLE dbo.CitizenConnectRequest
                      (
                          CitizenConnectId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CitizenConnectRequest PRIMARY KEY,
                          RequestType NVARCHAR(40) NOT NULL,
                          FullName NVARCHAR(160) NOT NULL,
                          MobileNumber NVARCHAR(20) NULL,
                          Email NVARCHAR(160) NULL,
                          District NVARCHAR(160) NULL,
                          Assembly NVARCHAR(160) NULL,
                          Subject NVARCHAR(250) NOT NULL,
                          Message NVARCHAR(MAX) NOT NULL,
                          Status NVARCHAR(40) NOT NULL CONSTRAINT DF_CitizenConnectRequest_Status DEFAULT ('New'),
                          AdminRemarks NVARCHAR(MAX) NULL,
                          IsDeleted BIT NOT NULL CONSTRAINT DF_CitizenConnectRequest_IsDeleted DEFAULT (0),
                          CreatedDate DATETIME NOT NULL CONSTRAINT DF_CitizenConnectRequest_CreatedDate DEFAULT (GETDATE()),
                          UpdatedDate DATETIME NULL
                      );

                      CREATE INDEX IX_CitizenConnectRequest_Search
                      ON dbo.CitizenConnectRequest(RequestType, Status, CreatedDate);
                  END");
        }

        public List<CitizenConnectVM> GetAll(string requestType = null, string status = null, string keyword = null)
        {
            EnsureTable();

            var sql = @"SELECT CitizenConnectId, RequestType, FullName, MobileNumber, Email, District, Assembly,
                               Subject, Message, Status, AdminRemarks, CreatedDate
                        FROM dbo.CitizenConnectRequest
                        WHERE IsDeleted = 0
                          AND (@RequestType IS NULL OR RequestType = @RequestType)
                          AND (@Status IS NULL OR Status = @Status)
                          AND (@Keyword IS NULL OR FullName LIKE @LikeKeyword OR MobileNumber LIKE @LikeKeyword
                               OR Email LIKE @LikeKeyword OR Subject LIKE @LikeKeyword OR District LIKE @LikeKeyword)
                        ORDER BY CreatedDate DESC";

            string cleanKeyword = String.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim();

            return db.Database.SqlQuery<CitizenConnectVM>(
                sql,
                new SqlParameter("@RequestType", (object)NullIfBlank(requestType) ?? DBNull.Value),
                new SqlParameter("@Status", (object)NullIfBlank(status) ?? DBNull.Value),
                new SqlParameter("@Keyword", (object)cleanKeyword ?? DBNull.Value),
                new SqlParameter("@LikeKeyword", cleanKeyword == null ? (object)DBNull.Value : "%" + cleanKeyword + "%"))
                .ToList();
        }

        public CitizenConnectVM GetById(int id)
        {
            EnsureTable();

            return db.Database.SqlQuery<CitizenConnectVM>(
                @"SELECT CitizenConnectId, RequestType, FullName, MobileNumber, Email, District, Assembly,
                         Subject, Message, Status, AdminRemarks, CreatedDate
                  FROM dbo.CitizenConnectRequest
                  WHERE CitizenConnectId = @Id AND IsDeleted = 0",
                new SqlParameter("@Id", id))
                .FirstOrDefault();
        }

        public bool Save(CitizenConnectVM model)
        {
            EnsureTable();
            Normalize(model);

            int duplicateCount = db.Database.SqlQuery<int>(
                @"SELECT COUNT(1)
                  FROM dbo.CitizenConnectRequest
                  WHERE IsDeleted = 0
                    AND RequestType = @RequestType
                    AND FullName = @FullName
                    AND ISNULL(MobileNumber, '') = ISNULL(@MobileNumber, '')
                    AND Subject = @Subject
                    AND CreatedDate >= DATEADD(DAY, -1, GETDATE())",
                P("@RequestType", model.RequestType),
                P("@FullName", model.FullName),
                P("@MobileNumber", model.MobileNumber),
                P("@Subject", model.Subject))
                .FirstOrDefault();

            if (duplicateCount > 0)
            {
                return false;
            }

            db.Database.ExecuteSqlCommand(
                @"INSERT INTO dbo.CitizenConnectRequest
                  (RequestType, FullName, MobileNumber, Email, District, Assembly, Subject, Message, Status, CreatedDate)
                  VALUES
                  (@RequestType, @FullName, @MobileNumber, @Email, @District, @Assembly, @Subject, @Message, 'New', GETDATE())",
                P("@RequestType", model.RequestType),
                P("@FullName", model.FullName),
                P("@MobileNumber", model.MobileNumber),
                P("@Email", model.Email),
                P("@District", model.District),
                P("@Assembly", model.Assembly),
                P("@Subject", model.Subject),
                P("@Message", model.Message));

            return true;
        }

        public bool UpdateStatus(int id, string status, string remarks)
        {
            EnsureTable();

            return db.Database.ExecuteSqlCommand(
                @"UPDATE dbo.CitizenConnectRequest
                  SET Status = @Status,
                      AdminRemarks = @AdminRemarks,
                      UpdatedDate = GETDATE()
                  WHERE CitizenConnectId = @Id AND IsDeleted = 0",
                new SqlParameter("@Id", id),
                P("@Status", NullIfBlank(status) ?? "In Review"),
                P("@AdminRemarks", remarks)) > 0;
        }

        public bool Delete(int id)
        {
            EnsureTable();

            return db.Database.ExecuteSqlCommand(
                @"UPDATE dbo.CitizenConnectRequest
                  SET IsDeleted = 1, UpdatedDate = GETDATE()
                  WHERE CitizenConnectId = @Id",
                new SqlParameter("@Id", id)) > 0;
        }

        private void Normalize(CitizenConnectVM model)
        {
            model.RequestType = NullIfBlank(model.RequestType) ?? "Contact";
            model.FullName = NullIfBlank(model.FullName);
            model.MobileNumber = NullIfBlank(model.MobileNumber);
            model.Email = NullIfBlank(model.Email);
            model.District = NullIfBlank(model.District);
            model.Assembly = NullIfBlank(model.Assembly);
            model.Subject = NullIfBlank(model.Subject);
            model.Message = NullIfBlank(model.Message);
        }

        private static string NullIfBlank(string value)
        {
            return String.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static SqlParameter P(string name, string value)
        {
            return new SqlParameter(name, (object)value ?? DBNull.Value);
        }
    }
}
