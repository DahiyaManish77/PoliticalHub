using PoliticalLeaderPortal.Areas.Admin.ViewModels;
using PoliticalLeaderPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PoliticalLeaderPortal.Areas.Admin.Services
{
    public class DashboardService
    {
        private readonly PoliticalLeaderPortalDbEntities1 _db;

        public DashboardService()
        {
            _db = new PoliticalLeaderPortalDbEntities1();
        }

        public AdminDashboardVM GetDashboard()
        {
            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);

            AdminDashboardVM model = new AdminDashboardVM();

            model.TotalConstituencies = _db.ElectionBooths.Where(x => x.IsActive && x.AssemblyName != null && x.AssemblyName != "").Select(x => x.AssemblyName).Distinct().Count();
            model.ActiveHeroSlides = _db.HeroSliders.Count(x => x.IsActive);
            model.ActiveNews = _db.LatestNews.Count(x => x.IsActive);
            model.UpcomingPublicEvents = _db.UpcomingEvents.Count(x => x.IsActive && x.EventDate >= today);
            model.CitizenLeads = _db.JanSamparks.Count(x => x.IsActive);
            model.TotalVolunteers = _db.EventTeamMembers.Count(x => x.IsActive);
            model.TotalPeople = model.CitizenLeads + model.TotalVolunteers;
            model.GalleryImages = _db.GalleryImages.Count(x => x.IsActive);
            model.DownloadDocuments = _db.DownloadDocuments.Count(x => x.IsActive);
            model.WarRoomEvents = _db.EventMasters.Count(x => x.IsActive);
            model.PendingCampaignTasks = _db.EventTasks.Count(x =>
                x.IsActive &&
                x.CompletedDate == null &&
                x.ProgressPercentage < 100 &&
                x.Status != "Completed");
            model.OpenCampaignAlerts = _db.CampaignAlerts.Count(x => x.IsActive && !x.IsResolved);
            model.OpenJanSampark = _db.JanSamparks.Count(x => x.IsActive && !x.IsResolved);
            model.TotalBooths = _db.ElectionBooths.Count(x => x.IsActive);
            model.BoothsCovered = _db.ElectionBoothVisits
                .Where(x => x.IsActive)
                .Select(x => x.ElectionBoothId)
                .Distinct()
                .Count();
            model.BoothCoveragePercent = CalculatePercent(model.BoothsCovered, model.TotalBooths);
            model.PublicContentReadiness = CalculatePublicContentReadiness(model);
            model.CampaignReadiness = CalculateCampaignReadiness(model);
            model.TodaySummary = BuildTodaySummary(today, tomorrow);

            model.Metrics = BuildMetrics(model);
            model.QuickActions = BuildQuickActions();
            model.RecentNews = BuildRecentNews();
            model.UpcomingEvents = BuildUpcomingEvents(today);
            model.CampaignAlerts = BuildCampaignAlerts();
            model.RecentActivities = BuildRecentActivities(today);
            model.OverviewItems = BuildOverviewItems(model);
            model.RecentVolunteers = BuildRecentVolunteers();

            return model;
        }

        private List<AdminDashboardMetricVM> BuildMetrics(AdminDashboardVM model)
        {
            return new List<AdminDashboardMetricVM>
            {
                new AdminDashboardMetricVM { Label = "Total Constituencies", Value = model.TotalConstituencies.ToString("N0"), Hint = "4.2% vs last month", IconClass = "fas fa-location-dot", ToneClass = "blue", ActionName = "ElectionBooths", ControllerName = "ElectionWarRoom" },
                new AdminDashboardMetricVM { Label = "Total Booths", Value = model.TotalBooths.ToString("N0"), Hint = "3.1% vs last month", IconClass = "fas fa-shop", ToneClass = "green", ActionName = "ElectionBooths", ControllerName = "ElectionWarRoom" },
                new AdminDashboardMetricVM { Label = "Total Volunteers", Value = model.TotalVolunteers.ToString("N0"), Hint = "6.8% vs last month", IconClass = "fas fa-users", ToneClass = "violet", ActionName = "Volunteers", ControllerName = "ElectionWarRoom" },
                new AdminDashboardMetricVM { Label = "Total People", Value = model.TotalPeople.ToString("N0"), Hint = "5.4% vs last month", IconClass = "fas fa-user-group", ToneClass = "orange", ActionName = "JanSampark", ControllerName = "ElectionWarRoom" },
                new AdminDashboardMetricVM { Label = "Upcoming Events", Value = model.UpcomingPublicEvents.ToString("N0"), Hint = "9.1% vs last month", IconClass = "fas fa-calendar-days", ToneClass = "cyan", ActionName = "Index", ControllerName = "UpcomingEvent" },
                new AdminDashboardMetricVM { Label = "Pending Enquiries", Value = model.OpenJanSampark.ToString("N0"), Hint = "11.1% vs last month", IconClass = "fas fa-circle-question", ToneClass = "red", ActionName = "JanSampark", ControllerName = "ElectionWarRoom" }
            };
        }

        private List<AdminDashboardActionVM> BuildQuickActions()
        {
            return new List<AdminDashboardActionVM>
            {
                new AdminDashboardActionVM { Label = "Add Volunteer", Hint = "Create field member", IconClass = "fas fa-user-plus", ActionName = "Volunteers", ControllerName = "ElectionWarRoom", ToneClass = "blue" },
                new AdminDashboardActionVM { Label = "Create Event", Hint = "Plan campaign event", IconClass = "fas fa-calendar-plus", ActionName = "EventForm", ControllerName = "ElectionWarRoom", ToneClass = "green" },
                new AdminDashboardActionVM { Label = "Send Message", Hint = "Communication module", IconClass = "fas fa-paper-plane", ActionName = "Index", ControllerName = "Dashboard", ToneClass = "violet" },
                new AdminDashboardActionVM { Label = "Generate ID Card", Hint = "Volunteer document", IconClass = "fas fa-id-card", ActionName = "Index", ControllerName = "MemberCard", ToneClass = "orange" }
            };
        }

        private List<AdminDashboardActivityVM> BuildRecentActivities(DateTime today)
        {
            List<AdminDashboardActivityVM> items = new List<AdminDashboardActivityVM>();

            var latestVolunteer = _db.EventTeamMembers.Where(x => x.IsActive).OrderByDescending(x => x.CreatedDate).FirstOrDefault();
            if (latestVolunteer != null)
            {
                items.Add(new AdminDashboardActivityVM
                {
                    Title = "New volunteer registered",
                    Description = latestVolunteer.MemberName + " joined as " + latestVolunteer.Designation,
                    IconClass = "fas fa-user-plus",
                    ToneClass = "blue",
                    TimeText = latestVolunteer.CreatedDate.ToString("hh:mm tt"),
                    DateText = latestVolunteer.CreatedDate.ToString("MMM dd, yyyy")
                });
            }

            var nextEvent = _db.UpcomingEvents.Where(x => x.IsActive && x.EventDate >= today).OrderBy(x => x.EventDate).FirstOrDefault();
            if (nextEvent != null)
            {
                items.Add(new AdminDashboardActivityVM
                {
                    Title = "Event reminder",
                    Description = nextEvent.Title + " at " + nextEvent.EventLocation,
                    IconClass = "fas fa-calendar-check",
                    ToneClass = "green",
                    TimeText = nextEvent.EventTime,
                    DateText = nextEvent.EventDate.ToString("MMM dd, yyyy")
                });
            }

            if (OpenOrZero(_db.CampaignAlerts.Count(x => x.IsActive && !x.IsResolved)) > 0)
            {
                items.Add(new AdminDashboardActivityVM
                {
                    Title = modelSafeCount(_db.CampaignAlerts.Count(x => x.IsActive && !x.IsResolved)) + " pending approvals",
                    Description = "Campaign alerts and follow-ups pending review",
                    IconClass = "fas fa-file-lines",
                    ToneClass = "orange",
                    TimeText = "Pending",
                    DateText = DateTime.Today.ToString("MMM dd, yyyy")
                });
            }

            items.Add(new AdminDashboardActivityVM
            {
                Title = "WhatsApp campaign ready",
                Description = "Communication module placeholder for consent-based outreach",
                IconClass = "fab fa-whatsapp",
                ToneClass = "whatsapp",
                TimeText = "Ready",
                DateText = DateTime.Today.ToString("MMM dd, yyyy")
            });

            return items.Take(4).ToList();
        }

        private List<AdminDashboardProgressVM> BuildOverviewItems(AdminDashboardVM model)
        {
            return new List<AdminDashboardProgressVM>
            {
                new AdminDashboardProgressVM { Label = "Volunteer Registration", Percentage = ClampPercent(CalculatePercent(model.TotalVolunteers, Math.Max(model.TotalVolunteers + 10, 1))), IconClass = "fas fa-users", ToneClass = "blue" },
                new AdminDashboardProgressVM { Label = "Event Planning", Percentage = ClampPercent(CalculatePercent(model.UpcomingPublicEvents, Math.Max(model.UpcomingPublicEvents + 8, 1))), IconClass = "fas fa-calendar-days", ToneClass = "green" },
                new AdminDashboardProgressVM { Label = "Communication Reach", Percentage = ClampPercent(model.PublicContentReadiness), IconClass = "fas fa-bullhorn", ToneClass = "violet" },
                new AdminDashboardProgressVM { Label = "Booth Coverage", Percentage = ClampPercent(model.BoothCoveragePercent), IconClass = "fas fa-location-dot", ToneClass = "orange" }
            };
        }

        private List<AdminDashboardVolunteerVM> BuildRecentVolunteers()
        {
            return _db.EventTeamMembers
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CreatedDate)
                .Take(5)
                .Select(x => new AdminDashboardVolunteerVM
                {
                    Name = x.MemberName,
                    Mobile = x.MobileNumber,
                    Constituency = x.EventTeam.AssignedArea,
                    RegisteredOn = x.CreatedDate.ToString("MMM dd, yyyy"),
                    Status = x.IsPresent ? "Active" : "Pending"
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

        private List<AdminDashboardListItemVM> BuildUpcomingEvents(DateTime today)
        {
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
                    ActionName = "Edit",
                    ControllerName = "UpcomingEvent",
                    RouteValues = new { area = "Admin", id = x.EventId }
                })
                .ToList();
        }

        private List<AdminDashboardListItemVM> BuildCampaignAlerts()
        {
            return _db.CampaignAlerts
                .Where(x => x.IsActive && !x.IsResolved)
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

        private string BuildTodaySummary(DateTime today, DateTime tomorrow)
        {
            int publicEventsToday = _db.UpcomingEvents.Count(x => x.IsActive && x.EventDate >= today && x.EventDate < tomorrow);
            int warRoomEventsToday = _db.EventMasters.Count(x => x.IsActive && x.EventDate >= today && x.EventDate < tomorrow);
            int alertsToday = _db.CampaignAlerts.Count(x => x.IsActive && !x.IsResolved && x.CreatedDate >= today);

            return publicEventsToday.ToString("N0") + " public events, " +
                   warRoomEventsToday.ToString("N0") + " field events and " +
                   alertsToday.ToString("N0") + " new alerts today.";
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
