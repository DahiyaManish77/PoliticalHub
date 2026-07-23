using System;
using System.ComponentModel.DataAnnotations;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom
{
    public class EventTaskActivityVM
    {
        public int EventTaskActivityId { get; set; }

        public int EventTaskId { get; set; }

        public string TaskTitle { get; set; }

        [Display(Name = "Activity Type")]
        public string ActivityType { get; set; }

        public string OldStatus { get; set; }

        public string NewStatus { get; set; }

        public int? ProgressPercentage { get; set; }

        [Display(Name = "Remarks")]
        public string ActivityRemarks { get; set; }

        public string AttachmentPath { get; set; }

        public string ActivityByMemberCode { get; set; }

        public string ActivityByName { get; set; }

        public DateTime ActivityDate { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public bool IsActive { get; set; }

        public int? CreatedBy { get; set; }

        public string CreatedByName { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? UpdatedBy { get; set; }

        public string UpdatedByName { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}