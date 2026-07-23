using System;
using System.Collections.Generic;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels
{
    public class AdminDashboardVM
    {
        public AdminDashboardVM()
        {
            Metrics = new List<AdminDashboardMetricVM>();
            QuickActions = new List<AdminDashboardActionVM>();
            RecentNews = new List<AdminDashboardListItemVM>();
            UpcomingEvents = new List<AdminDashboardListItemVM>();
            CampaignAlerts = new List<AdminDashboardListItemVM>();
            RecentActivities = new List<AdminDashboardActivityVM>();
            OverviewItems = new List<AdminDashboardProgressVM>();
            RecentVolunteers = new List<AdminDashboardVolunteerVM>();
        }

        public int TotalConstituencies { get; set; }
        public int ActiveHeroSlides { get; set; }
        public int ActiveNews { get; set; }
        public int UpcomingPublicEvents { get; set; }
        public int CitizenLeads { get; set; }
        public int TotalPeople { get; set; }
        public int TotalVolunteers { get; set; }
        public int GalleryImages { get; set; }
        public int DownloadDocuments { get; set; }
        public int WarRoomEvents { get; set; }
        public int PendingCampaignTasks { get; set; }
        public int OpenCampaignAlerts { get; set; }
        public int OpenJanSampark { get; set; }
        public int BoothsCovered { get; set; }
        public int TotalBooths { get; set; }
        public decimal BoothCoveragePercent { get; set; }
        public decimal PublicContentReadiness { get; set; }
        public decimal CampaignReadiness { get; set; }
        public string TodaySummary { get; set; }

        public List<AdminDashboardMetricVM> Metrics { get; set; }
        public List<AdminDashboardActionVM> QuickActions { get; set; }
        public List<AdminDashboardListItemVM> RecentNews { get; set; }
        public List<AdminDashboardListItemVM> UpcomingEvents { get; set; }
        public List<AdminDashboardListItemVM> CampaignAlerts { get; set; }
        public List<AdminDashboardActivityVM> RecentActivities { get; set; }
        public List<AdminDashboardProgressVM> OverviewItems { get; set; }
        public List<AdminDashboardVolunteerVM> RecentVolunteers { get; set; }
    }

    public class AdminDashboardMetricVM
    {
        public string Label { get; set; }
        public string Value { get; set; }
        public string Hint { get; set; }
        public string IconClass { get; set; }
        public string ToneClass { get; set; }
        public string ActionName { get; set; }
        public string ControllerName { get; set; }
    }

    public class AdminDashboardActionVM
    {
        public string Label { get; set; }
        public string Hint { get; set; }
        public string IconClass { get; set; }
        public string ActionName { get; set; }
        public string ControllerName { get; set; }
        public string ToneClass { get; set; }
    }

    public class AdminDashboardListItemVM
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string BadgeText { get; set; }
        public string BadgeClass { get; set; }
        public DateTime? Date { get; set; }
        public string ActionName { get; set; }
        public string ControllerName { get; set; }
        public object RouteValues { get; set; }
    }

    public class AdminDashboardActivityVM
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string IconClass { get; set; }
        public string ToneClass { get; set; }
        public string TimeText { get; set; }
        public string DateText { get; set; }
    }

    public class AdminDashboardProgressVM
    {
        public string Label { get; set; }
        public int Percentage { get; set; }
        public string IconClass { get; set; }
        public string ToneClass { get; set; }
    }

    public class AdminDashboardVolunteerVM
    {
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string Constituency { get; set; }
        public string RegisteredOn { get; set; }
        public string Status { get; set; }
    }
}
