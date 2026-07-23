using System;
using System.ComponentModel.DataAnnotations;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels.ElectionWarRoom
{
    public class CandidateProfileVM
    {
        public int CandidateProfileId { get; set; }

        [Required, StringLength(150)]
        public string FullName { get; set; }

        [StringLength(100)]
        public string PartyName { get; set; }

        [StringLength(120)]
        public string ElectionType { get; set; }

        [StringLength(150)]
        public string ConstituencyName { get; set; }

        [StringLength(120)]
        public string District { get; set; }

        [StringLength(120)]
        public string State { get; set; }

        [StringLength(200)]
        public string Education { get; set; }

        [StringLength(150)]
        public string Profession { get; set; }

        [StringLength(500)]
        public string PublicBio { get; set; }

        [StringLength(300)]
        public string ManifestoUrl { get; set; }

        [StringLength(300)]
        public string AffidavitUrl { get; set; }

        [StringLength(300)]
        public string PhotoUrl { get; set; }

        [StringLength(300)]
        public string FacebookUrl { get; set; }

        [StringLength(300)]
        public string TwitterUrl { get; set; }

        [StringLength(300)]
        public string InstagramUrl { get; set; }

        [StringLength(300)]
        public string YouTubeUrl { get; set; }

        public decimal DeclaredAssets { get; set; }
        public decimal DeclaredLiabilities { get; set; }

        [StringLength(500)]
        public string CriminalCaseSummary { get; set; }

        [StringLength(40)]
        public string ApprovalStatus { get; set; }

        public bool IsPublished { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
