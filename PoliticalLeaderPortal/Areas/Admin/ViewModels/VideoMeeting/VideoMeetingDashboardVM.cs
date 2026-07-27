using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.VideoMeeting
{
    public class VideoMeetingDashboardVM
    {
        public VideoMeetingDashboardVM()
        {
            Capabilities = new List<string>();
            Meetings = new List<VideoMeetingListItemVM>();
        }

        public bool ProviderConnected { get; set; }
        public int UpcomingMeetings { get; set; }
        public int LiveMeetings { get; set; }
        public int CompletedMeetings { get; set; }
        public IList<string> Capabilities { get; set; }
        public IList<VideoMeetingListItemVM> Meetings { get; set; }
    }

    public class VideoMeetingListItemVM
    {
        public int VideoMeetingId { get; set; }
        public string Title { get; set; }
        public string MeetingType { get; set; }
        public DateTime ScheduledStart { get; set; }
        public int DurationMinutes { get; set; }
        public int MaximumParticipants { get; set; }
        public string Status { get; set; }
        public bool AllowRecording { get; set; }
        public string SecureJoinToken { get; set; }
    }

    public class VideoMeetingEditVM
    {
        public int VideoMeetingId { get; set; }
        [Required, StringLength(180)] public string Title { get; set; }
        [StringLength(1000)] public string Description { get; set; }
        [Required, StringLength(40), Display(Name = "Meeting type")] public string MeetingType { get; set; }
        [Required, Display(Name = "Start date and time")] public DateTime ScheduledStart { get; set; }
        [Range(15, 1440), Display(Name = "Duration (minutes)")] public int DurationMinutes { get; set; }
        [Range(2, 5000), Display(Name = "Maximum participants")] public int MaximumParticipants { get; set; }
        [Display(Name = "Allow participant cameras")] public bool AllowParticipantCamera { get; set; }
        [Display(Name = "Allow participant microphones")] public bool AllowParticipantMicrophone { get; set; }
        [Display(Name = "Allow recording")] public bool AllowRecording { get; set; }
        [Display(Name = "Start recording automatically")] public bool AutoRecord { get; set; }
        [Display(Name = "Require host approval to join")] public bool RequireHostApproval { get; set; }
        [StringLength(2000), Display(Name = "Invite people")] public string Invitees { get; set; }
        public string Status { get; set; }
    }
}
