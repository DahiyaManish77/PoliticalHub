namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom
{
    public class CampaignPollOptionVM
    {
        public int CampaignPollOptionId { get; set; }
        public int CampaignPollId { get; set; }
        public string OptionText { get; set; }
        public int DisplayOrder { get; set; }
        public int ResponseCount { get; set; }
        public decimal ResponsePercent { get; set; }
    }
}
