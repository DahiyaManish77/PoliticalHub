using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PoliticalLeaderPortal.ViewModels.Poll
{
    public class PublicPollVM
    {
        public PublicPollVM()
        {
            Options = new List<PublicPollOptionVM>();
        }

        public int PollId { get; set; }
        public string Title { get; set; }
        public string Question { get; set; }
        public string Description { get; set; }
        public string PublicSlug { get; set; }
        public string Status { get; set; }
        public string DisplayMode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool ShowPublicResults { get; set; }
        public bool RequireConsent { get; set; }
        public bool AllowMultipleVotes { get; set; }
        public bool RequireMobile { get; set; }
        public bool RequireName { get; set; }
        public string ResultVisibility { get; set; }
        public string ThankYouMessage { get; set; }
        public string ClosedMessage { get; set; }
        public int TotalVotes { get; set; }
        public List<PublicPollOptionVM> Options { get; set; }
    }

    public class PublicPollOptionVM
    {
        public int PollOptionId { get; set; }
        public string OptionText { get; set; }
        public int VoteCount { get; set; }
        public decimal VotePercent { get; set; }
    }

    public class PublicPollVoteVM
    {
        public int PollId { get; set; }
        public string PublicSlug { get; set; }

        [Required(ErrorMessage = "Please select an option.")]
        public int PollOptionId { get; set; }

        public string RespondentName { get; set; }
        public string MobileNo { get; set; }
        public string AreaName { get; set; }
        public string Remarks { get; set; }
        public string Source { get; set; }
        public bool ConsentGiven { get; set; }
    }
}
