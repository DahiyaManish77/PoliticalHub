using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom
{
    public class CampaignPollVM
    {
        public CampaignPollVM()
        {
            Options = new List<CampaignPollOptionVM>();
            Responses = new List<CampaignPollResponseVM>();
        }

        public int CampaignPollId { get; set; }

        [Required]
        [Display(Name = "Poll Title")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Question")]
        public string Question { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Area / Constituency")]
        public string TargetArea { get; set; }

        [Display(Name = "Poll Type")]
        public string PollType { get; set; }

        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Public Results")]
        public bool ShowPublicResults { get; set; }

        [Display(Name = "Consent Required")]
        public bool RequireConsent { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        public string PublicSlug { get; set; }
        public string PublicUrl { get; set; }
        public string WhatsAppShareUrl { get; set; }
        public string FacebookShareUrl { get; set; }
        public int TotalResponses { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<CampaignPollOptionVM> Options { get; set; }
        public List<CampaignPollResponseVM> Responses { get; set; }
    }
}
