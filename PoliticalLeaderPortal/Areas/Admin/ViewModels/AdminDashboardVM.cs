using System;
using System.Collections.Generic;
using System.Web.Mvc;

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
        public int TodayFieldEvents { get; set; }
        public int OverdueTasks { get; set; }
        public int CriticalAlerts { get; set; }
        public int ActiveVerifiedDocuments { get; set; }
        public int ExpiringVerifiedDocuments { get; set; }
        public int NewCitizenIssuesToday { get; set; }
        public int TotalCampaignTasks { get; set; }
        public int CompletedCampaignTasks { get; set; }
        public int ResolvedJanSampark { get; set; }
        public int BoothsCovered { get; set; }
        public int TotalBooths { get; set; }
        public decimal PlannedEventBudget { get; set; }
        public decimal ActualEventExpense { get; set; }
        public decimal BoothCoveragePercent { get; set; }
        public decimal TaskCompletionPercent { get; set; }
        public decimal IssueResolutionPercent { get; set; }
        public decimal BudgetUtilizationPercent { get; set; }
        public decimal PublicContentReadiness { get; set; }
        public decimal CampaignReadiness { get; set; }
        public string TodaySummary { get; set; }
        public bool IsCampaignFiltered { get; set; }

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
        public string TypeText { get; set; }
        public string StatusText { get; set; }
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

    public class CampaignContextVM
    {
        public CampaignContextVM()
        {
            Campaigns = new List<SelectListItem>();
        }

        public int? CampaignId { get; set; }
        public string CampaignName { get; set; }
        public IList<SelectListItem> Campaigns { get; set; }
        public string ReturnUrl { get; set; }
    }
}
