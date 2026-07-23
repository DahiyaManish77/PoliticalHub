using System;
using System.ComponentModel.DataAnnotations;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom
{
    public class CampaignPollResponseVM
    {
        public int CampaignPollResponseId { get; set; }
        public int CampaignPollId { get; set; }

        [Required]
        public int CampaignPollOptionId { get; set; }

        public string OptionText { get; set; }
        public string RespondentName { get; set; }
        public string MobileNo { get; set; }
        public string AreaName { get; set; }
        public string Source { get; set; }
        public string Remarks { get; set; }
        public bool ConsentGiven { get; set; }
        public DateTime SubmittedOn { get; set; }
    }
}
