using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using PoliticalLeaderPortal.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Data.SqlClient;

namespace PoliticalLeaderPortal.Areas.Admin.Services
{
    public class DashboardService
    {
        private readonly PoliticalLeaderPortalDbEntities1 _db;

        public DashboardService()
        {
            _db = new PoliticalLeaderPortalDbEntities1();
        }

        public AdminDashboardVM GetDashboard(int? campaignId)
        {
            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);
            List<int> campaignEventIds = GetCampaignEventIds(campaignId);

            AdminDashboardVM model = new AdminDashboardVM();
            model.IsCampaignFiltered = campaignId.HasValue;

            model.TotalConstituencies = _db.ElectionBooths.Where(x => x.IsActive && x.AssemblyName != null && x.AssemblyName != "").Select(x => x.AssemblyName).Distinct().Count();
            model.ActiveHeroSlides = _db.HeroSliders.Count(x => x.IsActive);
            model.ActiveNews = _db.LatestNews.Count(x => x.IsActive);
            model.UpcomingPublicEvents = _db.UpcomingEvents.Count(x => x.IsActive && x.EventDate >= today);
            var issueQuery = _db.JanSamparks.Where(x => x.IsActive);
            var eventQuery = _db.EventMasters.Where(x => x.IsActive);
            var taskQuery = _db.EventTasks.Where(x => x.IsActive);
            if (campaignEventIds != null)
            {
                issueQuery = issueQuery.Where(x => x.EventId.HasValue && campaignEventIds.Contains(x.EventId.Value));
                eventQuery = eventQuery.Where(x => campaignEventIds.Contains(x.EventId));
                taskQuery = taskQuery.Where(x => campaignEventIds.Contains(x.EventId));
            }
            model.CitizenLeads = issueQuery.Count();
            model.TotalVolunteers = _db.VolunteerProfiles.Count(x =>
                x.IsActive && !x.IsDeleted && x.PersonMaster.IsActive && !x.PersonMaster.IsDeleted);
            model.TotalPeople = _db.PersonMasters.Count(x => x.IsActive && !x.IsDeleted);
            model.GalleryImages = _db.GalleryImages.Count(x => x.IsActive);
            model.DownloadDocuments = _db.DownloadDocuments.Count(x => x.IsActive);
            model.WarRoomEvents = eventQuery.Count();
            model.PendingCampaignTasks = taskQuery.Count(x =>
                x.CompletedDate == null &&
                x.ProgressPercentage < 100 &&
                x.Status != "Completed");
            model.TotalCampaignTasks = taskQuery.Count();
            model.CompletedCampaignTasks = taskQuery.Count(x =>
                x.CompletedDate != null || x.ProgressPercentage >= 100 || x.Status == "Completed");
            model.OverdueTasks = taskQuery.Count(x =>
                x.DueDate < today &&
                x.CompletedDate == null &&
                x.ProgressPercentage < 100 &&
                x.Status != "Completed");
            var alertQuery = _db.CampaignAlerts.Where(x => x.IsActive && !x.IsResolved);
            if (campaignId.HasValue)
            {
                alertQuery = alertQuery.Where(x => x.CampaignId == campaignId.Value);
            }
            model.OpenCampaignAlerts = alertQuery.Count();
            model.CriticalAlerts = _db.CampaignAlerts.Count(x =>
                x.IsActive && !x.IsResolved &&
                (!campaignId.HasValue || x.CampaignId == campaignId.Value) &&
                (x.Severity == "Critical" || x.Severity == "High"));
            model.OpenJanSampark = issueQuery.Count(x => !x.IsResolved);
            model.ResolvedJanSampark = issueQuery.Count(x => x.IsResolved);
            model.NewCitizenIssuesToday = issueQuery.Count(x => x.CreatedDate >= today);
            model.TodayFieldEvents = eventQuery.Count(x =>
                !x.IsCancelled && x.EventDate >= today && x.EventDate < tomorrow);
            model.TotalBooths = _db.ElectionBooths.Count(x => x.IsActive);
            model.BoothsCovered = _db.ElectionBoothVisits
                .Where(x => x.IsActive)
                .Select(x => x.ElectionBoothId)
                .Distinct()
                .Count();
            model.PlannedEventBudget = eventQuery
                .Where(x => !x.IsCancelled)
                .Select(x => (decimal?)x.Budget)
                .Sum() ?? 0;
            model.ActualEventExpense = eventQuery
                .Where(x => !x.IsCancelled)
                .Select(x => (decimal?)x.ActualExpense)
                .Sum() ?? 0;
            model.BoothCoveragePercent = CalculatePercent(model.BoothsCovered, model.TotalBooths);
            model.TaskCompletionPercent = CalculatePercent(model.CompletedCampaignTasks, model.TotalCampaignTasks);
            model.IssueResolutionPercent = CalculatePercent(
                model.ResolvedJanSampark,
                model.ResolvedJanSampark + model.OpenJanSampark);
            model.BudgetUtilizationPercent = CalculatePercent(
                Convert.ToInt32(Math.Round(model.ActualEventExpense, 0)),
                Convert.ToInt32(Math.Round(model.PlannedEventBudget, 0)));
            model.PublicContentReadiness = CalculatePublicContentReadiness(model);
            model.CampaignReadiness = CalculateCampaignReadiness(model);
            model.ActiveVerifiedDocuments = GetVerifiedDocumentCount(false);
            model.ExpiringVerifiedDocuments = GetVerifiedDocumentCount(true);
            model.TodaySummary = BuildTodaySummary(today, tomorrow, campaignId, campaignEventIds);

