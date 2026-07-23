using System.ComponentModel.DataAnnotations;

namespace PoliticalLeaderPortal.Areas.Admin.ViewModels
{
    public class AppDownloadSettingVM
    {
        public int AppDownloadSettingId { get; set; }

        [Required]
        [StringLength(120)]
        public string KickerText { get; set; }

        [Required]
        [StringLength(180)]
        public string HeadingText { get; set; }

        [StringLength(220)]
        public string SubHeadingText { get; set; }

        [Required]
        [Url]
        [Display(Name = "Google Play URL")]
        public string GooglePlayUrl { get; set; }

        [Required]
        [Url]
        [Display(Name = "Apple App Store URL")]
        public string AppleAppStoreUrl { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }
}
