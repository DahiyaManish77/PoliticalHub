using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using PoliticalLeaderPortal.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace PoliticalLeaderPortal.Areas.Admin.Services
{
    public class TodayScheduleService
    {
        private readonly PoliticalLeaderPortalDbEntities1 db;

        public TodayScheduleService()
        {
            db = new PoliticalLeaderPortalDbEntities1();
        }

        public List<TodayScheduleVM> GetAll(string keyword = null, DateTime? date = null)
        {
            EnsureTable();

            string search = "%" + (keyword ?? String.Empty).Trim() + "%";
            bool hasKeyword = !String.IsNullOrWhiteSpace(keyword);
            DateTime scheduleDate = date ?? DateTime.Today;

            return db.Database.SqlQuery<TodayScheduleVM>(
                @"SELECT TOP 500 *
                  FROM dbo.TodaySchedule
                  WHERE IsDeleted = 0
                    AND (@Date IS NULL OR ScheduleDate = @Date)
                    AND
                    (
                        @HasKeyword = 0
                        OR Title LIKE @Keyword
                        OR Place LIKE @Keyword
                        OR Description LIKE @Keyword
                        OR ContactPerson LIKE @Keyword
                        OR ContactMobile LIKE @Keyword
                        OR OrganizerName LIKE @Keyword
                        OR Category LIKE @Keyword
                        OR Status LIKE @Keyword
                    )
                  ORDER BY ScheduleDate DESC, ScheduleTime, TodayScheduleId DESC",
                new SqlParameter("@Date", scheduleDate),
                new SqlParameter("@HasKeyword", hasKeyword),
                new SqlParameter("@Keyword", search))
                .ToList();
        }

        public TodayScheduleVM GetById(int id)
        {
            EnsureTable();

            return db.Database.SqlQuery<TodayScheduleVM>(
                "SELECT TOP 1 * FROM dbo.TodaySchedule WHERE TodayScheduleId = @Id AND IsDeleted = 0",
                new SqlParameter("@Id", id))
                .FirstOrDefault();
        }

        public bool Save(TodayScheduleVM model, int? userId)
        {
            EnsureTable();
            Normalize(model);

            db.Database.ExecuteSqlCommand(
                @"INSERT INTO dbo.TodaySchedule
                  (ScheduleDate,ScheduleTime,Place,Title,Description,ContactPerson,ContactMobile,OrganizerName,Category,Priority,Status,MapLink,Notes,IsActive,IsDeleted,CreatedBy,CreatedDate)
                  VALUES
                  (@ScheduleDate,@ScheduleTime,@Place,@Title,@Description,@ContactPerson,@ContactMobile,@OrganizerName,@Category,@Priority,@Status,@MapLink,@Notes,@IsActive,0,@CreatedBy,GETDATE())",
                BuildParameters(model, userId, false).ToArray());

            return true;
        }

        public bool Update(TodayScheduleVM model, int? userId)
        {
            EnsureTable();
            Normalize(model);

            return db.Database.ExecuteSqlCommand(
                @"UPDATE dbo.TodaySchedule
                  SET ScheduleDate=@ScheduleDate,ScheduleTime=@ScheduleTime,Place=@Place,Title=@Title,Description=@Description,
                      ContactPerson=@ContactPerson,ContactMobile=@ContactMobile,OrganizerName=@OrganizerName,Category=@Category,
                      Priority=@Priority,Status=@Status,MapLink=@MapLink,Notes=@Notes,IsActive=@IsActive,
                      UpdatedBy=@UpdatedBy,UpdatedDate=GETDATE()
                  WHERE TodayScheduleId=@TodayScheduleId AND IsDeleted=0",
                BuildParameters(model, userId, true).ToArray()) > 0;
        }

        public bool Delete(int id, int? userId)
        {
            EnsureTable();

            return db.Database.ExecuteSqlCommand(
                @"UPDATE dbo.TodaySchedule
                  SET IsDeleted = 1, UpdatedBy = @UpdatedBy, UpdatedDate = GETDATE()
                  WHERE TodayScheduleId = @Id AND IsDeleted = 0",
                new SqlParameter("@Id", id),
                new SqlParameter("@UpdatedBy", (object)userId ?? DBNull.Value)) > 0;
        }

        public void EnsureTable()
        {
            db.Database.ExecuteSqlCommand(
                @"IF OBJECT_ID('dbo.TodaySchedule', 'U') IS NULL
                  BEGIN
                      CREATE TABLE dbo.TodaySchedule
                      (
                          TodayScheduleId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TodaySchedule PRIMARY KEY,
                          ScheduleDate DATE NOT NULL,
                          ScheduleTime NVARCHAR(20) NOT NULL,
                          Place NVARCHAR(250) NOT NULL,
                          Title NVARCHAR(250) NOT NULL,
                          Description NVARCHAR(500) NULL,
                          ContactPerson NVARCHAR(150) NULL,
                          ContactMobile NVARCHAR(20) NULL,
                          OrganizerName NVARCHAR(100) NULL,
                          Category NVARCHAR(100) NULL,
                          Priority NVARCHAR(50) NULL,
                          Status NVARCHAR(50) NULL,
                          MapLink NVARCHAR(500) NULL,
                          Notes NVARCHAR(MAX) NULL,
                          IsActive BIT NOT NULL CONSTRAINT DF_TodaySchedule_IsActive DEFAULT(1),
                          IsDeleted BIT NOT NULL CONSTRAINT DF_TodaySchedule_IsDeleted DEFAULT(0),
                          CreatedBy INT NULL,
                          CreatedDate DATETIME NOT NULL CONSTRAINT DF_TodaySchedule_CreatedDate DEFAULT(GETDATE()),
                          UpdatedBy INT NULL,
                          UpdatedDate DATETIME NULL
                      );

                      CREATE INDEX IX_TodaySchedule_Date
                      ON dbo.TodaySchedule(ScheduleDate, ScheduleTime, IsDeleted);
                  END");
        }

        private void Normalize(TodayScheduleVM model)
        {
            if (model.ScheduleDate == DateTime.MinValue)
            {
                model.ScheduleDate = DateTime.Today;
            }

            model.ScheduleTime = Clean(model.ScheduleTime);
            model.Place = Clean(model.Place);
            model.Title = Clean(model.Title);
            model.Description = Clean(model.Description);
            model.ContactPerson = Clean(model.ContactPerson);
            model.ContactMobile = Clean(model.ContactMobile);
            model.OrganizerName = Clean(model.OrganizerName);
            model.Category = Clean(model.Category) ?? "Public Program";
            model.Priority = Clean(model.Priority) ?? "Medium";
            model.Status = Clean(model.Status) ?? "Scheduled";
            model.MapLink = Clean(model.MapLink);
            model.Notes = Clean(model.Notes);
        }

        private List<SqlParameter> BuildParameters(TodayScheduleVM model, int? userId, bool includeId)
        {
            var parameters = new List<SqlParameter>();

            if (includeId)
            {
                parameters.Add(new SqlParameter("@TodayScheduleId", model.TodayScheduleId));
            }

            parameters.AddRange(new[]
            {
                P("@ScheduleDate", model.ScheduleDate.Date),
                P("@ScheduleTime", model.ScheduleTime),
                P("@Place", model.Place),
                P("@Title", model.Title),
                P("@Description", model.Description),
                P("@ContactPerson", model.ContactPerson),
                P("@ContactMobile", model.ContactMobile),
                P("@OrganizerName", model.OrganizerName),
                P("@Category", model.Category),
                P("@Priority", model.Priority),
                P("@Status", model.Status),
                P("@MapLink", model.MapLink),
                P("@Notes", model.Notes),
                P("@IsActive", model.IsActive),
                P(includeId ? "@UpdatedBy" : "@CreatedBy", userId)
            });

            return parameters;
        }

        private static string Clean(string value)
        {
            return String.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static SqlParameter P(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }
    }
}