            model.Metrics = BuildMetrics(model);
            model.QuickActions = BuildQuickActions();
            model.RecentNews = BuildRecentNews();
            model.UpcomingEvents = BuildUpcomingEvents(today, campaignEventIds);
            model.CampaignAlerts = BuildCampaignAlerts(campaignId);
            model.RecentActivities = BuildRecentActivities(today, campaignId, campaignEventIds);
            model.OverviewItems = BuildOverviewItems(model);
            model.RecentVolunteers = BuildRecentVolunteers();

            return model;
        }

        public CampaignContextVM GetCampaignContext(int? selectedCampaignId, string returnUrl)
        {
            var campaigns = _db.ElectionCampaigns
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.Status == "Active")
                .ThenByDescending(x => x.StartDate)
                .ThenBy(x => x.CampaignName)
                .Select(x => new
                {
                    x.CampaignId,
                    x.CampaignName,
                    x.ElectionType,
                    x.Status
                })
                .ToList();

            if (selectedCampaignId.HasValue &&
                !campaigns.Any(x => x.CampaignId == selectedCampaignId.Value))
            {
                selectedCampaignId = null;
            }

            return new CampaignContextVM
            {
                CampaignId = selectedCampaignId,
                CampaignName = campaigns
                    .Where(x => selectedCampaignId.HasValue && x.CampaignId == selectedCampaignId.Value)
                    .Select(x => x.CampaignName)
                    .FirstOrDefault(),
                ReturnUrl = returnUrl,
                Campaigns = campaigns.Select(x => new SelectListItem
                {
                    Value = x.CampaignId.ToString(),
                    Text = x.CampaignName + " · " + x.ElectionType + " · " + x.Status,
                    Selected = selectedCampaignId.HasValue && x.CampaignId == selectedCampaignId.Value
                }).ToList()
            };
        }

        public int? GetDefaultCampaignId()
        {
            return _db.ElectionCampaigns
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.Status == "Active")
                .ThenByDescending(x => x.StartDate)
                .Select(x => (int?)x.CampaignId)
                .FirstOrDefault();
        }

        public string GetCampaignName(int campaignId)
        {
            return _db.ElectionCampaigns
                .AsNoTracking()
                .Where(x => x.IsActive && x.CampaignId == campaignId)
                .Select(x => x.CampaignName)
                .FirstOrDefault();
        }

        private List<AdminDashboardMetricVM> BuildMetrics(AdminDashboardVM model)
        {
            return new List<AdminDashboardMetricVM>
            {
                new AdminDashboardMetricVM { Label = model.IsCampaignFiltered ? "Booth Coverage (All)" : "Booth Coverage", Value = model.BoothCoveragePercent.ToString("N0") + "%", Hint = model.BoothsCovered.ToString("N0") + " of " + model.TotalBooths.ToString("N0") + " constituency booths visited", IconClass = "fas fa-location-dot", ToneClass = "blue", ActionName = "ElectionBooths", ControllerName = "ElectionWarRoom" },
                new AdminDashboardMetricVM { Label = "Tasks Pending", Value = model.PendingCampaignTasks.ToString("N0"), Hint = model.OverdueTasks.ToString("N0") + " overdue tasks", IconClass = "fas fa-list-check", ToneClass = "orange", ActionName = "Tasks", ControllerName = "ElectionWarRoom" },
                new AdminDashboardMetricVM { Label = "Open Citizen Issues", Value = model.OpenJanSampark.ToString("N0"), Hint = model.NewCitizenIssuesToday.ToString("N0") + " received today", IconClass = "fas fa-comments", ToneClass = "violet", ActionName = "JanSampark", ControllerName = "ElectionWarRoom" },
                new AdminDashboardMetricVM { Label = "Campaign Alerts", Value = model.OpenCampaignAlerts.ToString("N0"), Hint = model.CriticalAlerts.ToString("N0") + " high or critical", IconClass = "fas fa-triangle-exclamation", ToneClass = "red", ActionName = "CampaignAlerts", ControllerName = "ElectionWarRoom" },
                new AdminDashboardMetricVM { Label = "Today's Field Events", Value = model.TodayFieldEvents.ToString("N0"), Hint = model.UpcomingPublicEvents.ToString("N0") + " public events across the website", IconClass = "fas fa-calendar-day", ToneClass = "cyan", ActionName = "Events", ControllerName = "ElectionWarRoom" },
                new AdminDashboardMetricVM { Label = model.IsCampaignFiltered ? "Active Volunteers (All)" : "Active Volunteers", Value = model.TotalVolunteers.ToString("N0"), Hint = "Central volunteer registry", IconClass = "fas fa-people-group", ToneClass = "green", ActionName = "Index", ControllerName = "People" },
                new AdminDashboardMetricVM { Label = model.IsCampaignFiltered ? "Verified Documents (All)" : "Verified Documents", Value = model.ActiveVerifiedDocuments.ToString("N0"), Hint = model.ExpiringVerifiedDocuments.ToString("N0") + " expire within 30 days", IconClass = "fas fa-id-card", ToneClass = "blue", ActionName = "Index", ControllerName = "VerifiedDocument" }
            };
        }

        private List<AdminDashboardActionVM> BuildQuickActions()
        {
            return new List<AdminDashboardActionVM>
            {
                new AdminDashboardActionVM { Label = "Add Volunteer", Hint = "Create field member", IconClass = "fas fa-user-plus", ActionName = "Create", ControllerName = "People", ToneClass = "blue" },
                new AdminDashboardActionVM { Label = "Create Event", Hint = "Plan field activity", IconClass = "fas fa-calendar-plus", ActionName = "CreateEvent", ControllerName = "ElectionWarRoom", ToneClass = "green" },
                new AdminDashboardActionVM { Label = "Create Task", Hint = "Assign responsibility", IconClass = "fas fa-list-check", ActionName = "CreateTask", ControllerName = "ElectionWarRoom", ToneClass = "violet" },
                new AdminDashboardActionVM { Label = "Record Citizen Issue", Hint = "Add Jan Sampark", IconClass = "fas fa-comment-medical", ActionName = "CreateJanSampark", ControllerName = "ElectionWarRoom", ToneClass = "orange" }
            };
        }

        private List<AdminDashboardActivityVM> BuildRecentActivities(DateTime today, int? campaignId, List<int> campaignEventIds)
        {
            List<AdminDashboardActivityVM> items = new List<AdminDashboardActivityVM>();

            var latestVolunteer = _db.VolunteerProfiles
                .Where(x => x.IsActive && !x.IsDeleted && x.PersonMaster.IsActive && !x.PersonMaster.IsDeleted)
                .OrderByDescending(x => x.CreatedDate).FirstOrDefault();
            if (latestVolunteer != null)
            {
                items.Add(new AdminDashboardActivityVM
                {
                    Title = "New volunteer registered",
                    Description = latestVolunteer.PersonMaster.FullName + " joined as " + latestVolunteer.PreferredRole,
                    IconClass = "fas fa-user-plus",
                    ToneClass = "blue",
                    TimeText = latestVolunteer.CreatedDate.ToString("hh:mm tt"),
                    DateText = latestVolunteer.CreatedDate.ToString("MMM dd, yyyy")
                });
            }

            var nextEventQuery = _db.EventMasters.Where(x => x.IsActive && !x.IsCancelled && x.EventDate >= today);
            if (campaignEventIds != null)
                nextEventQuery = nextEventQuery.Where(x => campaignEventIds.Contains(x.EventId));
            var nextEvent = nextEventQuery.OrderBy(x => x.EventDate).FirstOrDefault();
            if (nextEvent != null)
            {
                items.Add(new AdminDashboardActivityVM
                {
                    Title = "Event reminder",
                    Description = nextEvent.EventTitle + " at " + nextEvent.Venue,
                    IconClass = "fas fa-calendar-check",
                    ToneClass = "green",
                    TimeText = nextEvent.StartTime.HasValue ? nextEvent.StartTime.Value.ToString(@"hh\:mm") : "-",
                    DateText = nextEvent.EventDate.ToString("MMM dd, yyyy")
                });
            }

            int pendingAlertCount = _db.CampaignAlerts.Count(x =>
                x.IsActive && !x.IsResolved &&
                (!campaignId.HasValue || x.CampaignId == campaignId.Value));
            if (OpenOrZero(pendingAlertCount) > 0)
            {
                items.Add(new AdminDashboardActivityVM
                {
                    Title = modelSafeCount(pendingAlertCount) + " pending alerts",
                    Description = "Campaign alerts and follow-ups pending review",
                    IconClass = "fas fa-file-lines",
                    ToneClass = "orange",
                    TimeText = "Pending",
                    DateText = DateTime.Today.ToString("MMM dd, yyyy")
                });
            }

            var latestIssueQuery = _db.JanSamparks.Where(x => x.IsActive);
            if (campaignEventIds != null)
                latestIssueQuery = latestIssueQuery.Where(x => x.EventId.HasValue && campaignEventIds.Contains(x.EventId.Value));
            var latestIssue = latestIssueQuery
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefault();
            if (latestIssue != null)
            {
                items.Add(new AdminDashboardActivityVM
                {
                    Title = "Citizen issue recorded",
                    Description = latestIssue.Subject + " - " + latestIssue.Village,
                    IconClass = "fas fa-comments",
                    ToneClass = "violet",
                    TimeText = latestIssue.CreatedDate.ToString("hh:mm tt"),
                    DateText = latestIssue.CreatedDate.ToString("MMM dd, yyyy")
                });
            }

            return items.Take(4).ToList();
        }

        private List<AdminDashboardProgressVM> BuildOverviewItems(AdminDashboardVM model)
        {
            return new List<AdminDashboardProgressVM>
            {
                new AdminDashboardProgressVM { Label = "Booth Coverage", Percentage = ClampPercent(model.BoothCoveragePercent), IconClass = "fas fa-location-dot", ToneClass = "blue" },
                new AdminDashboardProgressVM { Label = "Task Completion", Percentage = ClampPercent(model.TaskCompletionPercent), IconClass = "fas fa-list-check", ToneClass = "green" },
                new AdminDashboardProgressVM { Label = "Issue Resolution", Percentage = ClampPercent(model.IssueResolutionPercent), IconClass = "fas fa-comments", ToneClass = "violet" },
                new AdminDashboardProgressVM { Label = "Budget Utilization", Percentage = ClampPercent(model.BudgetUtilizationPercent), IconClass = "fas fa-indian-rupee-sign", ToneClass = "orange" },
                new AdminDashboardProgressVM { Label = "Public Content Readiness", Percentage = ClampPercent(model.PublicContentReadiness), IconClass = "fas fa-globe", ToneClass = "blue" },
                new AdminDashboardProgressVM { Label = "Campaign Readiness", Percentage = ClampPercent(model.CampaignReadiness), IconClass = "fas fa-gauge-high", ToneClass = "green" }
            };
        }

        private List<AdminDashboardVolunteerVM> BuildRecentVolunteers()
        {
            return _db.VolunteerProfiles
                .Where(x => x.IsActive && !x.IsDeleted && x.PersonMaster.IsActive && !x.PersonMaster.IsDeleted)
                .OrderByDescending(x => x.CreatedDate)
                .Take(5)
                .ToList()
                .Select(x => new AdminDashboardVolunteerVM
                {
                    Name = x.PersonMaster.FullName,
                    Mobile = x.PersonMaster.MobileNumber,
                    Constituency = x.PersonMaster.AssemblyConstituencyId.HasValue ? "Assembly #" + x.PersonMaster.AssemblyConstituencyId.Value : "",
                    RegisteredOn = x.CreatedDate.ToString("MMM dd, yyyy"),
                    Status = String.IsNullOrWhiteSpace(x.Status) ? "Pending" : x.Status
                })
                .ToList();
        }

        private List<AdminDashboardListItemVM> BuildRecentNews()
        {
            return _db.LatestNews
                .OrderByDescending(x => x.PublishDate)
                .Take(5)
                .Select(x => new AdminDashboardListItemVM
                {
                    Title = x.Title,
                    Description = x.ShortDescription,
                    BadgeText = x.IsActive ? "Live" : "Draft",
                    BadgeClass = x.IsActive ? "success" : "secondary",
                    Date = x.PublishDate,
                    ActionName = "Edit",
                    ControllerName = "LatestNews",
                    RouteValues = new { area = "Admin", id = x.NewsId }
                })
                .ToList();
        }

        private List<AdminDashboardListItemVM> BuildUpcomingEvents(DateTime today, List<int> campaignEventIds)
        {
            if (campaignEventIds != null)
            {
                return _db.EventMasters
                    .Where(x => x.IsActive && !x.IsCancelled && x.EventDate >= today &&
                        campaignEventIds.Contains(x.EventId))
                    .OrderBy(x => x.EventDate)
                    .Take(5)
                    .ToList()
                    .Select(x => new AdminDashboardListItemVM
                    {
                        Title = x.EventTitle,
                        Description = x.Venue,
                        BadgeText = x.StartTime.HasValue ? x.StartTime.Value.ToString(@"hh\:mm") : "-",
                        BadgeClass = "primary",
                        Date = x.EventDate,
                        TypeText = x.EventType,
                        StatusText = x.Status,
                        ActionName = "EditEvent",
                        ControllerName = "ElectionWarRoom",
                        RouteValues = new { area = "Admin", id = x.EventId }
                    })
                    .ToList();
            }

            return _db.UpcomingEvents
                .Where(x => x.EventDate >= today)
                .OrderBy(x => x.EventDate)
                .ThenBy(x => x.DisplayOrder)
                .Take(5)
                .Select(x => new AdminDashboardListItemVM
                {
                    Title = x.Title,
                    Description = x.EventLocation,
                    BadgeText = x.EventTime,
                    BadgeClass = x.IsActive ? "primary" : "secondary",
                    Date = x.EventDate,
                    TypeText = "Public event",
                    StatusText = x.IsActive ? "Published" : "Draft",
                    ActionName = "Edit",
                    ControllerName = "UpcomingEvent",
                    RouteValues = new { area = "Admin", id = x.EventId }
                })
                .ToList();
        }

        private List<AdminDashboardListItemVM> BuildCampaignAlerts(int? campaignId)
        {
            return _db.CampaignAlerts
                .Where(x => x.IsActive && !x.IsResolved &&
                    (!campaignId.HasValue || x.CampaignId == campaignId.Value))
                .OrderByDescending(x => x.Severity == "Critical")
                .ThenByDescending(x => x.Severity == "High")
                .ThenByDescending(x => x.CreatedDate)
                .Take(5)
                .Select(x => new AdminDashboardListItemVM
                {
                    Title = x.AlertTitle,
                    Description = x.AssignedToName,
                    BadgeText = x.Severity,
                    BadgeClass = x.Severity == "Critical" ? "danger" : x.Severity == "High" ? "warning" : "secondary",
                    Date = x.CreatedDate,
                    ActionName = "CampaignAlerts",
                    ControllerName = "ElectionWarRoom",
                    RouteValues = new { area = "Admin" }
                })
                .ToList();
        }

        private string BuildTodaySummary(DateTime today, DateTime tomorrow, int? campaignId, List<int> campaignEventIds)
        {
            int publicEventsToday = _db.UpcomingEvents.Count(x => x.IsActive && x.EventDate >= today && x.EventDate < tomorrow);
            var warRoomQuery = _db.EventMasters.Where(x =>
                x.IsActive && x.EventDate >= today && x.EventDate < tomorrow);
            var overdueQuery = _db.EventTasks.Where(x =>
                x.IsActive && x.DueDate < today && x.CompletedDate == null &&
                x.ProgressPercentage < 100 && x.Status != "Completed");
            if (campaignEventIds != null)
            {
                warRoomQuery = warRoomQuery.Where(x => campaignEventIds.Contains(x.EventId));
                overdueQuery = overdueQuery.Where(x => campaignEventIds.Contains(x.EventId));
            }
            int warRoomEventsToday = warRoomQuery.Count();
            int alertsToday = _db.CampaignAlerts.Count(x =>
                x.IsActive && !x.IsResolved && x.CreatedDate >= today &&
                (!campaignId.HasValue || x.CampaignId == campaignId.Value));

            int overdueTasks = overdueQuery.Count();

            return publicEventsToday.ToString("N0") + " public events, " +
                   warRoomEventsToday.ToString("N0") + " field events, " +
                   alertsToday.ToString("N0") + " new alerts and " +
                   overdueTasks.ToString("N0") + " overdue tasks.";
        }

        private List<int> GetCampaignEventIds(int? campaignId)
        {
            if (!campaignId.HasValue)
                return null;

            return _db.Database.SqlQuery<int>(
                "SELECT EventId FROM dbo.EventCampaignContext WHERE OperationalCampaignId=@CampaignId",
                new SqlParameter("@CampaignId", campaignId.Value))
                .ToList();
        }

        private int GetVerifiedDocumentCount(bool expiringSoon)
        {
            string dateFilter = expiringSoon
                ? " AND ExpiryDate >= CAST(GETDATE() AS date) AND ExpiryDate < DATEADD(day,31,CAST(GETDATE() AS date))"
                : " AND (ExpiryDate IS NULL OR ExpiryDate >= CAST(GETDATE() AS date))";

            return _db.Database.SqlQuery<int>(
                "SELECT COUNT(*) FROM dbo.VerifiedDocument WHERE Status=N'Active'" + dateFilter)
                .Single();
        }

        private decimal CalculatePublicContentReadiness(AdminDashboardVM model)
        {
            int completed = 0;
            if (model.ActiveHeroSlides > 0) completed++;
            if (model.ActiveNews > 0) completed++;
            if (model.GalleryImages > 0) completed++;
            if (model.DownloadDocuments > 0) completed++;

            return CalculatePercent(completed, 4);
        }

        private decimal CalculateCampaignReadiness(AdminDashboardVM model)
        {
            int completed = 0;
            if (model.WarRoomEvents > 0) completed++;
            if (model.TotalBooths > 0) completed++;
            if (model.BoothsCovered > 0) completed++;
            if (model.OpenCampaignAlerts == 0) completed++;

            return CalculatePercent(completed, 4);
        }

        private decimal CalculatePercent(int value, int total)
        {
            if (total <= 0)
            {
                return 0;
            }

            return Math.Round((decimal)value * 100 / total, 1);
        }

        private int ClampPercent(decimal value)
        {
            if (value < 0) return 0;
            if (value > 100) return 100;
            return Convert.ToInt32(Math.Round(value, 0));
        }

        private int OpenOrZero(int value)
        {
            return value < 0 ? 0 : value;
        }

        private string modelSafeCount(int value)
        {
            return value.ToString("N0");
        }
    }
}
