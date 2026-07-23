using System;
using System.ComponentModel.DataAnnotations;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom
{
    public class CampaignAuditLogVM
    {
        public int CampaignAuditLogId { get; set; }

        [Required, StringLength(100)]
        public string ModuleName { get; set; }

        [StringLength(80)]
        public string RecordId { get; set; }

        [Required, StringLength(80)]
        public string ActionName { get; set; }

        [StringLength(120)]
        public string PerformedBy { get; set; }

        public int? PerformedByUserId { get; set; }

        public DateTime PerformedOn { get; set; }

        [StringLength(80)]
        public string IpAddress { get; set; }

        [StringLength(500)]
        public string Remarks { get; set; }

        public bool IsSensitive { get; set; }
    }
}
