using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.VoiceAgent
{
    public class VoiceAgentDashboardVM
    {
        public VoiceAgentDashboardVM() { Calls = new List<VoiceCallVM>(); MissingConfiguration = new List<string>(); }
        public bool IsConfigured { get; set; }
        public bool IsEnabled { get; set; }
        public string ProviderName { get; set; }
        public string PhoneNumber { get; set; }
        public string IncomingWebhookUrl { get; set; }
        public string StatusWebhookUrl { get; set; }
        public string RecordingWebhookUrl { get; set; }
        public string HealthCheckUrl { get; set; }
        public int ReadinessPercent { get; set; }
        public IList<string> MissingConfiguration { get; set; }
        public int TotalCalls { get; set; }
        public int AnsweredCalls { get; set; }
        public int MissedCalls { get; set; }
        public int CallsToday { get; set; }
        public int TotalMinutes { get; set; }
        public IList<VoiceCallVM> Calls { get; set; }
    }

    public class VoiceCallVM
    {
        public long VoiceCallId { get; set; }
        public string ProviderCallId { get; set; }
        public string Direction { get; set; }
        public string CallerNumber { get; set; }
        public string CalledNumber { get; set; }
        public string Status { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? AnsweredAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public int DurationSeconds { get; set; }
        public string Language { get; set; }
        public string Intent { get; set; }
        public string Transcript { get; set; }
        public string Summary { get; set; }
        public string RecordingUrl { get; set; }
        public string LocalRecordingPath { get; set; }
        public bool HasRecording { get { return !String.IsNullOrWhiteSpace(LocalRecordingPath) || !String.IsNullOrWhiteSpace(RecordingUrl); } }
    }

    public class VoiceAgentSettingVM
    {
        [Required, StringLength(50), Display(Name = "Voice API provider")]
        public string ProviderName { get; set; }
        [StringLength(100), Display(Name = "Account SID / API key")]
        public string AccountSid { get; set; }
        [StringLength(250), Display(Name = "Auth token / API secret")]
        public string AuthToken { get; set; }
        [StringLength(30), Display(Name = "Voice-enabled phone number")]
        public string PhoneNumber { get; set; }
        [Required, StringLength(100), Display(Name = "Webhook security secret")]
        public string WebhookSecret { get; set; }
        [StringLength(300), Display(Name = "Public HTTPS base URL")]
        public string PublicBaseUrl { get; set; }
        public bool IsEnabled { get; set; }
        public bool EnableRecording { get; set; }
        [Required, StringLength(500), Display(Name = "Recording consent announcement")]
        public string RecordingConsentText { get; set; }
        [Required, StringLength(500), Display(Name = "Hindi welcome message")]
        public string WelcomeMessageHindi { get; set; }
        [Required, StringLength(500), Display(Name = "English welcome message")]
        public string WelcomeMessageEnglish { get; set; }
        [StringLength(300), Display(Name = "AI API endpoint (optional)")]
        public string AiApiEndpoint { get; set; }
        [StringLength(250), Display(Name = "AI API key (optional)")]
        public string AiApiKey { get; set; }
        [Range(1, 3650), Display(Name = "Recording retention days")]
        public int RecordingRetentionDays { get; set; }
    }

    public class BulkVoiceCallerDashboardVM
    {
        public BulkVoiceCallerDashboardVM()
        {
            Campaigns = new List<BulkVoiceCampaignVM>();
            MissingConfiguration = new List<string>();
            EligiblePeople = new List<BulkVoiceRecipientVM>();
        }
        public bool IsConfigured { get; set; }
        public bool IsEnabled { get; set; }
        public string ProviderName { get; set; }
        public string PhoneNumber { get; set; }
        public int ReadinessPercent { get; set; }
        public int EligibleMembers { get; set; }
        public IList<string> MissingConfiguration { get; set; }
        public IList<BulkVoiceCampaignVM> Campaigns { get; set; }
        public IList<BulkVoiceRecipientVM> EligiblePeople { get; set; }
    }

    public class BulkVoiceCampaignVM
    {
        public int BulkVoiceCampaignId { get; set; }
        [Required, StringLength(120)]
        public string CampaignName { get; set; }
        [Required, StringLength(1000)]
        public string MessageText { get; set; }
        [StringLength(10)]
        public string LanguageCode { get; set; }
        public string Status { get; set; }
        public int TotalRecipients { get; set; }
        public int PendingCount { get; set; }
        public int InProgressCount { get; set; }
        public int CompletedCount { get; set; }
        public int FailedCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class BulkVoiceRecipientVM
    {
        public int PersonId { get; set; }
        public string FullName { get; set; }
        public string MobileNumber { get; set; }
        public string PreferredLanguage { get; set; }
        public string VillageName { get; set; }
        public string GramPanchayatName { get; set; }
        public DateTime? ConsentDate { get; set; }
    }
}
