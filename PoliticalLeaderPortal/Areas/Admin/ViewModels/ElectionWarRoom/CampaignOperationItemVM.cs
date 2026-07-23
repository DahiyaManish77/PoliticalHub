using System;
using System.ComponentModel.DataAnnotations;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom
{
    public class CampaignOperationItemVM
    {
        public int CampaignOperationItemId { get; set; }

        [Display(Name = "Module")]
        public string ModuleKey { get; set; }

        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Display(Name = "Category")]
        public string Category { get; set; }

        [Display(Name = "Owner")]
        public string OwnerName { get; set; }

        [Display(Name = "Mobile")]
        public string OwnerMobile { get; set; }

        [Display(Name = "Area / Booth")]
        public string AreaName { get; set; }

        [Display(Name = "Priority")]
        public string Priority { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }

        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Quantity")]
        public int? Quantity { get; set; }

        [Display(Name = "Budget")]
        public decimal? BudgetAmount { get; set; }

        [Display(Name = "Reference URL")]
        public string ReferenceUrl { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Compliance Note")]
        public string ComplianceNote { get; set; }

        [Display(Name = "Approved")]
        public bool IsApproved { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        public int? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
