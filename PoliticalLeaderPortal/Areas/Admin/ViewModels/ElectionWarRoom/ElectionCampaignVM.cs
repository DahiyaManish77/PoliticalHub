using System;
using System.ComponentModel.DataAnnotations;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom
{
    /// <summary>
    /// ViewModel for Election Campaign.
    /// </summary>
    public class ElectionCampaignVM
    {
        public int CampaignId { get; set; }

        [Required]
        [Display(Name = "Campaign Name")]
        [StringLength(200)]
        public string CampaignName { get; set; }

        [Required]
        [Display(Name = "Election Type")]
        [StringLength(100)]
        public string ElectionType { get; set; }

        [Display(Name = "State")]
        public int? StateId { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}