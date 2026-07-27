using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace PoliticalLeaderPortal.ViewModels.PeopleSay
{
    public class PeopleSaySubmissionVM
    {
        [Required, StringLength(150), Display(Name = "Your name")]
        public string PersonName { get; set; }

        [Required, StringLength(20), Display(Name = "Mobile number")]
        [RegularExpression(@"^[0-9+()\-\s]{10,15}$", ErrorMessage = "Enter a valid mobile number.")]
        public string MobileNumber { get; set; }

        [StringLength(150), Display(Name = "Village / City")]
        public string AreaName { get; set; }

        [StringLength(180), Display(Name = "Video title")]
        public string Title { get; set; }

        [StringLength(600), Display(Name = "Your message")]
        public string Message { get; set; }

        [Required, Display(Name = "Upload video")]
        public HttpPostedFileBase VideoFile { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "Consent is required.")]
        [Display(Name = "I confirm that this is my video and permit its publication on this website.")]
        public bool PublicationConsent { get; set; }
    }

    public class PeopleSayVideoVM
    {
        public int PeopleSayVideoId { get; set; }
        public string PersonName { get; set; }
        public string AreaName { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string VideoPath { get; set; }
        public string Status { get; set; }
        public string RejectionReason { get; set; }
        public string LeaderResponseVideoPath { get; set; }
        public string LeaderResponseMessage { get; set; }
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public int ShareCount { get; set; }
        public int DownloadCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool HasLeaderResponse { get { return !String.IsNullOrWhiteSpace(LeaderResponseVideoPath); } }
        public bool IsYouTube { get; set; }
        public string YoutubeVideoId { get; set; }
        public string ThumbnailUrl { get; set; }
    }

    public class PeopleSayHomeVM
    {
        public PeopleSayHomeVM()
        {
            Videos = new List<PeopleSayVideoVM>();
            Submission = new PeopleSaySubmissionVM();
        }
        public IList<PeopleSayVideoVM> Videos { get; set; }
        public PeopleSaySubmissionVM Submission { get; set; }
    }

    public class PeopleSayCommentVM
    {
        public int PeopleSayCommentId { get; set; }
        public int PeopleSayVideoId { get; set; }
        [Required, StringLength(100)] public string PersonName { get; set; }
        [Required, StringLength(500)] public string CommentText { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class PeopleSayAdminVM
    {
        public PeopleSayAdminVM()
        {
            Videos = new List<PeopleSayVideoVM>();
            Comments = new List<PeopleSayCommentVM>();
        }
        public string Status { get; set; }
        public string Keyword { get; set; }
        public IList<PeopleSayVideoVM> Videos { get; set; }
        public IList<PeopleSayCommentVM> Comments { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
    }

    public class PeopleSayAnalyticsVM
    {
        public PeopleSayAnalyticsVM() { TopVideos = new List<PeopleSayVideoVM>(); }
        public int TotalSubmissions { get; set; }
        public int ApprovedVideos { get; set; }
        public int PendingVideos { get; set; }
        public int TotalViews { get; set; }
        public int TotalLikes { get; set; }
        public int TotalComments { get; set; }
        public int TotalShares { get; set; }
        public int TotalDownloads { get; set; }
        public IList<PeopleSayVideoVM> TopVideos { get; set; }
    }
}
