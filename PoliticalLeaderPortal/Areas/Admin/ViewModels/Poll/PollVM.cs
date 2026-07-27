using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.Poll
{
    public class PollListVM
    {
        public int PollId { get; set; }
        public string Title { get; set; }
        public string Question { get; set; }
        public string CategoryName { get; set; }
        public string Status { get; set; }
        public string PublicSlug { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public int TotalVotes { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class PollEditVM
    {
        public PollEditVM()
        {
            Options = new List<PollOptionVM>();
            Setting = new PollSettingVM();
            Categories = new List<SelectListItem>();
        }

        public int PollId { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        [Display(Name = "Category")]
        public int PollCategoryId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Question is required.")]
        [StringLength(500)]
        public string Question { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Display(Name = "Poll Type")]
        public string PollType { get; set; }

        [Display(Name = "Question Type")]
        public string QuestionType { get; set; }

        [Display(Name = "Target Area")]
        public string TargetArea { get; set; }

        [Display(Name = "Website Placement")]
        public string DisplayMode { get; set; }

        public string PublicSlug { get; set; }

        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }

        public string Status { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public PollSettingVM Setting { get; set; }
        public List<PollOptionVM> Options { get; set; }
        public List<SelectListItem> Categories { get; set; }
    }

    public class PollSettingVM
    {
        public bool ShowPublicResults { get; set; }
        public bool RequireConsent { get; set; }
        public bool AllowMultipleVotes { get; set; }
        public bool RequireMobile { get; set; }
        public bool RequireName { get; set; }
        public string ResultVisibility { get; set; }
        public string DuplicatePolicy { get; set; }
        public int MaxVotesPerRespondent { get; set; }
        public bool IsAnonymous { get; set; }
        public string ThankYouMessage { get; set; }
        public string ClosedMessage { get; set; }
    }

    public class PollOptionVM
    {
        public int PollOptionId { get; set; }
        public int PollId { get; set; }
        public string OptionText { get; set; }
        public string OptionDescription { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public int VoteCount { get; set; }
        public decimal VotePercent { get; set; }
    }

    public class PollResultVM
    {
        public PollResultVM()
        {
            Options = new List<PollOptionVM>();
            Votes = new List<PollVoteVM>();
        }

        public int PollId { get; set; }
        public string Title { get; set; }
        public string Question { get; set; }
        public string Status { get; set; }
        public string PublicUrl { get; set; }
        public int TotalVotes { get; set; }
        public List<PollOptionVM> Options { get; set; }
        public List<PollVoteVM> Votes { get; set; }
    }

    public class PollVoteVM
    {
        public int PollVoteId { get; set; }
        public string OptionText { get; set; }
        public string RespondentName { get; set; }
        public string MobileNo { get; set; }
        public string AreaName { get; set; }
        public string Source { get; set; }
        public bool ConsentGiven { get; set; }
        public bool IsValid { get; set; }
        public DateTime SubmittedDate { get; set; }
    }
}
