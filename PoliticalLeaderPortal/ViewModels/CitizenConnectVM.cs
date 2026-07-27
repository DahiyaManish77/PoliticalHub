using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web;

namespace PoliticalLeaderPortal.ViewModels
{
    public class CitizenConnectVM
    {
        public CitizenConnectVM()
        {
            States = new List<SelectListItem>();
            Districts = new List<SelectListItem>();
            ParliamentaryConstituencies = new List<SelectListItem>();
            AssemblyConstituencies = new List<SelectListItem>();
            Blocks = new List<SelectListItem>();
            GramPanchayats = new List<SelectListItem>();
            Villages = new List<SelectListItem>();
        }

        public int CitizenConnectId { get; set; }

        [Required, StringLength(40)]
        public string RequestType { get; set; }

        [Required, StringLength(160), Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required, StringLength(20), Display(Name = "Mobile Number")]
        [RegularExpression(@"^[0-9+()\-\s]{10,15}$", ErrorMessage = "Enter a valid mobile number.")]
        public string MobileNumber { get; set; }

        [StringLength(160), EmailAddress]
        public string Email { get; set; }

        public int? StateId { get; set; }
        public int? DistrictId { get; set; }
        [Display(Name = "Parliamentary Constituency")]
        public int? ParliamentaryConstituencyId { get; set; }
        public int? AssemblyConstituencyId { get; set; }
        public int? BlockId { get; set; }
        public int? GramPanchayatId { get; set; }
        public int? VillageId { get; set; }

        [StringLength(160), Display(Name = "Village")]
        public string VillageName { get; set; }

        [StringLength(160)] public string District { get; set; }
        [StringLength(160)] public string Assembly { get; set; }

        [Required, StringLength(250)]
        public string Subject { get; set; }

        [Required, DataType(DataType.MultilineText)]
        public string Message { get; set; }

        [StringLength(100), Display(Name = "Preferred Volunteer Role")]
        public string PreferredRole { get; set; }

        [StringLength(500)] public string Skills { get; set; }
        [StringLength(150), Display(Name = "Available Days")] public string AvailableDays { get; set; }
        [StringLength(100), Display(Name = "Available Time")] public string AvailableTime { get; set; }

        [Display(Name = "I agree to receive volunteer coordination messages on WhatsApp")]
        public bool WhatsAppConsent { get; set; }
        [Display(Name = "I agree to receive SMS updates")]
        public bool SmsConsent { get; set; }
        [Display(Name = "I agree to receive email updates")]
        public bool EmailConsent { get; set; }
        [Display(Name = "I agree to receive coordination voice calls")]
        public bool VoiceConsent { get; set; }
        [Range(typeof(bool), "true", "true", ErrorMessage = "Consent is required to submit a volunteer application.")]
        public bool PrivacyConsent { get; set; }

        [StringLength(40)] public string Status { get; set; }
        public string AdminRemarks { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ConvertedPersonId { get; set; }
        public DateTime? ConvertedDate { get; set; }
        public bool IsConverted { get { return ConvertedPersonId.HasValue; } }

        [Display(Name = "Invitation card or supporting file")]
        public HttpPostedFileBase AttachmentFile { get; set; }

        public IList<SelectListItem> States { get; set; }
        public IList<SelectListItem> Districts { get; set; }
        public IList<SelectListItem> ParliamentaryConstituencies { get; set; }
        public IList<SelectListItem> AssemblyConstituencies { get; set; }
        public IList<SelectListItem> Blocks { get; set; }
        public IList<SelectListItem> GramPanchayats { get; set; }
        public IList<SelectListItem> Villages { get; set; }
    }
}
