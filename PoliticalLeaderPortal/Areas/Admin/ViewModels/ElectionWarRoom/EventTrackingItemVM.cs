using System;
using System.ComponentModel.DataAnnotations;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom
{
    public class EventTrackingItemVM
    {
        public int EventTrackingItemId { get; set; }

        [Required]
        public int EventId { get; set; }

        public string EventTitle { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        public string ItemName { get; set; }

        public string Village { get; set; }
        public string Ward { get; set; }
        public string Booth { get; set; }
        public string ResponsiblePerson { get; set; }
        public string ResponsibleMobile { get; set; }
        public int ExpectedQuantity { get; set; }
        public int ActualQuantity { get; set; }
        public string Unit { get; set; }
        public decimal EstimatedCost { get; set; }
        public decimal ActualCost { get; set; }
        public string ProviderName { get; set; }
        public string ProviderMobile { get; set; }
        public bool ReturnRequired { get; set; }
        public bool Returned { get; set; }
        public bool AppreciationPending { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
