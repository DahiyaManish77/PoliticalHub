using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.People
{
    public class PersonListItemVM
    {
        public int PersonId { get; set; }
        public string PersonCode { get; set; }
        public string FullName { get; set; }
        public string MobileNumber { get; set; }
        public string Email { get; set; }
        public string AreaName { get; set; }
        public string PreferredRole { get; set; }
        public string VerificationStatus { get; set; }
        public string ApprovalStatus { get; set; }
        public string VolunteerStatus { get; set; }
        public bool IsVolunteer { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class PeopleIndexVM
    {
        public PeopleIndexVM() { Rows = new List<PersonListItemVM>(); }
        public string Keyword { get; set; }
        public string Status { get; set; }
        public int? AssemblyConstituencyId { get; set; }
        public bool VolunteersOnly { get; set; }
        public int TotalPeople { get; set; }
        public int TotalVolunteers { get; set; }
        public int PendingVerification { get; set; }
        public int ActivePeople { get; set; }
        public IList<PersonListItemVM> Rows { get; set; }
        public IList<SelectListItem> AssemblyOptions { get; set; }
    }

    public class PersonEditVM
    {
        public PersonEditVM() { IsActive = true; VerificationStatus = "Pending"; ApprovalStatus = "Pending"; }
        public int PersonId { get; set; }
        public int? VolunteerProfileId { get; set; }

        [Required, StringLength(150)]
        [Display(Name = "Full name")]
        public string FullName { get; set; }

        [Required, StringLength(15)]
        [RegularExpression(@"^[0-9+()\-\s]{10,15}$", ErrorMessage = "Enter a valid mobile number.")]
        public string MobileNumber { get; set; }

        [EmailAddress, StringLength(200)]
        public string Email { get; set; }
        public string Gender { get; set; }
        [DataType(DataType.Date)] public DateTime? DateOfBirth { get; set; }
        public string AddressLine { get; set; }
        public string Landmark { get; set; }
        public string PinCode { get; set; }
        public string PreferredLanguage { get; set; }

        public int? StateId { get; set; }
        public int? DistrictId { get; set; }
        public int? ParliamentaryConstituencyId { get; set; }
        public int? AssemblyConstituencyId { get; set; }
        public int? TehsilId { get; set; }
        public int? BlockId { get; set; }
        public int? GramPanchayatId { get; set; }
        public int? VillageId { get; set; }
        public int? WardId { get; set; }
        public int? MandalId { get; set; }
        public int? SectorId { get; set; }
        public int? BoothId { get; set; }

        public bool IsVolunteer { get; set; }
        public string PreferredRole { get; set; }
        public string Skills { get; set; }
        public string AvailableDays { get; set; }
        public string AvailableTime { get; set; }
        public string EmergencyContactName { get; set; }
        public string EmergencyContactMobile { get; set; }
        public string ApprovalStatus { get; set; }
        public string VolunteerStatus { get { return ApprovalStatus; } set { ApprovalStatus = value; } }
        public DateTime? JoiningDate { get; set; }
        public string Notes { get; set; }

        public bool WhatsAppConsent { get; set; }
        public bool SmsConsent { get; set; }
        public bool EmailConsent { get; set; }
        public bool VoiceConsent { get; set; }
        public bool VoiceCallConsent { get { return VoiceConsent; } set { VoiceConsent = value; } }
        public bool IsOptedOut { get; set; }
        public string ConsentSource { get; set; }
        public string VerificationStatus { get; set; }
        public bool IsActive { get; set; }

        public IList<SelectListItem> States { get; set; }
        public IList<SelectListItem> Districts { get; set; }
        public IList<SelectListItem> ParliamentaryConstituencies { get; set; }
        public IList<SelectListItem> AssemblyConstituencies { get; set; }
        public IList<SelectListItem> Tehsils { get; set; }
        public IList<SelectListItem> Blocks { get; set; }
        public IList<SelectListItem> GramPanchayats { get; set; }
        public IList<SelectListItem> Villages { get; set; }
        public IList<SelectListItem> Wards { get; set; }
        public IList<SelectListItem> Mandals { get; set; }
        public IList<SelectListItem> Sectors { get; set; }
        public IList<SelectListItem> Booths { get; set; }
    }
}
