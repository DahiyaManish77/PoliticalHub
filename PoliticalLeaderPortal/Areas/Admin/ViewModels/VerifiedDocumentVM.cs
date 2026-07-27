using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels
{
    public class VerifiedDocumentVM
    {
        public int VerifiedDocumentId { get; set; }
        public string DocumentNumber { get; set; }
        public string VerificationCode { get; set; }

        [Required]
        [Display(Name = "Document type")]
        public string DocumentType { get; set; }

        [Required, StringLength(150)]
        [Display(Name = "Recipient name")]
        public string RecipientName { get; set; }

        [StringLength(80)]
        [Display(Name = "Member / employee ID")]
        public string RecipientReference { get; set; }

        [StringLength(120)]
        [Display(Name = "Role / designation")]
        public string RecipientRole { get; set; }

        [Display(Name = "Recipient photo")]
        public string RecipientPhotoPath { get; set; }

        public HttpPostedFileBase RecipientPhotoFile { get; set; }

        [Display(Name = "Campaign")]
        public int? CampaignId { get; set; }

        [StringLength(250)]
        public string Subject { get; set; }

        [AllowHtml]
        [Display(Name = "Letter content")]
        public string BodyText { get; set; }

        [Required, DataType(DataType.Date)]
        [Display(Name = "Issue date")]
        public DateTime IssueDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Valid until")]
        public DateTime? ExpiryDate { get; set; }

        [StringLength(150)]
        [Display(Name = "Issuer name")]
        public string IssuedByName { get; set; }

        [StringLength(120)]
        [Display(Name = "Issuer designation")]
        public string IssuedByDesignation { get; set; }

        public string Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CampaignName { get; set; }
        public IEnumerable<SelectListItem> Campaigns { get; set; }

        public bool IsLetter
        {
            get { return !String.Equals(DocumentType, "DigitalCard", StringComparison.OrdinalIgnoreCase); }
        }
    }

    public class VerifiedDocumentIndexVM
    {
        public VerifiedDocumentIndexVM()
        {
            Documents = new List<VerifiedDocumentVM>();
        }

        public IList<VerifiedDocumentVM> Documents { get; set; }
        public int ActiveCount { get; set; }
        public int RevokedCount { get; set; }
        public int ExpiredCount { get; set; }
    }

    public class PublicDocumentVerificationVM
    {
        public bool Found { get; set; }
        public bool IsValid { get; set; }
        public string VerificationState { get; set; }
        public string DocumentNumber { get; set; }
        public string DocumentType { get; set; }
        public string RecipientName { get; set; }
        public string RecipientReference { get; set; }
        public string RecipientRole { get; set; }
        public string CampaignName { get; set; }
        public string Subject { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string IssuedByName { get; set; }
        public string IssuedByDesignation { get; set; }
    }
}
