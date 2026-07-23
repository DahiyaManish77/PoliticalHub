using System;
using System.Collections.Generic;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom
{
    public class DashboardVM
    {
        public DashboardVM()
        {
            UpcomingEvents = new List<EventSummaryVM>();
            RecentActivities = new List<ActivitySummaryVM>();
            Alerts = new List<AlertSummaryVM>();
            TopWorkers = new List<TopWorkerVM>();
            TopTeams = new List<TeamSummaryVM>();
            LowCoverageBooths = new List<ElectionBoothVM>();
            FollowUpVisits = new List<ElectionBoothVisitVM>();
            ActionPlan = new List<ActionPlanItemVM>();
        }

        #region Dashboard Cards

        public int TotalEvents { get; set; }

        public int TodayEvents { get; set; }

        public int UpcomingEventsCount { get; set; }

        public int PendingTasks { get; set; }

        public int CompletedTasks { get; set; }

        public int TotalVehicles { get; set; }

        public int TotalAttendance { get; set; }

        public int TotalGuests { get; set; }

        public int TotalTeams { get; set; }

        public int TotalBooths { get; set; }

        public int VisitedBooths { get; set; }

        public decimal BoothCoveragePercentage { get; set; }

        public int TotalPolls { get; set; }

        public int TotalSurveyResponses { get; set; }

        public int OpenComplaints { get; set; }

        public int ResolvedComplaints { get; set; }

        public decimal TodayExpense { get; set; }

        public decimal MonthExpense { get; set; }

        public int TotalAlerts { get; set; }

        public int CriticalAlerts { get; set; }

        public int ActiveIssues { get; set; }

        public int FieldOperations { get; set; }

        public int TotalCitizenInteractions { get; set; }

        public int OperationalReadinessScore { get; set; }

        public decimal TaskCompletionPercentage { get; set; }

        public decimal ComplaintResolutionPercentage { get; set; }

        public string ReadinessStatus { get; set; }

        public string CommandFocus { get; set; }

        public int HighPriorityBooths { get; set; }

        public int TodayBoothVisits { get; set; }

        public int TodayJanSampark { get; set; }

        public int FollowUpBoothVisits { get; set; }

        public int OverdueTasks { get; set; }

        public int InProgressTasks { get; set; }

        public int VerifiedAttendance { get; set; }

        public int VerifiedVehicles { get; set; }

        public int PendingCampaignAlerts { get; set; }

        public int UnreadCampaignAlerts { get; set; }

        public int NewSupporters { get; set; }

        public int OppositionSupporters { get; set; }

        public int NeutralFamilies { get; set; }

        public int TotalVisitedHouses { get; set; }

        public int TotalHouses { get; set; }

        public decimal VoterContactPercentage { get; set; }

        public decimal SupporterConversionPercentage { get; set; }

        public decimal ResourceVerificationPercentage { get; set; }

        public string PollingDayRisk { get; set; }

        public string SuggestedNextMove { get; set; }

        #endregion

        #region Dashboard Collections

        public List<EventSummaryVM> UpcomingEvents { get; set; }

        public List<ActivitySummaryVM> RecentActivities { get; set; }

        public List<AlertSummaryVM> Alerts { get; set; }

        public List<TopWorkerVM> TopWorkers { get; set; }

        public List<TeamSummaryVM> TopTeams { get; set; }

        public List<ElectionBoothVM> LowCoverageBooths { get; set; }

        public List<ElectionBoothVisitVM> FollowUpVisits { get; set; }

        public List<ActionPlanItemVM> ActionPlan { get; set; }

        #endregion
    }

    public class EventSummaryVM
    {
        public int EventId { get; set; }

        public string EventCode { get; set; }

        public string EventTitle { get; set; }

        public string EventType { get; set; }

        public DateTime EventDate { get; set; }

        public string Venue { get; set; }

        public string District { get; set; }

        public string Status { get; set; }
    }

    public class ActivitySummaryVM
    {
        public string ModuleName { get; set; }

        public string Activity { get; set; }

        public string PerformedBy { get; set; }

        public DateTime ActivityDate { get; set; }
    }

    public class AlertSummaryVM
    {
        public int AlertId { get; set; }

        public string AlertTitle { get; set; }

        public string Severity { get; set; }

        public string Status { get; set; }

        public DateTime CreatedDate { get; set; }
    }

    public class TopWorkerVM
    {
        public string MemberCode { get; set; }

        public string WorkerName { get; set; }

        public int Vehicles { get; set; }

        public int Attendance { get; set; }

        public int TasksCompleted { get; set; }

        public int SurveyCount { get; set; }

        public int PerformanceScore { get; set; }
    }

    public class TeamSummaryVM
    {
        public string TeamName { get; set; }

        public int Members { get; set; }

        public int PresentMembers { get; set; }

        public string Status { get; set; }
    }

    public class ActionPlanItemVM
    {
        public string Title { get; set; }

        public string Detail { get; set; }

        public string ActionName { get; set; }

        public string Tone { get; set; }

        public int Count { get; set; }
    }
}
