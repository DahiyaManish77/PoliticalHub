using PoliticalLeaderPortal.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;

using PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom;

namespace PoliticalLeaderPortal.Areas.Admin.Services
{
    public class ElectionWarRoomService
    {
        private readonly PoliticalLeaderPortalDbEntities1 _db;
        private readonly string _connectionString;

        public ElectionWarRoomService()
        {
            _db = new PoliticalLeaderPortalDbEntities1();
            _connectionString = GetProviderConnectionString();
        }

        #region Power Event Tables

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

        private void EnsurePowerEventTables()
        {
            ExecuteSql(@"IF OBJECT_ID('dbo.EventPublicProfile', 'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.EventPublicProfile
                    (
                        EventId INT NOT NULL PRIMARY KEY,
                        SubTitle NVARCHAR(250) NULL,
                        EventScope NVARCHAR(40) NOT NULL DEFAULT('ElectionWarRoom'),
                        ShowOnHome BIT NOT NULL DEFAULT(0),
                        ShowInElectionWarRoom BIT NOT NULL DEFAULT(1),
                        IsConfidential BIT NOT NULL DEFAULT(0),
                        FinishDate DATETIME NULL,
                        FullAddress NVARCHAR(500) NULL,
                        GoogleMapLink NVARCHAR(500) NULL,
                        EventImagePath NVARCHAR(500) NULL,
                        ExpectedVehicles INT NOT NULL DEFAULT(0),
                        ActualVehicles INT NOT NULL DEFAULT(0),
                        ExpectedVolunteers INT NOT NULL DEFAULT(0),
                        ActualVolunteers INT NOT NULL DEFAULT(0),
                        ExpectedFoodPlates INT NOT NULL DEFAULT(0),
                        ActualFoodPlates INT NOT NULL DEFAULT(0),
                        ResponsiblePerson NVARCHAR(150) NULL,
                        ResponsibleMobile NVARCHAR(30) NULL,
                        TransportResponsible NVARCHAR(150) NULL,
                        FoodResponsible NVARCHAR(150) NULL,
                        MediaResponsible NVARCHAR(150) NULL,
                        VolunteerResponsible NVARCHAR(150) NULL,
                        CreatedDate DATETIME NOT NULL DEFAULT(GETDATE()),
                        UpdatedDate DATETIME NULL
                    );
                END");

            ExecuteSql(@"IF OBJECT_ID('dbo.EventTrackingItem', 'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.EventTrackingItem
                    (
                        EventTrackingItemId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        EventId INT NOT NULL,
                        Category NVARCHAR(80) NOT NULL,
                        ItemName NVARCHAR(180) NOT NULL,
                        Village NVARCHAR(120) NULL,
                        Ward NVARCHAR(80) NULL,
                        Booth NVARCHAR(80) NULL,
                        ResponsiblePerson NVARCHAR(150) NULL,
                        ResponsibleMobile NVARCHAR(30) NULL,
                        ExpectedQuantity INT NOT NULL DEFAULT(0),
                        ActualQuantity INT NOT NULL DEFAULT(0),
                        Unit NVARCHAR(40) NULL,
                        EstimatedCost DECIMAL(18,2) NOT NULL DEFAULT(0),
                        ActualCost DECIMAL(18,2) NOT NULL DEFAULT(0),
                        ProviderName NVARCHAR(150) NULL,
                        ProviderMobile NVARCHAR(30) NULL,
                        ReturnRequired BIT NOT NULL DEFAULT(0),
                        Returned BIT NOT NULL DEFAULT(0),
                        AppreciationPending BIT NOT NULL DEFAULT(0),
                        Status NVARCHAR(40) NULL,
                        Remarks NVARCHAR(MAX) NULL,
                        IsActive BIT NOT NULL DEFAULT(1),
                        CreatedBy INT NULL,
                        CreatedDate DATETIME NOT NULL DEFAULT(GETDATE()),
                        UpdatedBy INT NULL,
                        UpdatedDate DATETIME NULL
                    );
                END");
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

        private DataTable QuerySql(string sql, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            using (var adapter = new SqlDataAdapter(command))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                var table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        private void AttachEventProfile(EventVM vm)
        {
            if (vm == null || vm.EventId <= 0)
            {
                return;
            }

            EnsurePowerEventTables();

            DataTable table = QuerySql(
                "SELECT * FROM dbo.EventPublicProfile WHERE EventId = @EventId",
                new SqlParameter("@EventId", vm.EventId));

            if (table.Rows.Count == 0)
            {
                vm.EventScope = "ElectionWarRoom";
                vm.ShowInElectionWarRoom = true;
                return;
            }

            DataRow row = table.Rows[0];
            vm.SubTitle = Convert.ToString(row["SubTitle"]);
            vm.EventScope = Convert.ToString(row["EventScope"]);
            vm.ShowOnHome = Convert.ToBoolean(row["ShowOnHome"]);
            vm.ShowInElectionWarRoom = Convert.ToBoolean(row["ShowInElectionWarRoom"]);
            vm.IsConfidential = Convert.ToBoolean(row["IsConfidential"]);
            vm.FinishDate = row["FinishDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["FinishDate"]);
            vm.FullAddress = Convert.ToString(row["FullAddress"]);
            vm.GoogleMapLink = Convert.ToString(row["GoogleMapLink"]);
            vm.EventImagePath = Convert.ToString(row["EventImagePath"]);
            vm.ExpectedVehicles = Convert.ToInt32(row["ExpectedVehicles"]);
            vm.ActualVehicles = Convert.ToInt32(row["ActualVehicles"]);
            vm.ExpectedVolunteers = Convert.ToInt32(row["ExpectedVolunteers"]);
            vm.ActualVolunteers = Convert.ToInt32(row["ActualVolunteers"]);
            vm.ExpectedFoodPlates = Convert.ToInt32(row["ExpectedFoodPlates"]);
            vm.ActualFoodPlates = Convert.ToInt32(row["ActualFoodPlates"]);
            vm.ResponsiblePerson = Convert.ToString(row["ResponsiblePerson"]);
            vm.ResponsibleMobile = Convert.ToString(row["ResponsibleMobile"]);
            vm.TransportResponsible = Convert.ToString(row["TransportResponsible"]);
            vm.FoodResponsible = Convert.ToString(row["FoodResponsible"]);
            vm.MediaResponsible = Convert.ToString(row["MediaResponsible"]);
            vm.VolunteerResponsible = Convert.ToString(row["VolunteerResponsible"]);
        }

        private void SaveEventProfile(EventVM vm, HttpServerUtilityBase server)
        {
            EnsurePowerEventTables();

            string imagePath = SaveEventImage(vm.EventImageFile, server, vm.EventImagePath);

            ExecuteSql(@"MERGE dbo.EventPublicProfile AS Target
                USING (SELECT @EventId AS EventId) AS Source
                ON Target.EventId = Source.EventId
                WHEN MATCHED THEN UPDATE SET
                    SubTitle = @SubTitle,
                    EventScope = @EventScope,
                    ShowOnHome = @ShowOnHome,
                    ShowInElectionWarRoom = @ShowInElectionWarRoom,
                    IsConfidential = @IsConfidential,
                    FinishDate = @FinishDate,
                    FullAddress = @FullAddress,
                    GoogleMapLink = @GoogleMapLink,
                    EventImagePath = @EventImagePath,
                    ExpectedVehicles = @ExpectedVehicles,
                    ActualVehicles = @ActualVehicles,
                    ExpectedVolunteers = @ExpectedVolunteers,
                    ActualVolunteers = @ActualVolunteers,
                    ExpectedFoodPlates = @ExpectedFoodPlates,
                    ActualFoodPlates = @ActualFoodPlates,
                    ResponsiblePerson = @ResponsiblePerson,
                    ResponsibleMobile = @ResponsibleMobile,
                    TransportResponsible = @TransportResponsible,
                    FoodResponsible = @FoodResponsible,
                    MediaResponsible = @MediaResponsible,
                    VolunteerResponsible = @VolunteerResponsible,
                    UpdatedDate = GETDATE()
                WHEN NOT MATCHED THEN INSERT
                    (EventId, SubTitle, EventScope, ShowOnHome, ShowInElectionWarRoom, IsConfidential, FinishDate,
                     FullAddress, GoogleMapLink, EventImagePath, ExpectedVehicles, ActualVehicles, ExpectedVolunteers,
                     ActualVolunteers, ExpectedFoodPlates, ActualFoodPlates, ResponsiblePerson, ResponsibleMobile,
                     TransportResponsible, FoodResponsible, MediaResponsible, VolunteerResponsible)
                VALUES
                    (@EventId, @SubTitle, @EventScope, @ShowOnHome, @ShowInElectionWarRoom, @IsConfidential, @FinishDate,
                     @FullAddress, @GoogleMapLink, @EventImagePath, @ExpectedVehicles, @ActualVehicles, @ExpectedVolunteers,
                     @ActualVolunteers, @ExpectedFoodPlates, @ActualFoodPlates, @ResponsiblePerson, @ResponsibleMobile,
                     @TransportResponsible, @FoodResponsible, @MediaResponsible, @VolunteerResponsible);",
                new SqlParameter("@EventId", vm.EventId),
                new SqlParameter("@SubTitle", (object)vm.SubTitle ?? DBNull.Value),
                new SqlParameter("@EventScope", string.IsNullOrWhiteSpace(vm.EventScope) ? "ElectionWarRoom" : vm.EventScope),
                new SqlParameter("@ShowOnHome", vm.ShowOnHome),
                new SqlParameter("@ShowInElectionWarRoom", vm.ShowInElectionWarRoom),
                new SqlParameter("@IsConfidential", vm.IsConfidential),
                new SqlParameter("@FinishDate", (object)vm.FinishDate ?? DBNull.Value),
                new SqlParameter("@FullAddress", (object)vm.FullAddress ?? DBNull.Value),
                new SqlParameter("@GoogleMapLink", (object)vm.GoogleMapLink ?? DBNull.Value),
                new SqlParameter("@EventImagePath", (object)imagePath ?? DBNull.Value),
                new SqlParameter("@ExpectedVehicles", vm.ExpectedVehicles),
                new SqlParameter("@ActualVehicles", vm.ActualVehicles),
                new SqlParameter("@ExpectedVolunteers", vm.ExpectedVolunteers),
                new SqlParameter("@ActualVolunteers", vm.ActualVolunteers),
                new SqlParameter("@ExpectedFoodPlates", vm.ExpectedFoodPlates),
                new SqlParameter("@ActualFoodPlates", vm.ActualFoodPlates),
                new SqlParameter("@ResponsiblePerson", (object)vm.ResponsiblePerson ?? DBNull.Value),
                new SqlParameter("@ResponsibleMobile", (object)vm.ResponsibleMobile ?? DBNull.Value),
                new SqlParameter("@TransportResponsible", (object)vm.TransportResponsible ?? DBNull.Value),
                new SqlParameter("@FoodResponsible", (object)vm.FoodResponsible ?? DBNull.Value),
                new SqlParameter("@MediaResponsible", (object)vm.MediaResponsible ?? DBNull.Value),
                new SqlParameter("@VolunteerResponsible", (object)vm.VolunteerResponsible ?? DBNull.Value));

            vm.EventImagePath = imagePath;
        }

        private static string SaveEventImage(HttpPostedFileBase file, HttpServerUtilityBase server, string existingPath)
        {
            if (file == null || file.ContentLength <= 0)
            {
                return existingPath;
            }

            string extension = Path.GetExtension(file.FileName);
            string[] allowed = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

            if (string.IsNullOrWhiteSpace(extension) ||
                !allowed.Contains(extension.ToLowerInvariant()))
            {
                throw new InvalidOperationException("Only JPG, PNG, WEBP and GIF event images are allowed.");
            }

            string folder = server.MapPath("~/Uploads/ElectionWarRoom/Events");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string fileName = "event_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + extension.ToLowerInvariant();
            file.SaveAs(Path.Combine(folder, fileName));

            return "~/Uploads/ElectionWarRoom/Events/" + fileName;
        }

        #endregion

        #region Dashboard

        public DashboardVM GetDashboard()
        {
            DashboardVM vm = new DashboardVM();

            vm.TotalEvents = _db.EventMasters.Count(x => x.IsActive);

            vm.TodayEvents = _db.EventMasters.Count(x =>
                x.IsActive &&
                x.EventDate == DateTime.Today);

            vm.UpcomingEventsCount = _db.EventMasters.Count(x =>
                x.IsActive &&
                x.EventDate > DateTime.Today);

            vm.PendingTasks = _db.EventTasks.Count(x =>
                x.IsActive &&
                x.Status == "Pending");

            vm.CompletedTasks = _db.EventTasks.Count(x =>
                x.IsActive &&
                x.Status == "Completed");

            vm.TotalVehicles = _db.EventVehicles
                .Where(x => x.IsActive)
                .Sum(x => (int?)x.VehicleCount) ?? 0;

            vm.TotalAttendance = _db.EventAttendances.Count(x => x.IsActive);

            vm.TotalGuests = _db.EventGuests.Count(x => x.IsActive);

            vm.TotalTeams = _db.EventTeams.Count(x => x.IsActive);

            vm.TotalBooths = _db.ElectionBooths.Count(x => x.IsActive);

            vm.VisitedBooths = _db.ElectionBoothVisits
                .Select(x => x.ElectionBoothId)
                .Distinct()
                .Count();

            vm.TotalPolls = _db.EventPolls.Count(x => x.IsActive);

            vm.TotalSurveyResponses = _db.EventPollResponses.Count(x => x.IsActive);

            vm.OpenComplaints = _db.JanSamparks.Count(x =>
                x.IsActive &&
                x.Status != "Resolved");

            vm.ResolvedComplaints = _db.JanSamparks.Count(x =>
                x.IsActive &&
                x.Status == "Resolved");

            vm.TodayExpense =
                _db.EventExpenses
                .Where(x =>
                    x.IsActive &&
                    x.ExpenseDate.Year == DateTime.Today.Year &&
                    x.ExpenseDate.Month == DateTime.Today.Month &&
                    x.ExpenseDate.Day == DateTime.Today.Day)
                .Sum(x => (decimal?)x.Amount) ?? 0;

            vm.MonthExpense =
                _db.EventExpenses
                .Where(x =>
                    x.IsActive &&
                    x.ExpenseDate.Year == DateTime.Today.Year &&
                    x.ExpenseDate.Month == DateTime.Today.Month)
                .Sum(x => (decimal?)x.Amount) ?? 0;

            vm.TotalAlerts = _db.CampaignAlerts.Count(x => x.IsActive);

            vm.CriticalAlerts = _db.CampaignAlerts.Count(x =>
                x.IsActive &&
                x.Severity == "Critical");

            return vm;
        }

        #endregion

        // Event Management Methods
        #region Event

        public List<EventVM> GetEvents()
        {
            return _db.EventMasters
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.EventDate)
                .ThenByDescending(x => x.CreatedDate)
                .Select(x => new EventVM
                {
                    EventId = x.EventId,
                    EventCode = x.EventCode,
                    EventTitle = x.EventTitle,
                    EventType = x.EventType,
                    Description = x.Description,
                    EventDate = x.EventDate,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    State = x.State,
                    District = x.District,
                    Block = x.Block,
                    Village = x.Village,
                    Booth = x.Booth,
                    Venue = x.Venue,
                    Landmark = x.Landmark,
                    ExpectedCrowd = x.ExpectedCrowd,
                    ActualCrowd = x.ActualCrowd,
                    Budget = x.Budget,
                    ActualExpense = x.ActualExpense,
                    OrganizerName = x.OrganizerName,
                    OrganizerMobile = x.OrganizerMobile,
                    ChiefGuest = x.ChiefGuest,
                    ChiefGuestMobile = x.ChiefGuestMobile,
                    Priority = x.Priority,
                    Status = x.Status,
                    Remarks = x.Remarks
                })
                .ToList();
        }

        public List<EventVM> GetEvents(int? campaignId)
        {
            return FilterEventsByCampaign(GetEvents(), campaignId);
        }

        public EventVM GetEventById(int eventId)
        {
            EventMaster entity = _db.EventMasters
                .FirstOrDefault(x =>
                    x.EventId == eventId &&
                    x.IsActive);

            if (entity == null)
            {
                return null;
            }

            EventVM vm = new EventVM();

            vm.EventId = entity.EventId;
            vm.EventCode = entity.EventCode;
            vm.EventTitle = entity.EventTitle;
            vm.EventType = entity.EventType;
            vm.Description = entity.Description;
            vm.EventDate = entity.EventDate;
            vm.StartTime = entity.StartTime;
            vm.EndTime = entity.EndTime;
            vm.State = entity.State;
            vm.District = entity.District;
            vm.Block = entity.Block;
            vm.Village = entity.Village;
            vm.Booth = entity.Booth;
            vm.Venue = entity.Venue;
            vm.Landmark = entity.Landmark;
            vm.ExpectedCrowd = entity.ExpectedCrowd;
            vm.ActualCrowd = entity.ActualCrowd;
            vm.Budget = entity.Budget;
            vm.ActualExpense = entity.ActualExpense;
            vm.OrganizerName = entity.OrganizerName;
            vm.OrganizerMobile = entity.OrganizerMobile;
            vm.ChiefGuest = entity.ChiefGuest;
            vm.ChiefGuestMobile = entity.ChiefGuestMobile;
            vm.Priority = entity.Priority;
            vm.Status = entity.Status;
            vm.Remarks = entity.Remarks;

            AttachEventProfile(vm);
            AttachEventCampaign(vm);

            return vm;
        }

        public void SetEventCampaign(int eventId, int? campaignId, int userId)
        {
            EnsureEventCampaignContextTable();
            if (!campaignId.HasValue)
            {
                ExecuteSql("DELETE FROM dbo.EventCampaignContext WHERE EventId=@EventId",
                    new SqlParameter("@EventId", eventId));
                return;
            }

            ExecuteSql(
                @"MERGE dbo.EventCampaignContext AS target
                  USING (SELECT @EventId EventId) AS source ON target.EventId=source.EventId
                  WHEN MATCHED THEN UPDATE SET OperationalCampaignId=@CampaignId,UpdatedBy=@UserId,UpdatedDate=GETDATE()
                  WHEN NOT MATCHED THEN INSERT (EventId,OperationalCampaignId,CreatedBy,CreatedDate)
                  VALUES (@EventId,@CampaignId,@UserId,GETDATE());",
                new SqlParameter("@EventId", eventId),
                new SqlParameter("@CampaignId", campaignId.Value),
                new SqlParameter("@UserId", userId));
        }

        public List<EventVM> SearchEvents(string keyword, int? campaignId)
        {
            return FilterEventsByCampaign(SearchEvents(keyword), campaignId);
        }

        public List<EventVM> GetEventsByStatus(string status, int? campaignId)
        {
            return FilterEventsByCampaign(GetEventsByStatus(status), campaignId);
        }

        private List<EventVM> FilterEventsByCampaign(List<EventVM> events, int? campaignId)
        {
            if (!campaignId.HasValue) return events;
            HashSet<int> allowed = new HashSet<int>(GetEventIdsByCampaign(campaignId.Value));
            return events.Where(x => allowed.Contains(x.EventId)).ToList();
        }

        public List<int> GetEventIdsByCampaign(int campaignId)
        {
            EnsureEventCampaignContextTable();
            return _db.Database.SqlQuery<int>(
                "SELECT EventId FROM dbo.EventCampaignContext WHERE OperationalCampaignId=@CampaignId",
                new SqlParameter("@CampaignId", campaignId))
                .ToList();
        }

        private void AttachEventCampaign(EventVM vm)
        {
            EnsureEventCampaignContextTable();
            DataTable table = QuerySql(
                @"SELECT context.OperationalCampaignId,campaign.CampaignName
                  FROM dbo.EventCampaignContext context
                  INNER JOIN dbo.ElectionCampaign campaign ON campaign.CampaignId=context.OperationalCampaignId
                  WHERE context.EventId=@EventId",
                new SqlParameter("@EventId", vm.EventId));
            if (table.Rows.Count > 0)
            {
                vm.CampaignId = Convert.ToInt32(table.Rows[0]["OperationalCampaignId"]);
                vm.CampaignName = Convert.ToString(table.Rows[0]["CampaignName"]);
            }
        }

        private void EnsureEventCampaignContextTable()
        {
            ExecuteSql(@"IF OBJECT_ID(N'dbo.EventCampaignContext',N'U') IS NULL
                         THROW 50001, 'Run App_Data/EventCampaignContextUpgrade.sql before using campaign-scoped events.', 1;");
        }

        public List<EventVM> GetTodayEvents()
        {
            DateTime today = DateTime.Today;

            return _db.EventMasters
                .Where(x =>
                    x.IsActive &&
                    x.EventDate.Year == today.Year &&
                    x.EventDate.Month == today.Month &&
                    x.EventDate.Day == today.Day)
                .OrderBy(x => x.StartTime)
                .Select(x => new EventVM
                {
                    EventId = x.EventId,
                    EventCode = x.EventCode,
                    EventTitle = x.EventTitle,
                    EventType = x.EventType,
                    Venue = x.Venue,
                    Village = x.Village,
                    District = x.District,
                    OrganizerName = x.OrganizerName,
                    EventDate = x.EventDate,
                    StartTime = x.StartTime,
                    Status = x.Status,
                    Priority = x.Priority
                })
                .ToList();
        }

        public List<EventVM> GetUpcomingEvents()
        {
            DateTime today = DateTime.Today;

            return _db.EventMasters
                .Where(x =>
                    x.IsActive &&
                    x.EventDate > today)
                .OrderBy(x => x.EventDate)
                .ThenBy(x => x.StartTime)
                .Select(x => new EventVM
                {
                    EventId = x.EventId,
                    EventCode = x.EventCode,
                    EventTitle = x.EventTitle,
                    EventType = x.EventType,
                    EventDate = x.EventDate,
                    Venue = x.Venue,
                    Village = x.Village,
                    District = x.District,
                    OrganizerName = x.OrganizerName,
                    Priority = x.Priority,
                    Status = x.Status
                })
                .ToList();
        }

        public List<EventVM> GetCompletedEvents()
        {
            return _db.EventMasters
                .Where(x =>
                    x.IsActive &&
                    x.IsCompleted)
                .OrderByDescending(x => x.EventDate)
                .Select(x => new EventVM
                {
                    EventId = x.EventId,
                    EventCode = x.EventCode,
                    EventTitle = x.EventTitle,
                    EventDate = x.EventDate,
                    Village = x.Village,
                    Venue = x.Venue,
                    ActualCrowd = x.ActualCrowd,
                    ActualExpense = x.ActualExpense,
                    Status = x.Status
                })
                .ToList();
        }

        public List<EventVM> GetCancelledEvents()
        {
            return _db.EventMasters
                .Where(x =>
                    x.IsActive &&
                    x.IsCancelled)
                .OrderByDescending(x => x.EventDate)
                .Select(x => new EventVM
                {
                    EventId = x.EventId,
                    EventCode = x.EventCode,
                    EventTitle = x.EventTitle,
                    EventDate = x.EventDate,
                    Village = x.Village,
                    Venue = x.Venue,
                    Status = x.Status,
                    Remarks = x.Remarks
                })
                .ToList();
        }

        /// <summary>
        /// Creates a new Event.
        /// Used by Create Event screen.
        /// </summary>
        public bool SaveEvent(EventVM vm, int userId)
        {
            return SaveEvent(vm, userId, null);
        }

        public bool SaveEvent(EventVM vm, int userId, HttpServerUtilityBase server)
        {
            if (vm == null)
            {
                return false;
            }

            if (IsDuplicateEvent(
                vm.EventTitle,
                vm.EventDate,
                vm.Venue,
                0))
            {
                return false;
            }

            EventMaster entity = new EventMaster();

            entity.EventCode = GenerateEventCode();
            entity.EventTitle = vm.EventTitle;
            entity.EventType = vm.EventType;
            entity.Description = vm.Description;

            entity.EventDate = vm.EventDate;
            entity.StartTime = vm.StartTime;
            entity.EndTime = vm.EndTime;

            entity.State = vm.State;
            entity.District = vm.District;
            entity.Block = vm.Block;
            entity.Village = vm.Village;
            entity.Booth = vm.Booth;

            entity.Venue = vm.Venue;
            entity.Landmark = vm.Landmark;

            entity.ExpectedCrowd = vm.ExpectedCrowd;
            entity.ActualCrowd = vm.ActualCrowd;

            entity.Budget = vm.Budget;
            entity.ActualExpense = vm.ActualExpense;

            entity.OrganizerName = vm.OrganizerName;
            entity.OrganizerMobile = vm.OrganizerMobile;

            entity.ChiefGuest = vm.ChiefGuest;
            entity.ChiefGuestMobile = vm.ChiefGuestMobile;

            entity.Priority = vm.Priority;
            entity.Status = vm.Status;
            entity.Remarks = vm.Remarks;

            entity.IsPublished = false;
            entity.IsCompleted = false;
            entity.IsCancelled = false;
            entity.IsActive = true;

            entity.CreatedBy = userId;
            entity.CreatedDate = DateTime.Now;

            _db.EventMasters.Add(entity);

            _db.SaveChanges();

            vm.EventId = entity.EventId;

            SaveEventProfile(vm, server);

            return true;
        }

        /// <summary>
        /// Updates an existing Event.
        /// Used by Edit Event screen.
        /// </summary>
        public bool UpdateEvent(EventVM vm, int userId)
        {
            return UpdateEvent(vm, userId, null);
        }

        public bool UpdateEvent(EventVM vm, int userId, HttpServerUtilityBase server)
        {
            if (vm == null)
            {
                return false;
            }

            EventMaster entity = _db.EventMasters
                .FirstOrDefault(x =>
                    x.EventId == vm.EventId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            if (IsDuplicateEvent(
                vm.EventTitle,
                vm.EventDate,
                vm.Venue,
                vm.EventId))
            {
                return false;
            }

            entity.EventTitle = vm.EventTitle;
            entity.EventType = vm.EventType;
            entity.Description = vm.Description;

            entity.EventDate = vm.EventDate;
            entity.StartTime = vm.StartTime;
            entity.EndTime = vm.EndTime;

            entity.State = vm.State;
            entity.District = vm.District;
            entity.Block = vm.Block;
            entity.Village = vm.Village;
            entity.Booth = vm.Booth;

            entity.Venue = vm.Venue;
            entity.Landmark = vm.Landmark;

            entity.ExpectedCrowd = vm.ExpectedCrowd;
            entity.ActualCrowd = vm.ActualCrowd;

            entity.Budget = vm.Budget;
            entity.ActualExpense = vm.ActualExpense;

            entity.OrganizerName = vm.OrganizerName;
            entity.OrganizerMobile = vm.OrganizerMobile;

            entity.ChiefGuest = vm.ChiefGuest;
            entity.ChiefGuestMobile = vm.ChiefGuestMobile;

            entity.Priority = vm.Priority;
            entity.Status = vm.Status;
            entity.Remarks = vm.Remarks;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            SaveEventProfile(vm, server);

            return true;
        }

        /// <summary>
        /// Soft deletes an event.
        /// Record is not removed from database.
        /// Used from Event List Delete button.
        /// </summary>
        public bool DeleteEvent(int eventId, int userId)
        {
            EventMaster entity = _db.EventMasters
                .FirstOrDefault(x =>
                    x.EventId == eventId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.IsActive = false;
            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        public List<EventTrackingItemVM> GetTrackingItems(string category, int take = 300)
        {
            EnsurePowerEventTables();

            DataTable table = QuerySql(
                @"SELECT TOP (@Take) t.*, e.EventTitle
                  FROM dbo.EventTrackingItem t
                  INNER JOIN dbo.EventMaster e ON e.EventId = t.EventId
                  WHERE t.IsActive = 1 AND t.Category = @Category
                  ORDER BY t.CreatedDate DESC",
                new SqlParameter("@Take", take),
                new SqlParameter("@Category", category));

            return table.Rows.Cast<DataRow>().Select(MapTrackingItem).ToList();
        }

        public EventTrackingItemVM GetTrackingItemById(int id)
        {
            EnsurePowerEventTables();

            DataTable table = QuerySql(
                @"SELECT t.*, e.EventTitle
                  FROM dbo.EventTrackingItem t
                  INNER JOIN dbo.EventMaster e ON e.EventId = t.EventId
                  WHERE t.EventTrackingItemId = @Id AND t.IsActive = 1",
                new SqlParameter("@Id", id));

            return table.Rows.Count == 0 ? null : MapTrackingItem(table.Rows[0]);
        }

        public bool SaveTrackingItem(EventTrackingItemVM vm, int userId)
        {
            if (vm == null)
            {
                return false;
            }

            EnsurePowerEventTables();

            ExecuteSql(@"INSERT INTO dbo.EventTrackingItem
                (EventId, Category, ItemName, Village, Ward, Booth, ResponsiblePerson, ResponsibleMobile,
                 ExpectedQuantity, ActualQuantity, Unit, EstimatedCost, ActualCost, ProviderName, ProviderMobile,
                 ReturnRequired, Returned, AppreciationPending, Status, Remarks, IsActive, CreatedBy, CreatedDate)
                VALUES
                (@EventId, @Category, @ItemName, @Village, @Ward, @Booth, @ResponsiblePerson, @ResponsibleMobile,
                 @ExpectedQuantity, @ActualQuantity, @Unit, @EstimatedCost, @ActualCost, @ProviderName, @ProviderMobile,
                 @ReturnRequired, @Returned, @AppreciationPending, @Status, @Remarks, 1, @UserId, GETDATE())",
                TrackingParameters(vm, userId).ToArray());

            return true;
        }

        public bool UpdateTrackingItem(EventTrackingItemVM vm, int userId)
        {
            if (vm == null)
            {
                return false;
            }

            EnsurePowerEventTables();

            var parameters = TrackingParameters(vm, userId).ToList();
            parameters.Add(new SqlParameter("@Id", vm.EventTrackingItemId));

            ExecuteSql(@"UPDATE dbo.EventTrackingItem
                SET EventId = @EventId,
                    Category = @Category,
                    ItemName = @ItemName,
                    Village = @Village,
                    Ward = @Ward,
                    Booth = @Booth,
                    ResponsiblePerson = @ResponsiblePerson,
                    ResponsibleMobile = @ResponsibleMobile,
                    ExpectedQuantity = @ExpectedQuantity,
                    ActualQuantity = @ActualQuantity,
                    Unit = @Unit,
                    EstimatedCost = @EstimatedCost,
                    ActualCost = @ActualCost,
                    ProviderName = @ProviderName,
                    ProviderMobile = @ProviderMobile,
                    ReturnRequired = @ReturnRequired,
                    Returned = @Returned,
                    AppreciationPending = @AppreciationPending,
                    Status = @Status,
                    Remarks = @Remarks,
                    UpdatedBy = @UserId,
                    UpdatedDate = GETDATE()
                WHERE EventTrackingItemId = @Id",
                parameters.ToArray());

            return true;
        }

        public bool DeleteTrackingItem(int id, int userId)
        {
            EnsurePowerEventTables();

            ExecuteSql(@"UPDATE dbo.EventTrackingItem
                SET IsActive = 0, UpdatedBy = @UserId, UpdatedDate = GETDATE()
                WHERE EventTrackingItemId = @Id",
                new SqlParameter("@Id", id),
                new SqlParameter("@UserId", userId));

            return true;
        }

        public List<System.Web.Mvc.SelectListItem> GetEventDropdown()
        {
            return _db.EventMasters
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.EventDate)
                .Take(300)
                .ToList()
                .Select(x => new System.Web.Mvc.SelectListItem
                {
                    Value = x.EventId.ToString(),
                    Text = x.EventTitle + " - " + x.EventDate.ToString("dd MMM yyyy")
                })
                .ToList();
        }

        private static EventTrackingItemVM MapTrackingItem(DataRow row)
        {
            return new EventTrackingItemVM
            {
                EventTrackingItemId = Convert.ToInt32(row["EventTrackingItemId"]),
                EventId = Convert.ToInt32(row["EventId"]),
                EventTitle = Convert.ToString(row["EventTitle"]),
                Category = Convert.ToString(row["Category"]),
                ItemName = Convert.ToString(row["ItemName"]),
                Village = Convert.ToString(row["Village"]),
                Ward = Convert.ToString(row["Ward"]),
                Booth = Convert.ToString(row["Booth"]),
                ResponsiblePerson = Convert.ToString(row["ResponsiblePerson"]),
                ResponsibleMobile = Convert.ToString(row["ResponsibleMobile"]),
                ExpectedQuantity = Convert.ToInt32(row["ExpectedQuantity"]),
                ActualQuantity = Convert.ToInt32(row["ActualQuantity"]),
                Unit = Convert.ToString(row["Unit"]),
                EstimatedCost = Convert.ToDecimal(row["EstimatedCost"]),
                ActualCost = Convert.ToDecimal(row["ActualCost"]),
                ProviderName = Convert.ToString(row["ProviderName"]),
                ProviderMobile = Convert.ToString(row["ProviderMobile"]),
                ReturnRequired = Convert.ToBoolean(row["ReturnRequired"]),
                Returned = Convert.ToBoolean(row["Returned"]),
                AppreciationPending = Convert.ToBoolean(row["AppreciationPending"]),
                Status = Convert.ToString(row["Status"]),
                Remarks = Convert.ToString(row["Remarks"]),
                IsActive = Convert.ToBoolean(row["IsActive"]),
                CreatedDate = Convert.ToDateTime(row["CreatedDate"])
            };
        }

        private static IEnumerable<SqlParameter> TrackingParameters(EventTrackingItemVM vm, int userId)
        {
            yield return new SqlParameter("@EventId", vm.EventId);
            yield return new SqlParameter("@Category", (object)vm.Category ?? DBNull.Value);
            yield return new SqlParameter("@ItemName", (object)vm.ItemName ?? DBNull.Value);
            yield return new SqlParameter("@Village", (object)vm.Village ?? DBNull.Value);
            yield return new SqlParameter("@Ward", (object)vm.Ward ?? DBNull.Value);
            yield return new SqlParameter("@Booth", (object)vm.Booth ?? DBNull.Value);
            yield return new SqlParameter("@ResponsiblePerson", (object)vm.ResponsiblePerson ?? DBNull.Value);
            yield return new SqlParameter("@ResponsibleMobile", (object)vm.ResponsibleMobile ?? DBNull.Value);
            yield return new SqlParameter("@ExpectedQuantity", vm.ExpectedQuantity);
            yield return new SqlParameter("@ActualQuantity", vm.ActualQuantity);
            yield return new SqlParameter("@Unit", (object)vm.Unit ?? DBNull.Value);
            yield return new SqlParameter("@EstimatedCost", vm.EstimatedCost);
            yield return new SqlParameter("@ActualCost", vm.ActualCost);
            yield return new SqlParameter("@ProviderName", (object)vm.ProviderName ?? DBNull.Value);
            yield return new SqlParameter("@ProviderMobile", (object)vm.ProviderMobile ?? DBNull.Value);
            yield return new SqlParameter("@ReturnRequired", vm.ReturnRequired);
            yield return new SqlParameter("@Returned", vm.Returned);
            yield return new SqlParameter("@AppreciationPending", vm.AppreciationPending);
            yield return new SqlParameter("@Status", (object)vm.Status ?? DBNull.Value);
            yield return new SqlParameter("@Remarks", (object)vm.Remarks ?? DBNull.Value);
            yield return new SqlParameter("@UserId", userId);
        }

        /// <summary>
        /// Publishes an event so it becomes visible in reports
        /// and public modules.
        /// </summary>
        public bool PublishEvent(int eventId, int userId)
        {
            EventMaster entity = _db.EventMasters
                .FirstOrDefault(x =>
                    x.EventId == eventId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.IsPublished = true;
            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Marks an event as completed.
        /// Called after rally/meeting finishes.
        /// </summary>
        public bool CompleteEvent(int eventId, int actualCrowd, decimal actualExpense, int userId)
        {
            EventMaster entity = _db.EventMasters
                .FirstOrDefault(x =>
                    x.EventId == eventId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.IsCompleted = true;
            entity.Status = "Completed";
            entity.ActualCrowd = actualCrowd;
            entity.ActualExpense = actualExpense;
            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Cancels an event.
        /// Keeps history but marks event as cancelled.
        /// </summary>
        public bool CancelEvent(int eventId, string remarks, int userId)
        {
            EventMaster entity = _db.EventMasters
                .FirstOrDefault(x =>
                    x.EventId == eventId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.IsCancelled = true;
            entity.Status = "Cancelled";
            entity.Remarks = remarks;
            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Reopens a cancelled event.
        /// Useful when cancelled programme is rescheduled.
        /// </summary>
        public bool ReopenEvent(int eventId, int userId)
        {
            EventMaster entity = _db.EventMasters
                .FirstOrDefault(x =>
                    x.EventId == eventId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.IsCancelled = false;
            entity.Status = "Planned";
            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Search events by Event Code, Title, Organizer,
        /// Venue, Village or District.
        /// Used in Event Grid Search.
        /// </summary>
        public List<EventVM> SearchEvents(string keyword)
        {
            IQueryable<EventMaster> query = _db.EventMasters
                .Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                query = query.Where(x =>
                    x.EventCode.Contains(keyword) ||
                    x.EventTitle.Contains(keyword) ||
                    x.OrganizerName.Contains(keyword) ||
                    x.Venue.Contains(keyword) ||
                    x.Village.Contains(keyword) ||
                    x.District.Contains(keyword));
            }

            return query
                .OrderByDescending(x => x.EventDate)
                .Select(x => new EventVM
                {
                    EventId = x.EventId,
                    EventCode = x.EventCode,
                    EventTitle = x.EventTitle,
                    EventType = x.EventType,
                    EventDate = x.EventDate,
                    Venue = x.Venue,
                    Village = x.Village,
                    District = x.District,
                    OrganizerName = x.OrganizerName,
                    Priority = x.Priority,
                    Status = x.Status
                })
                .ToList();
        }

        /// <summary>
        /// Returns all events of a district.
        /// Used in District Dashboard.
        /// </summary>
        public List<EventVM> GetEventsByDistrict(string district)
        {
            return _db.EventMasters
                .Where(x =>
                    x.IsActive &&
                    x.District == district)
                .OrderByDescending(x => x.EventDate)
                .Select(x => new EventVM
                {
                    EventId = x.EventId,
                    EventCode = x.EventCode,
                    EventTitle = x.EventTitle,
                    EventDate = x.EventDate,
                    Village = x.Village,
                    Venue = x.Venue,
                    OrganizerName = x.OrganizerName,
                    Status = x.Status
                })
                .ToList();
        }

        /// <summary>
        /// Returns all events of a village.
        /// Used for Village Coordinators.
        /// </summary>
        public List<EventVM> GetEventsByVillage(string village)
        {
            return _db.EventMasters
                .Where(x =>
                    x.IsActive &&
                    x.Village == village)
                .OrderByDescending(x => x.EventDate)
                .Select(x => new EventVM
                {
                    EventId = x.EventId,
                    EventCode = x.EventCode,
                    EventTitle = x.EventTitle,
                    EventDate = x.EventDate,
                    Venue = x.Venue,
                    OrganizerName = x.OrganizerName,
                    Status = x.Status
                })
                .ToList();
        }

        /// <summary>
        /// Returns events according to status.
        /// Example:
        /// Planned
        /// Ongoing
        /// Completed
        /// Cancelled
        /// </summary>
        public List<EventVM> GetEventsByStatus(string status)
        {
            return _db.EventMasters
                .Where(x =>
                    x.IsActive &&
                    x.Status == status)
                .OrderByDescending(x => x.EventDate)
                .Select(x => new EventVM
                {
                    EventId = x.EventId,
                    EventCode = x.EventCode,
                    EventTitle = x.EventTitle,
                    EventDate = x.EventDate,
                    Venue = x.Venue,
                    District = x.District,
                    Priority = x.Priority,
                    Status = x.Status
                })
                .ToList();
        }

        /// <summary>
        /// Returns High / Medium / Low priority events.
        /// Used by Dashboard widgets.
        /// </summary>
        public List<EventVM> GetEventsByPriority(string priority)
        {
            return _db.EventMasters
                .Where(x =>
                    x.IsActive &&
                    x.Priority == priority)
                .OrderBy(x => x.EventDate)
                .Select(x => new EventVM
                {
                    EventId = x.EventId,
                    EventCode = x.EventCode,
                    EventTitle = x.EventTitle,
                    EventDate = x.EventDate,
                    District = x.District,
                    Village = x.Village,
                    Priority = x.Priority,
                    Status = x.Status
                })
                .ToList();
        }

        /// <summary>
        /// Returns all events scheduled on a given date.
        /// Used in Calendar and Daily Schedule.
        /// </summary>
        public List<EventVM> GetEventsByDate(DateTime eventDate)
        {
            return _db.EventMasters
                .Where(x =>
                    x.IsActive &&
                    x.EventDate.Year == eventDate.Year &&
                    x.EventDate.Month == eventDate.Month &&
                    x.EventDate.Day == eventDate.Day)
                .OrderBy(x => x.StartTime)
                .Select(x => new EventVM
                {
                    EventId = x.EventId,
                    EventCode = x.EventCode,
                    EventTitle = x.EventTitle,
                    Venue = x.Venue,
                    Village = x.Village,
                    OrganizerName = x.OrganizerName,
                    StartTime = x.StartTime,
                    Status = x.Status
                })
                .ToList();
        }

        /// <summary>
        /// Generates the next Event Code.
        /// Format : EVT000001
        /// Used while creating a new Event.
        /// </summary>
        public string GenerateEventCode()
        {
            EventMaster lastEvent = _db.EventMasters
                .OrderByDescending(x => x.EventId)
                .FirstOrDefault();

            if (lastEvent == null)
            {
                return "EVT000001";
            }

            int nextNumber = lastEvent.EventId + 1;

            return "EVT" + nextNumber.ToString("D6");
        }

        /// <summary>
        /// Checks duplicate Event.
        /// Same Title + Date + Venue should not exist.
        /// </summary>
        public bool IsDuplicateEvent(
            string eventTitle,
            DateTime eventDate,
            string venue,
            int eventId)
        {
            return _db.EventMasters.Any(x =>
                x.IsActive &&
                x.EventTitle == eventTitle &&
                x.EventDate == eventDate &&
                x.Venue == venue &&
                x.EventId != eventId);
        }

        /// <summary>
        /// Returns total estimated crowd
        /// of all active events.
        /// Dashboard KPI.
        /// </summary>
        public int GetTotalExpectedCrowd()
        {
            return _db.EventMasters
                .Where(x => x.IsActive)
                .Sum(x => (int?)x.ExpectedCrowd) ?? 0;
        }

        /// <summary>
        /// Returns actual crowd attended.
        /// Dashboard KPI.
        /// </summary>
        public int GetTotalActualCrowd()
        {
            return _db.EventMasters
                .Where(x => x.IsActive)
                .Sum(x => (int?)x.ActualCrowd) ?? 0;
        }

        /// <summary>
        /// Returns total planned budget.
        /// Dashboard KPI.
        /// </summary>
        public decimal GetTotalBudget()
        {
            return _db.EventMasters
                .Where(x => x.IsActive)
                .Sum(x => (decimal?)x.Budget) ?? 0;
        }

        /// <summary>
        /// Returns total actual expense.
        /// Dashboard KPI.
        /// </summary>
        public decimal GetTotalActualExpense()
        {
            return _db.EventMasters
                .Where(x => x.IsActive)
                .Sum(x => (decimal?)x.ActualExpense) ?? 0;
        }

        /// <summary>
        /// Returns today's event count.
        /// Used in Dashboard Cards.
        /// </summary>
        public int GetTodayEventCount()
        {
            DateTime today = DateTime.Today;

            return _db.EventMasters.Count(x =>
                x.IsActive &&
                x.EventDate.Year == today.Year &&
                x.EventDate.Month == today.Month &&
                x.EventDate.Day == today.Day);
        }

        /// <summary>
        /// Returns upcoming event count.
        /// Dashboard KPI.
        /// </summary>
        public int GetUpcomingEventCount()
        {
            DateTime today = DateTime.Today;

            return _db.EventMasters.Count(x =>
                x.IsActive &&
                x.EventDate > today);
        }

        /// <summary>
        /// Returns completed event count.
        /// Dashboard KPI.
        /// </summary>
        public int GetCompletedEventCount()
        {
            return _db.EventMasters.Count(x =>
                x.IsActive &&
                x.IsCompleted);
        }

        /// <summary>
        /// Returns cancelled event count.
        /// Dashboard KPI.
        /// </summary>
        public int GetCancelledEventCount()
        {
            return _db.EventMasters.Count(x =>
                x.IsActive &&
                x.IsCancelled);
        }

        /// <summary>
        /// Returns events scheduled in next 7 days.
        /// Used on Dashboard Upcoming Events widget.
        /// </summary>
        public List<EventVM> GetUpcomingWeekEvents()
        {
            DateTime today = DateTime.Today;
            DateTime endDate = today.AddDays(7);

            return _db.EventMasters
                .Where(x =>
                    x.IsActive &&
                    x.EventDate >= today &&
                    x.EventDate <= endDate)
                .OrderBy(x => x.EventDate)
                .ThenBy(x => x.StartTime)
                .Select(x => new EventVM
                {
                    EventId = x.EventId,
                    EventCode = x.EventCode,
                    EventTitle = x.EventTitle,
                    EventDate = x.EventDate,
                    Venue = x.Venue,
                    Village = x.Village,
                    District = x.District,
                    OrganizerName = x.OrganizerName,
                    Priority = x.Priority,
                    Status = x.Status
                })
                .ToList();
        }

        /// <summary>
        /// Returns latest created events.
        /// Used in Recent Activity widget.
        /// </summary>
        public List<EventVM> GetRecentEvents(int count)
        {
            return _db.EventMasters
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CreatedDate)
                .Take(count)
                .Select(x => new EventVM
                {
                    EventId = x.EventId,
                    EventCode = x.EventCode,
                    EventTitle = x.EventTitle,
                    EventDate = x.EventDate,
                    District = x.District,
                    Venue = x.Venue,
                    Status = x.Status
                })
                .ToList();
        }

        /// <summary>
        /// Returns high priority events.
        /// Used by Dashboard Alert panel.
        /// </summary>
        public List<EventVM> GetHighPriorityEvents()
        {
            return _db.EventMasters
                .Where(x =>
                    x.IsActive &&
                    x.Priority == "High")
                .OrderBy(x => x.EventDate)
                .Select(x => new EventVM
                {
                    EventId = x.EventId,
                    EventCode = x.EventCode,
                    EventTitle = x.EventTitle,
                    EventDate = x.EventDate,
                    Venue = x.Venue,
                    District = x.District,
                    Status = x.Status
                })
                .ToList();
        }

        /// <summary>
        /// Returns all ongoing events.
        /// Dashboard Live Monitoring.
        /// </summary>
        public List<EventVM> GetOngoingEvents()
        {
            return _db.EventMasters
                .Where(x =>
                    x.IsActive &&
                    x.Status == "Ongoing")
                .OrderBy(x => x.EventDate)
                .Select(x => new EventVM
                {
                    EventId = x.EventId,
                    EventCode = x.EventCode,
                    EventTitle = x.EventTitle,
                    Venue = x.Venue,
                    District = x.District,
                    OrganizerName = x.OrganizerName,
                    Status = x.Status
                })
                .ToList();
        }

        /// <summary>
        /// Returns district wise event count.
        /// Dashboard Charts.
        /// </summary>
        public Dictionary<string, int> GetDistrictEventSummary()
        {
            return _db.EventMasters
                .Where(x => x.IsActive)
                .GroupBy(x => x.District)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns priority wise event count.
        /// Used in Pie Chart.
        /// </summary>
        public Dictionary<string, int> GetPrioritySummary()
        {
            return _db.EventMasters
                .Where(x => x.IsActive)
                .GroupBy(x => x.Priority)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns status wise event count.
        /// Used in Dashboard Charts.
        /// </summary>
        public Dictionary<string, int> GetStatusSummary()
        {
            return _db.EventMasters
                .Where(x => x.IsActive)
                .GroupBy(x => x.Status)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        #endregion

        #region Vehicle

        /// <summary>
        /// Maps EventVehicle Entity to ViewModel.
        /// This method is used by all Vehicle queries to avoid duplicate mapping code.
        /// </summary>
        private EventVehicleVM MapVehicle(EventVehicle entity)
        {
            if (entity == null)
            {
                return null;
            }

            EventVehicleVM vm = new EventVehicleVM();

            vm.EventVehicleId = entity.EventVehicleId;
            vm.EventId = entity.EventId;

            if (entity.EventMaster != null)
            {
                vm.EventCode = entity.EventMaster.EventCode;
                vm.EventTitle = entity.EventMaster.EventTitle;
            }

            vm.WorkerCode = entity.WorkerCode;
            vm.WorkerName = entity.WorkerName;
            vm.WorkerMobile = entity.WorkerMobile;
            vm.Responsibility = entity.Responsibility;

            vm.VillageName = entity.VillageName;
            vm.BoothName = entity.BoothName;

            vm.VehicleType = entity.VehicleType;
            vm.VehicleCount = entity.VehicleCount;
            vm.EstimatedPersons = entity.EstimatedPersons;
            vm.ActualPersons = entity.ActualPersons;

            vm.DriverName = entity.DriverName;
            vm.DriverMobile = entity.DriverMobile;
            vm.VehicleNumber = entity.VehicleNumber;

            vm.ArrivalTime = entity.ArrivalTime;
            vm.DepartureTime = entity.DepartureTime;
            vm.ParkingZone = entity.ParkingZone;

            vm.FuelExpense = entity.FuelExpense;

            vm.Remarks = entity.Remarks;

            vm.IsVerified = entity.IsVerified;
            vm.VerifiedBy = entity.VerifiedBy;
            vm.VerifiedDate = entity.VerifiedDate;

            vm.IsActive = entity.IsActive;

            vm.CreatedBy = entity.CreatedBy;
            vm.CreatedDate = entity.CreatedDate;

            vm.UpdatedBy = entity.UpdatedBy;
            vm.UpdatedDate = entity.UpdatedDate;

            return vm;
        }

        /// <summary>
        /// Returns all active vehicles of an Event.
        /// Used by Vehicle Grid.
        /// </summary>
        public List<EventVehicleVM> GetVehicles(int eventId)
        {
            return _db.EventVehicles
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .OrderBy(x => x.VillageName)
                .ThenBy(x => x.WorkerName)
                .ToList()
                .Select(MapVehicle)
                .ToList();
        }

        /// <summary>
        /// Returns single vehicle information.
        /// Used by Edit Vehicle Screen.
        /// </summary>
        public EventVehicleVM GetVehicleById(int eventVehicleId)
        {
            EventVehicle entity =
                _db.EventVehicles
                .FirstOrDefault(x =>
                    x.EventVehicleId == eventVehicleId &&
                    x.IsActive);

            return MapVehicle(entity);
        }

        /// <summary>
        /// Returns all vehicles of a Village.
        /// Used in Village Wise Report.
        /// </summary>
        public List<EventVehicleVM> GetVehiclesByVillage(string villageName)
        {
            return _db.EventVehicles
                .Where(x =>
                    x.IsActive &&
                    x.VillageName == villageName)
                .OrderBy(x => x.WorkerName)
                .ToList()
                .Select(MapVehicle)
                .ToList();
        }

        /// <summary>
        /// Returns all vehicles of a Booth.
        /// Used in Booth Dashboard.
        /// </summary>
        public List<EventVehicleVM> GetVehiclesByBooth(string boothName)
        {
            return _db.EventVehicles
                .Where(x =>
                    x.IsActive &&
                    x.BoothName == boothName)
                .OrderBy(x => x.WorkerName)
                .ToList()
                .Select(MapVehicle)
                .ToList();
        }

        /// <summary>
        /// Returns all vehicles by Vehicle Type.
        /// Example:
        /// Bus
        /// Car
        /// Bike
        /// Tractor
        /// </summary>
        public List<EventVehicleVM> GetVehiclesByVehicleType(string vehicleType)
        {
            return _db.EventVehicles
                .Where(x =>
                    x.IsActive &&
                    x.VehicleType == vehicleType)
                .OrderBy(x => x.WorkerName)
                .ToList()
                .Select(MapVehicle)
                .ToList();
        }

        /// <summary>
        /// Returns vehicles assigned to a Driver.
        /// Used in Driver Report.
        /// </summary>
        public List<EventVehicleVM> GetVehiclesByDriver(string driverName)
        {
            return _db.EventVehicles
                .Where(x =>
                    x.IsActive &&
                    x.DriverName == driverName)
                .OrderBy(x => x.VehicleNumber)
                .ToList()
                .Select(MapVehicle)
                .ToList();
        }

        /// <summary>
        /// Search Vehicle Information.
        /// Used by Vehicle Search Box.
        /// </summary>
        public List<EventVehicleVM> SearchVehicles(string keyword)
        {
            IQueryable<EventVehicle> query =
                _db.EventVehicles
                .Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                query = query.Where(x =>

                    (x.WorkerCode != null &&
                     x.WorkerCode.Contains(keyword))

                    ||

                    (x.WorkerName != null &&
                     x.WorkerName.Contains(keyword))

                    ||

                    (x.DriverName != null &&
                     x.DriverName.Contains(keyword))

                    ||

                    (x.DriverMobile != null &&
                     x.DriverMobile.Contains(keyword))

                    ||

                    (x.VehicleNumber != null &&
                     x.VehicleNumber.Contains(keyword))

                    ||

                    (x.VehicleType != null &&
                     x.VehicleType.Contains(keyword))

                    ||

                    (x.VillageName != null &&
                     x.VillageName.Contains(keyword))

                    ||

                    (x.BoothName != null &&
                     x.BoothName.Contains(keyword)));
            }

            return query
                .OrderBy(x => x.VillageName)
                .ThenBy(x => x.WorkerName)
                .ToList()
                .Select(MapVehicle)
                .ToList();
        }

        /// <summary>
        /// Creates a new Event Vehicle record.
        /// Used in Create Vehicle screen.
        /// </summary>
        public bool SaveVehicle(EventVehicleVM vm, int userId)
        {
            if (vm == null)
            {
                return false;
            }

            EventVehicle entity = new EventVehicle();

            entity.EventId = vm.EventId;

            entity.WorkerCode = vm.WorkerCode;
            entity.WorkerName = vm.WorkerName;
            entity.WorkerMobile = vm.WorkerMobile;
            entity.Responsibility = vm.Responsibility;

            entity.VillageName = vm.VillageName;
            entity.BoothName = vm.BoothName;

            entity.VehicleType = vm.VehicleType;
            entity.VehicleCount = vm.VehicleCount;
            entity.EstimatedPersons = vm.EstimatedPersons;
            entity.ActualPersons = vm.ActualPersons;

            entity.DriverName = vm.DriverName;
            entity.DriverMobile = vm.DriverMobile;
            entity.VehicleNumber = vm.VehicleNumber;

            entity.ArrivalTime = vm.ArrivalTime;
            entity.DepartureTime = vm.DepartureTime;
            entity.ParkingZone = vm.ParkingZone;

            entity.FuelExpense = vm.FuelExpense;

            entity.Remarks = vm.Remarks;

            entity.IsVerified = false;
            entity.VerifiedBy = null;
            entity.VerifiedDate = null;

            entity.IsActive = true;

            entity.CreatedBy = userId;
            entity.CreatedDate = DateTime.Now;

            _db.EventVehicles.Add(entity);

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Updates Vehicle Information.
        /// Used by Edit Vehicle screen.
        /// </summary>
        public bool UpdateVehicle(EventVehicleVM vm, int userId)
        {
            if (vm == null)
            {
                return false;
            }

            EventVehicle entity =
                _db.EventVehicles
                .FirstOrDefault(x =>
                    x.EventVehicleId == vm.EventVehicleId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.EventId = vm.EventId;

            entity.WorkerCode = vm.WorkerCode;
            entity.WorkerName = vm.WorkerName;
            entity.WorkerMobile = vm.WorkerMobile;
            entity.Responsibility = vm.Responsibility;

            entity.VillageName = vm.VillageName;
            entity.BoothName = vm.BoothName;

            entity.VehicleType = vm.VehicleType;
            entity.VehicleCount = vm.VehicleCount;
            entity.EstimatedPersons = vm.EstimatedPersons;
            entity.ActualPersons = vm.ActualPersons;

            entity.DriverName = vm.DriverName;
            entity.DriverMobile = vm.DriverMobile;
            entity.VehicleNumber = vm.VehicleNumber;

            entity.ArrivalTime = vm.ArrivalTime;
            entity.DepartureTime = vm.DepartureTime;
            entity.ParkingZone = vm.ParkingZone;

            entity.FuelExpense = vm.FuelExpense;

            entity.Remarks = vm.Remarks;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Soft deletes Vehicle.
        /// Used from Vehicle List.
        /// </summary>
        public bool DeleteVehicle(int eventVehicleId, int userId)
        {
            EventVehicle entity =
                _db.EventVehicles
                .FirstOrDefault(x =>
                    x.EventVehicleId == eventVehicleId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.IsActive = false;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Checks duplicate Vehicle Number.
        /// Prevents duplicate registration.
        /// </summary>
        public bool IsDuplicateVehicleNumber(string vehicleNumber, int eventVehicleId)
        {
            if (string.IsNullOrWhiteSpace(vehicleNumber))
            {
                return false;
            }

            vehicleNumber = vehicleNumber.Trim();

            return _db.EventVehicles.Any(x =>
                x.IsActive &&
                x.VehicleNumber == vehicleNumber &&
                x.EventVehicleId != eventVehicleId);
        }

        /// <summary>
        /// Returns total vehicles assigned in an event.
        /// Used in Event Dashboard.
        /// </summary>
        public int GetVehicleCount(int eventId)
        {
            return _db.EventVehicles
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .Sum(x => (int?)x.VehicleCount) ?? 0;
        }

        /// <summary>
        /// Marks a vehicle record as verified.
        /// Used by Admin after validating vehicle details.
        /// </summary>
        public bool VerifyVehicle(int eventVehicleId, int userId)
        {
            EventVehicle entity = _db.EventVehicles
                .FirstOrDefault(x =>
                    x.EventVehicleId == eventVehicleId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.IsVerified = true;
            entity.VerifiedBy = userId;
            entity.VerifiedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Removes verification from a vehicle.
        /// Used if verification needs to be revoked.
        /// </summary>
        public bool UnVerifyVehicle(int eventVehicleId)
        {
            EventVehicle entity = _db.EventVehicles
                .FirstOrDefault(x =>
                    x.EventVehicleId == eventVehicleId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.IsVerified = false;
            entity.VerifiedBy = null;
            entity.VerifiedDate = null;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Returns all verified vehicles of an event.
        /// Used in Verified Vehicle Report.
        /// </summary>
        public List<EventVehicleVM> GetVerifiedVehicles(int eventId)
        {
            return _db.EventVehicles
                .Where(x =>
                    x.IsActive &&
                    x.IsVerified &&
                    x.EventId == eventId)
                .OrderBy(x => x.VillageName)
                .ThenBy(x => x.WorkerName)
                .ToList()
                .Select(MapVehicle)
                .ToList();
        }

        /// <summary>
        /// Returns vehicles pending verification.
        /// Used in Admin Dashboard.
        /// </summary>
        public List<EventVehicleVM> GetPendingVerificationVehicles(int eventId)
        {
            return _db.EventVehicles
                .Where(x =>
                    x.IsActive &&
                    !x.IsVerified &&
                    x.EventId == eventId)
                .OrderBy(x => x.CreatedDate)
                .ToList()
                .Select(MapVehicle)
                .ToList();
        }

        /// <summary>
        /// Returns total verified vehicles.
        /// Dashboard KPI.
        /// </summary>
        public int GetVerifiedVehicleCount(int eventId)
        {
            return _db.EventVehicles.Count(x =>
                x.IsActive &&
                x.IsVerified &&
                x.EventId == eventId);
        }

        /// <summary>
        /// Returns total pending verification vehicles.
        /// Dashboard KPI.
        /// </summary>
        public int GetPendingVerificationVehicleCount(int eventId)
        {
            return _db.EventVehicles.Count(x =>
                x.IsActive &&
                !x.IsVerified &&
                x.EventId == eventId);
        }

        /// <summary>
        /// Returns total estimated persons from all vehicles.
        /// Dashboard KPI.
        /// </summary>
        public int GetEstimatedPersonCount(int eventId)
        {
            return _db.EventVehicles
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .Sum(x => (int?)x.EstimatedPersons) ?? 0;
        }

        /// <summary>
        /// Returns actual persons arrived through vehicles.
        /// Dashboard KPI.
        /// </summary>
        public int GetActualPersonCount(int eventId)
        {
            return _db.EventVehicles
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .Sum(x => (int?)x.ActualPersons) ?? 0;
        }

        /// <summary>
        /// Returns total fuel expense for an event.
        /// Dashboard KPI.
        /// </summary>
        public decimal GetFuelExpense(int eventId)
        {
            return _db.EventVehicles
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .Sum(x => (decimal?)x.FuelExpense) ?? 0;
        }

        /// <summary>
        /// Returns village wise vehicle summary.
        /// Used in Dashboard Village Chart.
        /// </summary>
        public Dictionary<string, int> GetVillageVehicleSummary(int eventId)
        {
            return _db.EventVehicles
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .GroupBy(x => x.VillageName)
                .ToDictionary(
                    x => x.Key,
                    x => x.Sum(y => y.VehicleCount));
        }

        /// <summary>
        /// Returns booth wise vehicle summary.
        /// Used in Dashboard Booth Chart.
        /// </summary>
        public Dictionary<string, int> GetBoothVehicleSummary(int eventId)
        {
            return _db.EventVehicles
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .GroupBy(x => x.BoothName)
                .ToDictionary(
                    x => x.Key,
                    x => x.Sum(y => y.VehicleCount));
        }

        /// <summary>
        /// Returns vehicle type summary.
        /// Example:
        /// Bus
        /// Car
        /// Bike
        /// Tractor
        /// </summary>
        public Dictionary<string, int> GetVehicleTypeSummary(int eventId)
        {
            return _db.EventVehicles
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .GroupBy(x => x.VehicleType)
                .ToDictionary(
                    x => x.Key,
                    x => x.Sum(y => y.VehicleCount));
        }

        /// <summary>
        /// Returns driver wise vehicle summary.
        /// Used in Driver Dashboard.
        /// </summary>
        public Dictionary<string, int> GetDriverVehicleSummary(int eventId)
        {
            return _db.EventVehicles
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .GroupBy(x => x.DriverName)
                .ToDictionary(
                    x => x.Key,
                    x => x.Sum(y => y.VehicleCount));
        }

        /// <summary>
        /// Returns recently created vehicle entries.
        /// Used in Dashboard Recent Activity.
        /// </summary>
        public List<EventVehicleVM> GetRecentVehicles(int count)
        {
            return _db.EventVehicles
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CreatedDate)
                .Take(count)
                .ToList()
                .Select(MapVehicle)
                .ToList();
        }

        /// <summary>
        /// Returns all vehicles for Dashboard widget.
        /// Ordered by latest event first.
        /// </summary>
        public List<EventVehicleVM> GetDashboardVehicles(int count)
        {
            return _db.EventVehicles
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.EventMaster.EventDate)
                .ThenBy(x => x.VillageName)
                .Take(count)
                .ToList()
                .Select(MapVehicle)
                .ToList();
        }

        /// <summary>
        /// Returns total active vehicle records.
        /// Global Dashboard KPI.
        /// </summary>
        public int GetTotalVehicleRecords()
        {
            return _db.EventVehicles.Count(x => x.IsActive);
        }

        /// <summary>
        /// Returns total verified vehicle records.
        /// Global Dashboard KPI.
        /// </summary>
        public int GetTotalVerifiedVehicleRecords()
        {
            return _db.EventVehicles.Count(x =>
                x.IsActive &&
                x.IsVerified);
        }

        /// <summary>
        /// Returns total fuel expense across all events.
        /// Global Dashboard KPI.
        /// </summary>
        public decimal GetTotalFuelExpense()
        {
            return _db.EventVehicles
                .Where(x => x.IsActive)
                .Sum(x => (decimal?)x.FuelExpense) ?? 0;
        }

        #endregion
        #region Attendance

        /// <summary>
        /// Maps EventAttendance Entity to ViewModel.
        /// Used by all Attendance queries.
        /// </summary>
        private EventAttendanceVM MapAttendance(EventAttendance entity)
        {
            if (entity == null)
            {
                return null;
            }

            EventAttendanceVM vm = new EventAttendanceVM();

            vm.AttendanceId = entity.AttendanceId;
            vm.EventId = entity.EventId;

            if (entity.EventMaster != null)
            {
                vm.EventCode = entity.EventMaster.EventCode;
                vm.EventTitle = entity.EventMaster.EventTitle;
            }

            vm.AttendanceType = entity.AttendanceType;
            vm.PersonName = entity.PersonName;
            vm.FatherName = entity.FatherName;
            vm.MobileNumber = entity.MobileNumber;
            vm.Gender = entity.Gender;
            vm.Age = entity.Age;

            vm.State = entity.State;
            vm.District = entity.District;
            vm.Block = entity.Block;
            vm.Village = entity.Village;
            vm.Booth = entity.Booth;

            vm.WorkerName = entity.WorkerName;
            vm.WorkerCode = entity.WorkerCode;
            vm.MemberCode = entity.MemberCode;

            vm.AttendanceMode = entity.AttendanceMode;

            vm.CheckInTime = entity.CheckInTime;
            vm.CheckOutTime = entity.CheckOutTime;

            vm.IsVIP = entity.IsVIP;
            vm.IsVolunteer = entity.IsVolunteer;
            vm.IsWorker = entity.IsWorker;
            vm.IsVerified = entity.IsVerified;

            vm.Remarks = entity.Remarks;

            vm.IsActive = entity.IsActive;
            vm.CreatedBy = entity.CreatedBy;
            vm.CreatedDate = entity.CreatedDate;
            vm.UpdatedBy = entity.UpdatedBy;
            vm.UpdatedDate = entity.UpdatedDate;

            return vm;
        }

        /// <summary>
        /// Returns all attendance records of an event.
        /// Used in Attendance Grid.
        /// </summary>
        public List<EventAttendanceVM> GetAttendances(int eventId)
        {
            return _db.EventAttendances
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .OrderBy(x => x.CheckInTime)
                .ToList()
                .Select(MapAttendance)
                .ToList();
        }

        /// <summary>
        /// Returns a single attendance record.
        /// Used in Edit Attendance screen.
        /// </summary>
        public EventAttendanceVM GetAttendanceById(int attendanceId)
        {
            EventAttendance entity =
                _db.EventAttendances
                .FirstOrDefault(x =>
                    x.AttendanceId == attendanceId &&
                    x.IsActive);

            return MapAttendance(entity);
        }

        /// <summary>
        /// Returns VIP Attendance.
        /// Dashboard VIP Widget.
        /// </summary>
        public List<EventAttendanceVM> GetVIPAttendances(int eventId)
        {
            return _db.EventAttendances
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId &&
                    x.IsVIP)
                .OrderBy(x => x.PersonName)
                .ToList()
                .Select(MapAttendance)
                .ToList();
        }

        /// <summary>
        /// Returns Volunteer Attendance.
        /// Dashboard Volunteer Widget.
        /// </summary>
        public List<EventAttendanceVM> GetVolunteerAttendances(int eventId)
        {
            return _db.EventAttendances
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId &&
                    x.IsVolunteer)
                .OrderBy(x => x.PersonName)
                .ToList()
                .Select(MapAttendance)
                .ToList();
        }

        /// <summary>
        /// Returns Worker Attendance.
        /// Dashboard Worker Widget.
        /// </summary>
        public List<EventAttendanceVM> GetWorkerAttendances(int eventId)
        {
            return _db.EventAttendances
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId &&
                    x.IsWorker)
                .OrderBy(x => x.PersonName)
                .ToList()
                .Select(MapAttendance)
                .ToList();
        }

        /// <summary>
        /// Searches Attendance.
        /// Used in Attendance Search Box.
        /// </summary>
        public List<EventAttendanceVM> SearchAttendances(string keyword)
        {
            IQueryable<EventAttendance> query =
                _db.EventAttendances
                .Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                query = query.Where(x =>

                    (x.PersonName != null &&
                     x.PersonName.Contains(keyword))

                    ||

                    (x.MobileNumber != null &&
                     x.MobileNumber.Contains(keyword))

                    ||

                    (x.WorkerName != null &&
                     x.WorkerName.Contains(keyword))

                    ||

                    (x.WorkerCode != null &&
                     x.WorkerCode.Contains(keyword))

                    ||

                    (x.MemberCode != null &&
                     x.MemberCode.Contains(keyword))

                    ||

                    (x.Village != null &&
                     x.Village.Contains(keyword))

                    ||

                    (x.Booth != null &&
                     x.Booth.Contains(keyword)));
            }

            return query
                .OrderByDescending(x => x.CheckInTime)
                .ToList()
                .Select(MapAttendance)
                .ToList();
        }

        /// <summary>
        /// Saves a new attendance record.
        /// Used during Check-In or manual attendance entry.
        /// </summary>
        public bool SaveAttendance(EventAttendanceVM vm, int userId)
        {
            if (vm == null)
            {
                return false;
            }

            EventAttendance entity = new EventAttendance();

            entity.EventId = vm.EventId;

            entity.AttendanceType = vm.AttendanceType;
            entity.PersonName = vm.PersonName;
            entity.FatherName = vm.FatherName;
            entity.MobileNumber = vm.MobileNumber;
            entity.Gender = vm.Gender;
            entity.Age = vm.Age;

            entity.State = vm.State;
            entity.District = vm.District;
            entity.Block = vm.Block;
            entity.Village = vm.Village;
            entity.Booth = vm.Booth;

            entity.WorkerName = vm.WorkerName;
            entity.WorkerCode = vm.WorkerCode;
            entity.MemberCode = vm.MemberCode;

            entity.AttendanceMode = vm.AttendanceMode;

            entity.CheckInTime = vm.CheckInTime;
            entity.CheckOutTime = vm.CheckOutTime;

            entity.IsVIP = vm.IsVIP;
            entity.IsVolunteer = vm.IsVolunteer;
            entity.IsWorker = vm.IsWorker;
            entity.IsVerified = vm.IsVerified;

            entity.Remarks = vm.Remarks;

            entity.IsActive = true;
            entity.CreatedBy = userId;
            entity.CreatedDate = DateTime.Now;

            _db.EventAttendances.Add(entity);

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Updates attendance information.
        /// Used by Attendance Edit screen.
        /// </summary>
        public bool UpdateAttendance(EventAttendanceVM vm, int userId)
        {
            if (vm == null)
            {
                return false;
            }

            EventAttendance entity =
                _db.EventAttendances
                .FirstOrDefault(x =>
                    x.AttendanceId == vm.AttendanceId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.AttendanceType = vm.AttendanceType;
            entity.PersonName = vm.PersonName;
            entity.FatherName = vm.FatherName;
            entity.MobileNumber = vm.MobileNumber;
            entity.Gender = vm.Gender;
            entity.Age = vm.Age;

            entity.State = vm.State;
            entity.District = vm.District;
            entity.Block = vm.Block;
            entity.Village = vm.Village;
            entity.Booth = vm.Booth;

            entity.WorkerName = vm.WorkerName;
            entity.WorkerCode = vm.WorkerCode;
            entity.MemberCode = vm.MemberCode;

            entity.AttendanceMode = vm.AttendanceMode;

            entity.CheckInTime = vm.CheckInTime;
            entity.CheckOutTime = vm.CheckOutTime;

            entity.IsVIP = vm.IsVIP;
            entity.IsVolunteer = vm.IsVolunteer;
            entity.IsWorker = vm.IsWorker;
            entity.IsVerified = vm.IsVerified;

            entity.Remarks = vm.Remarks;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Soft deletes an attendance record.
        /// Used from Attendance List.
        /// </summary>
        public bool DeleteAttendance(int attendanceId, int userId)
        {
            EventAttendance entity =
                _db.EventAttendances
                .FirstOrDefault(x =>
                    x.AttendanceId == attendanceId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.IsActive = false;
            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Checks whether the person has already checked in
        /// for the same event.
        /// </summary>
        public bool IsDuplicateAttendance(int eventId, string mobileNumber, int attendanceId)
        {
            if (string.IsNullOrWhiteSpace(mobileNumber))
            {
                return false;
            }

            mobileNumber = mobileNumber.Trim();

            return _db.EventAttendances.Any(x =>
                x.IsActive &&
                x.EventId == eventId &&
                x.MobileNumber == mobileNumber &&
                x.AttendanceId != attendanceId);
        }

        /// <summary>
        /// Performs Check-Out for an attendee.
        /// Updates the CheckOutTime only.
        /// </summary>
        public bool CheckOutAttendance(int attendanceId, int userId)
        {
            EventAttendance entity =
                _db.EventAttendances
                .FirstOrDefault(x =>
                    x.AttendanceId == attendanceId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.CheckOutTime = DateTime.Now;
            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Marks attendance as verified.
        /// Used by Admin after verification.
        /// </summary>
        public bool VerifyAttendance(int attendanceId, int userId)
        {
            EventAttendance entity =
                _db.EventAttendances
                .FirstOrDefault(x =>
                    x.AttendanceId == attendanceId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.IsVerified = true;
            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Returns total attendance of an event.
        /// Dashboard KPI.
        /// </summary>
        public int GetTotalAttendance(int eventId)
        {
            return _db.EventAttendances.Count(x =>
                x.IsActive &&
                x.EventId == eventId);
        }

        /// <summary>
        /// Returns total verified attendance.
        /// Dashboard KPI.
        /// </summary>
        public int GetVerifiedAttendance(int eventId)
        {
            return _db.EventAttendances.Count(x =>
                x.IsActive &&
                x.EventId == eventId &&
                x.IsVerified);
        }

        /// <summary>
        /// Returns VIP attendance count.
        /// Dashboard KPI.
        /// </summary>
        public int GetVIPAttendanceCount(int eventId)
        {
            return _db.EventAttendances.Count(x =>
                x.IsActive &&
                x.EventId == eventId &&
                x.IsVIP);
        }

        /// <summary>
        /// Returns Worker attendance count.
        /// Dashboard KPI.
        /// </summary>
        public int GetWorkerAttendanceCount(int eventId)
        {
            return _db.EventAttendances.Count(x =>
                x.IsActive &&
                x.EventId == eventId &&
                x.IsWorker);
        }

        /// <summary>
        /// Returns Volunteer attendance count.
        /// Dashboard KPI.
        /// </summary>
        public int GetVolunteerAttendanceCount(int eventId)
        {
            return _db.EventAttendances.Count(x =>
                x.IsActive &&
                x.EventId == eventId &&
                x.IsVolunteer);
        }

        /// <summary>
        /// Returns currently checked-in attendees.
        /// Used in Live Attendance Dashboard.
        /// </summary>
        public int GetLiveAttendanceCount(int eventId)
        {
            return _db.EventAttendances.Count(x =>
                x.IsActive &&
                x.EventId == eventId &&
                x.CheckOutTime == null);
        }

        /// <summary>
        /// Returns Village wise attendance summary.
        /// Used in Dashboard Charts.
        /// </summary>
        public Dictionary<string, int> GetVillageAttendanceSummary(int eventId)
        {
            return _db.EventAttendances
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .GroupBy(x => x.Village)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns Booth wise attendance summary.
        /// Used in Dashboard Charts.
        /// </summary>
        public Dictionary<string, int> GetBoothAttendanceSummary(int eventId)
        {
            return _db.EventAttendances
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .GroupBy(x => x.Booth)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns Attendance Type summary.
        /// Example:
        /// Public
        /// Worker
        /// VIP
        /// Volunteer
        /// </summary>
        public Dictionary<string, int> GetAttendanceTypeSummary(int eventId)
        {
            return _db.EventAttendances
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .GroupBy(x => x.AttendanceType)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns recently checked-in attendees.
        /// Used in Dashboard Recent Activity.
        /// </summary>
        public List<EventAttendanceVM> GetRecentAttendances(int count)
        {
            return _db.EventAttendances
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CheckInTime)
                .Take(count)
                .ToList()
                .Select(MapAttendance)
                .ToList();
        }

        /// <summary>
        /// Returns attendance records for Dashboard Widget.
        /// </summary>
        public List<EventAttendanceVM> GetDashboardAttendances(int count)
        {
            return _db.EventAttendances
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CheckInTime)
                .Take(count)
                .ToList()
                .Select(MapAttendance)
                .ToList();
        }

        /// <summary>
        /// Returns total attendance across all events.
        /// Global Dashboard KPI.
        /// </summary>
        public int GetTotalAttendanceRecords()
        {
            return _db.EventAttendances.Count(x => x.IsActive);
        }

        /// <summary>
        /// Returns total verified attendance across all events.
        /// Global Dashboard KPI.
        /// </summary>
        public int GetTotalVerifiedAttendanceRecords()
        {
            return _db.EventAttendances.Count(x =>
                x.IsActive &&
                x.IsVerified);
        }

        #endregion
        #region Team

        /// <summary>
        /// Maps EventTeam entity to ViewModel.
        /// Used by all Team queries.
        /// </summary>
        private EventTeamVM MapTeam(EventTeam entity)
        {
            if (entity == null)
            {
                return null;
            }

            EventTeamVM vm = new EventTeamVM();

            vm.EventTeamId = entity.EventTeamId;
            vm.EventId = entity.EventId;

            if (entity.EventMaster != null)
            {
                vm.EventCode = entity.EventMaster.EventCode;
                vm.EventTitle = entity.EventMaster.EventTitle;
            }

            vm.TeamName = entity.TeamName;
            vm.TeamLeaderName = entity.TeamLeaderName;
            vm.TeamLeaderMobile = entity.TeamLeaderMobile;

            vm.TotalMembers = entity.TotalMembers;
            vm.RequiredMembers = entity.RequiredMembers;

            vm.AssignedArea = entity.AssignedArea;

            vm.DutyStartTime = entity.DutyStartTime;
            vm.DutyEndTime = entity.DutyEndTime;

            vm.Status = entity.Status;
            vm.Priority = entity.Priority;

            vm.Instructions = entity.Instructions;
            vm.Remarks = entity.Remarks;

            vm.IsCompleted = entity.IsCompleted;
            vm.CompletedDate = entity.CompletedDate;

            vm.ActiveMembers = entity.EventTeamMembers.Count(x => x.IsActive);

            vm.IsActive = entity.IsActive;
            vm.CreatedBy = entity.CreatedBy;
            vm.CreatedDate = entity.CreatedDate;
            vm.UpdatedBy = entity.UpdatedBy;
            vm.UpdatedDate = entity.UpdatedDate;

            return vm;
        }

        /// <summary>
        /// Returns all active teams of an event.
        /// Used in Team Management Grid.
        /// </summary>
        public List<EventTeamVM> GetTeams(int eventId)
        {
            return _db.EventTeams
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .OrderBy(x => x.TeamName)
                .ToList()
                .Select(MapTeam)
                .ToList();
        }

        /// <summary>
        /// Returns single Team.
        /// Used in Edit Team screen.
        /// </summary>
        public EventTeamVM GetTeamById(int eventTeamId)
        {
            EventTeam entity =
                _db.EventTeams
                .FirstOrDefault(x =>
                    x.EventTeamId == eventTeamId &&
                    x.IsActive);

            return MapTeam(entity);
        }

        /// <summary>
        /// Returns completed teams.
        /// Dashboard Widget.
        /// </summary>
        public List<EventTeamVM> GetCompletedTeams(int eventId)
        {
            return _db.EventTeams
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId &&
                    x.IsCompleted)
                .OrderBy(x => x.TeamName)
                .ToList()
                .Select(MapTeam)
                .ToList();
        }

        /// <summary>
        /// Returns pending teams.
        /// Dashboard Widget.
        /// </summary>
        public List<EventTeamVM> GetPendingTeams(int eventId)
        {
            return _db.EventTeams
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId &&
                    !x.IsCompleted)
                .OrderBy(x => x.TeamName)
                .ToList()
                .Select(MapTeam)
                .ToList();
        }

        /// <summary>
        /// Searches Teams.
        /// Used in Team Search.
        /// </summary>
        public List<EventTeamVM> SearchTeams(string keyword)
        {
            IQueryable<EventTeam> query =
                _db.EventTeams
                .Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                query = query.Where(x =>

                    (x.TeamName != null &&
                     x.TeamName.Contains(keyword))

                    ||

                    (x.TeamLeaderName != null &&
                     x.TeamLeaderName.Contains(keyword))

                    ||

                    (x.TeamLeaderMobile != null &&
                     x.TeamLeaderMobile.Contains(keyword))

                    ||

                    (x.AssignedArea != null &&
                     x.AssignedArea.Contains(keyword))

                    ||

                    (x.Status != null &&
                     x.Status.Contains(keyword))

                    ||

                    (x.Priority != null &&
                     x.Priority.Contains(keyword)));
            }

            return query
                .OrderBy(x => x.TeamName)
                .ToList()
                .Select(MapTeam)
                .ToList();
        }

        /// <summary>
        /// Creates a new Team.
        /// Used from Create Team screen.
        /// </summary>
        public bool SaveTeam(EventTeamVM vm, int userId)
        {
            if (vm == null)
            {
                return false;
            }

            EventTeam entity = new EventTeam();

            entity.EventId = vm.EventId;

            entity.TeamName = vm.TeamName;
            entity.TeamLeaderName = vm.TeamLeaderName;
            entity.TeamLeaderMobile = vm.TeamLeaderMobile;

            entity.TotalMembers = vm.TotalMembers;
            entity.RequiredMembers = vm.RequiredMembers;

            entity.AssignedArea = vm.AssignedArea;

            entity.DutyStartTime = vm.DutyStartTime;
            entity.DutyEndTime = vm.DutyEndTime;

            entity.Status = vm.Status;
            entity.Priority = vm.Priority;

            entity.Instructions = vm.Instructions;
            entity.Remarks = vm.Remarks;

            entity.IsCompleted = false;
            entity.CompletedDate = null;

            entity.IsActive = true;

            entity.CreatedBy = userId;
            entity.CreatedDate = DateTime.Now;

            _db.EventTeams.Add(entity);

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Updates Team information.
        /// Used from Edit Team screen.
        /// </summary>
        public bool UpdateTeam(EventTeamVM vm, int userId)
        {
            if (vm == null)
            {
                return false;
            }

            EventTeam entity =
                _db.EventTeams
                .FirstOrDefault(x =>
                    x.EventTeamId == vm.EventTeamId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.EventId = vm.EventId;

            entity.TeamName = vm.TeamName;
            entity.TeamLeaderName = vm.TeamLeaderName;
            entity.TeamLeaderMobile = vm.TeamLeaderMobile;

            entity.TotalMembers = vm.TotalMembers;
            entity.RequiredMembers = vm.RequiredMembers;

            entity.AssignedArea = vm.AssignedArea;

            entity.DutyStartTime = vm.DutyStartTime;
            entity.DutyEndTime = vm.DutyEndTime;

            entity.Status = vm.Status;
            entity.Priority = vm.Priority;

            entity.Instructions = vm.Instructions;
            entity.Remarks = vm.Remarks;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Soft deletes a Team.
        /// Used from Team List.
        /// </summary>
        public bool DeleteTeam(int eventTeamId, int userId)
        {
            EventTeam entity =
                _db.EventTeams
                .FirstOrDefault(x =>
                    x.EventTeamId == eventTeamId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.IsActive = false;
            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Marks Team as completed.
        /// Used when the assigned work is finished.
        /// </summary>
        public bool CompleteTeam(int eventTeamId, int userId)
        {
            EventTeam entity =
                _db.EventTeams
                .FirstOrDefault(x =>
                    x.EventTeamId == eventTeamId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.IsCompleted = true;
            entity.CompletedDate = DateTime.Now;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Reopens a completed Team.
        /// Used when work needs to continue.
        /// </summary>
        public bool ReOpenTeam(int eventTeamId, int userId)
        {
            EventTeam entity =
                _db.EventTeams
                .FirstOrDefault(x =>
                    x.EventTeamId == eventTeamId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.IsCompleted = false;
            entity.CompletedDate = null;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Checks duplicate Team Name within an Event.
        /// Used before creating or updating a Team.
        /// </summary>
        public bool IsDuplicateTeamName(int eventId, string teamName, int eventTeamId)
        {
            if (string.IsNullOrWhiteSpace(teamName))
            {
                return false;
            }

            teamName = teamName.Trim();

            return _db.EventTeams.Any(x =>
                x.IsActive &&
                x.EventId == eventId &&
                x.TeamName == teamName &&
                x.EventTeamId != eventTeamId);
        }

        /// <summary>
        /// Returns total active teams of an event.
        /// Dashboard KPI.
        /// </summary>
        public int GetTotalTeams(int eventId)
        {
            return _db.EventTeams.Count(x =>
                x.IsActive &&
                x.EventId == eventId);
        }

        /// <summary>
        /// Returns completed team count.
        /// Dashboard KPI.
        /// </summary>
        public int GetCompletedTeamCount(int eventId)
        {
            return _db.EventTeams.Count(x =>
                x.IsActive &&
                x.EventId == eventId &&
                x.IsCompleted);
        }

        /// <summary>
        /// Returns pending team count.
        /// Dashboard KPI.
        /// </summary>
        public int GetPendingTeamCount(int eventId)
        {
            return _db.EventTeams.Count(x =>
                x.IsActive &&
                x.EventId == eventId &&
                !x.IsCompleted);
        }

        /// <summary>
        /// Returns total members assigned to all teams.
        /// Dashboard KPI.
        /// </summary>
        public int GetTotalAssignedMembers(int eventId)
        {
            return _db.EventTeams
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .Sum(x => (int?)x.TotalMembers) ?? 0;
        }

        /// <summary>
        /// Returns total required members.
        /// Dashboard KPI.
        /// </summary>
        public int GetTotalRequiredMembers(int eventId)
        {
            return _db.EventTeams
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .Sum(x => (int?)x.RequiredMembers) ?? 0;
        }

        /// <summary>
        /// Returns assigned area summary.
        /// Used in Dashboard charts.
        /// </summary>
        public Dictionary<string, int> GetAssignedAreaSummary(int eventId)
        {
            return _db.EventTeams
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .GroupBy(x => x.AssignedArea)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns leader wise team count.
        /// Used in Leader Performance Report.
        /// </summary>
        public Dictionary<string, int> GetLeaderSummary(int eventId)
        {
            return _db.EventTeams
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .GroupBy(x => x.TeamLeaderName)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns priority wise summary.
        /// Dashboard Widget.
        /// </summary>
        public Dictionary<string, int> GetPrioritySummary(int eventId)
        {
            return _db.EventTeams
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .GroupBy(x => x.Priority)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns status wise summary.
        /// Dashboard Widget.
        /// </summary>
        public Dictionary<string, int> GetTeamStatusSummary(int eventId)
        {
            return _db.EventTeams
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .GroupBy(x => x.Status)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns recently created teams.
        /// Dashboard Recent Activity.
        /// </summary>
        public List<EventTeamVM> GetRecentTeams(int count)
        {
            return _db.EventTeams
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CreatedDate)
                .Take(count)
                .ToList()
                .Select(MapTeam)
                .ToList();
        }

        /// <summary>
        /// Returns latest teams for Dashboard widget.
        /// </summary>
        public List<EventTeamVM> GetDashboardTeams(int count)
        {
            return _db.EventTeams
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.EventMaster.EventDate)
                .ThenBy(x => x.TeamName)
                .Take(count)
                .ToList()
                .Select(MapTeam)
                .ToList();
        }

        /// <summary>
        /// Returns total active teams across all events.
        /// Global Dashboard KPI.
        /// </summary>
        public int GetTotalTeamRecords()
        {
            return _db.EventTeams.Count(x => x.IsActive);
        }

        /// <summary>
        /// Returns total completed teams across all events.
        /// Global Dashboard KPI.
        /// </summary>
        public int GetTotalCompletedTeamRecords()
        {
            return _db.EventTeams.Count(x =>
                x.IsActive &&
                x.IsCompleted);
        }

        /// <summary>
        /// Returns completion percentage of teams for an event.
        /// Dashboard KPI.
        /// </summary>
        public decimal GetTeamCompletionPercentage(int eventId)
        {
            int total = GetTotalTeams(eventId);

            if (total == 0)
            {
                return 0;
            }

            int completed = GetCompletedTeamCount(eventId);

            return Math.Round(
                ((decimal)completed / total) * 100,
                2);
        }

        #endregion

        #region Guest

        /// <summary>
        /// Maps EventGuest Entity to ViewModel.
        /// Used by all Guest Queries.
        /// </summary>
        private EventGuestVM MapGuest(EventGuest entity)
        {
            if (entity == null)
            {
                return null;
            }

            EventGuestVM vm = new EventGuestVM();

            vm.EventGuestId = entity.EventGuestId;
            vm.EventId = entity.EventId;

            if (entity.EventMaster != null)
            {
                vm.EventCode = entity.EventMaster.EventCode;
                vm.EventTitle = entity.EventMaster.EventTitle;
            }

            vm.GuestName = entity.GuestName;
            vm.Designation = entity.Designation;
            vm.Organization = entity.Organization;
            vm.MobileNumber = entity.MobileNumber;
            vm.Email = entity.Email;

            vm.GuestCategory = entity.GuestCategory;

            vm.InvitationStatus = entity.InvitationStatus;
            vm.ConfirmationStatus = entity.ConfirmationStatus;

            vm.ArrivalTime = entity.ArrivalTime;
            vm.DepartureTime = entity.DepartureTime;

            vm.VehicleRequired = entity.VehicleRequired;
            vm.VehicleDetails = entity.VehicleDetails;
            vm.DriverName = entity.DriverName;
            vm.DriverMobile = entity.DriverMobile;

            vm.HotelRequired = entity.HotelRequired;
            vm.HotelName = entity.HotelName;
            vm.RoomNumber = entity.RoomNumber;

            vm.SecurityRequired = entity.SecurityRequired;
            vm.SecurityLevel = entity.SecurityLevel;

            vm.StageSeatNumber = entity.StageSeatNumber;
            vm.ProtocolOfficer = entity.ProtocolOfficer;
            vm.EscortOfficer = entity.EscortOfficer;

            vm.Remarks = entity.Remarks;

            vm.IsActive = entity.IsActive;
            vm.CreatedBy = entity.CreatedBy;
            vm.CreatedDate = entity.CreatedDate;
            vm.UpdatedBy = entity.UpdatedBy;
            vm.UpdatedDate = entity.UpdatedDate;

            return vm;
        }

        /// <summary>
        /// Returns all guests of an event.
        /// Used in Guest Management Grid.
        /// </summary>
        public List<EventGuestVM> GetGuests(int eventId)
        {
            return _db.EventGuests
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .OrderBy(x => x.GuestName)
                .ToList()
                .Select(MapGuest)
                .ToList();
        }

        /// <summary>
        /// Returns a single guest.
        /// Used in Edit Guest screen.
        /// </summary>
        public EventGuestVM GetGuestById(int eventGuestId)
        {
            EventGuest entity =
                _db.EventGuests
                .FirstOrDefault(x =>
                    x.EventGuestId == eventGuestId &&
                    x.IsActive);

            return MapGuest(entity);
        }

        /// <summary>
        /// Returns confirmed guests.
        /// Dashboard Widget.
        /// </summary>
        public List<EventGuestVM> GetConfirmedGuests(int eventId)
        {
            return _db.EventGuests
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId &&
                    x.ConfirmationStatus == "Confirmed")
                .OrderBy(x => x.GuestName)
                .ToList()
                .Select(MapGuest)
                .ToList();
        }

        /// <summary>
        /// Returns guests requiring vehicle.
        /// Transport Dashboard.
        /// </summary>
        public List<EventGuestVM> GetVehicleRequiredGuests(int eventId)
        {
            return _db.EventGuests
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId &&
                    x.VehicleRequired)
                .OrderBy(x => x.ArrivalTime)
                .ToList()
                .Select(MapGuest)
                .ToList();
        }

        /// <summary>
        /// Returns guests requiring hotel.
        /// Accommodation Dashboard.
        /// </summary>
        public List<EventGuestVM> GetHotelRequiredGuests(int eventId)
        {
            return _db.EventGuests
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId &&
                    x.HotelRequired)
                .OrderBy(x => x.GuestName)
                .ToList()
                .Select(MapGuest)
                .ToList();
        }

        /// <summary>
        /// Returns guests requiring security.
        /// Security Dashboard.
        /// </summary>
        public List<EventGuestVM> GetSecurityRequiredGuests(int eventId)
        {
            return _db.EventGuests
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId &&
                    x.SecurityRequired)
                .OrderBy(x => x.GuestName)
                .ToList()
                .Select(MapGuest)
                .ToList();
        }

        /// <summary>
        /// Searches guests.
        /// Used in Guest Search.
        /// </summary>
        public List<EventGuestVM> SearchGuests(string keyword)
        {
            IQueryable<EventGuest> query =
                _db.EventGuests
                .Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                query = query.Where(x =>

                    (x.GuestName != null &&
                     x.GuestName.Contains(keyword))

                    ||

                    (x.MobileNumber != null &&
                     x.MobileNumber.Contains(keyword))

                    ||

                    (x.Organization != null &&
                     x.Organization.Contains(keyword))

                    ||

                    (x.Designation != null &&
                     x.Designation.Contains(keyword))

                    ||

                    (x.GuestCategory != null &&
                     x.GuestCategory.Contains(keyword))

                    ||

                    (x.ProtocolOfficer != null &&
                     x.ProtocolOfficer.Contains(keyword))

                    ||

                    (x.EscortOfficer != null &&
                     x.EscortOfficer.Contains(keyword)));
            }

            return query
                .OrderBy(x => x.GuestName)
                .ToList()
                .Select(MapGuest)
                .ToList();
        }

        /// <summary>
        /// Saves a new guest.
        /// Used from Create Guest screen.
        /// </summary>
        public bool SaveGuest(EventGuestVM vm, int userId)
        {
            if (vm == null)
            {
                return false;
            }

            EventGuest entity = new EventGuest();

            entity.EventId = vm.EventId;

            entity.GuestName = vm.GuestName;
            entity.Designation = vm.Designation;
            entity.Organization = vm.Organization;
            entity.MobileNumber = vm.MobileNumber;
            entity.Email = vm.Email;

            entity.GuestCategory = vm.GuestCategory;

            entity.InvitationStatus = vm.InvitationStatus;
            entity.ConfirmationStatus = vm.ConfirmationStatus;

            entity.ArrivalTime = vm.ArrivalTime;
            entity.DepartureTime = vm.DepartureTime;

            entity.VehicleRequired = vm.VehicleRequired;
            entity.VehicleDetails = vm.VehicleDetails;
            entity.DriverName = vm.DriverName;
            entity.DriverMobile = vm.DriverMobile;

            entity.HotelRequired = vm.HotelRequired;
            entity.HotelName = vm.HotelName;
            entity.RoomNumber = vm.RoomNumber;

            entity.SecurityRequired = vm.SecurityRequired;
            entity.SecurityLevel = vm.SecurityLevel;

            entity.StageSeatNumber = vm.StageSeatNumber;
            entity.ProtocolOfficer = vm.ProtocolOfficer;
            entity.EscortOfficer = vm.EscortOfficer;

            entity.Remarks = vm.Remarks;

            entity.IsActive = true;
            entity.CreatedBy = userId;
            entity.CreatedDate = DateTime.Now;

            _db.EventGuests.Add(entity);

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Updates guest information.
        /// Used from Edit Guest screen.
        /// </summary>
        public bool UpdateGuest(EventGuestVM vm, int userId)
        {
            if (vm == null)
            {
                return false;
            }

            EventGuest entity =
                _db.EventGuests
                .FirstOrDefault(x =>
                    x.EventGuestId == vm.EventGuestId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.EventId = vm.EventId;

            entity.GuestName = vm.GuestName;
            entity.Designation = vm.Designation;
            entity.Organization = vm.Organization;
            entity.MobileNumber = vm.MobileNumber;
            entity.Email = vm.Email;

            entity.GuestCategory = vm.GuestCategory;

            entity.InvitationStatus = vm.InvitationStatus;
            entity.ConfirmationStatus = vm.ConfirmationStatus;

            entity.ArrivalTime = vm.ArrivalTime;
            entity.DepartureTime = vm.DepartureTime;

            entity.VehicleRequired = vm.VehicleRequired;
            entity.VehicleDetails = vm.VehicleDetails;
            entity.DriverName = vm.DriverName;
            entity.DriverMobile = vm.DriverMobile;

            entity.HotelRequired = vm.HotelRequired;
            entity.HotelName = vm.HotelName;
            entity.RoomNumber = vm.RoomNumber;

            entity.SecurityRequired = vm.SecurityRequired;
            entity.SecurityLevel = vm.SecurityLevel;

            entity.StageSeatNumber = vm.StageSeatNumber;
            entity.ProtocolOfficer = vm.ProtocolOfficer;
            entity.EscortOfficer = vm.EscortOfficer;

            entity.Remarks = vm.Remarks;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Soft deletes a guest.
        /// Used from Guest List.
        /// </summary>
        public bool DeleteGuest(int eventGuestId, int userId)
        {
            EventGuest entity =
                _db.EventGuests
                .FirstOrDefault(x =>
                    x.EventGuestId == eventGuestId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.IsActive = false;
            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Checks duplicate guest by mobile number
        /// within the same event.
        /// </summary>
        public bool IsDuplicateGuest(int eventId, string mobileNumber, int eventGuestId)
        {
            if (string.IsNullOrWhiteSpace(mobileNumber))
            {
                return false;
            }

            mobileNumber = mobileNumber.Trim();

            return _db.EventGuests.Any(x =>
                x.IsActive &&
                x.EventId == eventId &&
                x.MobileNumber == mobileNumber &&
                x.EventGuestId != eventGuestId);
        }

        /// <summary>
        /// Marks guest as confirmed.
        /// Used after guest accepts invitation.
        /// </summary>
        public bool ConfirmGuest(int eventGuestId, int userId)
        {
            EventGuest entity =
                _db.EventGuests
                .FirstOrDefault(x =>
                    x.EventGuestId == eventGuestId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.ConfirmationStatus = "Confirmed";
            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Cancels guest confirmation.
        /// Used if guest declines or cancels attendance.
        /// </summary>
        public bool CancelGuestConfirmation(int eventGuestId, int userId)
        {
            EventGuest entity =
                _db.EventGuests
                .FirstOrDefault(x =>
                    x.EventGuestId == eventGuestId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.ConfirmationStatus = "Cancelled";
            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Returns total guests of an event.
        /// Dashboard KPI.
        /// </summary>
        public int GetTotalGuests(int eventId)
        {
            return _db.EventGuests.Count(x =>
                x.IsActive &&
                x.EventId == eventId);
        }

        /// <summary>
        /// Returns confirmed guest count.
        /// Dashboard KPI.
        /// </summary>
        public int GetConfirmedGuestCount(int eventId)
        {
            return _db.EventGuests.Count(x =>
                x.IsActive &&
                x.EventId == eventId &&
                x.ConfirmationStatus == "Confirmed");
        }

        /// <summary>
        /// Returns pending confirmation guest count.
        /// Dashboard KPI.
        /// </summary>
        public int GetPendingGuestCount(int eventId)
        {
            return _db.EventGuests.Count(x =>
                x.IsActive &&
                x.EventId == eventId &&
                x.ConfirmationStatus != "Confirmed");
        }

        /// <summary>
        /// Returns guests requiring vehicles.
        /// Dashboard KPI.
        /// </summary>
        public int GetVehicleRequiredGuestCount(int eventId)
        {
            return _db.EventGuests.Count(x =>
                x.IsActive &&
                x.EventId == eventId &&
                x.VehicleRequired);
        }

        /// <summary>
        /// Returns guests requiring hotel accommodation.
        /// Dashboard KPI.
        /// </summary>
        public int GetHotelRequiredGuestCount(int eventId)
        {
            return _db.EventGuests.Count(x =>
                x.IsActive &&
                x.EventId == eventId &&
                x.HotelRequired);
        }

        /// <summary>
        /// Returns guests requiring security.
        /// Dashboard KPI.
        /// </summary>
        public int GetSecurityRequiredGuestCount(int eventId)
        {
            return _db.EventGuests.Count(x =>
                x.IsActive &&
                x.EventId == eventId &&
                x.SecurityRequired);
        }

        /// <summary>
        /// Returns guest category summary.
        /// Used in Dashboard Charts.
        /// </summary>
        public Dictionary<string, int> GetGuestCategorySummary(int eventId)
        {
            return _db.EventGuests
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .GroupBy(x => x.GuestCategory)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns organization wise guest summary.
        /// Used in Dashboard Reports.
        /// </summary>
        public Dictionary<string, int> GetOrganizationSummary(int eventId)
        {
            return _db.EventGuests
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .GroupBy(x => x.Organization)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns security level summary.
        /// Used in Security Dashboard.
        /// </summary>
        public Dictionary<string, int> GetSecurityLevelSummary(int eventId)
        {
            return _db.EventGuests
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId &&
                    x.SecurityRequired)
                .GroupBy(x => x.SecurityLevel)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns recently added guests.
        /// Dashboard Recent Activity.
        /// </summary>
        public List<EventGuestVM> GetRecentGuests(int count)
        {
            return _db.EventGuests
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CreatedDate)
                .Take(count)
                .ToList()
                .Select(MapGuest)
                .ToList();
        }

        /// <summary>
        /// Returns latest guests for Dashboard widget.
        /// </summary>
        public List<EventGuestVM> GetDashboardGuests(int count)
        {
            return _db.EventGuests
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.EventMaster.EventDate)
                .ThenBy(x => x.GuestName)
                .Take(count)
                .ToList()
                .Select(MapGuest)
                .ToList();
        }

        /// <summary>
        /// Returns total active guest records.
        /// Global Dashboard KPI.
        /// </summary>
        public int GetTotalGuestRecords()
        {
            return _db.EventGuests.Count(x => x.IsActive);
        }

        /// <summary>
        /// Returns total confirmed guests across all events.
        /// Global Dashboard KPI.
        /// </summary>
        public int GetTotalConfirmedGuestRecords()
        {
            return _db.EventGuests.Count(x =>
                x.IsActive &&
                x.ConfirmationStatus == "Confirmed");
        }

        /// <summary>
        /// Returns confirmation percentage.
        /// Dashboard KPI.
        /// </summary>
        public decimal GetGuestConfirmationPercentage(int eventId)
        {
            int total = GetTotalGuests(eventId);

            if (total == 0)
            {
                return 0;
            }

            int confirmed = GetConfirmedGuestCount(eventId);

            return Math.Round(
                ((decimal)confirmed / total) * 100,
                2);
        }

        #endregion

        #region Arrangement

        /// <summary>
        /// Maps EventArrangement Entity to ViewModel.
        /// Used by all Arrangement Queries.
        /// </summary>
        private EventArrangementVM MapArrangement(EventArrangement entity)
        {
            if (entity == null)
            {
                return null;
            }

            EventArrangementVM vm = new EventArrangementVM();

            vm.EventArrangementId = entity.EventArrangementId;
            vm.EventId = entity.EventId;

            if (entity.EventMaster != null)
            {
                vm.EventCode = entity.EventMaster.EventCode;
                vm.EventTitle = entity.EventMaster.EventTitle;
            }

            vm.ArrangementType = entity.ArrangementType;
            vm.ArrangementName = entity.ArrangementName;

            vm.ResponsiblePerson = entity.ResponsiblePerson;
            vm.ResponsibleMobile = entity.ResponsibleMobile;

            vm.VendorName = entity.VendorName;
            vm.VendorMobile = entity.VendorMobile;

            vm.Quantity = entity.Quantity;

            vm.EstimatedCost = entity.EstimatedCost;
            vm.ActualCost = entity.ActualCost;

            vm.StartTime = entity.StartTime;
            vm.EndTime = entity.EndTime;

            vm.Status = entity.Status;
            vm.Priority = entity.Priority;

            vm.IsVerified = entity.IsVerified;
            vm.VerifiedBy = entity.VerifiedBy;
            vm.VerifiedDate = entity.VerifiedDate;

            vm.Remarks = entity.Remarks;

            vm.IsActive = entity.IsActive;
            vm.CreatedBy = entity.CreatedBy;
            vm.CreatedDate = entity.CreatedDate;
            vm.UpdatedBy = entity.UpdatedBy;
            vm.UpdatedDate = entity.UpdatedDate;

            return vm;
        }

        /// <summary>
        /// Returns all active arrangements of an event.
        /// Used in Arrangement Management Grid.
        /// </summary>
        public List<EventArrangementVM> GetArrangements(int eventId)
        {
            return _db.EventArrangements
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .OrderBy(x => x.ArrangementName)
                .ToList()
                .Select(MapArrangement)
                .ToList();
        }

        /// <summary>
        /// Returns a single arrangement.
        /// Used in Edit Arrangement screen.
        /// </summary>
        public EventArrangementVM GetArrangementById(int eventArrangementId)
        {
            EventArrangement entity =
                _db.EventArrangements
                .FirstOrDefault(x =>
                    x.EventArrangementId == eventArrangementId &&
                    x.IsActive);

            return MapArrangement(entity);
        }

        /// <summary>
        /// Returns verified arrangements.
        /// Dashboard Verification Widget.
        /// </summary>
        public List<EventArrangementVM> GetVerifiedArrangements(int eventId)
        {
            return _db.EventArrangements
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId &&
                    x.IsVerified)
                .OrderBy(x => x.ArrangementName)
                .ToList()
                .Select(MapArrangement)
                .ToList();
        }

        /// <summary>
        /// Returns pending verification arrangements.
        /// Dashboard Verification Widget.
        /// </summary>
        public List<EventArrangementVM> GetPendingArrangements(int eventId)
        {
            return _db.EventArrangements
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId &&
                    !x.IsVerified)
                .OrderBy(x => x.ArrangementName)
                .ToList()
                .Select(MapArrangement)
                .ToList();
        }

        /// <summary>
        /// Searches arrangements.
        /// Used in Arrangement Search Box.
        /// </summary>
        public List<EventArrangementVM> SearchArrangements(string keyword)
        {
            IQueryable<EventArrangement> query =
                _db.EventArrangements
                .Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                query = query.Where(x =>

                    (x.ArrangementName != null &&
                     x.ArrangementName.Contains(keyword))

                    ||

                    (x.ArrangementType != null &&
                     x.ArrangementType.Contains(keyword))

                    ||

                    (x.ResponsiblePerson != null &&
                     x.ResponsiblePerson.Contains(keyword))

                    ||

                    (x.VendorName != null &&
                     x.VendorName.Contains(keyword))

                    ||

                    (x.Status != null &&
                     x.Status.Contains(keyword))

                    ||

                    (x.Priority != null &&
                     x.Priority.Contains(keyword)));
            }

            return query
                .OrderBy(x => x.ArrangementName)
                .ToList()
                .Select(MapArrangement)
                .ToList();
        }

        /// <summary>
        /// Creates a new arrangement.
        /// Used from Create Arrangement screen.
        /// </summary>
        public bool SaveArrangement(EventArrangementVM vm, int userId)
        {
            if (vm == null)
            {
                return false;
            }

            EventArrangement entity = new EventArrangement();

            entity.EventId = vm.EventId;

            entity.ArrangementType = vm.ArrangementType;
            entity.ArrangementName = vm.ArrangementName;

            entity.ResponsiblePerson = vm.ResponsiblePerson;
            entity.ResponsibleMobile = vm.ResponsibleMobile;

            entity.VendorName = vm.VendorName;
            entity.VendorMobile = vm.VendorMobile;

            entity.Quantity = vm.Quantity;

            entity.EstimatedCost = vm.EstimatedCost;
            entity.ActualCost = vm.ActualCost;

            entity.StartTime = vm.StartTime;
            entity.EndTime = vm.EndTime;

            entity.Status = vm.Status;
            entity.Priority = vm.Priority;

            entity.IsVerified = false;
            entity.VerifiedBy = null;
            entity.VerifiedDate = null;

            entity.Remarks = vm.Remarks;

            entity.IsActive = true;
            entity.CreatedBy = userId;
            entity.CreatedDate = DateTime.Now;

            _db.EventArrangements.Add(entity);

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Updates an existing arrangement.
        /// Used from Edit Arrangement screen.
        /// </summary>
        public bool UpdateArrangement(EventArrangementVM vm, int userId)
        {
            if (vm == null)
            {
                return false;
            }

            EventArrangement entity =
                _db.EventArrangements
                .FirstOrDefault(x =>
                    x.EventArrangementId == vm.EventArrangementId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.EventId = vm.EventId;

            entity.ArrangementType = vm.ArrangementType;
            entity.ArrangementName = vm.ArrangementName;

            entity.ResponsiblePerson = vm.ResponsiblePerson;
            entity.ResponsibleMobile = vm.ResponsibleMobile;

            entity.VendorName = vm.VendorName;
            entity.VendorMobile = vm.VendorMobile;

            entity.Quantity = vm.Quantity;

            entity.EstimatedCost = vm.EstimatedCost;
            entity.ActualCost = vm.ActualCost;

            entity.StartTime = vm.StartTime;
            entity.EndTime = vm.EndTime;

            entity.Status = vm.Status;
            entity.Priority = vm.Priority;

            entity.Remarks = vm.Remarks;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Soft deletes an arrangement.
        /// Used from Arrangement List.
        /// </summary>
        public bool DeleteArrangement(int eventArrangementId, int userId)
        {
            EventArrangement entity =
                _db.EventArrangements
                .FirstOrDefault(x =>
                    x.EventArrangementId == eventArrangementId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.IsActive = false;
            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Marks an arrangement as verified.
        /// Used by Admin after physical verification.
        /// </summary>
        public bool VerifyArrangement(int eventArrangementId, int userId)
        {
            EventArrangement entity =
                _db.EventArrangements
                .FirstOrDefault(x =>
                    x.EventArrangementId == eventArrangementId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.IsVerified = true;
            entity.VerifiedBy = userId;
            entity.VerifiedDate = DateTime.Now;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Removes verification from an arrangement.
        /// Used when verification needs to be rolled back.
        /// </summary>
        public bool UnVerifyArrangement(int eventArrangementId, int userId)
        {
            EventArrangement entity =
                _db.EventArrangements
                .FirstOrDefault(x =>
                    x.EventArrangementId == eventArrangementId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.IsVerified = false;
            entity.VerifiedBy = null;
            entity.VerifiedDate = null;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Checks duplicate arrangement name within the same event.
        /// Used before creating or updating an arrangement.
        /// </summary>
        public bool IsDuplicateArrangement(int eventId, string arrangementName, int eventArrangementId)
        {
            if (string.IsNullOrWhiteSpace(arrangementName))
            {
                return false;
            }

            arrangementName = arrangementName.Trim();

            return _db.EventArrangements.Any(x =>
                x.IsActive &&
                x.EventId == eventId &&
                x.ArrangementName == arrangementName &&
                x.EventArrangementId != eventArrangementId);
        }

        /// <summary>
        /// Returns total arrangements of an event.
        /// Dashboard KPI.
        /// </summary>
        public int GetTotalArrangements(int eventId)
        {
            return _db.EventArrangements.Count(x =>
                x.IsActive &&
                x.EventId == eventId);
        }

        /// <summary>
        /// Returns verified arrangement count.
        /// Dashboard KPI.
        /// </summary>
        public int GetVerifiedArrangementCount(int eventId)
        {
            return _db.EventArrangements.Count(x =>
                x.IsActive &&
                x.EventId == eventId &&
                x.IsVerified);
        }

        /// <summary>
        /// Returns pending verification count.
        /// Dashboard KPI.
        /// </summary>
        public int GetPendingArrangementCount(int eventId)
        {
            return _db.EventArrangements.Count(x =>
                x.IsActive &&
                x.EventId == eventId &&
                !x.IsVerified);
        }

        /// <summary>
        /// Returns total estimated arrangement cost.
        /// Dashboard KPI.
        /// </summary>
        public decimal GetTotalEstimatedArrangementCost(int eventId)
        {
            return _db.EventArrangements
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .Sum(x => (decimal?)x.EstimatedCost) ?? 0;
        }

        /// <summary>
        /// Returns total actual arrangement cost.
        /// Dashboard KPI.
        /// </summary>
        public decimal GetTotalActualArrangementCost(int eventId)
        {
            return _db.EventArrangements
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .Sum(x => (decimal?)x.ActualCost) ?? 0;
        }

        /// <summary>
        /// Returns Arrangement Type summary.
        /// Dashboard Chart.
        /// </summary>
        public Dictionary<string, int> GetArrangementTypeSummary(int eventId)
        {
            return _db.EventArrangements
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .GroupBy(x => x.ArrangementType)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns Vendor wise arrangement summary.
        /// Vendor Performance Report.
        /// </summary>
        public Dictionary<string, int> GetVendorSummary(int eventId)
        {
            return _db.EventArrangements
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .GroupBy(x => x.VendorName)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns Responsible Person summary.
        /// Dashboard Widget.
        /// </summary>
        public Dictionary<string, int> GetResponsiblePersonSummary(int eventId)
        {
            return _db.EventArrangements
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .GroupBy(x => x.ResponsiblePerson)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns Priority summary.
        /// Dashboard Chart.
        /// </summary>
        public Dictionary<string, int> GetArrangementPrioritySummary(int eventId)
        {
            return _db.EventArrangements
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .GroupBy(x => x.Priority)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns Status summary.
        /// Dashboard Chart.
        /// </summary>
        public Dictionary<string, int> GetArrangementStatusSummary(int eventId)
        {
            return _db.EventArrangements
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .GroupBy(x => x.Status)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns latest arrangements.
        /// Dashboard Recent Activity.
        /// </summary>
        public List<EventArrangementVM> GetRecentArrangements(int count)
        {
            return _db.EventArrangements
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CreatedDate)
                .Take(count)
                .ToList()
                .Select(MapArrangement)
                .ToList();
        }

        /// <summary>
        /// Returns Dashboard Arrangement Widget.
        /// </summary>
        public List<EventArrangementVM> GetDashboardArrangements(int count)
        {
            return _db.EventArrangements
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.EventMaster.EventDate)
                .ThenBy(x => x.ArrangementName)
                .Take(count)
                .ToList()
                .Select(MapArrangement)
                .ToList();
        }

        /// <summary>
        /// Returns total arrangement records.
        /// Global Dashboard KPI.
        /// </summary>
        public int GetTotalArrangementRecords()
        {
            return _db.EventArrangements.Count(x => x.IsActive);
        }

        /// <summary>
        /// Returns total verified arrangement records.
        /// Global Dashboard KPI.
        /// </summary>
        public int GetTotalVerifiedArrangementRecords()
        {
            return _db.EventArrangements.Count(x =>
                x.IsActive &&
                x.IsVerified);
        }

        /// <summary>
        /// Returns arrangement verification percentage.
        /// Dashboard KPI.
        /// </summary>
        public decimal GetArrangementVerificationPercentage(int eventId)
        {
            int total = GetTotalArrangements(eventId);

            if (total == 0)
            {
                return 0;
            }

            int verified = GetVerifiedArrangementCount(eventId);

            return Math.Round(
                ((decimal)verified / total) * 100,
                2);
        }

        #endregion

        #region Event Expense

        /// <summary>
        /// Maps EventExpense Entity to ViewModel.
        /// Used by all Expense queries to avoid duplicate mapping code.
        /// </summary>
        private EventExpenseVM MapExpense(EventExpense entity)
        {
            if (entity == null)
            {
                return null;
            }

            EventExpenseVM vm = new EventExpenseVM();

            vm.EventExpenseId = entity.EventExpenseId;

            vm.EventId = entity.EventId;

            if (entity.EventMaster != null)
            {
                vm.EventTitle = entity.EventMaster.EventTitle;
            }

            vm.ExpenseCategory = entity.ExpenseCategory;
            vm.ExpenseHead = entity.ExpenseHead;

            vm.VendorName = entity.VendorName;
            vm.VendorMobile = entity.VendorMobile;

            vm.BillNumber = entity.BillNumber;
            vm.BillDate = entity.BillDate;

            vm.Quantity = entity.Quantity;
            vm.Unit = entity.Unit;

            vm.Rate = entity.Rate;
            vm.Amount = entity.Amount;

            vm.PaymentMode = entity.PaymentMode;
            vm.PaymentStatus = entity.PaymentStatus;

            vm.ExpenseStatus = entity.ExpenseStatus;

            switch (entity.ExpenseStatus)
            {
                case "Approved":
                    vm.ExpenseStatusColor = "success";
                    break;

                case "Rejected":
                    vm.ExpenseStatusColor = "danger";
                    break;

                case "Verified":
                    vm.ExpenseStatusColor = "primary";
                    break;

                case "Submitted":
                    vm.ExpenseStatusColor = "info";
                    break;

                case "Cancelled":
                    vm.ExpenseStatusColor = "dark";
                    break;

                default:
                    vm.ExpenseStatusColor = "secondary";
                    break;
            }

            vm.PaidTo = entity.PaidTo;
            vm.PaidBy = entity.PaidBy;

            vm.TransactionReference = entity.TransactionReference;

            vm.ExpenseDate = entity.ExpenseDate;

            vm.ApprovedBy = entity.ApprovedBy;

            vm.VerifiedBy = entity.VerifiedBy;
            vm.VerifiedDate = entity.VerifiedDate;

            vm.Remarks = entity.Remarks;

            vm.IsActive = entity.IsActive;

            vm.CreatedBy = entity.CreatedBy;
            vm.CreatedDate = entity.CreatedDate;

            vm.UpdatedBy = entity.UpdatedBy;
            vm.UpdatedDate = entity.UpdatedDate;

            return vm;
        }
        /// <summary>
        /// Returns all active expenses.
        /// Used by Expense Grid.
        /// </summary>
        public List<EventExpenseVM> GetExpenses()
        {
            return _db.EventExpenses
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.ExpenseDate)
                .ThenByDescending(x => x.CreatedDate)
                .ToList()
                .Select(MapExpense)
                .ToList();
        }

        /// <summary>
        /// Returns all expenses of an Event.
        /// Used in Event Expense screen.
        /// </summary>
        public List<EventExpenseVM> GetExpenses(int eventId)
        {
            return _db.EventExpenses
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .OrderByDescending(x => x.ExpenseDate)
                .ThenByDescending(x => x.CreatedDate)
                .ToList()
                .Select(MapExpense)
                .ToList();
        }

        /// <summary>
        /// Returns single expense information.
        /// Used by Edit Expense screen.
        /// </summary>
        public EventExpenseVM GetExpenseById(int eventExpenseId)
        {
            EventExpense entity =
                _db.EventExpenses
                .FirstOrDefault(x =>
                    x.EventExpenseId == eventExpenseId &&
                    x.IsActive);

            return MapExpense(entity);
        }

        /// <summary>
        /// Returns expenses by Category.
        /// Example:
        /// Food
        /// Fuel
        /// Stage
        /// Decoration
        /// </summary>
        public List<EventExpenseVM> GetExpensesByCategory(string expenseCategory)
        {
            return _db.EventExpenses
                .Where(x =>
                    x.IsActive &&
                    x.ExpenseCategory == expenseCategory)
                .OrderByDescending(x => x.ExpenseDate)
                .ToList()
                .Select(MapExpense)
                .ToList();
        }

        /// <summary>
        /// Returns expenses by Vendor.
        /// Used in Vendor Report.
        /// </summary>
        public List<EventExpenseVM> GetExpensesByVendor(string vendorName)
        {
            return _db.EventExpenses
                .Where(x =>
                    x.IsActive &&
                    x.VendorName == vendorName)
                .OrderByDescending(x => x.ExpenseDate)
                .ToList()
                .Select(MapExpense)
                .ToList();
        }

        /// <summary>
        /// Returns expenses by Expense Status.
        /// Example:
        /// Draft
        /// Submitted
        /// Verified
        /// Approved
        /// Rejected
        /// </summary>
        public List<EventExpenseVM> GetExpensesByStatus(string expenseStatus)
        {
            return _db.EventExpenses
                .Where(x =>
                    x.IsActive &&
                    x.ExpenseStatus == expenseStatus)
                .OrderByDescending(x => x.ExpenseDate)
                .ToList()
                .Select(MapExpense)
                .ToList();
        }

        /// <summary>
        /// Returns expenses by Payment Status.
        /// Example:
        /// Pending
        /// Paid
        /// Partial
        /// Cancelled
        /// </summary>
        public List<EventExpenseVM> GetExpensesByPaymentStatus(string paymentStatus)
        {
            return _db.EventExpenses
                .Where(x =>
                    x.IsActive &&
                    x.PaymentStatus == paymentStatus)
                .OrderByDescending(x => x.ExpenseDate)
                .ToList()
                .Select(MapExpense)
                .ToList();
        }

        /// <summary>
        /// Searches Expense.
        /// Used in Expense Search Box.
        /// </summary>
        public List<EventExpenseVM> SearchExpenses(string keyword)
        {
            IQueryable<EventExpense> query =
                _db.EventExpenses
                .Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                query = query.Where(x =>

                    (x.ExpenseCategory != null &&
                     x.ExpenseCategory.Contains(keyword))

                    ||

                    (x.ExpenseHead != null &&
                     x.ExpenseHead.Contains(keyword))

                    ||

                    (x.VendorName != null &&
                     x.VendorName.Contains(keyword))

                    ||

                    (x.BillNumber != null &&
                     x.BillNumber.Contains(keyword))

                    ||

                    (x.PaymentMode != null &&
                     x.PaymentMode.Contains(keyword))

                    ||

                    (x.PaymentStatus != null &&
                     x.PaymentStatus.Contains(keyword))

                    ||

                    (x.ExpenseStatus != null &&
                     x.ExpenseStatus.Contains(keyword))

                    ||

                    (x.TransactionReference != null &&
                     x.TransactionReference.Contains(keyword))

                    ||

                    (x.PaidTo != null &&
                     x.PaidTo.Contains(keyword))

                    ||

                    (x.PaidBy != null &&
                     x.PaidBy.Contains(keyword))

                    ||

                    (x.Remarks != null &&
                     x.Remarks.Contains(keyword)));
            }

            return query
                .OrderByDescending(x => x.ExpenseDate)
                .ThenByDescending(x => x.CreatedDate)
                .ToList()
                .Select(MapExpense)
                .ToList();
        }

        /// <summary>
        /// Creates a new Expense.
        /// Used from Create Expense screen.
        /// </summary>
        public bool SaveExpense(EventExpenseVM vm, int userId)
        {
            if (vm == null)
            {
                return false;
            }

            EventExpense entity = new EventExpense();

            entity.EventId = vm.EventId;

            entity.ExpenseCategory = vm.ExpenseCategory;
            entity.ExpenseHead = vm.ExpenseHead;

            entity.VendorName = vm.VendorName;
            entity.VendorMobile = vm.VendorMobile;

            entity.BillNumber = vm.BillNumber;
            entity.BillDate = vm.BillDate;

            entity.Quantity = vm.Quantity;
            entity.Unit = vm.Unit;

            entity.Rate = vm.Rate;
            entity.Amount = (vm.Quantity ?? 0) * vm.Rate;

            entity.PaymentMode = vm.PaymentMode;
            entity.PaymentStatus = string.IsNullOrWhiteSpace(vm.PaymentStatus)
                ? "Pending"
                : vm.PaymentStatus;

            entity.ExpenseStatus = string.IsNullOrWhiteSpace(vm.ExpenseStatus)
                ? "Draft"
                : vm.ExpenseStatus;

            entity.PaidTo = vm.PaidTo;
            entity.PaidBy = vm.PaidBy;

            entity.TransactionReference = vm.TransactionReference;

            entity.ExpenseDate = vm.ExpenseDate;

            entity.ApprovedBy = vm.ApprovedBy;

            entity.VerifiedBy = null;
            entity.VerifiedDate = null;

            entity.Remarks = vm.Remarks;

            entity.IsActive = true;

            entity.CreatedBy = userId;
            entity.CreatedDate = DateTime.Now;

            _db.EventExpenses.Add(entity);

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Updates Expense.
        /// Used from Edit Expense screen.
        /// </summary>
        public bool UpdateExpense(EventExpenseVM vm, int userId)
        {
            if (vm == null)
            {
                return false;
            }

            EventExpense entity =
                _db.EventExpenses
                .FirstOrDefault(x =>
                    x.EventExpenseId == vm.EventExpenseId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.EventId = vm.EventId;

            entity.ExpenseCategory = vm.ExpenseCategory;
            entity.ExpenseHead = vm.ExpenseHead;

            entity.VendorName = vm.VendorName;
            entity.VendorMobile = vm.VendorMobile;

            entity.BillNumber = vm.BillNumber;
            entity.BillDate = vm.BillDate;

            entity.Quantity = vm.Quantity;
            entity.Unit = vm.Unit;

            entity.Rate = vm.Rate;
            entity.Amount = (vm.Quantity ?? 0) * vm.Rate;

            entity.PaymentMode = vm.PaymentMode;
            entity.PaymentStatus = vm.PaymentStatus;

            entity.ExpenseStatus = vm.ExpenseStatus;

            entity.PaidTo = vm.PaidTo;
            entity.PaidBy = vm.PaidBy;

            entity.TransactionReference = vm.TransactionReference;

            entity.ExpenseDate = vm.ExpenseDate;

            entity.ApprovedBy = vm.ApprovedBy;

            entity.Remarks = vm.Remarks;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Soft deletes Expense.
        /// Used from Expense Grid.
        /// </summary>
        public bool DeleteExpense(int eventExpenseId, int userId)
        {
            EventExpense entity =
                _db.EventExpenses
                .FirstOrDefault(x =>
                    x.EventExpenseId == eventExpenseId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.IsActive = false;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Marks an Expense as Verified.
        /// Used by Finance/Admin.
        /// </summary>
        public bool VerifyExpense(int eventExpenseId, int userId)
        {
            EventExpense entity =
                _db.EventExpenses
                .FirstOrDefault(x =>
                    x.EventExpenseId == eventExpenseId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.VerifiedBy = userId;
            entity.VerifiedDate = DateTime.Now;
            entity.ExpenseStatus = "Verified";

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Approves an Expense.
        /// Used after verification.
        /// </summary>
        public bool ApproveExpense(
            int eventExpenseId,
            string approvedBy,
            int userId)
        {
            EventExpense entity =
                _db.EventExpenses
                .FirstOrDefault(x =>
                    x.EventExpenseId == eventExpenseId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.ApprovedBy = approvedBy;
            entity.ExpenseStatus = "Approved";

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Rejects an Expense.
        /// </summary>
        public bool RejectExpense(
            int eventExpenseId,
            string remarks,
            int userId)
        {
            EventExpense entity =
                _db.EventExpenses
                .FirstOrDefault(x =>
                    x.EventExpenseId == eventExpenseId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.ExpenseStatus = "Rejected";
            entity.Remarks = remarks;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Updates payment status.
        /// Example:
        /// Pending
        /// Paid
        /// Partial
        /// Cancelled
        /// </summary>
        public bool UpdatePaymentStatus(
            int eventExpenseId,
            string paymentStatus,
            string transactionReference,
            int userId)
        {
            EventExpense entity =
                _db.EventExpenses
                .FirstOrDefault(x =>
                    x.EventExpenseId == eventExpenseId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.PaymentStatus = paymentStatus;
            entity.TransactionReference = transactionReference;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Returns total expense of an Event.
        /// Dashboard KPI.
        /// </summary>
        public decimal GetTotalExpense(int eventId)
        {
            return _db.EventExpenses
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .Sum(x => (decimal?)x.Amount) ?? 0;
        }

        /// <summary>
        /// Returns total paid expense.
        /// Dashboard KPI.
        /// </summary>
        public decimal GetPaidExpense(int eventId)
        {
            return _db.EventExpenses
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId &&
                    x.PaymentStatus == "Paid")
                .Sum(x => (decimal?)x.Amount) ?? 0;
        }

        /// <summary>
        /// Returns total pending expense.
        /// Dashboard KPI.
        /// </summary>
        public decimal GetPendingExpense(int eventId)
        {
            return _db.EventExpenses
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId &&
                    x.PaymentStatus != "Paid")
                .Sum(x => (decimal?)x.Amount) ?? 0;
        }

        /// <summary>
        /// Returns today's expense.
        /// Dashboard KPI.
        /// </summary>
        public decimal GetTodayExpense()
        {
            DateTime today = DateTime.Today;

            return _db.EventExpenses
                .Where(x =>
                    x.IsActive &&
                    x.ExpenseDate.Year == today.Year &&
                    x.ExpenseDate.Month == today.Month &&
                    x.ExpenseDate.Day == today.Day)
                .Sum(x => (decimal?)x.Amount) ?? 0;
        }

        /// <summary>
        /// Returns current month's expense.
        /// Dashboard KPI.
        /// </summary>
        public decimal GetMonthExpense()
        {
            DateTime today = DateTime.Today;

            return _db.EventExpenses
                .Where(x =>
                    x.IsActive &&
                    x.ExpenseDate.Year == today.Year &&
                    x.ExpenseDate.Month == today.Month)
                .Sum(x => (decimal?)x.Amount) ?? 0;
        }
        #endregion

        #region Event Media

        /// <summary>
        /// Maps EventMedia Entity to EventMediaVM.
        /// Used by all Media queries.
        /// </summary>
        private EventMediaVM MapMedia(EventMedia entity)
        {
            if (entity == null)
            {
                return null;
            }

            EventMediaVM vm = new EventMediaVM();

            vm.EventMediaId = entity.EventMediaId;

            vm.EventId = entity.EventId;

            if (entity.EventMaster != null)
            {
                vm.EventTitle = entity.EventMaster.EventTitle;
            }

            vm.MediaType = entity.MediaType;
            vm.MediaCategory = entity.MediaCategory;

            vm.FileName = entity.FileName;
            vm.OriginalFileName = entity.OriginalFileName;

            vm.FilePath = entity.FilePath;
            vm.FileExtension = entity.FileExtension;

            vm.FileSize = entity.FileSize;

            if (entity.FileSize.HasValue)
            {
                long bytes = entity.FileSize.Value;

                if (bytes >= 1024 * 1024)
                {
                    vm.FileSizeText = Math.Round((decimal)bytes / (1024 * 1024), 2) + " MB";
                }
                else if (bytes >= 1024)
                {
                    vm.FileSizeText = Math.Round((decimal)bytes / 1024, 2) + " KB";
                }
                else
                {
                    vm.FileSizeText = bytes + " Bytes";
                }
            }

            vm.ThumbnailPath = entity.ThumbnailPath;

            vm.Caption = entity.Caption;
            vm.Description = entity.Description;

            vm.UploadedBy = entity.UploadedBy;
            vm.UploadedDate = entity.UploadedDate;

            vm.IsPrimary = entity.IsPrimary;
            vm.DisplayOrder = entity.DisplayOrder;

            vm.MediaStatus = entity.MediaStatus;

            switch (entity.MediaStatus)
            {
                case "Approved":
                    vm.MediaStatusColor = "success";
                    break;

                case "Rejected":
                    vm.MediaStatusColor = "danger";
                    break;

                case "Verified":
                    vm.MediaStatusColor = "primary";
                    break;

                case "Submitted":
                    vm.MediaStatusColor = "info";
                    break;

                case "Published":
                    vm.MediaStatusColor = "warning";
                    break;

                default:
                    vm.MediaStatusColor = "secondary";
                    break;
            }

            vm.VerifiedBy = entity.VerifiedBy;
            vm.VerifiedDate = entity.VerifiedDate;

            vm.IsActive = entity.IsActive;

            vm.CreatedBy = entity.CreatedBy;
            vm.CreatedDate = entity.CreatedDate;

            vm.UpdatedBy = entity.UpdatedBy;
            vm.UpdatedDate = entity.UpdatedDate;

            if (!string.IsNullOrWhiteSpace(entity.FileExtension))
            {
                string extension = entity.FileExtension.ToLower();

                vm.IsImage =
                    extension == ".jpg" ||
                    extension == ".jpeg" ||
                    extension == ".png" ||
                    extension == ".gif" ||
                    extension == ".bmp" ||
                    extension == ".webp";

                vm.IsVideo =
                    extension == ".mp4" ||
                    extension == ".avi" ||
                    extension == ".mov" ||
                    extension == ".wmv" ||
                    extension == ".mkv";

                vm.IsDocument =
                    extension == ".pdf" ||
                    extension == ".doc" ||
                    extension == ".docx" ||
                    extension == ".xls" ||
                    extension == ".xlsx" ||
                    extension == ".ppt" ||
                    extension == ".pptx";

                vm.IsAudio =
                    extension == ".mp3" ||
                    extension == ".wav" ||
                    extension == ".aac";
            }

            if (vm.IsImage)
            {
                vm.MediaIcon = "fa-solid fa-image";
            }
            else if (vm.IsVideo)
            {
                vm.MediaIcon = "fa-solid fa-video";
            }
            else if (vm.IsDocument)
            {
                vm.MediaIcon = "fa-solid fa-file";
            }
            else if (vm.IsAudio)
            {
                vm.MediaIcon = "fa-solid fa-music";
            }
            else
            {
                vm.MediaIcon = "fa-solid fa-file";
            }

            return vm;
        }

        /// <summary>
        /// Returns all active media.
        /// Used by Media Grid.
        /// </summary>
        public List<EventMediaVM> GetMedia()
        {
            return _db.EventMedias
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .ThenByDescending(x => x.UploadedDate)
                .ToList()
                .Select(MapMedia)
                .ToList();
        }

        /// <summary>
        /// Returns media of a specific event.
        /// </summary>
        public List<EventMediaVM> GetMedia(int eventId)
        {
            return _db.EventMedias
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .OrderBy(x => x.DisplayOrder)
                .ThenByDescending(x => x.UploadedDate)
                .ToList()
                .Select(MapMedia)
                .ToList();
        }

        /// <summary>
        /// Returns media by Id.
        /// </summary>
        public EventMediaVM GetMediaById(int eventMediaId)
        {
            EventMedia entity = _db.EventMedias
                .AsNoTracking()
                .FirstOrDefault(x =>
                    x.EventMediaId == eventMediaId &&
                    x.IsActive);

            return MapMedia(entity);
        }

        /// <summary>
        /// Returns only Images.
        /// </summary>
        public List<EventMediaVM> GetPhotos()
        {
            return _db.EventMedias
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.MediaType == "Photo")
                .OrderBy(x => x.DisplayOrder)
                .ThenByDescending(x => x.EventMediaId)
                .ToList()
                .Select(MapMedia)
                .ToList();
        }

        /// <summary>
        /// Returns only Videos.
        /// </summary>
        public List<EventMediaVM> GetVideos()
        {
            return _db.EventMedias
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.MediaType == "Video")
                .OrderBy(x => x.DisplayOrder)
                .ThenByDescending(x => x.UploadedDate)
                .ToList()
                .Select(MapMedia)
                .ToList();
        }

        /// <summary>
        /// Returns only Documents.
        /// </summary>
        public List<EventMediaVM> GetDocuments()
        {
            return _db.EventMedias
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.MediaType == "Document")
                .OrderBy(x => x.DisplayOrder)
                .ThenByDescending(x => x.UploadedDate)
                .ToList()
                .Select(MapMedia)
                .ToList();
        }

        /// <summary>
        /// Returns media by category.
        /// </summary>
        public List<EventMediaVM> GetMediaByCategory(string category)
        {
            return _db.EventMedias
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.MediaCategory == category)
                .OrderBy(x => x.DisplayOrder)
                .ThenByDescending(x => x.UploadedDate)
                .ToList()
                .Select(MapMedia)
                .ToList();
        }

        /// <summary>
        /// Returns media by status.
        /// </summary>
        public List<EventMediaVM> GetMediaByStatus(string mediaStatus)
        {
            return _db.EventMedias
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.MediaStatus == mediaStatus)
                .OrderBy(x => x.DisplayOrder)
                .ThenByDescending(x => x.UploadedDate)
                .ToList()
                .Select(MapMedia)
                .ToList();
        }

        /// <summary>
        /// Returns primary media of an event.
        /// </summary>
        public EventMediaVM GetPrimaryMedia(int eventId)
        {
            EventMedia entity = _db.EventMedias
                .AsNoTracking()
                .FirstOrDefault(x =>
                    x.EventId == eventId &&
                    x.IsPrimary &&
                    x.IsActive);

            return MapMedia(entity);
        }

        /// <summary>
        /// Returns Audio files.
        /// </summary>
        public List<EventMediaVM> GetAudios()
        {
            return _db.EventMedias
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.MediaType == "Audio")
                .OrderBy(x => x.DisplayOrder)
                .ThenByDescending(x => x.EventMediaId)
                .ToList()
                .Select(MapMedia)
                .ToList();
        }

        /// <summary>
        /// Returns Media by Type.
        /// </summary>
        public List<EventMediaVM> GetMediaByType(string mediaType)
        {
            return _db.EventMedias
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.MediaType == mediaType)
                .OrderBy(x => x.DisplayOrder)
                .ThenByDescending(x => x.EventMediaId)
                .ToList()
                .Select(MapMedia)
                .ToList();
        }

        /// <summary>
        /// Returns recent uploaded media.
        /// </summary>
        public List<EventMediaVM> GetRecentMedia(int count)
        {
            return _db.EventMedias
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.UploadedDate)
                .Take(count)
                .ToList()
                .Select(MapMedia)
                .ToList();
        }

        /// <summary>
        /// Returns dashboard media.
        /// </summary>
        public List<EventMediaVM> GetDashboardMedia(int count)
        {
            return _db.EventMedias
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.MediaStatus == "Published")
                .OrderByDescending(x => x.UploadedDate)
                .Take(count)
                .ToList()
                .Select(MapMedia)
                .ToList();
        }

        /// <summary>
        /// Searches media.
        /// </summary>
        public List<EventMediaVM> SearchMedia(string keyword)
        {
            IQueryable<EventMedia> query =
                _db.EventMedias
                .AsNoTracking()
                .Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                query = query.Where(x =>

                    (x.Caption != null &&
                     x.Caption.Contains(keyword))

                    ||

                    (x.Description != null &&
                     x.Description.Contains(keyword))

                    ||

                    (x.MediaCategory != null &&
                     x.MediaCategory.Contains(keyword))

                    ||

                    (x.MediaType != null &&
                     x.MediaType.Contains(keyword))

                    ||

                    (x.OriginalFileName != null &&
                     x.OriginalFileName.Contains(keyword))

                    ||

                    (x.UploadedBy != null &&
                     x.UploadedBy.Contains(keyword))

                    ||

                    (x.MediaStatus != null &&
                     x.MediaStatus.Contains(keyword))

                    ||

                    (x.EventMaster.EventTitle != null &&
                     x.EventMaster.EventTitle.Contains(keyword)));
            }

            return query
                .OrderBy(x => x.DisplayOrder)
                .ThenByDescending(x => x.EventMediaId)
                .ToList()
                .Select(MapMedia)
                .ToList();
        }

        /// <summary>
        /// Saves new media.
        /// </summary>
        public bool SaveMedia(EventMediaVM vm, int userId)
        {
            if (vm == null)
            {
                return false;
            }

            EventMedia entity = new EventMedia();

            entity.EventId = vm.EventId;

            entity.MediaType = vm.MediaType;
            entity.MediaCategory = vm.MediaCategory;

            entity.FileName = vm.FileName;
            entity.OriginalFileName = vm.OriginalFileName;

            entity.FilePath = vm.FilePath;
            entity.FileExtension = vm.FileExtension;
            entity.FileSize = vm.FileSize;

            entity.ThumbnailPath = vm.ThumbnailPath;

            entity.Caption = vm.Caption;
            entity.Description = vm.Description;

            entity.UploadedBy = vm.UploadedBy;
            entity.UploadedDate = DateTime.Now;

            entity.IsPrimary = vm.IsPrimary;
            entity.DisplayOrder = vm.DisplayOrder;

            entity.MediaStatus =
                string.IsNullOrWhiteSpace(vm.MediaStatus)
                ? "Draft"
                : vm.MediaStatus;

            entity.VerifiedBy = null;
            entity.VerifiedDate = null;

            entity.IsActive = true;

            entity.CreatedBy = userId;
            entity.CreatedDate = DateTime.Now;

            _db.EventMedias.Add(entity);

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Updates media.
        /// </summary>
        public bool UpdateMedia(EventMediaVM vm, int userId)
        {
            if (vm == null)
            {
                return false;
            }

            EventMedia entity =
                _db.EventMedias
                .FirstOrDefault(x =>
                    x.EventMediaId == vm.EventMediaId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.EventId = vm.EventId;

            entity.MediaType = vm.MediaType;
            entity.MediaCategory = vm.MediaCategory;

            entity.FileName = vm.FileName;
            entity.OriginalFileName = vm.OriginalFileName;

            entity.FilePath = vm.FilePath;
            entity.FileExtension = vm.FileExtension;
            entity.FileSize = vm.FileSize;

            entity.ThumbnailPath = vm.ThumbnailPath;

            entity.Caption = vm.Caption;
            entity.Description = vm.Description;

            entity.IsPrimary = vm.IsPrimary;
            entity.DisplayOrder = vm.DisplayOrder;

            entity.MediaStatus = vm.MediaStatus;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Soft deletes media.
        /// </summary>
        public bool DeleteMedia(int eventMediaId, int userId)
        {
            EventMedia entity =
                _db.EventMedias
                .FirstOrDefault(x =>
                    x.EventMediaId == eventMediaId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.IsActive = false;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }


        /// <summary>
        /// Verifies media.
        /// </summary>
        public bool VerifyMedia(int eventMediaId, int userId)
        {
            EventMedia entity =
                _db.EventMedias
                .FirstOrDefault(x =>
                    x.EventMediaId == eventMediaId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.VerifiedBy = userId;
            entity.VerifiedDate = DateTime.Now;
            entity.MediaStatus = "Verified";

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Approves media.
        /// </summary>
        public bool ApproveMedia(int eventMediaId, int userId)
        {
            EventMedia entity =
                _db.EventMedias
                .FirstOrDefault(x =>
                    x.EventMediaId == eventMediaId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.MediaStatus = "Approved";

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Rejects media.
        /// </summary>
        public bool RejectMedia(
            int eventMediaId,
            string remarks,
            int userId)
        {
            EventMedia entity =
                _db.EventMedias
                .FirstOrDefault(x =>
                    x.EventMediaId == eventMediaId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.MediaStatus = "Rejected";

            if (!string.IsNullOrWhiteSpace(remarks))
            {
                if (string.IsNullOrWhiteSpace(entity.Description))
                {
                    entity.Description = remarks;
                }
                else
                {
                    entity.Description += Environment.NewLine + remarks;
                }
            }

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Publishes media.
        /// </summary>
        public bool PublishMedia(int eventMediaId, int userId)
        {
            EventMedia entity =
                _db.EventMedias
                .FirstOrDefault(x =>
                    x.EventMediaId == eventMediaId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.MediaStatus = "Published";

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Unpublishes media.
        /// </summary>
        public bool UnPublishMedia(int eventMediaId, int userId)
        {
            EventMedia entity =
                _db.EventMedias
                .FirstOrDefault(x =>
                    x.EventMediaId == eventMediaId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.MediaStatus = "Approved";

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Checks duplicate stored file name.
        /// </summary>
        public bool IsDuplicateFileName(string fileName, int eventMediaId = 0)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            fileName = fileName.Trim();

            return _db.EventMedias.Any(x =>
                x.IsActive &&
                x.FileName == fileName &&
                x.EventMediaId != eventMediaId);
        }

        /// <summary>
        /// Checks duplicate original file name.
        /// </summary>
        public bool IsDuplicateOriginalFileName(string originalFileName, int eventMediaId = 0)
        {
            if (string.IsNullOrWhiteSpace(originalFileName))
            {
                return false;
            }

            originalFileName = originalFileName.Trim();

            return _db.EventMedias.Any(x =>
                x.IsActive &&
                x.OriginalFileName == originalFileName &&
                x.EventMediaId != eventMediaId);
        }

        /// <summary>
        /// Returns total media count.
        /// </summary>
        public int GetTotalMedia()
        {
            return _db.EventMedias.Count(x => x.IsActive);
        }

        /// <summary>
        /// Returns total photos.
        /// </summary>
        public int GetTotalPhotos()
        {
            return _db.EventMedias.Count(x =>
                x.IsActive &&
                x.MediaType == "Photo");
        }

        /// <summary>
        /// Returns total videos.
        /// </summary>
        public int GetTotalVideos()
        {
            return _db.EventMedias.Count(x =>
                x.IsActive &&
                x.MediaType == "Video");
        }

        /// <summary>
        /// Returns total documents.
        /// </summary>
        public int GetTotalDocuments()
        {
            return _db.EventMedias.Count(x =>
                x.IsActive &&
                x.MediaType == "Document");
        }

        /// <summary>
        /// Returns total audio files.
        /// </summary>
        public int GetTotalAudios()
        {
            return _db.EventMedias.Count(x =>
                x.IsActive &&
                x.MediaType == "Audio");
        }

        /// <summary>
        /// Returns total published media.
        /// </summary>
        public int GetTotalPublishedMedia()
        {
            return _db.EventMedias.Count(x =>
                x.IsActive &&
                x.MediaStatus == "Published");
        }

        /// <summary>
        /// Returns total approved media.
        /// </summary>
        public int GetTotalApprovedMedia()
        {
            return _db.EventMedias.Count(x =>
                x.IsActive &&
                x.MediaStatus == "Approved");
        }

        /// <summary>
        /// Returns total verified media.
        /// </summary>
        public int GetTotalVerifiedMedia()
        {
            return _db.EventMedias.Count(x =>
                x.IsActive &&
                x.MediaStatus == "Verified");
        }

        /// <summary>
        /// Returns total pending media.
        /// </summary>
        public int GetTotalPendingMedia()
        {
            return _db.EventMedias.Count(x =>
                x.IsActive &&
                (
                    x.MediaStatus == "Draft" ||
                    x.MediaStatus == "Submitted"
                ));
        }

        /// <summary>
        /// Returns total rejected media.
        /// </summary>
        public int GetTotalRejectedMedia()
        {
            return _db.EventMedias.Count(x =>
                x.IsActive &&
                x.MediaStatus == "Rejected");
        }

        /// <summary>
        /// Returns media count of an event.
        /// </summary>
        public int GetMediaCountByEvent(int eventId)
        {
            return _db.EventMedias.Count(x =>
                x.IsActive &&
                x.EventId == eventId);
        }

        /// <summary>
        /// Returns photo count of an event.
        /// </summary>
        public int GetPhotoCountByEvent(int eventId)
        {
            return _db.EventMedias.Count(x =>
                x.IsActive &&
                x.EventId == eventId &&
                x.MediaType == "Photo");
        }

        /// <summary>
        /// Returns video count of an event.
        /// </summary>
        public int GetVideoCountByEvent(int eventId)
        {
            return _db.EventMedias.Count(x =>
                x.IsActive &&
                x.EventId == eventId &&
                x.MediaType == "Video");
        }

        /// <summary>
        /// Returns document count of an event.
        /// </summary>
        public int GetDocumentCountByEvent(int eventId)
        {
            return _db.EventMedias.Count(x =>
                x.IsActive &&
                x.EventId == eventId &&
                x.MediaType == "Document");
        }

        /// <summary>
        /// Returns audio count of an event.
        /// </summary>
        public int GetAudioCountByEvent(int eventId)
        {
            return _db.EventMedias.Count(x =>
                x.IsActive &&
                x.EventId == eventId &&
                x.MediaType == "Audio");
        }

        /// <summary>
        /// Returns primary media count.
        /// </summary>
        public int GetPrimaryMediaCount()
        {
            return _db.EventMedias.Count(x =>
                x.IsActive &&
                x.IsPrimary);
        }

        /// <summary>
        /// Returns media grouped by media type.
        /// </summary>
        public Dictionary<string, int> GetMediaTypeSummary()
        {
            return _db.EventMedias
                .Where(x => x.IsActive)
                .GroupBy(x => x.MediaType)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns media grouped by category.
        /// </summary>
        public Dictionary<string, int> GetCategorySummary()
        {
            return _db.EventMedias
                .Where(x => x.IsActive)
                .GroupBy(x => x.MediaCategory)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns media grouped by uploader.
        /// </summary>
        public Dictionary<string, int> GetUploaderSummary()
        {
            return _db.EventMedias
                .Where(x => x.IsActive)
                .GroupBy(x => x.UploadedBy)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns monthly upload summary.
        /// </summary>
        public Dictionary<string, int> GetMonthlyUploadSummary(int year)
        {
            return _db.EventMedias
                .Where(x =>
                    x.IsActive &&
                    x.UploadedDate.Year == year)
                .GroupBy(x => x.UploadedDate.Month)
                .OrderBy(x => x.Key)
                .ToDictionary(
                    x => x.Key.ToString(),
                    x => x.Count());
        }
        #endregion

        #region Event Task

        /// <summary>
        /// Maps EventTask Entity to EventTaskVM.
        /// Used by all Task queries.
        /// </summary>
        private EventTaskVM MapTask(EventTask entity)
        {
            if (entity == null)
            {
                return null;
            }

            EventTaskVM vm = new EventTaskVM();

            vm.EventTaskId = entity.EventTaskId;

            vm.EventId = entity.EventId;

            if (entity.EventMaster != null)
            {
                vm.EventTitle = entity.EventMaster.EventTitle;
            }

            vm.ParentTaskId = entity.ParentTaskId;

            if (entity.EventTask2 != null)
            {
                vm.ParentTaskTitle = entity.EventTask2.TaskTitle;
            }

            vm.TaskTitle = entity.TaskTitle;
            vm.TaskDescription = entity.TaskDescription;
            vm.TaskCategory = entity.TaskCategory;

            vm.AssignedToMemberCode = entity.AssignedToMemberCode;
            vm.AssignedToName = entity.AssignedToName;

            vm.AssignedTeamId = entity.AssignedTeamId;

            if (entity.EventTeam != null)
            {
                vm.AssignedTeamName = entity.EventTeam.TeamName;
            }

            vm.AssignedByMemberCode = entity.AssignedByMemberCode;

            vm.AssignedDate = entity.AssignedDate;

            vm.StartDate = entity.StartDate;
            vm.DueDate = entity.DueDate;
            vm.CompletedDate = entity.CompletedDate;

            vm.Priority = entity.Priority;
            vm.Status = entity.Status;

            switch (entity.Status)
            {
                case "Completed":
                    vm.StatusColor = "success";
                    break;

                case "In Progress":
                    vm.StatusColor = "primary";
                    break;

                case "Pending":
                    vm.StatusColor = "warning";
                    break;

                case "Rejected":
                    vm.StatusColor = "danger";
                    break;

                case "Cancelled":
                    vm.StatusColor = "dark";
                    break;

                default:
                    vm.StatusColor = "secondary";
                    break;
            }

            vm.ProgressPercentage = entity.ProgressPercentage;

            vm.EstimatedHours = entity.EstimatedHours;
            vm.ActualHours = entity.ActualHours;

            vm.IsMilestone = entity.IsMilestone;
            vm.RequiresApproval = entity.RequiresApproval;

            vm.ApprovedByMemberCode = entity.ApprovedByMemberCode;
            vm.ApprovedDate = entity.ApprovedDate;

            vm.Remarks = entity.Remarks;

            vm.IsActive = entity.IsActive;

            vm.CreatedBy = entity.CreatedBy;
            vm.CreatedDate = entity.CreatedDate;

            vm.UpdatedBy = entity.UpdatedBy;
            vm.UpdatedDate = entity.UpdatedDate;

            if (entity.EventTaskActivities != null)
            {
                vm.ActivityCount =
                    entity.EventTaskActivities.Count(x => x.IsActive);
            }

            return vm;
        }

        /// <summary>
        /// Returns all active tasks.
        /// </summary>
        public List<EventTaskVM> GetTasks()
        {
            return _db.EventTasks
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.EventTaskId)
                .ToList()
                .Select(MapTask)
                .ToList();
        }

        /// <summary>
        /// Returns tasks of an event.
        /// </summary>
        public List<EventTaskVM> GetTasks(int eventId)
        {
            return _db.EventTasks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .OrderByDescending(x => x.EventTaskId)
                .ToList()
                .Select(MapTask)
                .ToList();
        }

        /// <summary>
        /// Returns task by Id.
        /// </summary>
        public EventTaskVM GetTaskById(int eventTaskId)
        {
            EventTask entity =
                _db.EventTasks
                .AsNoTracking()
                .FirstOrDefault(x =>
                    x.EventTaskId == eventTaskId &&
                    x.IsActive);

            return MapTask(entity);
        }

        /// <summary>
        /// Returns parent tasks only.
        /// </summary>
        public List<EventTaskVM> GetParentTasks()
        {
            return _db.EventTasks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.ParentTaskId == null)
                .OrderByDescending(x => x.EventTaskId)
                .ToList()
                .Select(MapTask)
                .ToList();
        }

        /// <summary>
        /// Returns child tasks.
        /// </summary>
        public List<EventTaskVM> GetChildTasks(int parentTaskId)
        {
            return _db.EventTasks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.ParentTaskId == parentTaskId)
                .OrderByDescending(x => x.EventTaskId)
                .ToList()
                .Select(MapTask)
                .ToList();
        }

        /// <summary>
        /// Returns tasks assigned to a team.
        /// </summary>
        public List<EventTaskVM> GetTasksByTeam(int teamId)
        {
            return _db.EventTasks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.AssignedTeamId == teamId)
                .OrderByDescending(x => x.EventTaskId)
                .ToList()
                .Select(MapTask)
                .ToList();
        }

        /// <summary>
        /// Returns tasks assigned to a member.
        /// </summary>
        public List<EventTaskVM> GetTasksByMember(string memberCode)
        {
            return _db.EventTasks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.AssignedToMemberCode == memberCode)
                .OrderByDescending(x => x.EventTaskId)
                .ToList()
                .Select(MapTask)
                .ToList();
        }

        /// <summary>
        /// Returns tasks by status.
        /// </summary>
        public List<EventTaskVM> GetTasksByStatus(string status)
        {
            return _db.EventTasks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.Status == status)
                .OrderByDescending(x => x.EventTaskId)
                .ToList()
                .Select(MapTask)
                .ToList();
        }

        /// <summary>
        /// Returns tasks by priority.
        /// </summary>
        public List<EventTaskVM> GetTasksByPriority(string priority)
        {
            return _db.EventTasks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.Priority == priority)
                .OrderByDescending(x => x.EventTaskId)
                .ToList()
                .Select(MapTask)
                .ToList();
        }

        /// <summary>
        /// Returns milestone tasks.
        /// </summary>
        public List<EventTaskVM> GetMilestoneTasks()
        {
            return _db.EventTasks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.IsMilestone)
                .OrderByDescending(x => x.EventTaskId)
                .ToList()
                .Select(MapTask)
                .ToList();
        }

        /// <summary>
        /// Searches tasks.
        /// </summary>
        public List<EventTaskVM> SearchTasks(string keyword)
        {
            IQueryable<EventTask> query =
                _db.EventTasks
                .AsNoTracking()
                .Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                query = query.Where(x =>

                    (x.TaskTitle != null &&
                     x.TaskTitle.Contains(keyword))

                    ||

                    (x.TaskDescription != null &&
                     x.TaskDescription.Contains(keyword))

                    ||

                    (x.TaskCategory != null &&
                     x.TaskCategory.Contains(keyword))

                    ||

                    (x.AssignedToName != null &&
                     x.AssignedToName.Contains(keyword))

                    ||

                    (x.AssignedToMemberCode != null &&
                     x.AssignedToMemberCode.Contains(keyword))

                    ||

                    (x.AssignedByMemberCode != null &&
                     x.AssignedByMemberCode.Contains(keyword))

                    ||

                    (x.Priority != null &&
                     x.Priority.Contains(keyword))

                    ||

                    (x.Status != null &&
                     x.Status.Contains(keyword))

                    ||

                    (x.EventMaster.EventTitle != null &&
                     x.EventMaster.EventTitle.Contains(keyword)));
            }

            return query
                .OrderByDescending(x => x.EventTaskId)
                .ToList()
                .Select(MapTask)
                .ToList();
        }

        /// <summary>
        /// Returns recent tasks.
        /// </summary>
        public List<EventTaskVM> GetRecentTasks(int count)
        {
            return _db.EventTasks
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CreatedDate)
                .Take(count)
                .ToList()
                .Select(MapTask)
                .ToList();
        }

        /// <summary>
        /// Returns dashboard tasks.
        /// </summary>
        public List<EventTaskVM> GetDashboardTasks(int count)
        {
            return _db.EventTasks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.Status != "Completed")
                .OrderBy(x => x.DueDate)
                .ThenByDescending(x => x.Priority)
                .Take(count)
                .ToList()
                .Select(MapTask)
                .ToList();
        }

        /// <summary>
        /// Returns overdue tasks.
        /// </summary>
        public List<EventTaskVM> GetOverdueTasks()
        {
            DateTime today = DateTime.Today;

            return _db.EventTasks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.DueDate.HasValue &&
                    x.DueDate.Value < today &&
                    x.Status != "Completed")
                .OrderBy(x => x.DueDate)
                .ToList()
                .Select(MapTask)
                .ToList();
        }

        /// <summary>
        /// Returns today's due tasks.
        /// </summary>
        public List<EventTaskVM> GetTodayTasks()
        {
            DateTime today = DateTime.Today;

            return _db.EventTasks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.DueDate.HasValue &&
                    x.DueDate.Value.Year == today.Year &&
                    x.DueDate.Value.Month == today.Month &&
                    x.DueDate.Value.Day == today.Day)
                .OrderBy(x => x.Priority)
                .ToList()
                .Select(MapTask)
                .ToList();
        }

        /// <summary>
        /// Returns upcoming tasks.
        /// </summary>
        public List<EventTaskVM> GetUpcomingTasks(int days)
        {
            DateTime today = DateTime.Today;
            DateTime endDate = today.AddDays(days);

            return _db.EventTasks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.DueDate.HasValue &&
                    x.DueDate.Value >= today &&
                    x.DueDate.Value <= endDate)
                .OrderBy(x => x.DueDate)
                .ToList()
                .Select(MapTask)
                .ToList();
        }

        /// <summary>
        /// Saves a new task.
        /// </summary>
        public bool SaveTask(EventTaskVM vm, int userId)
        {
            if (vm == null)
            {
                return false;
            }

            if (vm.ProgressPercentage < 0)
            {
                vm.ProgressPercentage = 0;
            }

            if (vm.ProgressPercentage > 100)
            {
                vm.ProgressPercentage = 100;
            }

            EventTask entity = new EventTask();

            entity.EventId = vm.EventId;

            entity.ParentTaskId = vm.ParentTaskId;

            entity.TaskTitle = vm.TaskTitle;
            entity.TaskDescription = vm.TaskDescription;
            entity.TaskCategory = vm.TaskCategory;

            entity.AssignedToMemberCode = vm.AssignedToMemberCode;
            entity.AssignedToName = vm.AssignedToName;

            entity.AssignedTeamId = vm.AssignedTeamId;

            entity.AssignedByMemberCode = vm.AssignedByMemberCode;

            entity.AssignedDate = DateTime.Now;

            entity.StartDate = vm.StartDate;
            entity.DueDate = vm.DueDate;
            entity.CompletedDate = vm.CompletedDate;

            entity.Priority = vm.Priority;

            entity.Status = string.IsNullOrWhiteSpace(vm.Status)
                ? "Pending"
                : vm.Status;

            entity.ProgressPercentage = vm.ProgressPercentage;

            entity.EstimatedHours = vm.EstimatedHours;
            entity.ActualHours = vm.ActualHours;

            entity.IsMilestone = vm.IsMilestone;
            entity.RequiresApproval = vm.RequiresApproval;

            entity.ApprovedByMemberCode = vm.ApprovedByMemberCode;
            entity.ApprovedDate = vm.ApprovedDate;

            entity.Remarks = vm.Remarks;

            entity.IsActive = true;

            entity.CreatedBy = userId;
            entity.CreatedDate = DateTime.Now;

            _db.EventTasks.Add(entity);

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Updates an existing task.
        /// </summary>
        public bool UpdateTask(EventTaskVM vm, int userId)
        {
            if (vm == null)
            {
                return false;
            }

            EventTask entity =
                _db.EventTasks
                .FirstOrDefault(x =>
                    x.EventTaskId == vm.EventTaskId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            if (vm.ProgressPercentage < 0)
            {
                vm.ProgressPercentage = 0;
            }

            if (vm.ProgressPercentage > 100)
            {
                vm.ProgressPercentage = 100;
            }

            entity.EventId = vm.EventId;

            entity.ParentTaskId = vm.ParentTaskId;

            entity.TaskTitle = vm.TaskTitle;
            entity.TaskDescription = vm.TaskDescription;
            entity.TaskCategory = vm.TaskCategory;

            entity.AssignedToMemberCode = vm.AssignedToMemberCode;
            entity.AssignedToName = vm.AssignedToName;

            entity.AssignedTeamId = vm.AssignedTeamId;

            entity.Priority = vm.Priority;
            entity.Status = vm.Status;

            entity.StartDate = vm.StartDate;
            entity.DueDate = vm.DueDate;
            entity.CompletedDate = vm.CompletedDate;

            entity.ProgressPercentage = vm.ProgressPercentage;

            entity.EstimatedHours = vm.EstimatedHours;
            entity.ActualHours = vm.ActualHours;

            entity.IsMilestone = vm.IsMilestone;
            entity.RequiresApproval = vm.RequiresApproval;

            entity.ApprovedByMemberCode = vm.ApprovedByMemberCode;
            entity.ApprovedDate = vm.ApprovedDate;

            entity.Remarks = vm.Remarks;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Soft deletes a task.
        /// </summary>
        public bool DeleteTask(int eventTaskId, int userId)
        {
            EventTask entity =
                _db.EventTasks
                .FirstOrDefault(x =>
                    x.EventTaskId == eventTaskId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.IsActive = false;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }


        /// <summary>
        /// Accepts a task.
        /// </summary>
        public bool AcceptTask(int eventTaskId, int userId)
        {
            EventTask entity =
                _db.EventTasks
                .FirstOrDefault(x =>
                    x.EventTaskId == eventTaskId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.Status = "Accepted";

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Starts a task.
        /// </summary>
        public bool StartTask(int eventTaskId, int userId)
        {
            EventTask entity =
                _db.EventTasks
                .FirstOrDefault(x =>
                    x.EventTaskId == eventTaskId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.Status = "In Progress";

            if (!entity.StartDate.HasValue)
            {
                entity.StartDate = DateTime.Now;
            }

            if (entity.ProgressPercentage < 1)
            {
                entity.ProgressPercentage = 1;
            }

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Updates task progress.
        /// </summary>
        public bool UpdateTaskProgress(
            int eventTaskId,
            int progressPercentage,
            decimal? actualHours,
            int userId)
        {
            EventTask entity =
                _db.EventTasks
                .FirstOrDefault(x =>
                    x.EventTaskId == eventTaskId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            if (progressPercentage < 0)
            {
                progressPercentage = 0;
            }

            if (progressPercentage > 100)
            {
                progressPercentage = 100;
            }

            entity.ProgressPercentage = progressPercentage;
            entity.ActualHours = actualHours;

            if (progressPercentage > 0 &&
                entity.Status == "Pending")
            {
                entity.Status = "In Progress";
            }

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Completes a task.
        /// </summary>
        public bool CompleteTask(int eventTaskId, int userId)
        {
            EventTask entity =
                _db.EventTasks
                .FirstOrDefault(x =>
                    x.EventTaskId == eventTaskId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.ProgressPercentage = 100;

            entity.CompletedDate = DateTime.Now;

            if (entity.RequiresApproval)
            {
                entity.Status = "Waiting Approval";
            }
            else
            {
                entity.Status = "Completed";
            }

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Approves a completed task.
        /// </summary>
        public bool ApproveTask(
            int eventTaskId,
            string approvedByMemberCode,
            int userId)
        {
            EventTask entity =
                _db.EventTasks
                .FirstOrDefault(x =>
                    x.EventTaskId == eventTaskId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.Status = "Completed";

            entity.ApprovedByMemberCode = approvedByMemberCode;
            entity.ApprovedDate = DateTime.Now;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Cancels a task.
        /// </summary>
        public bool CancelTask(
            int eventTaskId,
            string remarks,
            int userId)
        {
            EventTask entity =
                _db.EventTasks
                .FirstOrDefault(x =>
                    x.EventTaskId == eventTaskId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.Status = "Cancelled";

            entity.Remarks = remarks;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Returns total tasks.
        /// </summary>
        public int GetTotalTasks()
        {
            return _db.EventTasks.Count(x => x.IsActive);
        }

        /// <summary>
        /// Returns completed tasks.
        /// </summary>
        public int GetCompletedTasks()
        {
            return _db.EventTasks.Count(x =>
                x.IsActive &&
                x.Status == "Completed");
        }

        /// <summary>
        /// Returns pending tasks.
        /// </summary>
        public int GetPendingTasks()
        {
            return _db.EventTasks.Count(x =>
                x.IsActive &&
                x.Status == "Pending");
        }

        /// <summary>
        /// Returns tasks in progress.
        /// </summary>
        public int GetInProgressTasks()
        {
            return _db.EventTasks.Count(x =>
                x.IsActive &&
                x.Status == "In Progress");
        }

        /// <summary>
        /// Returns overdue tasks.
        /// </summary>
        public int GetOverdueTaskCount()
        {
            DateTime today = DateTime.Today;

            return _db.EventTasks.Count(x =>
                x.IsActive &&
                x.DueDate.HasValue &&
                x.DueDate.Value < today &&
                x.Status != "Completed");
        }

        /// <summary>
        /// Returns milestone tasks.
        /// </summary>
        public int GetMilestoneTaskCount()
        {
            return _db.EventTasks.Count(x =>
                x.IsActive &&
                x.IsMilestone);
        }

        /// <summary>
        /// Returns tasks waiting for approval.
        /// </summary>
        public int GetWaitingApprovalTaskCount()
        {
            return _db.EventTasks.Count(x =>
                x.IsActive &&
                x.Status == "Waiting Approval");
        }

        /// <summary>
        /// Returns task completion percentage.
        /// </summary>
        public decimal GetTaskCompletionPercentage()
        {
            int total = GetTotalTasks();

            if (total == 0)
            {
                return 0;
            }

            int completed = GetCompletedTasks();

            return Math.Round(
                ((decimal)completed / total) * 100,
                2);
        }

        /// <summary>
        /// Returns tasks grouped by priority.
        /// </summary>
        public Dictionary<string, int> GetTaskPrioritySummary()
        {
            return _db.EventTasks
                .Where(x => x.IsActive)
                .GroupBy(x => x.Priority)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns tasks grouped by status.
        /// </summary>
        public Dictionary<string, int> GetTaskStatusSummary()
        {
            return _db.EventTasks
                .Where(x => x.IsActive)
                .GroupBy(x => x.Status)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns task category summary.
        /// </summary>
        public Dictionary<string, int> GetTaskCategorySummary()
        {
            return _db.EventTasks
                .Where(x => x.IsActive)
                .GroupBy(x => x.TaskCategory)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns monthly task creation summary.
        /// </summary>
        public Dictionary<string, int> GetMonthlyTaskSummary(int year)
        {
            return _db.EventTasks
                .Where(x =>
                    x.IsActive &&
                    x.CreatedDate.Year == year)
                .GroupBy(x => x.CreatedDate.Month)
                .OrderBy(x => x.Key)
                .ToDictionary(
                    x => x.Key.ToString(),
                    x => x.Count());
        }

        /// <summary>
        /// Returns task count for an event.
        /// </summary>
        public int GetTaskCountByEvent(int eventId)
        {
            return _db.EventTasks.Count(x =>
                x.IsActive &&
                x.EventId == eventId);
        }

        /// <summary>
        /// Returns completed task count for an event.
        /// </summary>
        public int GetCompletedTaskCountByEvent(int eventId)
        {
            return _db.EventTasks.Count(x =>
                x.IsActive &&
                x.EventId == eventId &&
                x.Status == "Completed");
        }

        /// <summary>
        /// Maps EventTaskActivity Entity to ViewModel.
        /// </summary>
        private EventTaskActivityVM MapTaskActivity(EventTaskActivity entity)
        {
            if (entity == null)
            {
                return null;
            }

            EventTaskActivityVM vm = new EventTaskActivityVM();

            vm.EventTaskActivityId = entity.EventTaskActivityId;

            vm.EventTaskId = entity.EventTaskId;

            if (entity.EventTask != null)
            {
                vm.TaskTitle = entity.EventTask.TaskTitle;
            }

            vm.ActivityType = entity.ActivityType;

            vm.OldStatus = entity.OldStatus;
            vm.NewStatus = entity.NewStatus;

            vm.ProgressPercentage = entity.ProgressPercentage;

            vm.ActivityRemarks = entity.ActivityRemarks;

            vm.AttachmentPath = entity.AttachmentPath;

            vm.ActivityByMemberCode = entity.ActivityByMemberCode;
            vm.ActivityByName = entity.ActivityByName;

            vm.ActivityDate = entity.ActivityDate;

            vm.Latitude = entity.Latitude;
            vm.Longitude = entity.Longitude;

            vm.IsActive = entity.IsActive;

            vm.CreatedBy = entity.CreatedBy;
            vm.CreatedDate = entity.CreatedDate;

            vm.UpdatedBy = entity.UpdatedBy;
            vm.UpdatedDate = entity.UpdatedDate;

            return vm;
        }


        /// <summary>
        /// Searches task activities.
        /// </summary>
        public List<EventTaskActivityVM> SearchTaskActivities(string keyword)
        {
            IQueryable<EventTaskActivity> query =
                _db.EventTaskActivities
                .AsNoTracking()
                .Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                query = query.Where(x =>

                    (x.ActivityType != null &&
                     x.ActivityType.Contains(keyword))

                    ||

                    (x.ActivityRemarks != null &&
                     x.ActivityRemarks.Contains(keyword))

                    ||

                    (x.ActivityByMemberCode != null &&
                     x.ActivityByMemberCode.Contains(keyword))

                    ||

                    (x.ActivityByName != null &&
                     x.ActivityByName.Contains(keyword))

                    ||

                    (x.NewStatus != null &&
                     x.NewStatus.Contains(keyword))

                    ||

                    (x.OldStatus != null &&
                     x.OldStatus.Contains(keyword))

                    ||

                    (x.EventTask.TaskTitle != null &&
                     x.EventTask.TaskTitle.Contains(keyword)));
            }

            return query
                .OrderByDescending(x => x.ActivityDate)
                .ThenByDescending(x => x.EventTaskActivityId)
                .ToList()
                .Select(MapTaskActivity)
                .ToList();
        }

        /// <summary>
        /// Returns recent task activities.
        /// </summary>
        public List<EventTaskActivityVM> GetRecentTaskActivities(int count)
        {
            return _db.EventTaskActivities
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.ActivityDate)
                .Take(count)
                .ToList()
                .Select(MapTaskActivity)
                .ToList();
        }

        /// <summary>
        /// Returns dashboard task activities.
        /// </summary>
        public List<EventTaskActivityVM> GetTaskActivityDashboard(int count)
        {
            return _db.EventTaskActivities
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.ActivityDate)
                .Take(count)
                .ToList()
                .Select(MapTaskActivity)
                .ToList();
        }

        /// <summary>
        /// Returns activity count of a task.
        /// </summary>
        public int GetActivityCountByTask(int eventTaskId)
        {
            return _db.EventTaskActivities.Count(x =>
                x.IsActive &&
                x.EventTaskId == eventTaskId);
        }

        /// <summary>
        /// Returns today's activity count.
        /// </summary>
        public int GetTodayActivityCount()
        {
            DateTime today = DateTime.Today;

            return _db.EventTaskActivities.Count(x =>
                x.IsActive &&
                x.ActivityDate.Year == today.Year &&
                x.ActivityDate.Month == today.Month &&
                x.ActivityDate.Day == today.Day);
        }

        /// <summary>
        /// Returns activity summary grouped by activity type.
        /// </summary>
        public Dictionary<string, int> GetTaskActivityTypeSummary()
        {
            return _db.EventTaskActivities
                .Where(x => x.IsActive)
                .GroupBy(x => x.ActivityType)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns activity summary grouped by member.
        /// </summary>
        public Dictionary<string, int> GetTaskActivityMemberSummary()
        {
            return _db.EventTaskActivities
                .Where(x => x.IsActive)
                .GroupBy(x => x.ActivityByName)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns monthly activity summary.
        /// </summary>
        public Dictionary<string, int> GetMonthlyTaskActivitySummary(int year)
        {
            return _db.EventTaskActivities
                .Where(x =>
                    x.IsActive &&
                    x.ActivityDate.Year == year)
                .GroupBy(x => x.ActivityDate.Month)
                .OrderBy(x => x.Key)
                .ToDictionary(
                    x => x.Key.ToString(),
                    x => x.Count());
        }
        #endregion

        #region Event Poll

        /// <summary>
        /// Maps EventPoll Entity to ViewModel.
        /// Used by all Poll queries.
        /// </summary>
        private EventPollVM MapPoll(EventPoll entity)
        {
            if (entity == null)
            {
                return null;
            }

            EventPollVM vm = new EventPollVM();

            vm.EventPollId = entity.EventPollId;

            vm.EventId = entity.EventId;

            if (entity.EventMaster != null)
            {
                vm.EventTitle = entity.EventMaster.EventTitle;
            }

            vm.PollTitle = entity.PollTitle;
            vm.PollDescription = entity.PollDescription;

            vm.PollType = entity.PollType;
            vm.QuestionType = entity.QuestionType;

            vm.StartDate = entity.StartDate;
            vm.EndDate = entity.EndDate;

            vm.IsAnonymous = entity.IsAnonymous;
            vm.AllowMultipleSelection = entity.AllowMultipleSelection;
            vm.MaximumSelection = entity.MaximumSelection;

            vm.Status = entity.Status;

            switch (entity.Status)
            {
                case "Published":
                    vm.StatusColor = "success";
                    break;

                case "Closed":
                    vm.StatusColor = "dark";
                    break;

                case "Draft":
                    vm.StatusColor = "secondary";
                    break;

                case "Active":
                    vm.StatusColor = "primary";
                    break;

                case "Completed":
                    vm.StatusColor = "info";
                    break;

                default:
                    vm.StatusColor = "warning";
                    break;
            }

            vm.Remarks = entity.Remarks;

            vm.IsActive = entity.IsActive;

            vm.CreatedBy = entity.CreatedBy;
            vm.CreatedDate = entity.CreatedDate;

            vm.UpdatedBy = entity.UpdatedBy;
            vm.UpdatedDate = entity.UpdatedDate;

            if (entity.EventPollOptions != null)
            {
                vm.OptionCount =
                    entity.EventPollOptions.Count(x => x.IsActive);
            }

            if (entity.EventPollResponses != null)
            {
                vm.ResponseCount =
                    entity.EventPollResponses.Count(x => x.IsActive);
            }

            return vm;
        }

        /// <summary>
        /// Maps EventPollOption Entity to ViewModel.
        /// </summary>
        private EventPollOptionVM MapPollOption(EventPollOption entity)
        {
            if (entity == null)
            {
                return null;
            }

            EventPollOptionVM vm =
                new EventPollOptionVM();

            vm.EventPollOptionId =
                entity.EventPollOptionId;

            vm.EventPollId =
                entity.EventPollId;

            if (entity.EventPoll != null)
            {
                vm.PollTitle =
                    entity.EventPoll.PollTitle;
            }

            vm.OptionText =
                entity.OptionText;

            vm.DisplayOrder =
                entity.DisplayOrder;

            vm.VoteCount =
                entity.VoteCount;

            int totalVotes = 0;

            if (entity.EventPoll != null)
            {
                totalVotes =
                    entity.EventPoll.EventPollOptions
                    .Where(x => x.IsActive)
                    .Sum(x => (int?)x.VoteCount) ?? 0;
            }

            if (totalVotes > 0)
            {
                vm.VotePercentage =
                    Math.Round(
                        ((decimal)entity.VoteCount / totalVotes) * 100,
                        2);
            }

            if (entity.EventPoll != null)
            {
                int maxVote =
                    entity.EventPoll.EventPollOptions
                    .Where(x => x.IsActive)
                    .Max(x => (int?)x.VoteCount) ?? 0;

                vm.IsWinner =
                    entity.VoteCount == maxVote;
            }

            vm.IsActive =
                entity.IsActive;

            vm.CreatedBy =
                entity.CreatedBy;

            vm.CreatedDate =
                entity.CreatedDate;

            vm.UpdatedBy =
                entity.UpdatedBy;

            vm.UpdatedDate =
                entity.UpdatedDate;

            return vm;
        }

        /// <summary>
        /// Maps EventPollResponse Entity to ViewModel.
        /// </summary>
        private EventPollResponseVM MapPollResponse(EventPollResponse entity)
        {
            if (entity == null)
            {
                return null;
            }

            EventPollResponseVM vm =
                new EventPollResponseVM();

            vm.EventPollResponseId =
                entity.EventPollResponseId;

            vm.EventPollId =
                entity.EventPollId;

            if (entity.EventPoll != null)
            {
                vm.PollTitle =
                    entity.EventPoll.PollTitle;
            }

            vm.EventPollOptionId =
                entity.EventPollOptionId;

            if (entity.EventPollOption != null)
            {
                vm.OptionText =
                    entity.EventPollOption.OptionText;
            }

            vm.SurveyPersonMemberCode =
                entity.SurveyPersonMemberCode;

            vm.SurveyPersonName =
                entity.SurveyPersonName;

            vm.RespondentName =
                entity.RespondentName;

            vm.RespondentMobile =
                entity.RespondentMobile;

            vm.Gender =
                entity.Gender;

            vm.Age =
                entity.Age;

            vm.State =
                entity.State;

            vm.District =
                entity.District;

            vm.Block =
                entity.Block;

            vm.Village =
                entity.Village;

            vm.Booth =
                entity.Booth;

            vm.Latitude =
                entity.Latitude;

            vm.Longitude =
                entity.Longitude;

            vm.ResponseText =
                entity.ResponseText;

            vm.DeviceId =
                entity.DeviceId;

            vm.IsVerified =
                entity.IsVerified;

            vm.ResponseDate =
                entity.ResponseDate;

            vm.Remarks =
                entity.Remarks;

            vm.IsActive =
                entity.IsActive;

            vm.CreatedBy =
                entity.CreatedBy;

            vm.CreatedDate =
                entity.CreatedDate;

            vm.UpdatedBy =
                entity.UpdatedBy;

            vm.UpdatedDate =
                entity.UpdatedDate;

            return vm;
        }

        /// <summary>
        /// Returns all active polls.
        /// </summary>
        public List<EventPollVM> GetPolls()
        {
            return _db.EventPolls
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.EventPollId)
                .ToList()
                .Select(MapPoll)
                .ToList();
        }

        /// <summary>
        /// Returns polls of an event.
        /// </summary>
        public List<EventPollVM> GetPolls(int eventId)
        {
            return _db.EventPolls
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .OrderByDescending(x => x.EventPollId)
                .ToList()
                .Select(MapPoll)
                .ToList();
        }

        /// <summary>
        /// Returns poll by Id.
        /// </summary>
        public EventPollVM GetPollById(int eventPollId)
        {
            EventPoll entity =
                _db.EventPolls
                .AsNoTracking()
                .FirstOrDefault(x =>
                    x.EventPollId == eventPollId &&
                    x.IsActive);

            return MapPoll(entity);
        }

        /// <summary>
        /// Returns active polls.
        /// </summary>
        public List<EventPollVM> GetActivePolls()
        {
            return _db.EventPolls
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.Status == "Active")
                .OrderByDescending(x => x.EventPollId)
                .ToList()
                .Select(MapPoll)
                .ToList();
        }

        /// <summary>
        /// Returns published polls.
        /// </summary>
        public List<EventPollVM> GetPublishedPolls()
        {
            return _db.EventPolls
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.Status == "Published")
                .OrderByDescending(x => x.EventPollId)
                .ToList()
                .Select(MapPoll)
                .ToList();
        }

        /// <summary>
        /// Returns poll options.
        /// </summary>
        public List<EventPollOptionVM> GetPollOptions(int eventPollId)
        {
            return _db.EventPollOptions
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.EventPollId == eventPollId)
                .OrderBy(x => x.DisplayOrder)
                .ThenByDescending(x => x.EventPollOptionId)
                .ToList()
                .Select(MapPollOption)
                .ToList();
        }

        /// <summary>
        /// Returns poll option by Id.
        /// </summary>
        public EventPollOptionVM GetPollOptionById(int eventPollOptionId)
        {
            EventPollOption entity =
                _db.EventPollOptions
                .AsNoTracking()
                .FirstOrDefault(x =>
                    x.EventPollOptionId == eventPollOptionId &&
                    x.IsActive);

            return MapPollOption(entity);
        }

        /// <summary>
        /// Returns responses of a poll.
        /// </summary>
        public List<EventPollResponseVM> GetPollResponses(int eventPollId)
        {
            return _db.EventPollResponses
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.EventPollId == eventPollId)
                .OrderByDescending(x => x.ResponseDate)
                .ThenByDescending(x => x.EventPollResponseId)
                .ToList()
                .Select(MapPollResponse)
                .ToList();
        }

        /// <summary>
        /// Returns poll response by Id.
        /// </summary>
        public EventPollResponseVM GetPollResponseById(int eventPollResponseId)
        {
            EventPollResponse entity =
                _db.EventPollResponses
                .AsNoTracking()
                .FirstOrDefault(x =>
                    x.EventPollResponseId == eventPollResponseId &&
                    x.IsActive);

            return MapPollResponse(entity);
        }

        /// <summary>
        /// Searches polls.
        /// </summary>
        public List<EventPollVM> SearchPolls(string keyword)
        {
            IQueryable<EventPoll> query =
                _db.EventPolls
                .AsNoTracking()
                .Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                query = query.Where(x =>

                    (x.PollTitle != null &&
                     x.PollTitle.Contains(keyword))

                    ||

                    (x.PollDescription != null &&
                     x.PollDescription.Contains(keyword))

                    ||

                    (x.PollType != null &&
                     x.PollType.Contains(keyword))

                    ||

                    (x.QuestionType != null &&
                     x.QuestionType.Contains(keyword))

                    ||

                    (x.Status != null &&
                     x.Status.Contains(keyword))

                    ||

                    (x.EventMaster.EventTitle != null &&
                     x.EventMaster.EventTitle.Contains(keyword)));
            }

            return query
                .OrderByDescending(x => x.EventPollId)
                .ToList()
                .Select(MapPoll)
                .ToList();
        }

        /// <summary>
        /// Searches poll responses.
        /// </summary>
        public List<EventPollResponseVM> SearchPollResponses(string keyword)
        {
            IQueryable<EventPollResponse> query =
                _db.EventPollResponses
                .AsNoTracking()
                .Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                query = query.Where(x =>

                    (x.RespondentName != null &&
                     x.RespondentName.Contains(keyword))

                    ||

                    (x.RespondentMobile != null &&
                     x.RespondentMobile.Contains(keyword))

                    ||

                    (x.SurveyPersonName != null &&
                     x.SurveyPersonName.Contains(keyword))

                    ||

                    (x.SurveyPersonMemberCode != null &&
                     x.SurveyPersonMemberCode.Contains(keyword))

                    ||

                    (x.ResponseText != null &&
                     x.ResponseText.Contains(keyword))

                    ||

                    (x.Village != null &&
                     x.Village.Contains(keyword))

                    ||

                    (x.Block != null &&
                     x.Block.Contains(keyword))

                    ||

                    (x.District != null &&
                     x.District.Contains(keyword))

                    ||

                    (x.Booth != null &&
                     x.Booth.Contains(keyword))

                    ||

                    (x.EventPoll.PollTitle != null &&
                     x.EventPoll.PollTitle.Contains(keyword)));
            }

            return query
                .OrderByDescending(x => x.ResponseDate)
                .ThenByDescending(x => x.EventPollResponseId)
                .ToList()
                .Select(MapPollResponse)
                .ToList();
        }

        /// <summary>
        /// Returns recent polls.
        /// </summary>
        public List<EventPollVM> GetRecentPolls(int count)
        {
            return _db.EventPolls
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CreatedDate)
                .Take(count)
                .ToList()
                .Select(MapPoll)
                .ToList();
        }

        /// <summary>
        /// Returns dashboard polls.
        /// </summary>
        public List<EventPollVM> GetDashboardPolls(int count)
        {
            return _db.EventPolls
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.Status == "Active")
                .OrderBy(x => x.EndDate)
                .Take(count)
                .ToList()
                .Select(MapPoll)
                .ToList();
        }

        /// <summary>
        /// Returns expired polls.
        /// </summary>
        public List<EventPollVM> GetExpiredPolls()
        {
            DateTime today = DateTime.Today;

            return _db.EventPolls
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.EndDate.HasValue &&
                    x.EndDate.Value < today)
                .OrderByDescending(x => x.EndDate)
                .ToList()
                .Select(MapPoll)
                .ToList();
        }

        /// <summary>
        /// Returns upcoming polls.
        /// </summary>
        public List<EventPollVM> GetUpcomingPolls()
        {
            DateTime today = DateTime.Today;

            return _db.EventPolls
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.StartDate > today)
                .OrderBy(x => x.StartDate)
                .ToList()
                .Select(MapPoll)
                .ToList();
        }

        /// <summary>
        /// Returns recent poll responses.
        /// </summary>
        public List<EventPollResponseVM> GetRecentPollResponses(int count)
        {
            return _db.EventPollResponses
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.ResponseDate)
                .Take(count)
                .ToList()
                .Select(MapPollResponse)
                .ToList();
        }


        //=========================================================
        // Part 4 Starts
        // CRUD (Poll + Poll Option)
        //=========================================================

        /// <summary>
        /// Saves a new poll.
        /// </summary>
        public bool SavePoll(EventPollVM vm, int userId)
        {
            if (vm == null)
            {
                return false;
            }

            EventPoll entity = new EventPoll();

            entity.EventId = vm.EventId;

            entity.PollTitle = vm.PollTitle;
            entity.PollDescription = vm.PollDescription;

            entity.PollType = vm.PollType;
            entity.QuestionType = vm.QuestionType;

            entity.StartDate = vm.StartDate;
            entity.EndDate = vm.EndDate;

            entity.IsAnonymous = vm.IsAnonymous;
            entity.AllowMultipleSelection = vm.AllowMultipleSelection;
            entity.MaximumSelection = vm.MaximumSelection;

            entity.Status = string.IsNullOrWhiteSpace(vm.Status)
                ? "Draft"
                : vm.Status;

            entity.Remarks = vm.Remarks;

            entity.IsActive = true;

            entity.CreatedBy = userId;
            entity.CreatedDate = DateTime.Now;

            _db.EventPolls.Add(entity);

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Updates poll.
        /// </summary>
        public bool UpdatePoll(EventPollVM vm, int userId)
        {
            if (vm == null)
            {
                return false;
            }

            EventPoll entity =
                _db.EventPolls
                .FirstOrDefault(x =>
                    x.EventPollId == vm.EventPollId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.EventId = vm.EventId;

            entity.PollTitle = vm.PollTitle;
            entity.PollDescription = vm.PollDescription;

            entity.PollType = vm.PollType;
            entity.QuestionType = vm.QuestionType;

            entity.StartDate = vm.StartDate;
            entity.EndDate = vm.EndDate;

            entity.IsAnonymous = vm.IsAnonymous;
            entity.AllowMultipleSelection = vm.AllowMultipleSelection;
            entity.MaximumSelection = vm.MaximumSelection;

            entity.Status = vm.Status;

            entity.Remarks = vm.Remarks;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Soft deletes poll.
        /// </summary>
        public bool DeletePoll(int eventPollId, int userId)
        {
            EventPoll entity =
                _db.EventPolls
                .FirstOrDefault(x =>
                    x.EventPollId == eventPollId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.IsActive = false;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Saves poll option.
        /// </summary>
        public bool SavePollOption(EventPollOptionVM vm, int userId)
        {
            if (vm == null)
            {
                return false;
            }

            EventPollOption entity =
                new EventPollOption();

            entity.EventPollId = vm.EventPollId;

            entity.OptionText = vm.OptionText;

            entity.DisplayOrder = vm.DisplayOrder;

            entity.VoteCount = 0;

            entity.IsActive = true;

            entity.CreatedBy = userId;
            entity.CreatedDate = DateTime.Now;

            _db.EventPollOptions.Add(entity);

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Updates poll option.
        /// </summary>
        public bool UpdatePollOption(EventPollOptionVM vm, int userId)
        {
            EventPollOption entity =
                _db.EventPollOptions
                .FirstOrDefault(x =>
                    x.EventPollOptionId == vm.EventPollOptionId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.OptionText = vm.OptionText;

            entity.DisplayOrder = vm.DisplayOrder;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Soft deletes poll option.
        /// </summary>
        public bool DeletePollOption(int eventPollOptionId, int userId)
        {
            EventPollOption entity =
                _db.EventPollOptions
                .FirstOrDefault(x =>
                    x.EventPollOptionId == eventPollOptionId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.IsActive = false;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        //=========================================================
        // End Part 4
        //=========================================================

        //=========================================================
        // Part 5 Starts
        // Poll Workflow
        //=========================================================

        /// <summary>
        /// Publishes a poll.
        /// </summary>
        public bool PublishPoll(int eventPollId, int userId)
        {
            EventPoll entity =
                _db.EventPolls
                .FirstOrDefault(x =>
                    x.EventPollId == eventPollId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.Status = "Published";

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Activates a poll.
        /// </summary>
        public bool ActivatePoll(int eventPollId, int userId)
        {
            EventPoll entity =
                _db.EventPolls
                .FirstOrDefault(x =>
                    x.EventPollId == eventPollId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.Status = "Active";

            if (entity.StartDate > DateTime.Now)
            {
                entity.StartDate = DateTime.Now;
            }

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Closes a poll.
        /// </summary>
        public bool ClosePoll(int eventPollId, int userId)
        {
            EventPoll entity =
                _db.EventPolls
                .FirstOrDefault(x =>
                    x.EventPollId == eventPollId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.Status = "Closed";

            entity.EndDate = DateTime.Now;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Reopens a closed poll.
        /// </summary>
        public bool ReopenPoll(int eventPollId, int userId)
        {
            EventPoll entity =
                _db.EventPolls
                .FirstOrDefault(x =>
                    x.EventPollId == eventPollId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.Status = "Active";

            if (entity.EndDate.HasValue &&
                entity.EndDate.Value < DateTime.Now)
            {
                entity.EndDate = null;
            }

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Archives a poll.
        /// </summary>
        public bool ArchivePoll(int eventPollId, int userId)
        {
            EventPoll entity =
                _db.EventPolls
                .FirstOrDefault(x =>
                    x.EventPollId == eventPollId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.Status = "Archived";

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Marks poll as draft.
        /// </summary>
        public bool MarkPollAsDraft(int eventPollId, int userId)
        {
            EventPoll entity =
                _db.EventPolls
                .FirstOrDefault(x =>
                    x.EventPollId == eventPollId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.Status = "Draft";

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        //=========================================================
        // End Part 5
        //=========================================================

        //=========================================================
        // Part 6 Starts
        // Poll KPI + Reports
        //=========================================================

        /// <summary>
        /// Returns total polls.
        /// </summary>
        public int GetTotalPolls()
        {
            return _db.EventPolls.Count(x => x.IsActive);
        }

        /// <summary>
        /// Returns active polls.
        /// </summary>
        public int GetActivePollCount()
        {
            return _db.EventPolls.Count(x =>
                x.IsActive &&
                x.Status == "Active");
        }

        /// <summary>
        /// Returns published polls.
        /// </summary>
        public int GetPublishedPollCount()
        {
            return _db.EventPolls.Count(x =>
                x.IsActive &&
                x.Status == "Published");
        }

        /// <summary>
        /// Returns draft polls.
        /// </summary>
        public int GetDraftPollCount()
        {
            return _db.EventPolls.Count(x =>
                x.IsActive &&
                x.Status == "Draft");
        }

        /// <summary>
        /// Returns closed polls.
        /// </summary>
        public int GetClosedPollCount()
        {
            return _db.EventPolls.Count(x =>
                x.IsActive &&
                x.Status == "Closed");
        }

        /// <summary>
        /// Returns archived polls.
        /// </summary>
        public int GetArchivedPollCount()
        {
            return _db.EventPolls.Count(x =>
                x.IsActive &&
                x.Status == "Archived");
        }

        /// <summary>
        /// Returns total poll responses.
        /// </summary>
        public int GetTotalPollResponses()
        {
            return _db.EventPollResponses.Count(x => x.IsActive);
        }

        /// <summary>
        /// Returns verified responses.
        /// </summary>
        public int GetVerifiedPollResponses()
        {
            return _db.EventPollResponses.Count(x =>
                x.IsActive &&
                x.IsVerified);
        }

        /// <summary>
        /// Returns unverified responses.
        /// </summary>
        public int GetUnVerifiedPollResponses()
        {
            return _db.EventPollResponses.Count(x =>
                x.IsActive &&
                !x.IsVerified);
        }

        /// <summary>
        /// Returns total poll options.
        /// </summary>
        public int GetTotalPollOptions()
        {
            return _db.EventPollOptions.Count(x => x.IsActive);
        }

        /// <summary>
        /// Returns poll count of an event.
        /// </summary>
        public int GetPollCountByEvent(int eventId)
        {
            return _db.EventPolls.Count(x =>
                x.IsActive &&
                x.EventId == eventId);
        }

        /// <summary>
        /// Returns response count of a poll.
        /// </summary>
        public int GetResponseCountByPoll(int eventPollId)
        {
            return _db.EventPollResponses.Count(x =>
                x.IsActive &&
                x.EventPollId == eventPollId);
        }

        /// <summary>
        /// Returns option count of a poll.
        /// </summary>
        public int GetOptionCountByPoll(int eventPollId)
        {
            return _db.EventPollOptions.Count(x =>
                x.IsActive &&
                x.EventPollId == eventPollId);
        }

        /// <summary>
        /// Returns poll status summary.
        /// </summary>
        public Dictionary<string, int> GetPollStatusSummary()
        {
            return _db.EventPolls
                .Where(x => x.IsActive)
                .GroupBy(x => x.Status)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns poll type summary.
        /// </summary>
        public Dictionary<string, int> GetPollTypeSummary()
        {
            return _db.EventPolls
                .Where(x => x.IsActive)
                .GroupBy(x => x.PollType)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns question type summary.
        /// </summary>
        public Dictionary<string, int> GetQuestionTypeSummary()
        {
            return _db.EventPolls
                .Where(x => x.IsActive)
                .GroupBy(x => x.QuestionType)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns monthly poll summary.
        /// </summary>
        public Dictionary<string, int> GetMonthlyPollSummary(int year)
        {
            return _db.EventPolls
                .Where(x =>
                    x.IsActive &&
                    x.CreatedDate.Year == year)
                .GroupBy(x => x.CreatedDate.Month)
                .OrderBy(x => x.Key)
                .ToDictionary(
                    x => x.Key.ToString(),
                    x => x.Count());
        }

        //=========================================================
        // End Part 6
        //=========================================================

        //=========================================================
        // Part 7 Starts
        // Poll Response CRUD + Verification
        //=========================================================

        /// <summary>
        /// Saves a poll response.
        /// </summary>
        public bool SavePollResponse(EventPollResponseVM vm, int userId)
        {
            if (vm == null)
            {
                return false;
            }

            EventPollResponse entity =
                new EventPollResponse();

            entity.EventPollId = vm.EventPollId;

            entity.EventPollOptionId = vm.EventPollOptionId;

            entity.SurveyPersonMemberCode = vm.SurveyPersonMemberCode;
            entity.SurveyPersonName = vm.SurveyPersonName;

            entity.RespondentName = vm.RespondentName;
            entity.RespondentMobile = vm.RespondentMobile;

            entity.Gender = vm.Gender;
            entity.Age = vm.Age;

            entity.State = vm.State;
            entity.District = vm.District;
            entity.Block = vm.Block;
            entity.Village = vm.Village;
            entity.Booth = vm.Booth;

            entity.Latitude = vm.Latitude;
            entity.Longitude = vm.Longitude;

            entity.ResponseText = vm.ResponseText;

            entity.DeviceId = vm.DeviceId;

            entity.IsVerified = false;

            entity.ResponseDate = DateTime.Now;

            entity.Remarks = vm.Remarks;

            entity.IsActive = true;

            entity.CreatedBy = userId;
            entity.CreatedDate = DateTime.Now;

            _db.EventPollResponses.Add(entity);

            if (vm.EventPollOptionId.HasValue)
            {
                EventPollOption option =
                    _db.EventPollOptions.FirstOrDefault(x =>
                        x.EventPollOptionId == vm.EventPollOptionId.Value);

                if (option != null)
                {
                    option.VoteCount++;
                }
            }

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Updates poll response.
        /// </summary>
        public bool UpdatePollResponse(EventPollResponseVM vm, int userId)
        {
            if (vm == null)
            {
                return false;
            }

            EventPollResponse entity =
                _db.EventPollResponses
                .FirstOrDefault(x =>
                    x.EventPollResponseId == vm.EventPollResponseId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.EventPollOptionId = vm.EventPollOptionId;

            entity.RespondentName = vm.RespondentName;
            entity.RespondentMobile = vm.RespondentMobile;

            entity.Gender = vm.Gender;
            entity.Age = vm.Age;

            entity.State = vm.State;
            entity.District = vm.District;
            entity.Block = vm.Block;
            entity.Village = vm.Village;
            entity.Booth = vm.Booth;

            entity.Latitude = vm.Latitude;
            entity.Longitude = vm.Longitude;

            entity.ResponseText = vm.ResponseText;

            entity.Remarks = vm.Remarks;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Verifies poll response.
        /// </summary>
        public bool VerifyPollResponse(int eventPollResponseId, int userId)
        {
            EventPollResponse entity =
                _db.EventPollResponses
                .FirstOrDefault(x =>
                    x.EventPollResponseId == eventPollResponseId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.IsVerified = true;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Un-verifies poll response.
        /// </summary>
        public bool UnVerifyPollResponse(int eventPollResponseId, int userId)
        {
            EventPollResponse entity =
                _db.EventPollResponses
                .FirstOrDefault(x =>
                    x.EventPollResponseId == eventPollResponseId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.IsVerified = false;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Soft deletes poll response.
        /// </summary>
        public bool DeletePollResponse(int eventPollResponseId, int userId)
        {
            EventPollResponse entity =
                _db.EventPollResponses
                .FirstOrDefault(x =>
                    x.EventPollResponseId == eventPollResponseId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            if (entity.EventPollOptionId.HasValue)
            {
                EventPollOption option =
                    _db.EventPollOptions.FirstOrDefault(x =>
                        x.EventPollOptionId == entity.EventPollOptionId.Value);

                if (option != null &&
                    option.VoteCount > 0)
                {
                    option.VoteCount--;
                }
            }

            entity.IsActive = false;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        //=========================================================
        // End Part 7
        //=========================================================


        //=========================================================
        // Part 8 Starts
        // Poll Response Analytics + Reports
        //=========================================================

        /// <summary>
        /// Returns response summary by gender.
        /// </summary>
        public Dictionary<string, int> GetResponseGenderSummary()
        {
            return _db.EventPollResponses
                .Where(x => x.IsActive)
                .GroupBy(x => x.Gender)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns response summary by district.
        /// </summary>
        public Dictionary<string, int> GetResponseDistrictSummary()
        {
            return _db.EventPollResponses
                .Where(x => x.IsActive)
                .GroupBy(x => x.District)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns response summary by block.
        /// </summary>
        public Dictionary<string, int> GetResponseBlockSummary()
        {
            return _db.EventPollResponses
                .Where(x => x.IsActive)
                .GroupBy(x => x.Block)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns response summary by village.
        /// </summary>
        public Dictionary<string, int> GetResponseVillageSummary()
        {
            return _db.EventPollResponses
                .Where(x => x.IsActive)
                .GroupBy(x => x.Village)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns response summary by booth.
        /// </summary>
        public Dictionary<string, int> GetResponseBoothSummary()
        {
            return _db.EventPollResponses
                .Where(x => x.IsActive)
                .GroupBy(x => x.Booth)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns survey person performance summary.
        /// </summary>
        public Dictionary<string, int> GetSurveyPersonSummary()
        {
            return _db.EventPollResponses
                .Where(x => x.IsActive)
                .GroupBy(x => x.SurveyPersonName)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns monthly response summary.
        /// </summary>
        public Dictionary<string, int> GetMonthlyPollResponseSummary(int year)
        {
            return _db.EventPollResponses
                .Where(x =>
                    x.IsActive &&
                    x.ResponseDate.Year == year)
                .GroupBy(x => x.ResponseDate.Month)
                .OrderBy(x => x.Key)
                .ToDictionary(
                    x => x.Key.ToString(),
                    x => x.Count());
        }

        /// <summary>
        /// Returns today's response count.
        /// </summary>
        public int GetTodayPollResponseCount()
        {
            DateTime today = DateTime.Today;

            return _db.EventPollResponses.Count(x =>
                x.IsActive &&
                x.ResponseDate.Year == today.Year &&
                x.ResponseDate.Month == today.Month &&
                x.ResponseDate.Day == today.Day);
        }

        /// <summary>
        /// Returns option wise vote summary.
        /// </summary>
        public Dictionary<string, int> GetPollOptionVoteSummary(int eventPollId)
        {
            return _db.EventPollOptions
                .Where(x =>
                    x.IsActive &&
                    x.EventPollId == eventPollId)
                .OrderBy(x => x.DisplayOrder)
                .ToDictionary(
                    x => x.OptionText,
                    x => x.VoteCount);
        }

        /// <summary>
        /// Returns top survey persons.
        /// </summary>
        public List<EventPollResponseVM> GetTopSurveyResponses(int count)
        {
            return _db.EventPollResponses
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.ResponseDate)
                .Take(count)
                .ToList()
                .Select(MapPollResponse)
                .ToList();
        }

        //=========================================================
        // End Part 8
        //=========================================================

        #endregion
        //=========================================================
        // Part 1 Starts
        // Booth Mapping
        //=========================================================

        #region Election Booth

        /// <summary>
        /// Maps ElectionBooth Entity to ViewModel.
        /// Used by all Booth queries.
        /// </summary>
        private ElectionBoothVM MapBooth(ElectionBooth entity)
        {
            if (entity == null)
            {
                return null;
            }

            ElectionBoothVM vm =
                new ElectionBoothVM();

            vm.ElectionBoothId = entity.ElectionBoothId;

            vm.BoothCode = entity.BoothCode;
            vm.BoothNumber = entity.BoothNumber;
            vm.BoothName = entity.BoothName;

            vm.AssemblyName = entity.AssemblyName;
            vm.ParliamentName = entity.ParliamentName;

            vm.State = entity.State;
            vm.District = entity.District;
            vm.Block = entity.Block;
            vm.Village = entity.Village;

            vm.PollingStation = entity.PollingStation;

            vm.BoothInchargeMemberCode =
                entity.BoothInchargeMemberCode;

            vm.BoothInchargeName =
                entity.BoothInchargeName;

            vm.TotalVoters = entity.TotalVoters;

            vm.MaleVoters = entity.MaleVoters;
            vm.FemaleVoters = entity.FemaleVoters;
            vm.OtherVoters = entity.OtherVoters;

            vm.Supporters = entity.Supporters;
            vm.Opponents = entity.Opponents;
            vm.NeutralVoters = entity.NeutralVoters;

            vm.VisitedHouses = entity.VisitedHouses;
            vm.TotalHouses = entity.TotalHouses;

            vm.CoveragePercentage =
                entity.CoveragePercentage;

            vm.BoothStrength =
                entity.BoothStrength;

            vm.LastVisitDate =
                entity.LastVisitDate;

            vm.LastMeetingDate =
                entity.LastMeetingDate;

            vm.LastSurveyDate =
                entity.LastSurveyDate;

            vm.Priority =
                entity.Priority;

            switch (entity.Priority)
            {
                case "High":
                    vm.PriorityColor = "danger";
                    break;

                case "Medium":
                    vm.PriorityColor = "warning";
                    break;

                case "Low":
                    vm.PriorityColor = "success";
                    break;

                default:
                    vm.PriorityColor = "secondary";
                    break;
            }

            vm.Remarks =
                entity.Remarks;

            vm.IsActive =
                entity.IsActive;

            vm.CreatedBy =
                entity.CreatedBy;

            vm.CreatedDate =
                entity.CreatedDate;

            vm.UpdatedBy =
                entity.UpdatedBy;

            vm.UpdatedDate =
                entity.UpdatedDate;

            if (entity.ElectionBoothVisits != null)
            {
                vm.VisitCount =
                    entity.ElectionBoothVisits
                    .Count(x => x.IsActive);
            }

            if (entity.TotalVoters > 0)
            {
                vm.SupportPercentage =
                    Math.Round(
                        ((decimal)entity.Supporters /
                        entity.TotalVoters) * 100,
                        2);

                vm.OppositionPercentage =
                    Math.Round(
                        ((decimal)entity.Opponents /
                        entity.TotalVoters) * 100,
                        2);

                vm.NeutralPercentage =
                    Math.Round(
                        ((decimal)entity.NeutralVoters /
                        entity.TotalVoters) * 100,
                        2);
            }

            return vm;
        }

        /// <summary>
        /// Maps Booth Visit Entity to ViewModel.
        /// Used by all Booth Visit queries.
        /// </summary>
        private ElectionBoothVisitVM MapBoothVisit(ElectionBoothVisit entity)
        {
            if (entity == null)
            {
                return null;
            }

            ElectionBoothVisitVM vm =
                new ElectionBoothVisitVM();

            vm.ElectionBoothVisitId =
                entity.ElectionBoothVisitId;

            vm.ElectionBoothId =
                entity.ElectionBoothId;

            if (entity.ElectionBooth != null)
            {
                vm.BoothName =
                    entity.ElectionBooth.BoothName;
            }

            vm.VisitType =
                entity.VisitType;

            vm.VisitDate =
                entity.VisitDate;

            vm.VisitorMemberCode =
                entity.VisitorMemberCode;

            vm.VisitorName =
                entity.VisitorName;

            vm.TeamName =
                entity.TeamName;

            vm.TotalTeamMembers =
                entity.TotalTeamMembers;

            vm.HousesVisited =
                entity.HousesVisited;

            vm.FamiliesMet =
                entity.FamiliesMet;

            vm.PersonsMet =
                entity.PersonsMet;

            vm.NewSupporters =
                entity.NewSupporters;

            vm.OppositionSupporters =
                entity.OppositionSupporters;

            vm.NeutralFamilies =
                entity.NeutralFamilies;

            vm.PamphletsDistributed =
                entity.PamphletsDistributed;

            vm.MembershipFormsIssued =
                entity.MembershipFormsIssued;

            vm.MembershipFormsCollected =
                entity.MembershipFormsCollected;

            vm.PublicComplaints =
                entity.PublicComplaints;

            vm.FollowUpRequired =
                entity.FollowUpRequired;

            vm.NextVisitDate =
                entity.NextVisitDate;

            vm.VisitStatus =
                entity.VisitStatus;

            switch (entity.VisitStatus)
            {
                case "Completed":
                    vm.VisitStatusColor = "success";
                    break;

                case "Pending":
                    vm.VisitStatusColor = "warning";
                    break;

                case "Cancelled":
                    vm.VisitStatusColor = "danger";
                    break;

                case "Scheduled":
                    vm.VisitStatusColor = "primary";
                    break;

                default:
                    vm.VisitStatusColor = "secondary";
                    break;
            }

            vm.Latitude =
                entity.Latitude;

            vm.Longitude =
                entity.Longitude;

            vm.VisitSummary =
                entity.VisitSummary;

            vm.Remarks =
                entity.Remarks;

            vm.IsActive =
                entity.IsActive;

            vm.CreatedBy =
                entity.CreatedBy;

            vm.CreatedDate =
                entity.CreatedDate;

            vm.UpdatedBy =
                entity.UpdatedBy;

            vm.UpdatedDate =
                entity.UpdatedDate;

            return vm;
        }

        

        //=========================================================
        // End Part 1
        //=========================================================

        //=========================================================
        // Part 2 Starts
        // Booth Read Methods
        //=========================================================

        /// <summary>
        /// Returns all active booths.
        /// </summary>
        public List<ElectionBoothVM> GetBooths()
        {
            return _db.ElectionBooths
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.BoothNumber)
                .ThenByDescending(x => x.ElectionBoothId)
                .ToList()
                .Select(MapBooth)
                .ToList();
        }

        /// <summary>
        /// Returns booth by Id.
        /// </summary>
        public ElectionBoothVM GetBoothById(int electionBoothId)
        {
            ElectionBooth entity =
                _db.ElectionBooths
                .AsNoTracking()
                .FirstOrDefault(x =>
                    x.ElectionBoothId == electionBoothId &&
                    x.IsActive);

            return MapBooth(entity);
        }

        /// <summary>
        /// Returns booth by booth code.
        /// </summary>
        public ElectionBoothVM GetBoothByCode(string boothCode)
        {
            ElectionBooth entity =
                _db.ElectionBooths
                .AsNoTracking()
                .FirstOrDefault(x =>
                    x.BoothCode == boothCode &&
                    x.IsActive);

            return MapBooth(entity);
        }

        /// <summary>
        /// Returns booths by district.
        /// </summary>
        public List<ElectionBoothVM> GetBoothsByDistrict(string district)
        {
            return _db.ElectionBooths
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.District == district)
                .OrderBy(x => x.BoothNumber)
                .ToList()
                .Select(MapBooth)
                .ToList();
        }

        /// <summary>
        /// Returns booths by block.
        /// </summary>
        public List<ElectionBoothVM> GetBoothsByBlock(string block)
        {
            return _db.ElectionBooths
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.Block == block)
                .OrderBy(x => x.BoothNumber)
                .ToList()
                .Select(MapBooth)
                .ToList();
        }

        /// <summary>
        /// Returns booths by village.
        /// </summary>
        public List<ElectionBoothVM> GetBoothsByVillage(string village)
        {
            return _db.ElectionBooths
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.Village == village)
                .OrderBy(x => x.BoothNumber)
                .ToList()
                .Select(MapBooth)
                .ToList();
        }

        /// <summary>
        /// Returns booths by priority.
        /// </summary>
        public List<ElectionBoothVM> GetBoothsByPriority(string priority)
        {
            return _db.ElectionBooths
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.Priority == priority)
                .OrderBy(x => x.BoothNumber)
                .ToList()
                .Select(MapBooth)
                .ToList();
        }

        //
        // Booth Visit Read Methods
        //

        /// <summary>
        /// Returns booth visits.
        /// </summary>
        public List<ElectionBoothVisitVM> GetBoothVisits(int electionBoothId)
        {
            return _db.ElectionBoothVisits
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.ElectionBoothId == electionBoothId)
                .OrderByDescending(x => x.VisitDate)
                .ThenByDescending(x => x.ElectionBoothVisitId)
                .ToList()
                .Select(MapBoothVisit)
                .ToList();
        }

        /// <summary>
        /// Returns booth visit by Id.
        /// </summary>
        public ElectionBoothVisitVM GetBoothVisitById(int electionBoothVisitId)
        {
            ElectionBoothVisit entity =
                _db.ElectionBoothVisits
                .AsNoTracking()
                .FirstOrDefault(x =>
                    x.ElectionBoothVisitId == electionBoothVisitId &&
                    x.IsActive);

            return MapBoothVisit(entity);
        }

        /// <summary>
        /// Returns booth visits by visitor.
        /// </summary>
        public List<ElectionBoothVisitVM> GetBoothVisitsByMember(string memberCode)
        {
            return _db.ElectionBoothVisits
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.VisitorMemberCode == memberCode)
                .OrderByDescending(x => x.VisitDate)
                .ToList()
                .Select(MapBoothVisit)
                .ToList();
        }

        /// <summary>
        /// Returns booth visits by status.
        /// </summary>
        public List<ElectionBoothVisitVM> GetBoothVisitsByStatus(string visitStatus)
        {
            return _db.ElectionBoothVisits
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.VisitStatus == visitStatus)
                .OrderByDescending(x => x.VisitDate)
                .ToList()
                .Select(MapBoothVisit)
                .ToList();
        }

        //=========================================================
        // End Part 2
        //=========================================================

        //=========================================================
        // Part 3 Starts
        // Booth Search + Dashboard
        //=========================================================

        /// <summary>
        /// Searches booths.
        /// </summary>
        public List<ElectionBoothVM> SearchBooths(string keyword)
        {
            IQueryable<ElectionBooth> query =
                _db.ElectionBooths
                .AsNoTracking()
                .Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                query = query.Where(x =>

                    (x.BoothCode != null &&
                     x.BoothCode.Contains(keyword))

                    ||

                    (x.BoothNumber != null &&
                     x.BoothNumber.Contains(keyword))

                    ||

                    (x.BoothName != null &&
                     x.BoothName.Contains(keyword))

                    ||

                    (x.AssemblyName != null &&
                     x.AssemblyName.Contains(keyword))

                    ||

                    (x.ParliamentName != null &&
                     x.ParliamentName.Contains(keyword))

                    ||

                    (x.State != null &&
                     x.State.Contains(keyword))

                    ||

                    (x.District != null &&
                     x.District.Contains(keyword))

                    ||

                    (x.Block != null &&
                     x.Block.Contains(keyword))

                    ||

                    (x.Village != null &&
                     x.Village.Contains(keyword))

                    ||

                    (x.PollingStation != null &&
                     x.PollingStation.Contains(keyword))

                    ||

                    (x.BoothInchargeName != null &&
                     x.BoothInchargeName.Contains(keyword))

                    ||

                    (x.Priority != null &&
                     x.Priority.Contains(keyword))

                    ||

                    (x.BoothStrength != null &&
                     x.BoothStrength.Contains(keyword)));
            }

            return query
                .OrderBy(x => x.BoothNumber)
                .ThenByDescending(x => x.ElectionBoothId)
                .ToList()
                .Select(MapBooth)
                .ToList();
        }

        /// <summary>
        /// Searches booth visits.
        /// </summary>
        public List<ElectionBoothVisitVM> SearchBoothVisits(string keyword)
        {
            IQueryable<ElectionBoothVisit> query =
                _db.ElectionBoothVisits
                .AsNoTracking()
                .Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                query = query.Where(x =>

                    (x.VisitType != null &&
                     x.VisitType.Contains(keyword))

                    ||

                    (x.VisitorName != null &&
                     x.VisitorName.Contains(keyword))

                    ||

                    (x.VisitorMemberCode != null &&
                     x.VisitorMemberCode.Contains(keyword))

                    ||

                    (x.TeamName != null &&
                     x.TeamName.Contains(keyword))

                    ||

                    (x.VisitStatus != null &&
                     x.VisitStatus.Contains(keyword))

                    ||

                    (x.VisitSummary != null &&
                     x.VisitSummary.Contains(keyword))

                    ||

                    (x.Remarks != null &&
                     x.Remarks.Contains(keyword))

                    ||

                    (x.ElectionBooth.BoothName != null &&
                     x.ElectionBooth.BoothName.Contains(keyword))

                    ||

                    (x.ElectionBooth.BoothCode != null &&
                     x.ElectionBooth.BoothCode.Contains(keyword)));
            }

            return query
                .OrderByDescending(x => x.VisitDate)
                .ThenByDescending(x => x.ElectionBoothVisitId)
                .ToList()
                .Select(MapBoothVisit)
                .ToList();
        }

        /// <summary>
        /// Returns recent booths.
        /// </summary>
        public List<ElectionBoothVM> GetRecentBooths(int count)
        {
            return _db.ElectionBooths
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CreatedDate)
                .Take(count)
                .ToList()
                .Select(MapBooth)
                .ToList();
        }

        /// <summary>
        /// Returns dashboard booths.
        /// </summary>
        public List<ElectionBoothVM> GetDashboardBooths(int count)
        {
            return _db.ElectionBooths
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CoveragePercentage)
                .ThenBy(x => x.Priority)
                .Take(count)
                .ToList()
                .Select(MapBooth)
                .ToList();
        }

        /// <summary>
        /// Returns recent booth visits.
        /// </summary>
        public List<ElectionBoothVisitVM> GetRecentBoothVisits(int count)
        {
            return _db.ElectionBoothVisits
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.VisitDate)
                .Take(count)
                .ToList()
                .Select(MapBoothVisit)
                .ToList();
        }

        /// <summary>
        /// Returns upcoming booth visits.
        /// </summary>
        public List<ElectionBoothVisitVM> GetUpcomingBoothVisits()
        {
            DateTime today = DateTime.Today;

            return _db.ElectionBoothVisits
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.NextVisitDate.HasValue &&
                    x.NextVisitDate.Value >= today)
                .OrderBy(x => x.NextVisitDate)
                .ToList()
                .Select(MapBoothVisit)
                .ToList();
        }

        /// <summary>
        /// Returns booth visits requiring follow-up.
        /// </summary>
        public List<ElectionBoothVisitVM> GetFollowUpBoothVisits()
        {
            return _db.ElectionBoothVisits
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.FollowUpRequired)
                .OrderBy(x => x.NextVisitDate)
                .ThenByDescending(x => x.VisitDate)
                .ToList()
                .Select(MapBoothVisit)
                .ToList();
        }

        //=========================================================
        // End Part 3
        //=========================================================
        //=========================================================
        // Part 4 Starts
        // Booth CRUD
        //=========================================================

        /// <summary>
        /// Saves a new booth.
        /// </summary>
        public bool SaveBooth(ElectionBoothVM vm, int userId)
        {
            if (vm == null)
            {
                return false;
            }

            ElectionBooth entity = new ElectionBooth();

            entity.BoothCode = vm.BoothCode;
            entity.BoothNumber = vm.BoothNumber;
            entity.BoothName = vm.BoothName;

            entity.AssemblyName = vm.AssemblyName;
            entity.ParliamentName = vm.ParliamentName;

            entity.State = vm.State;
            entity.District = vm.District;
            entity.Block = vm.Block;
            entity.Village = vm.Village;

            entity.PollingStation = vm.PollingStation;

            entity.BoothInchargeMemberCode = vm.BoothInchargeMemberCode;
            entity.BoothInchargeName = vm.BoothInchargeName;

            entity.TotalVoters = vm.TotalVoters;
            entity.MaleVoters = vm.MaleVoters;
            entity.FemaleVoters = vm.FemaleVoters;
            entity.OtherVoters = vm.OtherVoters;

            entity.Supporters = vm.Supporters;
            entity.Opponents = vm.Opponents;
            entity.NeutralVoters = vm.NeutralVoters;

            entity.VisitedHouses = vm.VisitedHouses;
            entity.TotalHouses = vm.TotalHouses;

            entity.CoveragePercentage = vm.CoveragePercentage;

            entity.BoothStrength = vm.BoothStrength;

            entity.LastVisitDate = vm.LastVisitDate;
            entity.LastMeetingDate = vm.LastMeetingDate;
            entity.LastSurveyDate = vm.LastSurveyDate;

            entity.Priority = vm.Priority;

            entity.Remarks = vm.Remarks;

            entity.IsActive = true;

            entity.CreatedBy = userId;
            entity.CreatedDate = DateTime.Now;

            _db.ElectionBooths.Add(entity);

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Updates booth.
        /// </summary>
        public bool UpdateBooth(ElectionBoothVM vm, int userId)
        {
            if (vm == null)
            {
                return false;
            }

            ElectionBooth entity =
                _db.ElectionBooths
                .FirstOrDefault(x =>
                    x.ElectionBoothId == vm.ElectionBoothId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.BoothCode = vm.BoothCode;
            entity.BoothNumber = vm.BoothNumber;
            entity.BoothName = vm.BoothName;

            entity.AssemblyName = vm.AssemblyName;
            entity.ParliamentName = vm.ParliamentName;

            entity.State = vm.State;
            entity.District = vm.District;
            entity.Block = vm.Block;
            entity.Village = vm.Village;

            entity.PollingStation = vm.PollingStation;

            entity.BoothInchargeMemberCode = vm.BoothInchargeMemberCode;
            entity.BoothInchargeName = vm.BoothInchargeName;

            entity.TotalVoters = vm.TotalVoters;
            entity.MaleVoters = vm.MaleVoters;
            entity.FemaleVoters = vm.FemaleVoters;
            entity.OtherVoters = vm.OtherVoters;

            entity.Supporters = vm.Supporters;
            entity.Opponents = vm.Opponents;
            entity.NeutralVoters = vm.NeutralVoters;

            entity.VisitedHouses = vm.VisitedHouses;
            entity.TotalHouses = vm.TotalHouses;

            entity.CoveragePercentage = vm.CoveragePercentage;

            entity.BoothStrength = vm.BoothStrength;

            entity.LastVisitDate = vm.LastVisitDate;
            entity.LastMeetingDate = vm.LastMeetingDate;
            entity.LastSurveyDate = vm.LastSurveyDate;

            entity.Priority = vm.Priority;

            entity.Remarks = vm.Remarks;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Soft deletes booth.
        /// </summary>
        public bool DeleteBooth(int electionBoothId, int userId)
        {
            ElectionBooth entity =
                _db.ElectionBooths
                .FirstOrDefault(x =>
                    x.ElectionBoothId == electionBoothId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.IsActive = false;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        //=========================================================
        // End Part 4
        //=========================================================

        //=========================================================
        // Part 5 Starts
        // Booth Workflow
        //=========================================================

        /// <summary>
        /// Updates booth priority.
        /// </summary>
        public bool UpdateBoothPriority(int electionBoothId, string priority, int userId)
        {
            ElectionBooth entity =
                _db.ElectionBooths
                .FirstOrDefault(x =>
                    x.ElectionBoothId == electionBoothId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.Priority = priority;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Updates booth strength.
        /// </summary>
        public bool UpdateBoothStrength(int electionBoothId, string boothStrength, int userId)
        {
            ElectionBooth entity =
                _db.ElectionBooths
                .FirstOrDefault(x =>
                    x.ElectionBoothId == electionBoothId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.BoothStrength = boothStrength;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Updates booth survey date.
        /// </summary>
        public bool UpdateBoothSurveyDate(int electionBoothId, DateTime surveyDate, int userId)
        {
            ElectionBooth entity =
                _db.ElectionBooths
                .FirstOrDefault(x =>
                    x.ElectionBoothId == electionBoothId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.LastSurveyDate = surveyDate;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Updates booth meeting date.
        /// </summary>
        public bool UpdateBoothMeetingDate(int electionBoothId, DateTime meetingDate, int userId)
        {
            ElectionBooth entity =
                _db.ElectionBooths
                .FirstOrDefault(x =>
                    x.ElectionBoothId == electionBoothId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.LastMeetingDate = meetingDate;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Updates booth visit information.
        /// </summary>
        public bool UpdateBoothVisitInformation(
            int electionBoothId,
            DateTime visitDate,
            int visitedHouses,
            int userId)
        {
            ElectionBooth entity =
                _db.ElectionBooths
                .FirstOrDefault(x =>
                    x.ElectionBoothId == electionBoothId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.LastVisitDate = visitDate;

            entity.VisitedHouses = visitedHouses;

            if (entity.TotalHouses > 0)
            {
                entity.CoveragePercentage =
                    Math.Round(
                        ((decimal)visitedHouses /
                        entity.TotalHouses) * 100,
                        2);
            }
            else
            {
                entity.CoveragePercentage = 0;
            }

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Updates booth political statistics.
        /// </summary>
        public bool UpdateBoothPoliticalData(
            int electionBoothId,
            int supporters,
            int opponents,
            int neutralVoters,
            int userId)
        {
            ElectionBooth entity =
                _db.ElectionBooths
                .FirstOrDefault(x =>
                    x.ElectionBoothId == electionBoothId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.Supporters = supporters;
            entity.Opponents = opponents;
            entity.NeutralVoters = neutralVoters;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        //=========================================================
        // End Part 5
        //=========================================================

        //=========================================================
        // Part 6 Starts
        // Booth KPI + Reports
        //=========================================================

        /// <summary>
        /// Returns total booths.
        /// </summary>
        public int GetTotalBooths()
        {
            return _db.ElectionBooths.Count(x => x.IsActive);
        }

        /// <summary>
        /// Returns total booth visits.
        /// </summary>
        public int GetTotalBoothVisits()
        {
            return _db.ElectionBoothVisits.Count(x => x.IsActive);
        }

        /// <summary>
        /// Returns total visited houses.
        /// </summary>
        public int GetTotalVisitedHouses()
        {
            return _db.ElectionBooths
                .Where(x => x.IsActive)
                .Sum(x => (int?)x.VisitedHouses) ?? 0;
        }

        /// <summary>
        /// Returns total houses.
        /// </summary>
        public int GetTotalHouses()
        {
            return _db.ElectionBooths
                .Where(x => x.IsActive)
                .Sum(x => (int?)x.TotalHouses) ?? 0;
        }

        /// <summary>
        /// Returns average booth coverage.
        /// </summary>
        public decimal GetAverageBoothCoverage()
        {
            if (!_db.ElectionBooths.Any(x => x.IsActive))
            {
                return 0;
            }

            var coverage = _db.ElectionBooths
                .Where(x => x.IsActive)
                .Average(x => (decimal?)x.CoveragePercentage) ?? 0;

            return Math.Round(coverage, 2);
        }

        /// <summary>
        /// Returns high priority booths.
        /// </summary>
        public int GetHighPriorityBooths()
        {
            return _db.ElectionBooths.Count(x =>
                x.IsActive &&
                x.Priority == "High");
        }

        /// <summary>
        /// Returns medium priority booths.
        /// </summary>
        public int GetMediumPriorityBooths()
        {
            return _db.ElectionBooths.Count(x =>
                x.IsActive &&
                x.Priority == "Medium");
        }

        /// <summary>
        /// Returns low priority booths.
        /// </summary>
        public int GetLowPriorityBooths()
        {
            return _db.ElectionBooths.Count(x =>
                x.IsActive &&
                x.Priority == "Low");
        }

        /// <summary>
        /// Returns booths requiring follow-up.
        /// </summary>
        public int GetFollowUpBoothCount()
        {
            return _db.ElectionBoothVisits.Count(x =>
                x.IsActive &&
                x.FollowUpRequired);
        }

        /// <summary>
        /// Returns booth count by district.
        /// </summary>
        public Dictionary<string, int> GetBoothDistrictSummary()
        {
            return _db.ElectionBooths
                .Where(x => x.IsActive)
                .GroupBy(x => x.District)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns booth count by assembly.
        /// </summary>
        public Dictionary<string, int> GetAssemblyBoothSummary()
        {
            return _db.ElectionBooths
                .Where(x => x.IsActive)
                .GroupBy(x => x.AssemblyName)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns booth priority summary.
        /// </summary>
        public Dictionary<string, int> GetBoothPrioritySummary()
        {
            return _db.ElectionBooths
                .Where(x => x.IsActive)
                .GroupBy(x => x.Priority)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns booth strength summary.
        /// </summary>
        public Dictionary<string, int> GetBoothStrengthSummary()
        {
            return _db.ElectionBooths
                .Where(x => x.IsActive)
                .GroupBy(x => x.BoothStrength)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns monthly booth creation summary.
        /// </summary>
        public Dictionary<string, int> GetMonthlyBoothSummary(int year)
        {
            return _db.ElectionBooths
                .Where(x =>
                    x.IsActive &&
                    x.CreatedDate.Year == year)
                .GroupBy(x => x.CreatedDate.Month)
                .OrderBy(x => x.Key)
                .ToDictionary(
                    x => x.Key.ToString(),
                    x => x.Count());
        }

        /// <summary>
        /// Returns today's booth visits.
        /// </summary>
        public int GetTodayBoothVisitCount()
        {
            DateTime today = DateTime.Today;

            return _db.ElectionBoothVisits.Count(x =>
                x.IsActive &&
                x.VisitDate.Year == today.Year &&
                x.VisitDate.Month == today.Month &&
                x.VisitDate.Day == today.Day);
        }

        /// <summary>
        /// Returns booths with more than 80% coverage.
        /// </summary>
        public List<ElectionBoothVM> GetHighCoverageBooths()
        {
            return _db.ElectionBooths
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.CoveragePercentage >= 80)
                .OrderByDescending(x => x.CoveragePercentage)
                .ToList()
                .Select(MapBooth)
                .ToList();
        }

        /// <summary>
        /// Returns booths with less than 50% coverage.
        /// </summary>
        public List<ElectionBoothVM> GetLowCoverageBooths()
        {
            return _db.ElectionBooths
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.CoveragePercentage < 50)
                .OrderBy(x => x.CoveragePercentage)
                .ToList()
                .Select(MapBooth)
                .ToList();
        }

        //=========================================================
        // End Part 6
        //=========================================================
        //=========================================================
        // Part 7 Starts
        // Booth Visit CRUD
        //=========================================================

        /// <summary>
        /// Saves booth visit.
        /// </summary>
        public bool SaveBoothVisit(ElectionBoothVisitVM vm, int userId)
        {
            if (vm == null)
            {
                return false;
            }

            ElectionBoothVisit entity =
                new ElectionBoothVisit();

            entity.ElectionBoothId = vm.ElectionBoothId;

            entity.VisitType = vm.VisitType;

            entity.VisitDate = vm.VisitDate;

            entity.VisitorMemberCode = vm.VisitorMemberCode;
            entity.VisitorName = vm.VisitorName;

            entity.TeamName = vm.TeamName;
            entity.TotalTeamMembers = vm.TotalTeamMembers;

            entity.HousesVisited = vm.HousesVisited;
            entity.FamiliesMet = vm.FamiliesMet;
            entity.PersonsMet = vm.PersonsMet;

            entity.NewSupporters = vm.NewSupporters;
            entity.OppositionSupporters = vm.OppositionSupporters;
            entity.NeutralFamilies = vm.NeutralFamilies;

            entity.PamphletsDistributed = vm.PamphletsDistributed;

            entity.MembershipFormsIssued = vm.MembershipFormsIssued;
            entity.MembershipFormsCollected = vm.MembershipFormsCollected;

            entity.PublicComplaints = vm.PublicComplaints;

            entity.FollowUpRequired = vm.FollowUpRequired;

            entity.NextVisitDate = vm.NextVisitDate;

            entity.VisitStatus = vm.VisitStatus;

            entity.Latitude = vm.Latitude;
            entity.Longitude = vm.Longitude;

            entity.VisitSummary = vm.VisitSummary;

            entity.Remarks = vm.Remarks;

            entity.IsActive = true;

            entity.CreatedBy = userId;
            entity.CreatedDate = DateTime.Now;

            _db.ElectionBoothVisits.Add(entity);

            ElectionBooth booth =
                _db.ElectionBooths.FirstOrDefault(x =>
                    x.ElectionBoothId == vm.ElectionBoothId);

            if (booth != null)
            {
                booth.LastVisitDate = vm.VisitDate;

                booth.VisitedHouses += vm.HousesVisited;

                if (booth.TotalHouses > 0)
                {
                    booth.CoveragePercentage =
                        Math.Round(
                            ((decimal)booth.VisitedHouses /
                            booth.TotalHouses) * 100,
                            2);
                }
            }

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Updates booth visit.
        /// </summary>
        public bool UpdateBoothVisit(ElectionBoothVisitVM vm, int userId)
        {
            if (vm == null)
            {
                return false;
            }

            ElectionBoothVisit entity =
                _db.ElectionBoothVisits
                .FirstOrDefault(x =>
                    x.ElectionBoothVisitId == vm.ElectionBoothVisitId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.VisitType = vm.VisitType;

            entity.VisitDate = vm.VisitDate;

            entity.VisitorMemberCode = vm.VisitorMemberCode;
            entity.VisitorName = vm.VisitorName;

            entity.TeamName = vm.TeamName;
            entity.TotalTeamMembers = vm.TotalTeamMembers;

            entity.HousesVisited = vm.HousesVisited;
            entity.FamiliesMet = vm.FamiliesMet;
            entity.PersonsMet = vm.PersonsMet;

            entity.NewSupporters = vm.NewSupporters;
            entity.OppositionSupporters = vm.OppositionSupporters;
            entity.NeutralFamilies = vm.NeutralFamilies;

            entity.PamphletsDistributed = vm.PamphletsDistributed;

            entity.MembershipFormsIssued = vm.MembershipFormsIssued;
            entity.MembershipFormsCollected = vm.MembershipFormsCollected;

            entity.PublicComplaints = vm.PublicComplaints;

            entity.FollowUpRequired = vm.FollowUpRequired;

            entity.NextVisitDate = vm.NextVisitDate;

            entity.VisitStatus = vm.VisitStatus;

            entity.Latitude = vm.Latitude;
            entity.Longitude = vm.Longitude;

            entity.VisitSummary = vm.VisitSummary;

            entity.Remarks = vm.Remarks;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Soft deletes booth visit.
        /// </summary>
        public bool DeleteBoothVisit(int electionBoothVisitId, int userId)
        {
            ElectionBoothVisit entity =
                _db.ElectionBoothVisits
                .FirstOrDefault(x =>
                    x.ElectionBoothVisitId == electionBoothVisitId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.IsActive = false;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        //=========================================================
        // End Part 7
        //=========================================================

        //=========================================================
        // Part 8 Starts
        // Booth Visit Analytics + Reports
        //=========================================================

        /// <summary>
        /// Returns visit summary by visitor.
        /// </summary>
        public Dictionary<string, int> GetBoothVisitorSummary()
        {
            return _db.ElectionBoothVisits
                .Where(x => x.IsActive)
                .GroupBy(x => x.VisitorName)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns visit summary by team.
        /// </summary>
        public Dictionary<string, int> GetBoothTeamSummary()
        {
            return _db.ElectionBoothVisits
                .Where(x => x.IsActive)
                .GroupBy(x => x.TeamName)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns visit summary by visit type.
        /// </summary>
        public Dictionary<string, int> GetBoothVisitTypeSummary()
        {
            return _db.ElectionBoothVisits
                .Where(x => x.IsActive)
                .GroupBy(x => x.VisitType)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns visit summary by status.
        /// </summary>
        public Dictionary<string, int> GetBoothVisitStatusSummary()
        {
            return _db.ElectionBoothVisits
                .Where(x => x.IsActive)
                .GroupBy(x => x.VisitStatus)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns monthly booth visit summary.
        /// </summary>
        public Dictionary<string, int> GetMonthlyBoothVisitSummary(int year)
        {
            return _db.ElectionBoothVisits
                .Where(x =>
                    x.IsActive &&
                    x.VisitDate.Year == year)
                .GroupBy(x => x.VisitDate.Month)
                .OrderBy(x => x.Key)
                .ToDictionary(
                    x => x.Key.ToString(),
                    x => x.Count());
        }

        /// <summary>
        /// Returns total new supporters from booth visits.
        /// </summary>
        public int GetTotalNewSupporters()
        {
            return _db.ElectionBoothVisits
                .Where(x => x.IsActive)
                .Sum(x => (int?)x.NewSupporters) ?? 0;
        }

        /// <summary>
        /// Returns total opposition supporters identified.
        /// </summary>
        public int GetTotalOppositionSupporters()
        {
            return _db.ElectionBoothVisits
                .Where(x => x.IsActive)
                .Sum(x => (int?)x.OppositionSupporters) ?? 0;
        }

        /// <summary>
        /// Returns total neutral families.
        /// </summary>
        public int GetTotalNeutralFamilies()
        {
            return _db.ElectionBoothVisits
                .Where(x => x.IsActive)
                .Sum(x => (int?)x.NeutralFamilies) ?? 0;
        }

        /// <summary>
        /// Returns total pamphlets distributed.
        /// </summary>
        public int GetTotalPamphletsDistributed()
        {
            return _db.ElectionBoothVisits
                .Where(x => x.IsActive)
                .Sum(x => (int?)x.PamphletsDistributed) ?? 0;
        }

        /// <summary>
        /// Returns total membership forms issued.
        /// </summary>
        public int GetTotalMembershipFormsIssued()
        {
            return _db.ElectionBoothVisits
                .Where(x => x.IsActive)
                .Sum(x => (int?)x.MembershipFormsIssued) ?? 0;
        }

        /// <summary>
        /// Returns total membership forms collected.
        /// </summary>
        public int GetTotalMembershipFormsCollected()
        {
            return _db.ElectionBoothVisits
                .Where(x => x.IsActive)
                .Sum(x => (int?)x.MembershipFormsCollected) ?? 0;
        }

        /// <summary>
        /// Returns total public complaints.
        /// </summary>
        public int GetTotalPublicComplaints()
        {
            return _db.ElectionBoothVisits
                .Where(x => x.IsActive)
                .Sum(x => (int?)x.PublicComplaints) ?? 0;
        }

        /// <summary>
        /// Returns booths with pending follow-up.
        /// </summary>
        public List<ElectionBoothVisitVM> GetPendingFollowUpVisits()
        {
            return _db.ElectionBoothVisits
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.FollowUpRequired)
                .OrderBy(x => x.NextVisitDate)
                .ThenByDescending(x => x.VisitDate)
                .ToList()
                .Select(MapBoothVisit)
                .ToList();
        }

        /// <summary>
        /// Returns top performing visitors.
        /// </summary>
        public List<ElectionBoothVisitVM> GetTopBoothVisitors(int count)
        {
            return _db.ElectionBoothVisits
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.NewSupporters)
                .ThenByDescending(x => x.FamiliesMet)
                .Take(count)
                .ToList()
                .Select(MapBoothVisit)
                .ToList();
        }

        //=========================================================
        // End Part 8
        //=========================================================
        #endregion



        //=========================================================
        // Part 1 Starts
        // Jan Sampark Mapping
        //=========================================================

        #region Jan Sampark Mapping

        /// <summary>
        /// Maps JanSampark entity to JanSampark ViewModel.
        /// </summary>
        /// <param name="entity">JanSampark entity.</param>
        /// <returns>Mapped ViewModel.</returns>
        private JanSamparkVM MapJanSampark(JanSampark entity)
        {
            if (entity == null)
            {
                return null;
            }

            return new JanSamparkVM
            {
                JanSamparkId = entity.JanSamparkId,

                EventId = entity.EventId,
                ElectionBoothId = entity.ElectionBoothId,

                EventName = entity.EventMaster != null
                                ? entity.EventMaster.EventTitle
                                : string.Empty,

                BoothName = entity.ElectionBooth != null
                                ? entity.ElectionBooth.BoothName
                                : string.Empty,

                CitizenName = entity.CitizenName,
                FatherName = entity.FatherName,
                MobileNumber = entity.MobileNumber,
                Gender = entity.Gender,
                Age = entity.Age,

                State = entity.State,
                District = entity.District,
                Block = entity.Block,
                Village = entity.Village,
                Booth = entity.Booth,
                Address = entity.Address,

                Category = entity.Category,
                Subject = entity.Subject,
                Description = entity.Description,
                Priority = entity.Priority,
                Status = entity.Status,

                AssignedToMemberCode = entity.AssignedToMemberCode,
                AssignedToName = entity.AssignedToName,

                Resolution = entity.Resolution,
                ResolutionDate = entity.ResolutionDate,

                FollowUpRequired = entity.FollowUpRequired,
                FollowUpDate = entity.FollowUpDate,

                AttachmentPath = entity.AttachmentPath,

                Latitude = entity.Latitude,
                Longitude = entity.Longitude,

                IsResolved = entity.IsResolved,

                IsActive = entity.IsActive,

                CreatedBy = entity.CreatedBy,
                CreatedDate = entity.CreatedDate,

                UpdatedBy = entity.UpdatedBy,
                UpdatedDate = entity.UpdatedDate
            };
        }

        /// <summary>
        /// Maps JanSampark ViewModel to existing JanSampark entity.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="model">ViewModel.</param>
        private void MapJanSampark(JanSampark entity, JanSamparkVM model)
        {
            entity.EventId = model.EventId;
            entity.ElectionBoothId = model.ElectionBoothId;

            entity.CitizenName = model.CitizenName;
            entity.FatherName = model.FatherName;
            entity.MobileNumber = model.MobileNumber;
            entity.Gender = model.Gender;
            entity.Age = model.Age;

            entity.State = model.State;
            entity.District = model.District;
            entity.Block = model.Block;
            entity.Village = model.Village;
            entity.Booth = model.Booth;
            entity.Address = model.Address;

            entity.Category = model.Category;
            entity.Subject = model.Subject;
            entity.Description = model.Description;

            entity.Priority = model.Priority;
            entity.Status = model.Status;

            entity.AssignedToMemberCode = model.AssignedToMemberCode;
            entity.AssignedToName = model.AssignedToName;

            entity.Resolution = model.Resolution;
            entity.ResolutionDate = model.ResolutionDate;

            entity.FollowUpRequired = model.FollowUpRequired;
            entity.FollowUpDate = model.FollowUpDate;

            entity.AttachmentPath = model.AttachmentPath;

            entity.Latitude = model.Latitude;
            entity.Longitude = model.Longitude;

            entity.IsResolved = model.IsResolved;

            entity.IsActive = model.IsActive;
        }

        //=========================================================
        // Part 2 Starts
        // Read Methods
        //=========================================================

        /// <summary>
        /// Returns all active Jan Sampark records.
        /// </summary>
        public List<JanSamparkVM> GetJanSampark()
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CreatedDate)
                .ToList()
                .Select(MapJanSampark)
                .ToList();
        }

        /// <summary>
        /// Returns Jan Sampark details by Id.
        /// </summary>
        public JanSamparkVM GetJanSamparkById(int janSamparkId)
        {
            JanSampark entity = _db.JanSamparks
                .AsNoTracking()
                .FirstOrDefault(x =>
                    x.JanSamparkId == janSamparkId &&
                    x.IsActive);

            return MapJanSampark(entity);
        }

        /// <summary>
        /// Returns all records of a specific Event.
        /// </summary>
        public List<JanSamparkVM> GetJanSamparkByEvent(int eventId)
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.EventId == eventId)
                .OrderByDescending(x => x.CreatedDate)
                .ToList()
                .Select(MapJanSampark)
                .ToList();
        }

        /// <summary>
        /// Returns all records of a specific Booth.
        /// </summary>
        public List<JanSamparkVM> GetJanSamparkByBooth(int electionBoothId)
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.ElectionBoothId == electionBoothId)
                .OrderByDescending(x => x.CreatedDate)
                .ToList()
                .Select(MapJanSampark)
                .ToList();
        }

        /// <summary>
        /// Returns records by Status.
        /// </summary>
        public List<JanSamparkVM> GetJanSamparkByStatus(string status)
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.Status == status)
                .OrderByDescending(x => x.CreatedDate)
                .ToList()
                .Select(MapJanSampark)
                .ToList();
        }

        /// <summary>
        /// Returns records by Priority.
        /// </summary>
        public List<JanSamparkVM> GetJanSamparkByPriority(string priority)
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.Priority == priority)
                .OrderByDescending(x => x.CreatedDate)
                .ToList()
                .Select(MapJanSampark)
                .ToList();
        }

        /// <summary>
        /// Returns all resolved complaints.
        /// </summary>
        public List<JanSamparkVM> GetResolvedJanSampark()
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.IsResolved)
                .OrderByDescending(x => x.ResolutionDate)
                .ToList()
                .Select(MapJanSampark)
                .ToList();
        }

        /// <summary>
        /// Returns all pending complaints.
        /// </summary>
        public List<JanSamparkVM> GetPendingJanSampark()
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    !x.IsResolved)
                .OrderByDescending(x => x.CreatedDate)
                .ToList()
                .Select(MapJanSampark)
                .ToList();
        }

        /// <summary>
        /// Returns complaints requiring follow-up.
        /// </summary>
        public List<JanSamparkVM> GetFollowUpJanSampark()
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.FollowUpRequired)
                .OrderBy(x => x.FollowUpDate)
                .ToList()
                .Select(MapJanSampark)
                .ToList();
        }

        /// <summary>
        /// Returns recently created Jan Sampark records.
        /// </summary>
        public List<JanSamparkVM> GetRecentJanSampark(int count)
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CreatedDate)
                .Take(count)
                .ToList()
                .Select(MapJanSampark)
                .ToList();
        }

        //=========================================================
        // End Part 2
        //=========================================================

        //=========================================================
        // Part 3 Starts
        // Search + Dashboard
        //=========================================================

        /// <summary>
        /// Searches Jan Sampark records.
        /// Search by Citizen Name, Mobile Number,
        /// Father Name, Village, District, Subject or Category.
        /// </summary>
        public List<JanSamparkVM> SearchJanSampark(string keyword)
        {
            IQueryable<JanSampark> query = _db.JanSamparks
                .AsNoTracking()
                .Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                query = query.Where(x =>

                    (x.CitizenName != null &&
                     x.CitizenName.Contains(keyword))

                    ||

                    (x.FatherName != null &&
                     x.FatherName.Contains(keyword))

                    ||

                    (x.MobileNumber != null &&
                     x.MobileNumber.Contains(keyword))

                    ||

                    (x.Village != null &&
                     x.Village.Contains(keyword))

                    ||

                    (x.District != null &&
                     x.District.Contains(keyword))

                    ||

                    (x.Subject != null &&
                     x.Subject.Contains(keyword))

                    ||

                    (x.Category != null &&
                     x.Category.Contains(keyword)));
            }

            return query
                .OrderByDescending(x => x.CreatedDate)
                .ToList()
                .Select(MapJanSampark)
                .ToList();
        }

        /// <summary>
        /// Returns today's Jan Sampark records.
        /// </summary>
        public List<JanSamparkVM> GetTodayJanSampark()
        {
            DateTime today = DateTime.Today;

            return _db.JanSamparks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.CreatedDate.Year == today.Year &&
                    x.CreatedDate.Month == today.Month &&
                    x.CreatedDate.Day == today.Day)
                .OrderByDescending(x => x.CreatedDate)
                .ToList()
                .Select(MapJanSampark)
                .ToList();
        }

        /// <summary>
        /// Returns this month's Jan Sampark records.
        /// </summary>
        public List<JanSamparkVM> GetCurrentMonthJanSampark()
        {
            DateTime today = DateTime.Today;

            return _db.JanSamparks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.CreatedDate.Year == today.Year &&
                    x.CreatedDate.Month == today.Month)
                .OrderByDescending(x => x.CreatedDate)
                .ToList()
                .Select(MapJanSampark)
                .ToList();
        }

        /// <summary>
        /// Returns complaints assigned to a member.
        /// </summary>
        public List<JanSamparkVM> GetJanSamparkByAssignedMember(string memberCode)
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.AssignedToMemberCode == memberCode)
                .OrderByDescending(x => x.CreatedDate)
                .ToList()
                .Select(MapJanSampark)
                .ToList();
        }

        /// <summary>
        /// Returns complaints by Category.
        /// </summary>
        public List<JanSamparkVM> GetJanSamparkByCategory(string category)
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.Category == category)
                .OrderByDescending(x => x.CreatedDate)
                .ToList()
                .Select(MapJanSampark)
                .ToList();
        }

        /// <summary>
        /// Returns complaints by District.
        /// </summary>
        public List<JanSamparkVM> GetJanSamparkByDistrict(string district)
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.District == district)
                .OrderByDescending(x => x.CreatedDate)
                .ToList()
                .Select(MapJanSampark)
                .ToList();
        }

        /// <summary>
        /// Returns complaints by Village.
        /// </summary>
        public List<JanSamparkVM> GetJanSamparkByVillage(string village)
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.Village == village)
                .OrderByDescending(x => x.CreatedDate)
                .ToList()
                .Select(MapJanSampark)
                .ToList();
        }

        /// <summary>
        /// Returns high priority complaints.
        /// Dashboard Widget.
        /// </summary>
        public List<JanSamparkVM> GetHighPriorityJanSampark()
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.Priority == "High")
                .OrderByDescending(x => x.CreatedDate)
                .ToList()
                .Select(MapJanSampark)
                .ToList();
        }

        /// <summary>
        /// Returns overdue follow-up complaints.
        /// Dashboard Widget.
        /// </summary>
        public List<JanSamparkVM> GetOverdueFollowUps()
        {
            DateTime today = DateTime.Today;

            return _db.JanSamparks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.FollowUpRequired &&
                    x.FollowUpDate.HasValue &&
                    x.FollowUpDate.Value < today &&
                    !x.IsResolved)
                .OrderBy(x => x.FollowUpDate)
                .ToList()
                .Select(MapJanSampark)
                .ToList();
        }

        //=========================================================
        // End Part 3
        //=========================================================
        //=========================================================
        // Part 4 Starts
        // CRUD
        //=========================================================

        /// <summary>
        /// Creates a new Jan Sampark record.
        /// </summary>
        public bool SaveJanSampark(JanSamparkVM vm, int userId)
        {
            if (vm == null)
            {
                return false;
            }

            JanSampark entity = new JanSampark();

            MapJanSampark(entity, vm);

            entity.CreatedBy = userId;
            entity.CreatedDate = DateTime.Now;

            entity.IsResolved = false;
            entity.IsActive = true;

            _db.JanSamparks.Add(entity);

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Updates an existing Jan Sampark record.
        /// </summary>
        public bool UpdateJanSampark(JanSamparkVM vm, int userId)
        {
            if (vm == null)
            {
                return false;
            }

            JanSampark entity = _db.JanSamparks
                .FirstOrDefault(x =>
                    x.JanSamparkId == vm.JanSamparkId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            MapJanSampark(entity, vm);

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Soft deletes a Jan Sampark record.
        /// </summary>
        public bool DeleteJanSampark(int janSamparkId, int userId)
        {
            JanSampark entity = _db.JanSamparks
                .FirstOrDefault(x =>
                    x.JanSamparkId == janSamparkId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.IsActive = false;
            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Returns true if the complaint already exists.
        /// Duplicate is checked using Citizen Name,
        /// Mobile Number, Subject and Event.
        /// </summary>
        public bool IsDuplicateJanSampark(
            string citizenName,
            string mobileNumber,
            string subject,
            int? eventId,
            int janSamparkId)
        {
            return _db.JanSamparks.Any(x =>
                x.IsActive &&
                x.CitizenName == citizenName &&
                x.MobileNumber == mobileNumber &&
                x.Subject == subject &&
                x.EventId == eventId &&
                x.JanSamparkId != janSamparkId);
        }

        //=========================================================
        // End Part 4
        //=========================================================
        //=========================================================
        // Part 5 Starts
        // Workflow
        //=========================================================

        /// <summary>
        /// Assigns complaint to a team member.
        /// </summary>
        public bool AssignJanSampark(
            int janSamparkId,
            string memberCode,
            string memberName,
            int userId)
        {
            JanSampark entity = _db.JanSamparks
                .FirstOrDefault(x =>
                    x.JanSamparkId == janSamparkId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.AssignedToMemberCode = memberCode;
            entity.AssignedToName = memberName;
            entity.Status = "Assigned";

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Marks complaint as In Progress.
        /// </summary>
        public bool StartJanSamparkWork(
            int janSamparkId,
            int userId)
        {
            JanSampark entity = _db.JanSamparks
                .FirstOrDefault(x =>
                    x.JanSamparkId == janSamparkId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.Status = "In Progress";

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Marks complaint as resolved.
        /// </summary>
        public bool ResolveJanSampark(
            int janSamparkId,
            string resolution,
            int userId)
        {
            JanSampark entity = _db.JanSamparks
                .FirstOrDefault(x =>
                    x.JanSamparkId == janSamparkId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.Resolution = resolution;
            entity.ResolutionDate = DateTime.Now;
            entity.Status = "Resolved";
            entity.IsResolved = true;
            entity.FollowUpRequired = false;
            entity.FollowUpDate = null;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Reopens a resolved complaint.
        /// </summary>
        public bool ReopenJanSampark(
            int janSamparkId,
            int userId)
        {
            JanSampark entity = _db.JanSamparks
                .FirstOrDefault(x =>
                    x.JanSamparkId == janSamparkId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.Status = "Reopened";
            entity.IsResolved = false;
            entity.Resolution = null;
            entity.ResolutionDate = null;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Schedules complaint follow-up.
        /// </summary>
        public bool ScheduleJanSamparkFollowUp(
            int janSamparkId,
            DateTime followUpDate,
            int userId)
        {
            JanSampark entity = _db.JanSamparks
                .FirstOrDefault(x =>
                    x.JanSamparkId == janSamparkId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.FollowUpRequired = true;
            entity.FollowUpDate = followUpDate;
            entity.Status = "Follow Up";

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Completes complaint follow-up.
        /// </summary>
        public bool CompleteJanSamparkFollowUp(
            int janSamparkId,
            int userId)
        {
            JanSampark entity = _db.JanSamparks
                .FirstOrDefault(x =>
                    x.JanSamparkId == janSamparkId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.FollowUpRequired = false;
            entity.FollowUpDate = null;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        //=========================================================
        // End Part 5
        //=========================================================
        //=========================================================
        // Part 6 Starts
        // KPI + Reports
        //=========================================================

        /// <summary>
        /// Returns total active Jan Sampark records.
        /// </summary>
        public int GetTotalJanSampark()
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Count(x => x.IsActive);
        }

        /// <summary>
        /// Returns total resolved complaints.
        /// </summary>
        public int GetResolvedJanSamparkCount()
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Count(x =>
                    x.IsActive &&
                    x.IsResolved);
        }

        /// <summary>
        /// Returns total pending complaints.
        /// </summary>
        public int GetPendingJanSamparkCount()
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Count(x =>
                    x.IsActive &&
                    !x.IsResolved);
        }

        /// <summary>
        /// Returns total follow-up complaints.
        /// </summary>
        public int GetFollowUpJanSamparkCount()
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Count(x =>
                    x.IsActive &&
                    x.FollowUpRequired);
        }

        /// <summary>
        /// Returns total high priority complaints.
        /// </summary>
        public int GetHighPriorityJanSamparkCount()
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Count(x =>
                    x.IsActive &&
                    x.Priority == "High");
        }

        /// <summary>
        /// Returns complaints received today.
        /// </summary>
        public int GetTodayJanSamparkCount()
        {
            DateTime today = DateTime.Today;

            return _db.JanSamparks
                .AsNoTracking()
                .Count(x =>
                    x.IsActive &&
                    x.CreatedDate.Year == today.Year &&
                    x.CreatedDate.Month == today.Month &&
                    x.CreatedDate.Day == today.Day);
        }

        /// <summary>
        /// Returns complaints received this month.
        /// </summary>
        public int GetCurrentMonthJanSamparkCount()
        {
            DateTime today = DateTime.Today;

            return _db.JanSamparks
                .AsNoTracking()
                .Count(x =>
                    x.IsActive &&
                    x.CreatedDate.Year == today.Year &&
                    x.CreatedDate.Month == today.Month);
        }

        /// <summary>
        /// Returns complaints by status.
        /// </summary>
        public Dictionary<string, int> GetJanSamparkStatusSummary()
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Where(x => x.IsActive)
                .GroupBy(x => x.Status)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns complaints by priority.
        /// </summary>
        public Dictionary<string, int> GetJanSamparkPrioritySummary()
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Where(x => x.IsActive)
                .GroupBy(x => x.Priority)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns complaints by category.
        /// </summary>
        public Dictionary<string, int> GetJanSamparkCategorySummary()
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Where(x => x.IsActive)
                .GroupBy(x => x.Category)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns complaints by district.
        /// </summary>
        public Dictionary<string, int> GetJanSamparkDistrictSummary()
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Where(x => x.IsActive)
                .GroupBy(x => x.District)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns complaints by village.
        /// </summary>
        public Dictionary<string, int> GetJanSamparkVillageSummary()
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Where(x => x.IsActive)
                .GroupBy(x => x.Village)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns complaints assigned to each member.
        /// </summary>
        public Dictionary<string, int> GetJanSamparkAssignmentSummary()
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Where(x => x.IsActive)
                .GroupBy(x => x.AssignedToName)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        //=========================================================
        // End Part 6
        //=========================================================
        //=========================================================
        // Part 7 Starts
        // Child / Transaction CRUD
        //=========================================================

        /*
         * No child transaction table exists for JanSampark.
         *
         * All workflow information including:
         *
         * • Assignment
         * • Resolution
         * • Follow-Up
         * • Status
         *
         * is maintained in the JanSampark master table.
         *
         * Therefore, no child CRUD methods are required.
         */

        //=========================================================
        // End Part 7
        //=========================================================
        //=========================================================
        // Part 8 Starts
        // Analytics + Reports
        //=========================================================

        /// <summary>
        /// Returns monthly complaint trend.
        /// </summary>
        public Dictionary<int, int> GetJanSamparkMonthlyAnalytics(int year)
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.CreatedDate.Year == year)
                .GroupBy(x => x.CreatedDate.Month)
                .OrderBy(x => x.Key)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns yearly complaint trend.
        /// </summary>
        public Dictionary<int, int> GetJanSamparkYearlyAnalytics()
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Where(x => x.IsActive)
                .GroupBy(x => x.CreatedDate.Year)
                .OrderBy(x => x.Key)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns resolution percentage.
        /// </summary>
        public decimal GetJanSamparkResolutionPercentage()
        {
            int total = _db.JanSamparks
                .AsNoTracking()
                .Count(x => x.IsActive);

            if (total == 0)
            {
                return 0;
            }

            int resolved = _db.JanSamparks
                .AsNoTracking()
                .Count(x =>
                    x.IsActive &&
                    x.IsResolved);

            return Math.Round(((decimal)resolved * 100) / total, 2);
        }

        /// <summary>
        /// Returns follow-up percentage.
        /// </summary>
        public decimal GetJanSamparkFollowUpPercentage()
        {
            int total = _db.JanSamparks
                .AsNoTracking()
                .Count(x => x.IsActive);

            if (total == 0)
            {
                return 0;
            }

            int followUp = _db.JanSamparks
                .AsNoTracking()
                .Count(x =>
                    x.IsActive &&
                    x.FollowUpRequired);

            return Math.Round(((decimal)followUp * 100) / total, 2);
        }

        /// <summary>
        /// Returns assignment performance.
        /// </summary>
        public Dictionary<string, int> GetJanSamparkAssignmentAnalytics()
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.AssignedToName != null)
                .GroupBy(x => x.AssignedToName)
                .OrderByDescending(x => x.Count())
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns district wise analytics.
        /// </summary>
        public Dictionary<string, int> GetJanSamparkDistrictAnalytics()
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.District != null)
                .GroupBy(x => x.District)
                .OrderByDescending(x => x.Count())
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns village wise analytics.
        /// </summary>
        public Dictionary<string, int> GetJanSamparkVillageAnalytics()
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.Village != null)
                .GroupBy(x => x.Village)
                .OrderByDescending(x => x.Count())
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns category wise analytics.
        /// </summary>
        public Dictionary<string, int> GetJanSamparkCategoryAnalytics()
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.Category != null)
                .GroupBy(x => x.Category)
                .OrderByDescending(x => x.Count())
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns priority wise analytics.
        /// </summary>
        public Dictionary<string, int> GetJanSamparkPriorityAnalytics()
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.Priority != null)
                .GroupBy(x => x.Priority)
                .OrderByDescending(x => x.Count())
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns status wise analytics.
        /// </summary>
        public Dictionary<string, int> GetJanSamparkStatusAnalytics()
        {
            return _db.JanSamparks
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.Status != null)
                .GroupBy(x => x.Status)
                .OrderByDescending(x => x.Count())
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        //=========================================================
        // End Part 8
        //=========================================================

        #endregion

        #region Election Campaign

        //=========================================================
        // Part 1 Starts
        // Mapping
        //=========================================================

        /// <summary>
        /// Maps ElectionCampaign entity to ViewModel.
        /// </summary>
        private ElectionCampaignVM MapElectionCampaign(ElectionCampaign entity)
        {
            if (entity == null)
            {
                return null;
            }

            return new ElectionCampaignVM
            {
                CampaignId = entity.CampaignId,
                CampaignName = entity.CampaignName,
                ElectionType = entity.ElectionType,
                StateId = entity.StateId,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                Status = entity.Status,
                Description = entity.Description,

                IsActive = entity.IsActive,

                CreatedBy = entity.CreatedBy,
                CreatedDate = entity.CreatedDate,

                UpdatedBy = entity.UpdatedBy,
                UpdatedDate = entity.UpdatedDate
            };
        }

        /// <summary>
        /// Maps ViewModel to ElectionCampaign entity.
        /// </summary>
        private void MapElectionCampaign(
            ElectionCampaign entity,
            ElectionCampaignVM model)
        {
            entity.CampaignName = model.CampaignName;
            entity.ElectionType = model.ElectionType;
            entity.StateId = model.StateId;
            entity.StartDate = model.StartDate;
            entity.EndDate = model.EndDate;
            entity.Status = model.Status;
            entity.Description = model.Description;
        }

        //=========================================================
        // End Part 1
        //=========================================================
        //=========================================================
        // Part 2 Starts
        // Read Methods
        //=========================================================

        /// <summary>
        /// Returns all active election campaigns.
        /// </summary>
        public List<ElectionCampaignVM> GetElectionCampaign()
        {
            return _db.ElectionCampaigns
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CreatedDate)
                .ToList()
                .Select(MapElectionCampaign)
                .ToList();
        }

        /// <summary>
        /// Returns campaign by Id.
        /// </summary>
        public ElectionCampaignVM GetElectionCampaignById(int campaignId)
        {
            ElectionCampaign entity = _db.ElectionCampaigns
                .AsNoTracking()
                .FirstOrDefault(x =>
                    x.CampaignId == campaignId &&
                    x.IsActive);

            return MapElectionCampaign(entity);
        }

        /// <summary>
        /// Returns campaigns by status.
        /// </summary>
        public List<ElectionCampaignVM> GetElectionCampaignByStatus(string status)
        {
            return _db.ElectionCampaigns
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.Status == status)
                .OrderByDescending(x => x.CreatedDate)
                .ToList()
                .Select(MapElectionCampaign)
                .ToList();
        }

        /// <summary>
        /// Returns running campaigns.
        /// </summary>
        public List<ElectionCampaignVM> GetRunningElectionCampaigns()
        {
            DateTime today = DateTime.Today;

            return _db.ElectionCampaigns
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.StartDate <= today &&
                    (!x.EndDate.HasValue || x.EndDate >= today))
                .OrderBy(x => x.StartDate)
                .ToList()
                .Select(MapElectionCampaign)
                .ToList();
        }

        /// <summary>
        /// Returns upcoming campaigns.
        /// </summary>
        public List<ElectionCampaignVM> GetUpcomingElectionCampaigns()
        {
            DateTime today = DateTime.Today;

            return _db.ElectionCampaigns
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.StartDate > today)
                .OrderBy(x => x.StartDate)
                .ToList()
                .Select(MapElectionCampaign)
                .ToList();
        }

        /// <summary>
        /// Returns completed campaigns.
        /// </summary>
        public List<ElectionCampaignVM> GetCompletedElectionCampaigns()
        {
            DateTime today = DateTime.Today;

            return _db.ElectionCampaigns
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.EndDate.HasValue &&
                    x.EndDate.Value < today)
                .OrderByDescending(x => x.EndDate)
                .ToList()
                .Select(MapElectionCampaign)
                .ToList();
        }

        /// <summary>
        /// Returns campaigns by election type.
        /// </summary>
        public List<ElectionCampaignVM> GetElectionCampaignByType(string electionType)
        {
            return _db.ElectionCampaigns
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.ElectionType == electionType)
                .OrderByDescending(x => x.CreatedDate)
                .ToList()
                .Select(MapElectionCampaign)
                .ToList();
        }

        /// <summary>
        /// Returns recent campaigns.
        /// </summary>
        public List<ElectionCampaignVM> GetRecentElectionCampaigns(int count)
        {
            return _db.ElectionCampaigns
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CreatedDate)
                .Take(count)
                .ToList()
                .Select(MapElectionCampaign)
                .ToList();
        }

        //=========================================================
        // End Part 2
        //=========================================================

        //=========================================================
        // Part 3 Starts
        // Search + Dashboard
        //=========================================================

        /// <summary>
        /// Searches election campaigns.
        /// </summary>
        public List<ElectionCampaignVM> SearchElectionCampaign(string keyword)
        {
            IQueryable<ElectionCampaign> query = _db.ElectionCampaigns
                .AsNoTracking()
                .Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                query = query.Where(x =>

                    (x.CampaignName != null &&
                     x.CampaignName.Contains(keyword))

                    ||

                    (x.ElectionType != null &&
                     x.ElectionType.Contains(keyword))

                    ||

                    (x.Status != null &&
                     x.Status.Contains(keyword))

                    ||

                    (x.Description != null &&
                     x.Description.Contains(keyword)));
            }

            return query
                .OrderByDescending(x => x.CreatedDate)
                .ToList()
                .Select(MapElectionCampaign)
                .ToList();
        }

        /// <summary>
        /// Returns campaigns by state.
        /// </summary>
        public List<ElectionCampaignVM> GetElectionCampaignByState(int stateId)
        {
            return _db.ElectionCampaigns
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.StateId == stateId)
                .OrderByDescending(x => x.CreatedDate)
                .ToList()
                .Select(MapElectionCampaign)
                .ToList();
        }

        /// <summary>
        /// Returns campaigns created today.
        /// </summary>
        public List<ElectionCampaignVM> GetTodayElectionCampaigns()
        {
            DateTime today = DateTime.Today;

            return _db.ElectionCampaigns
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.CreatedDate.Year == today.Year &&
                    x.CreatedDate.Month == today.Month &&
                    x.CreatedDate.Day == today.Day)
                .OrderByDescending(x => x.CreatedDate)
                .ToList()
                .Select(MapElectionCampaign)
                .ToList();
        }

        /// <summary>
        /// Returns campaigns created in current month.
        /// </summary>
        public List<ElectionCampaignVM> GetCurrentMonthElectionCampaigns()
        {
            DateTime today = DateTime.Today;

            return _db.ElectionCampaigns
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.CreatedDate.Year == today.Year &&
                    x.CreatedDate.Month == today.Month)
                .OrderByDescending(x => x.CreatedDate)
                .ToList()
                .Select(MapElectionCampaign)
                .ToList();
        }

        /// <summary>
        /// Returns active campaigns for dashboard.
        /// </summary>
        public List<ElectionCampaignVM> GetDashboardElectionCampaigns()
        {
            return _db.ElectionCampaigns
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.Status == "Active")
                .OrderBy(x => x.StartDate)
                .ToList()
                .Select(MapElectionCampaign)
                .ToList();
        }

        /// <summary>
        /// Returns expired campaigns.
        /// </summary>
        public List<ElectionCampaignVM> GetExpiredElectionCampaigns()
        {
            DateTime today = DateTime.Today;

            return _db.ElectionCampaigns
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.EndDate.HasValue &&
                    x.EndDate.Value < today)
                .OrderByDescending(x => x.EndDate)
                .ToList()
                .Select(MapElectionCampaign)
                .ToList();
        }

        /// <summary>
        /// Returns campaigns ending soon.
        /// </summary>
        public List<ElectionCampaignVM> GetCampaignsEndingSoon(int days)
        {
            DateTime today = DateTime.Today;
            DateTime endDate = today.AddDays(days);

            return _db.ElectionCampaigns
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.EndDate.HasValue &&
                    x.EndDate.Value >= today &&
                    x.EndDate.Value <= endDate)
                .OrderBy(x => x.EndDate)
                .ToList()
                .Select(MapElectionCampaign)
                .ToList();
        }

        //=========================================================
        // End Part 3
        //=========================================================

        //=========================================================
        // Part 4 Starts
        // CRUD
        //=========================================================

        /// <summary>
        /// Saves a new election campaign.
        /// </summary>
        public bool SaveElectionCampaign(ElectionCampaignVM model, int userId)
        {
            if (model == null)
            {
                return false;
            }

            if (IsDuplicateElectionCampaign(model.CampaignName, model.CampaignId))
            {
                return false;
            }

            ElectionCampaign entity = new ElectionCampaign();

            MapElectionCampaign(entity, model);

            entity.IsActive = true;
            entity.CreatedBy = userId;
            entity.CreatedDate = DateTime.Now;

            _db.ElectionCampaigns.Add(entity);

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Updates an existing election campaign.
        /// </summary>
        public bool UpdateElectionCampaign(ElectionCampaignVM model, int userId)
        {
            if (model == null)
            {
                return false;
            }

            if (IsDuplicateElectionCampaign(model.CampaignName, model.CampaignId))
            {
                return false;
            }

            ElectionCampaign entity = _db.ElectionCampaigns
                .FirstOrDefault(x =>
                    x.CampaignId == model.CampaignId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            MapElectionCampaign(entity, model);

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Soft deletes an election campaign.
        /// </summary>
        public bool DeleteElectionCampaign(int campaignId, int userId)
        {
            ElectionCampaign entity = _db.ElectionCampaigns
                .FirstOrDefault(x =>
                    x.CampaignId == campaignId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.IsActive = false;
            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Checks whether the campaign already exists.
        /// </summary>
        public bool IsDuplicateElectionCampaign(string campaignName, int campaignId)
        {
            return _db.ElectionCampaigns.Any(x =>
                x.IsActive &&
                x.CampaignName == campaignName &&
                x.CampaignId != campaignId);
        }

        //=========================================================
        // End Part 4
        //=========================================================
        //=========================================================
        // Part 5 Starts
        // Workflow
        //=========================================================

        /// <summary>
        /// Starts the campaign.
        /// </summary>
        public bool StartElectionCampaign(int campaignId, int userId)
        {
            ElectionCampaign entity = _db.ElectionCampaigns
                .FirstOrDefault(x =>
                    x.CampaignId == campaignId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.Status = "Running";
            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Completes the campaign.
        /// </summary>
        public bool CompleteElectionCampaign(int campaignId, int userId)
        {
            ElectionCampaign entity = _db.ElectionCampaigns
                .FirstOrDefault(x =>
                    x.CampaignId == campaignId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.Status = "Completed";

            if (!entity.EndDate.HasValue)
            {
                entity.EndDate = DateTime.Now;
            }

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Cancels the campaign.
        /// </summary>
        public bool CancelElectionCampaign(int campaignId, int userId)
        {
            ElectionCampaign entity = _db.ElectionCampaigns
                .FirstOrDefault(x =>
                    x.CampaignId == campaignId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.Status = "Cancelled";
            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Reopens a completed campaign.
        /// </summary>
        public bool ReopenElectionCampaign(int campaignId, int userId)
        {
            ElectionCampaign entity = _db.ElectionCampaigns
                .FirstOrDefault(x =>
                    x.CampaignId == campaignId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.Status = "Running";
            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Activates a campaign.
        /// </summary>
        public bool ActivateElectionCampaign(int campaignId, int userId)
        {
            ElectionCampaign entity = _db.ElectionCampaigns
                .FirstOrDefault(x =>
                    x.CampaignId == campaignId);

            if (entity == null)
            {
                return false;
            }

            entity.IsActive = true;
            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Deactivates a campaign.
        /// Active campaign alerts prevent deactivation.
        /// </summary>
        public bool DeactivateElectionCampaign(int campaignId, int userId)
        {
            ElectionCampaign entity = _db.ElectionCampaigns
                .FirstOrDefault(x =>
                    x.CampaignId == campaignId &&
                    x.IsActive);

            if (entity == null)
            {
                return false;
            }

            bool hasActiveAlerts = _db.CampaignAlerts.Any(x =>
                x.CampaignId == campaignId &&
                x.IsActive);

            if (hasActiveAlerts)
            {
                return false;
            }

            entity.IsActive = false;
            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        //=========================================================
        // End Part 5
        //=========================================================
        //=========================================================
        // Part 6 Starts
        // KPI + Reports
        //=========================================================

        /// <summary>
        /// Returns total active campaigns.
        /// </summary>
        public int GetTotalElectionCampaign()
        {
            return _db.ElectionCampaigns
                .AsNoTracking()
                .Count(x => x.IsActive);
        }

        /// <summary>
        /// Returns total running campaigns.
        /// </summary>
        public int GetRunningElectionCampaignCount()
        {
            return _db.ElectionCampaigns
                .AsNoTracking()
                .Count(x =>
                    x.IsActive &&
                    x.Status == "Running");
        }

        /// <summary>
        /// Returns total completed campaigns.
        /// </summary>
        public int GetCompletedElectionCampaignCount()
        {
            return _db.ElectionCampaigns
                .AsNoTracking()
                .Count(x =>
                    x.IsActive &&
                    x.Status == "Completed");
        }

        /// <summary>
        /// Returns total upcoming campaigns.
        /// </summary>
        public int GetUpcomingElectionCampaignCount()
        {
            DateTime today = DateTime.Today;

            return _db.ElectionCampaigns
                .AsNoTracking()
                .Count(x =>
                    x.IsActive &&
                    x.StartDate > today);
        }

        /// <summary>
        /// Returns total cancelled campaigns.
        /// </summary>
        public int GetCancelledElectionCampaignCount()
        {
            return _db.ElectionCampaigns
                .AsNoTracking()
                .Count(x =>
                    x.IsActive &&
                    x.Status == "Cancelled");
        }

        /// <summary>
        /// Returns total active alerts of all campaigns.
        /// </summary>
        public int GetTotalCampaignAlertCount()
        {
            return _db.CampaignAlerts
                .AsNoTracking()
                .Count(x => x.IsActive);
        }

        /// <summary>
        /// Returns total resolved alerts.
        /// </summary>
        public int GetResolvedCampaignAlertCount()
        {
            return _db.CampaignAlerts
                .AsNoTracking()
                .Count(x =>
                    x.IsActive &&
                    x.IsResolved);
        }

        /// <summary>
        /// Returns campaign status summary.
        /// </summary>
        public Dictionary<string, int> GetElectionCampaignStatusSummary()
        {
            return _db.ElectionCampaigns
                .AsNoTracking()
                .Where(x => x.IsActive)
                .GroupBy(x => x.Status)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns election type summary.
        /// </summary>
        public Dictionary<string, int> GetElectionTypeSummary()
        {
            return _db.ElectionCampaigns
                .AsNoTracking()
                .Where(x => x.IsActive)
                .GroupBy(x => x.ElectionType)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns state wise campaign summary.
        /// </summary>
        public Dictionary<int?, int> GetElectionCampaignStateSummary()
        {
            return _db.ElectionCampaigns
                .AsNoTracking()
                .Where(x => x.IsActive)
                .GroupBy(x => x.StateId)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        //=========================================================
        // End Part 6
        //=========================================================
        //=========================================================
        // Part 7 Starts
        // Campaign Alert Support
        //=========================================================

        /// <summary>
        /// Returns true if campaign has active alerts.
        /// </summary>
        public bool HasActiveCampaignAlerts(int campaignId)
        {
            return _db.CampaignAlerts
                .AsNoTracking()
                .Any(x =>
                    x.CampaignId == campaignId &&
                    x.IsActive);
        }

        /// <summary>
        /// Returns total alerts of a campaign.
        /// </summary>
        public int GetCampaignAlertCount(int campaignId)
        {
            return _db.CampaignAlerts
                .AsNoTracking()
                .Count(x =>
                    x.CampaignId == campaignId &&
                    x.IsActive);
        }

        /// <summary>
        /// Returns total resolved alerts of a campaign.
        /// </summary>
        public int GetResolvedCampaignAlertCount(int campaignId)
        {
            return _db.CampaignAlerts
                .AsNoTracking()
                .Count(x =>
                    x.CampaignId == campaignId &&
                    x.IsActive &&
                    x.IsResolved);
        }

        /// <summary>
        /// Returns total pending alerts of a campaign.
        /// </summary>
        public int GetPendingCampaignAlertCount(int campaignId)
        {
            return _db.CampaignAlerts
                .AsNoTracking()
                .Count(x =>
                    x.CampaignId == campaignId &&
                    x.IsActive &&
                    !x.IsResolved);
        }

        /// <summary>
        /// Returns total dashboard alerts of a campaign.
        /// </summary>
        public int GetDashboardCampaignAlertCount(int campaignId)
        {
            return _db.CampaignAlerts
                .AsNoTracking()
                .Count(x =>
                    x.CampaignId == campaignId &&
                    x.IsActive &&
                    x.IsDashboard);
        }

        /// <summary>
        /// Returns total notification sent alerts of a campaign.
        /// </summary>
        public int GetNotificationSentAlertCount(int campaignId)
        {
            return _db.CampaignAlerts
                .AsNoTracking()
                .Count(x =>
                    x.CampaignId == campaignId &&
                    x.IsActive &&
                    x.IsNotificationSent);
        }

        //=========================================================
        // End Part 7
        //=========================================================
        //=========================================================
        // Part 8 Starts
        // Analytics + Reports
        //=========================================================

        /// <summary>
        /// Returns monthly campaign analytics.
        /// </summary>
        public Dictionary<int, int> GetElectionCampaignMonthlyAnalytics(int year)
        {
            return _db.ElectionCampaigns
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.CreatedDate.Year == year)
                .GroupBy(x => x.CreatedDate.Month)
                .OrderBy(x => x.Key)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns yearly campaign analytics.
        /// </summary>
        public Dictionary<int, int> GetElectionCampaignYearlyAnalytics()
        {
            return _db.ElectionCampaigns
                .AsNoTracking()
                .Where(x => x.IsActive)
                .GroupBy(x => x.CreatedDate.Year)
                .OrderBy(x => x.Key)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns campaign duration analytics.
        /// </summary>
        public Dictionary<string, int> GetCampaignDurationAnalytics()
        {
            return _db.ElectionCampaigns
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.EndDate.HasValue)
                .ToList()
                .GroupBy(x =>
                {
                    int days = (x.EndDate.Value - x.StartDate).Days;

                    if (days <= 30)
                        return "0-30 Days";

                    if (days <= 90)
                        return "31-90 Days";

                    if (days <= 180)
                        return "91-180 Days";

                    return "180+ Days";
                })
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns campaign alert analytics.
        /// </summary>
        public Dictionary<string, int> GetCampaignAlertAnalytics()
        {
            return _db.CampaignAlerts
                .AsNoTracking()
                .Where(x => x.IsActive)
                .GroupBy(x => x.AlertStatus)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns campaign severity analytics.
        /// </summary>
        public Dictionary<string, int> GetCampaignSeverityAnalytics()
        {
            return _db.CampaignAlerts
                .AsNoTracking()
                .Where(x => x.IsActive)
                .GroupBy(x => x.Severity)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());
        }

        /// <summary>
        /// Returns campaign notification analytics.
        /// </summary>
        public Dictionary<string, int> GetCampaignNotificationAnalytics()
        {
            return _db.CampaignAlerts
                .AsNoTracking()
                .Where(x => x.IsActive)
                .GroupBy(x => x.IsNotificationSent)
                .ToDictionary(
                    x => x.Key ? "Sent" : "Pending",
                    x => x.Count());
        }

        /// <summary>
        /// Returns campaign dashboard analytics.
        /// </summary>
        public Dictionary<string, int> GetCampaignDashboardAnalytics()
        {
            return _db.CampaignAlerts
                .AsNoTracking()
                .Where(x => x.IsActive)
                .GroupBy(x => x.IsDashboard)
                .ToDictionary(
                    x => x.Key ? "Dashboard" : "Hidden",
                    x => x.Count());
        }

        /// <summary>
        /// Returns campaign read analytics.
        /// </summary>
        public Dictionary<string, int> GetCampaignReadAnalytics()
        {
            return _db.CampaignAlerts
                .AsNoTracking()
                .Where(x => x.IsActive)
                .GroupBy(x => x.IsRead)
                .ToDictionary(
                    x => x.Key ? "Read" : "Unread",
                    x => x.Count());
        }

        /// <summary>
        /// Returns campaign resolution percentage.
        /// </summary>
        public decimal GetCampaignResolutionPercentage()
        {
            int total = _db.CampaignAlerts
                .AsNoTracking()
                .Count(x => x.IsActive);

            if (total == 0)
            {
                return 0;
            }

            int resolved = _db.CampaignAlerts
                .AsNoTracking()
                .Count(x =>
                    x.IsActive &&
                    x.IsResolved);

            return Math.Round(((decimal)resolved * 100) / total, 2);
        }

        //=========================================================
        // End Part 8
        //=========================================================

        #endregion
        #region Campaign Alert

        //=========================================================
        // Part 1 Starts
        // Campaign Alert Mapping Methods
        //=========================================================

        /// <summary>
        /// Maps CampaignAlert entity to CampaignAlertVM.
        /// </summary>
        /// <param name="entity">Campaign Alert entity.</param>
        /// <returns>Campaign Alert ViewModel.</returns>
        private CampaignAlertVM MapCampaignAlert(CampaignAlert entity)
        {
            if (entity == null)
            {
                return null;
            }

            CampaignAlertVM model = new CampaignAlertVM();

            MapCampaignAlert(entity, model);

            return model;
        }

        /// <summary>
        /// Maps CampaignAlert entity values to CampaignAlertVM.
        /// </summary>
        /// <param name="entity">Campaign Alert entity.</param>
        /// <param name="model">Campaign Alert ViewModel.</param>
        private void MapCampaignAlert(
            CampaignAlert entity,
            CampaignAlertVM model)
        {
            if (entity == null)
            {
                return;
            }

            if (model == null)
            {
                return;
            }

            model.CampaignAlertId = entity.CampaignAlertId;

            model.CampaignId = entity.CampaignId;

            model.CampaignName = entity.ElectionCampaign != null
                ? entity.ElectionCampaign.CampaignName
                : String.Empty;

            model.AlertTitle = entity.AlertTitle;

            model.AlertMessage = entity.AlertMessage;

            model.AlertType = entity.AlertType;

            model.ReferenceModule = entity.ReferenceModule;

            model.ReferenceId = entity.ReferenceId;

            model.Severity = entity.Severity;

            model.AlertStatus = entity.AlertStatus;

            model.AssignedToMemberCode = entity.AssignedToMemberCode;

            model.AssignedToName = entity.AssignedToName;

            model.ActionTaken = entity.ActionTaken;

            model.ActionTakenByMemberCode = entity.ActionTakenByMemberCode;

            model.ActionTakenDate = entity.ActionTakenDate;

            model.ExpiryDate = entity.ExpiryDate;

            model.IsDashboard = entity.IsDashboard;

            model.IsNotificationSent = entity.IsNotificationSent;

            model.IsResolved = entity.IsResolved;

            model.IsActive = entity.IsActive;

            model.CreatedBy = entity.CreatedBy;

            model.CreatedDate = entity.CreatedDate;

            model.UpdatedBy = entity.UpdatedBy;

            model.UpdatedDate = entity.UpdatedDate;

            model.AlertSource = entity.AlertSource;

            model.IsRead = entity.IsRead;

            model.ReadDate = entity.ReadDate;

            model.ReadByMemberCode = entity.ReadByMemberCode;
        }
        private void MapCampaignAlert(
    CampaignAlertVM model,
    CampaignAlert entity)
        {
            if (model == null)
            {
                return;
            }

            if (entity == null)
            {
                return;
            }

            entity.CampaignId = model.CampaignId;
            entity.AlertTitle = model.AlertTitle;
            entity.AlertMessage = model.AlertMessage;
            entity.AlertType = model.AlertType;
            entity.ReferenceModule = model.ReferenceModule;
            entity.ReferenceId = model.ReferenceId;
            entity.Severity = model.Severity;
            entity.AlertStatus = model.AlertStatus;
            entity.AssignedToMemberCode = model.AssignedToMemberCode;
            entity.AssignedToName = model.AssignedToName;
            entity.ActionTaken = model.ActionTaken;
            entity.ActionTakenByMemberCode = model.ActionTakenByMemberCode;
            entity.ActionTakenDate = model.ActionTakenDate;
            entity.ExpiryDate = model.ExpiryDate;
            entity.IsDashboard = model.IsDashboard;
            entity.IsNotificationSent = model.IsNotificationSent;
            entity.IsResolved = model.IsResolved;
            entity.AlertSource = model.AlertSource;
            entity.IsRead = model.IsRead;
            entity.ReadDate = model.ReadDate;
            entity.ReadByMemberCode = model.ReadByMemberCode;
        }
        //=========================================================
        // End Part 1
        //=========================================================


        //=========================================================
        // Part 2 Starts
        // Campaign Alert Read Methods
        //=========================================================

        /// <summary>
        /// Returns all active campaign alerts.
        /// </summary>
        public List<CampaignAlertVM> GetCampaignAlerts()
        {
            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Where(x => x.IsActive)
                      .OrderByDescending(x => x.CreatedDate)
                      .ToList()
                      .Select(MapCampaignAlert)
                      .ToList();
        }

        /// <summary>
        /// Returns campaign alert by id.
        /// </summary>
        public CampaignAlertVM GetCampaignAlertById(int campaignAlertId)
        {
            CampaignAlert entity = _db.CampaignAlerts
                                      .AsNoTracking()
                                      .FirstOrDefault(x =>
                                            x.CampaignAlertId == campaignAlertId
                                            && x.IsActive);

            return MapCampaignAlert(entity);
        }

        /// <summary>
        /// Returns all alerts of a campaign.
        /// </summary>
        public List<CampaignAlertVM> GetCampaignAlertsByCampaign(int campaignId)
        {
            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Where(x =>
                            x.CampaignId == campaignId &&
                            x.IsActive)
                      .OrderByDescending(x => x.CreatedDate)
                      .ToList()
                      .Select(MapCampaignAlert)
                      .ToList();
        }

        /// <summary>
        /// Returns alerts by severity.
        /// </summary>
        public List<CampaignAlertVM> GetCampaignAlertsBySeverity(string severity)
        {
            if (String.IsNullOrWhiteSpace(severity))
            {
                return new List<CampaignAlertVM>();
            }

            severity = severity.Trim();

            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Where(x =>
                            x.IsActive &&
                            x.Severity == severity)
                      .OrderByDescending(x => x.CreatedDate)
                      .ToList()
                      .Select(MapCampaignAlert)
                      .ToList();
        }

        /// <summary>
        /// Returns alerts by status.
        /// </summary>
        public List<CampaignAlertVM> GetCampaignAlertsByStatus(string status)
        {
            if (String.IsNullOrWhiteSpace(status))
            {
                return new List<CampaignAlertVM>();
            }

            status = status.Trim();

            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Where(x =>
                            x.IsActive &&
                            x.AlertStatus == status)
                      .OrderByDescending(x => x.CreatedDate)
                      .ToList()
                      .Select(MapCampaignAlert)
                      .ToList();
        }

        /// <summary>
        /// Returns latest campaign alerts.
        /// </summary>
        public List<CampaignAlertVM> GetRecentCampaignAlerts(int count)
        {
            if (count <= 0)
            {
                count = 10;
            }

            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Where(x => x.IsActive)
                      .OrderByDescending(x => x.CreatedDate)
                      .Take(count)
                      .ToList()
                      .Select(MapCampaignAlert)
                      .ToList();
        }

        /// <summary>
        /// Returns dashboard campaign alerts.
        /// </summary>
        public List<CampaignAlertVM> GetDashboardCampaignAlerts()
        {
            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Where(x =>
                            x.IsActive &&
                            x.IsDashboard)
                      .OrderByDescending(x => x.CreatedDate)
                      .ToList()
                      .Select(MapCampaignAlert)
                      .ToList();
        }

        /// <summary>
        /// Returns all unresolved campaign alerts.
        /// </summary>
        public List<CampaignAlertVM> GetPendingCampaignAlerts()
        {
            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Where(x =>
                            x.IsActive &&
                            !x.IsResolved)
                      .OrderByDescending(x => x.CreatedDate)
                      .ToList()
                      .Select(MapCampaignAlert)
                      .ToList();
        }

        /// <summary>
        /// Returns all resolved campaign alerts.
        /// </summary>
        public List<CampaignAlertVM> GetResolvedCampaignAlerts()
        {
            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Where(x =>
                            x.IsActive &&
                            x.IsResolved)
                      .OrderByDescending(x => x.ActionTakenDate)
                      .ToList()
                      .Select(MapCampaignAlert)
                      .ToList();
        }

        //=========================================================
        // End Part 2
        //=========================================================

        //=========================================================
        // Part 3 Starts
        // Campaign Alert Search + Dashboard
        //=========================================================

        /// <summary>
        /// Searches campaign alerts.
        /// </summary>
        public List<CampaignAlertVM> SearchCampaignAlerts(
            string keyword,
            string severity,
            string status,
            int? campaignId)
        {
            IQueryable<CampaignAlert> query = _db.CampaignAlerts
                                                 .AsNoTracking()
                                                 .Where(x => x.IsActive);

            if (!String.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                query = query.Where(x =>
                    x.AlertTitle.Contains(keyword) ||
                    x.AlertMessage.Contains(keyword) ||
                    x.AssignedToName.Contains(keyword));
            }

            if (!String.IsNullOrWhiteSpace(severity))
            {
                severity = severity.Trim();

                query = query.Where(x => x.Severity == severity);
            }

            if (!String.IsNullOrWhiteSpace(status))
            {
                status = status.Trim();

                query = query.Where(x => x.AlertStatus == status);
            }

            if (campaignId.HasValue)
            {
                query = query.Where(x => x.CampaignId == campaignId.Value);
            }

            return query.OrderByDescending(x => x.CreatedDate)
                        .ToList()
                        .Select(MapCampaignAlert)
                        .ToList();
        }

        /// <summary>
        /// Returns alerts by campaign.
        /// </summary>
        public List<CampaignAlertVM> GetCampaignAlertByCampaign(int campaignId)
        {
            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Where(x =>
                            x.IsActive &&
                            x.CampaignId == campaignId)
                      .OrderByDescending(x => x.CreatedDate)
                      .ToList()
                      .Select(MapCampaignAlert)
                      .ToList();
        }

        /// <summary>
        /// Returns alerts by alert type.
        /// </summary>
        public List<CampaignAlertVM> GetCampaignAlertByAlertType(string alertType)
        {
            if (String.IsNullOrWhiteSpace(alertType))
            {
                return new List<CampaignAlertVM>();
            }

            alertType = alertType.Trim();

            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Where(x =>
                            x.IsActive &&
                            x.AlertType == alertType)
                      .OrderByDescending(x => x.CreatedDate)
                      .ToList()
                      .Select(MapCampaignAlert)
                      .ToList();
        }

        /// <summary>
        /// Returns alerts assigned to a member.
        /// </summary>
        public List<CampaignAlertVM> GetCampaignAlertByAssignedMember(string memberCode)
        {
            if (String.IsNullOrWhiteSpace(memberCode))
            {
                return new List<CampaignAlertVM>();
            }

            memberCode = memberCode.Trim();

            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Where(x =>
                            x.IsActive &&
                            x.AssignedToMemberCode == memberCode)
                      .OrderByDescending(x => x.CreatedDate)
                      .ToList()
                      .Select(MapCampaignAlert)
                      .ToList();
        }

        /// <summary>
        /// Returns unread alerts.
        /// </summary>
        public List<CampaignAlertVM> GetUnreadCampaignAlerts()
        {
            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Where(x =>
                            x.IsActive &&
                            !x.IsRead)
                      .OrderByDescending(x => x.CreatedDate)
                      .ToList()
                      .Select(MapCampaignAlert)
                      .ToList();
        }

        /// <summary>
        /// Returns read alerts.
        /// </summary>
        public List<CampaignAlertVM> GetReadCampaignAlerts()
        {
            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Where(x =>
                            x.IsActive &&
                            x.IsRead)
                      .OrderByDescending(x => x.ReadDate)
                      .ToList()
                      .Select(MapCampaignAlert)
                      .ToList();
        }

        /// <summary>
        /// Returns expired alerts.
        /// </summary>
        public List<CampaignAlertVM> GetExpiredCampaignAlerts()
        {
            DateTime today = DateTime.Now;

            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Where(x =>
                            x.IsActive &&
                            x.ExpiryDate.HasValue &&
                            x.ExpiryDate.Value < today)
                      .OrderByDescending(x => x.ExpiryDate)
                      .ToList()
                      .Select(MapCampaignAlert)
                      .ToList();
        }

        /// <summary>
        /// Returns dashboard alerts.
        /// </summary>
        public List<CampaignAlertVM> GetDashboardAlerts()
        {
            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Where(x =>
                            x.IsActive &&
                            x.IsDashboard)
                      .OrderByDescending(x => x.CreatedDate)
                      .ToList()
                      .Select(MapCampaignAlert)
                      .ToList();
        }

        /// <summary>
        /// Returns high severity dashboard alerts.
        /// </summary>
        public List<CampaignAlertVM> GetHighSeverityDashboardAlerts()
        {
            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Where(x =>
                            x.IsActive &&
                            x.IsDashboard &&
                            (x.Severity == "High" ||
                             x.Severity == "Critical"))
                      .OrderByDescending(x => x.CreatedDate)
                      .ToList()
                      .Select(MapCampaignAlert)
                      .ToList();
        }

        /// <summary>
        /// Returns alerts where notification has not been sent.
        /// </summary>
        public List<CampaignAlertVM> GetPendingNotificationAlerts()
        {
            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Where(x =>
                            x.IsActive &&
                            !x.IsNotificationSent)
                      .OrderByDescending(x => x.CreatedDate)
                      .ToList()
                      .Select(MapCampaignAlert)
                      .ToList();
        }

        //=========================================================
        // End Part 3
        //=========================================================

        //=========================================================
        // Part 4 Starts
        // Campaign Alert CRUD
        //=========================================================

        /// <summary>
        /// Checks whether a duplicate campaign alert exists.
        /// </summary>
        public bool IsDuplicateCampaignAlert(
            string alertTitle,
            int? campaignId,
            int campaignAlertId)
        {
            if (String.IsNullOrWhiteSpace(alertTitle))
            {
                return false;
            }

            alertTitle = alertTitle.Trim();

            return _db.CampaignAlerts.Any(x =>
                x.IsActive &&
                x.AlertTitle == alertTitle &&
                x.CampaignId == campaignId &&
                x.CampaignAlertId != campaignAlertId);
        }

        /// <summary>
        /// Saves campaign alert.
        /// </summary>
        public bool SaveCampaignAlert(
            CampaignAlertVM model,
            int userId)
        {
            if (model == null)
            {
                return false;
            }

            if (IsDuplicateCampaignAlert(
                model.AlertTitle,
                model.CampaignId,
                0))
            {
                return false;
            }

            CampaignAlert entity = new CampaignAlert();

            MapCampaignAlert(model, entity);

            entity.IsActive = true;

            entity.CreatedBy = userId;
            entity.CreatedDate = DateTime.Now;

            entity.UpdatedBy = null;
            entity.UpdatedDate = null;

            _db.CampaignAlerts.Add(entity);

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Updates campaign alert.
        /// </summary>
        public bool UpdateCampaignAlert(
            CampaignAlertVM model,
            int userId)
        {
            if (model == null)
            {
                return false;
            }

            CampaignAlert entity = _db.CampaignAlerts
                                      .FirstOrDefault(x =>
                                          x.CampaignAlertId == model.CampaignAlertId &&
                                          x.IsActive);

            if (entity == null)
            {
                return false;
            }

            if (IsDuplicateCampaignAlert(
                model.AlertTitle,
                model.CampaignId,
                model.CampaignAlertId))
            {
                return false;
            }

            MapCampaignAlert(model, entity);

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Soft deletes campaign alert.
        /// </summary>
        public bool DeleteCampaignAlert(
            int campaignAlertId,
            int userId)
        {
            CampaignAlert entity = _db.CampaignAlerts
                                      .FirstOrDefault(x =>
                                          x.CampaignAlertId == campaignAlertId &&
                                          x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.IsActive = false;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        //=========================================================
        // End Part 4
        //=========================================================

        //=========================================================
        // Part 5 Starts
        // Campaign Alert Workflow
        //=========================================================

        /// <summary>
        /// Assigns campaign alert to a member.
        /// </summary>
        public bool AssignCampaignAlert(
            int campaignAlertId,
            string memberCode,
            string memberName,
            int userId)
        {
            CampaignAlert entity = _db.CampaignAlerts
                                      .FirstOrDefault(x =>
                                            x.CampaignAlertId == campaignAlertId &&
                                            x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.AssignedToMemberCode = memberCode;
            entity.AssignedToName = memberName;
            entity.AlertStatus = "Assigned";

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Resolves campaign alert.
        /// </summary>
        public bool ResolveCampaignAlert(
            int campaignAlertId,
            string actionTaken,
            string memberCode,
            int userId)
        {
            CampaignAlert entity = _db.CampaignAlerts
                                      .FirstOrDefault(x =>
                                            x.CampaignAlertId == campaignAlertId &&
                                            x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.ActionTaken = actionTaken;
            entity.ActionTakenByMemberCode = memberCode;
            entity.ActionTakenDate = DateTime.Now;

            entity.IsResolved = true;
            entity.AlertStatus = "Resolved";

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Reopens resolved campaign alert.
        /// </summary>
        public bool ReopenCampaignAlert(
            int campaignAlertId,
            int userId)
        {
            CampaignAlert entity = _db.CampaignAlerts
                                      .FirstOrDefault(x =>
                                            x.CampaignAlertId == campaignAlertId &&
                                            x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.IsResolved = false;
            entity.AlertStatus = "Open";

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Marks campaign alert as read.
        /// </summary>
        public bool MarkCampaignAlertAsRead(
            int campaignAlertId,
            string memberCode,
            int userId)
        {
            CampaignAlert entity = _db.CampaignAlerts
                                      .FirstOrDefault(x =>
                                            x.CampaignAlertId == campaignAlertId &&
                                            x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.IsRead = true;
            entity.ReadDate = DateTime.Now;
            entity.ReadByMemberCode = memberCode;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Marks notification as sent.
        /// </summary>
        public bool MarkNotificationSent(
            int campaignAlertId,
            int userId)
        {
            CampaignAlert entity = _db.CampaignAlerts
                                      .FirstOrDefault(x =>
                                            x.CampaignAlertId == campaignAlertId &&
                                            x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.IsNotificationSent = true;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Updates action taken remarks.
        /// </summary>
        public bool UpdateActionTaken(
            int campaignAlertId,
            string actionTaken,
            string memberCode,
            int userId)
        {
            CampaignAlert entity = _db.CampaignAlerts
                                      .FirstOrDefault(x =>
                                            x.CampaignAlertId == campaignAlertId &&
                                            x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.ActionTaken = actionTaken;
            entity.ActionTakenByMemberCode = memberCode;
            entity.ActionTakenDate = DateTime.Now;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        /// <summary>
        /// Expires campaign alert.
        /// </summary>
        public bool ExpireCampaignAlert(
            int campaignAlertId,
            int userId)
        {
            CampaignAlert entity = _db.CampaignAlerts
                                      .FirstOrDefault(x =>
                                            x.CampaignAlertId == campaignAlertId &&
                                            x.IsActive);

            if (entity == null)
            {
                return false;
            }

            entity.AlertStatus = "Expired";
            entity.ExpiryDate = DateTime.Now;
            entity.IsResolved = false;

            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            _db.SaveChanges();

            return true;
        }

        //=========================================================
        // End Part 5
        //=========================================================
        //=========================================================
        // Part 6 Starts
        // Campaign Alert KPI + Reports
        //=========================================================

        /// <summary>
        /// Returns total active campaign alerts.
        /// </summary>
        public int GetTotalCampaignAlerts()
        {
            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Count(x => x.IsActive);
        }

        /// <summary>
        /// Returns active campaign alerts.
        /// </summary>
        public int GetActiveCampaignAlerts()
        {
            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Count(x =>
                            x.IsActive &&
                            !x.IsResolved);
        }

        /// <summary>
        /// Returns pending campaign alerts.
        /// </summary>
        public int GetPendingCampaignAlertsCount()
        {
            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Count(x =>
                            x.IsActive &&
                            !x.IsResolved);
        }

        /// <summary>
        /// Returns resolved campaign alerts.
        /// </summary>
        public int GetResolvedCampaignAlertsCount()
        {
            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Count(x =>
                            x.IsActive &&
                            x.IsResolved);
        }

        /// <summary>
        /// Returns dashboard alerts.
        /// </summary>
        public int GetDashboardCampaignAlertsCount()
        {
            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Count(x =>
                            x.IsActive &&
                            x.IsDashboard);
        }

        /// <summary>
        /// Returns notification sent alerts.
        /// </summary>
        public int GetNotificationSentAlertsCount()
        {
            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Count(x =>
                            x.IsActive &&
                            x.IsNotificationSent);
        }

        /// <summary>
        /// Returns unread alerts.
        /// </summary>
        public int GetUnreadCampaignAlertsCount()
        {
            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Count(x =>
                            x.IsActive &&
                            !x.IsRead);
        }

        /// <summary>
        /// Returns read alerts.
        /// </summary>
        public int GetReadCampaignAlertsCount()
        {
            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Count(x =>
                            x.IsActive &&
                            x.IsRead);
        }

        /// <summary>
        /// Returns high severity alerts.
        /// </summary>
        public int GetHighSeverityAlertsCount()
        {
            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Count(x =>
                            x.IsActive &&
                            x.Severity == "High");
        }

        /// <summary>
        /// Returns critical alerts.
        /// </summary>
        public int GetCriticalAlertsCount()
        {
            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Count(x =>
                            x.IsActive &&
                            x.Severity == "Critical");
        }

        /// <summary>
        /// Returns campaign wise report.
        /// </summary>
        public List<CampaignAlertVM> GetCampaignAlertReport(int campaignId)
        {
            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Where(x =>
                            x.IsActive &&
                            x.CampaignId == campaignId)
                      .OrderByDescending(x => x.CreatedDate)
                      .ToList()
                      .Select(MapCampaignAlert)
                      .ToList();
        }

        /// <summary>
        /// Returns severity wise report.
        /// </summary>
        public List<CampaignAlertVM> GetSeverityReport(string severity)
        {
            if (String.IsNullOrWhiteSpace(severity))
            {
                return new List<CampaignAlertVM>();
            }

            severity = severity.Trim();

            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Where(x =>
                            x.IsActive &&
                            x.Severity == severity)
                      .OrderByDescending(x => x.CreatedDate)
                      .ToList()
                      .Select(MapCampaignAlert)
                      .ToList();
        }

        /// <summary>
        /// Returns status wise report.
        /// </summary>
        public List<CampaignAlertVM> GetStatusReport(string status)
        {
            if (String.IsNullOrWhiteSpace(status))
            {
                return new List<CampaignAlertVM>();
            }

            status = status.Trim();

            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Where(x =>
                            x.IsActive &&
                            x.AlertStatus == status)
                      .OrderByDescending(x => x.CreatedDate)
                      .ToList()
                      .Select(MapCampaignAlert)
                      .ToList();
        }

        //=========================================================
        // End Part 6
        //=========================================================
        //=========================================================
        // Part 7 Starts
        // Campaign Alert Notification Support
        //=========================================================

        /// <summary>
        /// Returns alerts pending for notification.
        /// </summary>
        public List<CampaignAlertVM> GetAlertsPendingNotification()
        {
            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Where(x =>
                            x.IsActive &&
                            !x.IsNotificationSent)
                      .OrderBy(x => x.CreatedDate)
                      .ToList()
                      .Select(MapCampaignAlert)
                      .ToList();
        }

        /// <summary>
        /// Returns today's expiring alerts.
        /// </summary>
        public List<CampaignAlertVM> GetTodayExpiringAlerts()
        {
            DateTime today = DateTime.Today;

            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Where(x =>
                            x.IsActive &&
                            x.ExpiryDate.HasValue &&
                            x.ExpiryDate.Value.Year == today.Year &&
                            x.ExpiryDate.Value.Month == today.Month &&
                            x.ExpiryDate.Value.Day == today.Day)
                      .OrderBy(x => x.ExpiryDate)
                      .ToList()
                      .Select(MapCampaignAlert)
                      .ToList();
        }

        /// <summary>
        /// Returns overdue alerts.
        /// </summary>
        public List<CampaignAlertVM> GetOverdueAlerts()
        {
            DateTime today = DateTime.Now;

            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Where(x =>
                            x.IsActive &&
                            !x.IsResolved &&
                            x.ExpiryDate.HasValue &&
                            x.ExpiryDate.Value < today)
                      .OrderBy(x => x.ExpiryDate)
                      .ToList()
                      .Select(MapCampaignAlert)
                      .ToList();
        }

        /// <summary>
        /// Returns critical alerts requiring immediate notification.
        /// </summary>
        public List<CampaignAlertVM> GetCriticalNotificationAlerts()
        {
            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Where(x =>
                            x.IsActive &&
                            !x.IsNotificationSent &&
                            x.Severity == "Critical")
                      .OrderByDescending(x => x.CreatedDate)
                      .ToList()
                      .Select(MapCampaignAlert)
                      .ToList();
        }

        /// <summary>
        /// Returns high severity alerts requiring notification.
        /// </summary>
        public List<CampaignAlertVM> GetHighSeverityNotificationAlerts()
        {
            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Where(x =>
                            x.IsActive &&
                            !x.IsNotificationSent &&
                            x.Severity == "High")
                      .OrderByDescending(x => x.CreatedDate)
                      .ToList()
                      .Select(MapCampaignAlert)
                      .ToList();
        }

        /// <summary>
        /// Returns unread dashboard alerts.
        /// </summary>
        public List<CampaignAlertVM> GetUnreadDashboardAlerts()
        {
            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Where(x =>
                            x.IsActive &&
                            x.IsDashboard &&
                            !x.IsRead)
                      .OrderByDescending(x => x.CreatedDate)
                      .ToList()
                      .Select(MapCampaignAlert)
                      .ToList();
        }

        /// <summary>
        /// Returns alerts assigned to a member awaiting action.
        /// </summary>
        public List<CampaignAlertVM> GetAssignedPendingAlerts(string memberCode)
        {
            if (String.IsNullOrWhiteSpace(memberCode))
            {
                return new List<CampaignAlertVM>();
            }

            memberCode = memberCode.Trim();

            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Where(x =>
                            x.IsActive &&
                            !x.IsResolved &&
                            x.AssignedToMemberCode == memberCode)
                      .OrderByDescending(x => x.CreatedDate)
                      .ToList()
                      .Select(MapCampaignAlert)
                      .ToList();
        }

        /// <summary>
        /// Returns alerts created today.
        /// </summary>
        public List<CampaignAlertVM> GetTodayAlerts()
        {
            DateTime today = DateTime.Today;

            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Where(x =>
                            x.IsActive &&
                            x.CreatedDate.Year == today.Year &&
                            x.CreatedDate.Month == today.Month &&
                            x.CreatedDate.Day == today.Day)
                      .OrderByDescending(x => x.CreatedDate)
                      .ToList()
                      .Select(MapCampaignAlert)
                      .ToList();
        }

        //=========================================================
        // End Part 7
        //=========================================================
        //=========================================================
        // Part 8 Starts
        // Campaign Alert Analytics + Reports
        //=========================================================

        /// <summary>
        /// Returns monthly campaign alert count.
        /// </summary>
        public int GetMonthlyCampaignAlertCount(int year, int month)
        {
            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Count(x =>
                            x.IsActive &&
                            x.CreatedDate.Year == year &&
                            x.CreatedDate.Month == month);
        }

        /// <summary>
        /// Returns yearly campaign alert count.
        /// </summary>
        public int GetYearlyCampaignAlertCount(int year)
        {
            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Count(x =>
                            x.IsActive &&
                            x.CreatedDate.Year == year);
        }

        /// <summary>
        /// Returns severity analytics.
        /// </summary>
        public Dictionary<string, int> GetSeverityAnalytics()
        {
            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Where(x => x.IsActive)
                      .GroupBy(x => x.Severity)
                      .ToDictionary(
                            x => x.Key,
                            x => x.Count());
        }

        /// <summary>
        /// Returns status analytics.
        /// </summary>
        public Dictionary<string, int> GetStatusAnalytics()
        {
            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Where(x => x.IsActive)
                      .GroupBy(x => x.AlertStatus)
                      .ToDictionary(
                            x => x.Key,
                            x => x.Count());
        }

        /// <summary>
        /// Returns campaign analytics.
        /// </summary>
        public Dictionary<int?, int> GetCampaignAnalytics()
        {
            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Where(x => x.IsActive)
                      .GroupBy(x => x.CampaignId)
                      .ToDictionary(
                            x => x.Key,
                            x => x.Count());
        }

        /// <summary>
        /// Returns alert type analytics.
        /// </summary>
        public Dictionary<string, int> GetAlertTypeAnalytics()
        {
            return _db.CampaignAlerts
                      .AsNoTracking()
                      .Where(x => x.IsActive)
                      .GroupBy(x => x.AlertType)
                      .ToDictionary(
                            x => x.Key,
                            x => x.Count());
        }

        /// <summary>
        /// Returns read analytics.
        /// </summary>
        public Dictionary<string, int> GetReadAnalytics()
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            result.Add("Read",
                _db.CampaignAlerts
                   .AsNoTracking()
                   .Count(x =>
                        x.IsActive &&
                        x.IsRead));

            result.Add("Unread",
                _db.CampaignAlerts
                   .AsNoTracking()
                   .Count(x =>
                        x.IsActive &&
                        !x.IsRead));

            return result;
        }

        /// <summary>
        /// Returns notification analytics.
        /// </summary>
        public Dictionary<string, int> GetNotificationAnalytics()
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            result.Add("Sent",
                _db.CampaignAlerts
                   .AsNoTracking()
                   .Count(x =>
                        x.IsActive &&
                        x.IsNotificationSent));

            result.Add("Pending",
                _db.CampaignAlerts
                   .AsNoTracking()
                   .Count(x =>
                        x.IsActive &&
                        !x.IsNotificationSent));

            return result;
        }

        /// <summary>
        /// Returns resolution percentage.
        /// </summary>
        public decimal GetResolutionPercentage()
        {
            int total = _db.CampaignAlerts
                           .AsNoTracking()
                           .Count(x => x.IsActive);

            if (total == 0)
            {
                return 0;
            }

            int resolved = _db.CampaignAlerts
                              .AsNoTracking()
                              .Count(x =>
                                    x.IsActive &&
                                    x.IsResolved);

            return Math.Round(((decimal)resolved * 100M) / total, 2);
        }

        /// <summary>
        /// Returns dashboard analytics.
        /// </summary>
        public Dictionary<string, int> GetDashboardAnalytics()
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            result.Add("Dashboard",
                _db.CampaignAlerts
                   .AsNoTracking()
                   .Count(x =>
                        x.IsActive &&
                        x.IsDashboard));

            result.Add("NonDashboard",
                _db.CampaignAlerts
                   .AsNoTracking()
                   .Count(x =>
                        x.IsActive &&
                        !x.IsDashboard));

            return result;
        }

        //=========================================================
        // End Part 8
        //=========================================================

        public CampaignErpBlueprintVM GetCampaignErpBlueprint()
        {
            CampaignErpBlueprintVM model = new CampaignErpBlueprintVM
            {
                Title = "Complete Campaign ERP Architecture",
                Subtitle = "Enterprise Election War Room roadmap aligned with campaign operations, lawful constituency management and role-based administration."
            };

            model.Pillars.Add(CreatePillar("Campaign Dashboard", "Live command center for countdown, schedule, tasks, alerts, booth coverage, volunteers, media and performance.", "Implemented", "bi-speedometer2", "Index", "Election countdown", "Today's schedule", "Pending tasks", "Booth coverage", "Alerts and notifications", "District performance"));
            model.Pillars.Add(CreatePillar("Campaign Management", "Election setup, manifesto, campaign goals, timeline, calendar, team ownership and status control.", "Implemented", "bi-calendar-range", "Campaigns", "State election", "Lok Sabha", "Vidhan Sabha", "Municipal", "Panchayat", "Campaign goals"));
            model.Pillars.Add(CreatePillar("Leader Campaign Kit", "Leader branding, approved biography, speeches, slogans, media kit and public promise tracker for party-style campaigns.", "Foundation", "bi-megaphone", "LeaderCampaignKit", "Leader profile", "Approved slogans", "Speech bank", "Media kit", "Manifesto points", "Training notes"));
            model.Pillars.Add(CreatePillar("Booth Committee Network", "Booth-level committee structure, page/pramukh coordination, meeting calendar and responsible contact tracking.", "Foundation", "bi-diagram-3", "BoothCommittee", "Booth committee", "Area coordinator", "Meeting plan", "Responsibility map", "Follow-up"));
            model.Pillars.Add(CreatePillar("Candidate Management", "Candidate profile, affidavit documents, media files, election history, expenses and approval workflow.", "Implemented", "bi-person-badge", "CandidateManagement", "Bio", "Education", "Profession", "Assets", "Affidavit", "Approval workflow"));
            model.Pillars.Add(CreatePillar("Constituency Management", "Administrative geography, booth mapping, past results, voter strength and performance insights.", "Foundation", "bi-map", "ElectionBooths", "State", "District", "Assembly", "Parliament", "Block", "Village", "Ward", "Polling booth"));
            model.Pillars.Add(CreatePillar("Booth Management", "Booth committees, booth volunteers, booth visits, attendance, daily reporting and weak/strong indicators.", "Implemented", "bi-building-check", "BoothMonitoring", "Booth committee", "Booth president", "Booth agents", "Panna pramukh", "Booth visits", "Daily reporting"));
            model.Pillars.Add(CreatePillar("Voter Management", "Lawful voter roll import, search, family mapping and feedback with duplicate prevention.", "Foundation", "bi-people", "BoothVisits", "Official roll import", "Search voter", "Family mapping", "Feedback", "Duplicate prevention", "Data governance"));
            model.Pillars.Add(CreatePillar("Volunteer Management", "Volunteer registration, verification, skill, availability, work assignment, attendance and performance.", "Implemented", "bi-person-check", "Volunteers", "Registration", "Verification", "Skill", "Availability", "Attendance", "Reward points", "Training"));
            model.Pillars.Add(CreatePillar("Membership Management", "Issue secure digital membership cards and official appointment or authorization letters with QR verification, expiry and revocation.", "Implemented", "bi-card-checklist", "MembershipDrive", "Digital card", "Official letters", "QR verification", "Expiry control", "Revocation", "Document register"));
            model.Pillars.Add(CreatePillar("Event Management", "Rallies, public meetings, road shows, guest management, attendance, expenses, media and QR readiness.", "Implemented", "bi-calendar-event", "RallyEvents", "Public meeting", "Rally", "Road show", "QR check-in", "Expense tracking", "Photo gallery", "Guest management"));
            model.Pillars.Add(CreatePillar("Task Management", "Daily tasks, assignment, priority, deadline, progress, reminder and escalation.", "Implemented", "bi-check2-square", "CampaignTasks", "Daily tasks", "Assign", "Priority", "Deadline", "Reminder", "Escalation", "Progress"));
            model.Pillars.Add(CreatePillar("Social Media War Room", "Content calendar, approval, publishing queue, hashtag tracking and performance analytics.", "Implemented", "bi-share", "SocialMediaWarRoom", "Facebook", "Instagram", "X", "YouTube", "WhatsApp", "Content approval", "Analytics"));
            model.Pillars.Add(CreatePillar("Poll & Survey Management", "Shareable public feedback polls with WhatsApp, Facebook, QR/public link support and source-wise response tracking.", "Implemented", "bi-bar-chart-line", "CampaignPolls", "Poll builder", "Public link", "WhatsApp share", "Facebook share", "Source tracking", "Results dashboard"));
            model.Pillars.Add(CreatePillar("Digital Media Library", "Photos, videos, documents, press coverage and approved campaign assets.", "Implemented", "bi-images", "Media", "Photos", "Videos", "Documents", "Press coverage", "Approval status"));
            model.Pillars.Add(CreatePillar("Finance and Donations", "Fund collection, donation progress, event spending, approvals and audit-ready expense records.", "Foundation", "bi-cash-stack", "FinanceAndDonations", "Fund collection", "Donation progress", "Campaign expenses", "Approvals", "Audit trail"));
            model.Pillars.Add(CreatePillar("Compliance and Security", "Role-based authorization, lawful data handling, audit logging and production readiness controls.", "Ongoing", "bi-shield-lock", "ComplianceCenter", "Role permissions", "Audit trail", "Legal voter data handling", "Secure uploads", "Production configuration"));

            model.DeliveryPhases.Add(new CampaignErpPhaseVM { Phase = "Phase 1", Focus = "Command Center", Outcome = "Dashboard, events, tasks, booth monitoring, Jan Sampark and alerts operational." });
            model.DeliveryPhases.Add(new CampaignErpPhaseVM { Phase = "Phase 2", Focus = "Campaign ERP Core", Outcome = "Campaign setup, candidate profile, constituency master and membership drive workflows." });
            model.DeliveryPhases.Add(new CampaignErpPhaseVM { Phase = "Phase 3", Focus = "Field Operations", Outcome = "Booth committees, volunteer verification, QR attendance, vehicle and event logistics." });
            model.DeliveryPhases.Add(new CampaignErpPhaseVM { Phase = "Phase 4", Focus = "Media and Social War Room", Outcome = "Content approval, media library, publishing calendar and performance dashboard." });
            model.DeliveryPhases.Add(new CampaignErpPhaseVM { Phase = "Phase 5", Focus = "Compliance and Analytics", Outcome = "Audit trail, role matrix, lawful voter import governance and executive reports." });

            model.GovernanceRules.Add("Use only official/lawfully obtained voter-list imports; do not scrape unofficial personal data.");
            model.GovernanceRules.Add("Do not edit EDMX-generated entity classes manually; add ViewModels, services and SQL upgrade scripts.");
            model.GovernanceRules.Add("Every sensitive workflow should enforce role-based authorization and audit important status changes.");
            model.GovernanceRules.Add("Avoid manipulative or discriminatory targeting; use data for lawful operations, service follow-up and campaign management.");
            model.GovernanceRules.Add("Keep UI compact, responsive and usable by district, block, village and booth-level workers.");

            return model;
        }

        public List<CandidateProfileVM> GetCandidateProfiles(string keyword)
        {
            EnsureCandidateProfileTable();

            string where = @"WHERE IsActive = 1
                AND (@Keyword IS NULL
                    OR FullName LIKE @LikeKeyword
                    OR PartyName LIKE @LikeKeyword
                    OR ConstituencyName LIKE @LikeKeyword
                    OR District LIKE @LikeKeyword
                    OR ApprovalStatus LIKE @LikeKeyword)";

            DataTable table = QuerySql(@"
SELECT TOP 200 *
FROM dbo.CandidateProfile
" + where + @"
ORDER BY CandidateProfileId DESC",
                new SqlParameter("@Keyword", string.IsNullOrWhiteSpace(keyword) ? (object)DBNull.Value : keyword.Trim()),
                new SqlParameter("@LikeKeyword", "%" + (keyword ?? string.Empty).Trim() + "%"));

            return MapCandidateProfiles(table);
        }

        public CandidateProfileVM GetCandidateProfileById(int id)
        {
            EnsureCandidateProfileTable();

            DataTable table = QuerySql(@"
SELECT TOP 1 *
FROM dbo.CandidateProfile
WHERE CandidateProfileId = @CandidateProfileId AND IsActive = 1",
                new SqlParameter("@CandidateProfileId", id));

            return table.Rows.Count == 0 ? null : MapCandidateProfile(table.Rows[0]);
        }

        public bool SaveCandidateProfile(CandidateProfileVM model, int userId)
        {
            EnsureCandidateProfileTable();

            ExecuteSql(@"
INSERT INTO dbo.CandidateProfile
    (FullName, PartyName, ElectionType, ConstituencyName, District, State, Education, Profession, PublicBio,
     ManifestoUrl, AffidavitUrl, PhotoUrl, FacebookUrl, TwitterUrl, InstagramUrl, YouTubeUrl,
     DeclaredAssets, DeclaredLiabilities, CriminalCaseSummary, ApprovalStatus, IsPublished, IsActive, CreatedBy)
VALUES
    (@FullName, @PartyName, @ElectionType, @ConstituencyName, @District, @State, @Education, @Profession, @PublicBio,
     @ManifestoUrl, @AffidavitUrl, @PhotoUrl, @FacebookUrl, @TwitterUrl, @InstagramUrl, @YouTubeUrl,
     @DeclaredAssets, @DeclaredLiabilities, @CriminalCaseSummary, @ApprovalStatus, @IsPublished, @IsActive, @UserId)",
                CandidateParameters(model, userId).ToArray());

            AddCampaignAudit("Candidate Management", model.FullName, "Create", userId, null, "Candidate profile created.", true);
            return true;
        }

        public bool UpdateCandidateProfile(CandidateProfileVM model, int userId)
        {
            EnsureCandidateProfileTable();

            List<SqlParameter> parameters = CandidateParameters(model, userId);
            parameters.Add(new SqlParameter("@CandidateProfileId", model.CandidateProfileId));

            ExecuteSql(@"
UPDATE dbo.CandidateProfile
SET FullName = @FullName,
    PartyName = @PartyName,
    ElectionType = @ElectionType,
    ConstituencyName = @ConstituencyName,
    District = @District,
    State = @State,
    Education = @Education,
    Profession = @Profession,
    PublicBio = @PublicBio,
    ManifestoUrl = @ManifestoUrl,
    AffidavitUrl = @AffidavitUrl,
    PhotoUrl = @PhotoUrl,
    FacebookUrl = @FacebookUrl,
    TwitterUrl = @TwitterUrl,
    InstagramUrl = @InstagramUrl,
    YouTubeUrl = @YouTubeUrl,
    DeclaredAssets = @DeclaredAssets,
    DeclaredLiabilities = @DeclaredLiabilities,
    CriminalCaseSummary = @CriminalCaseSummary,
    ApprovalStatus = @ApprovalStatus,
    IsPublished = @IsPublished,
    IsActive = @IsActive,
    UpdatedBy = @UserId,
    UpdatedDate = GETDATE()
WHERE CandidateProfileId = @CandidateProfileId",
                parameters.ToArray());

            AddCampaignAudit("Candidate Management", Convert.ToString(model.CandidateProfileId), "Update", userId, null, "Candidate profile updated.", true);
            return true;
        }

        public bool DeleteCandidateProfile(int id, int userId)
        {
            EnsureCandidateProfileTable();
            ExecuteSql(@"
UPDATE dbo.CandidateProfile
SET IsActive = 0,
    UpdatedBy = @UserId,
    UpdatedDate = GETDATE()
WHERE CandidateProfileId = @CandidateProfileId",
                new SqlParameter("@CandidateProfileId", id),
                new SqlParameter("@UserId", userId));
            AddCampaignAudit("Candidate Management", Convert.ToString(id), "Delete", userId, null, "Candidate profile deleted.", true);
            return true;
        }

        public List<SocialMediaPostVM> GetSocialMediaPosts(string keyword)
        {
            EnsureSocialMediaPostTable();

            DataTable table = QuerySql(@"
SELECT TOP 200 *
FROM dbo.SocialMediaPost
WHERE IsActive = 1
  AND (@Keyword IS NULL
    OR Platform LIKE @LikeKeyword
    OR ContentTitle LIKE @LikeKeyword
    OR ContentType LIKE @LikeKeyword
    OR ApprovalStatus LIKE @LikeKeyword
    OR PublishStatus LIKE @LikeKeyword
    OR AssignedTo LIKE @LikeKeyword)
ORDER BY ISNULL(ScheduledOn, CreatedDate) DESC",
                new SqlParameter("@Keyword", string.IsNullOrWhiteSpace(keyword) ? (object)DBNull.Value : keyword.Trim()),
                new SqlParameter("@LikeKeyword", "%" + (keyword ?? string.Empty).Trim() + "%"));

            return MapSocialMediaPosts(table);
        }

        public SocialMediaPostVM GetSocialMediaPostById(int id)
        {
            EnsureSocialMediaPostTable();

            DataTable table = QuerySql(@"
SELECT TOP 1 *
FROM dbo.SocialMediaPost
WHERE SocialMediaPostId = @SocialMediaPostId AND IsActive = 1",
                new SqlParameter("@SocialMediaPostId", id));

            return table.Rows.Count == 0 ? null : MapSocialMediaPost(table.Rows[0]);
        }

        public bool SaveSocialMediaPost(SocialMediaPostVM model, int userId)
        {
            EnsureSocialMediaPostTable();

            ExecuteSql(@"
INSERT INTO dbo.SocialMediaPost
    (Platform, ContentTitle, ContentType, Caption, MediaUrl, PublicUrl, ScheduledOn, AssignedTo,
     ApprovalStatus, PublishStatus, ReachCount, EngagementCount, ShareCount, CommentCount, ReviewRemarks, IsActive, CreatedBy)
VALUES
    (@Platform, @ContentTitle, @ContentType, @Caption, @MediaUrl, @PublicUrl, @ScheduledOn, @AssignedTo,
     @ApprovalStatus, @PublishStatus, @ReachCount, @EngagementCount, @ShareCount, @CommentCount, @ReviewRemarks, @IsActive, @UserId)",
                SocialMediaParameters(model, userId).ToArray());

            AddCampaignAudit("Social Media War Room", model.ContentTitle, "Create", userId, null, "Social media post created.", false);
            return true;
        }

        public bool UpdateSocialMediaPost(SocialMediaPostVM model, int userId)
        {
            EnsureSocialMediaPostTable();

            List<SqlParameter> parameters = SocialMediaParameters(model, userId);
            parameters.Add(new SqlParameter("@SocialMediaPostId", model.SocialMediaPostId));

            ExecuteSql(@"
UPDATE dbo.SocialMediaPost
SET Platform = @Platform,
    ContentTitle = @ContentTitle,
    ContentType = @ContentType,
    Caption = @Caption,
    MediaUrl = @MediaUrl,
    PublicUrl = @PublicUrl,
    ScheduledOn = @ScheduledOn,
    AssignedTo = @AssignedTo,
    ApprovalStatus = @ApprovalStatus,
    PublishStatus = @PublishStatus,
    ReachCount = @ReachCount,
    EngagementCount = @EngagementCount,
    ShareCount = @ShareCount,
    CommentCount = @CommentCount,
    ReviewRemarks = @ReviewRemarks,
    IsActive = @IsActive,
    UpdatedBy = @UserId,
    UpdatedDate = GETDATE()
WHERE SocialMediaPostId = @SocialMediaPostId",
                parameters.ToArray());

            AddCampaignAudit("Social Media War Room", Convert.ToString(model.SocialMediaPostId), "Update", userId, null, "Social media post updated.", false);
            return true;
        }

        public bool DeleteSocialMediaPost(int id, int userId)
        {
            EnsureSocialMediaPostTable();
            ExecuteSql(@"
UPDATE dbo.SocialMediaPost
SET IsActive = 0,
    UpdatedBy = @UserId,
    UpdatedDate = GETDATE()
WHERE SocialMediaPostId = @SocialMediaPostId",
                new SqlParameter("@SocialMediaPostId", id),
                new SqlParameter("@UserId", userId));
            AddCampaignAudit("Social Media War Room", Convert.ToString(id), "Delete", userId, null, "Social media post deleted.", false);
            return true;
        }

        public List<CampaignFinanceEntryVM> GetCampaignFinanceEntries(string keyword)
        {
            EnsureCampaignFinanceEntryTable();

            DataTable table = QuerySql(@"
SELECT TOP 200 *
FROM dbo.CampaignFinanceEntry
WHERE IsActive = 1
  AND (@Keyword IS NULL
    OR EntryType LIKE @LikeKeyword
    OR Title LIKE @LikeKeyword
    OR ReferenceNo LIKE @LikeKeyword
    OR PersonOrVendorName LIKE @LikeKeyword
    OR Category LIKE @LikeKeyword
    OR ApprovalStatus LIKE @LikeKeyword)
ORDER BY EntryDate DESC, CampaignFinanceEntryId DESC",
                new SqlParameter("@Keyword", string.IsNullOrWhiteSpace(keyword) ? (object)DBNull.Value : keyword.Trim()),
                new SqlParameter("@LikeKeyword", "%" + (keyword ?? string.Empty).Trim() + "%"));

            return MapCampaignFinanceEntries(table);
        }

        public CampaignFinanceEntryVM GetCampaignFinanceEntryById(int id)
        {
            EnsureCampaignFinanceEntryTable();

            DataTable table = QuerySql(@"
SELECT TOP 1 *
FROM dbo.CampaignFinanceEntry
WHERE CampaignFinanceEntryId = @CampaignFinanceEntryId AND IsActive = 1",
                new SqlParameter("@CampaignFinanceEntryId", id));

            return table.Rows.Count == 0 ? null : MapCampaignFinanceEntry(table.Rows[0]);
        }

        public bool SaveCampaignFinanceEntry(CampaignFinanceEntryVM model, int userId)
        {
            EnsureCampaignFinanceEntryTable();

            ExecuteSql(@"
INSERT INTO dbo.CampaignFinanceEntry
    (EntryType, Title, ReferenceNo, EntryDate, PersonOrVendorName, MobileNo, Category, PaymentMode,
     Amount, ProofUrl, ApprovalStatus, ApprovedBy, Remarks, IsActive, CreatedBy)
VALUES
    (@EntryType, @Title, @ReferenceNo, @EntryDate, @PersonOrVendorName, @MobileNo, @Category, @PaymentMode,
     @Amount, @ProofUrl, @ApprovalStatus, @ApprovedBy, @Remarks, @IsActive, @UserId)",
                CampaignFinanceParameters(model, userId).ToArray());

            AddCampaignAudit("Finance and Donations", model.Title, "Create", userId, null, "Finance entry created.", true);
            return true;
        }

        public bool UpdateCampaignFinanceEntry(CampaignFinanceEntryVM model, int userId)
        {
            EnsureCampaignFinanceEntryTable();

            List<SqlParameter> parameters = CampaignFinanceParameters(model, userId);
            parameters.Add(new SqlParameter("@CampaignFinanceEntryId", model.CampaignFinanceEntryId));

            ExecuteSql(@"
UPDATE dbo.CampaignFinanceEntry
SET EntryType = @EntryType,
    Title = @Title,
    ReferenceNo = @ReferenceNo,
    EntryDate = @EntryDate,
    PersonOrVendorName = @PersonOrVendorName,
    MobileNo = @MobileNo,
    Category = @Category,
    PaymentMode = @PaymentMode,
    Amount = @Amount,
    ProofUrl = @ProofUrl,
    ApprovalStatus = @ApprovalStatus,
    ApprovedBy = @ApprovedBy,
    Remarks = @Remarks,
    IsActive = @IsActive,
    UpdatedBy = @UserId,
    UpdatedDate = GETDATE()
WHERE CampaignFinanceEntryId = @CampaignFinanceEntryId",
                parameters.ToArray());

            AddCampaignAudit("Finance and Donations", Convert.ToString(model.CampaignFinanceEntryId), "Update", userId, null, "Finance entry updated.", true);
            return true;
        }

        public bool DeleteCampaignFinanceEntry(int id, int userId)
        {
            EnsureCampaignFinanceEntryTable();
            ExecuteSql(@"
UPDATE dbo.CampaignFinanceEntry
SET IsActive = 0,
    UpdatedBy = @UserId,
    UpdatedDate = GETDATE()
WHERE CampaignFinanceEntryId = @CampaignFinanceEntryId",
                new SqlParameter("@CampaignFinanceEntryId", id),
                new SqlParameter("@UserId", userId));
            AddCampaignAudit("Finance and Donations", Convert.ToString(id), "Delete", userId, null, "Finance entry deleted.", true);
            return true;
        }

        private void EnsureCampaignFinanceEntryTable()
        {
            ExecuteSql(@"IF OBJECT_ID('dbo.CampaignFinanceEntry', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CampaignFinanceEntry
    (
        CampaignFinanceEntryId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CampaignFinanceEntry PRIMARY KEY,
        EntryType NVARCHAR(40) NOT NULL,
        Title NVARCHAR(160) NOT NULL,
        ReferenceNo NVARCHAR(80) NULL,
        EntryDate DATETIME NOT NULL CONSTRAINT DF_CampaignFinanceEntry_Date DEFAULT(GETDATE()),
        PersonOrVendorName NVARCHAR(150) NULL,
        MobileNo NVARCHAR(30) NULL,
        Category NVARCHAR(120) NULL,
        PaymentMode NVARCHAR(80) NULL,
        Amount DECIMAL(18,2) NOT NULL CONSTRAINT DF_CampaignFinanceEntry_Amount DEFAULT(0),
        ProofUrl NVARCHAR(300) NULL,
        ApprovalStatus NVARCHAR(40) NOT NULL CONSTRAINT DF_CampaignFinanceEntry_Approval DEFAULT('Pending'),
        ApprovedBy NVARCHAR(120) NULL,
        Remarks NVARCHAR(500) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_CampaignFinanceEntry_Active DEFAULT(1),
        CreatedBy INT NULL,
        CreatedDate DATETIME NOT NULL CONSTRAINT DF_CampaignFinanceEntry_Created DEFAULT(GETDATE()),
        UpdatedBy INT NULL,
        UpdatedDate DATETIME NULL
    );
END");
        }

        private void EnsureSocialMediaPostTable()
        {
            ExecuteSql(@"IF OBJECT_ID('dbo.SocialMediaPost', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SocialMediaPost
    (
        SocialMediaPostId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SocialMediaPost PRIMARY KEY,
        Platform NVARCHAR(120) NOT NULL,
        ContentTitle NVARCHAR(180) NOT NULL,
        ContentType NVARCHAR(80) NULL,
        Caption NVARCHAR(600) NULL,
        MediaUrl NVARCHAR(300) NULL,
        PublicUrl NVARCHAR(300) NULL,
        ScheduledOn DATETIME NULL,
        AssignedTo NVARCHAR(80) NULL,
        ApprovalStatus NVARCHAR(40) NOT NULL CONSTRAINT DF_SocialMediaPost_Approval DEFAULT('Draft'),
        PublishStatus NVARCHAR(40) NOT NULL CONSTRAINT DF_SocialMediaPost_Publish DEFAULT('Planned'),
        ReachCount INT NOT NULL CONSTRAINT DF_SocialMediaPost_Reach DEFAULT(0),
        EngagementCount INT NOT NULL CONSTRAINT DF_SocialMediaPost_Engagement DEFAULT(0),
        ShareCount INT NOT NULL CONSTRAINT DF_SocialMediaPost_Share DEFAULT(0),
        CommentCount INT NOT NULL CONSTRAINT DF_SocialMediaPost_Comment DEFAULT(0),
        ReviewRemarks NVARCHAR(300) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_SocialMediaPost_Active DEFAULT(1),
        CreatedBy INT NULL,
        CreatedDate DATETIME NOT NULL CONSTRAINT DF_SocialMediaPost_Created DEFAULT(GETDATE()),
        UpdatedBy INT NULL,
        UpdatedDate DATETIME NULL
    );
END");
        }

        private void EnsureCandidateProfileTable()
        {
            ExecuteSql(@"IF OBJECT_ID('dbo.CandidateProfile', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CandidateProfile
    (
        CandidateProfileId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CandidateProfile PRIMARY KEY,
        FullName NVARCHAR(150) NOT NULL,
        PartyName NVARCHAR(100) NULL,
        ElectionType NVARCHAR(120) NULL,
        ConstituencyName NVARCHAR(150) NULL,
        District NVARCHAR(120) NULL,
        State NVARCHAR(120) NULL,
        Education NVARCHAR(200) NULL,
        Profession NVARCHAR(150) NULL,
        PublicBio NVARCHAR(500) NULL,
        ManifestoUrl NVARCHAR(300) NULL,
        AffidavitUrl NVARCHAR(300) NULL,
        PhotoUrl NVARCHAR(300) NULL,
        FacebookUrl NVARCHAR(300) NULL,
        TwitterUrl NVARCHAR(300) NULL,
        InstagramUrl NVARCHAR(300) NULL,
        YouTubeUrl NVARCHAR(300) NULL,
        DeclaredAssets DECIMAL(18,2) NOT NULL CONSTRAINT DF_CandidateProfile_Assets DEFAULT(0),
        DeclaredLiabilities DECIMAL(18,2) NOT NULL CONSTRAINT DF_CandidateProfile_Liabilities DEFAULT(0),
        CriminalCaseSummary NVARCHAR(500) NULL,
        ApprovalStatus NVARCHAR(40) NOT NULL CONSTRAINT DF_CandidateProfile_Approval DEFAULT('Draft'),
        IsPublished BIT NOT NULL CONSTRAINT DF_CandidateProfile_Published DEFAULT(0),
        IsActive BIT NOT NULL CONSTRAINT DF_CandidateProfile_Active DEFAULT(1),
        CreatedBy INT NULL,
        CreatedDate DATETIME NOT NULL CONSTRAINT DF_CandidateProfile_Created DEFAULT(GETDATE()),
        UpdatedBy INT NULL,
        UpdatedDate DATETIME NULL
    );
END");
        }

        private static List<SqlParameter> CandidateParameters(CandidateProfileVM model, int userId)
        {
            return new List<SqlParameter>
            {
                new SqlParameter("@FullName", model.FullName),
                new SqlParameter("@PartyName", (object)model.PartyName ?? DBNull.Value),
                new SqlParameter("@ElectionType", (object)model.ElectionType ?? DBNull.Value),
                new SqlParameter("@ConstituencyName", (object)model.ConstituencyName ?? DBNull.Value),
                new SqlParameter("@District", (object)model.District ?? DBNull.Value),
                new SqlParameter("@State", (object)model.State ?? DBNull.Value),
                new SqlParameter("@Education", (object)model.Education ?? DBNull.Value),
                new SqlParameter("@Profession", (object)model.Profession ?? DBNull.Value),
                new SqlParameter("@PublicBio", (object)model.PublicBio ?? DBNull.Value),
                new SqlParameter("@ManifestoUrl", (object)model.ManifestoUrl ?? DBNull.Value),
                new SqlParameter("@AffidavitUrl", (object)model.AffidavitUrl ?? DBNull.Value),
                new SqlParameter("@PhotoUrl", (object)model.PhotoUrl ?? DBNull.Value),
                new SqlParameter("@FacebookUrl", (object)model.FacebookUrl ?? DBNull.Value),
                new SqlParameter("@TwitterUrl", (object)model.TwitterUrl ?? DBNull.Value),
                new SqlParameter("@InstagramUrl", (object)model.InstagramUrl ?? DBNull.Value),
                new SqlParameter("@YouTubeUrl", (object)model.YouTubeUrl ?? DBNull.Value),
                new SqlParameter("@DeclaredAssets", model.DeclaredAssets),
                new SqlParameter("@DeclaredLiabilities", model.DeclaredLiabilities),
                new SqlParameter("@CriminalCaseSummary", (object)model.CriminalCaseSummary ?? DBNull.Value),
                new SqlParameter("@ApprovalStatus", string.IsNullOrWhiteSpace(model.ApprovalStatus) ? "Draft" : model.ApprovalStatus),
                new SqlParameter("@IsPublished", model.IsPublished),
                new SqlParameter("@IsActive", model.IsActive),
                new SqlParameter("@UserId", userId)
            };
        }

        private static List<CandidateProfileVM> MapCandidateProfiles(DataTable table)
        {
            List<CandidateProfileVM> items = new List<CandidateProfileVM>();
            foreach (DataRow row in table.Rows)
            {
                items.Add(MapCandidateProfile(row));
            }

            return items;
        }

        private static CandidateProfileVM MapCandidateProfile(DataRow row)
        {
            return new CandidateProfileVM
            {
                CandidateProfileId = Convert.ToInt32(row["CandidateProfileId"]),
                FullName = Convert.ToString(row["FullName"]),
                PartyName = Convert.ToString(row["PartyName"]),
                ElectionType = Convert.ToString(row["ElectionType"]),
                ConstituencyName = Convert.ToString(row["ConstituencyName"]),
                District = Convert.ToString(row["District"]),
                State = Convert.ToString(row["State"]),
                Education = Convert.ToString(row["Education"]),
                Profession = Convert.ToString(row["Profession"]),
                PublicBio = Convert.ToString(row["PublicBio"]),
                ManifestoUrl = Convert.ToString(row["ManifestoUrl"]),
                AffidavitUrl = Convert.ToString(row["AffidavitUrl"]),
                PhotoUrl = Convert.ToString(row["PhotoUrl"]),
                FacebookUrl = Convert.ToString(row["FacebookUrl"]),
                TwitterUrl = Convert.ToString(row["TwitterUrl"]),
                InstagramUrl = Convert.ToString(row["InstagramUrl"]),
                YouTubeUrl = Convert.ToString(row["YouTubeUrl"]),
                DeclaredAssets = row["DeclaredAssets"] == DBNull.Value ? 0 : Convert.ToDecimal(row["DeclaredAssets"]),
                DeclaredLiabilities = row["DeclaredLiabilities"] == DBNull.Value ? 0 : Convert.ToDecimal(row["DeclaredLiabilities"]),
                CriminalCaseSummary = Convert.ToString(row["CriminalCaseSummary"]),
                ApprovalStatus = Convert.ToString(row["ApprovalStatus"]),
                IsPublished = row["IsPublished"] != DBNull.Value && Convert.ToBoolean(row["IsPublished"]),
                IsActive = row["IsActive"] == DBNull.Value || Convert.ToBoolean(row["IsActive"]),
                CreatedDate = row["CreatedDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(row["CreatedDate"])
            };
        }

        private static List<SqlParameter> SocialMediaParameters(SocialMediaPostVM model, int userId)
        {
            return new List<SqlParameter>
            {
                new SqlParameter("@Platform", model.Platform),
                new SqlParameter("@ContentTitle", model.ContentTitle),
                new SqlParameter("@ContentType", (object)model.ContentType ?? DBNull.Value),
                new SqlParameter("@Caption", (object)model.Caption ?? DBNull.Value),
                new SqlParameter("@MediaUrl", (object)model.MediaUrl ?? DBNull.Value),
                new SqlParameter("@PublicUrl", (object)model.PublicUrl ?? DBNull.Value),
                new SqlParameter("@ScheduledOn", (object)model.ScheduledOn ?? DBNull.Value),
                new SqlParameter("@AssignedTo", (object)model.AssignedTo ?? DBNull.Value),
                new SqlParameter("@ApprovalStatus", string.IsNullOrWhiteSpace(model.ApprovalStatus) ? "Draft" : model.ApprovalStatus),
                new SqlParameter("@PublishStatus", string.IsNullOrWhiteSpace(model.PublishStatus) ? "Planned" : model.PublishStatus),
                new SqlParameter("@ReachCount", model.ReachCount),
                new SqlParameter("@EngagementCount", model.EngagementCount),
                new SqlParameter("@ShareCount", model.ShareCount),
                new SqlParameter("@CommentCount", model.CommentCount),
                new SqlParameter("@ReviewRemarks", (object)model.ReviewRemarks ?? DBNull.Value),
                new SqlParameter("@IsActive", model.IsActive),
                new SqlParameter("@UserId", userId)
            };
        }

        private static List<SocialMediaPostVM> MapSocialMediaPosts(DataTable table)
        {
            List<SocialMediaPostVM> items = new List<SocialMediaPostVM>();
            foreach (DataRow row in table.Rows)
            {
                items.Add(MapSocialMediaPost(row));
            }

            return items;
        }

        private static SocialMediaPostVM MapSocialMediaPost(DataRow row)
        {
            return new SocialMediaPostVM
            {
                SocialMediaPostId = Convert.ToInt32(row["SocialMediaPostId"]),
                Platform = Convert.ToString(row["Platform"]),
                ContentTitle = Convert.ToString(row["ContentTitle"]),
                ContentType = Convert.ToString(row["ContentType"]),
                Caption = Convert.ToString(row["Caption"]),
                MediaUrl = Convert.ToString(row["MediaUrl"]),
                PublicUrl = Convert.ToString(row["PublicUrl"]),
                ScheduledOn = row["ScheduledOn"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["ScheduledOn"]),
                AssignedTo = Convert.ToString(row["AssignedTo"]),
                ApprovalStatus = Convert.ToString(row["ApprovalStatus"]),
                PublishStatus = Convert.ToString(row["PublishStatus"]),
                ReachCount = row["ReachCount"] == DBNull.Value ? 0 : Convert.ToInt32(row["ReachCount"]),
                EngagementCount = row["EngagementCount"] == DBNull.Value ? 0 : Convert.ToInt32(row["EngagementCount"]),
                ShareCount = row["ShareCount"] == DBNull.Value ? 0 : Convert.ToInt32(row["ShareCount"]),
                CommentCount = row["CommentCount"] == DBNull.Value ? 0 : Convert.ToInt32(row["CommentCount"]),
                ReviewRemarks = Convert.ToString(row["ReviewRemarks"]),
                IsActive = row["IsActive"] == DBNull.Value || Convert.ToBoolean(row["IsActive"]),
                CreatedDate = row["CreatedDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(row["CreatedDate"])
            };
        }

        private static List<SqlParameter> CampaignFinanceParameters(CampaignFinanceEntryVM model, int userId)
        {
            return new List<SqlParameter>
            {
                new SqlParameter("@EntryType", model.EntryType),
                new SqlParameter("@Title", model.Title),
                new SqlParameter("@ReferenceNo", (object)model.ReferenceNo ?? DBNull.Value),
                new SqlParameter("@EntryDate", model.EntryDate == DateTime.MinValue ? DateTime.Today : model.EntryDate),
                new SqlParameter("@PersonOrVendorName", (object)model.PersonOrVendorName ?? DBNull.Value),
                new SqlParameter("@MobileNo", (object)model.MobileNo ?? DBNull.Value),
                new SqlParameter("@Category", (object)model.Category ?? DBNull.Value),
                new SqlParameter("@PaymentMode", (object)model.PaymentMode ?? DBNull.Value),
                new SqlParameter("@Amount", model.Amount),
                new SqlParameter("@ProofUrl", (object)model.ProofUrl ?? DBNull.Value),
                new SqlParameter("@ApprovalStatus", string.IsNullOrWhiteSpace(model.ApprovalStatus) ? "Pending" : model.ApprovalStatus),
                new SqlParameter("@ApprovedBy", (object)model.ApprovedBy ?? DBNull.Value),
                new SqlParameter("@Remarks", (object)model.Remarks ?? DBNull.Value),
                new SqlParameter("@IsActive", model.IsActive),
                new SqlParameter("@UserId", userId)
            };
        }

        private static List<CampaignFinanceEntryVM> MapCampaignFinanceEntries(DataTable table)
        {
            List<CampaignFinanceEntryVM> items = new List<CampaignFinanceEntryVM>();
            foreach (DataRow row in table.Rows)
            {
                items.Add(MapCampaignFinanceEntry(row));
            }

            return items;
        }

        private static CampaignFinanceEntryVM MapCampaignFinanceEntry(DataRow row)
        {
            return new CampaignFinanceEntryVM
            {
                CampaignFinanceEntryId = Convert.ToInt32(row["CampaignFinanceEntryId"]),
                EntryType = Convert.ToString(row["EntryType"]),
                Title = Convert.ToString(row["Title"]),
                ReferenceNo = Convert.ToString(row["ReferenceNo"]),
                EntryDate = row["EntryDate"] == DBNull.Value ? DateTime.Today : Convert.ToDateTime(row["EntryDate"]),
                PersonOrVendorName = Convert.ToString(row["PersonOrVendorName"]),
                MobileNo = Convert.ToString(row["MobileNo"]),
                Category = Convert.ToString(row["Category"]),
                PaymentMode = Convert.ToString(row["PaymentMode"]),
                Amount = row["Amount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Amount"]),
                ProofUrl = Convert.ToString(row["ProofUrl"]),
                ApprovalStatus = Convert.ToString(row["ApprovalStatus"]),
                ApprovedBy = Convert.ToString(row["ApprovedBy"]),
                Remarks = Convert.ToString(row["Remarks"]),
                IsActive = row["IsActive"] == DBNull.Value || Convert.ToBoolean(row["IsActive"]),
                CreatedDate = row["CreatedDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(row["CreatedDate"])
            };
        }


        public List<CampaignOperationItemVM> GetCampaignOperationItems(string moduleKey, string keyword)
        {
            EnsureCampaignOperationItemTable();
            DataTable table = QuerySql(@"
SELECT TOP 300 *
FROM dbo.CampaignOperationItem
WHERE IsActive = 1
  AND ModuleKey = @ModuleKey
  AND (@Keyword IS NULL OR Title LIKE @Search OR Category LIKE @Search OR OwnerName LIKE @Search OR AreaName LIKE @Search OR Status LIKE @Search)
ORDER BY CreatedOn DESC",
                new SqlParameter("@ModuleKey", moduleKey),
                new SqlParameter("@Keyword", string.IsNullOrWhiteSpace(keyword) ? (object)DBNull.Value : keyword),
                new SqlParameter("@Search", "%" + (keyword ?? string.Empty) + "%"));
            return MapCampaignOperationItems(table);
        }

        public CampaignOperationItemVM GetCampaignOperationItemById(int id)
        {
            EnsureCampaignOperationItemTable();
            DataTable table = QuerySql("SELECT TOP 1 * FROM dbo.CampaignOperationItem WHERE CampaignOperationItemId = @Id AND IsActive = 1", new SqlParameter("@Id", id));
            return MapCampaignOperationItems(table).FirstOrDefault();
        }

        public bool SaveCampaignOperationItem(CampaignOperationItemVM model, int userId)
        {
            EnsureCampaignOperationItemTable();
            ExecuteSql(@"
INSERT INTO dbo.CampaignOperationItem
(ModuleKey, Title, Category, OwnerName, OwnerMobile, AreaName, Priority, Status, StartDate, DueDate, Quantity, BudgetAmount, ReferenceUrl, Description, ComplianceNote, IsApproved, IsActive, CreatedBy, CreatedOn)
VALUES (@ModuleKey, @Title, @Category, @OwnerName, @OwnerMobile, @AreaName, @Priority, @Status, @StartDate, @DueDate, @Quantity, @BudgetAmount, @ReferenceUrl, @Description, @ComplianceNote, @IsApproved, @IsActive, @UserId, GETDATE())",
                CampaignOperationParameters(model, userId).ToArray());
            AddCampaignAudit("Campaign Operations", model.ModuleKey, "Create", userId, null, model.Title, false);
            return true;
        }

        public bool UpdateCampaignOperationItem(CampaignOperationItemVM model, int userId)
        {
            EnsureCampaignOperationItemTable();
            List<SqlParameter> parameters = CampaignOperationParameters(model, userId);
            parameters.Add(new SqlParameter("@Id", model.CampaignOperationItemId));
            ExecuteSql(@"
UPDATE dbo.CampaignOperationItem
SET ModuleKey = @ModuleKey, Title = @Title, Category = @Category, OwnerName = @OwnerName, OwnerMobile = @OwnerMobile,
    AreaName = @AreaName, Priority = @Priority, Status = @Status, StartDate = @StartDate, DueDate = @DueDate,
    Quantity = @Quantity, BudgetAmount = @BudgetAmount, ReferenceUrl = @ReferenceUrl, Description = @Description,
    ComplianceNote = @ComplianceNote, IsApproved = @IsApproved, IsActive = @IsActive, UpdatedBy = @UserId, UpdatedOn = GETDATE()
WHERE CampaignOperationItemId = @Id", parameters.ToArray());
            AddCampaignAudit("Campaign Operations", Convert.ToString(model.CampaignOperationItemId), "Update", userId, model.ModuleKey, model.Title, false);
            return true;
        }

        public bool DeleteCampaignOperationItem(int id, int userId)
        {
            EnsureCampaignOperationItemTable();
            ExecuteSql("UPDATE dbo.CampaignOperationItem SET IsActive = 0, UpdatedBy = @UserId, UpdatedOn = GETDATE() WHERE CampaignOperationItemId = @Id", new SqlParameter("@Id", id), new SqlParameter("@UserId", userId));
            AddCampaignAudit("Campaign Operations", Convert.ToString(id), "Delete", userId, null, "Campaign operation item deleted.", false);
            return true;
        }

        private void EnsureCampaignOperationItemTable()
        {
            ExecuteSql(@"IF OBJECT_ID('dbo.CampaignOperationItem', 'U') IS NULL
BEGIN
CREATE TABLE dbo.CampaignOperationItem
(
    CampaignOperationItemId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ModuleKey NVARCHAR(80) NOT NULL,
    Title NVARCHAR(220) NOT NULL,
    Category NVARCHAR(100) NULL,
    OwnerName NVARCHAR(150) NULL,
    OwnerMobile NVARCHAR(30) NULL,
    AreaName NVARCHAR(150) NULL,
    Priority NVARCHAR(40) NULL,
    Status NVARCHAR(50) NULL,
    StartDate DATE NULL,
    DueDate DATE NULL,
    Quantity INT NULL,
    BudgetAmount DECIMAL(18,2) NULL,
    ReferenceUrl NVARCHAR(500) NULL,
    Description NVARCHAR(1200) NULL,
    ComplianceNote NVARCHAR(800) NULL,
    IsApproved BIT NOT NULL DEFAULT(0),
    IsActive BIT NOT NULL DEFAULT(1),
    CreatedBy INT NULL,
    CreatedOn DATETIME NOT NULL DEFAULT(GETDATE()),
    UpdatedBy INT NULL,
    UpdatedOn DATETIME NULL
);
END;");
        }

        private List<SqlParameter> CampaignOperationParameters(CampaignOperationItemVM model, int userId)
        {
            return new List<SqlParameter>
            {
                new SqlParameter("@ModuleKey", model.ModuleKey),
                new SqlParameter("@Title", model.Title),
                new SqlParameter("@Category", (object)model.Category ?? DBNull.Value),
                new SqlParameter("@OwnerName", (object)model.OwnerName ?? DBNull.Value),
                new SqlParameter("@OwnerMobile", (object)model.OwnerMobile ?? DBNull.Value),
                new SqlParameter("@AreaName", (object)model.AreaName ?? DBNull.Value),
                new SqlParameter("@Priority", (object)model.Priority ?? DBNull.Value),
                new SqlParameter("@Status", (object)model.Status ?? DBNull.Value),
                new SqlParameter("@StartDate", (object)model.StartDate ?? DBNull.Value),
                new SqlParameter("@DueDate", (object)model.DueDate ?? DBNull.Value),
                new SqlParameter("@Quantity", (object)model.Quantity ?? DBNull.Value),
                new SqlParameter("@BudgetAmount", (object)model.BudgetAmount ?? DBNull.Value),
                new SqlParameter("@ReferenceUrl", (object)model.ReferenceUrl ?? DBNull.Value),
                new SqlParameter("@Description", (object)model.Description ?? DBNull.Value),
                new SqlParameter("@ComplianceNote", (object)model.ComplianceNote ?? DBNull.Value),
                new SqlParameter("@IsApproved", model.IsApproved),
                new SqlParameter("@IsActive", model.IsActive),
                new SqlParameter("@UserId", userId)
            };
        }

        private List<CampaignOperationItemVM> MapCampaignOperationItems(DataTable table)
        {
            List<CampaignOperationItemVM> items = new List<CampaignOperationItemVM>();
            foreach (DataRow row in table.Rows)
            {
                items.Add(new CampaignOperationItemVM
                {
                    CampaignOperationItemId = Convert.ToInt32(row["CampaignOperationItemId"]),
                    ModuleKey = Convert.ToString(row["ModuleKey"]),
                    Title = Convert.ToString(row["Title"]),
                    Category = Convert.ToString(row["Category"]),
                    OwnerName = Convert.ToString(row["OwnerName"]),
                    OwnerMobile = Convert.ToString(row["OwnerMobile"]),
                    AreaName = Convert.ToString(row["AreaName"]),
                    Priority = Convert.ToString(row["Priority"]),
                    Status = Convert.ToString(row["Status"]),
                    StartDate = row["StartDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["StartDate"]),
                    DueDate = row["DueDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["DueDate"]),
                    Quantity = row["Quantity"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["Quantity"]),
                    BudgetAmount = row["BudgetAmount"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["BudgetAmount"]),
                    ReferenceUrl = Convert.ToString(row["ReferenceUrl"]),
                    Description = Convert.ToString(row["Description"]),
                    ComplianceNote = Convert.ToString(row["ComplianceNote"]),
                    IsApproved = row["IsApproved"] != DBNull.Value && Convert.ToBoolean(row["IsApproved"]),
                    IsActive = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"]),
                    CreatedOn = row["CreatedOn"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(row["CreatedOn"]),
                    UpdatedOn = row["UpdatedOn"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["UpdatedOn"])
                });
            }
            return items;
        }
        public List<CampaignAuditLogVM> GetCampaignAuditLogs(string keyword)
        {
            EnsureCampaignAuditLogTable();

            DataTable table = QuerySql(@"
SELECT TOP 300 *
FROM dbo.CampaignAuditLog
WHERE @Keyword IS NULL
   OR ModuleName LIKE @LikeKeyword
   OR ActionName LIKE @LikeKeyword
   OR PerformedBy LIKE @LikeKeyword
   OR RecordId LIKE @LikeKeyword
   OR Remarks LIKE @LikeKeyword
ORDER BY PerformedOn DESC, CampaignAuditLogId DESC",
                new SqlParameter("@Keyword", string.IsNullOrWhiteSpace(keyword) ? (object)DBNull.Value : keyword.Trim()),
                new SqlParameter("@LikeKeyword", "%" + (keyword ?? string.Empty).Trim() + "%"));

            return MapCampaignAuditLogs(table);
        }


        public List<CampaignPollVM> GetCampaignPolls(string keyword)
        {
            EnsureCampaignPollTables();
            DataTable table = QuerySql(@"
SELECT TOP 200 p.*,
       (SELECT COUNT(1) FROM dbo.CampaignPollResponse r WHERE r.CampaignPollId = p.CampaignPollId) AS TotalResponses
FROM dbo.CampaignPoll p
WHERE p.IsActive = 1
  AND (@Keyword IS NULL OR p.Title LIKE @Search OR p.Question LIKE @Search OR p.TargetArea LIKE @Search OR p.PollType LIKE @Search)
ORDER BY p.CreatedOn DESC",
                new SqlParameter("@Keyword", string.IsNullOrWhiteSpace(keyword) ? (object)DBNull.Value : keyword),
                new SqlParameter("@Search", "%" + (keyword ?? string.Empty) + "%"));
            return MapCampaignPolls(table, null);
        }

        public CampaignPollVM GetCampaignPollById(int id, string baseUrl)
        {
            EnsureCampaignPollTables();
            DataTable table = QuerySql(@"SELECT TOP 1 * FROM dbo.CampaignPoll WHERE CampaignPollId = @CampaignPollId", new SqlParameter("@CampaignPollId", id));
            CampaignPollVM model = MapCampaignPolls(table, baseUrl).FirstOrDefault();
            if (model == null) return null;
            LoadCampaignPollDetails(model);
            return model;
        }

        public CampaignPollVM GetPublicCampaignPoll(string slug, string baseUrl)
        {
            EnsureCampaignPollTables();
            DataTable table = QuerySql(@"
SELECT TOP 1 *
FROM dbo.CampaignPoll
WHERE PublicSlug = @PublicSlug AND IsActive = 1
  AND (StartDate IS NULL OR StartDate <= CAST(GETDATE() AS DATE))
  AND (EndDate IS NULL OR EndDate >= CAST(GETDATE() AS DATE))",
                new SqlParameter("@PublicSlug", slug));
            CampaignPollVM model = MapCampaignPolls(table, baseUrl).FirstOrDefault();
            if (model == null) return null;
            LoadCampaignPollDetails(model);
            return model;
        }

        public bool SaveCampaignPoll(CampaignPollVM model, int userId, string baseUrl)
        {
            EnsureCampaignPollTables();
            model.PublicSlug = CreatePollSlug(model.Title);
            ExecuteSql(@"
INSERT INTO dbo.CampaignPoll
(Title, Question, Description, TargetArea, PollType, PublicSlug, StartDate, EndDate, ShowPublicResults, RequireConsent, IsActive, CreatedBy, CreatedOn)
VALUES (@Title, @Question, @Description, @TargetArea, @PollType, @PublicSlug, @StartDate, @EndDate, @ShowPublicResults, @RequireConsent, @IsActive, @UserId, GETDATE())",
                PollParameters(model, userId).ToArray());
            int pollId = Convert.ToInt32(QuerySql("SELECT CampaignPollId FROM dbo.CampaignPoll WHERE PublicSlug = @PublicSlug", new SqlParameter("@PublicSlug", model.PublicSlug)).Rows[0]["CampaignPollId"]);
            SaveCampaignPollOptions(pollId, model.Options);
            AddCampaignAudit("Poll & Survey Management", model.Title, "Create", userId, null, "Poll created with public share link.", false);
            return true;
        }

        public bool UpdateCampaignPoll(CampaignPollVM model, int userId, string baseUrl)
        {
            EnsureCampaignPollTables();
            List<SqlParameter> parameters = PollParameters(model, userId);
            parameters.Add(new SqlParameter("@CampaignPollId", model.CampaignPollId));
            ExecuteSql(@"
UPDATE dbo.CampaignPoll
SET Title = @Title, Question = @Question, Description = @Description, TargetArea = @TargetArea, PollType = @PollType,
    StartDate = @StartDate, EndDate = @EndDate, ShowPublicResults = @ShowPublicResults, RequireConsent = @RequireConsent,
    IsActive = @IsActive, UpdatedBy = @UserId, UpdatedOn = GETDATE()
WHERE CampaignPollId = @CampaignPollId", parameters.ToArray());
            ExecuteSql("UPDATE dbo.CampaignPollOption SET IsActive = 0 WHERE CampaignPollId = @CampaignPollId", new SqlParameter("@CampaignPollId", model.CampaignPollId));
            SaveCampaignPollOptions(model.CampaignPollId, model.Options);
            AddCampaignAudit("Poll & Survey Management", Convert.ToString(model.CampaignPollId), "Update", userId, null, "Poll updated.", false);
            return true;
        }

        public bool DeleteCampaignPoll(int id, int userId)
        {
            EnsureCampaignPollTables();
            ExecuteSql("UPDATE dbo.CampaignPoll SET IsActive = 0, UpdatedBy = @UserId, UpdatedOn = GETDATE() WHERE CampaignPollId = @CampaignPollId", new SqlParameter("@CampaignPollId", id), new SqlParameter("@UserId", userId));
            AddCampaignAudit("Poll & Survey Management", Convert.ToString(id), "Delete", userId, null, "Poll deactivated.", false);
            return true;
        }

        public bool SaveCampaignPollResponse(string slug, CampaignPollResponseVM model, string ipAddress, string userAgent)
        {
            CampaignPollVM poll = GetPublicCampaignPoll(slug, string.Empty);
            if (poll == null) return false;
            ExecuteSql(@"
INSERT INTO dbo.CampaignPollResponse
(CampaignPollId, CampaignPollOptionId, RespondentName, MobileNo, AreaName, Source, IpAddress, UserAgent, ConsentGiven, Remarks, SubmittedOn)
VALUES (@CampaignPollId, @CampaignPollOptionId, @RespondentName, @MobileNo, @AreaName, @Source, @IpAddress, @UserAgent, @ConsentGiven, @Remarks, GETDATE())",
                new SqlParameter("@CampaignPollId", poll.CampaignPollId),
                new SqlParameter("@CampaignPollOptionId", model.CampaignPollOptionId),
                new SqlParameter("@RespondentName", (object)model.RespondentName ?? DBNull.Value),
                new SqlParameter("@MobileNo", (object)model.MobileNo ?? DBNull.Value),
                new SqlParameter("@AreaName", (object)model.AreaName ?? DBNull.Value),
                new SqlParameter("@Source", (object)model.Source ?? "web"),
                new SqlParameter("@IpAddress", (object)ipAddress ?? DBNull.Value),
                new SqlParameter("@UserAgent", (object)userAgent ?? DBNull.Value),
                new SqlParameter("@ConsentGiven", model.ConsentGiven),
                new SqlParameter("@Remarks", (object)model.Remarks ?? DBNull.Value));
            return true;
        }

        private void EnsureCampaignPollTables()
        {
            ExecuteSql(@"IF OBJECT_ID('dbo.CampaignPoll', 'U') IS NULL
BEGIN
CREATE TABLE dbo.CampaignPoll
(
    CampaignPollId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    Question NVARCHAR(500) NOT NULL,
    Description NVARCHAR(1000) NULL,
    TargetArea NVARCHAR(150) NULL,
    PollType NVARCHAR(80) NULL,
    PublicSlug NVARCHAR(160) NOT NULL,
    StartDate DATE NULL,
    EndDate DATE NULL,
    ShowPublicResults BIT NOT NULL DEFAULT(0),
    RequireConsent BIT NOT NULL DEFAULT(1),
    IsActive BIT NOT NULL DEFAULT(1),
    CreatedBy INT NULL,
    CreatedOn DATETIME NOT NULL DEFAULT(GETDATE()),
    UpdatedBy INT NULL,
    UpdatedOn DATETIME NULL
);
END;
IF OBJECT_ID('dbo.CampaignPollOption', 'U') IS NULL
BEGIN
CREATE TABLE dbo.CampaignPollOption
(
    CampaignPollOptionId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CampaignPollId INT NOT NULL,
    OptionText NVARCHAR(250) NOT NULL,
    DisplayOrder INT NOT NULL DEFAULT(0),
    IsActive BIT NOT NULL DEFAULT(1)
);
END;
IF OBJECT_ID('dbo.CampaignPollResponse', 'U') IS NULL
BEGIN
CREATE TABLE dbo.CampaignPollResponse
(
    CampaignPollResponseId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CampaignPollId INT NOT NULL,
    CampaignPollOptionId INT NOT NULL,
    RespondentName NVARCHAR(150) NULL,
    MobileNo NVARCHAR(30) NULL,
    AreaName NVARCHAR(150) NULL,
    Source NVARCHAR(50) NULL,
    IpAddress NVARCHAR(64) NULL,
    UserAgent NVARCHAR(300) NULL,
    ConsentGiven BIT NOT NULL DEFAULT(0),
    Remarks NVARCHAR(500) NULL,
    SubmittedOn DATETIME NOT NULL DEFAULT(GETDATE())
);
END;");
        }

        private List<SqlParameter> PollParameters(CampaignPollVM model, int userId)
        {
            return new List<SqlParameter>
            {
                new SqlParameter("@Title", model.Title),
                new SqlParameter("@Question", model.Question),
                new SqlParameter("@Description", (object)model.Description ?? DBNull.Value),
                new SqlParameter("@TargetArea", (object)model.TargetArea ?? DBNull.Value),
                new SqlParameter("@PollType", (object)model.PollType ?? DBNull.Value),
                new SqlParameter("@PublicSlug", (object)model.PublicSlug ?? DBNull.Value),
                new SqlParameter("@StartDate", (object)model.StartDate ?? DBNull.Value),
                new SqlParameter("@EndDate", (object)model.EndDate ?? DBNull.Value),
                new SqlParameter("@ShowPublicResults", model.ShowPublicResults),
                new SqlParameter("@RequireConsent", model.RequireConsent),
                new SqlParameter("@IsActive", model.IsActive),
                new SqlParameter("@UserId", userId)
            };
        }

        private void SaveCampaignPollOptions(int pollId, IEnumerable<CampaignPollOptionVM> options)
        {
            int order = 1;
            foreach (CampaignPollOptionVM option in options.Where(x => !string.IsNullOrWhiteSpace(x.OptionText)))
            {
                ExecuteSql("INSERT INTO dbo.CampaignPollOption (CampaignPollId, OptionText, DisplayOrder, IsActive) VALUES (@CampaignPollId, @OptionText, @DisplayOrder, 1)",
                    new SqlParameter("@CampaignPollId", pollId),
                    new SqlParameter("@OptionText", option.OptionText.Trim()),
                    new SqlParameter("@DisplayOrder", order++));
            }
        }

        private void LoadCampaignPollDetails(CampaignPollVM model)
        {
            DataTable options = QuerySql(@"
SELECT o.*, COUNT(r.CampaignPollResponseId) AS ResponseCount
FROM dbo.CampaignPollOption o
LEFT JOIN dbo.CampaignPollResponse r ON r.CampaignPollOptionId = o.CampaignPollOptionId
WHERE o.CampaignPollId = @CampaignPollId AND o.IsActive = 1
GROUP BY o.CampaignPollOptionId, o.CampaignPollId, o.OptionText, o.DisplayOrder, o.IsActive
ORDER BY o.DisplayOrder", new SqlParameter("@CampaignPollId", model.CampaignPollId));
            model.Options = MapCampaignPollOptions(options);
            model.TotalResponses = model.Options.Sum(x => x.ResponseCount);
            foreach (CampaignPollOptionVM option in model.Options)
            {
                option.ResponsePercent = model.TotalResponses == 0 ? 0 : Math.Round((decimal)option.ResponseCount * 100 / model.TotalResponses, 2);
            }
            DataTable responses = QuerySql(@"
SELECT TOP 100 r.*, o.OptionText
FROM dbo.CampaignPollResponse r
INNER JOIN dbo.CampaignPollOption o ON o.CampaignPollOptionId = r.CampaignPollOptionId
WHERE r.CampaignPollId = @CampaignPollId
ORDER BY r.SubmittedOn DESC", new SqlParameter("@CampaignPollId", model.CampaignPollId));
            model.Responses = MapCampaignPollResponses(responses);
        }

        private List<CampaignPollVM> MapCampaignPolls(DataTable table, string baseUrl)
        {
            List<CampaignPollVM> records = new List<CampaignPollVM>();
            foreach (DataRow row in table.Rows)
            {
                CampaignPollVM item = new CampaignPollVM
                {
                    CampaignPollId = Convert.ToInt32(row["CampaignPollId"]),
                    Title = Convert.ToString(row["Title"]),
                    Question = Convert.ToString(row["Question"]),
                    Description = Convert.ToString(row["Description"]),
                    TargetArea = Convert.ToString(row["TargetArea"]),
                    PollType = Convert.ToString(row["PollType"]),
                    PublicSlug = Convert.ToString(row["PublicSlug"]),
                    StartDate = row["StartDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["StartDate"]),
                    EndDate = row["EndDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["EndDate"]),
                    ShowPublicResults = row["ShowPublicResults"] != DBNull.Value && Convert.ToBoolean(row["ShowPublicResults"]),
                    RequireConsent = row["RequireConsent"] == DBNull.Value || Convert.ToBoolean(row["RequireConsent"]),
                    IsActive = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"]),
                    CreatedOn = row["CreatedOn"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(row["CreatedOn"]),
                    TotalResponses = table.Columns.Contains("TotalResponses") && row["TotalResponses"] != DBNull.Value ? Convert.ToInt32(row["TotalResponses"]) : 0
                };
                ApplyPollShareLinks(item, baseUrl);
                records.Add(item);
            }
            return records;
        }

        private List<CampaignPollOptionVM> MapCampaignPollOptions(DataTable table)
        {
            List<CampaignPollOptionVM> records = new List<CampaignPollOptionVM>();
            foreach (DataRow row in table.Rows)
            {
                records.Add(new CampaignPollOptionVM
                {
                    CampaignPollOptionId = Convert.ToInt32(row["CampaignPollOptionId"]),
                    CampaignPollId = Convert.ToInt32(row["CampaignPollId"]),
                    OptionText = Convert.ToString(row["OptionText"]),
                    DisplayOrder = row["DisplayOrder"] == DBNull.Value ? 0 : Convert.ToInt32(row["DisplayOrder"]),
                    ResponseCount = row["ResponseCount"] == DBNull.Value ? 0 : Convert.ToInt32(row["ResponseCount"])
                });
            }
            return records;
        }

        private List<CampaignPollResponseVM> MapCampaignPollResponses(DataTable table)
        {
            List<CampaignPollResponseVM> records = new List<CampaignPollResponseVM>();
            foreach (DataRow row in table.Rows)
            {
                records.Add(new CampaignPollResponseVM
                {
                    CampaignPollResponseId = Convert.ToInt32(row["CampaignPollResponseId"]),
                    CampaignPollId = Convert.ToInt32(row["CampaignPollId"]),
                    CampaignPollOptionId = Convert.ToInt32(row["CampaignPollOptionId"]),
                    OptionText = Convert.ToString(row["OptionText"]),
                    RespondentName = Convert.ToString(row["RespondentName"]),
                    MobileNo = Convert.ToString(row["MobileNo"]),
                    AreaName = Convert.ToString(row["AreaName"]),
                    Source = Convert.ToString(row["Source"]),
                    Remarks = Convert.ToString(row["Remarks"]),
                    ConsentGiven = row["ConsentGiven"] != DBNull.Value && Convert.ToBoolean(row["ConsentGiven"]),
                    SubmittedOn = row["SubmittedOn"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(row["SubmittedOn"])
                });
            }
            return records;
        }

        private void ApplyPollShareLinks(CampaignPollVM model, string baseUrl)
        {
            string root = string.IsNullOrWhiteSpace(baseUrl) ? string.Empty : baseUrl.TrimEnd('/');
            model.PublicUrl = root + "/CampaignPoll/Details?slug=" + Uri.EscapeDataString(model.PublicSlug);
            model.WhatsAppShareUrl = "https://wa.me/?text=" + Uri.EscapeDataString(model.Title + " - " + model.PublicUrl + "&source=whatsapp");
            model.FacebookShareUrl = "https://www.facebook.com/sharer/sharer.php?u=" + Uri.EscapeDataString(model.PublicUrl + "&source=facebook");
        }

        private string CreatePollSlug(string title)
        {
            string clean = new string((title ?? "poll").ToLowerInvariant().Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray()).Trim('-');
            while (clean.Contains("--")) clean = clean.Replace("--", "-");
            if (string.IsNullOrWhiteSpace(clean)) clean = "poll";
            string slug = clean.Length > 120 ? clean.Substring(0, 120).Trim('-') : clean;
            string candidate = slug;
            int suffix = 1;
            while (QuerySql("SELECT TOP 1 CampaignPollId FROM dbo.CampaignPoll WHERE PublicSlug = @PublicSlug", new SqlParameter("@PublicSlug", candidate)).Rows.Count > 0)
            {
                candidate = slug + "-" + suffix++;
            }
            return candidate;
        }

        public void AddCampaignAudit(string moduleName, string recordId, string actionName, int userId, string ipAddress, string remarks, bool isSensitive)
        {
            EnsureCampaignAuditLogTable();

            ExecuteSql(@"
INSERT INTO dbo.CampaignAuditLog
    (ModuleName, RecordId, ActionName, PerformedBy, PerformedByUserId, IpAddress, Remarks, IsSensitive)
VALUES
    (@ModuleName, @RecordId, @ActionName, @PerformedBy, @PerformedByUserId, @IpAddress, @Remarks, @IsSensitive)",
                new SqlParameter("@ModuleName", moduleName),
                new SqlParameter("@RecordId", (object)recordId ?? DBNull.Value),
                new SqlParameter("@ActionName", actionName),
                new SqlParameter("@PerformedBy", userId > 0 ? "User #" + userId : "System"),
                new SqlParameter("@PerformedByUserId", userId > 0 ? (object)userId : DBNull.Value),
                new SqlParameter("@IpAddress", (object)ipAddress ?? DBNull.Value),
                new SqlParameter("@Remarks", (object)remarks ?? DBNull.Value),
                new SqlParameter("@IsSensitive", isSensitive));
        }

        private void EnsureCampaignAuditLogTable()
        {
            ExecuteSql(@"IF OBJECT_ID('dbo.CampaignAuditLog', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CampaignAuditLog
    (
        CampaignAuditLogId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CampaignAuditLog PRIMARY KEY,
        ModuleName NVARCHAR(100) NOT NULL,
        RecordId NVARCHAR(80) NULL,
        ActionName NVARCHAR(80) NOT NULL,
        PerformedBy NVARCHAR(120) NULL,
        PerformedByUserId INT NULL,
        PerformedOn DATETIME NOT NULL CONSTRAINT DF_CampaignAuditLog_PerformedOn DEFAULT(GETDATE()),
        IpAddress NVARCHAR(80) NULL,
        Remarks NVARCHAR(500) NULL,
        IsSensitive BIT NOT NULL CONSTRAINT DF_CampaignAuditLog_Sensitive DEFAULT(0)
    );
END");
        }

        private static List<CampaignAuditLogVM> MapCampaignAuditLogs(DataTable table)
        {
            List<CampaignAuditLogVM> items = new List<CampaignAuditLogVM>();
            foreach (DataRow row in table.Rows)
            {
                items.Add(new CampaignAuditLogVM
                {
                    CampaignAuditLogId = Convert.ToInt32(row["CampaignAuditLogId"]),
                    ModuleName = Convert.ToString(row["ModuleName"]),
                    RecordId = Convert.ToString(row["RecordId"]),
                    ActionName = Convert.ToString(row["ActionName"]),
                    PerformedBy = Convert.ToString(row["PerformedBy"]),
                    PerformedByUserId = row["PerformedByUserId"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["PerformedByUserId"]),
                    PerformedOn = row["PerformedOn"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(row["PerformedOn"]),
                    IpAddress = Convert.ToString(row["IpAddress"]),
                    Remarks = Convert.ToString(row["Remarks"]),
                    IsSensitive = row["IsSensitive"] != DBNull.Value && Convert.ToBoolean(row["IsSensitive"])
                });
            }

            return items;
        }

        public CampaignErpModuleVM GetCampaignErpModule(string moduleKey)
        {
            string key = (moduleKey ?? string.Empty).Trim().ToLowerInvariant();


            if (key == "leader-kit")
            {
                return BuildModule("Leader Campaign Kit", "Centralize approved leader campaign assets for a disciplined party-style campaign without unsafe automation or misinformation.", "Foundation", "Campaign Manager",
                    Section("Leader Brand Profile", "Maintain public biography, achievements, approved photos and official contact/social references.", "Leader bio", "Public achievements", "Approved photo", "Official social links", "Constituency focus", "Language versions"),
                    Section("Speech and Slogan Bank", "Store approved speeches, slogans, talking points and event-wise message notes.", "Speech title", "Language", "Audience context", "Approval status", "Version", "Reviewed by"),
                    Section("Media Kit", "Keep press-ready content aligned with compliance and brand discipline.", "Press note", "Poster template", "Logo usage", "Video byte", "Hashtag list", "Do-not-use notes"));
            }

            if (key == "manifesto")
            {
                return BuildModule("Manifesto and Promise Tracker", "Track public promises, manifesto points, area-specific demands and delivery/progress notes.", "Foundation", "Policy Team",
                    Section("Promise Register", "Create a structured register of promises and commitments.", "Promise title", "Category", "Area", "Priority", "Source event", "Owner", "Status"),
                    Section("Evidence and Updates", "Attach progress notes and references that can be verified before public use.", "Document link", "Progress note", "Verification status", "Last updated", "Reviewed by"),
                    Section("Public Communication", "Prepare approved explanations for public meetings and digital content.", "Short message", "Long explanation", "Language", "Approval status", "Publish channel"));
            }

            if (key == "booth-committee")
            {
                return BuildModule("Booth Committee Network", "Coordinate booth-level committee responsibilities, meetings and lawful voter-contact operations.", "Foundation", "Booth Coordinator",
                    Section("Committee Structure", "Maintain booth president, area coordinators and team ownership.", "Booth", "Committee role", "Person name", "Mobile", "Assigned area", "Active status"),
                    Section("Meeting and Follow-up", "Track booth meetings, attendance and pending field work.", "Meeting date", "Agenda", "Attendance", "Pending task", "Next follow-up", "Responsible person"),
                    Section("Compliance Guardrails", "Keep field work lawful and consent-aware.", "Data source", "Consent note", "Do-not-contact flag", "Escalation note", "Audit status"));
            }

            if (key == "page-social")
            {
                return BuildModule("Page and Social Coordination", "Coordinate official pages, volunteer amplification and content approvals without spam or platform abuse.", "Foundation", "Digital Team Lead",
                    Section("Channel Register", "Track official and volunteer-managed public channels.", "Platform", "Page name", "Owner", "Access status", "Content language", "Approval owner"),
                    Section("Content Discipline", "Align posts with approved messages and review workflow.", "Theme", "Post type", "Creative asset", "Caption", "Approval status", "Publish URL"),
                    Section("Performance Notes", "Record public metrics from platform dashboards.", "Reach", "Engagement", "Shares", "Comments", "Learning note", "Next action"));
            }

            if (key == "rally-material")
            {
                return BuildModule("Rally Material Kit", "Plan lawful rally material, stage content, banners, pamphlets, volunteer kits and distribution control.", "Foundation", "Event Manager",
                    Section("Material Planning", "Prepare event-wise materials with quantities and owners.", "Material type", "Design version", "Quantity", "Vendor", "Delivery date", "Storage location"),
                    Section("Distribution", "Track issue/return and area-wise distribution.", "Issued to", "Area", "Quantity issued", "Balance", "Return required", "Remarks"),
                    Section("Brand and Compliance", "Keep material aligned with approved symbols and legal disclaimers.", "Approval status", "Disclaimer", "Print proof", "Reviewed by", "Audit note"));
            }

            if (key == "training")
            {
                return BuildModule("Campaign Training", "Train volunteers and coordinators on message discipline, event duties, data privacy and compliance.", "Foundation", "Training Coordinator",
                    Section("Training Sessions", "Plan training batches for booth teams, volunteers and social media coordinators.", "Session title", "Audience", "Trainer", "Date", "Venue", "Attendance"),
                    Section("Learning Material", "Store approved guides and checklists.", "Guide title", "Language", "Version", "Document link", "Approval status"),
                    Section("Readiness Check", "Track whether teams are ready for assigned work.", "Team", "Score", "Gap", "Follow-up task", "Completed status"));
            }
            if (key == "candidate")
            {
                return BuildModule("Candidate Management", "Maintain candidate public profile, legal documents, campaign media, election history and approval workflow.", "Planned", "Campaign Manager",
                    Section("Candidate Profile", "Single source of truth for candidate biography and public information.", "Full name", "Party/organization", "Constituency", "Education", "Profession", "Public bio", "Social links"),
                    Section("Legal and Affidavit", "Store references to legally publishable affidavit, assets and case declarations.", "Affidavit document", "Assets summary", "Liabilities summary", "Criminal case declaration", "Approval status"),
                    Section("Campaign Media", "Connect candidate images, videos, manifesto and press documents with CMS media.", "Photo gallery", "Video gallery", "Manifesto file", "Press kit", "Display order"));
            }

            if (key == "membership")
            {
                return BuildModule("Membership Drive", "Extend existing membership into campaign-wise drives, renewals, approval and digital card operations.", "Existing module extension", "Membership Admin",
                    Section("Drive Setup", "Create membership campaigns by area, booth, team and date range.", "Drive name", "Campaign", "District", "Block", "Booth", "Start date", "End date", "Target members"),
                    Section("Approval Queue", "Verify member applications and assign them to local teams.", "Applicant name", "Mobile", "Area", "Verification status", "Approved by", "Team allocation"),
                    Section("Analytics", "Track growth and renewal performance.", "New members", "Renewals", "Rejected applications", "Digital cards issued", "Team performance"));
            }

            if (key == "social")
            {
                return BuildModule("Social Media War Room", "Plan, approve and monitor campaign content across social platforms without unsafe automation.", "Planned", "Media Manager",
                    Section("Content Calendar", "Prepare posts, reels, videos and poster plans with approval status.", "Post date", "Platform", "Content theme", "Caption", "Media asset", "Approval status"),
                    Section("Publishing Queue", "Track what should be posted and who is responsible.", "Assigned volunteer", "Scheduled time", "Publish status", "Public URL", "Review remarks"),
                    Section("Performance Snapshot", "Record public performance metrics from platform dashboards.", "Reach", "Engagement", "Shares", "Comments", "Video views", "Sentiment note"));
            }

            if (key == "finance")
            {
                return BuildModule("Finance and Donations", "Track campaign funds, event expenses, approvals and audit-ready evidence.", "Foundation", "Finance Admin",
                    Section("Donation Register", "Record lawful donation details and receipts.", "Donor name", "Receipt no", "Amount", "Mode", "PAN/reference where required", "Receipt file"),
                    Section("Expense Approval", "Connect event expenses with approval and payment proof.", "Expense type", "Event", "Vendor", "Estimated amount", "Actual amount", "Approved by", "Payment proof"),
                    Section("Finance Dashboard", "Summarize collection, spending and pending approvals.", "Total collected", "Total spent", "Pending approvals", "Budget variance", "Audit status"));
            }

            return BuildModule("Compliance and Security", "Define controls for role permissions, audit trail, secure uploads and lawful data handling.", "Ongoing", "System Admin",
                Section("Role Matrix", "Make every campaign module permission-driven.", "Role", "Module", "Can view", "Can create", "Can edit", "Can delete", "Can approve"),
                Section("Audit Trail", "Track important campaign data changes and status changes.", "Module", "Record id", "Action", "Old value", "New value", "Changed by", "Changed on"),
                Section("Data Governance", "Protect voter, volunteer and citizen data.", "Consent/source", "Import batch", "Duplicate check", "Retention rule", "Export permission", "Access log"));
        }

        private static CampaignErpPillarVM CreatePillar(string name, string description, string status, string icon, string actionName, params string[] capabilities)
        {
            return new CampaignErpPillarVM
            {
                Name = name,
                Description = description,
                Status = status,
                Icon = icon,
                ActionName = actionName,
                Capabilities = capabilities.ToList()
            };
        }

        private static CampaignErpModuleVM BuildModule(string name, string purpose, string status, string ownerRole, params CampaignErpModuleSectionVM[] sections)
        {
            CampaignErpModuleVM model = new CampaignErpModuleVM
            {
                Name = name,
                Purpose = purpose,
                Status = status,
                OwnerRole = ownerRole,
                Sections = sections.ToList()
            };

            model.SecurityRules.Add("RoleMenuAuthorize must protect this module before create/edit/delete screens are enabled.");
            model.SecurityRules.Add("Important status changes should be logged with user id and timestamp.");
            model.SecurityRules.Add("Uploads must validate extension, size and storage path.");
            model.SecurityRules.Add("Sensitive personal data should be collected only from lawful sources and shown only to authorized roles.");

            model.IntegrationNotes.Add("Use Controller -> Service -> ViewModel pattern.");
            model.IntegrationNotes.Add("Do not edit EDMX-generated model files manually.");
            model.IntegrationNotes.Add("Add SQL upgrade script first, then refresh EDMX through Visual Studio when database tables are finalized.");
            model.IntegrationNotes.Add("Keep forms compact: three fields per row on desktop, one field per row on mobile.");

            return model;
        }

        private static CampaignErpModuleSectionVM Section(string title, string description, params string[] fields)
        {
            return new CampaignErpModuleSectionVM
            {
                Title = title,
                Description = description,
                Fields = fields.ToList()
            };
        }
        #endregion
    }
}





