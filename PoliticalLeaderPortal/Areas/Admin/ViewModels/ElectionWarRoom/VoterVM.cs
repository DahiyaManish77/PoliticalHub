using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom
{
    public class VoterVM
    {
        public int VoterId { get; set; }

        [StringLength(30)]
        [Display(Name = "EPIC / Voter ID")]
        public string EpicNumber { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Voter Name")]
        public string VoterName { get; set; }

        [StringLength(200)]
        [Display(Name = "Father / Husband Name")]
        public string FatherOrHusbandName { get; set; }

        [StringLength(20)]
        public string Gender { get; set; }

        [Range(18, 120)]
        public int? Age { get; set; }

        [StringLength(20)]
        [Display(Name = "Mobile Number")]
        public string MobileNumber { get; set; }

        [StringLength(12, MinimumLength = 12)]
        [Display(Name = "Aadhaar Number")]
        public string AadhaarNumber { get; set; }

        [Display(Name = "Passport Size Photo")]
        public string VoterPhotoPath { get; set; }

        [Display(Name = "Aadhaar Photo")]
        public string AadhaarPhotoPath { get; set; }

        public HttpPostedFileBase VoterPhotoFile { get; set; }
        public HttpPostedFileBase AadhaarPhotoFile { get; set; }

        [StringLength(100)]
        public string Caste { get; set; }

        [StringLength(100)]
        public string Religion { get; set; }

        [StringLength(100)]
        public string Category { get; set; }

        [StringLength(200)]
        public string State { get; set; }

        [StringLength(200)]
        public string District { get; set; }

        [StringLength(200)]
        [Display(Name = "Assembly Constituency")]
        public string AssemblyConstituency { get; set; }

        [StringLength(200)]
        [Display(Name = "Parliament Constituency")]
        public string ParliamentConstituency { get; set; }

        [StringLength(200)]
        public string Block { get; set; }

        [StringLength(200)]
        public string Village { get; set; }

        [StringLength(200)]
        public string Ward { get; set; }

        [StringLength(50)]
        [Display(Name = "Part Number")]
        public string PartNumber { get; set; }

        [StringLength(50)]
        [Display(Name = "Section Number")]
        public string SectionNumber { get; set; }

        [StringLength(50)]
        [Display(Name = "Serial Number")]
        public string SerialNumber { get; set; }

        [StringLength(50)]
        [Display(Name = "House Number")]
        public string HouseNumber { get; set; }

        [StringLength(200)]
        [Display(Name = "Booth Number")]
        public string BoothNumber { get; set; }

        [StringLength(250)]
        [Display(Name = "Booth Name")]
        public string BoothName { get; set; }

        [StringLength(300)]
        [Display(Name = "Polling Station")]
        public string PollingStation { get; set; }

        [Display(Name = "Full Address")]
        public string Address { get; set; }

        [StringLength(50)]
        [Display(Name = "Voter Type")]
        public string VoterType { get; set; }

        [StringLength(50)]
        [Display(Name = "Political Status")]
        public string PoliticalStatus { get; set; }

        [StringLength(50)]
        [Display(Name = "Support Level")]
        public string SupportLevel { get; set; }

        [StringLength(200)]
        [Display(Name = "Influencer Name")]
        public string InfluencerName { get; set; }

        [StringLength(200)]
        [Display(Name = "Panna Pramukh")]
        public string PannaPramukhName { get; set; }

        public bool IsPriorityVoter { get; set; }
        public bool IsFirstTimeVoter { get; set; }
        public bool IsDeleted { get; set; }
        public string Remarks { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
