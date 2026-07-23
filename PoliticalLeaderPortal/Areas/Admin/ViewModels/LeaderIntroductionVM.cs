using System.ComponentModel.DataAnnotations;
using System.Web;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels
{
    public class LeaderIntroductionVM
    {
        public int LeaderIntroductionId { get; set; }

        [Required]
        [Display(Name = "Leader Name")]
        public string LeaderName { get; set; }

        [Display(Name = "Role / Tagline")]
        public string RoleTagline { get; set; }

        [Display(Name = "Section Label")]
        public string SectionLabel { get; set; }

        [Display(Name = "Intro Paragraph")]
        public string IntroText { get; set; }

        [Display(Name = "Second Paragraph")]
        public string SecondaryText { get; set; }

        [Display(Name = "Vision Title")]
        public string VisionTitle { get; set; }

        [Display(Name = "Vision Text")]
        public string VisionText { get; set; }

        [Display(Name = "Mission Title")]
        public string MissionTitle { get; set; }

        [Display(Name = "Mission Text")]
        public string MissionText { get; set; }

        [Display(Name = "Portrait Image")]
        public string PortraitImagePath { get; set; }

        [Display(Name = "Status Title")]
        public string StatusTitle { get; set; }

        [Display(Name = "Status Text")]
        public string StatusText { get; set; }

        [Display(Name = "Primary Button Text")]
        public string PrimaryButtonText { get; set; }

        [Display(Name = "Primary Button Url")]
        public string PrimaryButtonUrl { get; set; }

        [Display(Name = "Secondary Button Text")]
        public string SecondaryButtonText { get; set; }

        [Display(Name = "Secondary Button Url")]
        public string SecondaryButtonUrl { get; set; }

        public bool IsActive { get; set; }
        public HttpPostedFileBase PortraitImageFile { get; set; }
    }
}
