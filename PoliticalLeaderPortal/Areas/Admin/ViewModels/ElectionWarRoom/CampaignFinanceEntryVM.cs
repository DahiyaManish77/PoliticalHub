using System;
using System.ComponentModel.DataAnnotations;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom
{
    public class CampaignFinanceEntryVM
    {
        public int CampaignFinanceEntryId { get; set; }

        [Required, StringLength(40)]
        public string EntryType { get; set; }

        [Required, StringLength(160)]
        public string Title { get; set; }

        [StringLength(80)]
        public string ReferenceNo { get; set; }

        public DateTime EntryDate { get; set; }

        [StringLength(150)]
        public string PersonOrVendorName { get; set; }

        [StringLength(30)]
        public string MobileNo { get; set; }

        [StringLength(120)]
        public string Category { get; set; }

        [StringLength(80)]
        public string PaymentMode { get; set; }

        public decimal Amount { get; set; }

        [StringLength(300)]
        public string ProofUrl { get; set; }

        [StringLength(40)]
        public string ApprovalStatus { get; set; }

        [StringLength(120)]
        public string ApprovedBy { get; set; }

        [StringLength(500)]
        public string Remarks { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
