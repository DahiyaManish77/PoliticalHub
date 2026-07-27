using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionCampaignMaster
{
    public class ElectionMasterVM
    {
        public int ElectionId { get; set; }
        [Required, StringLength(160), Display(Name = "Election Name")]
        public string ElectionName { get; set; }
        [Required, StringLength(80), Display(Name = "Election Type")]
        public string ElectionType { get; set; }
        [Required, Range(2000, 2200), Display(Name = "Election Year")]
        public int ElectionYear { get; set; }
        [StringLength(120)] public string StateName { get; set; }
        [StringLength(120)] public string DistrictName { get; set; }
        [Display(Name = "Notification Date")] public DateTime? NotificationDate { get; set; }
        [Display(Name = "Nomination Start")] public DateTime? NominationStartDate { get; set; }
        [Display(Name = "Nomination End")] public DateTime? NominationEndDate { get; set; }
        [Display(Name = "Polling Date")] public DateTime? PollingDate { get; set; }
        [Display(Name = "Counting Date")] public DateTime? CountingDate { get; set; }
        [Required, StringLength(40)] public string Status { get; set; }
        [Display(Name = "Active")] public bool IsActive { get; set; }
        [AllowHtml] public string Description { get; set; }
    }

    public class CampaignMasterVM
    {
        public int CampaignMasterId { get; set; }
        [Required, Display(Name = "Election")] public int ElectionId { get; set; }
        public string ElectionName { get; set; }
        [Required, StringLength(180), Display(Name = "Campaign Name")] public string CampaignName { get; set; }
        [Required, StringLength(50), Display(Name = "Campaign Code")] public string CampaignCode { get; set; }
        [StringLength(180), Display(Name = "Candidate Name")] public string CandidateName { get; set; }
        [StringLength(150), Display(Name = "Constituency")] public string ConstituencyName { get; set; }
        [StringLength(80), Display(Name = "Constituency Number")] public string ConstituencyNumber { get; set; }
        [StringLength(150), Display(Name = "Campaign Owner")] public string CampaignOwner { get; set; }
        [Required, DataType(DataType.Date), Display(Name = "Start Date")] public DateTime StartDate { get; set; }
        [DataType(DataType.Date), Display(Name = "End Date")] public DateTime? EndDate { get; set; }
        [Required, StringLength(40)] public string Status { get; set; }
        [Range(0, 100), Display(Name = "Progress %")] public int ProgressPercent { get; set; }
        [Display(Name = "Primary Campaign")] public bool IsPrimary { get; set; }
        [Display(Name = "Active")] public bool IsActive { get; set; }
        [AllowHtml] public string Goals { get; set; }
        [AllowHtml] public string Description { get; set; }
    }

    public class ElectionCampaignDashboardVM
    {
        public int TotalElections { get; set; }
        public int ActiveElections { get; set; }
        public int TotalCampaigns { get; set; }
        public int ActiveCampaigns { get; set; }
        public IList<ElectionMasterVM> Elections { get; set; }
        public IList<CampaignMasterVM> Campaigns { get; set; }
        public ElectionCampaignDashboardVM() { Elections = new List<ElectionMasterVM>(); Campaigns = new List<CampaignMasterVM>(); }
    }
}
