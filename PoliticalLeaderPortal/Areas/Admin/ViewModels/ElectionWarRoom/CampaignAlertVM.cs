using System;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom
{
    /// <summary>
    /// ViewModel used for Campaign Alert module.
    /// This ViewModel is reused throughout the entire module.
    /// </summary>
    public class CampaignAlertVM
    {
        public int CampaignAlertId { get; set; }

        public int? CampaignId { get; set; }

        public string CampaignName { get; set; }

        public string AlertTitle { get; set; }

        public string AlertMessage { get; set; }

        public string AlertType { get; set; }

        public string ReferenceModule { get; set; }

        public int? ReferenceId { get; set; }

        public string Severity { get; set; }

        public string AlertStatus { get; set; }

        public string AssignedToMemberCode { get; set; }

        public string AssignedToName { get; set; }

        public string ActionTaken { get; set; }

        public string ActionTakenByMemberCode { get; set; }

        public DateTime? ActionTakenDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public bool IsDashboard { get; set; }

        public bool IsNotificationSent { get; set; }

        public bool IsResolved { get; set; }

        public bool IsActive { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string AlertSource { get; set; }

        public bool IsRead { get; set; }

        public DateTime? ReadDate { get; set; }

        public string ReadByMemberCode { get; set; }
    }
}